--
-- free-trial-confirmation-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_confirmation_timer = "free_trial_confirmation_timer"

function free_trial_confirmation_stop_timer()
	if kernel.timerexists(free_trial_confirmation_timer) then
		kernel.timerremove(free_trial_confirmation_timer)
	end
end

local function all_done()
	free_trial_confirmation_stop_timer()
	kernel.timerremove("free_trial_cancel_idle_timer")
	kernel.executescript( "free_trial_clear_user_data" ) 
	kernel.executescript( "start_over" ) 
end

function free_trial_confirmation_start_timer()
	if kernel.timerexists(free_trial_confirmation_timer) then
		return
	end
	local timeout = kernel.ttsgettimeout( "short", 5000 )
	kernel.timercreate(free_trial_confirmation_timer, timeout, timeout,
		function()
			view_analytics_event( "context", "Timeout" )
			all_done()
		end )
	kernel.timerstart(free_trial_confirmation_timer)
end

kernel.registeractorhitnotify(
	"ok_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_confirmation_ok_button" )
		all_done()
	end
)

free_trial_confirmation_start_timer()

