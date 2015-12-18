--
-- start-view-leave.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved

set_number = 1

if bluray_banner_pressed ~= true then
    kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
end
bluray_banner_pressed = nil

if culture_idle_timer ~= nil then culture_idle_timer:stop() end
kernel.timerstop("carousel-user-timer")
kernel.timerstop("carousel-timer")
kernel.timerstop("carousel-wait")
kernel.timerstop("burn-in-view-duration-timer")
abe_idle_timer:stop()
kernel.timerstop("start-idle-timer")
if burn_in_timer ~= nil then burn_in_timer:stop() end
showing_burn_in_view = false
user_on_flip_view = false
user_on_overlay = false
if overlay_scene_manager ~= nil then
	overlay_scene_manager:hide()
end