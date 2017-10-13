--
-- blu-ray-upsell-view-leave.lua
-- Copyright (C) 2014 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

blu_ray_upsell_view_timer:stop()

kernel.resume_idle_timer( "idle_timer" )