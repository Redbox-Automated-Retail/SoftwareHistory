--
-- start-over.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Clean up any free trial timers that may be remaining active
if kernel.timerexists("free_trial_cancel_idle_timer") then
	kernel.timerremove("free_trial_cancel_idle_timer")
end

if kernel.timerexists("free_trial_need_more_time_timer") then
	kernel.timerremove("free_trial_need_more_time_timer")
end

if ShoppingCart ~= nil then
	ShoppingCart:close_analytics( "True" )
	CloseAnalyticsOnStartView = false
	ShoppingCart:abandon( "User chose to abandon session." )
	if ShoppingCart.TicketsCart ~= nil then
		ShoppingCart.TicketsCart:reset_cart()
	end
end
ABE_ON = false -- this line is here to turn off ABE mode when using the AB testing screens.
view_unwindtobottom_safe( "start-over" )
kernel.setfilter( "ada_accessible", "False" )

if kernel.getculture() == "es-MX" then
	culture_idle_timer:start()
end
