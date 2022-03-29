--
-- return-manager.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newreturnmanager( shopping_cart )
	local t = {}

	-- Get a reference to the cart within the array, not the current global cart pointer, which is reset often
	t.shopping_cart = ShoppingCartList[ shopping_cart.session_id ]
	t.max_put_away_attempts_exceeded = false
	
	t.init = function( self )
		kernel.log( "ReturnManager:init()" )
		self.has_ended = false
        self.has_shown_return_view = false;
		self.is_machine_full = false;
        kernel.set_session_value("BrowseState.InReturnFlow", true)
		self.did_show_success_message = false
	end

	t.show_message = function( self, message, unwind_views )
		if self.shopping_cart ~= nil and self.shopping_cart.session_id ~= nil then
			view_analytics_event2( "context", "returnError", tostring(self.shopping_cart.session_id) )
		end
		
		if unwind_views == true then
            if (message == strings.hardware_error_message) then
                kernel.show_message_popup_view_hardware_error(
				    function()
					    kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_message timer expired" )
				    end,
				    function()
					    kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_message second button clicked" )
				    end
                )
            else
                -- this code should never run but just in case...
			    kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_message unexpected message text." )
            end
		else
			return_manager_error_message_view_manager:show()
			return_manager_error_message_view_manager:set_message( message )
		end
		self.has_ended = true
        kernel.set_session_value("BrowseState.InReturnFlow", false)
	end

	t.close_session_show_maintenance = function( self, abandon_reason, alert_type, jobid, error_code, screen_code )
		self.shopping_cart:close_analytics( "False" )
		self.shopping_cart:abandon( abandon_reason )
		self:show_message( strings.hardware_error_message, false )
		self.has_ended = true
        kernel.set_session_value("BrowseState.InReturnFlow", false)
		if screen_code == nil then
			screen_code = "RPCSSM"
		end
		show_maintenance_view( "return.hs", alert_type, jobid, error_code, screen_code, true, "HAL" )
	end

	t.show_success_message = function( self, item_id, job_id )
	    if (self.did_show_success_message == false) then
		    self.did_show_success_message = true
		    local switch_culture_unwind = function()
			    if DefaultCulture ~= nil and DefaultCulture ~= "" then
				    kernel.setculture( DefaultCulture )
			    else
				    kernel.setculture( "en-US" )
			    end
			    localization_reload_resources()
			    kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_success_message switch_culture_unwind()" )
		    end
		    if self.shopping_cart ~= nil and self.shopping_cart.session_id ~= nil then
			    kernel.shopping_cart_commit(self.shopping_cart.session_id, "User successfully returned product with barcode: " .. tostring( item_id ), true )
		    end

			if kernel.can_show_return_visit_promo_offer_view() then
				kernel.show_return_visit_promo_offer_view(switch_culture_unwind)
			else
				kernel.show_message_popup_view_return_success(switch_culture_unwind)
			end

            kernel.log("return ended by showing success message for job id: " .. tostring(job_id))
		    self.has_ended = true
            kernel.set_session_value("BrowseState.InReturnFlow", false)
        end
	end
	
	t.show_backward_inserted_message = function( self )
        kernel.show_message_popup_view_return_backward_inserted(
			function()
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_backward_inserted_message timer expired" )
			end,
			function()
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_backward_inserted_message second button clicked" )
			end
        )

		self.has_ended = true
        kernel.set_session_value("BrowseState.InReturnFlow", false)
	end

	t.show_unrecognized_barcode_returned_message = function( self )
        kernel.show_message_popup_view_return_unrecognized_barcode(
			function()
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_unrecognized_barcode_returned_message timer" )
			end,
			function()
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager:show_unrecognized_barcode_returned_message continue button" )
			end
        )

		self.has_ended = true
        kernel.set_session_value("BrowseState.InReturnFlow", false)
	end

	t.begin_return = function( self, return_type )

        kernel.hide_message_popup_view(true)
		local function register_return_responders( hardware_manager, return_manager  )

			hardware_manager:reset_event_responders()

			hardware_manager.hardware_event_responders.MachineError = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.MachineError not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end
				kernel.log( "ReturnManager event responder for MachineError" )
				kernel.analyticsaddelement( "return", "failureReason", "MachineError", return_manager.shopping_cart.session_id )
				kernel.setmacro( "MaintenanceView:ErrorCode", "MachineError" )
				return_manager:close_session_show_maintenance(
					"The return job failed due to a hardware error.",
					"HardwareError",
					jobid,
					"MachineError, Customer Return",
					"MachineError" )
			end

			hardware_manager.hardware_event_responders.ReturnEndWithBackwardDisc = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.ReturnEndWithBackwardDisc not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log( "ReturnManager event responder for ReturnEndWithBackwardDisc" )
				kernel.analyticsaddelement( "return", "failureReason", "ReturnEndWithBackwardDisc", return_manager.shopping_cart.session_id )
				-- Remove this handler so it does not run when the job finally ends
				hardware_manager.hardware_event_responders.ReturnEnd = nil

				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				return_manager.shopping_cart:abandon(
					"The return job failed because the customer inserted the item backward or no barcode was present." )
				kernel.hardware_alert_manager_send_alert_job(
					"ReturnRejected",
					"HAL Script return.hs sent message ReturnEndWithBackardDisc",
					"The return job failed because the customer inserted the item backward or no barcode was present.  The item was put away as UNKNOWN.",
					jobid,
					"ReturnEndWithBackwardDisc, Customer Return")
				-- Here maintenance view check is done in the stack
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager: ReturnEndWithBackwardDisc" )
			end

			hardware_manager.hardware_event_responders.MachineFull = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.MachineFull not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log("ReturnManager event responder for MachineFull")
				kernel.analyticsaddelement( "return", "failureReason", "MachineFull", return_manager.shopping_cart.session_id )
				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				self.is_machine_full = true
				kernel.show_machine_full_warning()
		end

			hardware_manager.hardware_event_responders.NoEmptySlot = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.NoEmptySlot not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log("ReturnManager event responder for NoEmptySlot")
				kernel.analyticsaddelement( "return", "failureReason", "NoEmptySlot", return_manager.shopping_cart.session_id )
				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				return_manager.shopping_cart:abandon( "The return job failed because the machine is full." )
				if not in_maintenance_mode() then
					return_manager:show_message( strings.hardware_error_message, true )
				end
			end

			hardware_manager.hardware_event_responders.ReturnEnd = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.ReturnEnd not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log("ReturnManager event responder for ReturnEnd")
				if not in_maintenance_mode() then
					local item_id = "UNKNOWN"
					if #args > 0 then
						item_id = args[1]
					end
					--if item_id ~= "UNKNOWN" then -- This should work but we need the message code to take down the movie and switch the views...
						return_manager:show_success_message( item_id,  jobid)
					--end
				end
			end

			hardware_manager.hardware_event_responders.ItemInsertError = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.ItemInsertError not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				-- Remove this handler so it does not run when the job finally ends
				hardware_manager.hardware_event_responders.ReturnEnd = nil
				if not in_maintenance_mode() then
					return_manager:show_backward_inserted_message()
					view_analytics_event2( "context", "returnError", tostring(self.shopping_cart.session_id) )
				end
			end

			hardware_manager.hardware_event_responders.UnrecognizedBarcodeReturned = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.UnrecognizedBarcodeReturned not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				-- Remove this handler so it does not run when the job finally ends
				hardware_manager.hardware_event_responders.ReturnEnd = nil
				if not in_maintenance_mode() then
					return_manager:show_unrecognized_barcode_returned_message()
                    view_analytics_event2( "context", "UnrecognizedBarcodeReturned", tostring( self.shopping_cart.session_id ) )
				end
			end

			hardware_manager.hardware_event_responders.ReturnRetry = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.ReturnRetry not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				-- Deprecated
				kernel.log( "ReturnManager event responder for ReturnRetry " .. tostring( jobid ) )
			end

			hardware_manager.hardware_event_responders.DisableCancel = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.DisableCancel not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end
			end

			hardware_manager.hardware_event_responders.MaxPutAwayAttemptsExceeded = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.MaxPutAwayAttemptsExceeded not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log( "ReturnManager event responder for MaxPutAwayAttemptsExceeded" )
				max_put_away_attempts_exceeded = true
				kernel.analyticsaddelement( "return", "failureReason", "MaxPutAwayAttemptsExceeded", return_manager.shopping_cart.session_id)
				kernel.setmacro( "MaintenanceView:ErrorCode", "005PMXA" )
				return_manager:close_session_show_maintenance(
					"The return job failed - Max Put Away Attempts Exceeded.",
					"ReturnRejected",
					jobid,
					"MaxPutAwayAttemptsExceeded, Customer Return",
					"005PMXA")
			end

			hardware_manager.hardware_event_responders.VendDoorError = function( args, jobid )
                if last_return_job_id ~= jobid then 
                    kernel.log("hardware_event_responders.VendDoorError not executed.  Jobid does not match. last_return_job = " .. tostring(last_return_job_id).. " jobid " .. tostring(jobid))
                    return 
                end

				kernel.log( "ReturnManager event responder for VendDoorError" )
				kernel.analyticsaddelement( "return", "failureReason", "VendDoorError", return_manager.shopping_cart.session_id)
				kernel.setmacro( "MaintenanceView:ErrorCode", "0128VNDR" )
				return_manager:close_session_show_maintenance(
					"The return job failed - vend door did not respond to HAL.",
					"ReturnRejected",
					jobid,
					"VendDoorError, Customer Return",
					"0128VNDR")
			end
		end

		register_return_responders( HardwareManager, self )
		local return_start_time = os.time()

		local result = HardwareManager:return_item(
			function(job_id)
				--On_Ready
                kernel.log("Return Job on ready event firing for job id: " .. tostring(job_id))
                if self.has_shown_return_view == false then
                    self.has_shown_return_view = true
				    ShoppingCart:add_session_event( "Hardware", "HAL return job is running." )

                    if kernel.is_message_popup_view_showing() then
                        kernel.hide_message_popup_view(true)
                    end
				    if return_manager_error_message_view_manager.is_showing then
					    return_manager_error_message_view_manager:hide( true )
				    end
					if kernel.peekview() == "return_visit_promo_offer_view" then
						kernel.view_goback_safe("return_visit_promo_offer_view", true)
				    end

				    view_show_safe( "return_view" )
				    self.has_ended = false
				    -- Analytic session is only started AFTER the view changes
				    kernel.analyticsaddnode( "session", "return", self.shopping_cart.session_id )
				    kernel.analyticsaddelement( "return", "startTime", tostring( os.date( "%c", return_start_time )), self.shopping_cart.session_id)
                else
                    kernel.log("Return view was already shown and therefore will not be shown again.")
                end
			end,
			function( job_result )
				--On_Complete
                kernel.log("Return flow on complete processing...")
                local function contains_code( results, code )
	                for _, v in ipairs( results ) do
		                if v.code == code then
			                return true
		                end
	                end
	                return false
                end

                local function get_result( results, code )
	                for _, v in ipairs( results ) do
		                if v.code == code then
			                return v
		                end
	                end
	                return {}
                end

				local function is_digital_code( barcode )
                    local product = BarcodeToProduct[barcode]
                    if product ~= nil and product.product_id ~= nil then
                        local master_product = MasterProductCatalog[product.product_id]
                        if master_product ~= nil and master_product.product_type_id == 17 then
                            return true
                        end
                    end
                    return false
				end

                local returned_item_id = nil
				local bad_codes = {}
				local should_show_success = false
				local EmptySlotCount = 9999
				local DumpBinRemainingCapacity = 0
				local ReturnSlotBufferValue = 1
                local barcodes
				
				if job_result ~= nil then
                    local peel_image_result = get_result( job_result.results, "PeeledImageFile" )

					for _, v in ipairs( job_result.results ) do
						-- Check the available slot count
						if  (v.code == "EmptySlotCount") then
							-- this is the current empty slot count in the kiosk
							EmptySlotCount = tonumber(v.message)
						elseif  (v.code == "DumpBinRemainingCapacity") then
							-- this value is used to ensure we do not exceed the dump bin capacity when scheduling a thin-vmz
							DumpBinRemainingCapacity = tonumber(v.message)
						elseif  (v.code == "ReturnSlotBufferValue") then
							-- this is the # of reserverd slots to keep available for hal to work with inventory (typically value is 1)
							-- this will be the value used to trigger a thin-vmz so people can return disks
							ReturnSlotBufferValue = tonumber(v.message)
						elseif  (v.code == "SLOTINUSE" and max_put_away_attempts_exceeded == true) then
							max_put_away_attempts_exceeded = false  -- there are 4 of these messages, we only want to send 1 message
							kernel.invsvcs_return(
								StoreManager:get_number(),
								"KioskReturn",
								v.item_id,
								0,
								0,
								v.time_stamp24 )
						elseif v.code == "ItemReturned"
						and v.item_id ~= nil and v.item_id ~= "UNKNOWN" then
							kernel.log( "ReturnManager:v.code = " .. tostring ( v.code ) .. ", v.item_id = " .. tostring( v.item_id ) .. ", v.return_time = " .. tostring( v.return_time ) )
                            local type_value = "KioskReturn"
                            local failed_security_read = false

                            if contains_code( job_result.results, "NoFraudMarkersDetected" ) == true
                            and kernel.is_paper_fraud( v.item_id ) == true then
                                kernel.log("Return is fraud.  found NoFraudMarkersDetected")
                                type_value = "FailedIRSecurity"
                                failed_security_read = true
                                peel_image_result = {} --already marked as fraud don't need to peel check
                                if kernel.rental_inventory_get_rapidstatus( v.item_id ) == nil then
                                    BarcodeStatus[v.item_id] = { barcode = tostring( v.item_id ), status = "Fraud" }
                                    kernel.log( "barcode = " .. tostring( v.item_id ) .. ", status = Fraud inserted into BarcodeStatus" )
                                end
                                kernel.healthsvcs_sendalert2(
	                                StoreManager:get_number(),
	                                "ReturnRejected",
	                                "SECURITYRETURN",
	                                "Security missing on " .. tostring( v.item_id )
                                )
                            end

                            kernel.log('peel_image_result.message' .. tostring(peel_image_result.message))

							kernel.invsvcs_return2(
								StoreManager:get_number(),
								type_value,
								v.item_id,
								v.deck,
								v.slot,
								v.return_time,
                                failed_security_read,
                                peel_image_result.message
                            )

							if is_digital_code( v.item_id ) then
								BarcodeStatus[v.item_id] = { barcode = tostring( v.item_id ), status = "Unknown Title" }
								kernel.log( "Digital Code returned: barcode = " .. tostring( v.item_id ) .. ", status = Unknown Title - inserted into BarcodeStatus" )
							end

							returned_item_id = v.item_id
							should_show_success = ( v.message == "The item was successfully returned." )

							
							local return_table = {
								titleId = lookup_id,
								titleBarcode = tostring( v.item_id ),
								deck = tostring( v.deck ),
								slot = tostring( v.slot )
							}
							kernel.analyticsaddelements( "return", return_table, self.shopping_cart.session_id )
						else
							table.insert( bad_codes, v.code )
						end
					end
				end

				VMZEnabledMachine = ( qlm_deck == nil or qlm_deck == 0 )
				if EmptySlotCount <= ReturnSlotBufferValue and VMZEnabledMachine then
					self.free_slots_schedule_thin( self, DumpBinRemainingCapacity )
				end

				local return_end_time = os.time()
				local return_table = {
					success = tostring( should_show_success ),
					endTime = tostring( os.date( "%c", return_end_time ) ),
					duration = tostring( os.difftime( return_end_time, return_start_time) )
				}
				kernel.analyticsaddelements( "return", return_table, self.shopping_cart.session_id )

				if should_show_success == true then
					self.shopping_cart:close_analytics( "False" )
				end

				if #bad_codes > 0 then
					-- Moved analytics event to ItemInsertError responder so it can be added before the view changes to message-view
					-- view_analytics_event2( "context", "returnError", self.shopping_cart.session_id )
					kernel.analyticsaddelement( "return", "failureReason", tostring( bad_codes[1] ), self.shopping_cart.session_id)
					self.shopping_cart:close_analytics( "True" )
							
					local was_handled = handle_critical_hardware_errors(
						"return",
						job_result.job_id,
						bad_codes )
					if was_handled then
						self.shopping_cart:abandon()
					end
				else
					if not self.did_show_success_message
					and should_show_success
					and not in_maintenance_mode() then
                        kernel.log("return manager showing suceess message from safety check for job id: " .. tostring(job_result.job_id))
						self.show_success_message( returned_item_id,  job_result.job_id)
					end
				end

                local current_view = kernel.peekview()
				if ( current_view == "return_view"
                or current_view == "error_message_view"
				or kernel.is_message_popup_view_showing()
				or current_view == "return_visit_promo_offer_view" )
				and self.is_machine_full == false
				and not self.has_ended then
                    kernel.log("Return flow complete catch all processing for job id" .. tostring(job_result.job_id))
                    return_pending = false  -- since the last return didn't succeed don't start another one 
                    last_return_job_id = nil
					self.has_ended = true
                    kernel.set_session_value("BrowseState.InReturnFlow", false)
                    kernel.message_popup_view_stop_timer()
					if kernel.getculture() ~= "en-US" then
						if DefaultCulture ~= nil and DefaultCulture ~= "" then
							kernel.setculture( DefaultCulture )
						else
							kernel.setculture( "en-US" )
						end
						localization_reload_resources()
					end
					kernel.close_open_session_and_start_new_rental_session( "ReturnManager: on_complete, has_ended == false" )					
                else
                    -- return completion handled normally
                    kernel.log("Return Manager On complete finished for job id: " .. tostring(job_result.job_id) .. " Current return job id: " .. tostring(last_return_job_id))

                    -- clear out last_return_job_id and check for a pending return
                    last_return_job_id = nil
                    if return_pending == true then
                        kernel.log("Return manager pending return.")
                        kernel.executescript("return_product")
                    end
                end
			end,
			function()
				--On_Busy
                kernel.log("Return manager on busy")
                show_hardware_busy_message()
			end )

		if not result.success then
			self.has_ended = true
            last_return_job_id = nil
            kernel.set_session_value("BrowseState.InReturnFlow", false)
			if not in_maintenance_mode() then
				kernel.log("return-product view_unwindtobottom()")
				kernel.close_open_session_and_start_new_rental_session( "ReturnManager: unable to create_return" )
			end
			return
		end

        -- store the job id so we only process events from the last return job
        last_return_job_id = result.job_id

       
	end

	t.free_slots_schedule_thin = function( self, DumpBinRemainingCapacity )
		-- set schedule_time to be immediate
		local schedule_time = newdatetime(os.date("*t"))
		
		-- thin-vmz jobs from scheduled jobs visible in this list
		local is_thin_vmz_running = #HardwareManager:get_hardware_jobs_by_program( { "thin-vmz" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
		
		-- thin-vmz jobs from immediate jobs visible in this list
		local is_thin_vmz_running2 = #HardwareManager:get_hardware_jobs_by_program2( { "thin-vmz" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
		
		-- only run a thin if the job hasn't been scheduled or is running yet
		if (not is_thin_vmz_running) and (not is_thin_vmz_running2) then
			local label = "thin scheduled due to low slot availability"
			local barcodesToBin, rebalances, redeployments, barcodesToVmz = get_vmz_thin_barcodes()
			
			-- There could be many thins, only schedule to thin, up to the number of MakeRoomBuffer or DumpBinRemainingCapacity
			local MaxToDumpBin = MakeRoomBuffer
			if ( MaxToDumpBin > DumpBinRemainingCapacity ) then
				MaxToDumpBin = DumpBinRemainingCapacity
			end
			
			-- The lists are in reverse order in how they should be processed
			-- Maintain the proper processing order of DNR's, Dammaged, Fraud, then thins
			restricted_barcodes_to_bin = {}
			barcodesInBin = tonumber("0")

			-- Take X disks from barcodesToBin
			local iStart = #barcodesToBin - MaxToDumpBin
			if iStart < 0 then
				iStart = 0
			end
			for i, v in ipairs(barcodesToBin) do
				if ( tonumber(i) > iStart and barcodesInBin < MaxToDumpBin ) then
					table.insert( restricted_barcodes_to_bin, v )
					barcodesInBin = barcodesInBin + 1
				end
			end
			
			-- Take X disks from barcodesToVmz (but not reblances)
			local rebalances_raw = get_rebalance_barcodes_raw()
			local redeploys_raw = get_redeploy_barcodes_raw()
			for i, v in ipairs(barcodesToVmz) do
				if ( barcodesInBin < MaxToDumpBin ) then
					if ( rebalances_raw[ v.barcode ] == nil and redeploys_raw[ v.barcode ] == nil ) then
						table.insert( restricted_barcodes_to_bin, 1, v )
						barcodesInBin = barcodesInBin + 1
					end
				else
					break
				end
			end

			-- Take X disks from redeployments (but not reblances)
			for i, v in ipairs(redeployments) do
				if ( barcodesInBin < MaxToDumpBin ) then
					table.insert( restricted_barcodes_to_bin, 1, v )
					barcodesInBin = barcodesInBin + 1
				else
					break
				end
			end

			if ( barcodesInBin > 0 ) then
				barcodesToBin = restricted_barcodes_to_bin
			end

			if MaxToDumpBin > 0 then
				if #barcodesToBin > 0 then
					rebalances = {} -- ensure rebalances is an empty list
					redeployments = {} -- ensure redeployments is an empty list
					barcodesToVmz = {} -- ensure the barcodesToVmz is an empty list
					HardwareManager:vmz_thin( label, schedule_time, barcodesToBin, rebalances, redeployments, barcodesToVmz )
				else
					kernel.log("Tried to schedule a thin-vmz job, but there were no disks to dump!")
				end
			else 
				kernel.log("Tried to schedule a thin-vmz job, but no capacity left in dump bin!")
			end
		end
	end
	
	return t
end

