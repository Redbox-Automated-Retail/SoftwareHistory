--
-- email-view-update.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if current_email_manager == nil then
	return
end

current_email_manager:handle_actor_hit()