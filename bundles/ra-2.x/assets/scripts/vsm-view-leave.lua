--
-- vsm-view-leave.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local wait_time = 3000

local ad = kernel.marketing_ad_get_screen_ad( "VSM", false )
if ad ~= nil and ad.visible == true and ad.minimum_display ~= nil then
    wait_time = ad.minimum_display
end

if ShoppingCart.vend_manager.vsm_shown_counter == 0 then
	kernel.log( "showing vsm for " .. tostring( wait_time ) .. " milliseconds" )
	kernel.timerwait( wait_time )
	ShoppingCart.vend_manager.vsm_shown_counter = ShoppingCart.vend_manager.vsm_shown_counter + 1
end