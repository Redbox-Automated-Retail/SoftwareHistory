--
-- vsm-view-leave.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart.vend_manager.vsm_shown_counter == 0 then
	kernel.log( "showing vsm for 3 seconds" )
	kernel.timerwait( 3000 )
	ShoppingCart.vend_manager.vsm_shown_counter = ShoppingCart.vend_manager.vsm_shown_counter + 1
end