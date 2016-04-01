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
        kernel.ttsinreturnflow(true)
		self.did_show_success_message = false
		self.message_view_manager = newmessageviewmanager()
		self.error_message_view_manager = newerrormessageviewmanager()
	end

	t.show_message = function( self, message, unwind_views )
		if self.shopping_cart ~= nil and self.shopping_cart.session_id ~= nil then
			view_analytics_event2( "context", "returnError", tostring(self.shopping_cart.session_id) )
		end
		
		if unwind_views == true then
			self.message_view_manager:show()
			self.message_view_manager:hide_title()
			self.message_view_manager:set_message( message )
			self.message_view_manager:start_timer(
				function()
					ABE_ON = false
					view_unwindtobottom_safe( "ReturnManager:show_message timer expired" )
				end
			)
			self.message_view_manager:hide_first_button()
			self.message_view_manager:show_second_button(
				strings.continue_button,
				function()
					ABE_ON = false
					view_unwindtobottom_safe( "ReturnManager:show_message second button clicked" )
				end
			)
		else
			kernel.popmovie()
			self.error_message_view_manager:show()
			self.error_message_view_manager:set_message( message )
		end
		self.has_ended = true
        kernel.ttsinreturnflow( false )
	end

	t.close_session_show_maintenance = function( self, abandon_reason, alert_type, jobid, error_code, screen_code )
		self.shopping_cart:close_analytics( "False" )
		self.shopping_cart:abandon( abandon_reason )
		self:show_message( strings.hardware_error_message, false )
		self.has_ended = true
        kernel.ttsinreturnflow( false )
		if screen_code == nil then
			screen_code = "RPCSSM"
		end
		show_maintenance_view( "return.hs", alert_type, jobid, error_code, screen_code, true )
	end

	t.show_success_message = function( self, item_id, return_type )
	
		local switch_culture_unwind = function()
			if DefaultCulture ~= nil and DefaultCulture ~= "" then
				kernel.setculture( DefaultCulture )
			else
				kernel.setculture( "en-US" )
			end
			localization_reload_resources()
			ABE_ON = false
			view_unwindtobottom_safe( "ReturnManager:show_success_message switch_culture_unwind()" )
		end
		if self.shopping_cart ~= nil and self.shopping_cart.session_id ~= nil then
			self.shopping_cart:commit( "User successfully returned product with barcode: " .. tostring( item_id ), true )
		end
		self.message_view_manager:show()
		self.message_view_manager:set_title( strings.title_thanks )
		self.message_view_manager:set_message( strings.inventory_updated_message )
		self.message_view_manager:start_timer( switch_culture_unwind, 5000 )
		self.did_show_success_message = true
		self.has_ended = true
        kernel.ttsinreturnflow( false )
	end
	
	t.show_backward_inserted_message = function( self )
		self.message_view_manager:show()
		self.message_view_manager:set_title(strings.return_error)
		self.message_view_manager:set_message(strings.backward_insert_error)
		self.message_view_manager:show_second_button(
			strings.continue_button,
			function()
				ABE_ON = false
				view_unwindtobottom_safe( "ReturnManager:show_backward_inserted_message second button clicked" )
			end
		)
		self.message_view_manager:start_timer(
			function()
				ABE_ON = false
				view_unwindtobottom_safe( "ReturnManager:show_backward_inserted_message timer expired" )
			end
		)
		self.has_ended = true
        kernel.ttsinreturnflow( false )
	end

	t.begin_return = function( self, return_type )

		self.message_view_manager:hide( true )

		local function register_return_responders( hardware_manager, return_manager  )

			hardware_manager:reset_event_responders()

			hardware_manager.hardware_event_responders.MachineError = function( args, jobid )
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
				kernel.log( "ReturnManager event responder for ReturnEndWithBackwardDisc" )
				kernel.analyticsaddelement( "return", "failureReason", "ReturnEndWithBackwardDisc", return_manager.shopping_cart.session_id )
				-- Remove this handler so it does not run when the job finally ends
				hardware_manager.hardware_event_responders.ReturnEnd = nil

				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				return_manager.shopping_cart:abandon(
					"The return job failed because the customer inserted the item backward or no barcode was present." )
				AlertManager.send_job_alert(
					"ReturnRejected",
					"HAL Script return.hs sent message ReturnEndWithBackardDisc",
					"The return job failed because the customer inserted the item backward or no barcode was present.  The item was put away as UNKNOWN.",
					jobid,
					"ReturnEndWithBackwardDisc, Customer Return")
				-- Here maintenance view check is done in the stack
				ABE_ON = false
				view_unwindtobottom_safe( "ReturnManager: ReturnEndWithBackwardDisc" )
			end

			hardware_manager.hardware_event_responders.MachineFull = function( args, jobid )
				kernel.log("ReturnManager event responder for MachineFull")
				kernel.analyticsaddelement( "return", "failureReason", "MachineFull", return_manager.shopping_cart.session_id )
				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				view_show_safe( "machine_full_view" )
			end

			hardware_manager.hardware_event_responders.NoEmptySlot = function( args, jobid )
				kernel.log("ReturnManager event responder for NoEmptySlot")
				kernel.analyticsaddelement( "return", "failureReason", "NoEmptySlot", return_manager.shopping_cart.session_id )
				--return_manager.shopping_cart:close_analytics( "True" ) -- should be handled by the close timer now
				return_manager.shopping_cart:abandon( "The return job failed because the machine is full." )
				if not in_maintenance_mode() then
					return_manager:show_message( strings.hardware_error_message, true )
				end
			end

			hardware_manager.hardware_event_responders.ReturnEnd = function( args, jobid )
				kernel.log("ReturnManager event responder for ReturnEnd")
				if not in_maintenance_mode() then
					local item_id = "UNKNOWN"
					if #args > 0 then
						item_id = args[1]
					end
					--if item_id ~= "UNKNOWN" then -- This should work but we need the message code to take down the movie and switch the views...
						return_manager:show_success_message( item_id )
					--end
				end
			end

			hardware_manager.hardware_event_responders.ItemInsertError = function( args, jobid )
				-- Remove this handler so it does not run when the job finally ends
				hardware_manager.hardware_event_responders.ReturnEnd = nil
				if not in_maintenance_mode() then
					return_manager:show_backward_inserted_message()
					view_analytics_event2( "context", "returnError", tostring(self.shopping_cart.session_id) )
				end
			end

			hardware_manager.hardware_event_responders.ReturnRetry = function( args, jobid )
				-- Deprecated
				kernel.log( "ReturnManager event responder for ReturnRetry " .. tostring( jobid ) )
			end

			hardware_manager.hardware_event_responders.DisableCancel = function( args, jobid )
				kernel.hideactor( "return_back_button" )
				kernel.hideactor( "return_back_button_arrow" )
			end

			hardware_manager.hardware_event_responders.MaxPutAwayAttemptsExceeded = function( args, jobid )
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
			function()
				--On_Ready
				ShoppingCart:add_session_event( "Hardware", "HAL return job is running." )
				if self.message_view_manager.is_showing then
					self.message_view_manager:hide( true )
				end
				if self.error_message_view_manager.is_showing then
					self.error_message_view_manager:hide( true )
					kernel.switchmovie( "return_movie" )
				end
				view_show_safe( "return_view" )
				self.has_ended = false
				-- Analytic session is only started AFTER the view changes
				kernel.analyticsaddnode( "session", "return", self.shopping_cart.session_id )
				kernel.analyticsaddelement( "return", "startTime", tostring( os.date( "%c", return_start_time )), self.shopping_cart.session_id)
			end,
			function( job_result )
				--On_Complete
                local function has_disc_detection_indicator( barcode )
                    local product = BarcodeToProduct[barcode]
                    if product ~= nil and product.product_id ~= nil then
                        local master_product = MasterProductCatalog[product.product_id]
                        if master_product ~= nil and master_product.disc_detection_indicator == true then
                            return true
                        end
                    end
                    return false
                end

                local function contains_code( results, code )
	                for _, v in ipairs( results ) do
		                if v.code == code then
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
				
				if job_result ~= nil then
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

							if return_type == "unplayable_return" then
								type_value = "UnplayableReturn"
							end
                            if EnableDiscSecurity == true
                            and has_disc_detection_indicator( v.item_id ) == true
                            and contains_code( job_result.results, "NoFraudMarkersDetected" ) == true
                            and contains_code( job_result.results, "FraudScanError" ) == false
                            and contains_code( job_result.results, "LastFraudPostError" ) == false then
                                type_value = "FailedSecurityRead"
                                failed_security_read = true
                            end

							kernel.invsvcs_return2(
								StoreManager:get_number(),
								type_value,
								v.item_id,
								v.deck,
								v.slot,
								v.return_time,
                                failed_security_read
                            )

                            if EnableDiscSecurity == true
                            and has_disc_detection_indicator( v.item_id ) == true
                            and contains_code( job_result.results, "NoFraudMarkersDetected" ) == true then
                                kernel.healthsvcs_sendalert2(
	                                StoreManager:get_number(),
	                                "ReturnRejected",
	                                "SECURITYRETURN",
	                                "Security missing on " .. tostring( v.item_id )
                                )
                            end
                            if EnableDiscSecurity == true
                            and has_disc_detection_indicator( v.item_id ) == false
                            and contains_code( job_result.results, "FraudMarkersRead" ) == true then
                                kernel.healthsvcs_sendalert2(
	                                StoreManager:get_number(),
	                                "ReturnRejected",
	                                "SECURITYRETURNERROR",
	                                "Security erroneously detected on " .. tostring( v.item_id )
                                )
                            end
                            if EnableDiscSecurity == true
                            and ( contains_code( job_result.results, "FraudScanError" ) == true
                            or contains_code( job_result.results, "LastFraudPostError" ) == true ) then
                                kernel.healthsvcs_sendalert2(
	                                StoreManager:get_number(),
	                                "ReturnRejected",
	                                "SECURITYHARDWAREERROR",
	                                "Security hardware having issues"
                                )
                            end

							returned_item_id = v.item_id
							should_show_success = ( v.message == "The item was successfully returned." )

							local lookup_product = BarcodeToProduct[v.item_id]
							local lookup_id = "NotFound"
							if lookup_product ~= nil then
								lookup_id = tostring( lookup_product.product_id )
							end
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
						self.show_success_message( returned_item_id )
					end
				end

				if ( kernel.peekview() == "return_view"
				or kernel.peekview() == "message_view" )
				and not self.has_ended then
					self.has_ended = true
                    kernel.ttsinreturnflow( false )
					self.message_view_manager:stop_timer()
					if kernel.getculture() ~= "en-US" then
						if DefaultCulture ~= nil and DefaultCulture ~= "" then
							kernel.setculture( DefaultCulture )
						else
							kernel.setculture( "en-US" )
						end
						localization_reload_resources()
						kernel.popmovie()
					end
					view_unwindto_safe( Start_Over_View_Name, true, "ReturnManager: on_complete, has_ended == false" )					
				end
			end,
			function()
				--On_Busy
				if not in_maintenance_mode() then
					-- The back button cannot cancel the job now because it is already scheduled, so hide the button
					kernel.hideactor( "return_back_button" )
					kernel.hideactor( "return_back_button_arrow" )

					self.error_message_view_manager:show()
					self.error_message_view_manager:set_message( strings.hardware_busy_message )
					self.shopping_cart:add_session_event(
						"Hardware",
						"HAL is busy running another job, user waiting for return." )
				end
			end )

		if not result.success then
			self.has_ended = true
            kernel.ttsinreturnflow( false )
			if not in_maintenance_mode() then
				kernel.log("return-product view_unwindtobottom()")
				ABE_ON = false
				view_unwindtobottom_safe( "ReturnManager: unable to create_return" )
			end
			return
		end

		kernel.registeractorhitnotify(
			"return_back_button",
			function()
				view_analytics_event2( "buttonPress", "return-product-cancel", self.shopping_cart.session_id )
				local return_end_time = os.time()
				local return_table = {
					success = "False",
					endTime = tostring( os.date( "%c", return_end_time ) ),
					duration = tostring( os.difftime( return_end_time, return_start_time) )
				}
				kernel.analyticsaddelements( "return", return_table, self.shopping_cart.session_id )
				self.shopping_cart:close_analytics( "True" )

				kernel.log( "User cancelled return: " .. tostring( result.job_id ) )
				kernel.hideactor( "return_back_button" )
				kernel.hideactor( "return_back_button_arrow" )
				start_idle_timer:reset()
				kernel.hardwareterminatejob( result.job_id )
				ABE_ON = false
				view_unwindtobottom_safe( "ReturnManager: return_back_button pressed" )
			end )

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

