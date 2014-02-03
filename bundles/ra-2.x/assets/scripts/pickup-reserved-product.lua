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
view_analytics_start( Start_Over_View_Name, "OnlinePickup" )
view_analytics_event( "buttonPress", "pickup_reservation_button" )

-- There are many divergent UI paths which lead out from here, many of which result in cancellation
-- But once the swipe view is up, the code will not know if it was an online pickup anymore
CloseAnalyticsOnStartView = true


local function reservation_error_handler()
	local function continue_func()
		ABE_ON = false
		view_unwindtobottom_safe( "reservation_error_handler" )
	end
	view_show_safe( "reservation_not_found_view" )
end

show_customer_blacklisted_message = function( on_ok_or_timerexpired )
	local next_func = function()
		if on_ok_or_timerexpired ~= nil then
			on_ok_or_timerexpired()
		end
		view_unwindto_safe( Start_Over_View_Name, true, "show_customer_blacklisted_message" )
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
	ABE_ON = false
	view_goback_safe( "swipe_view:back_button", false )
end

swipe_view_next_func = function()
	ReservationManager:start_new()
	local account_reservations = ReservationManager:get_account_reservations( true )
	local message_view_manager = newmessageviewmanager()
	if account_reservations == nil
	or table_count( account_reservations ) == 0 then
		kernel.log( "get_account_reservations() returns null or zero rows in swipe_view_next_funct()" )
		reservation_error_handler()
	else
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
						local result = {
							barcode = code,
							status = barcode_indexed_results[code] or "CustomerDisabled"
						}
						if result.status ~= "Fullfilled" then
							kernel.log( "> Adding " .. tostring( result.barcode ) .. " to reset barcode list" )
							barcodes_to_reset[#barcodes_to_reset + 1] = result.barcode
						end
						group_results[#group_results + 1] = result

						local attributes = {
							barcode = result.barcode,
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

		ShoppingCart:validatepickup(
			reference_numbers,
			function( result )
				if result.properties.is_online == false
				or result.properties.stand_alone == true then
					result.success = true
				end

				if result.success then
					ShoppingCart:commit( "Pickup Validation Successful." )

					local function qualify_for_reco()
						local qualifies = false
						local num_reserved_titles = #ReservationManager:get_barcodes_to_vend()
						local res_version_2 = ReservationManager:is_reservations_version( 2 )
						local res_version_3 = ReservationManager:is_reservations_version( 3 )

						kernel.log( "pickup_reserved_product:qualify_for_reco() - num_reservations=" .. tostring( num_reservations ) .. "   res_version_2:" .. tostring( res_version_2 ) .. "   res_version_3:" .. tostring( res_version_3 ) .. "   num_reserved_titles=" .. tostring( num_reserved_titles ) )
						if num_reservations == 1
						and num_reserved_titles < 5
						and ( res_version_2 == true or ( res_version_3 == true and ShoppingCart:is_gift_card() == false and ReservationManager:is_gc() == false ) )
						and result.properties.is_online == true
						and result.properties.stand_alone == false then
							ShoppingCart.recommendations_generator:GenerateRecomendationsTop20()
							local reco_list = ShoppingCart.recommendations_generator.recommended_list
							if #reco_list > 2 then
								qualifies = true
							end
						end
						return qualifies
					end
				
					if RecommendationOnPickup == true and qualify_for_reco() then
						-- Account qualifies for Recomendations On Pickup
						
						-- Add recommended information to KA
						local recommended_titles = ""
						for k, v in ipairs( ShoppingCart.recommendations_generator.recommended_list ) do
							recommended_titles = recommended_titles .. tostring( v.product_id )
							if k ~= #ShoppingCart.recommendations_generator.recommended_list then
								recommended_titles = recommended_titles .. ","
							end
						end
						kernel.analyticsaddelement( "session", "recommendationsFlow", "True", ShoppingCart.session_id )
						kernel.analyticsaddelement( "session", "recommendedTitles", recommended_titles, ShoppingCart.session_id )
						kernel.analyticsaddelement( "session", "recommendedTitlesShown", tostring( #ShoppingCart.recommendations_generator.recommended_list ), ShoppingCart.session_id )
						
						view_show_safe( "reco_on_pickup_view" )
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
				else
					kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )
					show_customer_blacklisted_message( on_pickup_validation_failure )
				end
			end
		)
	end
end

view_show_safe( "swipe_view" )
view_analytics_event( "context", "onlinePickup" )
