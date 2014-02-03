--
-- free-trial-email-entry-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local path = kernel.getmacro( "JVHardTake:Email:EditPath")
local title = kernel.getmacro("JVHardTake:KeyBoard:PanelTag")
local value = kernel.getmacro("JVHardTake:KeyBoard:Text") or ""

if path == "edit" then
	value = ""
	kernel.setmacro( "JVHardTake:KeyBoard:Text", "" )	
end

JVHardTake_key_board_max_length = 50
JVHardTake_key_board_minimum_length = 1
JVHardTake_key_board_value = value
kernel.setactortext( "text_label", value)

kernel.showactor("yahoo_button")
kernel.showactor("hotmail_button")
kernel.showactor("gmail_button")
kernel.showactor("aol_button")
kernel.showactor("under_score_button")
kernel.showactor("at_button")
kernel.showactor("dot_button")
kernel.showactor( "dotcom_button" )
kernel.showactor( "clear_button" )
kernel.showactor( "clear_button_icon" )
kernel.setactorwidth( "text_label", 615 )
kernel.setactorx( "back_button", 695 )
kernel.setactorx( "back_button_arrow", 715 )
kernel.showactor( "note_label" )
kernel.setactorwidth( "space_bar_button", 315 )

kernel.registeractorhitnotify(
	"cancel_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_email_entry_cancel_signup_button" )
		view_show("free_trial_cancel_popup")
	end
)

kernel.registeractorhitnotify(
	"ok_button",
	function()
		kernel.setmacro( "JVHardTake:Email:EditPath", "" )
		local text = kernel.getmacro( "JVHardTake:KeyBoard:Text" ) or ""
		local EmailValidator = newemailvalidator()
		
		if EmailValidator.validate_email( text ) == false then
				kernel.markactordirty( "error_label" )
				kernel.setactortext( "error_label", "Please enter a valid email address.") --strings.email_error_message )
			return 
		else
			view_analytics_event( "buttonPress", "free_trial_email_entry_next_button" )		
			view_show("free_trial_zip_email_confirmation_view")
		end
	end
)

kernel.registeractorhitnotify(
	"image_next_right_arrow",
	function()
		kernel.setmacro( "JVHardTake:Email:EditPath", "" )
		view_analytics_event( "buttonPress", "free_trial_email_entry_next_button" )		
		view_show("free_trial_zip_email_confirmation_view")
	end
)

if value ~= nil and string.len( value ) >= JVHardTake_key_board_minimum_length then	
	kernel.enableactor( "ok_button" )		
	kernel.setactorforegroundcolor( "ok_button", "white" )
	kernel.hideactor("image_next_right_arrow_gray")
	kernel.disableactor("image_next_right_arrow_gray")
	kernel.showactor("image_next_right_arrow")
	kernel.enableactor( "image_next_right_arrow" )
else
	kernel.disableactor( "ok_button" )
	kernel.setactorforegroundcolor( "ok_button", "gray" )
	kernel.showactor("image_next_right_arrow_gray")
	kernel.hideactor("image_next_right_arrow")
	kernel.disableactor( "image_next_right_arrow" )
end