--
-- email-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved

local title = kernel.getmacro("Email:PanelTag")
local value = kernel.getmacro("Email:Text")

if current_email_manager ~= nil and current_email_manager.email_optin == true then
	kernel.setactorimage("image_request_sale_updates", "image_green_selected_checkbox" )
end

if ShoppingCart.TicketsCart.email_optin == true then
	kernel.hideactor("image_request_sale_updates" )
	kernel.hideactor("request_sale_updates_label" )
end

if current_email_manager ~= nil and current_email_manager.display_no_thanks == true then
	kernel.showactor("cancel_button")
	kernel.setactory("request_sale_updates_label",675)
	kernel.setactordimension("request_sale_updates_label",300,50)
end

if value == nil or #value < 3 then
	kernel.disableactor( "ok_button" )
end