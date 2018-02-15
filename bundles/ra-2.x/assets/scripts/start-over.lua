--
-- start-over.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.get_application_state_value("KeepSessionOnStartViewEnter") == false then
    view_analytics_event( "buttonPress", "startOver" )

    if ShoppingCart ~= nil then
	    ShoppingCart:close_analytics( "True" )
	    CloseAnalyticsOnStartView = false
	    ShoppingCart:abandon( "User chose to abandon session." )
    end

    kernel.set_application_state_value("IsADAAccessible", false)
end

view_unwindtobottom_safe( "start-over" )

