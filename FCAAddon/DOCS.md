![Uconnect Logo](https://www.driveuconnect.com/content/dam/uconnect/global/header/Uconnect-small.png) <img src="https://upload.wikimedia.org/wikipedia/en/thumb/4/49/Home_Assistant_logo_%282023%29.svg/2048px-Home_Assistant_logo_%282023%29.svg.png" width="128" height="128"> 

Credit to https://github.com/wubbl0rz/FiatChamp for creating the base addon. Also to https://github.com/altrdev/FiatUconnect and https://github.com/mahil4711/fiat_vehicle_mqtt for resources.

Connect your FCA/Stellantis car 🚗 or truck 🚚 to Home Assistant. Needs a vehicle with a valid subscription tied to a Uconnect account.

## Car Brands

- Jeep: Works ✅
- Fiat: Experimental ⚠️
- Ram: Works ✅
- Dodge: Unknown ❔
- AlfaRomeo: Experimental ⚠️
- Chrysler: Experimental ⚠️
- Maserati: Unknown ❔

## Tested Vehicles

See tested and working vehicles at the following discussion post: https://github.com/Blueion76/FCAUconnect-HA/discussions/2

If your vehicle works, please upload it! This will help others tremendously.

## Prerequisites 📃

<img src="https://raw.githubusercontent.com/Blueion76/FCAUconnect-HA/refs/heads/master/mqtt-logo.png" width="128" height="128"> 
Official Home Assistant MQTT Addon running or an external MQTT broker. Broker must be connected to the Home Assistant MQTT integration.


Make sure your vehicle works with one of the following Uconnect sites. Older vehicles that only use Mopar.com (SiriusXM Guardian) do not work.

- Fiat: https://myuconnect.fiat.com/
- Jeep: https://myuconnect.jeep.com/
- Ram: https://connect.ramtrucks.com/
- Dodge: https://connect.dodge.com/
- AlfaRomeo: https://myalfaconnect.alfaromeo.com/
- Chrysler: https://connect.chrysler.com/choose-country
- Maserati: https://connect.maserati.com/

## Features ✔️

- Imports statistics like battery level 🔋, tire pressure ‍💨, odometer ⏲ etc. into Home Assistant.
- Multiple Brands: Fiat, Jeep, Ram, Dodge, AlfaRomeo, Chrysler & Maserati
- Supports multiple cars on the same account. 🚙🚗🚕
- Location tracking.🌍
- Home Assistant zones (home 🏠, work 🏦 etc.) support.
- Uses the same data source as the official app 📱.
- Remote commands (unlock doors 🚪, switch air conditioner 🧊 on , ...) Some commands may not work with all cars.
- Available commands are:
  - "UpdateLocation" (updates the vehicle location)
  - "DeepRefresh" (refresh battery level %) (ev only)
  - "Alarm" (trigger vehicle horn and lights)
  - "PreconditioningOn/Off" (I believe this triggers/stops HVAC on some vehicles)
  - "TrunkLock/LiftgateLock" (lock/unlock trunk) (your vehicle may not use one or either)
  - "DoorLock" (unlock/lock vehicle doors)
  - "HVAC" (starts/stops the HVAC on some vehicles)
  - "StartEngine" (remote starts the vehicle engine)
  - "StopEngine" (cancels the remote start request)
  - "SuppressAlarm" (suppresses the vehicle theft alarm)
  - "FetchNow" (useful for Home Assistant automations to update the API)
  - "ChargeNow" (ev only - charges the vehicle)
  - "Lights" (I believe this toggles the vehicles headlights on/off, haven't tested)
  - "Comfort" (another alternative to the other HVAC options- I have no idea why FCA uses multiple commands across vehicles)
 
## What doesn't work (yet)? ❌

- Live Vehicle Status. Some vehicles report live vehicle status for things like the locks, my vehicle does not so I'm currently unable to test/code this in. Feel free to submit a PR if you have a compatible vehicle.
- Notification Reporting. The official site and app shows notifications indicating a successful command. I'm not sure how to code this in. Feel free to submit a PR if you find a way.

## What will NEVER work? ❌

- Things the FCA API does not support such as real time tracking or adjusting the audio volume.
- Some commands are vehicle specific and do not work across all makes and models. 

## How to install 🛠️

### Home Assistant OS or Supervised

Follow the official docs:

https://www.home-assistant.io/addons/ 

Short version:

- Switch on "Advanced Mode" in your users profile. (if you haven't already)
- Go to Settings -> Add-ons -> ADD-ON STORE
- Top right three dots -> Repositories -> Add https://github.com/Blueion76/FCAUconnect-HA
- Top right three dots -> Check for updates.
- Refresh Page. (F5)
- Store should show this repo now and you can install the addon.

### Standalone Docker Container

When using Home Assistant as a self managed Docker container you can use FCAUConnect in standalone mode. You need to update the container yourself and export all the needed environment variables. __This is for advanced users only.__
The supervisor token can be generated on the user profile page inside Home Assistant (Long-Lived Access Token).

Docker Compose example:

``` yaml
version: "3.9"                                                                                                                                     
services:
  FCAUconnect:
    image: ghcr.io/blueion76/image-amd64-fca-uconnect:latest 
    container_name: FCAUconnect
    environment:
      - 'STANDALONE=True'
      - 'FCAUconnect_FCAUser=youruconnectemail'
      - 'FCAUconnect_FCAPw=youruconnectpassword'
      - 'FCAUconnect_FCAPin=1234'
      - 'FCAUconnect_SupervisorToken=xxxxxxxxxxxxxxxxx.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx'
      - 'FCAUconnect_StartDelaySeconds=2'
      - 'FCAUconnect_Region=America' # Can be America, Europe, Asia or Canada
      - 'FCAUconnect_Brand=Jeep' # Can be Jeep, Fiat, Chrysler, Maserati, AlfaRomeo, Ram, or Dodge
      - 'FCAUconnect_MqttUser=yourmqttserverusername'
      - 'FCAUconnect_MqttPw=yourmqttserverpassword'
      - 'FCAUconnect_MqttServer=192.168.1.2'
      - 'FCAUconnect_MqttPort=1883'
      - 'FCAUconnect_Debug=true' # Can be set to false if not needed
      - 'FCAUconnect_RefreshInterval=2'
      - 'FCAUconnect_AutoDeepRefresh=false'
      - 'FCAUconnect_HomeAssistantUrl=http://192.168.1.3:8123'
```
## Options / Usage

At startup the addon will automatically connect to your Home Assistant MQTT Broker. You can configure your own MQTT server in the configuration.

- PIN is only needed if you want to send commands to your car. It's the same PIN you set for the official vehicle branded app.
- Use DEBUG carefully. It will dump sensitive information to the log including session tokens.
- Automatic refresh of location and battery level may drain your battery more. The car has to wakeup its computers, read new values and send them back to the API. This will get executed every "Refresh interval" and at every command even if your car is not at home. __Recommendation:__  Use a Home Assistant automation instead. I have setup an automation that if the odometer has gone up the car will update it's location status every 30 seconds till the odometer stays in place. Then it will change to every 3 hours.
- MQTT override can be used if you want to utilize an external MQTT broker. __You do not need this if you are using the official Home Assistant MQTT addon.__

<img src="https://raw.githubusercontent.com/Blueion76/FCAUconnect-HA/refs/heads/master/options.png" width="700px">

## Useful Resources

Cards: 
  - [Ultra Vehicle Card](https://github.com/WJDDesigns/Ultra-Vehicle-Card)
  - [Vehicle Status Card](https://github.com/ngocjohn/vehicle-status-card)
  
