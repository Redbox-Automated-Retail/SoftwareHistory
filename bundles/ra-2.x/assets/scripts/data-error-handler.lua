--
-- data-error-handler.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


kernel.registerdatastoreerrorhandler(
	function( dataStoreName )
		kernel.log( "Data Store Error[ " .. tostring( dataStoreName ) .. " ] Handler BEGIN" )
		kernel.executescript( "obj_maintenance_mode_timer" )
		if progress_manager ~= nil then progress_manager:stop() end
		kernel.timerstop( "statistics-timer" )
		kernel.timerstop( "quickreturn-timer" )
		scheduler_services.shutdown()
		if HardSyncMonitor ~= nil then	HardSyncMonitor:stop() end
		if QLMMonitor ~= nil then QLMMonitor:stop() end
		if ReservationManager ~= nil then ReservationManager:reset() end
		if BreakingNewsTimer ~= nil then BreakingNewsTimer:stop() end

		if DataAccessAlreadySent ~= nil then
			kernel.log( "> kiosk already in maintenance mode... exiting")
		else
			AllowEntry = DataAccessAlreadySent == nil or DataAccessAlreadySent == false
			DataAccessAlreadySent = true
			local store_id = kernel.getmachinevalueordefault( "Store", "ID", "Not Configured" )
			kernel.setmacro( "StoreNumber", store_id )
			--kernel.setmacro( "MaintenanceView:ZipCode", "Error" )
			kernel.add_maintenance_mode_reason("Database", "SOFT-FILE01")

			local datafilename = "unknown"
			if dataStoreName ~= nil then
				datafilename = dataStoreName .. ".data"
			end
			kernel.hardware_alert_manager_send_alert(
				"ApplicationCrash",
				"Kiosk Engine Data File Error",
				"There was an error accessing or updating the database: [ " .. 
					tostring( datafilename ) .. 
					" ] <br><br>Placing Kiosk into Maintenance Mode.<br>SOFT-FILE01",
				nil,
				nil,
				"mm:DataStoreError, SOFT-FILE01"
			)
		end
		kernel.log( "Data Store Error Handler END" )
	end )