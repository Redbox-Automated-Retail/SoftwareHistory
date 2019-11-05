--
-- lib-sync-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function run_soft_sync( on_complete )
    -- N.B. We need something more intelligent here
    --local result = kernel.hardwareisbusy()
    --if result.success == false
    --or result.job_id ~= "idle" then
    --	return
    --end
	if HardwareManager == nil then
		HardwareManager = newhardwaremanager()
	end
    HardwareManager:soft_sync(
        function( job_result )
            ProductCatalog:reset_keys_cache()
            BarcodePosition:reset_keys_cache()
            ProductToBarcode:reset_keys_cache()
            ProductHistory:reset_keys_cache()
            kernel.datastorecloseall()
			if on_complete ~= nil then
				on_complete()
			end
        end )
end

function start_sync_timers()
	kernel.timercreate(
		"soft-sync-timer",
		30000,
		SoftSyncTimer,
		function()
            local view_name = kernel.peekview()
            if view_name == kernel.get_start_over_view_name()
            or kernel.is_in_maintenance_mode()
            or view_name == "maintenance_sync_view" then
                run_soft_sync()
            end
		end )
	kernel.timerstart( "soft-sync-timer" )

	kernel.timercreate(
		"snapshot-timer",
		kernel.random( 60000, 3600000 ),
		28800000,
		function()
			HardwareManager:soft_sync(
				function( job_result )
					force_server_snapshot( job_result.results )
					kernel.datastorecloseall()
				end )
		end )
	kernel.timerstart( "snapshot-timer" )
end

function run_initial_sync()
	if not kernel.storesvcs_shouldruninitialsync() then
		kernel.log("Need not run initialsync")
		return false
	end
    kernel.setmacro( "MaintenanceView:ErrorCode", "INITSYNC" )
	-- kernel.pushview and kernel.showview functions replaced with view_show function.
	kernel.close_open_session_and_start_new_rental_session()
	view_show( get_maintenance_view_from_screen_code("INITSYNC") )
	local hardware_manager = newhardwaremanager()
	hardware_manager.hardware_event_responders.MaxPutAwayAttemptsExceeded = function( args, jobid )
        kernel.setmacro( "MaintenanceView:ErrorCode", "005PMXA" )
        kernel.hardware_alert_manager_send_alert_job(
            "HardwareError",
			"HAL script: check-picker-bounded.hs sent message MaxPutAwayAttemptsExceeded",
			"Unable to put an item away after the configured number of slot retries. Placing the kiosk into maintenance mode",
			jobid, 
			"MaxPutAwayAttemptsExceeded, 005PMXA"
		)
	end

	kernel.log("Checking for existing sync job")
	local currentJobs = hardware_manager:get_hardware_jobs_by_program({ "sync" }, { "Garbage", "Stopped", "Completed", "Errored" })
	local sync_running = false

	for k,v in ipairs(currentJobs) do
		kernel.log("Job label = " .. v.label or "")
		if v.label == "Run Initial Sync" then
			sync_running = true
		end
	end

	if sync_running == false and stop_initial_sync == false then
		kernel.log("Scheduling bounded check picker script")
		hardware_manager:schedule_job(
			"check-picker-bounded",
			"Run Initial Sync",
			false,
			IntialSyncType or "Highest",
			nil,
			nil,
			nil,
			function( job_result )
				--kernel.hardware_alert_manager_send_alert2("Run Initial Sync", job_result.job_id)
				local send_alert = job_result.status == "Errored"
				local error_messages = ""
				for _, v in ipairs( job_result.results ) do
					if  (v.code == "ItemReturned" or v.code == "DiskInPicker")
					and v.item_id ~= nil then
						kernel.invsvcs_return(
							StoreManager:get_number(),
							"InventorySync",
							v.item_id,
							v.deck,
							v.slot,
							v.return_time)
					else
                        error_messages = error_messages .. kernel.hardware_alert_manager_format_job_result( v )
						send_alert = true
					end
				end
				if job_result.status == "Errored" then
                    error_messages = error_messages .. kernel.hardware_alert_manager_format_errors( jobid )
				end
				if send_alert == true and stop_initial_sync == false then
                    kernel.hardware_alert_manager_send_alert(
						"HardwareError",
						"Run Initial Sync Failed",
						"HAL Script: check-picker-bounded.hs was unable to put an item away for the following reasons:" ..
						error_messages,
						nil,
						nil,
						"InitialSyncFailed"
					)
				else
					SyncJobBasedOnRealHardware(hardware_manager)
				end
			end,
			nil,
			false )
	else 
		kernel.log("Reconnecting to existing initial sync job")
		SyncJobBasedOnRealHardware(hardware_manager)
	end

	return true
end

function SyncJobBasedOnRealHardware(hardware_manager)
	if( max_decks ~= nil and max_slots ~= nil ) then
		kernel.log("Scheduling initial sync job")
		schedule_initial_sync({ max_slots, max_decks, 1, 1 }, hardware_manager)
	end
end

function schedule_initial_sync(stack, hardware_mgr)
	hardware_mgr:schedule_job(
		"sync",
		"Run Initial Sync",
		false,
		IntialSyncType or "Highest",
		nil,
		stack,
		nil,
		function( job_result )
			local error_messages = ""
			send_alert = job_result.status == "Errored" or job_result.status == "Garbage"

			for _, v in ipairs( job_result.results ) do
				if v.code ~= "InventoryState" then
                    error_messages = error_messages .. kernel.hardware_alert_manager_format_job_result( v )
					send_alert = true
				end
			end

			if job_result.status == "Errored" or job_result.status == "Garbage" then
				error_messages = error_messages .. kernel.hardware_alert_manager_format_errors( jobid )
			end

			if #job_result.results == 0 then
				error_messages = error_messages .. "<p>The job returned no results.</p>"
				send_alert = true
			end

			if send_alert == true and stop_initial_sync == false then
                kernel.hardware_alert_manager_send_alert(
					"HardwareError",
					"Run Initial Sync Failed",
					"HAL Script: sync.hs was unable to complete for the following reasons:" ..
					error_messages,
					nil,
					nil,
					"InitialSyncFailed"
				)
			else
				kernel.storesvcs_setruninitialsync( false )
				initialize_scheduled_tasks()
				--Instead of kernel.popview and kernel.showview, view_goback is used which in turn calls the two functions
				view_goback( "schedule_initial_sync" )
			end
		end,
		nil,
		false) -- Reconnect to existing job if there is one
end

function register_initial_sync_config_callback()
    kernel.storesvcs_registerinitialsyncconfigcallback(
		function( key, value )
		if key == true then
		    stop_initial_sync = false
			run_initial_sync()
				kernel.log( "Intial sync state value changed.: " .. tostring(key) )
				if(value == "Foreground") then
					IntialSyncType = "Highest"
				else
					IntialSyncType = "Normal"
				end
		else
		    kernel.log( "Intial sync state value changed.: " .. tostring(key) )
			stop_initial_sync = true
		    local hardware_manager = newhardwaremanager()
		    local currentjobs = hardware_manager:get_hardware_jobs_by_program({ "sync" }, { "Garbage", "Stopped", "Completed", "Errored" })
			if(currentjobs ~= nil) then
	        for k,v in ipairs(currentjobs) do
				kernel.log("Job label = " .. v.label or "")
				if v.label == "Run Initial Sync" then
					local job_id = v.id
					kernel.log("Trashing job_id:"..job_id)
					hardware_manager:trash_job(job_id)
				end
		    end
		    end
		    local current_view = kernel.getlastshownview()
		    if(current_view == "red_maintenance_view" or current_view == "orange_maintenance_view") then
		        initialize_scheduled_tasks()
				--Instead of kernel.popview and kernel.showview, view_goback is used which in turn calls the two functions
				view_goback( "register_initial_sync_config_callback" )
			end

		end
	end )
end

--[[function run_server_snapshot( items, to_be_deleted )
	local function on_complete( result )
		kernel.log( "run_server_snapshot - on_complete: success = " .. tostring( result.success ) )
		kernel.log( result )
		
		if result.success == true then 
			if #items > 0 then
				kernel.log( "> deleting " .. tostring( #to_be_deleted ) .. " rows from RemovedQueue" )			
				for k, v in ipairs( to_be_deleted ) do
					RemovedQueue:remove( v )
				end
			end
		else
			kernel.timercreate( "force_server_snapshot_retry", 300000, 300000,
				function()
					kernel.log( "5 minute retry of snapshot" )
					local hardware_manager = newhardwaremanager()
					hardware_manager:soft_sync(
						function( soft_sync_result )
							force_server_snapshot( soft_sync_result.results )
						end
					)
					kernel.timerremove( "force_server_snapshot_retry" )
				end
			)
			kernel.timerstart( "force_server_snapshot_retry" )
		end
		
		kernel.timerstart( "hard-sync-monitor" )
	end
	
	kernel.log( "run_server_snapshot - starting" )
	kernel.timerstop( "hard-sync-monitor" )
	kernel.invsvcs_snapshotimmediate( StoreManager:get_number(), items, on_complete )
end]]

function force_server_snapshot( soft_sync_results )
	kernel.log("Force server snapshot called")
	--kernel.log(">> Soft Sync Results: ".. DataDumper( result_table, nil, true ) )

	-- Change for Backward Return feature - do not send soft sync results while a Hard Sync is still running
	-- or else exception returns will be created before the Backward Return messages get to the server to accurately charge the customer
	kernel.log("> checking for running sync jobs...")
	local hardwarejobs = HardwareManager:get_hardware_jobs_by_program(
		{"sync", "sync-locations", "thin", "unload-thin", "thin-vmz", "clean-vmz" },
		{"Pending", "Stopped", "Suspended", "Garbage", "Completed", "Errored" })
	local job_count = table_count( hardwarejobs )
    kernel.log("> there are " .. tostring( job_count ) .. " hard sync / thin jobs actively running" )
	if job_count > 0 then
		kernel.log( "> force snapshot aborted to allow hard sync / thin jobs to complete first." )
		return
	end

	local items = {}
	for _, v in ipairs( soft_sync_results ) do
		if v.code == "SoftSync"
		and v.item_id ~= nil
		and v.item_id ~= ""
		and v.item_id ~= "EMPTY"
		and v.item_id ~= "UNDEFINED"
		and v.item_id ~= "UNKNOWN"
		and v.deck ~= 0
		and v.slot ~= 0 
		and v.deck ~= qlm_deck
		then
			table.insert(
				items,
				{ barcode = v.item_id, deck = v.deck, slot = v.slot, status = "In" } )
		end
	end
	kernel.log("Barcodes reported IN: ".. tostring(#items) )
	if #items > 0 then
		-- Sending up an empty Items{} array will clear the kiosk inventory on the server...
		-- We are getting empty soft sync results when they should not be.. so this safety check is created...
		-- we really should figure out how we get empty soft sync results...
		local to_be_deleted = {}
		RemovedQueue:prime_cache_keys()
		for k, v in RemovedQueue:pairs() do
			--if VerboseLogging == true then kernel.log( "> RemovedQueue k:" .. tostring( k ) .. ", v.barcode:" .. tostring( v.barcode )) end
			if 	v.barcode ~= nil 
			and v.barcode ~= ""
			and v.barcode ~= "UNKNOWN"
			then
				table.insert(
					items,
					{ barcode = v.barcode, status = "Removed" } )
			end
			to_be_deleted[#to_be_deleted+1] = k
		end
		kernel.log( "> deleting " .. tostring(#to_be_deleted) .. " rows from RemovedQueue" )
		for k, v in ipairs( to_be_deleted ) do
			RemovedQueue:remove( v )
		end
	
		kernel.log("Total Barcodes reported: ".. tostring(#items) )
		kernel.invsvcs_snapshot(
			StoreManager:get_number(),
			items )
	else
		kernel.log("Empty Soft-Sync results NOT sent to server.")
	end
end