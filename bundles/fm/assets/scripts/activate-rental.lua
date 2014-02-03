--
-- activate-rental.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


kernel.healthsvcs_sendauditevent(
	StoreID,
	"FMA_Close",
	username,
	"FMA Bundle",
	"Field Maintenance Application bundle closed." )
	
if kernel.getlasthitactorname() == "cancel_button" then
	kernel.logto( "UserLoginLog", "Field Maintenance | Login cancel button pressed" )
else
	kernel.logto( "UserLoginLog", "Field Maintenance | LOGOUT user ".. tostring(username)  )
end

if #newhardwaremanager():get_hardware_jobs_by_label_contains_text( "maintenance sync by ", { "Stopped", "Errored", "Garbage", "Suspended", "Completed" } ) > 0 then
	view_show( "maintenance_sync_view" )
else
	kernel.closesecurebrowser()
	kernel.activatedefaultbundle()
end
