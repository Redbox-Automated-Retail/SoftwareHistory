--
-- vend-view-leave.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.popmovie()
show_email_confirmation = false
culture_idle_timer:resume()

clear_coming_soon_vend_screen_if_up()

--Executing all the callbacks in the Queue after vend process
kernel.callbackflush()