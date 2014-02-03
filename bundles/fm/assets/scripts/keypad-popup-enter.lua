--
-- slot-popup-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

KeyPadController.set_display_target( "slot_textbox", 2 )

if kernel.peekview() == "keypad_popup_alert_view" then
	kernel.setbordercolor( "send_alert_button", {["r"] = 157, ["g"] = 157,["b"] = 157} )
	kernel.setactorforegroundcolor( "send_alert_button", "gray" )
	kernel.disableactor( "send_alert_button" )
end