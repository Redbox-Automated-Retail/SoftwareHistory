--
-- maintenance-sync-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local store_id = kernel.getmachinevalueordefault( "Store", "ID", "Not Configured" )
local zip = split(Stores[store_id].zip, "-", 1)
kernel.setmacro("StoreNumber", store_id)
kernel.setmacro("MaintenanceView:ZipCode", tostring(zip[1]))
kernel.setmacro( "MaintenanceView:ErrorCode", "0016SYNC" )
idle_timer:stop()

kernel.timercreate(
	"maintenance-sync-view-timer",
	60000,
	60000,
	function()
		if #newhardwaremanager():get_hardware_jobs_by_label_contains_text( "maintenance sync by ", { "Stopped", "Errored", "Garbage", "Suspended", "Completed" } ) == 0
		and kernel.peekview() == "maintenance_sync_view" then 
			kernel.activatedefaultbundle()
		end
	end )
kernel.timerstart( "maintenance-sync-view-timer" )
