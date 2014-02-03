--
-- logout-control-panel.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.logto( "UserLoginLog", "Control Panel | LOGOUT" )
kernel.logto( "ControlPanelActionLog", "Control Panel | LOGOUT" )
ActivityManager:close_control_panel()
