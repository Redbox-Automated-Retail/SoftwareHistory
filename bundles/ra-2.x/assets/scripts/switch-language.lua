--
-- switch-language.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Start an analytics session if it does not already exist and capture button press on start_view
kernel.disableactor("espanol_button")
kernel.log( "language_toggle, setting language_switch=true")
view_analytics_start( kernel.get_start_over_view_name(), "BrowseVend" )
view_analytics_event( "buttonPress", "language_toggle" )
language_switch = true
kernel.log( "Function:switch_language()")
kernel.timerstop("carousel-timer")
kernel.timerstop("carousel-user-timer")
localization_toggle_culture()
refresh_start_view()
next_user_motion = move_actor_right
kernel.timerstart("carousel-user-timer")
if post_switch_language_action ~= nil then
	kernel.log( "Calling post_switch_language_action..." )
	post_switch_language_action()
	post_switch_language_action = nil
end


