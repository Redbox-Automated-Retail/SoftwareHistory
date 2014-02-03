--
-- show-hal-tester.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local msgbox = newmessageboxmanager()
local ms_hal_tester_path = "C:\\Program Files\\Redbox\\MS HAL Tester\\bin"
local directory_exists = kernel.doesdirectoryexist(ms_hal_tester_path)

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

if(directory_exists == false) then
	msgbox:show( "Could not find MSHALTester directory.", msgbox.ButtonTypeOK, msgbox.IconTypeError )
	return
end

local pid = kernel.startprocess( ms_hal_tester_path .. "\\MSHALTester.exe", nil, false )
if pid < 0 then
	msgbox:show( "Unable to start MSHALTester.exe.", msgbox.ButtonTypeOK, msgbox.IconTypeError )
end
		
	
