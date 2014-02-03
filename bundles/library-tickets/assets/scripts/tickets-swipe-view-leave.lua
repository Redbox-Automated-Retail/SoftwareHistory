--
-- tickets-swipe-view-leave.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.popmovie()
kernel.trackdatastop()
kernel.timerremove( "tickets_swipe_timer" )
idle_timer:resume()