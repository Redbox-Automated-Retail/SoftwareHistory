--
-- login-view-leave.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
kernel.removekeypressnotify("login_key_handler")
local progress_manager = newprogressmanager()
progress_manager:stop()
