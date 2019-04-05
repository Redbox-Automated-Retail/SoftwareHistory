--
-- data-error-handler.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


kernel.registerdatastoreerrorhandler(
	function( dataStoreName )
		kernel.log( "Data Store Error[ " .. tostring( dataStoreName ) .. " ] Handler BEGIN" )
		if progress_manager ~= nil then progress_manager:stop() end

		scheduler_services.shutdown()
		if HardSyncMonitor ~= nil then	HardSyncMonitor:stop() end
		if QLMMonitor ~= nil then QLMMonitor:stop() end

		if DataAccessAlreadySent ~= nil then
			kernel.log( "> FMA user already notified... exiting")
		else
			local store_id = kernel.getmachinevalueordefault( "Store", "ID", "Not Configured" )
			kernel.setmacro( "StoreNumber", store_id )
			kernel.setmacro( "MaintenanceView:ZipCode", "Error" )
			kernel.setmacro( "MaintenanceView:ErrorCode", "SOFT-FILE01" )
			kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "SOFT-FILE01" ] or "image_red_diagonal_stripes" )
			view_show( "maintenance_view" )

			local datafilename = "unknown"
			if dataStoreName ~= nil then
				datafilename = dataStoreName .. ".data"
			end
			kernel.hardware_alert_manager_send_alert(
				"ApplicationCrash",
				"Kiosk Engine Data File Error",
				"There was an error accessing or updating the database: [ " .. 
					tostring( datafilename ) .. 
					" ] <br><br>Field Maintenance Application unable to function.<br>SOFT-FILE02",
				nil,
				nil,
				"DataStoreError,SOFT-FILE02"
			)

			local msgbox = newmessageboxmanager()
			local callbacks = {}
			callbacks["OK"] = function()
				local callbacks2 ={}
				callbacks2["OK"] = function()
					kernel.applyunsafeshutdown()
				end
				local default_bundle = kernel.getmachinevalueordefault( "Core", "DefaultBundle", "NONE" )
				if default_bundle == "NONE" or default_bundle == "" then
					msgbox:show("No default bundle is specified.\r\nPlease contact Software Support.", msgbox.ButtonTypeOK, msgbox.IconTypeError, callbacks2)
				else
					kernel.activatedefaultbundle()
				end
			end

			msgbox:show("The Kiosk Engine reports errors\r\naccessing [ " .. datafilename .. " ]\r\n" ..
				"Kiosk is unable to operate.\r\n\r\n" ..
				"Please contact Software Support.",msgbox.ButtonTypeOK, msgbox.IconTypeError, callbacks )
		end

		DataAccessAlreadySent = true
		kernel.log( "Data Store Error Handler END" )
	end )
