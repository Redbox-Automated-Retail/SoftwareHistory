--
-- idle-timeout-message-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.suspend_idle_timer( "idle_timer" )
kernel.configure_idle_timeout_message_view(idle_timer_expire_session)