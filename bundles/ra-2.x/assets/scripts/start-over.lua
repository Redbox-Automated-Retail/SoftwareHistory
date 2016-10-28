--
-- start-over.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "buttonPress", "startOver" )

if ShoppingCart ~= nil then
	ShoppingCart:close_analytics( "True" )
	CloseAnalyticsOnStartView = false
	ShoppingCart:abandon( "User chose to abandon session." )
end
ABE_ON = false -- this line is here to turn off ABE mode when using the AB testing screens.
kernel.setfilter( "ada_accessible", "False" )
view_unwindtobottom_safe( "start-over" )

if kernel.getculture() == "es-MX" then
	culture_idle_timer:start()
end
