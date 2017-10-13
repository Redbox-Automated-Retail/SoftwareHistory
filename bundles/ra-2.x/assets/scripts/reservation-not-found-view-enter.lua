--
-- reservation-not-found-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.stop_idle_timer( "idle_timer" )

-- Custom RNF-Idle-Timer - on timeout goes directly back to the welcome screen
if not kernel.timerexists( "rnf_idle_timer" ) then
	rnf_idle_timer = newidletimer( "rnf_idle_timer" )
	rnf_idle_timer:initialize(
		40000,
		function()
			kernel.executescript( "start_over" )
		end )		
end

-- Start the custom rnf_idle_timer
rnf_idle_timer:start()

kernel.registeractorhitnotify(
	"reservation_not_found_ok_button",
	function()
		kernel.executescript( "start_over" )
	end )

