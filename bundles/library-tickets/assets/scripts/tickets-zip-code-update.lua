--
-- tickets-zip-code-update.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

if kernel.getlasthitactorname() == "next_button" or kernel.getlasthitactorname() == "image_next_white_arrow_big" then
	zip_code_value = kernel.getmacro( "ZipCode:Text" )

	if tickets_zip_code_next_authorize_func ~= nil then
		ShoppingCart.TicketsCart.zip_code = zip_code_value
		tickets_zip_code_next_authorize_func()
	else
		view_goback_safe( "zipcode:next_button", false )
	end
	
	return
elseif kernel.getlasthitactorname() == "clearall_button" then
	kernel.setmacro( "ZipCode:Text", "" )
	kernel.markactordirty( "text_label" )
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )

	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
	
	return
elseif kernel.getlasthitactorname() == "back_button" then
	zip_code_value = kernel.getmacro( "ZipCode:Text" )
	zip_code_value = string.sub(zip_code_value, 0, string.len(zip_code_value) - 1)
	kernel.setmacro( "ZipCode:Text", zip_code_value)
	kernel.markactordirty( "text_label" )
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.hideactor("image_next_white_arrow_big")
	kernel.showactor("next_button_right_arrow_icon")
	
	return
end

local text = kernel.getmacro( "ZipCode:Text" )
if string.len( text ) >= zip_code_minimum_length then
	return
end

text = text .. kernel.getlasthitactortag()
kernel.setmacro( "ZipCode:Text", text )
kernel.markactordirty( "text_label" )

if string.len( text ) == zip_code_minimum_length then
	kernel.enableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "white" )
	kernel.hideactor("next_button_right_arrow_icon")
	kernel.showactor("image_next_white_arrow_big")
end