--
-- activate-cp.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
kernel.closesecurebrowser()
kernel.activatebundle( "Control Panel Application", "*" )
