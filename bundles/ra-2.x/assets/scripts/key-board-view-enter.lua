--
-- key-board-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--ELECTR
local title = kernel.getmacro("KeyBoard:PanelTag")
local value = kernel.getmacro("KeyBoard:Text") or ""

if title == "EMAIL ADDRESS" or string.find( title, "CORREO ELECTR" ) ~= nil then
	if show_email_confirmation == nil then
		show_email_confirmation = true
	end
	kernel.showactor("yahoo_button")
	kernel.showactor("hotmail_button")
	kernel.showactor("gmail_button")
	kernel.showactor("aol_button")
	kernel.showactor("under_score_button")
	kernel.showactor("at_button")
	kernel.showactor("dot_button")
	kernel.showactor( "dotcom_button" )
	kernel.showactor( "clear_button" )
	kernel.showactor( "clear_button_icon" )
	kernel.setactorwidth( "text_label", 615 )
	kernel.setactorx( "back_button", 695 )
	if kernel.getculture() == "en-US" then
		kernel.setactorx( "back_button_arrow", 715 )	
	else
		kernel.setactorx( "back_button_arrow", 714 )	
	end	
	kernel.showactor( "note_label" )
	kernel.setactorwidth( "space_bar_button", 315 )
	kernel.hideactor("image_next_right_arrow_gray")
elseif title == "PROMO CODE" or string.find( title, "PROMOCI" ) ~= nil then
	kernel.setactorx("two_button",143)
	kernel.setactorx("three_button",219)
	kernel.setactorx("four_button",295)
	kernel.setactorx("five_button",371)
	kernel.setactorx("six_button",447)
	kernel.setactorx("seven_button",523)
	kernel.setactorx("eight_button",599)
	kernel.setactorx("nine_button",675)
	kernel.setactorx("zero_button",751)
	kernel.hideactor("marketing_label")
	if value == nil or #value < 4 then
		kernel.disableactor("ok_button")
		kernel.disableactor("image_next_right_arrow")
	end
end
