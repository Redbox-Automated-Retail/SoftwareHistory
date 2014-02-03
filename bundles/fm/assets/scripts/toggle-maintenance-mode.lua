--
-- toggle-maintenance-mode.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

maintenance_mode = not maintenance_mode
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

if maintenance_mode then
	local result = kernel.hardwaremaintenancemodesuspendjobs()
	if not result.success then
		newmessageboxmanager():show( "Error turning on maintenance mode." )
	else
		local hardwarejobs = newhardwaremanager():get_hardware_jobs_by_program2( { "quick-return-daemon" }, { "Pending", "Stopped", "Completed", "Suspended", "Garbage" } )
		for k, v in ipairs(hardwarejobs) do
			newhardwaremanager():suspend_job(v.id)
		end
	end
else
	local result = kernel.hardwaremaintenancemoderesumejobs()
	if not result.success then
		newmessageboxmanager():show("Error turning off maintenance mode.")
	else
		local hardwarejobs = newhardwaremanager():get_hardware_jobs_by_program2( { "quick-return-daemon" }, { "Pending", "Stopped", "Completed", "Running", "Garbage" } )
		for k, v in ipairs(hardwarejobs) do
			newhardwaremanager():resume_job(v.id)
		end
	end
end

kernel.setactortext("maintenance_mode_button", iif(maintenance_mode, "Turn Off\r\nMaintenance Mode", "Turn On\r\nMaintenance Mode"))
kernel.setactorforegroundcolor("inv_mgmt_button", iif(maintenance_mode, "gray", "white"))
kernel.setactorforegroundcolor("inv_sync_button", iif(maintenance_mode, "gray", "white"))
kernel.setactorforegroundcolor("inv_qlm_button", iif(maintenance_mode, "gray", "white"))

if maintenance_mode == true then
	 kernel.disableactor("inv_mgmt_button")
     kernel.disableactor("inv_sync_button")
     kernel.disableactor("inv_qlm_button")
elseif maintenance_mode == false then
	 kernel.enableactor("inv_mgmt_button")
     kernel.enableactor("inv_sync_button")
     kernel.enableactor("inv_qlm_button")
end
kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_MaintenanceMode",
				username,
				"FMA Bundle",
				"Maintenance Mode has been " .. iif( maintenance_mode, "enabled", "disabled" ))

kernel.setmachinevalue("Field Maintenance", "MaintenanceMode", tostring(maintenance_mode))
