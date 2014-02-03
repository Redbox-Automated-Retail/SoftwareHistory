--
-- start-view-leave-AB.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved

set_number = 1
browse_selected_sort = "Newest"

if culture_idle_timer ~= nil then culture_idle_timer:stop() end
kernel.timerstop("carousel-timer")
kernel.timerstop("carousel-wait")
kernel.timerstop("burn-in-view-duration-timer")
if burn_in_timer ~= nil then burn_in_timer:stop() end
showing_burn_in_view = false
user_on_flip_view = false