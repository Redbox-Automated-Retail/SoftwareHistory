--
-- launch-vmz-merch-screen.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

view_show( "vmz_merch_flow_view" )
--view_show( "vmz_merch_rebalances_view" )