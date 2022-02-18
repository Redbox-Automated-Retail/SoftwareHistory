--
-- show-select-planogram.lua
-- Copyright (C) 2021 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
check_box_state = {}
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
view_show("show_planogram_view")