--
-- obj-hard-sync-monitor.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newhardsyncmonitor()
	local t = {}
	t.start = function( self, start, frequency )
		kernel.timercreate(
			"hard-sync-monitor",
			start or kernel.random(60000,600000),
			frequency or 600000,
			self.on_timer )
		kernel.timerstart( "hard-sync-monitor" )
	end
	
	t.stop = function( self )
        kernel.log("HardSyncMonitor:stop()")
        kernel.timerstop( "hard-sync-monitor" )
	end

	t.on_timer = function()
	
		local is_sync_type = function(label, sync_type)
			local sync_type_index = string.find(label, sync_type)
			kernel.log("Sync being compared: ".. label)
			if sync_type_index == nil then
				return false
			elseif sync_type_index > 0 then
				return true
			end
		end
		
		local function check_and_send_is_kiosk_clean()
			local hardware_manager = newhardwaremanager()
			local completed_sync_jobs = hardware_manager:get_hardware_jobs_by_program( {"sync", "sync-locations"}, {"Suspended", "Pending", "Running", "Errored", "Stopped", "Garbage"} )
			if table_count( completed_sync_jobs ) > 0 then
				for _, v in ipairs( completed_sync_jobs ) do
					if is_sync_type( v.label, "Full inventory sync") then
						kernel.log( "check_and_send_is_kiosk_clean() found Full inventory sync, enqueueproperty IsKioskClean" )
						kernel.healthsvcs_enqueueproperty( "IsKioskClean", tostring( kernel.hardwaresupportscamerafraudscan() ) )
						kernel.healthsvcs_enqueueproperty( "IsKioskPeeledClean", tostring(kernel.peeled_fraud_image_upload_enabled()))
					end
                    if is_sync_type( v.label, "Fraud Sync") then
						kernel.log( "check_and_send_is_kiosk_clean() found Fraud inventory sync, enqueueproperty IsKioskClean" )
						kernel.healthsvcs_enqueueproperty( "IsKioskClean", tostring( kernel.hardwaresupportscamerafraudscan() ) )
						kernel.healthsvcs_enqueueproperty( "IsKioskPeeledClean", tostring(kernel.peeled_fraud_image_upload_enabled()))
                        if kernel.getmachinevalueordefault( "Store", "RemoteThinRequested", "True" ) == "False" then
                            kernel.removemachinevalue("store", "RemoteFraudSyncRequested" )
                        end
					end
				end
			end
		end

        kernel.timerstop( "hard-sync-monitor" )
        kernel.log("HardSyncMonitor.on_timer: Executing")

        if kernel.is_ir_security_enabled() == true or kernel.peeled_fraud_image_upload_enabled() == true then
			kernel.log( "kernel.is_ir_security_enabled() == true, calling check_and_send_is_kiosk_clean()" )
			check_and_send_is_kiosk_clean()
		else
			kernel.log( "kernel.is_ir_security_enabled() == false, not calling check_and_send_is_kiosk_clean()" )
		end

		local hardwaremgr = newhardwaremanager()
		
		local hardwarejobs = hardwaremgr:get_hardware_jobs_by_program(
			{"sync", "sync-locations"},
			{"Pending", "Running", "Suspended", "Garbage"})
		local job_count = table_count( hardwarejobs )
        kernel.log("HardSyncMonitor.on_timer: Total completed sync jobs: " .. tostring( job_count) )
		
		if job_count > 0 then
			-- Send an alert for each sync job found
			for _, v in ipairs(hardwarejobs) do
				kernel.log("Job:".. v.id)
				if is_sync_type (v.label, "Run Initial Sync") then
					kernel.hardware_alert_manager_send_alert2("Run Initial Sync", v.id)
				elseif is_sync_type( v.label, "FMA-Sync") then
					kernel.hardware_alert_manager_send_alert2("FMA-Sync", v.id)
				elseif is_sync_type( v.label, "New Install Sync") then
					kernel.hardware_alert_manager_send_alert2("New Install Sync", v.id)
				else
					kernel.hardware_alert_manager_send_alert2(v.label, v.id)
				end
				
                --kernel.process_job_for_frauds( v.id, true )
                kernel.process_hardware_results( v.id, v.program )

				-- Unknowns that are now known must be sent to server to apply correct customer return time charges
				local job_result_table = kernel.hardwaregetprogramresults( v.id )
				local results = job_result_table["results"]
				if results ~= nil then
					kernel.log( "> processing sync[" .. tostring( v.id ) .. "] for converted UNKNOWNs..." )
					local fraud_count = 0
					for _, sr in ipairs( results ) do
						if sr.code == "PeeledImageFile" then
						    -- The call will determine whether the image should actually be sent.
							kernel.upload_disc_image_from_sync( tostring(sr.item_id), tostring(sr.message) )
						elseif sr.code == "InventoryState" then 
							if VerboseLogging == true then
								kernel.log( "> i:" .. tostring( sr.item_id ) .. ", pi:" .. tostring( sr.previous_item ) .. 
									", rt:" .. tostring( sr.return_time ) .. ", location ".. tostring( sr.deck ) .. "," .. tostring( sr.slot) )
							end
                            
							if sr.previous_item ~= nil and sr.previous_item == "UNKNOWN" 
							and sr.item_id ~= nil and sr.item_id ~= "" and sr.item_id ~= "UNKNOWN" 
							and sr.return_time ~= nil and sr.return_time ~= "NONE" 
							then
								kernel.invsvcs_return(
									StoreManager:get_number(),
									"BackwardsDiscReturn",
									sr.item_id,
									sr.deck,
									sr.slot,
									sr.return_time
								)
								kernel.log( "> barcode: " .. tostring( sr.item_id ) .. ", returned: " .. tostring( sr.return_time ) .. " sent to server" )
							end
                        elseif sr.code == "NoFraudMarkersDetected" and sr.item_id ~= nil and sr.item_id ~= "" and sr.item_id ~= "UNKNOWN" then
                            local barcode = sr.item_id
                            if kernel.is_paper_fraud( barcode ) == true then

                                kernel.send_fraud_alert( barcode, true, sr.time_stamp24 )
                                
                                fraud_count = fraud_count + 1
                                
                                if kernel.rental_inventory_get_rapidstatus( barcode ) == nil then
                                    BarcodeStatus[barcode] = { barcode = tostring( barcode ), status = "Fraud" }
                                    kernel.log( "barcode = " .. tostring( barcode ) .. ", status = fraud inserted into barcodestatus" )
                                end
                            end
						end
					end
                    if fraud_count > 0 then
                        kernel.send_fraud_clean_statistic( fraud_count )
                    end
					kernel.log( "> processing sync COMPLETE" )                    
				end
			end
			-- Send ONLY one inventory snapshot
			hardwaremgr:soft_sync(
				function( soft_sync_result )
					-- This only needs to be done once no matter how many hardware jobs were found
					force_server_snapshot( soft_sync_result.results )

					-- Clear out the jobs
					for _, v in ipairs( hardwarejobs ) do
						local result_table = kernel.hardwaregetprogramresults( v.id )
						kernel.hardwaretrashjob( v.id )
					end

					kernel.timerstart("hard-sync-monitor")
					kernel.log("HardSyncMonitor.on_timer:  Complete" )
				end )
		else
			kernel.log("HardSyncMonitor.on_timer:  Complete" )
			if hal_comm_failure == false then
				kernel.timerstart( "hard-sync-monitor" )
			else
				kernel.log( "HardSyncMonitor is unable to communicate with HAL, hard-sync-monitor timer will not be resumed until rental app reload." )
			end
		end
	end
	return t
end
