--
-- browse-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- This screen uses the actual Start Over script instead
CloseAnalyticsOnStartView = false

if browse_filter_family_tracker.selected_family == "Movies" then
	if (#ShoppingCart.items) == 5 then
		set_actors_opacity( 0.2 )
	else
		set_actors_opacity( 1 )
	end
end

local function TogglePayButton()
	if ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_title_marketing == true
	and CartHasAnyPostOfferItems( ShoppingCart ) == false
	then
		kernel.hideactor( "my_rentals_button" )
		kernel.showactor( "title_marketing_no_button" )
	else
		kernel.showactor( "my_rentals_button" )
		kernel.hideactor( "title_marketing_no_button" )
	end
end

movie_product_map = {}
-- Left and Right Arrows
kernel.registeractorhitnotify(
	"left_arrow_button",
	function()
		view_analytics_event( "pageChange", "browse-page-back" )
		move_left(10)
	end
)
kernel.registeractorhitnotify(
	"right_arrow_button",
	function()
		view_analytics_event( "pageChange", "browse-page-next" )
		move_right(10)
	end
)

-- Add movie Plus signs
for i = 1, 10 do
	kernel.registeractorhitnotify(
		"add_movie_" .. tostring( i ),
		function()
			view_analytics_event( "buttonPress", "shopping-cart-add-item" )
			selected_movie = "movie_" .. tostring( i )
			if browse_filter_mode == "for-sale" then
				browse_selection_cart_action = "purchase"
			else
				browse_selection_cart_action = "rental"
			end
			item_added_from_view = kernel.peekview()
			add_item_tocart( 11 )
			TogglePayButton()
		end
	)
	kernel.registeractorhitnotify(
		"add_movie_lower_" .. tostring( i ),
		function()
			view_analytics_event( "buttonPress", "shopping-cart-add-item-ada" )
			selected_movie = "movie_" .. tostring( i )
			if browse_filter_mode == "for-sale" then
				browse_selection_cart_action = "purchase"
			else
				browse_selection_cart_action = "rental"
			end
			item_added_from_view = kernel.peekview()
			add_item_tocart( 11 )
			TogglePayButton()
		end
	)
end

-- Remove movie from cart X buttons
for i = 1, 5 do
	kernel.registeractorhitnotify(
		"cancel_" .. tostring( i ),
		function()
			view_analytics_event( "buttonPress", "shopping-cart-remove-item" )
			selected_movie = "selectedmovie_" .. tostring( i )
			remove_movie(10)
			TogglePayButton()
		end
	)
end

function switch_product_family_tab_function()
	if browse_filter_family_tracker.selected_family == "Movies" then
		browse_filter_family_tracker.selected_family = "Games"
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_games )
		browse_filter_genre_selected = nil
		set_actors_opacity( 0.2 )
		for i = 1, 10 do
			kernel.hideactor("banner_" .. tostring( i ))
			kernel.hideactor("banner_coming_soon_" .. tostring( i ))
			kernel.hideactor( "button_email_reminder_" .. tostring( i ))
			kernel.hideactor( "button_email_reminder_set_" .. tostring( i ))
		end
		if browse_selected_sort == "Blu-ray" then
			browse_filter_family_tracker:set_family_type( "ALL" )
			browse_selected_sort = "Newest"
			browse_filter_mode = "all"
		elseif browse_filter_mode =="coming-soon" then
			browse_filter_family_tracker:set_family_type( "ALL" )
			browse_selected_sort = "coming-soon"
			browse_filter_mode = "coming-soon"
		elseif browse_filter_mode =="for-sale" then
			if ProductCatalog:is_product_available( "for-sale" )
			and EnableSellThruOfUsed == true
			and GamesSellThru == true then
				browse_filter_family_tracker:set_family_type( "for-sale" )
				browse_selected_sort = "for-sale"
				browse_filter_mode = "for-sale"
			else
				browse_filter_family_tracker:set_family_type( "ALL" )
				browse_selected_sort = "Newest"
				browse_filter_mode = "all"
			end
		end
	else
		browse_filter_family_tracker.selected_family = "Movies"
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
		browse_filter_genre_selected = ALL
		if(#ShoppingCart.items) ~= 5 then
			set_actors_opacity( 1 )
		end
		if browse_selected_sort == "Xbox 360" or browse_selected_sort == "Xbox One" or browse_selected_sort == "PS2" or browse_selected_sort == "PS3" or browse_selected_sort == "PS4" or browse_selected_sort == "Wii" or browse_selected_sort == "NintendoDS" or browse_selected_sort == "Wii U" then
			browse_filter_family_tracker:set_family_type( "ALL" )
			browse_selected_sort = "Newest"
		end
		if browse_filter_mode == "for-sale" then
			if ProductCatalog:is_product_available( "for-sale" ) and EnableSellThruOfUsed == true then
				browse_filter_family_tracker:set_family_type( "for-sale" )
				browse_selected_sort = "for-sale"
				browse_filter_mode = "for-sale"
			else
				browse_filter_family_tracker:set_family_type( "ALL" )
				browse_selected_sort = "Newest"
				browse_filter_mode = "all"
			end
		end
	end
	view_analytics_event( "filterChange", "browse-filter," .. browse_filter_family_tracker.selected_family )
	if browse_selected_sort == "Newest" then
		browse_filter_family_tracker:set_family_type( "ALL" )
	end
	set_number = 1
	show_browse_view(10)
	check_product_availability()
	highlight_tab(1)
end

kernel.registeractorhitnotify(
	"tab_switch_product_family",
	function()
		switch_product_family_tab_function()
	end
)
kernel.registeractorhitnotify(
	"image_next_white_arrow",
	function()
		switch_product_family_tab_function()
	end
)
kernel.registeractorhitnotify(
	"abc_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,alpha" )
		set_number = 1
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_selected_sort = "alpha"
		kernel.executescript("browse_filter_alpha")
		display_alpha_screen_layout()
	end
)
kernel.registeractorhitnotify(
	"newest_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Newest" )
		set_number = 1
		browse_selected_sort = "Newest"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"for_sale_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,for-sale" )
		set_number = 1
		browse_selected_sort = "for-sale"
		browse_filter_mode = "for-sale"
		browse_filter_family_tracker:set_family_type( "for-sale" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"coming_soon_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Coming-soon" )
		set_number = 1
		browse_selected_sort = "coming-soon"
		browse_filter_mode = "coming-soon"
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"blu_ray_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Blu-ray" )
		set_number = 1
		browse_selected_sort = "Blu-ray"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Blu-ray" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"xbox_360_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox 360" )
		set_number = 1
		browse_selected_sort = "Xbox 360"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Xbox 360" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"xbox_one_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox One" )
		set_number = 1
		browse_selected_sort = "Xbox One"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Xbox One" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"ps3_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS3" )
		set_number = 1
		browse_selected_sort = "PS3"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "PS3" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"ps4_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS4" )
		set_number = 1
		browse_selected_sort = "PS4"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "PS4" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"wii_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii" )
		set_number = 1
		browse_selected_sort = "Wii"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Wii" )
		show_browse_view(10)
	end
)
kernel.registeractorhitnotify(
	"wii_u_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii U" )
		set_number = 1
		browse_selected_sort = "Wii U"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Wii U" )
		show_browse_view(10)
	end
)

-- Tabs at the top
function RegisterProductFamilyTabs()
	for i = 1, 7 do
		kernel.registeractorhitnotify(
			"tab_" .. tostring( i ),
			function()
				browse_filter_genre_selected = ProductFamiliesMetaData[ browse_filter_family_tracker.selected_family ].filter_genres[i]
				local tab_text = kernel.getactortext( "tab_" .. tostring( i ) )
				if string.len(tab_text) == 0
					and i==2 then -- single out the Top 20 tab (2nd tab, ie i==2) that is currently split into 2 actors
					-- handles the tabs that has the text split between 2 new actors
					tab_text = kernel.getactortext( "tab_" .. tostring( i ) .. "_text_a")
						.. " "
						.. kernel.getactortext( "tab_" .. tostring( i ) .. "_text_b")
				end
				view_analytics_event( "filterChange", "browse-filter-genre," .. tab_text )
				set_number = 1
				show_browse_view(10)
				check_product_availability()
				highlight_tab( i )
			end
		)
	end
end

show_browse_view(10)

check_product_availability()
for i = 1, 7 do
	local genre = ProductFamiliesMetaData[ browse_filter_family_tracker.selected_family ].filter_genres[i]
	if browse_filter_genre_selected == genre then
		highlight_tab(i)
		break
	end
end

RegisterProductFamilyTabs()
load_shopping_cart()
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

-- Mingle #3981 for Title Marketing special browse view
TogglePayButton()

if #ShoppingCart.items == 5
and browse_is_cart_full_open == false
and shopping_cart_full_message_shown == false then
	show_shopping_cart_full()
end