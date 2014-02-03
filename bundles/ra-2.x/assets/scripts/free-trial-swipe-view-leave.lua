--
-- free-trial-swipe-view-leave.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.popmovie()
kernel.trackdatastop()
kernel.timerremove( "free_trial_swipe_timer" )
idle_timer:resume()

--culture_idle_timer:resume()
