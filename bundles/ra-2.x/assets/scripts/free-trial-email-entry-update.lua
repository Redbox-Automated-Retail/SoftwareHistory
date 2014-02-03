--
-- free-trial-email-entry-update.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local actorName = kernel.getlasthitactorname()
kernel.setmacro( "JVHardTake:Email:EditPath", "" )

if actorName == nil then
	return
end

function free_trial_email_enable_or_disable_ok()
	local text = kernel.getmacro("JVHardTake:KeyBoard:Text") or ""
	
	if string.len( text ) >= JVHardTake_key_board_minimum_length and string.len( text ) <= JVHardTake_key_board_max_length then
		kernel.enableactor( "ok_button" )		
		kernel.setactorforegroundcolor( "ok_button", "white" )
		kernel.hideactor("image_next_right_arrow_gray")
		kernel.disableactor("image_next_right_arrow_gray")
		kernel.showactor("image_next_right_arrow")
		kernel.enableactor( "image_next_right_arrow" )
	else
		kernel.disableactor( "ok_button" )		
		kernel.setactorforegroundcolor( "ok_button", "gray" )
		kernel.showactor("image_next_right_arrow_gray")
		kernel.hideactor("image_next_right_arrow")
		kernel.disableactor( "image_next_right_arrow" )
	end
end

if actorName== "ok_button" or actorName == "image_next_right_arrow" then
	return
elseif actorName == "clear_button" or actorName == "clear_button_icon" then
	kernel.setmacro( "JVHardTake:KeyBoard:Text", "" )
	kernel.markactordirty( "text_label" )
	kernel.setactortext( "text_label", "" )
	free_trial_email_enable_or_disable_ok()
	return
elseif actorName == "back_button" or actorName == "back_button_arrow" then
	key_pad_value = kernel.getmacro( "JVHardTake:KeyBoard:Text" )
	key_pad_value = string.sub(key_pad_value, 0, string.len(key_pad_value) - 1)
	kernel.setmacro( "JVHardTake:KeyBoard:Text", key_pad_value)
	kernel.markactordirty( "text_label" )
	kernel.setactortext( "text_label", key_pad_value )
	free_trial_email_enable_or_disable_ok()
	return
end

local text = kernel.getmacro( "JVHardTake:KeyBoard:Text" ) or ""
text = text .. kernel.getlasthitactortag() 

if string.len( text ) > JVHardTake_key_board_max_length then
	return
end

kernel.markactordirty( "error_label" )
kernel.setactortext( "error_label", "" )
kernel.setmacro( "JVHardTake:KeyBoard:Text", text )
kernel.markactordirty( "text_label" )
kernel.setactortext( "text_label", text )

free_trial_email_enable_or_disable_ok()
