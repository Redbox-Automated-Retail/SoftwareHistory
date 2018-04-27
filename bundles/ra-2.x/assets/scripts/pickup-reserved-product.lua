--
-- pickup-reserved-product.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Stop the reservation expire timer, so reservations can't expire while in the pickup flow
kernel.timerstop( "reservation-expire-timer" )

kernel.disableactor( "pickup_reservation_button" )

ShoppingCart = newshoppingcart()
ShoppingCart:start_new()
ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart

-- Start an analytics session if it does not already exist and capture button press on start_view
kernel.view_analytics_start("OnlinePickup")
view_analytics_event( "buttonPress", "pickup_reservation_button" )

local function reservation_error_handler()
	local function continue_func()
		kernel.close_open_session_and_start_new_rental_session( "reservation_error_handler" )
	end
	view_show_safe( "reservation_not_found_view" )
end

show_customer_blacklisted_message = function( on_ok_or_timerexpired )
	local next_func = function()
		if on_ok_or_timerexpired ~= nil then
			on_ok_or_timerexpired()
		end
		kernel.close_open_session_and_start_new_rental_session( "show_customer_blacklisted_message" )
	end
	local message_view_manager = newmessageviewmanager()
	message_view_manager:show()
	message_view_manager:hide_title()
	message_view_manager:hide_first_button()
	message_view_manager:set_blacklist_message( strings.customer_black_list_title )
	message_view_manager:set_message_text( strings.customer_black_list_body )
	message_view_manager:show_second_button( strings.okay_button, next_func )
	message_view_manager:start_timer( next_func, 15000 )
end

swipe_view_cancel_func = function()
	view_goback_safe( "swipe_view:back_button", false )
end

swipe_view_next_func = function()
	ReservationManager:start_new()

	local account_reservations = ReservationManager:get_account_reservations( true )
	local on_pickup_validation_failure = function()
		ShoppingCart:commit( "Pickup Validation Failure." )
		kernel.log( "on_pickup_validation_failure() BEGIN" )
		local barcodes_to_reset = {}
		local barcode_indexed_results = {}
		for ref, res in ipairs( account_reservations ) do
			for k, v in ipairs( res.barcodes ) do
				barcode_indexed_results[v.barcode] = "CustomerDisabled"
			end
		end
		for ref, res in pairs( account_reservations ) do
			local pick_up_results = {}
			local barcode_count = 0
			for group, codes in pairs( res.barcodes ) do
				local group_results = {}
				for _, code in ipairs(codes) do
				
					local multi_disc = false
					if BarcodeToProduct[code] ~= nil and BarcodeToProduct[code].product_id ~= nil then
						multi_disc = is_multi_disc( BarcodeToProduct[code].product_id )
					end
					
					local result = {
						barcode = code,
						multi_disc = multi_disc,
						status = barcode_indexed_results[code] or "CustomerDisabled"
					}
					if result.status ~= "Fullfilled" then
						kernel.log( "> Adding " .. tostring( result.barcode ) .. " to reset barcode list" )
						barcodes_to_reset[#barcodes_to_reset + 1] = result.barcode
					end
					group_results[#group_results + 1] = result

					local attributes = {
						barcode = result.barcode,
						multi_disc = result.multi_disc,
						status = result.status
					}
					kernel.analyticsaddnode( "onlinePickup", "cartItem", ShoppingCart.session_id )
					kernel.analyticsaddattributes( "cartItem", attributes, ShoppingCart.session_id )
					barcode_count = barcode_count + 1
				end
				pick_up_results[group] = group_results
			end
			kernel.log( "on_pickup_validation_failure() pick_up_results for reservation " .. ref )
			for group, group_results in pairs( pick_up_results ) do
				kernel.log("on_pickup_validation_failure() pick_up_results for group " .. group )
				for _, result in ipairs(group_results) do
					kernel.log(
						"on_pickup_validation_failure() group_result barcode " .. tostring( result.barcode ) ..
						" multi_disc " .. tostring( result.multi_disc ) ..
						" status " .. tostring( result.status ) )
				end
			end
			if barcode_count == 0 then
				kernel.log( "> Reservation " .. ref .. " had 0 barcodes, skipping Pickup() call" )
			else
				kernel.log( "on_pickup_validation_failure(): Calling kernel.resvsvcs_pickup" )
				kernel.resvsvcs_pickup(
					StoreManager:get_number(),
					res.email_address,
					ref,
					pick_up_results,
					ShoppingCart.blocklisted,
					nil )
			end
		end

		kernel.log( "on_pickup_validation_failure(): Removing reservations for ..." ..  string.sub( ReservationManager.card_hash_id or "NULL ", 1, 5 ) )
		Reservations:remove( ReservationManager.card_hash_id )

		if #barcodes_to_reset <= 0 then
			kernel.log( "on_pickup_validation_failure(): No barcodes to reset" )
		else
			kernel.log( "on_pickup_validation_failure(): resetting status to Known for " .. tostring(#barcodes_to_reset) .. " discs" )
			update_barcode_status( barcodes_to_reset, "Known" )
		end
		ShoppingCart:close_analytics( "False" )
		kernel.log( "on_pickup_validation_failure() COMPLETE" )
	end

	if account_reservations == nil
	or table_count( account_reservations ) == 0 then
		kernel.log( "get_account_reservations() returns null or zero rows in swipe_view_next_funct()" )
		reservation_error_handler()
	else
		local message_view_manager = newmessageviewmanager()
		local num_reservations = 0
		local reference_numbers = {}
		local count = 0
		for ref, res in pairs( account_reservations ) do
			num_reservations = num_reservations + 1
			table.insert( reference_numbers, ref )
			for _, codes in pairs( res.barcodes ) do
				count = count + #codes
			end
		end

		local continue_func = function()
			view_show_safe( "vend_view" )
			if quick_return_is_running == false then
				ReservationManager.vend_manager:show_movie_or_coming_soon()
			end

			kernel.log( "pickup_reserved_product: Vending reserved items" )
			ReservationManager:vend()
		end

		local reservation_continue = function ()
			view_analytics_event( "buttonPress", "continue_button" )
			continue_func()
		end
		

        local vend_only_reserved_titles = function(email_address, reference_number)
	        kernel.log( "recommendation_on_pickup_view:vend_only_reserved_titles" )
	        kernel.rental_shopping_cart_remove_non_reserved_products(ShoppingCart.session_id)
	        view_show_safe( "vend_view" )
	        if quick_return_is_running == false then
		        ShoppingCart.vend_manager:show_movie_or_coming_soon()
	        end
	        ShoppingCart:vend_reservation_cart( email_address, reference_number, false )
        end

        local vend_all_titles = function(email_address, reference_number)
	        kernel.log( "recommendation_on_pickup_view:vend_all_titles" )
	        view_show_safe( "vend_view" )
	        if quick_return_is_running == false then
		        ShoppingCart.vend_manager:show_movie_or_coming_soon()
	        end
	        ShoppingCart:vend_reservation_cart( email_address, reference_number, true )
        end

        local reservation_authorize = function(email_address, reference_number)
		    ShoppingCart:resv_authorize(
			    reference_number,
			    function( result )
				    if result.success == true then
					    vend_all_titles(email_address, reference_number)
				    else
					    kernel.log( "recommendation_on_pickup_view:reservation_authorize - resv_authorize failed" )
					    local message_view_manager = newtwolinemessageviewmanager()
					    message_view_manager:show()
					    if result.properties.response ~= "Approved" then
						    view_analytics_event( "context", tostring( result.properties.response ) )
					    end
					    message_view_manager:set_message_header( strings.rop_auth_fail_header )
					    message_view_manager:set_message_line_one( strings.rop_auth_fail_message_line_one )
					    message_view_manager:set_message_line_two( strings.rop_auth_fail_message_line_two )
                        
					    message_view_manager:start_timer( 
                            function() 
                                vend_only_reserved_titles(email_address, reference_number)
                            end
                        )
				    end
			    end
		    )
        end

        local get_reservation_info = function ()
            kernel.log("ReservationManager:get_reservation_info - calling get reservation barcodes callback")
            local reference_number = ""
            local reservation = nil
            local email_address = ""
            local account_reservations = ReservationManager:get_account_reservations() or {}
            kernel.log( "ReservationManager:get_reservation_info - number of reservations found: " .. tostring(#account_reservations) )
            for ref, res in pairs( account_reservations ) do
	            reference_number = ref
	            reservation = res
	            email_address = res.email_address
            end
            ShoppingCart.reservation_card_hash_id = ReservationManager.card_hash_id
            local reservation_info = {}
            reservation_info.barcodes = ReservationManager:get_barcodes_to_vend()
            reservation_info.reference_number = reference_number
            reservation_info.reservation = reservation
            reservation_info.email_address = email_address
            kernel.log( "ReservationManager:get_reservation_info - reservation info dump - reference_numer: " .. tostring( reservation_info.reference_number or "" )
                            .. " email address contains data: " .. tostring( reservation_info.email_address ~= nil and reservation_info.email_address ~= '' )
                            .. " barcodes: " .. tostring( #(reservation_info.barcodes or {}) ) )
            return reservation_info
        end

		ShoppingCart:validatepickup(
			reference_numbers,
			function( result )
				if result.properties.is_online == false
				or result.properties.stand_alone == true then
					ShoppingCart:set_blocklisted( ShoppingCart:is_card_blocklisted() )
					if ShoppingCart.blocklisted == true then
                        kernel.log("pickup_reserved_product:swipe_view_next_func blocklisted=true")
						result.success = false
					else
						result.success = true
					end
				end

				if result.success then
					ShoppingCart:commit( "Pickup Validation Successful." )

					local function qualify_for_reco()
						local qualifies = false
                        local barcodes = ReservationManager:get_barcodes_to_vend()
						local num_reserved_titles = #barcodes
						local res_version_2 = ReservationManager:is_reservations_version( 2 )
						local res_version_3 = ReservationManager:is_reservations_version( 3 )

						kernel.log( "pickup_reserved_product:qualify_for_reco() - num_reservations=" .. tostring( num_reservations ) .. "   res_version_2:" .. tostring( res_version_2 ) .. "   res_version_3:" .. tostring( res_version_3 ) .. "   num_reserved_titles=" .. tostring( num_reserved_titles ) )
						kernel.log( "pickup_reserved_product:qualify_for_reco() part 2 - is_gift_card=" .. tostring( ShoppingCart:is_gift_card() ) .. "   is_gc:" .. tostring( ReservationManager:is_gc() ) .. "   is_mnp:" .. tostring( ReservationManager:is_mnp() ) .. "   is_mdv=" .. tostring( ReservationManager:is_mdv() ) )
						kernel.log( "pickup_reserved_product:qualify_for_reco() part 3 - is_loyalty_redemption=" .. tostring( ReservationManager:is_loyalty_redemption() ) .. "   is_online:" .. tostring( result.properties.is_online ) .. "   stand_alone:" .. tostring( result.properties.stand_alone ) .. "   has_purchase=" .. tostring( ReservationManager:has_purchase() ) )
						if num_reservations == 1
    						and num_reserved_titles < 5
	    					and ( res_version_2 == true or ( res_version_3 == true and ShoppingCart:is_gift_card() == false and ReservationManager:is_gc() == false ) )
		    				and ReservationManager:is_mnp() == false
			    			and ReservationManager:is_mdv() == false
                            and ReservationManager:is_loyalty_redemption() == false
                            and ReservationManager:uses_credits() == false
    						and result.properties.is_online == true
	    					and result.properties.stand_alone == false
                            and ReservationManager:has_purchase() == false
                        then
                            if kernel.get_recommended_product_count() > 2 then
								qualifies = true
							end
						end
                        kernel.log( "pickup_reserved_product:qualify_for_reco() qualifies: " .. tostring(qualifies) )
						return qualifies
					end

                    kernel.register_get_reservation_info_callback(get_reservation_info)

                    RecommendationOnPickup = kernel.config_getvalue( "system", "General", "RecommendationOnPickup" ) == true
					if RecommendationOnPickup == true and qualify_for_reco() and kernel.ttsenabled() == false then
						-- Account qualifies for Recomendations On Pickup
						
						-- Add recommended information to KA
                        kernel.add_recommended_products_to_analytics()
						
                        -- get ready to show recommendation on pickup view
                        kernel.register_reservation_authorize_callback(reservation_authorize)
                        kernel.register_vend_only_reserved_titles_callback(vend_only_reserved_titles)
                        -- TODO-RECO '99999' is sent up for the postal code because the postal code from the reservation is not passed through
                        ShoppingCart.postal_code = '99999'

						view_show_safe( "recommendation_on_pickup_view" )
                        
					else
						kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )
						local msg = string.gsub( strings.reservation_pickup_body, "{0}", tostring( num_reservations ) )
						msg = string.gsub( msg, "{1}", tostring( count ) )
						message_view_manager:show()
						if kernel.getculture() == "en-US" then
							message_view_manager:set_title( strings.reservation_pickup_header )
						else
							message_view_manager:set_message_header( strings.reservation_pickup_header )
						end
						message_view_manager:set_message( msg )
						message_view_manager:hide_first_button()
						message_view_manager:show_second_button( strings.continue_button, reservation_continue )
						message_view_manager:start_timer( continue_func, 15000 )
					end
				elseif ShoppingCart.blocklisted == true then
					view_analytics_event( "context", "blocklisted" )
					on_pickup_validation_failure()

					local blocklisted_complete = function()
						message_view_manager:hide( true )
						kernel.executescript( "start_over" )
					end
					message_view_manager:hide_title()
					message_view_manager:show()
					message_view_manager:set_message( strings.authorization_failure_message )
					message_view_manager:hide_first_button()
					message_view_manager:show_second_button( strings.continue_button, blocklisted_complete )
					message_view_manager:start_timer( blocklisted_complete )
				else
					kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )
					show_customer_blacklisted_message( on_pickup_validation_failure )
				end
			end
		)
	end
end

kernel.set_swipe_view_parameters(swipe_view_cancel_func, swipe_view_next_func, "ReservationPickup")
view_show_safe( "swipe_view" )
view_analytics_event( "context", "onlinePickup" )
