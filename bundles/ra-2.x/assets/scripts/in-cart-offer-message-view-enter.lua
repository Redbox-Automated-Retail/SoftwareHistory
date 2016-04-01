--
-- in-cart-offer-message-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()
culture_idle_timer:suspend()

LoadInCartPromoMessaging()
ShoppingCart.in_cart_promo_offer_made = true

if free_in_cart_template_is_running() then
    kernel.hideactor( "no_button" )
end

local timeout = kernel.ttsgettimeout( "long", 30000 )
kernel.timercreate(
	"in_cart_promo_popup_timer",
	timeout,
	timeout,
	function()
		kernel.timerstop( "in_cart_promo_popup_timer" )
		kernel.timerremove( "in_cart_promo_popup_timer" )
		if free_in_cart_template_is_running() then
            free_in_cart_template_set_promo()
            view_show_safe( "shopping_cart_view" )
        else
            view_goback_safe( "in_cart_offer_message_view:timeout", true )
			ShoppingCart.in_cart_promo_offer_cancel = true
			ShoppingCart.in_cart_title_marketing = false
			kernel.executescript( "check_out" )
        end
	end
)
kernel.timerstart( "in_cart_promo_popup_timer" )

kernel.registeractorhitnotify(
	"no_button",
	function()
		view_analytics_event2( "buttonPress", "in-cart-promo-decline", ShoppingCart.session_id )
		kernel.timerstop( "in_cart_promo_popup_timer" )
		kernel.timerremove( "in_cart_promo_popup_timer" )
		view_goback_safe( "in_cart_offer_message_view:no_button", true )
		ShoppingCart.in_cart_promo_offer_cancel = true
		ShoppingCart.in_cart_title_marketing = false
		kernel.executescript( "check_out" )
	end
)

kernel.registeractorhitnotify(
	"yes_button",
	function()
		view_analytics_event2( "buttonPress", "in-cart-promo-accept", ShoppingCart.session_id )
		kernel.timerstop( "in_cart_promo_popup_timer" )
		kernel.timerremove( "in_cart_promo_popup_timer" )
        if free_in_cart_template_is_running() then
            free_in_cart_template_set_promo()
            view_show_safe( "shopping_cart_view" )
        else
            local promo_data = GetInCartPromoAction()
		    if promo_data.offer_action == "browse_filter" or promo_data.offer_action == "title_marketing" then
			    browse_filter_family_tracker.selected_family = promo_data.product_family
			    browse_filter_alpha_selected = nil
			    browse_filter_genre_selected = nil
			    kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
			    set_number = 1
			    browse_filter_family_tracker:set_family_type( promo_data.product_type or "ALL" )
                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
			    if promo_data.offer_action == "title_marketing" then
				    ShoppingCart.in_cart_title_marketing = true
			    end
		    end
		    view_unwindto_safe( browse_view_name, true, "in-cart-offer-message-view:yes_button" )
        end
	end
)