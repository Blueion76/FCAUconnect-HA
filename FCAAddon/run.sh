#!/usr/bin/with-contenv bashio

export FCAUconnect_MqttUser=$(bashio::config 'OverrideMqttUser')
export FCAUconnect_MqttPw=$(bashio::config 'OverrideMqttPw')
export FCAUconnect_MqttServer=$(bashio::config 'OverrideMqttServer')
export FCAUconnect_MqttPort=$(bashio::config 'OverrideMqttPort')
  
test "$FCAUconnect_MqttUser" = "null" && export FCAUconnect_MqttUser=$(bashio::services "mqtt" "username")
test "$FCAUconnect_MqttPw" = "null" && export FCAUconnect_MqttPw=$(bashio::services "mqtt" "password")
test "$FCAUconnect_MqttServer" = "null" && export FCAUconnect_MqttServer=$(bashio::services "mqtt" "host")
test "$FCAUconnect_MqttPort" = "null" && export FCAUconnect_MqttPort=$(bashio::services "mqtt" "port")
  
export FCAUconnect_StartDelaySeconds=$(bashio::config 'StartDelaySeconds')
export FCAUconnect_SupervisorToken=$SUPERVISOR_TOKEN
  
export FCAUconnect_FCAUser=$(bashio::config 'FCAUser')
export FCAUconnect_FCAPw=$(bashio::config 'FCAPw')
export FCAUconnect_FCAPin=$(bashio::config 'FCAPin')

export FCAUconnect_Brand=$(bashio::config 'Brand')
export FCAUconnect_Region=$(bashio::config 'Region')

export FCAUconnect_Debug=$(bashio::config 'Debug')
export FCAUconnect_RefreshInterval=$(bashio::config 'RefreshInterval')
export FCAUconnect_AutoDeepRefresh=$(bashio::config 'AutoDeepRefresh')
export FCAUconnect_AutoDeepInterval=$(bashio::config 'AutoDeepInterval')
export FCAUconnect_StartDelaySeconds=$(bashio::config 'StartDelaySeconds')

cd /build/
./FCAClient
