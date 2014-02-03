--
-- free-trial-create-cancel-idle-timer.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.timerexists("free_trial_cancel_idle_timer") then
	kernel.timerremove("free_trial_cancel_idle_timer")
end

-- This timer is global like idle_timer
free_trial_cancel_idle_timer = newidletimer("free_trial_cancel_idle_timer")

free_trial_cancel_idle_timer:initialize(
	40000, 
	function()
		idle_timer_show_prompt_english(function()
			view_analytics_event( "context", "Timeout" )
			view_show("free_trial_cancel_confirmation_view")
		end)
	end
)

free_trial_cancel_idle_timer:start()