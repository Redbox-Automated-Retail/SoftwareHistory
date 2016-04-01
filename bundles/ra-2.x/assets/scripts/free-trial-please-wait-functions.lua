--
-- free-trial-please-wait-message.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function show_free_trial_please_wait_view(onTimeout)
	if free_trial_cancel_idle_timer ~= nil then
		free_trial_cancel_idle_timer:suspend()
	end
	
	view_show("free_trial_please_wait_message_view")
	
	if kernel.timerexists("free_trial_please_wait_message_timer") then
		kernel.timerremove("free_trial_please_wait_message_timer")
	end
	
    local timeout = kernel.ttsgettimeout( "short", 5000 )
	kernel.timercreate("free_trial_please_wait_message_timer", timeout, timeout,
		function()
			if kernel.timerexists("free_trial_please_wait_message_timer") then
				kernel.timerremove("free_trial_please_wait_message_timer")
			end
			view_analytics_event( "context", "Timeout" )
			view_show("free_trial_whoops_popup")
			if onTimeout ~= nil then onTimeout() end			
		end )
		
	kernel.timerstart("free_trial_please_wait_message_timer")	
end

function hide_free_trial_please_wait_view()
	if kernel.timerexists("free_trial_please_wait_message_timer") then
		kernel.timerremove("free_trial_please_wait_message_timer")
	end
	kernel.popview()
	
	if free_trial_cancel_idle_timer ~= nil then
		free_trial_cancel_idle_timer:resume()	
	else
		free_trial_cancel_idle_timer = newidletimer("free_trial_cancel_idle_timer")

		free_trial_cancel_idle_timer:initialize(
			40000, 
			function()
					if kernel.timerexists("free_trial_please_wait_message_timer") then
						kernel.timerremove("free_trial_please_wait_message_timer")
					end
					
					idle_timer_show_prompt_english(function()
					view_analytics_event( "context", "Timeout" )
					view_show("free_trial_cancel_confirmation_view")
				end)
			end
		)

		free_trial_cancel_idle_timer:start()
	end	
end

function remove_free_trial_please_wait_view_on_error()	
	if kernel.timerexists("free_trial_please_wait_message_timer") then
		kernel.timerremove("free_trial_please_wait_message_timer")
	end
end