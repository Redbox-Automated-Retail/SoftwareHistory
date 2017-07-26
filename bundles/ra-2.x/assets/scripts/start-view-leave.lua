--
-- start-view-leave.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved

-- this can be removed when the browse view is converted to WPF
if overlay_scene_manager ~= nil then
	overlay_scene_manager:hide()
end

kernel.leave_start_view()