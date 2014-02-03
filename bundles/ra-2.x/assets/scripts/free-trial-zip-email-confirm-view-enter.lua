--
-- free-trial-zip-email-confirm-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.registeractorhitnotify(
	"accept_optin_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_zip_email_confirmation_next_button" )
		view_show("free_trial_terms_and_privacy_view")
	end
)

kernel.registeractorhitnotify(
	"reject_optin_button",
	function()		
		view_analytics_event( "buttonPress", "free_trial_zip_email_confirmation_cancel_signup_button" )
		view_show("free_trial_cancel_popup")
	end
)

kernel.registeractorhitnotify(
	"free_trial_edit_zip",
	function()
		view_analytics_event( "buttonPress", "free_trial_zip_email_confirmation_zip_edit" )
		kernel.setmacro( "JVHardTake:Zip:EditPath", "edit" )
		kernel.setmacro( "JVHardTake:KeyPad:Text", "" )
		view_show("free_trial_zip_code_view")
	end
)

if kernel.getmacro( "JVHardTake:User:HasProfile") == "true" then
	kernel.hideactor("free_trial_edit_email")
else
	kernel.registeractorhitnotify(
		"free_trial_edit_email",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_email_confirmation_email_edit" )
			kernel.setmacro( "JVHardTake:Email:EditPath", "edit" )
			view_show("free_trial_email_entry_view")
		end
	)
end

kernel.setactortext( "free_trial_email_presentation_text", "Please confirm or edit your info:") --strings.free_trial_email_presentation_text )
kernel.setactortext( "free_trial_edit_email", "EDIT") --strings.edit_button)
kernel.setactortext( "free_trial_edit_zip", "EDIT") --strings.edit_button)
kernel.setactortext( "accept_optin_button", "CONFIRM") --strings.confirm_button )
kernel.setactortext( "reject_optin_button", "CANCEL SIGNUP") --strings.cancel_signup_button)

local email = kernel.getmacro("JVHardTake:KeyBoard:Text")
local zip = kernel.getmacro("JVHardTake:KeyPad:Text")

kernel.setactortext( "free_trial_current_email", email)
kernel.setactortext( "free_trial_current_zip", zip)