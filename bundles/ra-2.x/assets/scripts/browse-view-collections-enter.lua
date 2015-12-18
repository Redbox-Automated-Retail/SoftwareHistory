--
-- browse-view-collections-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.CollectionsPopupShown", true)

initialize_idle_timer()
idle_timer:start()
if collections_set_number == nil then
	collections_set_number = 1
end

local page_number_changed = function(new_page_number)
	collections_set_number = new_page_number
	-- draw top menu to repaint the button overhang into the browse area
	draw_top_menu()
end

local item_selected = function( collection_product )
	if collection_product ~= nil then
		view_analytics_event( "filterChange", "browse-filter-genre," .. collection_product.product_name )
	end
	show_collection_items( collection_product )
end

local draw_collections = function ()
	local temp_family_name = nil
	local temp_family_type = nil
	local temp_include_list = nil
	local temp_exclude_list = nil
	if ShoppingCart.in_cart_promo_offer_made == true and ShoppingCart.in_cart_promo_offer_cancel == false then
		temp_family_name = browse_filter_family_tracker.selected_family
		temp_family_type = browse_filter_family_tracker:get_family_type()
		temp_include_list, temp_exclude_list = GetInCartTitleLists(true)
	end
	local collection_products = kernel.rental_get_collection_products(temp_family_name, temp_family_type, temp_include_list, temp_exclude_list)

	kernel.draw_images_set(
		"Collections",
		browse_filter_family_tracker.selected_family,
		collection_products, 
		collections_set_number,
		browse_goto_alpha,
		browse_filter_alpha_selected,
		page_number_changed,
		nil, 
		item_selected,
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		nil) 
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_goback_safe("Collections Back Button", false)
		local current_view = kernel.peekview()
		if (current_view == "start_view") then
			view_show("browse_view")
		end
	end
)

draw_collections_view = function ()
	draw_collections()
	kernel.draw_action_bar(
		#ShoppingCart.items,
		on_checkout_button_click,
		should_show_no_thanks_browse_button())
	kernel.draw_mini_cart(
		"browse_view_collections",
		ShoppingCart.items,
		ShoppingCart.in_cart_title_marketing,
		remove_product_on_hit,
		false)
	draw_top_menu()
end

draw_collections_view()