--
-- key-pad-update.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

if kernel.getlasthitactorname() == "next_button" then
	
    kernel.disableactor( "next_button" )
	kernel.hideactor( "next_button_right_arrow_icon" )

    key_pad_value = kernel.getmacro( "KeyPad:Text" )
	
    if credit_button_hit == true then
		ShoppingCart.postal_code = key_pad_value
		key_pad_next_getcustomercredits_func()
	else
		if key_pad_next_authorize_func ~= nil then
            ShoppingCart.postal_code = key_pad_value
			show_credits_popup = true
            key_pad_next_authorize_func()
		else
			view_goback_safe( "keypad:next_button", false )
		end
	end
	
	return
elseif kernel.getlasthitactorname() == "clearall_button" then
	kernel.setmacro( "KeyPad:Text", "" )
	kernel.markactordirty( "text_label" )
	kernel.disableactor( "next_button" )
	kernel.hideactor("next_button_right_arrow_icon")
	
	if kernel.ttsenabled() == true then
		kernel.setmacro( "TTSKeyPad:Text", "" )	
		kernel.ttsrepeat();  -- assests have been set and toggled
	end

	return
elseif kernel.getlasthitactorname() == "back_button" then
	key_pad_value = kernel.getmacro( "KeyPad:Text" )
	key_pad_value = string.sub(key_pad_value, 0, string.len(key_pad_value) - 1)
	kernel.setmacro( "KeyPad:Text", key_pad_value)
	kernel.markactordirty( "text_label" )
	kernel.disableactor( "next_button" )
	kernel.hideactor("next_button_right_arrow_icon")

	if kernel.ttsenabled() == true then
		local splitzip = ""
		for digit in key_pad_value:gmatch("%w+") do splitzip = splitzip .. digit .. " " end
		kernel.setmacro( "TTSKeyPad:Text", splitzip )	
		kernel.ttsrepeat();  -- assests have been set and toggled
	end
	
	return
end

local text = kernel.getmacro( "KeyPad:Text" )
if string.len( text ) >= key_pad_minimum_length then
	return
end

text = text .. kernel.getlasthitactortag()
kernel.setmacro( "KeyPad:Text", text )
kernel.markactordirty( "text_label" )

if string.len( text ) == key_pad_minimum_length then
	kernel.enableactor( "next_button" )
	kernel.showactor("next_button_right_arrow_icon")
end

if kernel.ttsenabled() == true then
	local splitzip = ""
	for digit in text:gmatch("%w") do splitzip = splitzip .. digit .. " " end
	kernel.setmacro( "TTSKeyPad:Text", splitzip )	
	kernel.ttsrepeat();  -- assests have been set and toggled
end

