--
-- tickets-production-selection-email.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
email_manager = newemailmanager()
email_manager:init(64)
email_manager:set_email_optin( ShoppingCart.TicketsCart.email_optin )
email_manager:set_message( tickets_strings.email_message_receive_event_info )

email_manager:set_validate_event(
	function( text )
		local email_manager_email_validator = newemailvalidator()
		if email_manager_email_validator.validate_email( text ) == false then
			return tickets_strings.email_error_message
		end
		return nil
	end )
email_manager:set_enter_event(
	function()
		ShoppingCart.TicketsCart.email_info_address = email_manager:get_value()

		--Email optin
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
		--Email event info
		local subscriptionTable = { [1] = { ["SubscriptionType"] = "Event", ["SubscriptionValue"] = ShoppingCart.TicketsCart.event.event_id, ["NotificationType"] = "Email" } }
		kernel.ticketsvcs_sendnotification(
			StoreManager:get_number(),
			subscriptionTable,
			email_manager:get_value(),
			nil
		)
		
		table.insert( ShoppingCart.TicketsCart.email_info_sent_product_list, ShoppingCart.TicketsCart.event.event_id )
		
		remove_lock_update_timer()
		view_goback()
	end )
email_manager:set_cancel_event(
	function()
		view_goback()
	end )
	
email_manager:set_value( ShoppingCart.TicketsCart.email_info_address or "" )
email_manager:show( tickets_strings.email_me .. ShoppingCart.TicketsCart.event.sort_name )
email_manager:set_ok_button_text( " NEXT" )
email_manager:show_ok_button_arrow()