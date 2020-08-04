--
-- return-to-main.lua
-- Copyright (C) 2019 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

view_unwindto( "main_menu_view", true, "return_to_main" )