--
-- vend-functions.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newvendmanager()
	local t = {}
    coming_soon_product_list = nil
	t.vending = false
	t.aborted_jobs = {}
	t.job_id = nil
    t.vend_job_completed = false
	t.vsm_shown_counter = 0

    t.show_wait_screen = false
    t.arcus_reset = false
    t.disc_vended_after_arcus_reset = false

	t.message_view_manager = newmessageviewmanager()

	kernel.register_is_vend_job_completed_callback( 
		function() 
			return t.vend_job_completed
		end
	)

	t.init = function( self )
		self.vending = false
		self.vend_job_completed = false
	end

	t.prepos_vend_abort = function( self )
		self.vending = false
		if self.job_id ~= nil then
			self.aborted_jobs[ self.job_id ] = true
			HardwareManager:prepos_vend_abort( self.job_id )
		end
	end

	t.prepos_vend_continue = function( self )

		HardwareManager.hardware_event_responders.VendGraphicDisplay = function( args, jobid )
            kernel.log( "VendGraphicDisplay" )
            -- flush the callback queue before proceeding because the vend job may have already finished and if so it
            -- would close the current session and start a new one.  If so we don't want to show VSM, coming  soon or vend animiation
            kernel.callbackflush()
            if self.vend_job_completed == false then
                local ad = kernel.marketing_ad_get_screen_ad( "VSM", false )
                local show_ad = ad ~= nil and ad.visible == true
			    if (show_ad == true or kernel.getresourceproperty( "vsm_image", "program_name" ) ~= nil )
                    and t.show_wait_screen == false
                then
                    if kernel.peekview() ~= "vsm_view"  then
                        kernel.log( "Show VSM" )
                        t.show_vsm_view(ad)
                    end
                elseif coming_soon_product_list ~= nil and t.show_wait_screen == false then
                    view_show_safe( "coming_soon_view" )
			    else
				    view_unwindto_safe( "vend_view", false, "HardwareManager.hardware_event_responders.VendGraphicDisplay" )
                    t.show_animation()
			    end
            else
                kernel.log("VendGraphicDisplay processing aborted because vend job" .. tostring(jobid) .. " is already completed.")
            end
		end
		HardwareManager.hardware_event_responders.PickupGraphicDisplay = function( args, jobid )
			--Display the vend animation once the kiosk starts pushing the disc out of the picker.
			view_unwindto_safe( "vend_view", false, "HardwareManager.hardware_event_responders.PickupGraphicDisplay" )
			if kernel.rental_shopping_cart_get_pickup_recommendations_shown( ShoppingCart.session_id ) == true then
                kernel.log( "Show Animation RecomendationOnPickupVend" )
                kernel.vend_view_show_animation("RecomendationOnPickupVend")
			else
                kernel.log( "Show Animation RentalsOnTheirWay (PickupGraphicDisplay)" )
                kernel.vend_view_show_animation("RentalsOnTheirWay")
			end
		end
		if quick_return_is_running == true then
			t.message_view_manager:hide()
			t.show_movie_or_coming_soon( barcodes_to_vend )
		end

		HardwareManager:prepos_vend_continue( self.job_id )
	end

	t.prepos_vend = function(
			self,
			barcodes_to_vend,
			on_complete )

		if self.vending == true then
			kernel.log( "VendManager:vend: vending flag is true, exiting function." )
			return
		end
		self.vend_job_completed = false

		HardwareManager:reset_event_responders()
        if kernel.is_arcus_reset_enabled() then
            HardwareManager.hardware_event_responders.HardwareCorrectionStart = function( args, jobid)
                kernel.notify_arcus_reset("HardwareCorrectionStart")
                self.show_wait_screen = true
                self.arcus_reset = true
            end
            HardwareManager.hardware_event_responders.HardwareCorrectionSuccess = function( args, jobid)
                self.show_wait_screen = false
                kernel.notify_arcus_reset("HardwareCorrectionSuccess")
            end
            HardwareManager.hardware_event_responders.HardwareCorrectionFailure = function( args, jobid)
                self.show_wait_screen = false
                kernel.notify_arcus_reset("HardwareCorrectionFailure")
            end
            HardwareManager.hardware_event_responders.ItemVended = function(args, jobid)
                if self.arcus_reset then
                    self.disc_vended_after_arcus_reset = true
                end
            end
        end

		kernel.log( "VendManager: barcodes_to_vend follows:" )
		for k, v in ipairs( barcodes_to_vend ) do
			kernel.log( "...VendManager: k = " .. tostring( k ) .. ", v = " .. tostring( v ) )
		end
		local result = HardwareManager:prepos_vend(
			barcodes_to_vend,
			function( status, job_result )
				if self.aborted_jobs[ job_result.job_id ] ~= nil
				and self.aborted_jobs[ job_result.job_id ] == true then
					-- We do not want to process results and send server calls if the prepos_vend was aborted
					kernel.log( "on_complete() skipped for aborted prepos_vend[ " .. tostring( job_result.job_id ) .. " ]" )
					
					-- Only change the vending flag if the aborted job is the current vending job
					-- otherwise if the customer swipes again successfully you are incorrectly setting this flag!
					if job_result.job_id == self.job_id then
						self.vend_job_completed = true
						self.vending = false
					end
					
					return
				end
 				if job_result.job_id == self.job_id then
	              self.vend_job_completed = true
			    end
				local vended_count = 0
				local vend_results = {}
				local bad_codes = {}
				local should_run_soft_sync = false
				for _, v in ipairs( job_result.results ) do
					local add_code = true
					if v.code ~= "SoftSync" then
						-- These codes are used to set Transaction status on the server, per barcode in the cart
						local status_code = "NotVended"
						if v.code == "ItemVended" then
							vended_count = vended_count + 1
							status_code = "Fullfilled"
						elseif v.code == "VendError" then
							status_code = "InventoryError"
							kernel.analyticsaddview( ShoppingCart.session_id, "message_view" )
							view_analytics_event( "context", "transactionError" )
							should_run_soft_sync = true
							table.insert( bad_codes, v.code )
						elseif v.code == "UnclaimedVend" then
							status_code = "ItemNotTaken"
							kernel.analyticsaddview( ShoppingCart.session_id, "message_view" )
							view_analytics_event( "context", "transactionError" )
							table.insert( bad_codes, v.code )
						elseif v.code == "MachineFull" then
							status_code = "InventoryCapacity"
							v.item_id = first_barcode
						elseif v.code == "PutRequiresRetry" then
							v.item_id = "UNKNOWN" --most likely a disc already in the picker, we don't call PUT on a Vend unless Unclaimed, which has its own result
							status_code = v.code
						elseif v.code == "EmptyStuckThresholdExceeded" then
							kernel.analyticsaddview( ShoppingCart.session_id, "message_view" )
							view_analytics_event( "context", "transactionError" )
							table.insert( bad_codes, v.code )
							local msg = "HAL Job ID: " .. tostring( job_result.job_id )
							msg = msg .. "<br><br>Code: EmptyStuckThresholdExceeded"
							msg = msg .. "<br><br>Message: There are too many empty/stuck failures transactions on this kiosk, please investigate."
							kernel.log(msg)
							kernel.hardware_alert_manager_send_alert(
								"HardwareError",
								"",
								msg,
								nil,
								nil,
								"EmptyStuckThresholdExceeded"
							)
						elseif v.code == "CameraCaptureFailure" then
							-- Mingle 5945
							-- This code de-couples camera failure messages from vend transactions
							add_code = false
							local msg = "HAL Job ID: " .. tostring( job_result.job_id )
							msg = msg .. "<br><br>Code: " .. v.code .. " Customer Vend" 
							msg = msg .. "<br><br>Message: The CAMERA CAPTURE instruction failed."
							kernel.log(msg)
							kernel.hardware_alert_manager_send_alert(
								"HardwareError",
								"",
								msg,
								nil,
								nil,
								v.code
							)
						else
							table.insert( bad_codes, v.code )
							if v.code == "ItemStuckInGripper"
							or v.code == "ObstructionDetected"
							or v.code == "MachineError"
							or v.code == "PutFailureMaxAttempts"
							or v.code == "MoveFailure"
							then
								kernel.analyticsaddview( ShoppingCart.session_id, "message_view" )
								view_analytics_event( "context", "transactionError" )
								status_code = "HardwareError"
							end
						end
						if add_code == true then
							kernel.log( "VendManager: adding vend result entry: status = " .. tostring( status_code ) .. ", barcode = " .. tostring( v.item_id ) )
							table.insert(
								vend_results,
								{ status = status_code, barcode = v.item_id } )
						end
					end
				end

				if on_complete ~= nil then
					-- The on_complete function will call reconcile() which will update transaction status per barcode
					kernel.log( "VendManager: calling on_complete handler." )
					on_complete( vend_results )
					kernel.log( "VendManager: on_complete handler COMPLETE." )
				end
				if should_run_soft_sync then
					--kernel.log("Soft Sync suggested but currently disabled.")
					kernel.log( "VendManager: calling run_soft_sync()" )
					run_soft_sync()
					kernel.log( "VendManager: run_soft_sync() COMPLETE" )
				end
				-- N.B. We put this on the callback queue so it happens after any soft sync that may have run
				--kernel.callbackenqueue(
				--    function()
						kernel.log( "VendManager: vended_count = " .. tostring( vended_count ) .. ", #barcodes_to_vend = " .. tostring( #barcodes_to_vend ) )
						if vended_count < #barcodes_to_vend then
							kernel.log( "VendManager: vended_count = " .. tostring( vended_count ) .. ", #barcodes_to_vend = " .. tostring( #barcodes_to_vend ) )
							kernel.log( "VendManager: Showing customer friendly error message." )
							local continue_handler = function()
								kernel.log( "VendManager:bad_codes follows:" )
								for _, v in ipairs( bad_codes ) do
									kernel.log( "...VendManager:bad code value = " .. tostring( v ) )
								end
								local did_handle = handle_critical_hardware_errors( "vend", job_result.job_id, bad_codes )
								if not did_handle then
									kernel.close_open_session_and_start_new_rental_session( "handle_critical_hardware_errors" )
								end
							end
                            kernel.show_message_popup_view_vend_error(continue_handler, continue_handler)
						else
							kernel.log( "VendManager: Vend was successful." )
							kernel.close_open_session_and_start_new_rental_session( "prepos_vend:vend successful" )
						end
                        if t.arcus_reset then
                            if t.disc_vended_after_arcus_reset then
                                kernel.notify_arcus_reset("HardwareCorrectionVendSuccess")
                            else
                                kernel.notify_arcus_reset("HardwareCorrectionVendFailure")
                            end
                        end
				--    end,
				--    nil )
			end,
			function()
				--OnReady
			end,
			function()
				--OnBusy

				-- MINGLE #2625 MIGHT HAVE TO NUKE THIS ENTIRE BLOCK, NOT SURE YET
				if quick_return_is_running == true then
					local hardwarejobs =
						HardwareManager:get_hardware_jobs_by_program(
							{"quick-return" },
							{"Pending", "Stopped", "Completed", "Suspended", "Errored", "Garbage" } )
					if #hardwarejobs > 0 then
						t.message_view_manager:show()
						t.message_view_manager:hide_title()
						t.message_view_manager:set_message( strings.vend_quick_return_wait )
						t.message_view_manager:hide_first_button()
					end
				end
			end )
		if result.success == nil or result.success == false then
			kernel.log( "WARNING: Call to VendManager:prepos_vend failed." )
			self.vending = false
			return
		end
		self.job_id = result.job_id
		self.vending = true
	end

    t.show_vsm_view = function(ad)
        local show_ad = ad ~= nil and ad.visible == true
		view_show_safe( "vsm_view" )
         if show_ad == true then
            kernel.marketing_ad_get_screen_ad( "VSM", true )
            kernel.setactorimage( "vsm_image_placeholder", ad.image_name )
         else
		    kernel.setactorimage( "vsm_image_placeholder", "vsm_image" )
         end
    end

    t.setup_vsm_view = function()
		local vsm_program_name = kernel.getresourceproperty( "vsm_image", "program_name" )
        local ad = kernel.marketing_ad_get_screen_ad( "VSM", false )
        local show_ad = ad ~= nil and ad.visible == true
		if show_ad == true or vsm_program_name ~= nil then
            t.show_vsm_view(ad)
            if show_ad == false then
			    UpdateVSMImpression( vsm_program_name )
			    kernel.analyticsaddattribute( "vendingMovie", "type", "Marketing", ShoppingCart.session_id )
			    kernel.analyticsaddattribute( "vendingMovie", "programName", vsm_program_name, ShoppingCart.session_id )
            end
        end
    end

	t.show_animation = function()
        if kernel.vend_view_get_animation() == "None" then
            kernel.log( "Show Animation PleaseWait" )
            kernel.vend_view_show_animation("PleaseWait")
		    kernel.analyticsaddattribute( "vendingMovie", "type", "Default", ShoppingCart.session_id )
        end
	end

	t.show_movie_or_coming_soon = function( barcodes_to_vend )
        kernel.log("Vend Manager:Show movie or coming soon")
        if t.show_wait_screen then
            t.show_animation()
        elseif safe_to_change_view() == true
        and kernel.vend_view_get_animation() == "None"
		and kernel.peekview() ~= "vsm_view"
		then
			if ( barcodes_to_vend == nil or #barcodes_to_vend == 0 )
			and ( kernel.current_rental_shopping_cart_item_count() == 0 ) then
                kernel.log("Vend Manager:getting reserved barcodes to vend")
                local temp_barcodes_to_vend = kernel.get_reserved_barcodes_to_vend()
			    if temp_barcodes_to_vend ~= nil and #temp_barcodes_to_vend > 0 then
				    barcodes_to_vend = temp_barcodes_to_vend
                end
			end

			kernel.analyticsaddnode( "session", "vendingMovie", ShoppingCart.session_id )

            local ad = kernel.marketing_ad_get_screen_ad( "VSM", false )
			if ( ad ~= nil and ad.visible == true ) 
                or kernel.getresourceproperty( "vsm_image", "program_name" ) ~= nil
			    or ( kernel.current_rental_shopping_cart_item_count == 0 and (barcodes_to_vend == nil or #barcodes_to_vend == 0 ))
			then
                t.setup_vsm_view()
			else
				kernel.timercreate(
					"coming-soon-timer",
					5,
					8,
					function()
						kernel.log( ">  timer executing" )
						kernel.timerstop( "coming-soon-timer")
						local title_generator = newcomingsoongenerator()

						local items_to_vend = kernel.get_rental_shopping_cart_items( ShoppingCart.session_id )
						if #items_to_vend == 0 then
							for _, barcode in ipairs( barcodes_to_vend ) do
								local barcode_to_product = BarcodeToProduct[barcode]
								local current_product_id = nil
								local master_product = nil
								if barcode_to_product ~= nil then
									current_product_id = barcode_to_product.product_id
								end
								if current_product_id ~= nil then
									master_product = MasterProductCatalog[current_product_id]
								end
								if master_product ~= nil then
									table.insert( items_to_vend, master_product )
								end
							end
						end

						local results = title_generator:get_products( items_to_vend, 3 )
						kernel.log( ">  generator results received" )
						--view_analytics_event2( "comingSoonShown", tostring( #results ), ShoppingCart.session_id )
						if #results == 0 then
							if coming_soon_scene_manager ~= nil then
								coming_soon_scene_manager:clear()
							end
                            t.show_animation()
						else
							kernel.log( ">  generator results being displayed" )
                            -- yup another ugly global
                            coming_soon_product_list = results
                            view_show_safe( "coming_soon_view" )
							kernel.analyticsaddattribute( "vendingMovie", "type", "comingSoon", ShoppingCart.session_id )
						end

						kernel.timerremove( "coming-soon-timer" )
						kernel.log( "Coming soon title display COMPLETE" )
					end )
				kernel.timerstart( "coming-soon-timer" )
			end
		end
		--kernel.analyticsclose(ShoppingCart.session_id, "False") --Need to wait until after the cart has been processed during the Reconcile()
	end

	return t
end
