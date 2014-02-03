--
-- tickets-check-out.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

tickets_swipe_view_cancel_func = function()
    view_unwindto_safe( "tickets_shopping_cart_view", false, "tickets_check_out:tickets_swipe_view_cancel_func" )
end

tickets_swipe_view_next_func = function()
	ShoppingCart.TicketsCart.customer.Email = "LOOKING_YOU_UP"
	kernel.ticketsvcs_getemailbycard(
		StoreManager:get_number(),
		function( result )
			if result.success == true and result.properties.Email ~= nil then
				ShoppingCart.TicketsCart.customer.Email = result.properties.Email
			else
				ShoppingCart.TicketsCart.customer.Email = ""
			end
		end
	)
	
	if ShoppingCart.TicketsCart.selected_delivery_method_id == "1" then
		if kernel.trackgetfirstname() ~= nil and kernel.trackgetfirstname() ~= ""
		and kernel.trackgetlastname() ~= nil and kernel.trackgetlastname() ~= ""
		then
			local TicketsMessageView = newticketsmessageviewmanager()
			TicketsMessageView:show()
			view_analytics_event( "context", "willCallID" )
			TicketsMessageView:set_message_header( kernel.trackgetlastname() .. " " .. kernel.trackgetfirstname() )
			TicketsMessageView:set_message( tickets_strings.will_call_confirmation_text )
			TicketsMessageView:show_first_button(
				tickets_strings.NO,
				function()
					view_analytics_event( "buttonPress", "will-call-confirmation-view-no" )
					local cust_name_manager = newcustnamemanager()
					cust_name_manager:init(64)
					cust_name_manager:set_message( tickets_strings.cust_name_message )

					cust_name_manager:set_enter_event( 
						function()
							ShoppingCart.TicketsCart.customer.FirstName = cust_name_manager:get_first_value()
							ShoppingCart.TicketsCart.customer.LastName = cust_name_manager:get_last_value()
							view_show( "tickets_zip_code_view" )
						end
					)
					cust_name_manager:show( tickets_strings.cust_name_header )
				end
			)
			TicketsMessageView:show_second_button(
				tickets_strings.YES,
				function()
					view_analytics_event( "buttonPress", "will-call-confirmation-view-yes" )
					--kernel.trackgetfirstname() returns last name and kernel.trackgetlastname() returns first name
					ShoppingCart.TicketsCart.customer.FirstName = kernel.trackgetlastname()
					ShoppingCart.TicketsCart.customer.LastName = kernel.trackgetfirstname()
					view_show( "tickets_zip_code_view" )
				end
			)
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "will-call-confirmation-view-timeout" )
					local cust_name_manager = newcustnamemanager()
					cust_name_manager:init(64)
					cust_name_manager:set_message( tickets_strings.cust_name_message )
					
					cust_name_manager:set_enter_event( 
						function()
							ShoppingCart.TicketsCart.customer.FirstName = cust_name_manager:get_first_value()
							ShoppingCart.TicketsCart.customer.LastName = cust_name_manager:get_last_value()
							view_show( "tickets_zip_code_view" )
						end
					)
					cust_name_manager:show( tickets_strings.cust_name_header )
				end
			)
		else
			local cust_name_manager = newcustnamemanager()
			cust_name_manager:init(64)
			cust_name_manager:set_message( tickets_strings.cust_name_message )

			cust_name_manager:set_enter_event( 
				function()
					ShoppingCart.TicketsCart.customer.FirstName = cust_name_manager:get_first_value()
					ShoppingCart.TicketsCart.customer.LastName = cust_name_manager:get_last_value()
					view_show( "tickets_zip_code_view" )
				end
			)
			cust_name_manager:show( tickets_strings.cust_name_header )
		end
	else
		view_show( "tickets_zip_code_view" )
	end
end

tickets_zip_code_next_authorize_func = function()
	if ShoppingCart.TicketsCart.customer.Email == "LOOKING_YOU_UP" then
		view_show( "tickets_kiosk_thinking_view" )
	else
		local email_manager = newemailmanager()
		email_manager:init( 64 )
		email_manager:set_email_optin( ShoppingCart.TicketsCart.email_optin )
		
		if ShoppingCart.TicketsCart.selected_delivery_method_id == "1" then
			email_manager:set_message( tickets_strings.email_message_will_call )
		elseif ShoppingCart.TicketsCart.selected_delivery_method_id == "2" then
			email_manager:set_message( tickets_strings.email_message_print_at_home )
		end
		
		email_manager:set_validate_event(
			function( text )
				local email_manager_email_validator = newemailvalidator()
				if email_manager_email_validator.validate_email( text ) == false then
					return tickets_strings.email_error_message
				end
				return nil
			end
		)
		email_manager:set_enter_event(
			function()
				ShoppingCart.TicketsCart.customer.Email = email_manager:get_value()
				
				if ShoppingCart.TicketsCart.email_optin == false and email_manager:get_email_optin() == true then
					local subscriptionTable = { [1] = { ["SubscriptionType"] = "General", ["SubscriptionValue"] = "", ["NotificationType"] = "Email" } }
					kernel.ticketsvcs_sendnotification(
						StoreManager:get_number(),
						subscriptionTable,
						email_manager:get_value(),
						nil
					)
					ShoppingCart.TicketsCart.email_optin = email_manager:get_email_optin()
				end
				
				remove_lock_update_timer()
				place_order()
			end
		)
		
		if ShoppingCart.TicketsCart.customer.Email ~= nil and #ShoppingCart.TicketsCart.customer.Email > 0 then
			email_manager:set_value( ShoppingCart.TicketsCart.customer.Email )
		else
			email_manager:set_value( ShoppingCart.TicketsCart.email_info_address )
		end
		
		email_manager:set_cancel_button_text( "NO THANKS" )
		email_manager:show( strings.email_tag )
		email_manager:set_ok_button_text( "   DONE" )
	end
end

place_order = function()
	view_show_safe( "tickets_auth_transaction_view" )
	PlaceOrderResponse = nil
	if not kernel.sessioncaptureaccountdata2( ShoppingCart.session_id ) then
		kernel.log( "|Tickets| WARNING: PlaceOrder was called with invalid account data." )
		view_unwindto( "tickets_shopping_cart_view", true, "PlaceOrder: invalid account data." )
		return
	end
	if ShoppingCart.TicketsCart.zip_code ~= nil then
		kernel.sessionsetpostalcode2( ShoppingCart.session_id, ShoppingCart.TicketsCart.zip_code )
	end
	ShoppingCart.TicketsCart:set_order_items()
	ShoppingCart.TicketsCart:set_unlock_tickets()
	kernel.ticketsvcs_placeorder(
		StoreManager:get_number(),
		ShoppingCart.session_id,
		ShoppingCart.TicketsCart.customer,
		ShoppingCart.TicketsCart.order_header,
		ShoppingCart.TicketsCart.order_items,
		ShoppingCart.TicketsCart.unlock_tickets,
		ShoppingCart.TicketsCart:get_promotion_ids(),
		function( result )
			kernel.log( "|Tickets| received PlaceOrder response" )
			PlaceOrderResponse = result
			kernel.log( PlaceOrderResponse )
		end
	)
	
	local timeout_counter = 0
	kernel.timercreate(
		"place_order_timer",
		1000,
		1000,
		function()
			if PlaceOrderResponse ~= nil then
				kernel.timerstop( "place_order_timer" )
				kernel.timerremove( "place_order_timer" )
				process_place_order_response()
			elseif timeout_counter == 30 then
				kernel.timerstop( "place_order_timer" )
				kernel.timerremove( "place_order_timer" )
				
				local TicketsMessageView = newticketsmessageviewmanager()
				TicketsMessageView:show()
				view_analytics_event( "context", "authenticationIssue" )
				TicketsMessageView:set_message_header( tickets_strings.sorry )
				TicketsMessageView:set_message( tickets_strings.place_order_authentication_issue )
				TicketsMessageView:show_second_button(
					tickets_strings.OK,
					function()
						view_analytics_event( "buttonPress", "place-order-timeout-ok-button" )
						view_unwindto( "tickets_shopping_cart_view", true, "PlaceOrder: User hit OK button." )
					end
				)
				TicketsMessageView:start_timer(
					function()
						view_unwindto( "tickets_shopping_cart_view", true, "PlaceOrder: Timeout." )
					end,
					10000
				)
			else
				timeout_counter = timeout_counter + 1
			end
		end
	)
	kernel.timerstart( "place_order_timer" )
end

process_place_order_response = function()
	if PlaceOrderResponse.success == true then
		view_show_safe( "tickets_purchase_confirmation_view" )
		view_analytics_event( "context", "place-order-order-id-" .. tostring( PlaceOrderResponse.properties.order_id ) )
		kernel.registeractorhitnotify(
			"done_button",
			function()
				kernel.timerstop( "confirmation_timer" )
				kernel.timerremove( "confirmation_timer" )
				view_analytics_event( "buttonPress", "purchase-confirmation-view-done-button" )
				ShoppingCart.TicketsCart:end_session()
			end
		)
		kernel.timercreate(
			"confirmation_timer",
			5000,
			nil,
			function()
				kernel.timerstop( "confirmation_timer" )
				kernel.timerremove( "confirmation_timer" )
				view_analytics_event( "context", "purchase-confirmation-view-timer" )
				ShoppingCart.TicketsCart:end_session()
			end
		)
		kernel.timerstart( "confirmation_timer" )
	else
		if ShoppingCart.TicketsCart.order_header["OrderId"] == "0" then
			ShoppingCart.TicketsCart.order_header["OrderId"] = PlaceOrderResponse.properties.order_id
		end
		view_analytics_event( "context", "place-order-error-code-" .. tostring( PlaceOrderResponse.properties.response ) )
		local TicketsMessageView = newticketsmessageviewmanager()
		TicketsMessageView:show()
		if PlaceOrderResponse.properties.response == "MerchantServiceFailure"
		or PlaceOrderResponse.properties.response == "AccountNotAuthorizedInCP"
		or PlaceOrderResponse.properties.response == "CardDeclined"
		then
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			TicketsMessageView:set_message( tickets_strings.place_order_card_not_authorized )
			TicketsMessageView:show_second_button(
				tickets_strings.OK,
				function()
					view_analytics_event( "buttonPress", "place-order-error-code-" .. tostring( PlaceOrderResponse.properties.response ) .. "-ok-button" )
					view_unwindto( "tickets_shopping_cart_view", true, "PlaceOrder: User hit OK button." )
				end
			)
			TicketsMessageView:start_timer(
				function()
					view_unwindto( "tickets_shopping_cart_view", true, "PlaceOrder: Timeout." )
				end,
				10000
			)
		else
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			TicketsMessageView:set_message( tickets_strings.place_order_not_complete )
			TicketsMessageView:show_second_button(
				tickets_strings.OK,
				function()
					view_analytics_event( "buttonPress", "place-order-error-code-" .. tostring( PlaceOrderResponse.properties.response ) .. "-ok-button" )
					kernel.executescript( "start_over" )
				end
			)
			TicketsMessageView:start_timer(
				function()
					kernel.executescript( "start_over" )
				end,
				15000
			)
		end
	end
end

view_show_safe( "tickets_swipe_view" )
