--
-- tickets-kiosk-thinking-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local email_manager = newemailmanager()
email_manager:init( 64 )

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
		remove_lock_update_timer()
		place_order()
	end
)

local email_from_card = ShoppingCart.TicketsCart.customer.Email or ""

local _1_sec_counter = 0
kernel.timercreate(
	"kiosk_thinking",
	1000,
	1000,
	function()
		_1_sec_counter = _1_sec_counter + 1
		email_from_card = ShoppingCart.TicketsCart.customer.Email 

		if email_from_card ~= "LOOKING_YOU_UP" then
			kernel.timerstop( "kiosk_thinking" )
			kernel.timerremove( "kiosk_thinking" )
			if email_from_card ~= nil and #email_from_card > 0 then
				email_manager:set_value( email_from_card )
			else
				email_manager:set_value( ShoppingCart.TicketsCart.email_info_address )
			end
			email_manager:show( strings.email_tag )
			email_manager:set_ok_button_text( "   DONE" )
		end
		if _1_sec_counter == 5 then
			kernel.timerstop( "kiosk_thinking" )
			kernel.timerremove( "kiosk_thinking" )
			email_manager:set_value( ShoppingCart.TicketsCart.email_info_address )
			email_manager:show( strings.email_tag )
			email_manager:set_ok_button_text( "   DONE" )
		end
	end 
)
kernel.timerstart( "kiosk_thinking" )
