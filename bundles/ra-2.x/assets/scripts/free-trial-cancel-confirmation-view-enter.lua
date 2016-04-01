--
-- free-trial-confirmation-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_cancel_idle_timer:stop()
kernel.timerremove("free_trial_cancel_idle_timer")
free_trial_cancel_idle_timer = nil

free_trial_confirmation_timer = "free_trial_confirmation_timer"

function free_trial_confirmation_stop_timer()
	if kernel.timerexists(free_trial_confirmation_timer) then
		kernel.timerremove(free_trial_confirmation_timer)
	end
end

function free_trial_confirmation_start_timer()
	if kernel.timerexists(free_trial_confirmation_timer) then
		return
	end

	local timeout = kernel.ttsgettimeout( "short", 5000 )
	kernel.timercreate(free_trial_confirmation_timer, timeout, timeout,
		function()
			free_trial_confirmation_stop_timer()
			view_analytics_event( "context", "Timeout" )
			kernel.executescript( "start_over" ) 
		end )
	kernel.timerstart(free_trial_confirmation_timer)
end

kernel.registeractorhitnotify(
	"freetrial_cancelconfirmation_done_button",
	function()
		free_trial_confirmation_stop_timer()
		view_analytics_event( "buttonPress", "free_trial_cancel_confirmation_done_button" )
		kernel.executescript( "start_over" ) 
	end
)

kernel.log("Cancelling free trial...")

local email = kernel.getmacro("JVHardTake:KeyBoard:Text") 
local zip   = kernel.getmacro("JVHardTake:KeyPad:Text") 
ShoppingCart:create_rbi_free_trial(email, zip, "cancel", function() kernel.log("Free trial has been cancelled.") end)

free_trial_confirmation_start_timer()

-- Since user has elected to cancel, clear all of his data. However, don't clear the data until we've made the cancel kiosk service call
kernel.executescript("free_trial_clear_user_data")
