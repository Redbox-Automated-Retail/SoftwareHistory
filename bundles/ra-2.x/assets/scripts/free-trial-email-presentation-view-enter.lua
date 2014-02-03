--
-- free-trial-email-presentation-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.registeractorhitnotify(
	"accept_optin_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_email_presentation_next_button" )
		view_show("free_trial_zip_email_confirmation_view")
	end
)

kernel.registeractorhitnotify(
	"next_button_right_arrow_icon",
	function()
		view_analytics_event( "buttonPress", "free_trial_email_presentation_next_button" )
		view_show("free_trial_zip_email_confirmation_view")
	end
)

kernel.registeractorhitnotify(
	"image_next_white_arrow_big",
	function()
		view_analytics_event( "buttonPress", "free_trial_email_presentation_next_button" )
		view_show("free_trial_zip_email_confirmation_view")
	end
)

kernel.registeractorhitnotify(
	"reject_optin_button",
	function()		
		view_analytics_event( "buttonPress", "free_trial_email_presentation_cancel_signup_button" )
		view_show("free_trial_cancel_popup")
	end
)

--kernel.setactortext( "accept_optin_button", "NEXT") --strings.confirm_button )
kernel.setactortext( "reject_optin_button", "CANCEL SIGNUP") --strings.cancel_signup_button)
kernel.setactortext( "free_trial_email_presentation_current_email", kernel.getmacro("JVHardTake:KeyBoard:Text"))
