--
-- browse-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- This screen uses the actual Start Over script instead
CloseAnalyticsOnStartView = false

browse_collection = nil

local title_rollup_overlay = newtitlerollupoverlay()

-- Add movie Plus signs
function add_movie_on_hit( actor_name )
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	
	local function product_on_hit( product )
		selected_movie = actor_name
        if kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and (kernel.peekview() == "browse_view" or kernel.peekview() == "browse_view_ada_accessible") then
			browse_selection_cart_action = "purchase"
		else
			browse_selection_cart_action = "rental"
		end
		item_added_from_view = kernel.peekview()
		add_item_to_cart( product )
	end
	
	local product_from_actor = get_product_from_actor( actor_name )
	if product_from_actor ~= nil 
        and product_from_actor.product_id ~= 1139 
        and product_from_actor.is_movie == true 
        and (product_from_actor.kiosk_product_type == 3 
            or ( kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and kernel.peekview() == "browse_view_ada_accessible" ) )
        then
		
        view_analytics_event( "buttonPress", "shopping-cart-modal-add-item" )
		
        local is_purchase = kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and (kernel.peekview() == "browse_view" or kernel.peekview() == "browse_view_ada_accessible")
		
		local dvd_product = nil
		local blu_ray_product = nil

		if product_from_actor.dvd_index ~= nil and product_from_actor.dvd_index > 0 then
			dvd_product = product_from_actor.kiosk_products[product_from_actor.dvd_index]
        elseif product_from_actor.product_type_id == 1 then
            dvd_product = product_from_actor
		end
		if product_from_actor.blu_ray_index ~= nil and product_from_actor.blu_ray_index > 0 then
			blu_ray_product = product_from_actor.kiosk_products[product_from_actor.blu_ray_index]
        elseif product_from_actor.product_type_id == 2 then
            blu_ray_product = product_from_actor
		end
		
		local dvd_product_ui_info = get_product_ui_info(dvd_product)
		local blu_ray_product_ui_info = get_product_ui_info(blu_ray_product)

		-- if we are in "for sale" mode and the product is not purchasable then pass nil to the overlay
		local overlay_dvd_product = dvd_product
		if is_purchase and not dvd_product_ui_info.show_in_purchase_overlay then
			overlay_dvd_product = nil
		end

		local overlay_blu_ray_product = blu_ray_product
		if is_purchase and not blu_ray_product_ui_info.show_in_purchase_overlay then
			overlay_blu_ray_product = nil
		end

		title_rollup_overlay:show(
			kernel.getactorx( actor_name ),
			kernel.getactory( actor_name ),
			kernel.getactorwidth( actor_name ),
			kernel.getactorheight( actor_name ),
			false,
			overlay_dvd_product,
			function()
				if is_purchase == true and dvd_product ~= nil and dvd_product.product_id ~= nil then
					view_analytics_event( "buttonPress", "buy," .. tostring( dvd_product.product_id ) )
				else
					if browse_collection ~= nil then
						view_analytics_event( "buttonPress", "shopping-cart-add-item," .. browse_collection.product_name )
					else
						view_analytics_event( "buttonPress", "shopping-cart-add-item" )
					end
				end
				product_on_hit( dvd_product )
			end,
			overlay_blu_ray_product,
			function()
				if is_purchase == true and blu_ray_product ~= nil and blu_ray_product.product_id ~= nil then
					view_analytics_event( "buttonPress", "buy," .. tostring( blu_ray_product.product_id ) )
				else
					if browse_collection ~= nil then
						view_analytics_event( "buttonPress", "shopping-cart-add-item," .. browse_collection.product_name )
					else
						view_analytics_event( "buttonPress", "shopping-cart-add-item" )
					end
				end
				product_on_hit( blu_ray_product )
			end,
			function()
				if is_purchase == true then
					view_analytics_event( "buttonPress", "buy-cancel-add-item" )
				else
					view_analytics_event( "buttonPress", "shopping-cart-cancel-add-item" )
				end
			end,
			nil,
			is_purchase
		)
	elseif product_from_actor ~= nil then
		view_analytics_event( "buttonPress", "shopping-cart-add-item" )
		product_on_hit( product_from_actor )
	end
end

kernel.registeractorhitnotify(
	"abc_button",
	function()
		if title_rollup_overlay ~= nil then
			title_rollup_overlay:hide()
		end
		view_analytics_event( "filterChange", "browse-filter-family,alpha" )
		set_number = 1
		browse_filter_family_tracker:set_family_type( "ALL" )
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "alpha")
		kernel.executescript("browse_filter_alpha")
		display_alpha_screen_layout()
	end
)
kernel.registeractorhitnotify(
	"newest_button",
	function()
		if title_rollup_overlay ~= nil then
			title_rollup_overlay:hide()
		end
		view_analytics_event( "filterChange", "browse-filter-family,Newest" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"for_sale_button",
	function()
		if title_rollup_overlay ~= nil then
			title_rollup_overlay:hide()
		end
		view_analytics_event( "filterChange", "browse-filter-family,for-sale" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")
		browse_filter_family_tracker:set_family_type( "for-sale" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"coming_soon_button",
	function()
		if title_rollup_overlay ~= nil then
			title_rollup_overlay:hide()
		end
		view_analytics_event( "filterChange", "browse-filter-family,Coming-soon" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "coming-soon")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "ComingSoon")
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
		check_coming_soon_collections_popup()
	end
)
kernel.registeractorhitnotify(
	"blu_ray_button",
	function()
		if title_rollup_overlay ~= nil then
			title_rollup_overlay:hide()
		end
		view_analytics_event( "filterChange", "browse-filter-family,Blu-ray" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Blu-ray")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "Blu-ray" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"xbox_360_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox 360" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Xbox 360")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "Xbox 360" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"xbox_one_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox One" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Xbox One")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "Xbox One" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"ps3_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS3" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "PS3")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "PS3" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"ps4_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS4" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "PS4")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "PS4" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"wii_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Wii")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "Wii" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"wii_u_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii U" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Wii U")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_family_tracker:set_family_type( "Wii U" )
		show_browse_view(10)
	end
)

show_browse_view(10)

check_product_availability()
idle_timer:start()

priced_item_popup_timer = newidletimer("priced_item_popup_timer")
priced_item_popup_timer:initialize(
	40,
	function()
		priced_item_popup_timer:stop()
		price_set_item = false
		local message_view_manager = newmessageviewmanager()
		message_view_manager:hide( true )

		if selected_movie ~= nil and StoreManager.PricingManager:is_early_released( selected_movie ) then
			message_view_manager:show()
			view_analytics_event( "context", "$3 Firsts warning message" )
			message_view_manager:hide_title()

			-- The message needs to include the prices from the actual price file (and MD adjustment if needed)
			local product_pricing = StoreManager.PricingManager:get_product_price( selected_movie.product_id )
			local first_night_string = "$3"
			if product_pricing.initial_night ~= nil then
				first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
			end
			local extra_night_string = "$1"
			if product_pricing.extra_night ~= nil then
				extra_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.extra_night )
			end

			local message_text = string.gsub( strings.nonstandered_priced_item, "{EXTRANIGHT}", extra_night_string )
			message_view_manager:set_message_header( string.gsub( strings.nonstandered_priced_item_header, "{0}", first_night_string ) )
			message_view_manager:set_message_text( string.gsub( message_text, "{0}", first_night_string ) )

			message_view_manager:show_first_button(
				strings.back_button,
				function()
					message_view_manager:hide_message()
				end )
			message_view_manager:show_third_button(
				strings.add_to_cart,
				function()
					ShoppingCart:add_item( selected_movie, 1, "rental" )
				end )
			message_view_manager:show_back_button_arrow(
				function()
					message_view_manager:hide_message()
				end )
			idle_timer:start()
		end
	end )

if price_set_item then
	price_set_item = false
	priced_item_popup_timer:start()
end

if show_alert_message then
	show_alert_message = false
	check_for_warning_popups( check_alert_table, selected_movie, i )
end

if #ShoppingCart.items == 5
and browse_is_cart_full_open == false
and shopping_cart_full_message_shown == false then
	show_shopping_cart_full()
end

if (kernel.peekview() == "browse_view") then
	draw_action_bar_and_mini_cart(kernel.peekview())
	draw_top_menu()
elseif (kernel.peekview() == "browse_view_ada_accessible") then
	draw_action_bar()
	draw_ada_control()
end
