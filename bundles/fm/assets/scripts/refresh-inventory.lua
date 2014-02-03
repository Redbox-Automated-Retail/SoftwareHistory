--
-- refresh-inventory.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
idle_timer:stop()
kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
kernel.disableactor( "remove_disc_button" )
kernel.setactortext( "select_button", "Select All")
kernel.setactortag( "select_button", "all")
kernel.setactortext( "selected_status", nil )

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

for k,_ in pairs(check_box_state) do
	kernel.setactorimage(k, "check_box")
end
check_box_state = {}
local progress_manager = newprogressmanager()
progress_manager:start(false)
local hardware_manager = newhardwaremanager()
kernel.log( "Refresh Inventory requested by user" )
hardware_manager:soft_sync(	function()
								reload_inventory()
								showinventory()
								progress_manager:stop()
								idle_timer:start()								
							end )	
