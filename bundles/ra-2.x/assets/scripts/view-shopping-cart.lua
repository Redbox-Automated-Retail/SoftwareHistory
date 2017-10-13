--
-- view-shopping-cart.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.IsShoppingCartFullMessagesOpen", false)

if free_in_cart_template_show_message() or rtwo_in_cart_template_show_message() then
    view_show_safe( "in_cart_offer_message_view" )
elseif kernel.current_rental_shopping_cart_bluray_upsell_dialog_needs_to_be_shown() == true then
	view_show_safe( "blu_ray_upsell_view" )
else
	view_show_safe( "shopping_cart_view" )
end