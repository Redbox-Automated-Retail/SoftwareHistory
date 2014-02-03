--
-- free-trial-not-eligible-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_not_eligible_timer = "free_trial_not_eligible_timer"

function free_trial_not_eligible_stop_timer()
	if kernel.timerexists(free_trial_not_eligible_timer) then
		kernel.timerremove(free_trial_not_eligible_timer)
	end
end

function free_trial_not_eligible_start_timer()
	if kernel.timerexists(free_trial_not_eligible_timer) then
		return
	end
	
	kernel.timercreate(free_trial_not_eligible_timer, 5000, 5000,
		function()
			free_trial_not_eligible_stop_timer()
			view_analytics_event( "context", "Timeout" )
			view_show("free_trial_cancel_confirmation_view")
		end )
	kernel.timerstart(free_trial_not_eligible_timer)
end

kernel.registeractorhitnotify(
	"freetrial_not_eligible_continue_button",
	function()
		free_trial_not_eligible_stop_timer()
		view_analytics_event( "buttonPress", "free_trial_not_eligible_continue_button" )
		view_show("free_trial_cancel_confirmation_view")
	end
)

free_trial_not_eligible_start_timer()

