--
-- control-panel-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()

ActivityManager:update_elo_pcap_status()
ActivityManager:update_watch_dog_status()
ActivityManager:update_screen_saver_status()
ActivityManager:update_touch_screen_status()
ActivityManager:update_out_of_order_status()
ActivityManager:update_hal_service_status()
ActivityManager:update_close_kiosk_status()

if maintenancemodestate == nil then
	maintenancemodestate = kernel.getmachinevalue("Field Maintenance", "MaintenanceMode") == "true"
	kernel.setmachinevalue("Field Maintenance", "MaintenanceMode", tostring(maintenancemodestate))
end

if maintenancemodestate then
	kernel.setactortext( "start_maintenance_mode", "Turn Maintenance Mode Off" )
else
	kernel.setactortext( "start_maintenance_mode", "Turn Maintenance Mode On" )
end

if kernel.iswindows7() == true then
	kernel.hideactor( "open_hyper_terminal" )
end

KeyNavigationManager:initialize()