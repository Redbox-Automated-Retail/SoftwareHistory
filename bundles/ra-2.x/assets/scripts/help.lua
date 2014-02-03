--
-- help.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.disableactor( "help_button" )

-- Start an analytics session if it does not already exist and capture button press on start_view
view_analytics_start( Start_Over_View_Name, "BrowseVend" )

if kernel.getlasthitactorname() ~= "tickets_terms_conditions_button" and kernel.getlasthitactorname() ~= "tickets_terms_conditions_small_button" then
	view_analytics_event( "buttonPress", "help_button" )
end

help_scene_manager = newscenemanager()

help_show_faq()

view_show_safe( "help_view" )

-- Necessary for those times when this is launched from the Start/Welcome View
CloseAnalyticsOnStartView = true
