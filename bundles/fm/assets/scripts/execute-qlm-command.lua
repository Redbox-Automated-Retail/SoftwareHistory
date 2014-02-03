	--
-- execute-qlm-command.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

if kernel.getlasthitactortag() == nil then
	return
end

qlm_info:execute_command(
	kernel.getlasthitactortag() )
	
	
