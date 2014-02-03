--
-- perform-activity.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


KeyNavigationManager:select_last_hit_actor()
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "ControlPanelActionLog", log_value )

ActivityManager:execute( kernel.getlasthitactortag() )
