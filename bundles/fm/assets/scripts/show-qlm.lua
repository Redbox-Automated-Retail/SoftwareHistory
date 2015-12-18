--
-- show-qlm.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
check_box_state = {}
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

view_show( "card_reader_popup_view" )

--if VMZEnabledMachine == true then
--	view_show( "vmz_view" )
--else
--	view_show( "qlm_view" )
--end
