--
-- keyboard-update2.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

KeyPadController.update()

if kernel.peekview() == "keypad_popup_alert_view" then
	local text = kernel.getmacro( "keyboard:slot_textbox" )
	if  text ~= nil
	and string.len( text ) >= 1 and tonumber( text ) > 0 then
		kernel.setbordercolor( "send_alert_button", "white" )
		kernel.setactorforegroundcolor( "send_alert_button", "white" )
		kernel.enableactor( "send_alert_button" )
	else
		kernel.setbordercolor( "send_alert_button", {["r"] = 157, ["g"] = 157,["b"] = 157} )
		kernel.setactorforegroundcolor( "send_alert_button", "gray" )
		kernel.disableactor( "send_alert_button" )
	end
end
