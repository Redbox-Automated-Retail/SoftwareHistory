--
-- key-board-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--ELECTR
local title = kernel.getmacro("KeyBoard:PanelTag")
local value = kernel.getmacro("KeyBoard:Text") or ""
local emailPromo = "EMAIL"

if title == "EMAIL ADDRESS" or string.find( title, "CORREO ELECTR" ) ~= nil then
	if show_email_confirmation == nil then
		show_email_confirmation = true
	end
elseif title == "PROMO CODE" or string.find( title, "PROMOCI" ) ~= nil then
    emailPromo = "Promo"    
end

local cancel_event = current_keyboard_manager.cancel_event
if show_email_confirmation and not check_email_address then
    cancel_event = function()
		show_email_confirmation = false
					
		local message_view_manager = newmessageviewmanager()
		message_view_manager:hide( true )
		message_view_manager:show()
		message_view_manager:show_first_button( strings.email_first_button_text )
		message_view_manager:show_third_button( strings.email_second_button_text )
		kernel.registeractorhitnotify(
			"first_button",
			function()
				kernel.setmacro( "KeyBoard:Text", value or "" )
			    kernel.markactordirty( "text_label" )
				current_keyboard_manager.cancel_event()	
                if kernel.ttsenabled() == true then
				    kernel.ttsrepeat();  -- assests have been updated
			    end			
				return
			end )
		kernel.registeractorhitnotify(
			"third_button",
			function()
				view_goback_safe( "keyboardmanager:handle_actor_hit (third_button)" )
				kernel.setmacro( "KeyBoard:Text", value or "" )
			    kernel.markactordirty( "text_label" )
				kernel.setmacro( "KeyBoard:ErrorLabel", value or "" )
			    kernel.markactordirty( "error_label" )
			    if kernel.ttsenabled() == true then
				    kernel.ttsrepeat();  -- assests have been updated
			    end
			end	)
		kernel.hideactor( "mature_rating_back_button_arrow" )
		message_view_manager:set_title( strings.email_confirmation_header_message )
		message_view_manager:set_message( strings.email_confirmation_body_message )
    end
end

kernel.keyboard_show(
        emailPromo, 
        ABE_ON, 
        kernel.getculture() ~= "en-US", 
        current_keyboard_manager.cancel_text,
        current_keyboard_manager.validate_event,
        current_keyboard_manager.enter_func, 
        cancel_event)