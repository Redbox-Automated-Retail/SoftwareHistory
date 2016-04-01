--
-- idle-timeout-message-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

local timeout = kernel.ttsgettimeout( "medium", 15000 )
kernel.configure_idle_timeout_message_view(
    timeout,
    idle_timer_expire_session,
    function() view_goback_safe( "need_more_time:second_button", false ) end
)