--
-- free-trial-offer-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.executescript("free_trial_clear_user_data")
idle_timer:suspend()
kernel.executescript( "free_trial_create_need_more_time_timer" )

kernel.registeractorhitnotify(
	"accept_optin_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_offer_screen_yes_button" )
		view_show("free_trial_swipe_view")
	end
)

kernel.registeractorhitnotify(
	"reject_optin_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_offer_screen_no_thanks_button" )		
		kernel.executescript( "start_over" ) 
	end
)

kernel.setactortext( "free_trial_offer_text", strings.free_trial_offer_text )
kernel.setactortext( "accept_optin_button", "YES") --strings.yes_button )
kernel.setactortext( "reject_optin_button", "NO THANKS") --strings.no_thanks_button)
