--
-- tickets-zip-code-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.setmacro( "ZipCode:Text", "" )
zip_code_minimum_length = 5
zip_code_value = nil

local text = kernel.getmacro( "ZipCode:Text" )
if  text ~= nil
and string.len( text ) >= zip_code_minimum_length then
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