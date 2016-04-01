--
-- product-detail-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved

local product_list = {}

local display_product = browse_selection_selected_product
-- for the blu-ray filter only (non-ADA), check if there is a parent title rollup product. If so then use that 
if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Blu-ray" and browse_selection_selected_product.kiosk_product_type ~= 3 
		and browse_selection_selected_product.title_rollup_product_group_id ~= nil 
		and kernel.getfilter( "ada_accessible" ) ~= "True" then
	local parent_product = nil
	parent_product = kernel.rental_get_title_rollup(browse_selection_selected_product.title_rollup_product_group_id)
	if parent_product ~= nil and parent_product.kiosk_product_type == 3 then
		display_product = parent_product
		if VerboseLogging then
			kernel.log("Changing display product from title product to title rollup")
		end
	end
end

local product_type = ProductTypes[display_product.product_type_id]
local product_family = ProductFamilies[product_type.product_family_id]
local product_type_md = ProductTypesMetaData[product_type.product_type_id]

-- based on the kiosk product type figure out if the product is dvd, blu-ray, both, or other (game or case) 
local dvd_product = nil
local bluray_product = nil
local other_product = nil  -- game or case which use the old Add and Buy Buttons

-- Determine what type of kiosk product is being displayed.
if display_product.kiosk_product_type == 3 and display_product.product_id ~= 1139 then 
	-- TitleRollup
	if display_product.dvd_index ~= nil and display_product.dvd_index > 0 then
		dvd_product = display_product.kiosk_products[display_product.dvd_index]
	end
	if display_product.blu_ray_index ~= nil and display_product.blu_ray_index > 0 then
		bluray_product = display_product.kiosk_products[display_product.blu_ray_index]
	end
else
	-- Title and MDV 
	if display_product.product_id ~= 1139 and product_family.product_family_name == "Movies" then
		if product_type.product_type_name == "DVD" then
			dvd_product = display_product
		elseif product_type.product_type_name == "Blu-ray" then
			bluray_product = display_product
		end
	else
	  other_product = display_product;
	end
end

-- initialize variables used to enable/disable buttons
local current_date_string = get_current_date_string( "%4d%02d%02d%02d%02d" )
local product_is_case = other_product ~= nil and other_product.product_id == 1139
local shopping_cart_is_full = #ShoppingCart.items == 5

local case_price_purchase = StoreManager.PricingManager:get_empty_case_price()
local movie_price_purchase = StoreManager:get_purchase_price()
local sell_through_enabled = StoreManager:is_sell_thru_enabled()

-- DVD variables
local dvd_product_ui_info = get_product_ui_info(dvd_product)
local dvd_available = dvd_product_ui_info["available"]
local dvd_available_and_instock = dvd_product_ui_info["available_and_instock"]
local dvd_already_in_cart = dvd_product_ui_info["already_in_cart"]
local dvd_is_purchasable = dvd_product_ui_info["is_purchasable"]
local dvd_show_buy_button = dvd_product_ui_info["show_buy_button"]
local dvd_show_in_purchase_overlay = dvd_product_ui_info["show_in_purchase_overlay"]


-- Blu-ray variables
local bluray_product_ui_info = get_product_ui_info(bluray_product)
local bluray_available =  bluray_product_ui_info["available"]
local bluray_available_and_instock = bluray_product_ui_info["available_and_instock"]
local bluray_already_in_cart = bluray_product_ui_info["already_in_cart"]
local bluray_is_purchasable = bluray_product_ui_info["is_purchasable"]
local bluray_show_buy_button = bluray_product_ui_info["show_buy_button"]
local bluray_show_in_purchase_overlay = bluray_product_ui_info["show_in_purchase_overlay"]
	

-- Other product variables (games and cases)
local game_price_purchase = nil
if other_product ~= nil and other_product.sellthru_price ~= nil then
	game_price_purchase = StoreManager.PricingManager:get_adjusted_price( other_product.sellthru_price, "purchase" )
end
local other_product_ui_info = get_product_ui_info(other_product)
local other_product_available = other_product_ui_info["available"]
local other_product_available_and_instock = other_product_ui_info["available_and_instock"]
local other_product_already_in_cart = other_product_ui_info["already_in_cart"]
local other_product_is_purchasable = other_product_ui_info["is_purchasable"]

if VerboseLogging then
	local dvdProductName = nil
	local dvdProductId = nil
	if dvd_product ~= nil then
	  dvdProductName = dvd_product.long_name
	  dvdProductId = dvd_product.product_id
	end

	local blurayProductName = nil
	local blurayProductId = nil
	if bluray_product ~= nil then
		blurayProductName = bluray_product.long_name
		blurayProductId = bluray_product.product_id
	end

	local otherProductName = nil
	local otherProductId = nil
	if other_product ~= nil then
		otherProductName = other_product.long_name
		otherProductId = other_product.product_id
	end
		
	kernel.log("browse_selection_selected_product: " .. tostring(browse_selection_selected_product ~= nil and browse_selection_selected_product.product_id or nil) .. " " .. tostring(browse_selection_selected_product ~= nil and browse_selection_selected_product.long_name or nil))
	kernel.log("Kiosk Product Type: " .. display_product.kiosk_product_type)
	kernel.log("Product Type: " .. product_type.product_type_name)
	kernel.log("DVD product: " .. tostring(dvdProductId) .. " " .. tostring(dvdProductName) )
	kernel.log("Blu-ray product: " .. tostring(blurayProductId) .. " " .. tostring(blurayProductName))
	kernel.log("Other product: " .. tostring(otherProductId) .. " " .. tostring(otherProductName))
	kernel.log("Store Manager Sell through enabled: " .. tostring(sell_through_enabled))
	kernel.log("Movie purchase price: " .. tostring(movie_price_purchase))
	kernel.log("Game purchase price: " .. tostring(game_price_purchase))
	kernel.log("Case purchase price: " .. tostring(case_price_purchase))
	kernel.log("Product is case: " .. tostring(product_is_case))
	kernel.log("Shopping cart is full: " .. tostring(shopping_cart_is_full))
	kernel.log("DVD available: " .. tostring(dvd_available))
	kernel.log("DVD available and instock: " .. tostring(dvd_available_and_instock))
	kernel.log("DVD already in cart: " .. tostring(dvd_already_in_cart))
	kernel.log("DVD is purchasable: " .. tostring(dvd_is_purchasable))
	kernel.log("DVD show buy button: " .. tostring(dvd_show_buy_button))
	kernel.log("DVD show in purchase overlay: " .. tostring(dvd_show_in_purchase_overlay))
	kernel.log("Blu-ray available: " .. tostring(bluray_available))
	kernel.log("Blu-ray available and instock: " .. tostring(bluray_available_and_instock))
	kernel.log("Blu-ray already in cart: " .. tostring(bluray_already_in_cart))
	kernel.log("Blu-ray is purchasable: " .. tostring(bluray_is_purchasable))
	kernel.log("Blu-ray show buy button: " .. tostring(bluray_show_buy_button))
	kernel.log("Blu-ray show in purchase overlay: " .. tostring(bluray_show_in_purchase_overlay))
	kernel.log("Other product available: " .. tostring(other_product_available))
	kernel.log("Other product available and instock: " .. tostring(other_product_available_and_instock))
	kernel.log("Other product already in cart: " .. tostring(other_product_already_in_cart))
	kernel.log("Other product is purchasable: " .. tostring(other_product_is_purchasable))
end 

local function get_current_index()
	-- first check to see if the product_detail_index is still correct.  If not then take the time to loop through the list to find the right one
	if product_list ~= nil and product_detail_index ~= nil and product_detail_index <= #product_list and product_list[product_detail_index] ~= nil and browse_selection_selected_product ~= nil
			and product_list[product_detail_index].product_id == browse_selection_selected_product.product_id then
		return product_detail_index
	else
		-- Look through all the products and find the matching index
		for k,product in ipairs(product_list) do
			if product ~= nil and browse_selection_selected_product.product_id == product.product_id then
				product_detail_index = k
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
		product_detail_index = index + difference
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
		increment_paddle_click_count()
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
	if 
	((dvd_available or bluray_available) and 
				-- if they exist, both dvd and bluray must be out of stock and not in the cart 
				(not dvd_available or (dvd_available and not dvd_available_and_instock and not dvd_already_in_cart)) and
				(not bluray_available or (bluray_available and not bluray_available_and_instock and not bluray_already_in_cart))
			)
			or
			(other_product_available and not other_product_available_and_instock and not other_product_already_in_cart and not product_is_case) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_gray_small" )
		kernel.setactortext( "flag", strings.flag_out_of_stock )
		return
	end

	-- Coming Soon
	if ((dvd_product ~= nil or bluray_product ~= nil) and dvd_available == false and bluray_available == false) or
		(other_product ~= nil and other_product_available == false) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_blue_small" )
		kernel.setactortext( "flag", strings.flag_coming_soon )
		return_time = "9pm"
		return
	end

	-- Award Winner
	if display_product.genres[1002] ~= nil then
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_gold_small" )
		kernel.setactortext( "flag", strings.flag_award_winner )
		return
	end

	-- Special Alternate Title Pricing
	if StoreManager.PricingManager:is_early_released( display_product )
	and StoreManager.PricingManager:is_alternate_priced( display_product.product_id ) then
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		local product_pricing = StoreManager.PricingManager:get_product_price( display_product.product_id )
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
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product)
	and browse_filter_family_tracker.selected_family ~= "Games"
	and display_product.genres[1025] ~= nil then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_back_again )
		return
	end

	-- Classics
	if daysByCalculatedSortDate ~= nil
	and daysByCalculatedSortDate <= 14
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product)
	and browse_filter_family_tracker.selected_family ~= "Movies"
	and display_product.genres[1095] ~= nil then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_classics )
		return
	end

	-- New!
	if days <= 28 then
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product) then
		kernel.showactor( "flag" )
		kernel.setactorimage( "flag", "image_flag_red_small" )
		kernel.setactortext( "flag", strings.flag_new )
		return
	end

	-- Last Chance
	if days >= 105
	--and not is_product_selected(dvd_product) and not is_product_selected(bluray_product) and not is_product_selected(other_product)
	and not product_is_case
	and browse_filter_family_tracker.selected_family ~= "Games"
	and kernel.is_product_thinned( display_product.product_id ) == true
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
		increment_paddle_click_count()
	end)

idle_timer:start()

-- get the ordered list of products to be shown
if product_detail_shown_from_view == "browse_view_collection" then
	product_list = browse_collection_products
elseif kernel.get_session_value("BrowseState.BrowseSelectedSort") ~= "alpha" then
	product_list = ProductCatalog:filter_grid()
else
	product_list = alpha_filtered_products
end

-- display rating text and labels
local rating_label = ""
local rating = nil
kernel.setmacro( "ProductDetail:Rating", "" )
kernel.setmacro( "TTS:RatingLabel", "" )
kernel.setmacro( "TTS:Rating", "" )
kernel.setmacro( "TTS:RatingDescription", "" )
if display_product.rating_id ~= nil then
	if product_family.product_family_name == "Games" then
		rating_label = "ESRB Rating: "
		rating = ProductRatings[display_product.rating_id]
	elseif product_family.product_family_name == "Movies" then
		if display_product.genres[1017] ~= nil then
			rating_label = "TV Rating: "
			rating = TVRatings[display_product.rating_id]
		else
			rating_label = "MPAA Rating: "
			rating = Ratings[display_product.rating_id]
		end
	end

	if rating ~= nil and product_family.product_family_name == "Games" then
		kernel.showactor( "rating_label" )
		kernel.showactor( "rating" )
		kernel.showactor( "rating_description" )
		kernel.setactortext( "rating_label", rating_label )
        kernel.setmacro( "TTS:RatingLabel", rating_label )
		kernel.setactortext( "rating", "Rated" )
		local empty_spaces = "                                    "
        if rating.rating_description ~= nil
		and rating.rating_description ~= "" then
			kernel.setactortext( "rating_description", empty_spaces .. rating.name .. " - " .. rating.rating_description )
            kernel.setmacro( "TTS:Rating", rating.name )
            kernel.setmacro( "TTS:RatingDescription", rating.rating_description )
		else
			kernel.setactortext( "rating_description", empty_spaces .. rating.name )
            kernel.setmacro( "TTS:Rating", rating.name )
		end
	elseif rating ~= nil then
		if kernel.getculture() == "en-US" then
			kernel.setactortext( "rating_label", rating_label )
            kernel.setmacro( "TTS:RatingLabel", rating_label )
		end
		kernel.setmacro( "ProductDetail:Rating", rating.name )
        kernel.setmacro( "TTS:Rating", rating.name )
	else
		kernel.setactortext( "rating_label", "" )
        kernel.setmacro( "TTS:RatingLabel", "" )
	end

	-- adjust position and visibility of actors based on whether rating is shown
	if not product_is_case and rating ~= nil and (rating.name ~= nil or rating.name ~= "") then
		kernel.setmacro( "ProductDetail:Rating", rating.name )
        kernel.setmacro( "TTS:Rating", "Rated " .. rating.name )
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
			kernel.setactorx( "selected_product_running_time", 602 )
			kernel.setactorx( "selected_product_national_street_date", 650 )
			kernel.setactorx( "selected_product_CC", 700 )
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

 -- Product type 
if product_family.product_family_name == "Movies" or product_is_case then
		kernel.hideactor( "selected_product_Media_big" )
		kernel.hideactor( "selected_product_Media" )
else 
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
end

-- Set running time
if not product_is_case and product_family.product_family_name == "Movies"
and display_product.running_time ~= nil
and display_product.running_time ~= ""
and display_product.running_time ~= "00:00" then
	local index = string.find( tostring(display_product.running_time), ":" )
	if index ~= nil then
		local hour = string.sub( display_product.running_time, 1, index - 1 )
		local minutes = string.sub( display_product.running_time, index + 1 )

		local first_char = string.sub(hour,1,1)
		local second_char= string.sub(hour,2,-1)

		if string.len(hour) == 2 and first_char == "0" then
			hour = second_char
		else
			hour = string.sub( display_product.running_time, 1, index - 1 )
		end
		kernel.setmacro( "ProductDetail:RunningTime", tostring(hour .. ":" .. minutes) )
	else
		kernel.setmacro( "ProductDetail:RunningTime", tostring(display_product.running_time) )
	end
else
	kernel.setmacro( "ProductDetail:RunningTime", "" )
	kernel.setactorx( "selected_product_CC", kernel.getactorx( "selected_product_national_street_date" ) + 2 )
	kernel.setactorx( "selected_product_national_street_date", kernel.getactorx( "selected_product_running_time" ) )
	kernel.hideactor( "selected_product_running_time" )
end

-- Set closed caption
local closed_captioned = display_product.closed_captioned
if not product_is_case and closed_captioned ~= nil and closed_captioned == true then
	kernel.showactor( "selected_product_CC" )
else
	kernel.hideactor( "selected_product_CC" )
end

-- Set Street date
local national_street_date = display_product.national_street_date
if not product_is_case and national_street_date ~= nil and national_street_date ~= "" then
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

-- Set the image
if product_family.product_family_name == "Movies" then
	source_image_name = "image_movie_missing_box_art"
elseif product_family.product_family_name == "Games" then
	source_image_name = "image_game_missing_box_art"
end
local image_registered = kernel.dataserviceregisterimage(	display_product.image_file, display_product.image_file )

if image_registered then
	source_image_name = string.format("cache:%s", display_product.image_file)
end
kernel.setactorimage("selected_product_image", source_image_name)
kernel.setactortext("selected_product_image", string.format("cache:%s", display_product.image_file))

-- if the product is a game and already in the cart gray out the image
if other_product ~= nil and not product_is_case then
	for k,v in ipairs(ShoppingCart.items) do
		if v.product_id == other_product.product_id then
			kernel.setactoropacity( "selected_product_image", 0.4 )
		end
	end
end

local releaseDate = string.sub(display_product.merchandise_date, 1, 8)
local calculatedSortDate = string.sub(display_product.sort_date, 1, 8)
local days = kernel.getelapseddays( releaseDate, get_current_date_string( "%4d%02d%02d" ) )
local daysByCalculatedSortDate = kernel.getelapseddays( calculatedSortDate, get_current_date_string( "%4d%02d%02d" ) )

local temp_time = os.time() + ( 24*60*60 )
local return_time = os.date( "9pm " .. "%A, %m/%d/%y", ( temp_time ) )
if kernel.getculture() == "es-MX" then
	local english_day = os.date("%A", temp_time)
	local spanish_day = strings[english_day]
	return_time = string.gsub(return_time, english_day, spanish_day)
end

local product_history = ProductHistory[display_product.product_id]

-- Draw the appropriate flag above the box art
show_flag_product_detail( releaseDate, days, daysByCalculatedSortDate )

if product_type_md ~= nil
and product_type_md.detail_banner_resource_name ~= nil then
	kernel.setmacro( "ProductDetail:ProductType", string.upper( tostring( product_type.product_type_name ) ) )
else
	kernel.setmacro( "ProductDetail:ProductType", "" )
end

kernel.setmacro( "ProductDetail:Name", display_product.long_name )

local title_size = kernel.measureactortext("selected_product_title")
local title_label = display_product.long_name
while title_size.lines_filled > 2 do
	title_label = string.sub(title_label, 0, string.len(title_label) - 5).. "..."
	kernel.setactortext("selected_product_title", title_label)
	title_size = kernel.measureactortext("selected_product_title")
end

kernel.setmacro( "ProductDetail:Description", display_product.description )

-- Show starring
if product_family.product_family_name == "Games" or product_is_case then
	kernel.hideactor( "starring_label" )
	kernel.hideactor( "starring" )
	if product_family.product_family_name == "Games" then
		kernel.moveactory( "category_label", -20 )
		kernel.moveactory( "category", -20 )
		kernel.moveactory( "running_time_label", -20 )
		kernel.moveactory( "running_time", -20 )
		kernel.moveactory( "rating_label", -20 )
		kernel.moveactory( "rating", -20 )
		kernel.moveactory( "rating_description", -20 )
	end
else
	if display_product.starring ~= nil then
		local starring_label = ""
		for _, v in ipairs( display_product.starring ) do
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
		kernel.setmacro( "ProductDetail:Starring", starring_label )
	end
end

-- Show Genres (Category)
if product_is_case then
	kernel.hideactor( "category_label" )
	kernel.hideactor( "category" )
else
	local product_genre = nil
	local genre_count = 0
	for i, v in pairs( display_product.genres ) do
		local genres = ProductGenreData[product_family.product_family_name].filter_genres[i]
		if genres ~= nil and product_genre == nil then
			product_genre = genres
			genre_count = genre_count + 1
		elseif genres ~= nil and product_genre ~= nil then
			product_genre = product_genre..", "..genres
		end
	end
	kernel.setmacro( "ProductDetail:Category", product_genre )
end

-- Show pricing
local product_price_test = StoreManager.PricingManager:is_early_released( display_product )

if product_price_test == false
and kernel.get_session_value("BrowseState.BrowseFilterMode") == "ComingSoon"
and display_product.is_available == false
and StoreManager.PricingManager:is_alternate_priced(display_product.product_id) then
	product_price_test = true
end

local GetPricingText = function(product, is_initial_night)
	local result = ""
	if product ~= nil then
		local pricing_value = StoreManager.PricingManager:get_product_price( product.product_id )
		local product_pricing_text = strings.price_a_day
		
		local pricing_product_type = ProductTypes[product.product_type_id]		
		local product_format_text = ""
		if pricing_product_type ~= nil then
			if pricing_product_type.product_type_name == "DVD" then
				product_format_text = strings.dvd
			elseif pricing_product_type.product_type_name == "Blu-ray" then
				product_format_text = strings.blu_ray
			end
		end

		local formatted_pricing_value = nil
		if is_initial_night then
			formatted_pricing_value = string.format( "%s%.2f", strings.currency_symbol, pricing_value.initial_night )
		else
			formatted_pricing_value = string.format( "%s%.2f", strings.currency_symbol, pricing_value.extra_night )
		end
		result =  formatted_pricing_value .. " " .. product_pricing_text .. " " .. product_format_text
	
	end
	return result
end

-- first night pricing
if product_is_case then
	kernel.hideactor( "first_night_price_label" )
	kernel.hideactor( "first_night_price" )
else
	local price_text = ""
	local dvd_price_text = GetPricingText(dvd_product, true)
	local bluray_price_text = GetPricingText(bluray_product, true)
	local other_price_text = GetPricingText(other_product, true)
	if dvd_price_text ~= "" and (dvd_available or not bluray_available) then
		price_text = dvd_price_text
		if bluray_price_text ~= "" and (bluray_available or not dvd_available) then
			price_text = price_text .. ", " .. bluray_price_text
		end
	elseif bluray_price_text ~= "" then
		price_text = bluray_price_text
	else 
		price_text = other_price_text
	end
	kernel.setmacro( "ProductDetail:FirstNightPrice", price_text) 
	if product_price_test then
		kernel.setactortext( "first_night_price_label" , strings.first_day_price )
		if product_family.product_family_name ~= "Movies" then
			kernel.setactory( "first_night_price_label" , 551 )
			kernel.setactory( "first_night_price" , 551 )
		end
	else
		if not product_is_case then
			kernel.setactortext( "first_night_price_label" , strings.price_tag )
			kernel.setactory( "first_night_price_label" , 551 )
			kernel.setactory( "first_night_price" , 551 )
			if kernel.getculture() == "en-US" then
				kernel.setactorx( "first_night_price" , 586 )
			elseif kernel.getculture() == "es-MX" then
				kernel.setactorx( "first_night_price" , 595 )
			end
		end
	end
end

-- extra night pricing
if product_is_case or not product_price_test then
	kernel.hideactor( "extra_night_price_label" )
	kernel.hideactor( "extra_night_price" )
else 	
	local extra_night_price_text = ""
	local extra_night_dvd_price_text = GetPricingText(dvd_product, false)
	local extra_night_bluray_price_text = GetPricingText(bluray_product, false)
	local extra_night_other_price_text = GetPricingText(other_product, false)
	if extra_night_dvd_price_text ~= "" and (dvd_available or not bluray_available) then
		extra_night_price_text = extra_night_dvd_price_text
		if extra_night_bluray_price_text ~= "" and (bluray_available or not dvd_available) then
			extra_night_price_text = extra_night_price_text .. ", " .. extra_night_bluray_price_text
		end
	elseif extra_night_bluray_price_text ~= "" then
		extra_night_price_text = extra_night_bluray_price_text
	else 
		extra_night_price_text = extra_night_other_price_text
	end
	kernel.setmacro( "ProductDetail:ExtraNightPrice", extra_night_price_text )
	if product_price_test then
		kernel.setactortext( "extra_night_price_label" , strings.extra_day_price )
		if product_family.product_family_name ~= "Movies" then
			kernel.setactory( "extra_night_price_label" , 571 )
			kernel.setactory( "extra_night_price" , 571 )
		end
	end
end

-- return time
if product_is_case then
	kernel.hideactor( "return_time_label" )
	kernel.hideactor( "return_time" )
else
	kernel.setmacro( "ProductDetail:ReturnTime", return_time )
	if product_price_test and product_family.product_family_name ~= "Movies" then
		kernel.setactory( "return_time_label" , 591 )
		kernel.setactory( "return_time" , 591 )
		kernel.setactory( "separator_2" , 611 )
	end
end

-- separator_1
if product_is_case then
	kernel.hideactor( "separator_1" )
end

-- Sub-Platform Indicator
if product_family.product_family_name == "Movies" then
	kernel.hideactor( "separator_3" )
	kernel.hideactor( "sub_platform_indicator" )
else
	if display_product.sub_platform_id ~= nil then
		kernel.setactorimage( "sub_platform_indicator", SubPlatformIndicatorData[display_product.sub_platform_id].sub_platform_image )
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

-- define logic for add and buy buttons
local add_product_to_cart_as_buy = function(product_to_buy)
	view_analytics_event( "buttonPress", "buy," .. tostring( product_to_buy.product_id))
	browse_selection_cart_action = "purchase"
	if product_to_buy.product_id ~= 1139 then
		check_for_warning_popups( check_alert_table, product_to_buy, nil )
	else
		add_item_to_shopping_cart( product_to_buy, nil, "purchase" )
	end
end

kernel.registeractorhitnotify(
	"buy_used_product_button",
	function()
		if VerboseLogging then
			kernel.log("Buy button overlay: DVD parameter: " .. tostring(dvd_show_in_purchase_overlay and dvd_product.product_id or nil))
			kernel.log("Buy button overlay: Blu-ray parameter: " .. tostring(bluray_show_in_purchase_overlay and bluray_product.product_id or nil))
		end
		local title_rollup_overlay = newtitlerollupoverlay()
		title_rollup_overlay:show( 384, 226, 275, 384, true,
			dvd_show_in_purchase_overlay and dvd_product or nil, 
			function()
				item_added_from_view = kernel.peekview()
				add_product_to_cart_as_buy(dvd_product)
			end, 
			bluray_show_in_purchase_overlay and bluray_product or nil, 
			function()
				item_added_from_view = kernel.peekview()
				add_product_to_cart_as_buy(bluray_product)
			end, 
			function()
				view_analytics_event( "buttonPress", "buy-cancel-add-item")
			end, 
			nil,
			true )		
	end
)

kernel.registeractorhitnotify(
	"buy_other_product_button",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_buy(display_product)
	end
)

local function add_rental_product(product_to_rent)
	browse_selection_cart_action = "rental"
	check_for_warning_popups( check_alert_table, product_to_rent, nil )
end

local add_product_to_cart_as_rent = function( product_to_rent )
	if from_carousel_collections == true then
		view_analytics_event( "CarouselCollection", "rent," .. tostring( product_to_rent.product_id ) )
	elseif from_carousel == true then
		view_analytics_event( "Carousel", "rent," .. tostring( product_to_rent.product_id ) )
	end
	if browse_collection ~= nil then
		view_analytics_event( browse_collection.product_name, "rent," .. tostring( product_to_rent.product_id ) )
	end

	view_analytics_event( "buttonPress", "rent," .. tostring( product_to_rent.product_id ) )
	
	local message_view_manager = newmessageviewmanager()
	message_view_manager:hide( true )
	if( StoreManager.PricingManager:is_early_released( product_to_rent ) ) then
		message_view_manager:show()
		view_analytics_event( "context", "$3 Firsts warning message" )
		idle_timer:start()
		message_view_manager:hide_title()

		-- The message needs to include the prices from the actual price file (and MD adjustment if needed)
		local product_pricing = StoreManager.PricingManager:get_product_price( product_to_rent.product_id )

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
				add_rental_product(product_to_rent)
			end )
		message_view_manager:show_back_button_arrow(
			function()
				message_view_manager:hide_message()
			end )
	else
		add_rental_product(product_to_rent)
	end
end

kernel.registeractorhitnotify(
	"add_button_dvd",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(dvd_product)
	end
)
kernel.registeractorhitnotify(
	"plus_button_dvd",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(dvd_product)
	end
)

kernel.registeractorhitnotify(
	"add_button_bluray",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(bluray_product)
	end
)
kernel.registeractorhitnotify(
	"plus_button_bluray",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(bluray_product)
	end
)

kernel.registeractorhitnotify(
	"add_button_other_product",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(other_product)
	end
)
kernel.registeractorhitnotify(
	"plus_button_other_product",
	function()
		item_added_from_view = kernel.peekview()
		add_product_to_cart_as_rent(other_product)
	end
)

-- buy dvd or blu-ray button
if ((dvd_product == nil or not dvd_show_buy_button) and (bluray_product == nil or not bluray_show_buy_button)) or 
	shopping_cart_is_full then
	kernel.hideactor( "buy_used_product_button" )
end

-- buy other product button and associated text
if (other_product == nil or (other_product_already_in_cart and not product_is_case) or not other_product_is_purchasable) or 
		shopping_cart_is_full then
	kernel.hideactor( "buy_other_product_button" )
	kernel.hideactor( "buy_other_product_amount_label" )	
else
	-- buy button price 
	local price_purchase_string = ""
	local price_purchase = nil
	if product_is_case then
		price_purchase = case_price_purchase
	elseif dvd_product ~= nil or bluray_product ~= nil then
		price_purchase = movie_price_purchase
	elseif other_product ~= nil then
		price_purchase = game_price_purchase
	end
	if price_purchase ~= nil then
		price_purchase_string = string.format( "%s%.2f", strings.currency_symbol, price_purchase )
	end
	kernel.setmacro( "ProductDetail:PurchasePrice", price_purchase_string )
	kernel.log( "Adjusted purchase price string = " .. tostring( kernel.getmacro( "ProductDetail:PurchasePrice" )))
	kernel.markactordirty( "buy_other_product_amount_label" )
end

-- + DVD button and associated text
if not shopping_cart_is_full and 
		(dvd_already_in_cart 
		or 
		(dvd_available and not dvd_available_and_instock and (bluray_available_and_instock or bluray_already_in_cart))
		) then	
	if dvd_already_in_cart then
		kernel.showactor("dvd_in_cart")
		kernel.showactor("dvd_in_cart2") -- spanish only
	else
		kernel.showactor("dvd_out_of_stock")
		kernel.showactor("dvd_out_of_stock2") -- english only
	end
	kernel.setactortext("add_button_dvd","")
	kernel.disableactor("add_button_dvd")
	kernel.hideactor( "plus_button_dvd" )
elseif not dvd_available_and_instock or shopping_cart_is_full then
	kernel.hideactor( "add_button_dvd" )
	kernel.hideactor( "plus_button_dvd" )
end

-- + Blu-ray button and associated text
if not shopping_cart_is_full and 
		(bluray_already_in_cart 
		or 
		(bluray_available and not bluray_available_and_instock and (dvd_available_and_instock or dvd_already_in_cart))
		) then
	if bluray_already_in_cart then
		kernel.showactor("bluray_in_cart")
		kernel.showactor("bluray_in_cart2") -- spanish only
	else
		kernel.showactor("bluray_out_of_stock")
		kernel.showactor("bluray_out_of_stock2") -- english only
	end
	kernel.setactortext("add_button_bluray","")
	kernel.disableactor("add_button_bluray")
	kernel.hideactor( "plus_button_bluray" )
elseif not bluray_available_and_instock or shopping_cart_is_full then
	kernel.hideactor( "add_button_bluray" )
	kernel.hideactor( "plus_button_bluray" )
end

-- + Add (other product)
if product_is_case or not other_product_available_and_instock or other_product_already_in_cart or shopping_cart_is_full then
	kernel.hideactor( "add_button_other_product" )
	kernel.hideactor( "plus_button_other_product" )
end

-- Coming Soon banner, date and Email me button
if display_product.is_available == false then
	local coming_soon_release_date = StoreManager.PricingManager:get_early_release_date(display_product)
	local release_date = getmonth(string.sub(coming_soon_release_date, 5, 6 )).." " .. string.sub(coming_soon_release_date, 7, 8 )
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
									item_id = tostring( display_product.product_id ),
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
					if ShoppingCart.coming_soon_optin:indexof_optin_item( display_product.product_id ) == 0 then
						table.insert( ShoppingCart.coming_soon_optin.optin_items,
							{
								item_id = tostring( display_product.product_id ),
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
	else 
		kernel.showactor("coming_soon_violator")
		kernel.setactorimage("coming_soon_violator","image_banner_coming_soon")
		kernel.setactortext("coming_soon_violator",strings.coming_label.." "..release_date )
	end
end 

-- Out of stock 
if display_product.is_available == true and not dvd_available_and_instock 
		and not bluray_available_and_instock and not other_product_available_and_instock and
		not dvd_already_in_cart and not bluray_already_in_cart and not other_product_already_in_cart then
	kernel.setactoropacity( "selected_product_image", 0.4 )
	if not dvd_already_in_cart and not bluray_already_in_cart and not other_product_already_in_cart and not product_is_case then
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

        kernel.setmacro( "TTS:OutOfStock", kernel.getactortext( "out_of_stock_line1" ) .. " " .. kernel.getactortext( "out_of_stock_line2" ) .. " " .. kernel.getactortext( "out_of_stock_line3" ) )
	end
end

kernel.registerkeypressnotify(
	"product_detail_view",
	function( key, keyCode, modifier )
		if modifier == "Control" and keyCode == 4 then
			statistics_popup_view_product = display_product
			view_show_safe( "statistics_popup_view" )
		end
	end
)

if #product_list == 1 then
	kernel.hideactor( "left_arrow" )
	kernel.hideactor( "right_arrow" )
end

view_analytics_event( "context", "productDisplayed," .. tostring( display_product.product_id ) )

if kernel.rental_is_collections_enabled() == true and display_product.coming_soon ~= true and display_product.collections ~= nil and #(display_product.collections) > 0 then
	kernel.registeractorhitnotify(
		"collection_button",
		function()
			view_analytics_event( "buttonPress", "see-related-collection" )
			show_collection_items( kernel.rental_get_collection( display_product.collections[1] ) )
		end)
else
	kernel.hideactor("collection_button")
end
