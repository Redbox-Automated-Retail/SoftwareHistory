--
-- key-pad-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
show_email_confirmation = nil
local text = kernel.getmacro( "KeyPad:Text" )

if kernel.ttsenabled() == true then
	kernel.setmacro( "TTSKeyPad:Text", text )
end

if  text ~= nil
and string.len( text ) >= key_pad_minimum_length then
	kernel.enableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "white" )
	kernel.hideactor("next_button_right_arrow_icon")
	kernel.showactor("image_next_white_arrow_big")
else
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
end
