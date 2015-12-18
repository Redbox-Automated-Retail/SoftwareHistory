--
-- key-pad-update.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

if kernel.getlasthitactorname() == "next_button" or kernel.getlasthitactorname() == "image_next_white_arrow_big" then
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
	kernel.setactorforegroundcolor( "next_button", "gray" )

	-- for validating right arrow on next button 
--	kernel.setactorbackgroundcolor( "next_button", "gray" )
	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
	
	if TextToSpeechEnabled == true then
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
	kernel.setactorforegroundcolor( "next_button", "gray" )
--	kernel.setactorbackgroundcolor( "next_button", "30,30,30" )
	kernel.hideactor("image_next_white_arrow_big")
	kernel.showactor("next_button_right_arrow_icon")

	if TextToSpeechEnabled == true then
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
	kernel.setactorforegroundcolor( "next_button", "white" )
--	kernel.setactorbackgroundcolor( "next_button", "gray" )
	kernel.hideactor("next_button_right_arrow_icon")
	kernel.showactor("image_next_white_arrow_big")
	
--[[else
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.setactorbackgroundcolor( "next_button", "gray" )
	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
]]
end

if TextToSpeechEnabled == true then
	local splitzip = ""
	for digit in text:gmatch("%w") do splitzip = splitzip .. digit .. " " end
	kernel.setmacro( "TTSKeyPad:Text", splitzip )	
	kernel.ttsrepeat();  -- assests have been set and toggled
end

