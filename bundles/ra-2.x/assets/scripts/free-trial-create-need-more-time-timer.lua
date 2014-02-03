--
-- free-trial-create-need-more-time-timer.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Replaces idle_timer with an English-only version

if kernel.timerexists("free_trial_need_more_time_timer") then
	kernel.timerremove("free_trial_need_more_time_timer")
end

-- This timer is global like idle_timer
free_trial_need_more_time_timer = newidletimer("free_trial_need_more_time_timer")

local function free_trial_need_more_timer_expire()
	if kernel.timerexists("free_trial_need_more_time_timer") then
		kernel.timerremove("free_trial_need_more_time_timer")
	end
	idle_timer_expire_session()
	view_goback_safe( "need_more_time:second_button", false )
end

free_trial_need_more_time_timer:initialize(
	40000, 
	function()
		local message_view_manager = newmessageviewmanager()
		message_view_manager:show()
		view_analytics_event( "context", "idleTimer" )

		message_view_manager:hide_message()
		
		message_view_manager:set_idle_timer_title("Need more time?")
		
		message_view_manager:show_first_button(
			"NO THANKS", 
			function()
				if kernel.timerexists("free_trial_need_more_time_timer") then
					kernel.timerremove("free_trial_need_more_time_timer")
				end
				idle_timer_expire_session()
			end
		)
		
		message_view_manager:show_second_button(
			"YES",
			function() view_goback_safe( "need_more_time:second_button", false ) end
		)
		
		message_view_manager:start_timer( free_trial_need_more_timer_expire )
	end
)

free_trial_need_more_time_timer:start()