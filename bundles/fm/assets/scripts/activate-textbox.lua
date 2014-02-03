--
-- activate-textbox.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

local function change_border_background(actor_name, color)
	local underscore_position = string.find(actor_name, "_", 1, true)
	local actor_border = string.sub(actor_name, 1, underscore_position)
	local actor_border_name = actor_border .. "border"
	kernel.setactorbackgroundcolor(actor_border_name, color)
end

change_border_background(active_actor, "black")
active_actor = kernel.getlasthitactorname()
change_border_background(active_actor, "red")
