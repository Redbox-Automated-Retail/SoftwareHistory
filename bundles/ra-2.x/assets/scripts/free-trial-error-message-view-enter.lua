--
-- free-trial-error-message-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_cancel_idle_timer:stop()
kernel.timerremove("free_trial_cancel_idle_timer")
free_trial_cancel_idle_timer = nil
free_trial_error_message_timer = "free_trial_error_message_timer"

function free_trial_error_message_stop_timer()
	if kernel.timerexists(free_trial_error_message_timer) then
		kernel.timerremove(free_trial_error_message_timer)
	end
	if kernel.timerexists("free_trial_please_wait_message_timer") then
		kernel.timerremove("free_trial_please_wait_message_timer")
	end
end

function free_trial_error_message_start_timer()
	if kernel.timerexists(free_trial_error_message_timer) then
		return
	end
	local timeout = kernel.ttsgettimeout( "short", 5000 )
	kernel.timercreate(free_trial_error_message_timer, timeout, timeout,
		function()
			free_trial_error_message_stop_timer()
			view_analytics_event( "context", "Timeout" )
			kernel.executescript( "start_over" ) 
		end )
	kernel.timerstart(free_trial_error_message_timer)
end

kernel.registeractorhitnotify(
	"free_trial_error_message_ok_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_whoops_ok_button" )
		free_trial_error_message_stop_timer()
		kernel.executescript( "start_over" ) 
	end
)

free_trial_error_message_start_timer()

