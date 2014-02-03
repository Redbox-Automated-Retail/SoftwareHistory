--
-- ada_accessible_mode.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


kernel.log( "User clicked the ADA Accessible button" )

-- Start an analytics session if it does not already exist and capture button press on start_view
view_analytics_start( Start_Over_View_Name, "BrowseVend" )
view_analytics_event( "buttonPress", "ada_accessible_mode" )

if kernel.isactorvisible( "tickets_button" ) then
	-- If the tickets button is visible popup a new screen for ABE.
	view_show( "abe_start_view" )
else
	-- This line is the only differnece from the standard browse_products script
	kernel.setfilter( "ada_accessible", "True" )
	kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
	kernel.executescript( "browse_products" )
end