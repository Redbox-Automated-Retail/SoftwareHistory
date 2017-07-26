--
-- browse-view-collection-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.CollectionsPopupShown", true)
initialize_idle_timer()
idle_timer:start()
kernel.registeractorhitnotify(
	"more_collections_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-genre,MORE_COLLECTIONS" )
		show_browse_view_collections()
	end
)

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_goback_safe("Collection Back Button", false)
		local current_view = kernel.peekview()
		if (current_view == "start_view") then
			view_show("browse_view")
		end
	end
)


if collection_set_number == nil then
	collection_set_number = 1
end

local page_number_changed = function(new_page_number, page_change_source)
	collection_set_number = new_page_number
	-- draw top menu to repaint the button overhang into the browse area
	draw_top_menu()
end

local add_item_selected = function( actor_name )
	if browse_collection ~= nil then
		view_analytics_event( "buttonPress", "shopping-cart-modal-add-item," .. browse_collection.product_name )
	end
	add_movie_on_hit( actor_name )
end

local draw_collection = function ()
	kernel.draw_images_set(
		"Collection",
        kernel.get_session_value("BrowseState.BrowseTitleFamily"),
		browse_collection_products, 
		collection_set_number,
		page_number_changed,
		update_products_shown, 
		show_product_detail_view,
		add_item_selected,
		nil,
		is_product_selected,
		is_product_in_stock,
		is_shopping_cart_full,
		get_test_price,
		get_early_release_date2,
		is_title_marketing_mode,
		is_coming_soon_optin_enabled,
		is_coming_soon_item_registered,
		register_email_reminder,
		unregister_email_reminder)
end
	
draw_collection_view = function ()
	draw_collection()	
	kernel.draw_action_bar(
		#ShoppingCart.items,
		on_checkout_button_click,
		should_show_no_thanks_browse_button())
	kernel.draw_mini_cart(
		"browse_view_collection",
		ShoppingCart.items,
		ShoppingCart.in_cart_title_marketing,
		remove_product_on_hit,
		true)

	if browse_collection ~= nil then
		kernel.setactortext("collection_legal_text", browse_collection.legal_info)
		kernel.setactortext("collection_title", browse_collection.product_name)
	end

	draw_top_menu()
end

draw_collection_view()
