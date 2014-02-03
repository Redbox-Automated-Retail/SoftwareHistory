--
-- free-trial-gift-card-swiped-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.registeractorhitnotify(
	"free_trial_gift_card_swiped_view_ok_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_gift_card_swiped_ok_button" )
		kernel.timerremove( "free_trial_gift_card_swiped_view_ok_timer" )
		view_goback_safe( "ok", false )
	end
)

-- set a timer to just move on if OK is not pressed

kernel.timercreate(
	"free_trial_gift_card_swiped_view_ok_timer",
	5000,
	5000,
	function()
		view_analytics_event( "context", "Timeout" )
		kernel.timerremove( "free_trial_gift_card_swiped_view_ok_timer" )
		view_goback_safe( "timer", false )
	end )
kernel.timerstart( "free_trial_gift_card_swiped_view_ok_timer" )