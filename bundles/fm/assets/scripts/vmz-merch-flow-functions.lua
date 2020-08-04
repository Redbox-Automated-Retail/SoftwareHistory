--
-- vmz-merch-flow-functions.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function newvmzmerchflowfunctions()
	local t = {}
	
	t.stepnumber = 0
	t.loaded = false
	t.non_thins_in_vmz_barcode_list = {}
	t.rebalances_outside_vmz_barcode_list = {}
	t.high_thin_barcode = nil
	t.high_thin_redeploy_barcode = nil
	t.high_rebalance_barcode = nil
	t.hardware_mgr = newhardwaremanager()
	t.fma_message_box_manager = newfmamessageboxmanager()
	t.progress_manager = newprogressmanager()
	t.stepcomplete = { }
	t.max_steps = 10
	t.vmzmerchsummaryresults = {}
	t.vmzmerchsummaryresultscount = 0
	t.countofnonthinsinvmz = nil
	t.vmz_merch_flow_idle_timer = nil
	t.skipinsertmarker = false
	t.rebalance_in_data = nil
	t.rebalance_in_count = 0
	t.vmz_removed_thins_count = 0
	t.vmz_removed_rebalances_outside_vmz_count = 0
	t.inventory_count = 0
	t.fill_quantity = 0
	t.vmz_centered = false
	t.canceled_by_user = false
	
	local VmzButtonLog = function ( self, button_name )
		local button_text = nil
		local button_macrotext = kernel.getactortext( button_name )
		if button_macrotext ~= nil then
			button_text = 	kernel.getmacro( string.sub( button_macrotext, 3, #button_macrotext - 1 ) )
		end
		local message = "VMZ Flow - Button '" .. tostring(button_text) .. "' pressed"
		if self.stepnumber ~= nil then
			message = message .. " on page: '" .. (strings["vmz_merch_flow_title_" .. self.stepnumber] or "") .. "'"
		end
		kernel.logto( "FieldMaintenanceActionLog", message )
	end

	local reset_idle_timer = function ( self )
		self.vmz_merch_flow_idle_timer:stop()
		self.vmz_merch_flow_idle_timer:start()
	end
	
	local failed_disc_removal_popup = function ( self, remove_deck, remove_slot, insert_deck, insert_slot )
		local callbacks ={}
		callbacks["OK"] = function()
			self:nextstep()
			self.fma_message_box_manager:hide()
		end
		
		local message = strings.failed_disc_removal_message
		message = string.gsub( message, "{0}", tostring( remove_deck ) )
		message = string.gsub( message, "{1}", tostring( remove_slot ) )
		message = string.gsub( message, "{2}", tostring( insert_deck ) )
		message = string.gsub( message, "{3}", tostring( insert_slot ) )
		reset_idle_timer( self )
		self.fma_message_box_manager:show( strings.failed_disc_removal_header , message, "Continue", false, callbacks )
	end
	
	local failed_disc_insert_popup = function ( self, insert_deck, insert_slot )
		local callbacks ={}
		callbacks["OK"] = function()
			self:nextstep()
			self.fma_message_box_manager:hide()
		end

		local message = strings.failed_disc_insert_message
		message = string.gsub(message,"{0}", insert_deck )
		message = string.gsub(message,"{1}", insert_slot )
		reset_idle_timer( self )
		self.fma_message_box_manager:show( strings.failed_disc_insert_header , message, "Continue", false, callbacks )
	end
	
	local hardware_failure_popup = function ( self )
		local callbacks ={}
		callbacks["OK"] = function()
			kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_CloseMerchandizeVMZ",
				username,
				"FMA Bundle",
				"FMA Merchandize VMZ> OK to Hardware Failure Popup" )
			self:close()
		end
		reset_idle_timer( self )
		self.fma_message_box_manager:show( strings.hardware_error_header , strings.hardware_error_message, "Close", false, callbacks )
	end

	local close_door_popup = function ( self )
		local callbacks ={}
		callbacks["OK"] = function()
			reset_idle_timer( self )
			self.fma_message_box_manager:hide()
		end
		reset_idle_timer( self )
		self.fma_message_box_manager:show( strings.door_open_title , strings.door_open_body, "Continue", false, callbacks )
	end
	
	local rotations_exceeded_popup = function ( self )
		local callbacks ={}
		callbacks["OK"] = function()
			reset_idle_timer( self )
			self.fma_message_box_manager:hide()
		end
		reset_idle_timer( self )
		self.fma_message_box_manager:show( strings.rotations_exceeded_title , strings.rotations_exceeded_body, "Continue", false, callbacks )
	end

	local removal_hardware_move_offset = function ( self, deck, slot )
		local immediate_result = self.hardware_mgr:executeimmediate( "MOVE SLOT-OFFSET=5", false, function()
			self.progress_manager:stop()
			failed_disc_removal_popup( self, deck, slot, deck, slot )
		end )
		if immediate_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local vend_problem = function( self, deck, slot )
		local immediate_result = self.hardware_mgr:executeimmediate("LOC RESET DECK=" .. tostring(deck) .. " SLOT=" .. tostring(slot) , false, function()
			if deck ~= nil and slot ~= nil then
				-- move back to the position - then do the offset
				local command = "MOVE DECK=" .. tostring(deck) .. " SLOT=" .. tostring(slot)
				local immediate_result2 = self.hardware_mgr:executeimmediate(command , false, function()
					removal_hardware_move_offset( self, deck, slot )
				end )
				if immediate_result2 == false then
					hardware_failure_popup( self )
				end
			else
				removal_hardware_move_offset( self, deck, slot )
			end
		end)
		if immediate_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local insert_hardware_move_offset = function ( self, deck, slot )
		local immediate_result = self.hardware_mgr:executeimmediate( "MOVE SLOT-OFFSET=5", false, function()
			self.progress_manager:stop()
			failed_disc_insert_popup( self,	deck, slot )
		end )
		if immediate_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local insert_problem = function( self, deck, slot )
		kernel.log( "insert_problem on step: " .. tostring( self.stepnumber ) )
		local immediate_result = self.hardware_mgr:executeimmediate( "LOC RESET DECK=" .. tostring( deck ) .. " SLOT=" .. tostring( slot ) , false, 
			function()
				if deck ~= nil and slot ~= nil then
					-- move back to the position - then do the offset
					local command = "MOVE DECK=" .. tostring( deck ) .. " SLOT=" .. tostring( slot )
					local immediate_result2 = self.hardware_mgr:executeimmediate( command , false, function()
						insert_hardware_move_offset( self, deck, slot )
					end )
					if immediate_result2 == false then
						hardware_failure_popup( self )
					end
				else
					insert_hardware_move_offset( self, deck, slot )
				end
			end)
		if immediate_result == false then
			hardware_failure_popup( self )
		end
	end

	local remove_from_inventory = function( result_set )
		for _, v in ipairs( result_set ) do
			if v.code == "ItemVended"
			or v.code == "VmzMerch" then
				if v.item_id ~= nil 
				and v.item_id ~= "UNKNOWN" 
				and v.item_id ~= "" then
					local deck_slot_key = tostring( v.deck ).. "@".. tostring( v.slot )
					BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }
					kernel.log( "> " .. v.item_id .. " added to RemovedQueue." )
					RemovedQueue[ v.item_id ] = { ["barcode"] = v.item_id, ["status"] = "Success" }
					remove_barcode( v.item_id )
				end
			end
		end
	end
	
	local setup_vend_hardware_event_responders = function( self )
		self.hardware_mgr:reset_event_responders()
		self.hardware_mgr.hardware_event_responders.ItemVended = function( args, jobid ) self.fma_message_box_manager:increment_count() end
		self.hardware_mgr.hardware_event_responders.GripperTimedOut = function( args, jobid ) self.fma_message_box_manager:increment_count() end
		self.hardware_mgr.hardware_event_responders.ItemNotFound = function( args, jobid ) self.fma_message_box_manager:increment_count() end
		self.hardware_mgr.hardware_event_responders.MachineError = function( args, jobid ) hardware_failure_popup( self ) end
	end

	local send_disc_action = function( results, disc_action_job_name, action )
		local timestamp = nil
		if results ~= nil then
			local barcode_list = {}
			for _, v in ipairs( results ) do
				if timestamp == nil and v.time_stamp24 ~= nil and v.time_stamp24 ~= "" then
					timestamp = v.time_stamp24
				end
				if v.code == "ItemVended"
				or v.code == "VmzMerch" then
					if v.item_id ~= nil 
					and v.item_id ~= "UNKNOWN" 
					and v.item_id ~= "" then
						table.insert( barcode_list, { tostring(v.item_id), tostring(action) } ) -- action 0=Inserted  1=Removed
					else
						kernel.log("Attempting vending an unknown barcode.")
					end
				elseif v.code == "ItemNotFound" or v.code  == "UnclaimedVend" then
					--don't put in removed item disc action
				elseif v.code == "MachineError" then
					--probably can display or something
				end
			end
			if #barcode_list > 0 then
				-- Send DiscAction Message
				kernel.fmadiscaction(
					StoreID,
					username,
					disc_action_job_name,
					timestamp,
					barcode_list)
			end
		end
	end
	
	local schedule_delayed_unload = function( self )
		-- VMZ unloads can only be scheduled up specifed config setting
		local time_offset = "01:00:00"
		
		if ( UnloadDelayMinutes > 0 and UnloadDelayMinutes <= 1440 ) then
			time_offset = string.format("%02d:%02d:00", math.floor( UnloadDelayMinutes / 60 ), UnloadDelayMinutes % 60 )
		end

		local schedule_time = newdatetime(os.date("*t")) + time_offset
		local job_type = iif( VMZEnabledMachine, "VMZ", "QLM" )
		local label = job_type .. "-unload scheduled by " .. username
		self.hardware_mgr:qlm_unload(label, schedule_time.backing)
	end
	
	local close_merch_flow = function ( self )
		schedule_delayed_unload( self )

		kernel.healthsvcs_enqueuestatistic(
			"RedeployFillQuantity",
			self.fill_quantity )

		kernel.healthsvcs_sendauditevent(
			StoreID,
			"FMA_CloseMerchandizeVMZ",
			username,
			"FMA Bundle",
			"FMA Merchandize VMZ> Close selected" )

		self.progress_manager:stop()
		self:close()
	end
	
	local load_rebalances_and_new_releases_button_setup = function( self )
		if close_in_progress == true then
			kernel.log( "VMZ closing in progress: Disabling Show Rebalance Button" )
			self.stepcomplete[ self.stepnumber ] = true
		elseif self.rebalance_in_data == nil or #self.rebalance_in_data == 0 then
			kernel.log( "No Rebalances to view: Disabling Show Rebalance Button" )
			self.stepcomplete[ self.stepnumber ] = true
			kernel.disableactor( "vend_button" )
		else
			kernel.enableactor( "vend_button" )
		end

		if self.stepcomplete[ self.stepnumber ] == false or close_in_progress == true then
			kernel.disableactor( "continue_button" )
		else
			kernel.enableactor( "continue_button" )
		end
		
--		if self.fill_quantity > 0
--		and close_in_progress == false
--		and ( self.stepcomplete[ self.stepnumber ] == true or self.rebalance_in_count == 0 or enable_rotate_button == true ) then
--			enable_rotate_button = true
			kernel.enableactor( "alternate_button" )
--		else
--			kernel.disableactor( "alternate_button" )
--		end
	end
	
	local prep_unload_zone = function ( self, final_rotation )
        local job_success = self.hardware_mgr:prep_unload_zone(drum_rotations, last_deck_loaded, function( results )
			load_rebalances_and_new_releases_button_setup( self )
			local rotations_exceeded = false
			local rotation_successful = false
			local door_open = false
			local hardware_error = false
			for k, v in ipairs( results ) do
				if v.code == "LocationChangedCount" then
					rotation_successful = true
				elseif v.code == "RotationsExceeded" then
					rotations_exceeded = true
				elseif v.code == "DoorOpen" then
					door_open = true
				else
					hardware_error = true
				end
			end
			
			if hardware_error then
				self.progress_manager:stop()
				kernel.log("HardwareError: PrepUnloadZone results.code:"..tostring( results["code"]) )
				hardware_failure_popup( self )
			elseif door_open then
				self.progress_manager:stop()
				close_door_popup( self )
				kernel.enableactor( "continue_button" )
			elseif rotation_successful then
				if not final_rotation then
					self.progress_manager:stop()
					if not rotations_exceeded then
						drum_rotations = drum_rotations + 1
					else
						self.progress_manager:stop()
						rotations_exceeded_popup( self )
					end
				else
					close_merch_flow( self )
				end
			else
				if not final_rotation then
					self.progress_manager:stop()
					rotations_exceeded_popup( self )
				else
					close_merch_flow( self )
				end
			end
		end)
		
		if job_success == false then
			self.progress_manager:stop()
			hardware_failure_popup( self )
		end
	end
	
	local get_last_deck_loaded = function( self )
		kernel.setmacro("keyboard:title_area", "   # of Last Deck with New Releases")
		KeyPadController.on_show = nil
		KeyPadController.on_validate = function(last_deck)
			-- check if the slot value is greater than 0 and less than the max dec value
			if tonumber(last_deck) < 1 or tonumber(last_deck) > max_decks then
				newmessageboxmanager():show("deck number is invalid" ) 
				return false
			end
			kernel.setmacro("keyboard:title_area", "   Enter a Slot")
			last_deck_loaded = tonumber(last_deck);
			self.progress_manager:start()
			prep_unload_zone( self, true )
			return true
		end
		KeyPadController.show()
		kernel.disableactor("close_area")
		kernel.hideactor("close_area")
	end
	
	local load_rebalances_and_new_releases = function( self )
		-- set up alternate button (Rotate Drum)
		kernel.registeractorhitnotify( 
			"alternate_button",
			function()
				kernel.disableactor( "alternate_button" )
				self.progress_manager:start()
				VmzButtonLog( self, "alternate_button" )
				prep_unload_zone( self, false )
			end
		)

		-- set up vend button
		kernel.registeractorhitnotify( 
			"vend_button",
			function()
				kernel.disableactor( "vend_button" )
				VmzButtonLog( self, "vend_button" )
				self.stepcomplete[ self.stepnumber ] = true
				
				-- View Discs to Rebalance
				view_show( "vmz_merch_rebalances_view" )
				idle_timer:stop()
			end
		)

		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				close_in_progress = true
				self.progress_manager:start()
				kernel.disableactor( "alternate_button" )
				kernel.disableactor( "vend_button" )
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )

				if drum_rotations == 0 then
					get_last_deck_loaded( self )
				else
					prep_unload_zone( self, true )
				end
			end
		)
		
		load_rebalances_and_new_releases_button_setup( self )

		if close_in_progress == true then
			self.progress_manager:start()
		end
	end

	local get_barcodes_in_bin = function ( self, callback )
		local job_result = self.hardware_mgr:get_barcodes_in_bin( function( results )
			local barcodes_in_bin = {}
			for _,v in ipairs( results ) do
				if v.code == "DumpBinInventoryItem" then
					table.insert( barcodes_in_bin, {["item_id"]=tostring(v.item_id), ["code"]="VmzMerch", ["time_stamp24"]=v.time_stamp24 } )
				end
			end
			if callback ~= nil then
				callback( barcodes_in_bin )
			end
		end)
		if job_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local in_zone = function( self, deck, slot)
		local zone = nil
		if self.vmzmerchsummaryresults["NoRebalances"] == nil then
			local RD = tonumber(self.vmzmerchsummaryresults.LastRebalanceInfo.deck)
			local RS = tonumber(self.vmzmerchsummaryresults.LastRebalanceInfo.slot)
			if RD > tonumber(deck)
			or ( RD == tonumber(deck) and RS > tonumber(slot) ) then
				zone = "REBALANCE"
			end
		end
		if zone == nil and self.vmzmerchsummaryresults["NoThinRedeploys"] == nil then
			local RD = tonumber(self.vmzmerchsummaryresults.LastThinRedeployInfo.deck)
			local RS = tonumber(self.vmzmerchsummaryresults.LastThinRedeployInfo.slot)
			if RD > tonumber(deck)
			or ( RD == tonumber(deck) and RS > tonumber(slot) ) then
				zone = "REDEPLOY"
			end
		end
		if zone == nil then
			zone = "THIN"
		end
		return zone
	end

	local remove_thins_rebalances_dumpbin = function( self )
		kernel.log("Remove Thins Rebalances Dumpbin page")

		-- set up continue button
		kernel.registeractorhitnotify( "continue_button", function()
			kernel.disableactor( "continue_button" )
			self.progress_manager:start()
			VmzButtonLog( self, "continue_button" )
		
			-- Begin the process to send DiscActions for THINS, REBALANCES, DUMPBIN
			local job_result = self.hardware_mgr:vmz_removed_thins( function( results )
				local vmz_removed_rebalances = {}
				local vmz_removed_thins = {}
				local vmz_removed_thin_redeploys = {}
				local vmz_removed_nonthins = {}

				local finalize_inventory_status = function ()
					self.vmz_removed_thins_count = #vmz_removed_rebalances + #vmz_removed_thins + #vmz_removed_thin_redeploys

					if #vmz_removed_rebalances > 0 then
						remove_from_inventory( vmz_removed_rebalances )
						send_disc_action( vmz_removed_rebalances, "VMZ_Merch_Rebalance", "1" )
					end
					if #vmz_removed_thins > 0 then
						remove_from_inventory( vmz_removed_thins )
						send_disc_action( vmz_removed_thins, "VMZ_Merch_Thin", "1" )
					end
					if #vmz_removed_thin_redeploys > 0 then
						remove_from_inventory( vmz_removed_thin_redeploys )
						send_disc_action( vmz_removed_thin_redeploys, "VMZ_Merch_Thin_Redeploy", "1" )
					end

					-- Collect List of DUMPBIN barcodes and send DISCACTION
					get_barcodes_in_bin( self, function ( barcodes_in_bin )
						if #barcodes_in_bin > 0 then
							send_disc_action( barcodes_in_bin, "VMZ_Merch_Dumpbin", "1" )
						end
						
						if false == self.hardware_mgr:executeimmediate("DUMPBIN RESET", false, function() 
							if false == self.hardware_mgr:executeimmediate("DUMPBIN RESETCOUNTERMOVE", false, function()
								if false == self.hardware_mgr:executeimmediate("VMZ MARK-EMPTY", false, function()
									self.progress_manager:stop()
									self:nextstep()
								end) then
									hardware_failure_popup( self )
								end
							end) then
								hardware_failure_popup( self )
							end
						end) then
							hardware_failure_popup( self )
						end
					end)
				end

				-- Collect List of REBALANCES, THINS and send DISCACTION
				for _,v in ipairs( results ) do
					local entry = {
							["item_id"]=v.item_id,
							["deck"]=v.deck,
							["slot"]=v.slot,
							["code"]="VmzMerch",
							["time_stamp24"]=v.time_stamp24 }
							
					if v.code == "RebalanceDiskRemovedFromVMZ" then
						table.insert( vmz_removed_rebalances, entry )
					elseif v.code == "ThinDiskRemovedFromVMZ" then
						table.insert( vmz_removed_thins, entry )
					elseif v.code == "ThinRedeployDiskRemovedFromVMZ" then
						table.insert( vmz_removed_thin_redeploys, entry )
					end
				end
				
				if self.high_rebalance_barcode ~= nil then
					table.insert( vmz_removed_rebalances, {
						["item_id"]=self.high_rebalance_barcode,
						["deck"]=self.vmzmerchsummaryresults.LastRebalanceInfo.deck,
						["slot"]=self.vmzmerchsummaryresults.LastRebalanceInfo.slot,
						["code"]="VmzMerch",
						["time_stamp24"]=os.date("%Y%m%d%H%M") } )
				end
				if self.high_thin_redeploy_barcode ~= nil then
					table.insert( vmz_removed_thin_redeploys, {
						["item_id"]=self.high_thin_redeploy_barcode,
						["deck"]=self.vmzmerchsummaryresults.LastThinRedeployInfo.deck,
						["slot"]=self.vmzmerchsummaryresults.LastThinRedeployInfo.slot,
						["code"]="VmzMerch",
						["time_stamp24"]=os.date("%Y%m%d%H%M") } )
				end
				if self.high_thin_barcode ~= nil then
					table.insert( vmz_removed_thins, {
						["item_id"]=self.high_thin_barcode,
						["deck"]=self.vmzmerchsummaryresults.LastThinInfo.deck,
						["slot"]=self.vmzmerchsummaryresults.LastThinInfo.slot,
						["code"]="VmzMerch",
						["time_stamp24"]=os.date("%Y%m%d%H%M") } )
				end
				
				-- Add the VMZ non-thins to appropriate table
				local reset_list = {}
				local need_to_cleanup_non_thins = false
				for _,v in ipairs( self.non_thins_in_vmz_barcode_list ) do
					if v.removed == false then
						need_to_cleanup_non_thins = true
						local entry = {
								["item_id"]=v.item_id,
								["deck"]=v.deck,
								["slot"]=v.slot,
								["code"]="VmzMerch",
								["time_stamp24"]=v.time_stamp24 }

						-- FSR WILL remove this disc now - put it in the list to reset the location
						table.insert( reset_list, {["deck"]=v.deck, ["slot"]=v.slot } )
						
						-- Put the disc into the corresponding list
						local zone = in_zone( self, v.deck, v.slot )
						if zone == "THIN" then
							table.insert( vmz_removed_thins, entry )
						elseif zone == "REDEPLOY" then
							table.insert( vmz_removed_thin_redeploys, entry )
						elseif zone == "REBALANCE" then
							table.insert( vmz_removed_rebalances, entry )
						end
					end
				end
				
				if need_to_cleanup_non_thins == true and #reset_list > 0 then
					if false == self.hardware_mgr:reset_locations( reset_list, function()
						finalize_inventory_status()
					end) then
						hardware_failure_popup( self )
					end
				else
					finalize_inventory_status()
				end
				
			end)
			if job_result == false then
				hardware_failure_popup( self )
			end
		end)
		kernel.enableactor( "continue_button" )
	end

	local results_contain_non_continuable_error = function ( self, job_results )
		local non_continuable_error_found = false
		if job_results ~= nil then
			for _, v in ipairs( job_results.results ) do
				if v.code == "MoveFailure"
				or v.code == "PutFailureMaxAttempts"
				or v.code == "ItemStuckInGripper"
				or v.code == "ObstructionDetected"
				or v.code == "MachineError" then
					non_continuable_error_found = true
				end
			end
		end
		return non_continuable_error_found
	end
	
	local load_marker_disk = function ( self, deck, slot ) 
		local job_start_result = {}
		self.progress_manager:start()
		kernel.log( "Inserting Marker Disk:".. tostring(deck) .. "  slot:" .. tostring(slot))
		local job_start_result = self.hardware_mgr:file_marker_disk( 
			function ( job_result )
				kernel.log("Job file_marker_disk finished with code: " .. tostring( job_result.code ) )
				local code = job_result.results[#job_result.results].code
				if true == results_contain_non_continuable_error( self, job_result ) then
					hardware_failure_popup( self )
				elseif code == "ItemReturned" then
					if false == self.hardware_mgr:executeimmediate("LOC RESET DECK=" .. tostring(deck) .. " SLOT=" .. tostring(slot) , false, function()
							self.progress_manager:stop()
							self:nextstep()
						end)
					then
						hardware_failure_popup( self )
					end
				else
					insert_problem( self, deck, slot )
				end
			end,
			deck,
			slot
			)
		if job_start_result == false then
			hardware_failure_popup( self )
		end
	end

	local vend_disc_for_marker = function ( self, deck, slot, item_id )
		local job_start_result = {}
		local vend_list = {}
		kernel.log( "Vending disc for marker - deck:".. tostring(deck) .. "  slot:" .. tostring(slot))

		-- Ensure the slot is marked empty and in the RemovedQueue in case there are time outs and the job does not finish.
		local deck_slot_key = tostring( deck ).. "@".. tostring( slot )
		BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }
		kernel.log( "> " .. item_id .. " added to RemovedQueue." )
		RemovedQueue[ item_id ] = { ["barcode"] = item_id, ["status"] = "Success" }
		remove_barcode( item_id )

		table.insert( vend_list, {["deck"]=deck, ["slot"]=slot } )
		self.progress_manager:start()
		local job_start_result = self.hardware_mgr:vend_locations( vend_list,
			--on complete
			function( status, job_result )
				kernel.log("Job finished with status: " .. tostring( status ) )
				local code = "Error"
				if job_result ~= nil
				and #job_result.results > 0 then
					code = job_result.results[#job_result.results].code
				end
				if true == results_contain_non_continuable_error( self, job_result )
				or #job_result.results == 0 then
					hardware_failure_popup( self )
				elseif code == "ItemVended" then
					self.progress_manager:stop()
					self:nextstep()
				else
					self.skipinsertmarker = true
					vend_problem( self, deck, slot )
				end
			end,
			--ready
			nil,
			--onbusy
			nil
		)
		if job_start_result == nil
		or job_start_result.success == false then
			hardware_failure_popup( self )
		end
	end
	
	local set_removed_flag_non_thins_in_vmz = function( self, results )
		for _, v in ipairs( results ) do
			if v.code == "ItemVended" then
				for _,v2 in ipairs( self.non_thins_in_vmz_barcode_list ) do
					if v.item_id == v2.item_id then
						v2.removed = true
					end
				end
			end
		end
	end

	local tally_vended_rebalances_outside_vmz = function ( self, results )
		for _, v in ipairs( results ) do
			if v.code == "ItemVended" then
				if v.item_id ~= nil 
				and v.item_id ~= "UNKNOWN" 
				and v.item_id ~= "" then
					self.vmz_removed_rebalances_outside_vmz_count = self.vmz_removed_rebalances_outside_vmz_count + 1
				end
			end
		end
	end

	local vend_n_count = function ( self, title, barcode_list, cancelable, disc_action_job_name )
		local job_id = nil
		local callbacks = nil

		if cancelable then
			callbacks = {}
			callbacks["CANCEL"] = function()
				kernel.log( "User canceling vend barcodes job" )
				
				kernel.logto( "FieldMaintenanceActionLog", "VMZ Flow canceled by user: " .. tostring(username) )
				kernel.healthsvcs_sendauditevent(
					StoreID,
					"FMA_CanceledMerchandizeVMZ",
					username,
					"FMA Bundle",
					"FMA Merchandize VMZ> Canceled Flow" )
				
				self.progress_manager:start()
				kernel.hardwareterminatejob( job_id )
				self.progress_manager:stop()
				self.canceled_by_user = true
				self:close()
			end
		end
		
		-- Show vending view (Mingle #6421 & 6446)
		self.fma_message_box_manager:show( title , "", nil, cancelable, callbacks )
		self.fma_message_box_manager:increment_count()
		setup_vend_hardware_event_responders( self )
		kernel.disableactor( "vend_button" )
		kernel.enableactor( "continue_button" )
		local job_start_result = {}
		job_start_result = self.hardware_mgr:vend_locations( barcode_list,
			--on complete
			function( status, job_result )
				kernel.log("Job vending barcode list finished with status: " .. tostring( status ) )
				reset_idle_timer( self )
				
				-- Track inventory up to this point
				remove_from_inventory( job_result.results )
				send_disc_action( job_result.results, disc_action_job_name, "1" )
				if disc_action_job_name == "VMZ_Merch_NonThins" then
					set_removed_flag_non_thins_in_vmz( self, job_result.results )
				end
				if disc_action_job_name == "VMZ_Merch_NotInZone" then
					tally_vended_rebalances_outside_vmz( self, job_result.results )
				end
				
				-- Don't hide the messagebox if there was a hardware error, the error messagebox OK button will close it
				local hide_message_box = true
				if true == results_contain_non_continuable_error( self, job_result ) then
					hide_message_box = false
				end
				if hide_message_box == true then 
					self.fma_message_box_manager:hide()
				end
				
				if true == results_contain_non_continuable_error( self, job_result )
				or #job_result.results == 0 then
					hardware_failure_popup( self )
				end
			end,
			--ready
			nil,
			--onbusy
			nil
		)
		
		if job_start_result == nil
		or job_start_result.job_id == nil
		or job_start_result.success == false then
			hardware_failure_popup( self )
		else
			job_id = job_start_result.job_id
		end
	end

	local remove_rebalances_outside_vmz_through_vend_door = function ( self ) -- Mingle #6445
		-- set up vend button
		kernel.registeractorhitnotify(
			"vend_button",
			function()
				kernel.disableactor( "vend_button" )
				VmzButtonLog( self, "vend_button" )
				self.vmz_merch_flow_idle_timer:stop()
				if #self.rebalances_outside_vmz_barcode_list > 0 then
					self.fma_message_box_manager.max_count = #self.rebalances_outside_vmz_barcode_list
					vend_n_count( self, strings.vend_rebalances_outside_vmz, self.rebalances_outside_vmz_barcode_list, false, "VMZ_Merch_NotInZone" )
				end
				
				kernel.disableactor( "vend_button" )
				kernel.enableactor( "continue_button" )
			end
		)

		-- set up continue button, initially disabled
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				self:nextstep()
			end
		)
		kernel.enableactor( "vend_button" )
		kernel.disableactor( "continue_button" )
		
	end

	local load_rebalance_marker = function ( self ) -- Mingle #6443
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				load_marker_disk( self, self.vmzmerchsummaryresults.LastRebalanceInfo.deck, self.vmzmerchsummaryresults.LastRebalanceInfo.slot )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local remove_first_rebalance_from_vmz = function ( self ) -- Mingle #6441
		self.skipinsertmarker = false
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				self.high_rebalance_barcode = tostring(self.vmzmerchsummaryresults.LastRebalanceInfo.item_id)
				vend_disc_for_marker( self,
					self.vmzmerchsummaryresults.LastRebalanceInfo.deck,
					self.vmzmerchsummaryresults.LastRebalanceInfo.slot,
					self.high_rebalance_barcode )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local load_thin_marker = function ( self ) -- Mingle #6438
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function( job_result )
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				load_marker_disk( self, self.vmzmerchsummaryresults.LastThinInfo.deck, self.vmzmerchsummaryresults.LastThinInfo.slot )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local load_thin_redeploy_marker = function ( self )
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function( job_result )
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				load_marker_disk( self, self.vmzmerchsummaryresults.LastThinRedeployInfo.deck, self.vmzmerchsummaryresults.LastThinRedeployInfo.slot )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local remove_first_thin_from_vmz = function ( self ) -- Mingle #6432
		self.skipinsertmarker = false
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				self.high_thin_barcode = tostring(self.vmzmerchsummaryresults.LastThinInfo.item_id)
				vend_disc_for_marker( self,
					self.vmzmerchsummaryresults.LastThinInfo.deck,
					self.vmzmerchsummaryresults.LastThinInfo.slot,
					self.high_thin_barcode )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local remove_first_thin_redeploy_from_vmz = function ( self )
		self.skipinsertmarker = false
		-- set up continue button
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				reset_idle_timer( self )
				self.high_thin_redeploy_barcode = tostring(self.vmzmerchsummaryresults.LastThinRedeployInfo.item_id)
				vend_disc_for_marker( self,
					self.vmzmerchsummaryresults.LastThinRedeployInfo.deck,
					self.vmzmerchsummaryresults.LastThinRedeployInfo.slot,
					self.high_thin_redeploy_barcode )
			end
		)
		kernel.enableactor( "continue_button" )
	end

	local remove_non_thins_from_vmz = function ( self )	-- Mingle #6419
		-- set up vend button
		kernel.registeractorhitnotify(
			"vend_button",
			function()
				kernel.disableactor( "vend_button" )
				VmzButtonLog( self, "vend_button" )
				self.vmz_merch_flow_idle_timer:stop()
				vend_n_count( self, strings.remove_non_thins_from_vmz, self.non_thins_in_vmz_barcode_list, true, "VMZ_Merch_NonThins" )
			end
		)

		-- set up continue button, initially disabled
		kernel.registeractorhitnotify( 
			"continue_button",
			function()
				kernel.disableactor( "continue_button" )
				VmzButtonLog( self, "continue_button" )
				self:nextstep()
			end
		)
		kernel.enableactor( "vend_button" )
		kernel.disableactor( "continue_button" )
		
	end

	local calculate_fill_quantity = function( self )
			-- Calculate the number of redeploys to stock the machine with
			kernel.log("Fill Quantity Calculation: ")
			kernel.log("    Initial Fill Quantity: " .. tostring(kernel.rental_inventory_get_fill_quantity()) )
			kernel.log("    + vmz_removed_thins_count: " .. tostring(self.vmz_removed_thins_count) )
			kernel.log("    + vmz_removed_rebalances_outside_vmz_count: " .. tostring(self.vmz_removed_rebalances_outside_vmz_count) )
			kernel.log("    - inventory_count: " .. tostring(self.inventory_count) )
			kernel.log("    - rebalance_in_count: " .. tostring(self.rebalance_in_count) )

			local fill_quantity = kernel.rental_inventory_get_fill_quantity()
				+ self.vmz_removed_thins_count
				+ self.vmz_removed_rebalances_outside_vmz_count
				- self.inventory_count
				- self.rebalance_in_count

			if fill_quantity <= 10 then
				kernel.log("    Fill Quantity is too low - setting to zero" )
				fill_quantity = 0
			end
			
			kernel.log("FINAL Fill Quantity: " .. tostring(fill_quantity) )

			return fill_quantity
	end

	local get_rebalances_in_data = function ( self )
		kernel.log( "Running - get_rebalances_in_data" );
		self.progress_manager:start()

		if self.rebalance_in_data == nil then
			kernel.log("Load rebalance in")
			local rebalances_in_data = kernel.rental_inventory_get_rebalancesin()

			if rebalances_in_data ~= nil then
				local rebalances_in_data_indexed = {}
				for k, v in pairs( rebalances_in_data ) do
					table.insert( rebalances_in_data_indexed, v )
					self.rebalance_in_count = self.rebalance_in_count + v.Quantity
				end
				self.rebalance_in_data = rebalances_in_data_indexed
			end
		end

		-- The loading Chain is complete, move on the the 1st step
		self.progress_manager:stop()
		self.loaded = true -- This is set in the last of the string of hardware calls
		self:nextstep()
	end
	
	local get_current_inventory_count = function ( self )
		kernel.log( "Running - get_current_inventory_counts" );
		self.progress_manager:start()
		self.hardware_mgr:get_inventory_stats( function( results )
			-- Process the get_inventory_stats results
			local usable_empty_slots = 0
			local total_slots = 0
			for k, v in ipairs( results ) do
				if v.code == "UsableEmptyCount" then
					usable_empty_slots = tonumber( v.message )
				end
				if v.code == "TotalMachineSlots" then
					total_slots = tonumber( v.message )
				end
			end
			self.inventory_count = total_slots - usable_empty_slots

			-- Next retreive the get_rebalances_in_data
			self.progress_manager:stop()
			get_rebalances_in_data( self )
		end )
	end
	
	local get_rebalances_outside_vmz = function ( self )
		kernel.log( "Running - get_rebalances_outside_vmz" );
		self.progress_manager:start()
		self.rebalances_outside_vmz_barcode_list = {}
		get_rebalance_barcodes( false, true, function(rebalances_outside_vmz)
			if rebalances_outside_vmz ~= nil then
				if #rebalances_outside_vmz > 10 then
					for _,v in ipairs( rebalances_outside_vmz ) do
						if #self.rebalances_outside_vmz_barcode_list < 10 then
							table.insert( self.rebalances_outside_vmz_barcode_list, v )
						end 
					end
				else
					self.rebalances_outside_vmz_barcode_list = rebalances_outside_vmz
				end
			end

			-- Next retreive the get_current_inventory_count
			self.progress_manager:stop()
			get_current_inventory_count( self )
		end)
	end
	
	local get_non_thins_in_vmz = function ( self )
		kernel.log( "Running - get_non_thins_in_vmz" );
		self.progress_manager:start()
		self.non_thins_in_vmz_barcode_list = {}
		local job_result = self.hardware_mgr:get_non_thins_in_vmz( function( results )
			--code to retrieve list of non-thins
			for _,v in ipairs( results ) do
				if v.code == "NonThinInVMZ" then
					table.insert( self.non_thins_in_vmz_barcode_list, {["item_id"]=v.item_id, ["deck"]=v.deck, ["slot"]=v.slot, ["removed"]=false } )
				elseif v.code == "NonThinInVMZCount" then
					self.countofnonthinsinvmz = v.message
				else
					kernel.log( "Code Found when processing get_non_thins_in_vmz(): " .. v.code .. " message: " .. tostring(v.message) )
				end
			end

			-- we now have count...
			kernel.log( "Non Thin discs found in Vmz found " .. #self.non_thins_in_vmz_barcode_list .. " expecting " .. tostring(self.countofnonthinsinvmz) )
			self.fma_message_box_manager.max_count = self.countofnonthinsinvmz
			
			-- Next retreive the rebalances_outside_vmz
			self.progress_manager:stop()
			get_rebalances_outside_vmz( self )
		end)
		if job_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local get_vmz_merch_summary = function( self )
		kernel.log("Running - get_vmz_merch_summary")
		progress_manager:start()
		local job_result = self.hardware_mgr:get_vmz_merch_summary( 
			function ( results )
				for _,v in ipairs(results) do
					self.vmzmerchsummaryresults[tostring(v.code)] = v
                    kernel.log( "VMZ Summary Result: " .. v.code .. " - " .. v.message )
					self.vmzmerchsummaryresultscount = self.vmzmerchsummaryresultscount + 1
				end
				progress_manager:stop()
				kernel.log("self.vmzmerchsummaryresults count = " .. self.vmzmerchsummaryresultscount )
				
				-- Next, retreive the non_thins_in_vmz
				get_non_thins_in_vmz( self )
			end)
		if job_result == false then
			hardware_failure_popup( self )
		end
	end

	local function verify_hardware_status_before_launching_vmz_merch_flow( self, on_complete )
		schedule_time = newdatetime()
		local job_result = HardwareManager:get_hardware_status(
			schedule_time.backing,
			function( results )
				kernel.log( results )
				local hardware_status_ok = true
				for k, v in ipairs( results.results ) do
					if v.code ~= "HardwareStatusOk" then
						hardware_status_ok = false
					end
				end
				if hardware_status_ok then
					on_complete( self )
				else
					hardware_failure_popup( self )
				end
			end
		)
		if job_result == false then
			hardware_failure_popup( self )
		end
	end
	
	local enable_close_button = function( self )
		-- Show and Enable CANCEL button for step 1
		kernel.showactor( "merch_close_area" )
		kernel.enableactor( "merch_close_area" )
		kernel.registeractorhitnotify( 
			"merch_close_area",
			function()
				kernel.disableactor( "merch_close_area" )
				kernel.logto( "FieldMaintenanceActionLog", "VMZ Flow canceled by user: " .. tostring(username) )
				kernel.healthsvcs_sendauditevent(
					StoreID,
					"FMA_CanceledMerchandizeVMZ",
					username,
					"FMA Bundle",
					"FMA Merchandize VMZ> Canceled Flow" )
				self.canceled_by_user = true
				self:close()
			end
		)
	end

	t.createIdleTimer = function ( self )
		if kernel.timerexists("vmz_merch_flow_idle_timer") then
			kernel.timerremove("vmz_merch_flow_idle_timer")
		end
		self.vmz_merch_flow_idle_timer = newidletimer("vmz_merch_flow_idle_timer")
		self.vmz_merch_flow_idle_timer:initialize(
			VMZMerchFlowTimeOut,
			function()
				-- TODO: handle any unsent analytics regarding current state
				self.vmz_merch_flow_idle_timer:stop()
				kernel.timerremove("vmz_merch_flow_idle_timer")
				kernel.healthsvcs_sendauditevent(
					StoreID,
					"VMZ_Merch_Flow_Timeout",
					username,
					"FMA Bundle",
					"VMZ Merch Flow Timeout" )
				self:close( function() kernel.executescript("activate_rental") end )
				
			end
		)
		self.vmz_merch_flow_idle_timer:start()
	end
	
	t.load = function( self )
		kernel.log("Loading newvmzmerchflowfunctions()")
		self.stepnumber = 0 -- moves to step 1 after getting vmz_merch_summary
		self.vmz_removed_thins_count = 0
		self.vmz_removed_rebalances_outside_vmz_count = 0
		self.rebalance_in_count = 0
		self.inventory_count = 0
		self.fill_quantity = 0
		self.vmzmerchsummaryresults = {}
		self.vmzmerchsummaryresultscount = 0
		self.countofnonthinsinvmz = nil
		self.skipinsertmarker = false
		self:createIdleTimer()
		self.high_thin_barcode = nil
		self.high_rebalance_barcode = nil
		self.high_thin_redeploy_barcode = nil
		self.rebalance_in_data = nil
		self.vmz_centered = false
		
		if drum_rotations == nil then
			drum_rotations = 0
		end
		
		if last_deck_loaded == nil then
			last_deck_loaded = 0
		end
		
		if close_in_progress == nil then
			close_in_progress = false
		end
		
		kernel.logto( "FieldMaintenanceActionLog", "VMZ Flow entered by user: " .. tostring(username) )
		kernel.healthsvcs_sendauditevent(
			StoreID,
			"FMA_MerchandizeVMZ",
			username,
			"FMA Bundle",
			"FMA: VMZ Management> Merchandize VMZ selected" )

		--add value for each page/step
		self.stepcomplete = { }
		for k = 1, self.max_steps do
			table.insert( self.stepcomplete, false )
		end

		local reserved_barcodes = {}
        local reserved_barcode_list = kernel.get_reserved_barcodes()
        if reserved_barcode_list ~= nil and #reserved_barcode_list > 0 then
			for _, barcode in pairs( reserved_barcode_list ) do
				local row = {}
				row.barcode = barcode
				table.insert( reserved_barcodes, row )
			end
        end
		
		-- Call string of hardware jobs to get full status of flow
		if #reserved_barcodes ~= 0 then
			kernel.log( "Reservations in kiosk to unmark hal merch codes: " .. tostring(#reserved_barcodes) )
			self.hardware_mgr:clear_merch_status( reserved_barcodes, function() verify_hardware_status_before_launching_vmz_merch_flow( self, get_vmz_merch_summary ) end )
		else
			verify_hardware_status_before_launching_vmz_merch_flow( self, get_vmz_merch_summary )
		end

	end
	
	t.nextstep = function( self )
		kernel.log( "Next Step - Currently " .. tostring( self.stepnumber ) ) --debug code
		self.stepnumber = self.stepnumber + 1
		kernel.log( "Moving to Step " .. tostring( self.stepnumber ) )

		local skipstep = function( self )
			kernel.log( "Skipping Step " .. tostring( self.stepnumber ) )
			self:nextstep()
		end
		-- Skip Steps if needed
		-- ...may have to verify certain codes gets ran.
		if self.stepnumber == 1 and #self.non_thins_in_vmz_barcode_list == 0 then
			skipstep( self )
		elseif self.stepnumber == 2 -- Step 2 = remove_first_thin_from_vmz
		and self.vmzmerchsummaryresults["NoThins"] ~= nil then
			self.skipinsertmarker = true
			skipstep( self )
        elseif self.stepnumber == 3 
        and self.vmzmerchsummaryresults["NoThins"] == nil
        and self.vmzmerchsummaryresults["VMZThinCount"].message == "1" then
           skipstep( self )
		elseif self.stepnumber == 4  -- Step 4 = remove_first_thin_redeploy_from_vmz
        and self.vmzmerchsummaryresults["NoThinRedeploys"] ~= nil then
			self.skipinsertmarker = true
			skipstep( self )
        elseif self.stepnumber == 5 
        and self.vmzmerchsummaryresults["NoThinRedeploys"] == nil
        and self.vmzmerchsummaryresults["VMZThinRedeployCount"].message == "1" then
           skipstep( self )
		elseif self.stepnumber == 6  -- Step 6 = remove_first_rebalance_from_vmz
        and self.vmzmerchsummaryresults["NoRebalances"] ~= nil then
			self.skipinsertmarker = true
			skipstep( self )
        elseif self.stepnumber == 7 
        and self.vmzmerchsummaryresults["NoRebalances"] == nil
        and self.vmzmerchsummaryresults["VMZRebalanceCount"].message == "1" then
            skipstep( self )
        elseif ( self.stepnumber == 3	-- Step 3 = load_thin_marker
		or self.stepnumber == 5		-- Step 5 = load_thin_redeploy_marker
		or self.stepnumber == 7 )	-- Step 7 = load_rebalance_marker
		and self.skipinsertmarker == true then
			skipstep( self )
		elseif self.stepnumber == 8 -- Step 8 = remove_rebalances_outside_vmz_through_vend_door
		and #self.rebalances_outside_vmz_barcode_list == 0 then
			skipstep( self )
		else
			kernel.log( "Preparing to Configure Step " .. tostring( self.stepnumber ) )
			if self.stepnumber > self.max_steps then
				kernel.log("Passed Max # of steps, closing VMZ Merch Flow View")
				-- TODO: additional loging? analytics?
				self:close() --this correct?
			else
				self:configurestep()
			end
		end
	end
	
	t.updatetext = function( self )
		if self.stepnumber == nil then
			kernel.log( "Failed to get stepnumber... " .. tostring( self.stepnumber))
			return
		end
		
		--set content text
		kernel.setmacro( "VMZMerchFlow:Title", strings["vmz_merch_flow_title_" .. self.stepnumber] or "" )
		kernel.setmacro( "VMZMerchFlow:Body", strings["vmz_merch_flow_body_" .. self.stepnumber] or "" )
		
		if self.stepnumber == 10 then
			-- update the content text to reflect the count
			self.fill_quantity = calculate_fill_quantity( self )
			--local content = strings["vmz_merch_flow_body_" .. self.stepnumber] or ""
			--content = string.gsub( content,"{0}", tostring(self.fill_quantity) )
			--kernel.setmacro( "VMZMerchFlow:Body", tostring(content) )
			kernel.setmacro( "VMZMerchFlow:RedploymentBody", strings["vmz_merch_flow_redeployment_body_" .. self.stepnumber] or "" )
			kernel.setmacro( "VMZMerchFlow:QuantityBody", tostring(self.fill_quantity or 0) )
			kernel.setmacro( "VMZMerchFlow:RemainingBody", strings["vmz_merch_flow_remaining_body_" .. self.stepnumber] or "" )
        elseif self.stepnumber == 2 and self.vmzmerchsummaryresults.VMZThinCount.message == "1" then
            kernel.setmacro( "VMZMerchFlow:RedploymentBody", "** Only 1 thin disc exists.\nThere is no need to insert the thin marker disc.\nSkipping thin marker disc insertion step. **" )
			kernel.setmacro( "VMZMerchFlow:QuantityBody", "" )
			kernel.setmacro( "VMZMerchFlow:RemainingBody", "" ) 
        elseif self.stepnumber == 4 and self.vmzmerchsummaryresults.VMZThinRedeployCount.message == "1" then
            kernel.setmacro( "VMZMerchFlow:RedploymentBody", "** Only 1 redeploy disc exists.\nThere is no need to insert the redeploy marker disc.\nSkipping redeploy marker disc insertion step. **" )
			kernel.setmacro( "VMZMerchFlow:QuantityBody", "" )
			kernel.setmacro( "VMZMerchFlow:RemainingBody", "" ) 
        elseif self.stepnumber == 6 and self.vmzmerchsummaryresults.VMZRebalanceCount.message == "1" then
            kernel.setmacro( "VMZMerchFlow:RedploymentBody", "** Only 1 rebalance disc exists.\nThere is no need to insert the rebalance marker disc.\nSkipping rebalance marker disc insertion step. **" )
			kernel.setmacro( "VMZMerchFlow:QuantityBody", "" )
			kernel.setmacro( "VMZMerchFlow:RemainingBody", "" )
		else
			kernel.setmacro( "VMZMerchFlow:RedploymentBody", "" )
			kernel.setmacro( "VMZMerchFlow:QuantityBody", "" )
			kernel.setmacro( "VMZMerchFlow:RemainingBody", "" )
		end
		
		--set button text
		kernel.setmacro( "VMZMerchFlow:ContinueButton", strings["vmz_merch_flow_continue_button_" .. self.stepnumber] or "" )
		kernel.setmacro( "VMZMerchFlow:VendButton", strings["vmz_merch_flow_vend_button_" .. self.stepnumber] or "" )
		kernel.setmacro( "VMZMerchFlow:AlternateButton", strings["vmz_merch_flow_alternate_button_" .. self.stepnumber] or "" )
	end
	
	t.configurestep = function( self )
		kernel.log( "Configure Step " .. tostring(self.stepnumber) )
		self.canceled_by_user = false
	
		-- New step - disable the buttons until configuration is complete
		kernel.disableactor( "vend_button" )
		kernel.disableactor( "continue_button" )
		kernel.disableactor( "alternate_button" )
		
		kernel.disableactor( "merch_close_area" )
		kernel.hideactor( "merch_close_area" )
	
		local center_vmz = function( self, on_complete )
			if self.vmz_centered == false then
				self.progress_manager:start()
				-- Ensure Drum has VMZ centered for merchandizing
				if false == self.hardware_mgr:executeimmediate("VMZ MOVE-VIEWABLE", false, function()
					self.vmz_centered = true
					on_complete( self )
					self.progress_manager:stop()
				end) then
					self.progress_manager:stop()
					hardware_failure_popup( self )
				end
			else
				on_complete( self )
			end
		end
		
		self.hardware_mgr:reset_event_responders()
		reset_idle_timer( self )
		
		-- Update Screen Text
		self:updatetext()

		-- Toggle Screen Assets
		kernel.showactor( "background_body_area" )
		
		if self.stepnumber == 3 then
			kernel.showactor( "disc_image" )
			kernel.setactorimage( "disc_image", "image_orange_disc" )
		elseif self.stepnumber == 5 then
			kernel.showactor( "disc_image" )
			kernel.setactorimage( "disc_image", "image_grey_disc" )
		elseif self.stepnumber == 7 then
			kernel.showactor( "disc_image" )
			kernel.setactorimage( "disc_image", "image_blue_disc" )
		else
			kernel.hideactor( "disc_image" )
		end
		
		if  nil == strings["vmz_merch_flow_continue_button_" .. tostring(self.stepnumber)] then
			kernel.log("Hide/Disable Continue Button")
			kernel.hideactor( "continue_button" )
		else
			kernel.log("Show/Enable Continue Button")
			kernel.showactor( "continue_button" )
		end
		if  nil == strings["vmz_merch_flow_vend_button_" .. tostring(self.stepnumber)] then
			kernel.log("Hide/Disable Vend Button")
			kernel.hideactor( "vend_button" )
		else
			kernel.log("Show/Enable Vend Button")
			kernel.showactor( "vend_button" )
		end
		if  nil == strings["vmz_merch_flow_alternate_button_" .. tostring(self.stepnumber)] then
			kernel.log("Hide/Disable Alternate Button")
			kernel.hideactor( "alternate_button" )
		else
			kernel.log("Show/Enable Alternate Button")
			kernel.showactor( "alternate_button" )
			
			-- Alternate button is activated, so center the vend button assuming screen size is 1024
			kernel.setactorx( "vend_button", 512 - ( kernel.getactorwidth("vend_button") / 2 ) )
		end

		if self.stepnumber == 1 then
			enable_close_button( self )
			remove_non_thins_from_vmz( self )
		elseif self.stepnumber == 2 then
			remove_first_thin_from_vmz( self )
		elseif self.stepnumber == 3 then
			load_thin_marker( self )
		elseif self.stepnumber == 4 then
			remove_first_thin_redeploy_from_vmz( self )
		elseif self.stepnumber == 5 then
			load_thin_redeploy_marker( self )
		elseif self.stepnumber == 6 then
			remove_first_rebalance_from_vmz( self )
		elseif self.stepnumber == 7 then
			load_rebalance_marker( self )
		elseif self.stepnumber == 8 then
			remove_rebalances_outside_vmz_through_vend_door( self )
		elseif self.stepnumber == 9 then
			center_vmz( self, remove_thins_rebalances_dumpbin )
		elseif self.stepnumber == 10 then
			kernel.disableactor( "vend_button" )
			kernel.disableactor( "continue_button" )
			kernel.disableactor( "alternate_button" )
			center_vmz( self, load_rebalances_and_new_releases )
		elseif self.stepnumber > 10 then
			self:close()
		end
	end
	
	t.close = function( self, on_complete )
		self.loaded = false

		if self.canceled_by_user == false then
			VMZMerchFlowEnabled = false
		end
		self.progress_manager:start()	
		
		kernel.setmacro( "VMZMerchFlow:Title", nil )
		kernel.setmacro( "VMZMerchFlow:Body", nil )
		kernel.setmacro( "VMZMerchFlow:VendButton", nil )
		kernel.setmacro( "VMZMerchFlow:ContinueButton", nil )
		kernel.hideactor( "vend_button" )
		kernel.hideactor( "continue_button" )
		kernel.hideactor( "alternate_button" )
		
		self.vmz_merch_flow_idle_timer:stop()
		if kernel.timerexists("vmz_merch_flow_idle_timer") then
			kernel.timerremove("vmz_merch_flow_idle_timer")
		end
		
		self.hardware_mgr:soft_sync( function(soft_sync_result)
			reload_inventory()
			force_server_snapshot( soft_sync_result.results )
			
			self.progress_manager:stop()
			
			--final step in close
			if on_complete ~= nil then
				on_complete()
			else 
				view_unwindto( "vmz_view", true, "vmz_merch_flow_functions" )
                view_show( "card_reader_popup_view" )
			end
		end)

		kernel.sync_empties()
	end
	
	return t
end
