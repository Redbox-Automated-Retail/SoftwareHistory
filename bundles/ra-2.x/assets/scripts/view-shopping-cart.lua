--
-- view-shopping-cart.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
browse_is_cart_full_open = false

local promo_code = kernel.rental_promotion_get_promotion()
if promo_code == nil and free_in_cart_template_is_running() and free_in_cart_template_is_cart_valid() and ShoppingCart.in_cart_promo_offer_made == false then
    view_show_safe( "in_cart_offer_message_view" )
elseif ShoppingCart.blu_ray_upsell:show() == true then
	view_show_safe( "blu_ray_upsell_view" )
else
	view_show_safe( "shopping_cart_view" )
end