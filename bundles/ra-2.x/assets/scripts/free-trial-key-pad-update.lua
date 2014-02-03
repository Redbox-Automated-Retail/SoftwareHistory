--
-- free-trial-key-pad-update.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

if kernel.getlasthitactorname() == "next_button" or kernel.getlasthitactorname() == "image_next_white_arrow_big" then
	return
elseif kernel.getlasthitactorname() == "clearall_button" then
	kernel.setmacro( "JVHardTake:KeyPad:Text", "" )
	kernel.markactordirty( "text_label" )
	kernel.setactortext( "text_label", "" )
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
	
	return
elseif kernel.getlasthitactorname() == "back_button" then
	key_pad_value = kernel.getmacro( "JVHardTake:KeyPad:Text" )
	key_pad_value = string.sub(key_pad_value, 0, string.len(key_pad_value) - 1)
	kernel.setmacro( "JVHardTake:KeyPad:Text", key_pad_value)
	kernel.markactordirty( "text_label" )
	kernel.setactortext( "text_label", key_pad_value )
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.hideactor("image_next_white_arrow_big")
	kernel.showactor("next_button_right_arrow_icon")
	
	return
end

local text = kernel.getmacro( "JVHardTake:KeyPad:Text" )
if string.len( text ) >= JVHardTake_key_pad_minimum_length then
	return
end

text = text .. kernel.getlasthitactortag()
kernel.setmacro( "JVHardTake:KeyPad:Text", text )
kernel.markactordirty( "text_label" )
kernel.setactortext( "text_label", text )

if string.len( text ) == JVHardTake_key_pad_minimum_length then
	kernel.enableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "white" )
	kernel.hideactor("next_button_right_arrow_icon")
	kernel.showactor("image_next_white_arrow_big")
end



