--
-- return-product.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


kernel.disableactor( "return_button" )

-- Start an analytics session if it does not already exist and capture button press on start_view
kernel.view_analytics_start("Return")
view_analytics_event( "buttonPress", "return_button" )

local return_manager = newreturnmanager( ShoppingCart )
return_manager:init()
return_manager:begin_return( "standard_return" )
