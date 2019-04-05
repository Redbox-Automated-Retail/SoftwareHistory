--
-- authenticate.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local progress_manager = newprogressmanager()
kernel.hideactor("message_label")
kernel.markactordirty("message_label")

username = kernel.getmacro("keyboard:login_textbox") or ""
local password = <redacted>"keyboard:password_textbox") or ""

if string.len(username) == 0 or string.len(password) == 0 then
	kernel.showactor( "message_label" )
	kernel.markactordirty( "message_label" )
	return
end

idle_timer:stop()
progress_manager:start()

kernel.setmacro( "FMA:StoreNumber", StoreID )
kernel.setmacro( "FMA:UserName", username )

kernel.authsvcs_authenticateuser(
	StoreID,
	username,
	lv_password,
	<redacted>,
	function( r )
		progress_manager:stop()
		if r.success then
			kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_Login",
				username,
				"FMA Bundle",
				"Successful login into the Field Maintenance Application bundle." )
			kernel.logto( "UserLoginLog", "Field Maintenance | LOGIN SUCCESS for user : ".. username  .. ", online = " .. tostring(r.properties.is_online) )
			kernel.logto( "FieldMaintenanceActionLog", "Field Maintenance | LOGIN SUCCESS for user : ".. username  .. ", online = " .. tostring(r.properties.is_online) )
			view_show( "main_menu_view" )
		else
			--kernel.log( "FMA failed for user: ".. username .. " with reason code: " .. r.properties.response )
			kernel.logto( "UserLoginLog", "Field Maintenance | LOGIN FAILED, reason: " .. r.properties.response )
			kernel.logto( "FieldMaintenanceActionLog", "Field Maintenance | LOGIN FAILED, reason: " .. r.properties.response )
			if r.properties.response == "AccountLocked" then
				kernel.setactortext("message_label", "Too many failed login attempts. User ID locked.")
			else
				kernel.setactortext("message_label", "Invalid UserName/Password")
			end
			kernel.showactor( "message_label" )
			kernel.markactordirty( "message_label" )
		end
		
		idle_timer:start()
	end )
