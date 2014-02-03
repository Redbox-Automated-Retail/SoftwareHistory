--
-- in-cart-offer-message-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

LoadInCartPromoMessaging()
ShoppingCart.in_cart_promo_offer_made = true

idle_timer:suspend()
culture_idle_timer:suspend()

kernel.timercreate(
	"in_cart_promo_popup_timer",
	30000,
	30000,
	function()
		kernel.timerstop( "in_cart_promo_popup_timer" )
		if kernel.peekview() == "in_cart_offer_message_view" then
			--in cart offer message view is discarded from the stack when time out occurs.
			view_goback_safe( "in_cart_offer_message_view:timeout", true )
			ShoppingCart.in_cart_promo_offer_cancel = true
			ShoppingCart.in_cart_title_marketing = false
			kernel.executescript( "check_out" )
		end
		kernel.timerremove( "in_cart_promo_popup_timer" )
	end )
kernel.timerstart( "in_cart_promo_popup_timer" )

kernel.registeractorhitnotify(
	"no_button",
	function()
		view_analytics_event2( "buttonPress", "in-cart-promo-decline", ShoppingCart.session_id )
		kernel.timerstop( "in_cart_promo_popup_timer" )
		kernel.timerremove( "in_cart_promo_popup_timer" )
		--in cart offer message view is discarded when user rejects the offer.
		view_goback_safe( "in_cart_offer_message_view:no_button", true )
		ShoppingCart.in_cart_promo_offer_cancel = true
		ShoppingCart.in_cart_title_marketing = false
		kernel.executescript( "check_out" )
	end )

kernel.registeractorhitnotify(
	"yes_button",
	function()
		view_analytics_event2( "buttonPress", "in-cart-promo-accept", ShoppingCart.session_id )
		kernel.timerstop( "in_cart_promo_popup_timer" )
		kernel.timerremove( "in_cart_promo_popup_timer" )
		--get the action to be performed if the user accepts the offer.
		local promo_data = GetInCartPromoAction()
		if promo_data.offer_action == "browse_filter" 
		or promo_data.offer_action == "title_marketing" 
		then
			browse_filter_family_tracker.selected_family = promo_data.product_family
			browse_filter_alpha_selected = nil
			browse_filter_genre_selected = "ALL"				
			browse_selected_sort = "Newest"
			set_number = 1
			browse_filter_family_tracker:set_family_type( promo_data.product_type or "ALL" )
			browse_filter_mode = "all"
			if promo_data.offer_action == "title_marketing" then
				ShoppingCart.in_cart_title_marketing = true
			end
		end
		view_unwindto_safe( browse_view_name, true, "in-cart-offer-message-view:yes_button" )
	end	)

