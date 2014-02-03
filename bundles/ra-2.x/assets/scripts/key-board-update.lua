--
-- key-board-update.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if current_keyboard_manager == nil then
	return
end

current_keyboard_manager:handle_actor_hit()
