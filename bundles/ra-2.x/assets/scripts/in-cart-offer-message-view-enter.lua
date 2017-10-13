--
-- in-cart-offer-message-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.suspend_idle_timer( "idle_timer" )

LoadInCartPromoMessaging()
kernel.rental_shopping_cart_update_in_cart_offer_made( ShoppingCart.session_id, true)

if free_in_cart_template_is_running() or ( rtwo_in_cart_template_is_running() and rtwo_in_cart_template_get_valid_item_count() > 1 ) then
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
        elseif rtwo_in_cart_template_is_running() and rtwo_in_cart_template_get_valid_item_count() == 1 then
			kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, true )
            kernel.rental_shopping_cart_update_in_cart_title_marketing( ShoppingCart.session_id, false )
            view_show_safe( "shopping_cart_view" )
        elseif rtwo_in_cart_template_is_running() and rtwo_in_cart_template_get_valid_item_count() > 1 then
            kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, false )
            rtwo_in_cart_template_set_promo()
            view_show_safe( "shopping_cart_view" )
        else
            view_goback_safe( "in_cart_offer_message_view:timeout", true )
			kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, true )
            kernel.rental_shopping_cart_update_in_cart_title_marketing( ShoppingCart.session_id, false )
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
        if rtwo_in_cart_template_is_running() and rtwo_in_cart_template_get_valid_item_count() == 1 then
		    kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, true )
            kernel.rental_shopping_cart_update_in_cart_title_marketing( ShoppingCart.session_id, false )
            view_show_safe( "shopping_cart_view" )
        else
            view_goback_safe( "in_cart_offer_message_view:no_button", true )
		    kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, true )
            kernel.rental_shopping_cart_update_in_cart_title_marketing( ShoppingCart.session_id, false )
		    kernel.executescript( "check_out" )
        end
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
        elseif rtwo_in_cart_template_is_running() and rtwo_in_cart_template_get_valid_item_count() > 1 then
            kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, false )
            rtwo_in_cart_template_set_promo()
            view_show_safe( "shopping_cart_view" )
        else
            local promo_data = GetInCartPromoAction()
		    if promo_data.offer_action == "browse_filter" or promo_data.offer_action == "title_marketing" then
                kernel.set_session_value("BrowseState.BrowseTitleFamily", promo_data.product_family)
                kernel.set_session_value("BrowseState.BrowseGenre", nil)
			    kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
                kernel.set_session_value("BrowseState.PageNumber", 1)
                kernel.set_session_value("BrowseState.BrowseTitleType", promo_data.product_type or "ALL")
                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
			    if promo_data.offer_action == "title_marketing" then
                    kernel.rental_shopping_cart_update_in_cart_title_marketing( ShoppingCart.session_id, true )
			    end
		    end
		    view_unwindto_safe( browse_view_name, true, "in-cart-offer-message-view:yes_button" )
        end
	end
)