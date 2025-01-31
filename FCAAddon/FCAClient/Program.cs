using System.Collections.Concurrent;
using System.Globalization;
using Cocona;
using CoordinateSharp;
using FCAUconnect;
using FCAUconnect.HA;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

var builder = CoconaApp.CreateBuilder();

builder.Configuration.AddEnvironmentVariables("FCAUconnect_");

//todo: integrate reports and events
//todo: schedule turn charging off
//todo: better handling of auto refresh battery and location ...

builder.Services.AddOptions<AppConfig>()
  .Bind(builder.Configuration)
  .ValidateDataAnnotations()
  .ValidateOnStart();

var app = builder.Build();

var persistentHaEntities = new ConcurrentDictionary<string, IEnumerable<HaEntity>>();
var AppConfig = builder.Configuration.Get<AppConfig>();
var forceLoopResetEvent = new AutoResetEvent(false);
var haClient = new HaRestApi(AppConfig.HomeAssistantUrl, AppConfig.SupervisorToken);

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Is(AppConfig.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
  .WriteTo.Console()
  .CreateLogger();

Log.Information("Delay start for seconds: {0}", AppConfig.StartDelaySeconds);
await Task.Delay(TimeSpan.FromSeconds(AppConfig.StartDelaySeconds));

if (AppConfig.Brand is FcaBrand.Ram or FcaBrand.Dodge or FcaBrand.AlfaRomeo)
{
  Log.Warning("{0} support is experimental.", AppConfig.Brand);
}

await app.RunAsync(async (CoconaAppContext ctx) =>
{
  Log.Information("{0}", AppConfig.ToStringWithoutSecrets());
  Log.Debug("{0}", AppConfig.Dump());

  IFiatClient fiatClient =
    AppConfig.UseFakeApi
      ? new FiatClientFake()
      : new FiatClient(AppConfig.FCAUser, AppConfig.FCAPw, AppConfig.Brand, AppConfig.Region);

  var mqttClient = new SimpleMqttClient(AppConfig.MqttServer,
    AppConfig.MqttPort,
    AppConfig.MqttUser,
    AppConfig.MqttPw,
    AppConfig.DevMode ? "FCAUconnectDEV" : "FCAUconnect");

  await mqttClient.Connect();

  while (!ctx.CancellationToken.IsCancellationRequested)
  {
    Log.Information("Now fetching new data...");

    GC.Collect();

    try
    {
      await fiatClient.LoginAndKeepSessionAlive();

      foreach (var vehicle in await fiatClient.Fetch())
      {
        Log.Information("FOUND CAR: {0}", vehicle.Vin);

        if (AppConfig.AutoRefreshBattery)
        {
          await TrySendCommand(fiatClient, FiatCommand.DEEPREFRESH, vehicle.Vin);
        }

        if (AppConfig.AutoRefreshLocation)
        {
          await TrySendCommand(fiatClient, FiatCommand.VF, vehicle.Vin);
        }

        await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken);

        var vehicleName = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname;
        var suffix = AppConfig.DevMode ? "DEV" : "";

        var haDevice = new HaDevice()
        {
          Name = vehicleName + suffix,
          Identifier = vehicle.Vin + suffix,
          Manufacturer = vehicle.Make,
          Model = vehicle.ModelDescription,
          Version = "1.0"
        };

        var currentCarLocation = new Coordinate(vehicle.Location.Latitude, vehicle.Location.Longitude);

        var zones = await haClient.GetZonesAscending(currentCarLocation);

        Log.Debug("Zones: {0}", zones.Dump());

        var tracker = new HaDeviceTracker(mqttClient, "Vehicle_GPS_Location", haDevice)
        {
          Lat = currentCarLocation.Latitude.ToDouble(),
          Lon = currentCarLocation.Longitude.ToDouble(),
          StateValue = zones.FirstOrDefault()?.FriendlyName ?? "not_home"
        };
        
        Log.Information("Car is at location: {0}", tracker.Dump());

        Log.Debug("Announce sensor: {0}", tracker.Dump());
        await tracker.Announce();
        await tracker.PublishState();

        var compactDetails = vehicle.Details.Compact("car");
        var unitSystem = await haClient.GetUnitSystem();

        Log.Information("Using unit system: {0}", unitSystem.Dump());

        var shouldConvertKmToMiles = (AppConfig.ConvertKmToMiles || unitSystem.Length != "km");

        Log.Information("Convert km -> miles ? {0}", shouldConvertKmToMiles);

        var sensors = compactDetails.Select(detail =>
        {
          var sensor = new HaSensor(mqttClient, detail.Key, haDevice)
          {
            Value = detail.Value
          };

          if (detail.Key.EndsWith("_value"))
          {
            var unitKey = detail.Key.Replace("_value", "_unit");

            compactDetails.TryGetValue(unitKey, out var tmpUnit);

            if (tmpUnit == "km")
            {
              sensor.DeviceClass = "distance";

              if (shouldConvertKmToMiles && int.TryParse(detail.Value, out var kmValue))
              {
                var miValue = Math.Round(kmValue * 0.62137, 2);
                sensor.Value = miValue.ToString(CultureInfo.InvariantCulture);
                tmpUnit = "mi";
              }
            }

            switch (tmpUnit)
            {
              case "volts":
                sensor.DeviceClass = "voltage";
                sensor.Unit = "V";
                break;
              case null or "null":
                sensor.Unit = "";
                break;
              default:
                sensor.Unit = tmpUnit;
                break;
            }
          }

          return sensor;
        }).ToDictionary(k => k.Name, v => v);

        if (sensors.TryGetValue("EV_Battery_State_of_Charge", out var stateOfChargeSensor))
        {
          stateOfChargeSensor.DeviceClass = "battery";
          stateOfChargeSensor.Unit = "%";
        }

        if (sensors.TryGetValue("EV_Time_To_Fully_Charge", out var timeToFullyChargeSensor))
        {
          timeToFullyChargeSensor.DeviceClass = "duration";
          timeToFullyChargeSensor.Unit = "min";
        }

        Log.Debug("Announce sensors: {0}", sensors.Dump());
        Log.Information("Pushing new sensors and values to Home Assistant");

        await Parallel.ForEachAsync(sensors.Values, async (sensor, token) => { await sensor.Announce(); });

        Log.Debug("Waiting for home assistant to process all sensors");
        await Task.Delay(TimeSpan.FromSeconds(5), ctx.CancellationToken);

        await Parallel.ForEachAsync(sensors.Values, async (sensor, token) => { await sensor.PublishState(); });

        var lastUpdate = new HaSensor(mqttClient, "Last_API_Update", haDevice)
        {
          Value = DateTime.Now.ToString("O"),
          DeviceClass = "timestamp"
        };
                
        await lastUpdate.Announce();
        await lastUpdate.PublishState();
        
        var localTime = GetLocalTime(vehicle.Location.TimeStamp);
        Log.Debug($"Location TimeStamp: {localTime}");
        
        var trackerTimeStamp = new HaSensor(mqttClient, "Last_Location_Update", haDevice)
        {
            Value = localTime.ToString("O"),  // ISO 8601 string format
            DeviceClass = "timestamp"
        };

        await trackerTimeStamp.Announce();
        await trackerTimeStamp.PublishState();

        
        var haEntities = persistentHaEntities.GetOrAdd(vehicle.Vin, s =>
          CreateInteractiveEntities(fiatClient, mqttClient, vehicle, haDevice));

        foreach (var haEntity in haEntities)
        {
          Log.Debug("Announce sensor: {0}", haEntity.Dump());
          await haEntity.Announce();
        }
      }
    }
    catch (FlurlHttpException httpException)
    {
      Log.Warning($"Error connecting to the FCA API. \n" +
                  $"This can happen from time to time. Retrying in {AppConfig.RefreshInterval} minutes.");

      Log.Debug("ERROR: {0}", httpException.Message);
      Log.Debug("STATUS: {0}", httpException.StatusCode);

      var task = httpException.Call?.Response?.GetStringAsync();

      if (task != null)
      {
        Log.Debug("RESPONSE: {0}", await task);
      }
    }
    catch (Exception e)
    {
      Log.Error("{0}", e);
    }

    Log.Information("Fetching COMPLETED. Next update in {0} minutes.", AppConfig.RefreshInterval);

    WaitHandle.WaitAny(new[]
    {
      ctx.CancellationToken.WaitHandle,
      forceLoopResetEvent
    }, TimeSpan.FromMinutes(AppConfig.RefreshInterval));
  }
});

async Task<bool> TrySendCommand(IFiatClient fiatClient, FiatCommand command, string vin)
{
  Log.Information("SEND COMMAND {0}: ", command.Message);

  if (string.IsNullOrWhiteSpace(AppConfig.FCAPin))
  {
    throw new Exception("PIN NOT SET");
  }

  var pin = AppConfig.FCAPin;

  try
  {
    await fiatClient.SendCommand(vin, command.Message, pin, command.Action);
    await Task.Delay(TimeSpan.FromSeconds(5));
    Log.Information("Command: {0} SUCCESSFUL", command.Message);
  }
  catch (Exception e)
  {
    Log.Error("Command: {0} ERROR. Maybe wrong pin?", command.Message);
    Log.Debug("{0}", e);
    return false;
  }

  return true;
}
/* 

Create Buttons inside Home Assistant

*/
IEnumerable<HaEntity> CreateInteractiveEntities(IFiatClient fiatClient, SimpleMqttClient mqttClient, Vehicle vehicle,
  HaDevice haDevice)
{
  var updateLocationButton = new HaButton(mqttClient, "UpdateLocation", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.VF, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });
  
  var deepRefreshButton = new HaButton(mqttClient, "DeepRefresh", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.DEEPREFRESH, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var alarmButton = new HaButton(mqttClient, "VehicleAlarm", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.HBLF, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var hvacButton = new HaButton(mqttClient, "PreconditioningOn", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.ROPRECOND, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var hvacButton = new HaButton(mqttClient, "PreconditioningOff", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.ROPRECOND_OFF, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var startEngineButton = new HaButton(mqttClient, "StartEngine", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.REON, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var stopEngineButton = new HaButton(mqttClient, "StopEngine", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.REOFF, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });


  var lockDoorsButton = new HaButton(mqttClient, "DoorLock", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.RDL, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var unlockDoorsButton = new HaButton(mqttClient, "DoorUnlock", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.RDU, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var fetchNowButton = new HaButton(mqttClient, "FetchNow", haDevice, async button =>
  {
      Log.Information($"Force Fetch Now");
      await Task.Run(() => forceLoopResetEvent.Set());
  });

  var suppressAlarmButton = new HaButton(mqttClient, "SuppressAlarm", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.TA, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var lockTrunkButton = new HaButton(mqttClient, "LockTrunk", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.ROTRUNKLOCK, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var unlockTrunkButton = new HaButton(mqttClient, "UnlockTrunk", haDevice, async button =>
  {
      if (await TrySendCommand(fiatClient, FiatCommand.ROTRUNKUNLOCK, vehicle.Vin))
      {
          forceLoopResetEvent.Set();
      }
  });

  var chargeNowButton = new HaButton(mqttClient, "ChargeNow", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.CNOW, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var lockLiftgateButton = new HaButton(mqttClient, "LockLiftgate", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROLIFTGATELOCK, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var unlockLiftgateButton = new HaButton(mqttClient, "UnlockLiftgate", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROLIFTGATEUNLOCK, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var comfortOnButton = new HaButton(mqttClient, "ComfortOn", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROCOMFORTON, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var comfortOffButton = new HaButton(mqttClient, "ComfortOff", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROCOMFORTOFF, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var hvacOffButton = new HaButton(mqttClient, "HVACOff", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROHVACOFF, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var hvacOnButton = new HaButton(mqttClient, "HVACOn", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROHVACON, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});

  var lightsButton = new HaButton(mqttClient, "Lights", haDevice, async button =>
{
    if (await TrySendCommand(fiatClient, FiatCommand.ROLIGHTS, vehicle.Vin))
    {
        forceLoopResetEvent.Set();
    }
});


  return new HaEntity[]
  {
    updateLocationButton,
    deepRefreshButton,
    alarmButton,
    hvacButton,
    startEngineButton,
    stopEngineButton,
    lockDoorsButton,
    unlockDoorsButton,
    fetchNowButton,
    suppressAlarmButton,
    lockTrunkButton,
    unlockTrunkButton,
    chargeNowButton
  };
}
/* 

Get Current Time

*/
DateTime GetLocalTime(long timeStamp)
{
    return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).UtcDateTime.ToLocalTime();
}
