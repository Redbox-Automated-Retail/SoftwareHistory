--
-- pickup-next.lua
-- Copyright (C) 2020 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

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

local function reservation_error_handler()
	local function continue_func()
		kernel.close_open_session_and_start_new_rental_session("reservation_error_handler")
	end

	kernel.show_reservation_not_found_view(continue_func, continue_func)
end

swipe_view_next_func = function()
	ReservationManager:start_new()

	local on_pickup_validation_failure = function()
        kernel.on_pickup_validation_failure()
	end

	if kernel.get_account_reservations_count(true) == 0 then
		kernel.log( "get_account_reservations() returns null or zero rows in swipe_view_next_funct()" )
		reservation_error_handler()
	else
		local message_view_manager = newmessageviewmanager()
        local account_reservation_info = kernel.get_account_reservations_info()
		local reference_numbers = account_reservation_info.reference_numbers  -- {}
		local non_rap_reference_numbers =  account_reservation_info.non_rap_reference_numbers --{}
		local rap_reference_numbers = account_reservation_info.rap_reference_numbers --{}
		local number_of_reservations = account_reservation_info.number_of_reservations --0
        
        kernel.log("number_of_reservations matching card hash: "..tostring(number_of_reservations))
        kernel.log("reference_numbers: ")
        for ref_num, v in pairs(reference_numbers) do 
            kernel.log(tostring(v))
        end
        kernel.log("non_rap_reference_numbers: ")
        for ref_num, v in pairs(non_rap_reference_numbers) do
            kernel.log(tostring(v))
        end
        kernel.log("rap_reference_numbers: ")
        for ref_num, v in pairs(rap_reference_numbers) do
            kernel.log(tostring(v))
        end

		local continue_func = function()
			view_show_safe( "vend_view" )
			ReservationManager.vend_manager:show_movie_or_coming_soon()

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
	        ShoppingCart.vend_manager:show_movie_or_coming_soon()
	        ShoppingCart:vend_reservation_cart( email_address, reference_number, false )
        end

        local vend_all_titles = function(email_address, reference_number)
	        kernel.log( "recommendation_on_pickup_view:vend_all_titles" )
	        view_show_safe( "vend_view" )
	        ShoppingCart.vend_manager:show_movie_or_coming_soon()
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

		local pickup_authorize = function( reference_number, cart_was_modified, on_complete )
			kernel.log("pickup_authorize called for reference number " .. reference_number)
			ShoppingCart:pickupauthorize(
				reference_number,
				cart_was_modified,
				function( reference_number, result )
					kernel.log("pickup_authorize complete callback called for reference number " .. reference_number)
					if on_complete ~= nil then
						on_complete( reference_number, result )
					end
				end
			)
		end

		local cancel_reservation_for_offline = function(reference_number)
			kernel.cancel_reservation( reference_number )

			kernel.resvsvcs_cancel(
				StoreManager:get_number(),
				reference_number,
				nil
			)

			local on_complete = function()
				kernel.close_open_session_and_start_new_rental_session()
			end

			kernel.show_authorize_at_pickup_offline_message( on_complete )
		end

		local cancel_reservation_for_declined = function(reference_number)
			kernel.cancel_reservation( reference_number )

			kernel.resvsvcs_cancel(
				StoreManager:get_number(),
				reference_number,
				nil
			)

			local on_complete = function()
				message_view_manager:hide( true )
				kernel.close_open_session_and_start_new_rental_session()
			end
						
			message_view_manager:show()
			if kernel.getculture() == "en-US" then
				message_view_manager:set_title( strings.authorize_at_pickup_declined_header )
			else
				message_view_manager:set_message_header( strings.authorize_at_pickup_declined_header )
			end
			message_view_manager:set_message( strings.authorize_at_pickup_declined_message )
			message_view_manager:hide_first_button()
			message_view_manager:show_second_button( strings.continue_button, on_complete )
			message_view_manager:start_timer( on_complete )
		end

		local rap_rop_on_complete = function( reference_number, result )
			kernel.log("rap_rop_on_complete called for reference number " .. reference_number)
			if result.success == true then
				kernel.shopping_cart_commit( ShoppingCart.session_id, "Pickup Authorize Successful." )
				vend_all_titles(email_address, reference_number)
			elseif result.properties.is_online == false or result.properties.stand_alone == true then
				cancel_reservation_for_offline(reference_number)
			elseif result.properties.response =="RopRentalLimitExceeded" then
				kernel.log( "recommendation_on_pickup_view:pickup_authorize - added items exceed rental limit" )
				local message_view_manager = newtwolinemessageviewmanager()
				message_view_manager:show()
				view_analytics_event( "context", tostring( result.properties.response ) )
				message_view_manager:set_message_header( strings.rop_auth_fail_header )
				message_view_manager:set_message_line_one( strings.rop_auth_fail_message_line_one )
				message_view_manager:set_message_line_two( strings.rop_auth_fail_message_line_two )
                        
				message_view_manager:start_timer( 
                    function() 
						kernel.shopping_cart_commit( ShoppingCart.session_id, "Pickup Authorize Successful for original reservation only." )
                        vend_only_reserved_titles(email_address, reference_number)
                    end
                ) 
			else
				cancel_reservation_for_declined(reference_number)
			end
		end

		local only_rap_reservation_on_complete = function( reference_number, result )
			kernel.log("only_rap_reservation_on_complete called for reference number " .. reference_number)
			if result.success == true then
				kernel.shopping_cart_commit(ShoppingCart.session_id, "Pickup Authorize Successful." )
				kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )

				kernel.analyticsserviceaddropflowfalse()
                kernel.show_reservation_details_view(continue_func)
			elseif result.properties.is_online == false or result.properties.stand_alone == true then
				cancel_reservation_for_offline(reference_number)
			else
				cancel_reservation_for_declined(reference_number)
			end
		end

		local first_rap_reservation_on_complete
		local middle_rap_reservation_on_complete
		local last_rap_reservation_on_complete
		local rap_auth_success = false

		local function get_next_rap_reference_number( current_reference_number )
			kernel.log("get_next_rap_reference_number called for reference number " .. current_reference_number)
			for index, ref in ipairs( rap_reference_numbers ) do
				kernel.log("get_next_rap_reference_number rap_reference_numbers[" .. tostring(index) .. "] =  " .. ref)
				if tostring(ref) == tostring(current_reference_number) and rap_reference_numbers[index + 1] ~= nil then
				kernel.log("get_next_rap_reference_number returning reference number " ..rap_reference_numbers[index + 1])
					return rap_reference_numbers[index + 1]
				end
			end
			kernel.log("get_next_rap_reference_number returning nil. ")
			return nil
		end

		local function get_rap_on_complete( reference_number )
			kernel.log("get_rap_on_complete called for reference number " .. reference_number)
			for index, ref in ipairs( rap_reference_numbers ) do
				if ref == reference_number then
					if index == #rap_reference_numbers then
						kernel.log("get_rap_on_complete returning last_rap_reservation_on_complete ")
						return last_rap_reservation_on_complete
					else
						kernel.log("get_rap_on_complete returning middle_rap_reservation_on_complete ")
						return middle_rap_reservation_on_complete
					end
				end
			end
  		    kernel.log("get_rap_on_complete returning nil ")
			return nil
		end

		first_rap_reservation_on_complete = function( reference_number, result )
			kernel.log("first_rap_reservation_on_complete called for reference number " .. reference_number)
			if result.success == true then
				rap_auth_success = true
				--next rap reservation
				local next_rap_reference_number = get_next_rap_reference_number( reference_number )
				local next_rap_on_complete = get_rap_on_complete( next_rap_reference_number )
				if next_rap_reference_number ~= nil and next_rap_on_complete ~= nil then
					kernel.set_session_value("UserInfo.PostalCode", '99999')
					kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, next_rap_reference_number)
					pickup_authorize(next_rap_reference_number, false, next_rap_on_complete)
				end
			elseif result.properties.is_online == false or result.properties.stand_alone == true then
				for index, ref in ipairs( rap_reference_numbers ) do
					kernel.cancel_reservation( ref )
					
					kernel.resvsvcs_cancel(
						StoreManager:get_number(),
						ref,
						nil
					)
				end

				local on_complete = function()
					if #non_rap_reference_numbers > 0 then
						if continue_func ~= nil then
							continue_func()
						end
					end
					kernel.close_open_session_and_start_new_rental_session()
				end

				kernel.show_authorize_at_pickup_offline_message( on_complete )
			elseif result.properties.response == "ReservationCancelled" then
				kernel.cancel_reservation( reference_number )
					
				kernel.resvsvcs_cancel(
					StoreManager:get_number(),
					reference_number,
					nil
				)

				--next rap reservation
				local next_rap_reference_number = get_next_rap_reference_number( reference_number )
				local next_rap_on_complete = get_rap_on_complete( next_rap_reference_number )
				if next_rap_reference_number ~= nil and next_rap_on_complete ~= nil then
					kernel.set_session_value("UserInfo.PostalCode", '99999')
					kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, next_rap_reference_number)
					pickup_authorize(next_rap_reference_number, false, next_rap_on_complete)
				end
			else
				for index, ref in ipairs( rap_reference_numbers ) do
					kernel.cancel_reservation( ref )
					
					kernel.resvsvcs_cancel(
						StoreManager:get_number(),
						ref,
						nil
					)
				end

				local on_complete = function()
					message_view_manager:hide( true )
					if #non_rap_reference_numbers > 0 then
						if continue_func ~= nil then
							continue_func()
						end
					end
					kernel.close_open_session_and_start_new_rental_session()
				end
						
				message_view_manager:show()
				if kernel.getculture() == "en-US" then
					message_view_manager:set_title( strings.authorize_at_pickup_declined_header )
				else
					message_view_manager:set_message_header( strings.authorize_at_pickup_declined_header )
				end
				message_view_manager:set_message( strings.authorize_at_pickup_declined_message )
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button( strings.continue_button, on_complete )
				message_view_manager:start_timer( on_complete )
			end
		end

		middle_rap_reservation_on_complete = function( reference_number, result )
			kernel.log("middle_rap_reservation_on_complete called for reference number " .. reference_number)
			if result.success == true then
				rap_auth_success = true
				--next rap reservation
				local next_rap_reference_number = get_next_rap_reference_number( reference_number )
				local next_rap_on_complete = get_rap_on_complete( next_rap_reference_number )
				if next_rap_reference_number ~= nil and next_rap_on_complete ~= nil then
					kernel.set_session_value("UserInfo.PostalCode", '99999')
					kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, next_rap_reference_number)
					pickup_authorize(next_rap_reference_number, false, next_rap_on_complete)
				end
			elseif result.properties.response == "ReservationCancelled" then
				kernel.cancel_reservation( reference_number )
					
				kernel.resvsvcs_cancel(
					StoreManager:get_number(),
					reference_number,
					nil
				)

				--next rap reservation
				local next_rap_reference_number = get_next_rap_reference_number( reference_number )
				local next_rap_on_complete = get_rap_on_complete( next_rap_reference_number )
				if next_rap_reference_number ~= nil and next_rap_on_complete ~= nil then
					kernel.set_session_value("UserInfo.PostalCode", '99999')
					kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, next_rap_reference_number)
					pickup_authorize(next_rap_reference_number, false, next_rap_on_complete)
				end
			else
			        kernel.log("middle_rap_reservation_on_complete rap_reference_numbers: ")
					for ref_num, v in pairs(rap_reference_numbers) do
						kernel.log(tostring(v))
					end

				local current_index = 0
				for index, ref in ipairs( rap_reference_numbers ) do
					if tostring(ref) == tostring(reference_number) then
						current_index = index
					end
				end
				kernel.log("current_index:" .. current_index)
				if current_index ~= 0 then
					for index, ref in ipairs( rap_reference_numbers ) do
						if index >= current_index then
							kernel.cancel_reservation( ref )

							kernel.resvsvcs_cancel(
								StoreManager:get_number(),
								ref,
								nil
							)
						end
					end
				end

				local on_complete = function()
					message_view_manager:hide( true )
					if continue_func ~= nil then
						continue_func()
					end
					if rap_auth_success == false then
						kernel.close_open_session_and_start_new_rental_session()
					end
				end
						
				message_view_manager:show()
				if kernel.getculture() == "en-US" then
					message_view_manager:set_title( strings.authorize_at_pickup_declined_header )
				else
					message_view_manager:set_message_header( strings.authorize_at_pickup_declined_header )
				end
				message_view_manager:set_message( strings.authorize_at_pickup_declined_message )
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button( strings.continue_button, on_complete )
				message_view_manager:start_timer( on_complete )
			end
		end

		last_rap_reservation_on_complete = function( reference_number, result )
			kernel.log("last_rap_reservation_on_complete called for reference number " .. reference_number)
			if result.success == true then
				kernel.shopping_cart_commit(ShoppingCart.session_id, "Pickup Authorize Successful." )
				kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )

				kernel.analyticsserviceaddropflowfalse()
                kernel.show_reservation_details_view(continue_func)
			elseif result.properties.response == "ReservationCancelled" then
				kernel.cancel_reservation( reference_number )
					
				kernel.resvsvcs_cancel(
					StoreManager:get_number(),
					reference_number,
					nil
				)

				kernel.shopping_cart_commit(ShoppingCart.session_id, "Pickup Authorize Successful." )
				kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )

				kernel.analyticsserviceaddropflowfalse()
                kernel.show_reservation_details_view(continue_func)
			else
				kernel.cancel_reservation( reference_number )

				kernel.resvsvcs_cancel(
					StoreManager:get_number(),
					reference_number,
					nil
				)

				local on_complete = function()
					message_view_manager:hide( true )
					if continue_func ~= nil then
						continue_func()
					end
					if rap_auth_success == false then
						kernel.close_open_session_and_start_new_rental_session()
					end
				end
						
				message_view_manager:show()
				if kernel.getculture() == "en-US" then
					message_view_manager:set_title( strings.authorize_at_pickup_declined_header )
				else
					message_view_manager:set_message_header( strings.authorize_at_pickup_declined_header )
				end
				message_view_manager:set_message( strings.authorize_at_pickup_declined_message )
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button( strings.continue_button, on_complete )
				message_view_manager:start_timer( on_complete )
			end
		end

		local rop_pickup_authorize = function(reference_number, cart_was_modified)
			pickup_authorize(reference_number, cart_was_modified, rap_rop_on_complete)
		end

		local show_recommendation_on_pickup_view = function()
			-- Add recommended information to KA
            kernel.add_recommended_products_to_analytics()
						
            -- get ready to show recommendation on pickup view
            kernel.register_reservation_authorize_callback(reservation_authorize)
            kernel.register_vend_only_reserved_titles_callback(vend_only_reserved_titles)
			kernel.register_authorize_on_pickup_callback(rop_pickup_authorize)
            -- TODO-RECO '99999' is sent up for the postal code because the postal code from the reservation is not passed through
            kernel.set_session_value("UserInfo.PostalCode", '99999')

			view_show_safe( "recommendation_on_pickup_view" )
		end

		local function cancel_all_account_reservations(cancel_reason_code)
			for index, ref in ipairs( reference_numbers ) do
				kernel.cancel_reservation( ref )
					
				kernel.resvsvcs_cancel(
					StoreManager:get_number(),
					ref,
					cancel_reason_code,
					nil
				)
			end
		end

		local function validate_pickup()
			ShoppingCart:validatepickup(
				reference_numbers,
				function( result )
					kernel.log( "pickup_reserved_product  is_online:" .. tostring( result.properties.is_online ) .. "   stand_alone:" .. tostring( result.properties.stand_alone )  )
					if result.properties.is_online == false
					or result.properties.stand_alone == true then
						kernel.log("Kiosk offline during ValidatePickup. Cancelling all reservations for the account")

						cancel_all_account_reservations()

						result.success = false

					end

					-- Set the validated reservation list.
					kernel.set_validated_reservations(result.properties.validated_reservations)

					-- refresh our ref number lists.
					account_reservation_info = kernel.get_account_reservations_info()
					reference_numbers = account_reservation_info.reference_numbers  -- {}
					non_rap_reference_numbers =  account_reservation_info.non_rap_reference_numbers --{}
					rap_reference_numbers = account_reservation_info.rap_reference_numbers --{}
					number_of_reservations = account_reservation_info.number_of_reservations --0
        
					kernel.log("number of remaining reservations matching customer: "..tostring(number_of_reservations))
					kernel.log("reference_numbers: ")
					for ref_num, v in pairs(reference_numbers) do 
						kernel.log(tostring(v))
					end
					kernel.log("non_rap_reference_numbers: ")
					for ref_num, v in pairs(non_rap_reference_numbers) do
						kernel.log(tostring(v))
					end
					kernel.log("rap_reference_numbers: ")
					for ref_num, v in pairs(rap_reference_numbers) do
						kernel.log(tostring(v))
					end
						
					if result.success then

						-- remove cancelled reservations.
						if result.properties.cancelled_reservations ~= nil then
							for i = 1, #result.properties.cancelled_reservations do
								kernel.cancel_reservation(result.properties.cancelled_reservations[i])
							end
						end

						-- check to see if there is anything left to vend.
						local barcodes = kernel.get_reserved_barcodes_to_vend()
						if #barcodes == 0 then
							kernel.log("pickup_reserved_product no valid reservations found.")
							result.success = false
						end
					end
					if result.success then
						kernel.shopping_cart_commit(ShoppingCart.session_id, "Pickup Validation Successful." )
    					local is_live = result.properties.is_online == true  and result.properties.stand_alone == false

						RecommendationOnPickup = kernel.config_getvalue( "system", "General", "RecommendationOnPickup" ) == true

						if RecommendationOnPickup == true and 
							result.properties.allow_rop == true and
							is_live and 
							kernel.is_qualified_for_recommendation_on_pickup() and 
							kernel.ttsenabled() == false then
							-- Account qualifies for Recomendations On Pickup

							show_recommendation_on_pickup_view()
						else
							kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )

							kernel.analyticsserviceaddropflowfalse()
							if #rap_reference_numbers == 1 then
								kernel.set_session_value("UserInfo.PostalCode", '99999')
								kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, rap_reference_numbers[1])
								pickup_authorize(rap_reference_numbers[1], false, only_rap_reservation_on_complete)
							else
								if #rap_reference_numbers > 0 then
									if rap_reference_numbers[1] ~= nil then
										kernel.set_session_value("UserInfo.PostalCode", '99999')
										kernel.populate_shopping_cart_with_reservation_items(ShoppingCart.session_id, rap_reference_numbers[1])
										pickup_authorize(rap_reference_numbers[1], false, first_rap_reservation_on_complete)
									end
								else
		                            kernel.show_reservation_details_view(continue_func)
								end
							end
						end
					else
						local cancel_reason_code = result.properties.cancel_reason;
						if kernel.session_get_is_blocklisted(ShoppingCart.session_id) == true then
						    cancel_reason_code = "CUSTOMER BLOCKLISTED"
						end
						if cancel_reason_code ~= nil then
							view_analytics_event( "context", "blocklisted" )
							cancel_all_account_reservations(cancel_reason_code)

							on_pickup_validation_failure()

							local blocklisted_complete = function()
								message_view_manager:hide( true )
								kernel.close_open_session_and_start_new_rental_session()
							end
							message_view_manager:hide_title()
							message_view_manager:show()
							message_view_manager:set_message( strings.authorization_failure_message )
							message_view_manager:hide_first_button()
							message_view_manager:show_second_button( strings.continue_button, blocklisted_complete )
							message_view_manager:start_timer( blocklisted_complete )
						else
							cancel_all_account_reservations()
							kernel.analyticsaddelement( "session", "recommendationsFlow", "False", ShoppingCart.session_id )

							kernel.analyticsserviceaddropflowfalse()
							show_customer_blacklisted_message( on_pickup_validation_failure )
						end
					end
				end
			)
		end

		-- Call Pickup Validation to start the actual pickup processing
		validate_pickup()

	end
end