--
-- credit-balance-view-leave.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.timerstop( "credit_balance_view_timer" )
kernel.timerremove( "credit_balance_view_timer" )
kernel.resume_idle_timer( "idle_timer" )
