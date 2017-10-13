--
-- key-pad-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
show_email_confirmation = nil

local ad = kernel.marketing_ad_get_screen_ad( "ZipCode", true )
if ad ~= nil and ad.visible == true then
    kernel.showactor( "ad" )
    kernel.setactorimage( "ad", ad.image_name )
    kernel.setactory( "bg_top_border", 118 )
    kernel.setactory( "zipcode_label", 75 )
    kernel.setactory( "text_label", 135 )
    if kernel.getculture() == "es-MX" then
        kernel.hideactor( "zipcode_label" )
        kernel.showactor( "zipcode_label_small" )
    end
end

local text = kernel.getmacro( "KeyPad:Text" )
if kernel.ttsenabled() == true then
	kernel.setmacro( "TTSKeyPad:Text", text )
end
if  text ~= nil and string.len( text ) >= key_pad_minimum_length then
	kernel.enableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "228,32,69" )
	kernel.showactor("next_button_right_arrow_icon")
else
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "white" )
	kernel.hideactor("next_button_right_arrow_icon")
end