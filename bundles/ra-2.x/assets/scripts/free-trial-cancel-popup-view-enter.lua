--
-- free-trial-cancel-popup-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_cancel_idle_timer:suspend()

free_trial_cancel_popup_timer = "free_trial_cancel_popup_timer"

function free_trial_cancel_popup_stop_timer()
	if kernel.timerexists(free_trial_cancel_popup_timer) then
		kernel.timerremove(free_trial_cancel_popup_timer)
	end
end

function free_trial_cancel_popup_start_timer()
	if kernel.timerexists(free_trial_cancel_popup_timer) then
		return
	end
	local timeout = kernel.ttsgettimeout( "medium", 15000 )
	kernel.timercreate(free_trial_cancel_popup_timer, timeout, timeout,
		function()
			free_trial_cancel_popup_stop_timer()
			view_analytics_event( "context", "Timeout" )
			view_show("free_trial_cancel_confirmation_view")
		end )
	kernel.timerstart(free_trial_cancel_popup_timer)
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		free_trial_cancel_popup_stop_timer()
		free_trial_cancel_idle_timer:resume()
		view_analytics_event( "buttonPress", "free_trial_cancel_back_button" )
		view_goback_safe("free_trial_cancel_popup", false)
	end 
)

kernel.registeractorhitnotify(
	"back_button_arrow",
	function()
		free_trial_cancel_popup_stop_timer()
		free_trial_cancel_idle_timer:resume()
		view_analytics_event( "buttonPress", "free_trial_cancel_back_button" )
		view_goback_safe("free_trial_cancel_popup", false)
	end 
)
	
kernel.registeractorhitnotify(
	"continue_button",
	function()
		free_trial_cancel_popup_stop_timer()
		view_analytics_event( "buttonPress", "free_trial_cancel_yes_button" )
		view_show("free_trial_cancel_confirmation_view")
	end
)

free_trial_cancel_popup_start_timer()
