--
-- keyboard-update.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
--KeyPadController.update()

if kernel.getlasthitactorname() == nil or kernel.getlasthitactortag() == nil then
	return
end

local macro_name = "keyboard:" .. active_actor
local text = kernel.getmacro( macro_name )
text = text or ""

local key_data_begin, key_data_end = string.find( kernel.getlasthitactortag(), "keydata=" )

if key_data_begin == nil then
	return
end

local key_data = string.sub( kernel.getlasthitactortag(), key_data_begin+8, string.len( kernel.getlasthitactortag() ))

if string.len(text) + 1 > key_board_minimum_length and (key_data ~= "clear" and key_data ~= "back") then
	return
end

if key_data == "clear" then
	kernel.setmacro( macro_name, "" )
	kernel.markactordirty( active_actor )
	if active_actor == "password_textbox" <redacted>
		lv_password = ""
	end
	return
elseif key_data == "space" then
	if active_actor == "password_textbox" <redacted>
		lv_password = <redacted> .. " "
		text = text .. "*"
	else
		text = text .. " "
	end

	kernel.setmacro( macro_name, text )
	kernel.markactordirty( active_actor )
	return
elseif key_data == "back" then
	if active_actor == "password_textbox" and string.len(lv_password) > 0 then
		lv_password = <redacted> lv_password, 1, string.len( lv_password ) - 1 )
	end

	if string.len( text ) > 0 then
		text = string.sub( text, 1, string.len( text ) - 1 )
		kernel.setmacro( macro_name, text )
		kernel.markactordirty( active_actor )
	end
else
	if upper_case_flag == true then
		key_data = string.upper(key_data)
	end

	if active_actor == "password_textbox" <redacted>
		lv_password = <redacted> .. key_data
		key_data = "*"
	end

	text = text .. key_data
	kernel.setmacro( macro_name, text )
	kernel.markactordirty( active_actor )
end
