--
-- free-trial-soft-take-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_soft_take_timer = "free_trial_soft_take_timer"

function free_trial_soft_take_stop_timer()
	if kernel.timerexists(free_trial_soft_take_timer) then
		kernel.timerremove(free_trial_soft_take_timer)
	end
end

function free_trial_soft_take_start_timer()
	if kernel.timerexists(free_trial_soft_take_timer) then
		return
	end
	
    local timeout = kernel.ttsgettimeout( "short", 5000 )
	kernel.timercreate(free_trial_soft_take_timer, timeout, timeout,
		function()
			free_trial_soft_take_stop_timer()
			view_analytics_event( "context", "Timeout" )
			--kernel.executescript( "start_over" ) 
			view_show("free_trial_cancel_confirmation_view")
		end )
	kernel.timerstart(free_trial_soft_take_timer)
end

kernel.registeractorhitnotify(
	"freetrial_soft_take_done_button",
	function()
		free_trial_soft_take_stop_timer()
		view_analytics_event( "buttonPress", "freetrial_softtake_done_button" )
		--kernel.executescript( "start_over" ) 
		view_show("free_trial_cancel_confirmation_view")
	end
)

free_trial_soft_take_start_timer()