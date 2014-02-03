--
-- add-promo-code.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

ShoppingCart:get_promo_code(
	function()
		view_unwindto_safe( "shopping_cart_view", false, "ShoppingCart:get_promo_code (on_complete)" )
	end,
	function()
		view_unwindto_safe( "shopping_cart_view", false, "ShoppingCart:get_promo_code (on_error)" )
	end)
	
	
	
