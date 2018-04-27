--
-- machine-full-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.stop_idle_timer( "idle_timer" )

machine_full_idle_timer = newidletimer("machine_full_idle_timer")
machine_full_idle_timer:initialize(
	15000,
	function()
		view_analytics_event( "timeout", "machine_full_idle_timer" )
		kernel.executescript( "start_over" )
	end
)
machine_full_idle_timer:start()

kernel.registeractorhitnotify(
	"machine_full_back_button",
	function()
		view_analytics_event( "buttonPress", "machine_full_back_button" )
		kernel.executescript( "start_over" )
	end )

kernel.registeractorhitnotify(
	"machine_full_rent_button",
	function()
		view_analytics_event( "buttonPress", "machine_full_rent_button" )
        kernel.close_open_session_and_start_new_rental_session()
        view_show_safe( "browse_view" )
	end )
