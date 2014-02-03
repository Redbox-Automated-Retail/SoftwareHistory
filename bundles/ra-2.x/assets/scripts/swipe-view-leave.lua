--
-- swipe-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
kernel.popmovie()
kernel.trackdatastop()
kernel.timerremove( "swipe_timer" )
culture_idle_timer:resume()
