![Uconnect Logo](https://www.driveuconnect.com/content/dam/uconnect/global/header/Uconnect-small.png) <img src="https://upload.wikimedia.org/wikipedia/en/thumb/4/49/Home_Assistant_logo_%282023%29.svg/2048px-Home_Assistant_logo_%282023%29.svg.png" width="128" height="128"> 

Credit to https://github.com/wubbl0rz/FiatChamp for creating the base addon. Also to https://github.com/altrdev/FiatUconnect and https://github.com/mahil4711/fiat_vehicle_mqtt for resources.

Connect your FCA/Stellantis car 🚗 or truck 🚚 to Home Assistant. Needs a vehicle with a valid subscription tied to a Uconnect account.

## Car Brands

- Jeep: Works ✅
- Fiat: Unknown ❔
- Ram: Unknown ❔
- Dodge: Unknown ❔
- AlfaRomeo: Works ✅
- Chrysler: Unknown ❔


## Tested Vehicles

See tested and working vehicles at the following discussion post: https://github.com/Blueion76/FCAUconnect-HA/discussions/2

If your vehicle works, please upload it! This will help others tremendously.

## Prerequisites 📃

<img src="https://raw.githubusercontent.com/Blueion76/FCAUconnect-HA/refs/heads/master/mqtt-logo.png" width="128" height="128"> 
Official Home Assistant MQTT Addon running or an external MQTT broker. Broker must be connected to the Home Assistant MQTT integration.


Make sure your vehicle works with one of the following Uconnect sites. Older vehicles that only use Mopar.com do not work.

- Fiat: https://myuconnect.fiat.com/
- Jeep: https://myuconnect.jeep.com/
- Ram: https://connect.ramtrucks.com/
- Dodge: https://connect.dodge.com/
- AlfaRomeo: https://myalfaconnect.alfaromeo.com/
- Chrysler: https://connect.chrysler.com/us/en/login/

## Features ✔️

- Imports statistics like battery level 🔋, tire pressure ‍💨, odometer ⏲ etc. into Home Assistant.
- Multiple Brands: Fiat, Jeep, Ram, Dodge, AlfaRomeo
- Supports multiple cars on the same account. 🚙🚗🚕
- Location tracking.🌍
- Home Assistant zones (home 🏠, work 🏦 etc.) support.
- Uses the same data source as the official app 📱.
- Remote commands (unlock doors 🚪, switch air conditioner 🧊 on , ...) Some commands may not work with all cars. Available commands are:
  - "DeepRefresh" (refresh battery level %) (ev only?)
  - "Alarm" (trigger vehicle horn and lights)
  - "ChargeNOW" (starts charging) (ev only)
  - "Trunk" (open/close trunk lock)
  - "DoorLock" (unlock/lock vehicle doors)
  - "HVAC" (starts the HVAC in the car)
  - "StartEngine" (remote starts the vehicle engine)
  - "StopEngine" (cancels the remote start request)
  - "SuppressAlarm" (suppresses the vehicle theft alarm)
  - "UpdateLocation" (updates the vehicle location)

## What doesn't work (yet)? ❌

- Eco Reports (statistics). I could not find any API yet. The official app shows it so in theory it should be accessible.
- Live Vehicle Status. Some users report that you can find the engine/lock status but I haven't been able to replicate this on my 2024 Wagoneer L. Feel free to submit a PR if you find a way.

## What will NEVER work? ❌

- Things the FCA API does not support such as real time tracking or adjusting the audio volume.

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

## Options / Usage

At startup the addon will automatically connect to your Home Assistant MQTT Broker. You can configure your own MQTT server in the configuration.

- PIN is only needed if you want to send commands to your car. Its the same PIN you set for the official app/website.
- Use DEBUG carefully. It will dump sensitive information to the log including session tokens.
- Automatic refresh of location and battery level may drain your battery more. The car has to wakeup some parts, read new values and send them back to the API. This will get executed every "Refresh interval" and at every command even if your car is not at home. __Recommendation:__  Use a Home Assistant automation instead. I have setup an automation that if the odometer has gone up the car will update it's location status every 30 seconds till the odometer stays in place. Then it will change to every 3 hours.
- MQTT override can be used if you want to utilize an external MQTT broker. __You do not need this if you are using the official Home Assistant MQTT addon.__

<img src="https://raw.githubusercontent.com/Blueion76/FCAUconnect-HA/refs/heads/master/options.png" width="700px">

## Useful Resources

Cards: 
  - [Ultra Vehicle Card](https://github.com/WJDDesigns/Ultra-Vehicle-Card)
  - [Vehicle Status Card](https://github.com/ngocjohn/vehicle-status-card)
  
