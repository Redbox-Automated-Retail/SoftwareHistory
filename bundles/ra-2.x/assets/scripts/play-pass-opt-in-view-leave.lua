--
-- play-pass-opt-in-view-leave.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.timerstop( "play_pass_opt_in_view_timer" )
kernel.timerremove( "play_pass_opt_in_view_timer" )
idle_timer:resume()