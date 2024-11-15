![Uconnect Logo](https://www.driveuconnect.com/content/dam/uconnect/global/header/Uconnect-small.png)

source , credit and all intellengency from https://github.com/wubbl0rz/FiatChamp

Connect your FIAT, Jeep, Ram, Dodge, AlfaRomeo car 🚗 or truck 🚚 to Home Assistant. Needs a vehicle with enabled uconnect services and valid account.

## Car Brands

Fiat: Works ✅
Jeep: Works ✅
Ram: Experimental ⚠️
Dodge: Unknown ⛔
AlfaRomeo: Unknown ⛔


## Tested Vehicles

2024 Jeep Wagoneer L

## Prerequisites 📃

- Official Home Assistant MQTT Addon (recommended) running or external mqtt broker. Broker must be connected to Home Assistant MQTT integration.

![image](https://user-images.githubusercontent.com/30373916/196045271-44287d3f-93ba-49c0-a72f-0bc92042efbb.png)

Make sure your car works with one of the following uconnect sites. Older vehicles that only uses mopar.com do not seem to work.

- Fiat: https://myuconnect.fiat.com/
- Jeep: https://myuconnect.jeep.com
- Ram: https://connect.ramtrucks.com/
- Dodge: https://connect.dodge.com
- AlfaRomeo: https://myalfaconnect.alfaromeo.com/

## Features ✔️

- Imports values like battery level 🔋, tyre pressure ‍💨, odometer ⏲ etc. into Home Assistant.
- Multiple Brands: Fiat, Jeep, Ram, Dodge, AlfaRomeo
- Supports multiple cars on the same account. 🚙🚗🚕
- Location tracking.🌍
- Home Assistant zones (home 🏠, work 🏦 etc.) support.
- Uses the same data source as the official app 📱.
- Remote commands (open doors 🚪, switch air conditioner 🧊 on , ...) are supported since version 2.0. Some commands may not work with all cars. Available commands are:
  - "UpdateLocation" (updates gps location of the car) 
  - "RefreshBatteryStatus" (refresh battery level %)
  - "DeepRefresh" (same as "RefreshBatteryStatus")
  - "Blink" (blink lights)
  - "ChargeNOW" (starts charging)
  - "Trunk" (open/close trunk lock)
  - "DoorLock" (unlock/lock vehicle doors)
  - "HVAC" (turn on the temperature preconditioning in the car)
  - "StartEngine" (remote starts the vehicle engine)
  - "StopEngine" (cancels the remote start request)
  - "SuppressAlarm" (suppresses the vehicle theft alarm)

## What doesn't work (yet)? ❌

- Eco Reports (statistics). I could not find any API yet. The official app shows it so in theory it should be accessible.
- Live Vehicle Engine Status. Some users report that you can find the engine status but I haven't been able to replicate this on my 2024 Wagoneer L.

## What will NEVER work? ❌

- Things the fiat api does not support. Like real time tracking or adjusting the music volume. Maybe they add some new features in the future. 

## How to install 🛠️

### Home Assistant OS or Supervised

Follow the official docs:

https://www.home-assistant.io/addons/ 

Short version:

- Switch on "Advanced Mode" in your users profile. (if you haven't already)
- Go to Settings -> Add-ons -> ADD-ON STORE
- Top right three dots. Add repo. https://github.com/Blueion76/FCAUconnect-HA
- Top right three dots. Check for updates.
- Refresh Page. (F5)
- Store should show this repo now and you can install the addon.

