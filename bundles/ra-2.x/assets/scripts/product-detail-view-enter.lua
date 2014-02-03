--
-- product-detail-view-enter.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved

local product_list = {}
local previous_image_index = 0

local function get_current_index()
	local image_file = kernel.getactortext("selected_product_image")
	local product = {}
	for k,v in ipairs(product_list) do
		product = v
		if product ~= nil then
			local p = previous_image_index
			if string.sub(image_file, 7) == product.image_file and k > previous_image_index then
				return k
			end
		end
	end
	return 0
end

local function get_product(index, difference)
	local product = {}
	product = product_list[index + difference]
	if product ~= nil then
		browse_selection_selected_product = product
	end
	if browse_selection_selected_product ~= nil then
		local image_registered = kernel.registerimagefromcache(	browse_selection_selected_product.image_file,	browse_selection_selected_product.image_file )
		if image_registered then
			source_image_name = string.format( "cache:%s", browse_selection_selected_product.image_file )
		end
		previous_image_index = index
		--Need to reload product_detail_view, before reloading, pop the stack.
		view_goback_safe( "product_detail_view", true )
		view_show_safe( "product_detail_view" )
	end
end

local move_left = function ()
	local index = get_current_index()
	if index == 1 then
		index = #product_list
		get_product(index, 0)
	else
		get_product(index, -1)
	end
end

kernel.registeractorhitnotify("left_arrow",
	function()
		view_analytics_event( "pageChange", "product-detail-page-back" )
		move_left()
	end)

local move_right = function ()
	local index = get_current_index()
	if index == #product_list then
		index = 1
		get_product(index, 0)
	else
		get_product(index, 1)
	end
end

local show_flag_product_detail = function( releaseDate, days, daysByCalculatedSortDate )

	if ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_title_marketing == true
	then
		-- Mingle #3981 do not show flags in special title marketing mode
		return
	end

	-- Out of Stock
	if ProductCatalog:in_stock( browse_selection_selected_product.product_id, product_history ) == false
	and is_product_selected( browse_selection_selected_product ) == false
	and ( browse_selection_selected_product.is_available
	or browse_selection_selected_product.product_id == 1139 )
	then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_gray_small" )
		kernel.setactortext( "flag", strings.flag_out_of_stock )
		return
	end

	-- Coming Soon
	if browse_selection_selected_product.is_available == false then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_blue_small" )
		kernel.setactortext( "flag", strings.flag_coming_soon )
		return_time = "9pm"
		return
	end

	-- Award Winner
	if browse_selection_selected_product.genres[1002] ~= nil
	and not is_product_selected( browse_selection_selected_product ) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_gold_small" )
		kernel.setactortext( "flag", strings.flag_award_winner )
		return
	end

	-- Special Alternate Title Pricing
	if StoreManager.PricingManager:is_early_released( browse_selection_selected_product )
	and StoreManager.PricingManager:is_alternate_priced( browse_selection_selected_product.product_id )
	and not is_product_selected( browse_selection_selected_product ) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		local product_pricing = StoreManager.PricingManager:get_product_price( browse_selection_selected_product.product_id )
		local first_night_string = "$3"
		if product_pricing.initial_night ~= nil then
			first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
		end
		kernel.setactortext( "flag", string.gsub( strings.flag_price_test, "{0}", first_night_string ) )
		return
	end

	-- Back Again
	if daysByCalculatedSortDate ~= nil
	and daysByCalculatedSortDate <= 14
	and not is_product_selected( browse_selection_selected_product )
	and browse_filter_family_tracker.selected_family ~= "Games"
	and browse_selection_selected_product.genres[1025] ~= nil then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_back_again )
		return
	end

	-- Classics
	if daysByCalculatedSortDate ~= nil
	and daysByCalculatedSortDate <= 14
	and not is_product_selected( browse_selection_selected_product )
	and browse_filter_family_tracker.selected_family ~= "Movies"
	and browse_selection_selected_product.genres[1095] ~= nil then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_classics )
		return
	end

	-- New!
	if days <= 28
	and not is_product_selected( browse_selection_selected_product ) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_new )
		return
	end

	-- Last Chance
	if days >= 105
	and not is_product_selected( browse_selection_selected_product )
	and browse_selection_selected_product.product_id ~= 1139
	and browse_filter_family_tracker.selected_family ~= "Games"
	and is_product_thinned( browse_selection_selected_product.product_id ) == true
	then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_orange_small" )
		kernel.setactortext( "flag", strings.flag_last_chance )
	end
end

kernel.registeractorhitnotify("right_arrow",
	function()
		view_analytics_event( "pageChange", "product-detail-page-next" )
		move_right()
	end)

idle_timer:start()

if browse_selected_sort ~= "alpha" then
local filtered_products_list = ProductCatalog:filter_grid()
for k,v in ipairs(filtered_products_list) do
	local product = v
	if product ~= nil then
		local filtered_list_product_type = ProductTypes[product.product_type_id]
		local filtered_list_product_family = ProductFamilies[filtered_list_product_type.product_family_id]
		if filtered_list_product_family.product_family_name == browse_filter_family_tracker.selected_family then
			table.insert(product_list, product)
		end
	end
	end
else
	product_list = alpha_filtered_products
end

local product_type = ProductTypes[browse_selection_selected_product.product_type_id]
local product_family = ProductFamilies[product_type.product_family_id]
local product_type_md = ProductTypesMetaData[product_type.product_type_id]

local rating_label = ""
local rating = nil
kernel.setmacro( "ProductDetail:Rating", "")
if browse_selection_selected_product.rating_id ~= nil then
	if product_family.product_family_name == "Games" then
		rating_label = "ESRB Rating: "
		rating = ProductRatings[browse_selection_selected_product.rating_id]
	elseif product_family.product_family_name == "Movies" then
		if browse_selection_selected_product.genres[1017] ~= nil then
			rating_label = "TV Rating: "
			rating = TVRatings[browse_selection_selected_product.rating_id]
		else
			rating_label = "MPAA Rating: "
			rating = Ratings[browse_selection_selected_product.rating_id]
		end
	end

	if rating ~= nil and product_family.product_family_name == "Games" then
		kernel.showactor( "rating_label" )
		kernel.showactor( "rating" )
		kernel.showactor( "rating_description" )
		kernel.setactortext( "rating_label", rating_label )
		kernel.setactortext( "rating", "Rated" )
		local empty_spaces = "                                    "
		if rating.rating_description ~= nil
		and rating.rating_description ~= "" then
			kernel.setactortext( "rating_description", empty_spaces.. rating.name.." ".."-".." "..rating.rating_description )
		else
			kernel.setactortext( "rating_description", empty_spaces.. rating.name )
		end
	elseif rating ~= nil then
		if kernel.getculture() == "en-US" then
			kernel.setactortext( "rating_label", rating_label )
		end
		kernel.setmacro( "ProductDetail:Rating", rating.name )
	else
		kernel.setactortext( "rating_label", "" )
	end

	if rating ~= nil then
		if rating.name ~= nil or rating.name ~= "" then
			kernel.setmacro( "ProductDetail:Rating", rating.name )
			if string.len(rating.name) > 5 then
				kernel.showactor( "selected_product_Rating_big" )
				kernel.hideactor( "selected_product_Rating" )
				kernel.moveactorx( "selected_product_Media", 17 )
				kernel.moveactorx( "selected_product_Media_big", 17 )
				kernel.moveactorx( "selected_product_running_time", 17 )
				kernel.moveactorx( "selected_product_national_street_date", 17 )
				kernel.moveactorx( "selected_product_CC", 17 )
			else
				kernel.hideactor( "selected_product_Rating_big" )
				kernel.showactor( "selected_product_Rating" )
				kernel.setactorx( "selected_product_Media", 602 )
				kernel.setactorx( "selected_product_Media_big", 602 )
				kernel.setactorx( "selected_product_running_time", 650 )
				kernel.setactorx( "selected_product_national_street_date", 698 )
				kernel.setactorx( "selected_product_CC", 748 )
			end
		else
			kernel.hideactor( "selected_product_Rating" )
			kernel.hideactor( "selected_product_Rating_big" )
			kernel.moveactorx( "selected_product_Media", -56 )
			kernel.moveactorx( "selected_product_Media_big", -56 )
			kernel.moveactorx( "selected_product_running_time", -56 )
			kernel.moveactorx( "selected_product_national_street_date", -56 )
			kernel.moveactorx( "selected_product_CC", -56 )
		end
	else
		kernel.hideactor( "selected_product_Rating" )
		kernel.hideactor( "selected_product_Rating_big" )
		kernel.moveactorx( "selected_product_Media", -56 )
		kernel.moveactorx( "selected_product_Media_big", -56 )
		kernel.moveactorx( "selected_product_running_time", -56 )
		kernel.moveactorx( "selected_product_national_street_date", -56 )
		kernel.moveactorx( "selected_product_CC", -56 )
	end
end

if string.len(product_type.product_type_name) > 3 then
	kernel.showactor( "selected_product_Media_big" )
	kernel.hideactor( "selected_product_Media" )
	kernel.moveactorx( "selected_product_running_time", 32)
	kernel.moveactorx( "selected_product_national_street_date", 32)
	kernel.moveactorx( "selected_product_CC", 32)
else
	kernel.hideactor( "selected_product_Media_big" )
	kernel.showactor( "selected_product_Media" )
end
kernel.setmacro( "ProductDetail:Product_type", string.upper( tostring( product_type.product_type_name ) ) )

if product_family.product_family_name == "Movies"
and browse_selection_selected_product.running_time ~= nil
and browse_selection_selected_product.running_time ~= ""
and browse_selection_selected_product.running_time ~= "00:00" then
	local index = string.find( tostring(browse_selection_selected_product.running_time), ":" )
	if index ~= nil then
		local hour = string.sub( browse_selection_selected_product.running_time, 1, index - 1 )
		local minutes = string.sub( browse_selection_selected_product.running_time, index + 1 )

		local first_char = string.sub(hour,1,1)
		local second_char= string.sub(hour,2,-1)

		if string.len(hour) == 2 and first_char == "0" then
			hour = second_char
		else
			hour = string.sub( browse_selection_selected_product.running_time, 1, index - 1 )
		end
		kernel.setmacro( "ProductDetail:RunningTime", tostring(hour .. ":" .. minutes) )
	else
		kernel.setmacro( "ProductDetail:RunningTime", tostring(browse_selection_selected_product.running_time) )
	end
else
	kernel.setmacro( "ProductDetail:RunningTime", "" )
	kernel.setactorx( "selected_product_CC", kernel.getactorx( "selected_product_national_street_date" ) + 2 )
	kernel.setactorx( "selected_product_national_street_date", kernel.getactorx( "selected_product_running_time" ) )
	kernel.hideactor( "selected_product_running_time" )
end

local closed_captioned = browse_selection_selected_product.closed_captioned
if  closed_captioned ~= nil and closed_captioned == true then
	kernel.showactor( "selected_product_CC" )
else
	kernel.hideactor( "selected_product_CC" )
end

local national_street_date = browse_selection_selected_product.national_street_date
if  national_street_date ~= nil and national_street_date ~= "" then
	kernel.showactor( "selected_product_national_street_date" )
	kernel.setmacro( "ProductDetail:NationalStreetDate", string.sub( national_street_date, 1, 4) )
else
	kernel.hideactor( "selected_product_national_street_date" )
	kernel.setactorx( "selected_product_CC", kernel.getactorx( "selected_product_national_street_date" ))
end

if product_type_md == nil then
	AlertManager.send_alert(
		"ApplicationCrash",
		"Missing Product Type",
		"Product Type '" ..
			product_type.product_type_name ..
			"' (" ..
			product_type.product_type_id ..
			") is missing a product_type_md key.")
end

local product_index = 0
for k,v in ipairs(product_list) do
	if v.product_id == browse_selection_selected_product.product_id then
		previous_image_index = product_index
		break
	else
		product_index = product_index + 1
	end
end

--Vaibhav: set image of the product
if product_family.product_family_name == "Movies" then
	if kernel.getculture() == "es-MX" then
		source_image_name = "image_movie_missing_box_art_es_MX"
	else
		source_image_name = "image_movie_missing_box_art"
	end
elseif product_family.product_family_name == "Games" then
	if kernel.getculture() == "es-MX" then
		source_image_name = "image_game_missing_box_art_es_MX"
	else
		source_image_name = "image_game_missing_box_art"
	end
end
local image_registered = kernel.registerimagefromcache(	browse_selection_selected_product.image_file, browse_selection_selected_product.image_file )

if image_registered then
	source_image_name = string.format("cache:%s", browse_selection_selected_product.image_file)
end
kernel.setactorimage("selected_product_image", source_image_name)
kernel.setactortext("selected_product_image", string.format("cache:%s", browse_selection_selected_product.image_file))

local releaseDate = string.sub(browse_selection_selected_product.release_date, 1, 8)
local calculatedSortDate = string.sub(browse_selection_selected_product.sort_date, 1, 8)
local days = kernel.getelapseddays( releaseDate, get_current_date_string( "%4d%02d%02d" ) )
local daysByCalculatedSortDate = kernel.getelapseddays( calculatedSortDate, get_current_date_string( "%4d%02d%02d" ) )

local temp_time = os.time() + ( 24*60*60 )
local return_time = os.date( "9pm " .. "%A, %m/%d/%y", ( temp_time ) )
if kernel.getculture() == "es-MX" then
	local english_day = os.date("%A", temp_time)
	local spanish_day = strings[english_day]
	return_time = string.gsub(return_time, english_day, spanish_day)
end

local product_history = ProductHistory[browse_selection_selected_product.product_id]

-- Draw the appropriate flag above the box art
show_flag_product_detail( releaseDate, days, daysByCalculatedSortDate )

if product_type.product_type_name == "Blu-ray" then
	kernel.showactor("banner")
	kernel.setactorimage("banner","image_banner_blu_ray")
	kernel.setactortext("banner", "Blu-ray")
end

if product_type_md ~= nil
and product_type_md.detail_banner_resource_name ~= nil then
	kernel.setmacro( "ProductDetail:ProductType", string.upper( tostring( product_type.product_type_name ) ) )
else
	kernel.setmacro( "ProductDetail:ProductType", "" )
end

kernel.setmacro( "ProductDetail:Name", browse_selection_selected_product.long_name )

local title_size = kernel.measureactortext("selected_product_title")
local title_label = browse_selection_selected_product.long_name
while title_size.lines_filled > 2 do
	title_label = string.sub(title_label, 0, string.len(title_label) - 5).. "..."
	kernel.setactortext("selected_product_title", title_label)
	title_size = kernel.measureactortext("selected_product_title")
end

kernel.setmacro( "ProductDetail:Description", browse_selection_selected_product.description )

if browse_selection_selected_product.starring ~= nil then
	local starring_label = ""
	for _, v in ipairs( browse_selection_selected_product.starring ) do
		if v ~= "" then
			if string.len( starring_label ) + 2 + string.len(v) > 70 then
				break
			end
			if string.len( starring_label ) > 0 then
				starring_label = starring_label .. ", "
			end
			starring_label = starring_label .. v
		end
	end
	if product_family.product_family_name == "Games" then
		kernel.hideactor( "starring_label" )
		kernel.hideactor( "starring" )
		kernel.moveactory( "category_label", -20 )
		kernel.moveactory( "category", -20 )
		kernel.moveactory( "running_time_label", -20 )
		kernel.moveactory( "running_time", -20 )
		kernel.moveactory( "rating_label", -20 )
		kernel.moveactory( "rating", -20 )
		kernel.moveactory( "rating_description", -20 )
	else
		kernel.setmacro( "ProductDetail:Starring", starring_label )
	end
end

local product_genre = nil
local genre_count = 0
for i,v in pairs(browse_selection_selected_product.genres) do
	if v == true then
		local genres = ProductGenreData[product_family.product_family_name].filter_genres[i]
		if genres ~= nil and product_genre == nil then
			product_genre = genres
			genre_count = genre_count + 1
		elseif genres ~= nil and product_genre ~= nil then
			product_genre = product_genre..", "..genres
		end
	end
end
kernel.setmacro( "ProductDetail:Category", product_genre )

local product_pricing = StoreManager.PricingManager:get_product_price( browse_selection_selected_product.product_id )
local product_price_test = StoreManager.PricingManager:is_early_released( browse_selection_selected_product )

if product_price_test == false
and browse_filter_mode == "coming-soon"
and browse_selection_selected_product.is_available == false
and StoreManager.PricingManager:is_alternate_priced(browse_selection_selected_product.product_id) then
	product_price_test = true
end

kernel.setmacro( "ProductDetail:FirstNightPrice", string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night ) )
kernel.setmacro( "ProductDetail:ExtraNightPrice", string.format( "%s%.2f", strings.currency_symbol, product_pricing.extra_night ) )

kernel.setmacro( "ProductDetail:ReturnTime", return_time )

-- Sub-Platform Indicator
if product_family.product_family_name == "Movies" then
	kernel.hideactor( "separator_3" )
	kernel.hideactor( "sub_platform_indicator" )
else
	if browse_selection_selected_product.sub_platform_id ~= nil then
		kernel.setactorimage( "sub_platform_indicator", SubPlatformIndicatorData[browse_selection_selected_product.sub_platform_id].sub_platform_image )
		kernel.showactor( "separator_3" )
		kernel.showactor( "sub_platform_indicator" )
		kernel.setactorwidth( "rating_description", 230 )
		if kernel.getculture() == "es-MX" then
			kernel.setactorx( "separator_3", 774 )
			kernel.setactorx( "sub_platform_indicator", 776 )
			kernel.setactorwidth( "return_time" , 100 )
		elseif kernel.getculture() == "en-US" then
			kernel.setactorx( "separator_3", 764 )
			kernel.setactorx( "sub_platform_indicator", 766 )
			kernel.setactorwidth( "return_time", 160 )
		end
	else
		kernel.hideactor( "separator_3" )
		kernel.hideactor( "sub_platform_indicator" )
		kernel.setactorwidth( "rating_description", 380 )

		if kernel.getculture() == "es-MX" then
			kernel.setactorwidth( "return_time" , 240 )
		elseif kernel.getculture() == "en-US" then
			kernel.setactorwidth( "return_time", 330 )
		end
	end
end

if browse_selection_selected_product.is_available
and ProductCatalog:in_stock( browse_selection_selected_product.product_id ) then

	local add_product_to_cart_as_buy = function()
		view_analytics_event( "buttonPress", "buy," .. tostring( browse_selection_selected_product.product_id))
		browse_selection_cart_action = "purchase"
		if browse_selection_selected_product.product_id ~= 1139 then
			check_for_warning_popups( check_alert_table, browse_selection_selected_product, nil )
		else
			add_item_to_shopping_cart( browse_selection_selected_product, nil, "purchase" )
		end
		end

	kernel.registeractorhitnotify(
		"buy_used_dvd_button",
		function()
			item_added_from_view = kernel.peekview()
			add_product_to_cart_as_buy()
		end
	)

	local function add_item_tocart()
		browse_selection_cart_action = "rental"
		check_for_warning_popups( check_alert_table, browse_selection_selected_product, nil )
	end

	local add_product_to_cart_as_rent = function()
		view_analytics_event( "buttonPress", "rent," .. tostring( browse_selection_selected_product.product_id ))

		local message_view_manager = newmessageviewmanager()
		message_view_manager:hide( true )
		if( StoreManager.PricingManager:is_early_released( browse_selection_selected_product )) then
			message_view_manager:show()
			view_analytics_event( "context", "$3 Firsts warning message" )
			idle_timer:start()
			message_view_manager:hide_title()

			-- The message needs to include the prices from the actual price file (and MD adjustment if needed)
			local product_pricing = StoreManager.PricingManager:get_product_price( browse_selection_selected_product.product_id )

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
					add_item_tocart()
				end )
			message_view_manager:show_back_button_arrow(
				function()
					message_view_manager:hide_message()
				end )
		else
			add_item_tocart()
		end
	end

	kernel.registeractorhitnotify(
		"add_button",
		function()
			item_added_from_view = kernel.peekview()
			add_product_to_cart_as_rent()
		end
	)
	kernel.registeractorhitnotify(
		"plus_button",
		function()
			item_added_from_view = kernel.peekview()
			add_product_to_cart_as_rent()
		end
	)

	local current_date_string = get_current_date_string( "%4d%02d%02d%02d%02d" )

	if browse_selection_selected_product.product_id == 1139 then
		local price_purchase = StoreManager.PricingManager:get_empty_case_price()
		kernel.setmacro( "ProductDetail:PurchasePrice", string.format( "%s%.2f", strings.currency_symbol, price_purchase ) )
	else
		local count, as_a_rental = ShoppingCart:count_for_item( browse_selection_selected_product.product_id )

		if product_family.product_family_name == "Games" then
			local price_purchase = nil
			if browse_selection_selected_product.sellthru_price ~= nil then
				price_purchase = StoreManager.PricingManager:get_adjusted_price( browse_selection_selected_product.sellthru_price, "purchase" )
			end

			if GamesSellThru == true
			and price_purchase ~= nil
			and price_purchase > 0
			and browse_selection_selected_product.sell_thru == true
			and browse_selection_selected_product.sellthru_date ~= nil
			and browse_selection_selected_product.sellthru_date <= current_date_string
			and browse_selection_selected_product.can_be_sold
			and EnableSellThruOfUsed == true then
				kernel.setmacro( "ProductDetail:PurchasePrice", string.format( "%s%.2f", strings.currency_symbol, price_purchase ) )
			else
				kernel.hideactor( "buy_used_dvd_button" )
				kernel.hideactor( "buy_used_amount_label" )
			end
		else
			local price_purchase = StoreManager:get_purchase_price()

			if StoreManager:is_sell_thru_enabled()
			and price_purchase ~= nil
			and price_purchase > 0
			and browse_selection_selected_product.sell_thru == true
			and browse_selection_selected_product.can_be_sold
			and ( count == 0 ) then
				kernel.setmacro( "ProductDetail:PurchasePrice", string.format( "%s%.2f", strings.currency_symbol, price_purchase ) )
			else
				kernel.hideactor( "buy_used_dvd_button" )
				kernel.hideactor( "buy_used_amount_label" )
			end
		end
	end

	kernel.log( "Adjusted purchase price string = " .. tostring( kernel.getmacro( "ProductDetail:PurchasePrice" )))
	kernel.markactordirty( "buy_used_amount_label" )
else
	if browse_selection_selected_product.is_available == false then
		local coming_soon_release_date = StoreManager.PricingManager:get_early_release_date(browse_selection_selected_product)
		local release_date = getmonth(string.sub(coming_soon_release_date, 5, 6 )).." " .. string.sub(coming_soon_release_date, 7, 8 )
		kernel.hideactor("banner")
		kernel.showactor("coming_soon_violator")
		kernel.hideactor("flag")
		kernel.setactorimage("coming_soon_violator","image_banner_coming_soon")
		kernel.setactortext("coming_soon_violator",strings.coming_label.." "..release_date )
		if ComingSoonOptIn == true then
			kernel.registeractorhitnotify( "email_me_button",
				function()
					if ShoppingCart.coming_soon_optin.email_address == nil then
						local coming_soon_optin_keyboard_manager = newkeyboardmanager()
						coming_soon_optin_keyboard_manager:init( 64 )
						coming_soon_optin_keyboard_manager:set_message( strings.coming_soon_email_optin_email_prompt_message )
						coming_soon_optin_keyboard_manager:set_marketing( strings.coming_soon_email_optin_email_marketing_message )
						show_email_confirmation = false
						coming_soon_optin_keyboard_manager:set_validate_event(
							function( text )
								local coming_soon_optin_email_validator = newemailvalidator()
								if coming_soon_optin_email_validator.validate_email( text ) == false then
									return strings.email_error_message
								end
								return nil
							end
						)
						coming_soon_optin_keyboard_manager:set_enter_event(
							function()
								ShoppingCart.coming_soon_optin.email_address = coming_soon_optin_keyboard_manager:get_value()
								table.insert( ShoppingCart.coming_soon_optin.optin_items,
									{
										item_id = tostring( browse_selection_selected_product.product_id ),
										item_type = "KioskRentalTitleID",
										optin_method_type = "EmailAddress",
										optin_method_data = ShoppingCart.coming_soon_optin.email_address,
										template_name = "EmailTemplate",
										campaign_name = "ComingSoonRentalCampaign"
									}
								)
								view_goback_safe( "coming soon email optin - email entry view" )
								view_goback_safe( "coming soon email optin - product detail view" )
							end
						)
						coming_soon_optin_keyboard_manager:show( strings.email_tag )
					else
						if ShoppingCart.coming_soon_optin:indexof_optin_item( browse_selection_selected_product.product_id ) == 0 then
							table.insert( ShoppingCart.coming_soon_optin.optin_items,
								{
									item_id = tostring( browse_selection_selected_product.product_id ),
									item_type = "KioskRentalTitleID",
									optin_method_type = "EmailAddress",
									optin_method_data = ShoppingCart.coming_soon_optin.email_address,
									template_name = "EmailTemplate",
									campaign_name = "ComingSoonRentalCampaign"
								}
							)
						end
						view_goback_safe( "coming soon email optin - product detail view" )
					end
				end
			)
			kernel.hideactor( "coming_soon_violator" )
			kernel.showactor( "coming_soon_date" )
			release_date = string.sub( coming_soon_release_date, 5, 6 ) .. "/" .. string.sub( coming_soon_release_date, 7, 8 )
			if string.sub( release_date, 1, 1 ) == "0" then
				release_date = string.sub( release_date, 2 )
			end
			kernel.setactortext( "coming_soon_date", strings.coming_label .. " " .. release_date )
			kernel.showactor( "email_me_button" )
		end
	elseif browse_selection_selected_product.is_available == true
	and ProductCatalog:in_stock( browse_selection_selected_product.product_id ) == false then
		kernel.setactoropacity( "selected_product_image", 0.4 )
		if ShoppingCart:has_item( browse_selection_selected_product.product_id ) == false
		and browse_selection_selected_product.product_id ~= 1139 then
			kernel.showactor( "out_of_stock_line1" )
			kernel.showactor( "out_of_stock_line2" )
			local out_of_stock_message
			if kernel.getculture() == "en-US" then
				out_of_stock_message = string.gsub( kernel.getactortext("out_of_stock_line2"), "{0}", string.sub( string.lower( product_family.product_family_name ), 0 , ( string.len( product_family.product_family_name ) - 1 )))
			elseif kernel.getculture() == "es-MX" then
				if product_family.product_family_name == "Movies" then
					out_of_stock_message = string.gsub( kernel.getactortext("out_of_stock_line2"), "{0}", strings.Movie)
				else
					out_of_stock_message = string.gsub( kernel.getactortext("out_of_stock_line2"), "{0}", string.sub( string.lower( strings.Games ), 0 , ( string.len( strings.Games ) - 1 )))
				end
			end

			kernel.setactortext( "out_of_stock_line2" , out_of_stock_message )
			kernel.showactor( "out_of_stock_line3" )
		end
	end
	kernel.hideactor( "buy_used_dvd_button" )
	kernel.hideactor( "buy_used_amount_label" )
	kernel.hideactor( "add_button" )
	kernel.hideactor( "plus_button" )
end

for k,v in ipairs(ShoppingCart.items) do
	if v.product_id == browse_selection_selected_product.product_id
	and browse_selection_selected_product.product_id ~= 1139 then
		kernel.hideactor( "buy_used_dvd_button" )
		kernel.hideactor("add_button")
		kernel.hideactor( "plus_button" )
		kernel.setactoropacity( "selected_product_image", 0.4 )
	end
end

if #ShoppingCart.items == 5 then
	kernel.hideactor( "buy_used_dvd_button" )
	kernel.hideactor( "buy_used_amount_label" )
	kernel.hideactor("add_button")
	kernel.hideactor( "plus_button" )
end

kernel.registerkeypressnotify(
	"product_detail_view",
	function( key, keyCode, modifier )
		if modifier == "Control" and keyCode == 4 then
			view_show_safe( "statistics_popup_view" )
		end
	end
)

if browse_selection_selected_product.product_id == 1139 then
	-- Hide these fields for empty case whether it is in stock or out of stock
	kernel.hideactor( "starring_label" )
	kernel.hideactor( "starring" )
	kernel.hideactor( "category_label" )
	kernel.hideactor( "category" )
	kernel.hideactor( "running_time_label" )
	kernel.hideactor( "running_time" )
	kernel.hideactor( "rating_label" )
	kernel.hideactor( "rating" )
	kernel.hideactor( "rating_description" )
	kernel.hideactor( "first_night_price_label" )
	kernel.hideactor( "first_night_price" )
	kernel.hideactor( "extra_night_price_label" )
	kernel.hideactor( "extra_night_price" )
	kernel.hideactor( "return_time_label" )
	kernel.hideactor( "return_time" )
	kernel.hideactor( "add_button" )
	kernel.hideactor( "plus_button" )
	kernel.hideactor( "first_night_price_a_day" )
end

if product_price_test then
	if product_family.product_family_name == "Movies" then
	kernel.setactortext( "first_night_price_label" , strings.first_day_price )
	kernel.hideactor( "first_night_price_a_day" )
	kernel.setactortext( "extra_night_price_label" , strings.extra_day_price )
	else
		kernel.setactortext( "first_night_price_label" , strings.first_day_price )
		kernel.hideactor( "first_night_price_a_day" )
		kernel.setactortext( "extra_night_price_label" , strings.extra_day_price )
		kernel.setactory( "first_night_price_label" , 551 )
		kernel.setactory( "first_night_price" , 551 )
		kernel.setactory( "first_night_price_a_day" , 551 )
		kernel.setactory( "extra_night_price_label" , 571 )
		kernel.setactory( "extra_night_price" , 571 )
		kernel.setactory( "return_time_label" , 605 )
		kernel.setactory( "return_time" , 591 )
		kernel.setactory( "separator_2" , 611 )
	end
else
	if browse_selection_selected_product.product_id ~= 1139 then
		kernel.setactortext( "first_night_price_label" , strings.price_tag )
		kernel.showactor( "first_night_price_a_day" )
		kernel.setactory( "first_night_price_label" , 551 )
		kernel.setactory( "first_night_price_a_day" , 551 )
		kernel.setactory( "first_night_price" , 551 )
		if kernel.getculture() == "en-US" then
			kernel.setactorx( "first_night_price" , 586 )
			kernel.setactorx( "first_night_price_a_day" , 624 )
		elseif kernel.getculture() == "es-MX" then
			kernel.setactorx( "first_night_price" , 595 )
			kernel.setactorx( "first_night_price_a_day" , 634 )
		end
	end
	kernel.hideactor( "extra_night_price_label" )
	kernel.hideactor( "extra_night_price" )
end

if browse_selection_selected_product.product_id == 1139 then
	kernel.hideactor( "selected_product_Rating" )
	kernel.hideactor( "selected_product_Rating_big" )
	kernel.hideactor( "selected_product_Media" )
	kernel.hideactor( "selected_product_Media_big" )
	kernel.hideactor( "selected_product_running_time" )
	kernel.hideactor( "selected_product_national_street_date" )
	kernel.hideactor( "selected_product_CC" )
	kernel.hideactor( "separator_1" )
end

if #product_list == 1 then
	kernel.hideactor( "left_arrow" )
	kernel.hideactor( "right_arrow" )
end

view_analytics_event( "context", "productDisplayed," .. tostring( browse_selection_selected_product.product_id ) )