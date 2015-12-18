--
-- start-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.timerstart( "reservation-expire-timer" )

idle_timer:stop()

kernel.initialize_session()

topten_list = {}
local startPoint = 1
max_top_count = 11
min_carousel_items = 8
price_set_item = false
show_alert_message = false
--collections_popup_shown = false;
--browse_paddle_click_count = 0;
marketing_config = kernel.get_marketing_config()
-- idle timer needs to be reinitialized because of the collections popup
initialize_idle_timer()
user_on_flip_view = false
user_on_overlay = false
overlay_scene_manager = newtitlerollupoverlay()
from_carousel_collections = false
from_carousel = false
HasDigitalSignupOffer = false
AcceptedDigitalSignupOffer = false

if ABE_ON == nil then
	ABE_ON = false
	kernel.setfilter( "ada_accessible", "False" )
end

local _ProductTypes = ProductTypes
local _ProductCatalog = ProductCatalog
local _ProductFamilies = ProductFamilies
local _ProductTypesMetaData = ProductTypesMetaData
local _ProductRating = ProductRatings

movie_positions = {}
if movie_products == nil then
	movie_products = {}
end
local last_flipped_actor = ""

MonthMap = {
	["01"] = { ["long_name"] = "January", ["short_name"] = "Jan" },
	["02"] = { ["long_name"] = "February", ["short_name"] = "Feb" },
	["03"] = { ["long_name"] = "March", ["short_name"] = "Mar" },
	["04"] = { ["long_name"] = "April", ["short_name"] = "Apr" },
	["05"] = { ["long_name"] = "May", ["short_name"] = "May" },
	["06"] = { ["long_name"] = "June", ["short_name"] = "Jun" },
	["07"] = { ["long_name"] = "July", ["short_name"] = "Jul" },
	["08"] = { ["long_name"] = "August", ["short_name"] = "Aug" },
	["09"] = { ["long_name"] = "September", ["short_name"] = "Sep" },
	["10"] = { ["long_name"] = "October", ["short_name"] = "Oct" },
	["11"] = { ["long_name"] = "November", ["short_name"] = "Nov" },
	["12"] = { ["long_name"] = "December", ["short_name"] = "Dec" }
}

local product = {}
strings = kernel.getdictionary( "strings" )

check_alert_table = {
	movie_mature_warning_shown = false,
	blu_ray_warning_shown = false,
	mature_game_warning_shown = false,
	sub_platform_warning_shown = false
}

carousel_contains_item = function( item_id, item_type )

	local is_match = function( product_id )
		return product_id == item_id
	end

	local in_product = function( product )
		if is_multi_disc( product.product_id ) == true then
			local product_id_list = ProductGroup[MultiDiscXREF[product.product_id].product_group_id].product_id_list
			for _, v in ipairs( product_id_list ) do
				if is_match( v ) then
					return true
				end
			end
		else
			if is_match( product.product_id ) then
				return true
			end
		end
		return false
	end

	for _, carousel_product in ipairs( topten_list ) do
		if carousel_product.kiosk_product_type == 4 then -- do nothing!
		elseif carousel_product.kiosk_product_type == 3 then --title rollup
			for _,rollup_product in pairs(carousel_product.kiosk_products) do
				if in_product(rollup_product) then
					return true
				end
			end
		else
			if in_product(carousel_product) then
				return true
			end
		end
	end
	return false
end

-- Reset ADA filter each time the start view is displayed
kernel.setfilter( "ada_accessible", "False" )

-- Reload pricing in case the date has changed since last session
if StoreManager ~= nil then
	StoreManager:reload_pricing_data( "_v2" )
	StoreManager:check_product_catalog_for_games_platforms()
end

for k,v in _ProductCatalog:pairs() do
	local product = MasterProductCatalog[v.product_id]
	if product ~= nil then
			local product_catalog = ProductCatalog[v.product_id]
			local current_date = get_current_date_string( "%4d%02d%02d%02d%02d" )
			local release_date = string.sub(product.merchandise_date, 1, 12)

			product_catalog.is_available = current_date > release_date
			or StoreManager.PricingManager:is_early_released( product )

			product_catalog.is_coming_soon = not (release_date <= current_date
			or kernel.getelapseddays( current_date, release_date ) > product.coming_soon_days)

			ProductCatalog[v.product_id] = product_catalog
	end
end

ProductGenreData = {
	["Movies"] = {
		filter_genres = {
			[1093] = strings.lower_genre_1093,
			[1000] = strings.lower_genre_1000,
			[1004] = strings.lower_genre_1004,
			[1005] = strings.lower_genre_1005,
			[1006] = strings.lower_genre_1006,
			[1009] = strings.lower_genre_1009
		}
	},
	["Games"] = {
		filter_genres = {
			[1026] = strings.lower_genre_1026,
			[1094] = strings.lower_genre_1006,
			[1028] = strings.lower_genre_1028,
			[1030] = strings.lower_genre_1030,
			[1034] = strings.lower_genre_1034,
			[1036] = strings.lower_genre_1036
		}
	}
}

ProductGenreData_MX = {
	["Movies"] = {
		filter_genres = {
			[1093] = strings.lower_genre_1093,
			[1000] = strings.lower_genre_1000,
			[1005] = strings.lower_genre_1005,
			[1009] = strings.lower_genre_1009,
			[1004] = strings.lower_genre_1004,
			[1006] = strings.lower_genre_1006
		}
	},
	["Games"] = {
		filter_genres = {
			[1026] = strings.lower_genre_1026,
			[1094] = strings.lower_genre_1006,
			[1028] = strings.lower_genre_1028,
			[1030] = strings.lower_genre_1030,
			[1034] = strings.lower_genre_1034,
			[1036] = strings.lower_genre_1036
		}
	}
}

-- Variables to track the current carousel movement.
carousel_repeat_interval = 3000
carousel_user_repeat_interval = 2000
is_carousel_running = false
carousel_count = 0
idle_timer_duration = 30000

selected_genre = "ALL"
-- N.B. Added per Jim Polubinski for Quick Return integration
--      If nothing was suspended with the companion function, this does nothing.
--      JDP 20090903
if FMAMaintenanceMode == false then
	kernel.hardwaremaintenancemoderesumejobs()
end

if kernel.getlastshownview() == Start_Over_View_Name then
	kernel.applysafeshutdown()
	kernel.trimbitmapcache( 128 )
	local hardware_manager = newhardwaremanager()
	hardware_manager.hardware_event_responders.MaxPutAwayAttemptsExceeded = function( args, jobid )
		AlertManager.send_job_alert(
			"HardwareError",
			"HAL script: check-picker.hs sent message MaxPutAwayAttemptsExceeded",
			"Unable to put an item away after the configured number of slot retries. Placing the kiosk into maintenance mode",
			jobid,
			"mm:MaxPutAwayAttemptsExceeded, 005PMXA"
		)
		if not in_maintenance_mode() then
			kernel.setmacro( "MaintenanceView:ErrorCode", "005PMXA" )
			kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "005PMXA" ] or "red_diagonal_stripes" )
			view_show( "maintenance_view" )
		end
	end
	local jobs = hardware_manager:get_hardware_jobs_by_program(
		{ "check-picker" },
		{ "Stopped", "Completed", "Garbage", "Errored" } )
	if table_count( jobs ) == 0 then
		hardware_manager:check_picker()
	end
end

-- Pre-setting these values back to defaults here saves time
kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
browse_sort_mode = "release-date"
browse_filter_description = nil
browse_filter_genre_selected = nil
browse_filter_alpha_selected = nil
browse_filter_coming_soon_selected = nil
browse_is_cart_full_open = false
shopping_cart_full_message_shown = false
added_from_carousel = false

-- Handles those edge cases where the user does not come from the standard UI path
kernel.log( "CloseAnalyticsOnStartView = " .. tostring( CloseAnalyticsOnStartView ))
if CloseAnalyticsOnStartView == true then
	ShoppingCart:close_analytics( "True" )
	CloseAnalyticsOnStartView = false
end

if language_switch == nil or language_switch == false then
	ShoppingCart = newshoppingcart()
	ShoppingCart:start_new()
	ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
else
	language_switch = false
	kernel.enableactor("espanol_button")
end

-- handle ABE mode.
abe_idle_timer = newidletimer( "abe_idle_timer" )
abe_idle_timer:initialize(
	15000,
	function()
		turn_abe_off()
	end
)

turn_abe_on = function()
	kernel.showactor( "handicap_on_button" )
	kernel.hideactor( "handicap_button" )
	kernel.hideactor( "handicap_button_icon" )
	ABE_ON = true
	kernel.setfilter( "ada_accessible", "True" )
	kernel.showactor( "abe_turned_on_label" )
	kernel.log( "ABE mode turned ON." )

	abe_idle_timer:start()
end

turn_abe_off = function()
	abe_idle_timer:stop()
	kernel.showactor( "handicap_button" )
	kernel.showactor( "handicap_button_icon" )
	kernel.hideactor( "handicap_on_button" )
	ABE_ON = false
	kernel.setfilter( "ada_accessible", "False" )
	kernel.hideactor( "abe_turned_on_label" )
	kernel.log( "ABE mode turned OFF." )
end

toggle_ABE_mode = function()
	if ABE_ON == true  then
		turn_abe_off()
	else
		turn_abe_on()
	end
	reload_carousel( true )
	refresh_start_view()
end

-- set default abe_mode or retain status on a language change.
if ABE_ON then
	turn_abe_on()
else
	turn_abe_off()
end

kernel.registeractorhitnotify(
	"handicap_button",
	function()
		toggle_ABE_mode()
	end
)

kernel.registeractorhitnotify(
	"handicap_button_icon",
	function()
		toggle_ABE_mode()
	end
)

kernel.registeractorhitnotify(
	"handicap_on_button",
	function()
		toggle_ABE_mode()
	end
)

-- Even though these reference actors not yet created, they won't throw errors unless called before view is drawn
browse_pager = newpager()
browse_pager.next_changed = function( enabled )
	if enabled == true then
		kernel.setactorimage( "right_arrow", "right_arrow" )
		kernel.setactorforegroundcolor( "right_arrow", "White" )
		kernel.enableactor( "right_arrow" )
	else
		kernel.setactorimage( "right_arrow", "right_arrow_disabled" )
		kernel.setactorforegroundcolor( "right_arrow", "Gray" )
		kernel.disableactor( "right_arrow" )
	end
end

browse_pager.prev_changed = function( enabled )
	if enabled == true then
		kernel.setactorimage( "left_arrow", "left_arrow" )
		kernel.setactorforegroundcolor( "left_arrow", "White" )
		kernel.enableactor( "left_arrow" )
	else
		kernel.setactorimage( "left_arrow", "left_arrow_disabled" )
		kernel.setactorforegroundcolor( "left_arrow", "Gray" )
		kernel.disableactor( "left_arrow" )
	end
end

browse_filter_family_tracker = ProductFamilies:new_selection_tracker( "Movies" )
ProductCatalog:get_default_family()

selection_grid_scene_manager = newscenemanager()
family_button_scene_manager = newscenemanager()

local add_product_to_cart = function ( product ) 
	browse_selection_cart_action = "rental"
	added_from_carousel = true
	item_added_from_view = kernel.peekview()

	if( StoreManager.PricingManager:is_early_released( product ) ) then
		price_set_item = true
	else
		show_alert_message = true
	end
	
	if product.is_game == true then
		browse_filter_family_tracker.selected_family = "Games"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = nil
		abe_product_family_macro = strings.browse_family_games
	elseif product.is_movie == true then
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = ALL
		abe_product_family_macro = strings.browse_family_movies
	end

	if show_alert_message then
		show_alert_message = false
		if ABE_ON then
			kernel.setfilter( "ada_accessible", "True" )
			kernel.setmacro( "Browse:ProductFamily", abe_product_family_macro )
		end
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "carousel-add-item" )
		--view_analytics_event( "buttonPress", tostring( product_type ) )
		check_for_warning_popups( check_alert_table, product, nil )
	end
end

local flip_overlay = function()
	if user_on_overlay then
		kernel.timerstart( "carousel-timer" )
		user_on_overlay = false
		overlay_scene_manager:hide()
	else
		user_on_overlay = true
		kernel.hideactor("add_to_cart")
		kernel.hideactor("plus_button")
		kernel.timerstop( "carousel-timer" )
		kernel.timerstop( "carousel-user-timer" )

		local center_poster_image = movie_positions[5]
		local x = kernel.getactorx(center_poster_image);
		local y = kernel.getactory(center_poster_image);
		local width = kernel.getactorwidth(center_poster_image);
		local height = kernel.getactorheight(center_poster_image);

		local dvd_product = nil
		local blu_ray_product = nil

		-- Determine what type of kiosk product is being displayed.
		if center_poster_product.kiosk_product_type == 3 then 
			-- TitleRollup
			if center_poster_product.dvd_index ~= nil and center_poster_product.dvd_index > 0 then
				dvd_product = center_poster_product.kiosk_products[center_poster_product.dvd_index]
			end
			if center_poster_product.blu_ray_index ~= nil and center_poster_product.blu_ray_index > 0 then
				blu_ray_product = center_poster_product.kiosk_products[center_poster_product.blu_ray_index]
			end
		else
			-- Title and MDV 
			local product_type = _ProductTypes[center_poster_product.product_type_id]
			local product_family = _ProductFamilies[product_type.product_family_id]
			if product_type.product_type_name == "DVD" then
				dvd_product = center_poster_product
			elseif product_type.product_type_name == "Blu-ray" then
				blu_ray_product = center_poster_product
			end
		end

		local dvd_button_on_hit_function = function()
			--analytic needed
			user_on_overlay = false
			add_product_to_cart(dvd_product)
		end

		local blu_ray_button_on_hit_function = function()
			--analytic needed
			user_on_overlay = false
			add_product_to_cart(blu_ray_product)
		end

		local cancel_button_on_hit_function = function()
			user_on_overlay = false
			kernel.showactor("add_to_cart")
			kernel.showactor("plus_button")
			kernel.timerstart( "carousel-timer" )
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "carousel-cancel-add-item" )
		end

		local overlay_expire_function = function()
			user_on_overlay = false
			kernel.showactor("add_to_cart")
			kernel.showactor("plus_button")
			kernel.timerstart( "carousel-timer" )
		end

		overlay_scene_manager:show( 	
									x, 
									y, 
									width,
									height, 
									false,
									dvd_product, 
									dvd_button_on_hit_function, 
									blu_ray_product, 
									blu_ray_button_on_hit_function, 
									cancel_button_on_hit_function,
									overlay_expire_function,
									false )
	end
end

local flip_cover = function ( actor_name )
	--This should remove the artwork and replace it with the right panel of the product detail, minus buttons and banners
	if kernel.isactorvisible( actor_name ) then
		kernel.timerstop( "carousel-user-timer" )
		user_on_flip_view = true
		last_flipped_actor = actor_name

		kernel.hideactor( actor_name )

		local product = movie_products[actor_name]
		
		-- Format Movie/Game Title
		local title_label = product.long_name
		kernel.setactortext( "selected_product_title", title_label )

		local title_size = kernel.measureactortext( "selected_product_title" )
		while title_size.lines_filled > 2 do
			title_label = string.sub( title_label, 0, string.len( title_label ) - 5 ).. "..."
			kernel.setactortext( "selected_product_title", title_label )
			title_size = kernel.measureactortext( "selected_product_title" )
		end

		-- Format Movie/Game Description
		local description = product.description
		kernel.setactortext( "selected_product_description", description)
		local description_size = kernel.measureactortext( "selected_product_description" )
		while description_size.lines_filled > 5 do
			description = string.sub(description, 0, string.len(description) - 10)
			kernel.setactortext( "selected_product_description", description)
			description_size = kernel.measureactortext( "selected_product_description" )
		end
		if description_size.lines_filled == 5 then
			description = description .. " ..."
		else
			description = description
		end
		kernel.setactortext( "selected_product_description", description )
		
		--** Format attributes specific to Movies and Games
		local product_type = _ProductTypes[product.product_type_id]
		local product_family = _ProductFamilies[product_type.product_family_id]
		local product_type_md = _ProductTypesMetaData[product_type.product_type_id]

		if product_type_md == nil then
			AlertManager.send_alert(
				"ApplicationCrash",
				"Missing Product Type",
				"Product Type '" ..
				product_type.product_type_name ..
				"' (" ..
				product_type.product_type_id ..
				" ) is missing a product_type_md key."
			)
		end

		--Format Movie/Game National Street Date
		local national_street_year = product.national_street_date
		if product_family.product_family_name ~= "Games" then
			if national_street_year ~= nil and national_street_year ~= "" then
				kernel.showactor( "national_street_date" )
				kernel.setactory( "national_street_date", 235 )
				kernel.setactory( "selected_product_description", 270)
				kernel.setmacro( "NationalStreetDate", string.sub( national_street_year, 0, 4 ) )
				local size = kernel.measureactortext( "selected_product_title" )
				if size.lines_filled > 1 then
					kernel.setactory( "national_street_date", 260 )
					kernel.setactory( "selected_product_description", 280 )
				end
			end
		else
			kernel.hideactor( "national_street_date" )
		end

		kernel.setactortext( "selected_product_title", title_label )

		--Format Movie/Game Rating
		local rating = nil
		if product.rating_id ~= nil then
			if product_family.product_family_name == "Games" then
				rating_label = "ESRB Rating: "
				rating =_ProductRating[product.rating_id]
			elseif product_family.product_family_name == "Movies" then
				if product.genres[1017] ~= nil then
					rating_label = "TV Rating: "
					rating = TVRatings[product.rating_id]
				else
					rating_label = "MPAA Rating: "
					rating = Ratings[product.rating_id]
				end
			end
			if rating ~= nil and product_family.product_family_name == "Games" then
				kernel.setactortext( "rating_label", rating_label )
				kernel.setactortext( "rating", "Rated" )
				if rating.rating_description ~= nil
				and rating.rating_description ~= "" then
					kernel.setactortext( "rating_description", "          " ..rating.name.." ".."-".." "..rating.rating_description )
				else
					kernel.setactortext( "rating_description", "          " ..rating.name )
				end
			elseif rating ~= nil then
				kernel.setactortext( "rating_label", rating_label )
				kernel.setactortext( "rating", rating.name )
			else
				kernel.setactortext( "rating_label", "" )
				kernel.setactortext( "rating", "" )
			end
		end

		-- Format Movie/Game Running Time
		if product.running_time ~= nil then
			local index = string.find( tostring( product.running_time ), ":" )
			if index ~= nil then
				local hour = string.sub( product.running_time, 1, index - 1 )
				local minutes = string.sub( product.running_time, index + 1 )

				local first_char = string.sub( hour, 1, 1 )
				local second_char= string.sub( hour, 2, -1 )

				if string.len( hour ) == 2 and first_char == "0" then
					hour = second_char
				else
					hour = string.sub( product.running_time, 1, index - 1 )
				end

				kernel.setactortext( "running_time", tostring( hour .. ":" .. minutes ) )
			else
				kernel.setactortext( "running_time", "" )
			end
		end

		-- Format Movie/Game Starring
		local max_chars_starring = 70
		if kernel.getculture() == "es-MX" then
			max_chars_starring = 40
		end

		local starring_label = ""
		if product.starring ~= nil then
			for _, v in ipairs( product.starring ) do
				if v ~= "" then
					if string.len( starring_label ) + 2 + string.len(v) > max_chars_starring then
					break
					end
				end
				if string.len( starring_label ) > 0 then
					starring_label = starring_label .. ", "
				end
				starring_label = starring_label .. v
			end
		end
		kernel.setactortext( "actors", starring_label)

		local starring_size = kernel.measureactortext( "actors" )
		while starring_size.lines_filled > 2 do
			starring_label = string.sub( starring_label, 0, string.len( starring_label ) - 10 )
			kernel.setactortext( "actors", starring_label )
			starring_size = kernel.measureactortext( "actors" )
		end

		if starring_size.lines_filled == 2 then
			starring_label = starring_label .. "..."
		else
			starring_label = starring_label
		end

		kernel.setactortext( "actors", starring_label )

		-- Format Movie/Game Director
		local directors_label = ""
		if product.directors ~= nil then
			for _, v in ipairs( product.directors ) do
				if v ~= "" then
					if string.len( directors_label ) + 2 + string.len(v) > 70 then
					break
					end
				end
				if string.len( directors_label ) > 0 then
					directors_label = directors_label .. ", "
				end
				directors_label = directors_label .. v
			end
		end

		kernel.setactortext( "director", directors_label )

		local directors_size = kernel.measureactortext( "director" )

		while directors_size.lines_filled > 2 do
			directors_label = string.sub( directors_label, 0, string.len( directors_label ) - 10 )
			kernel.setactortext( "director", directors_label)
			directors_size = kernel.measureactortext( "director" )
		end

		if directors_size.lines_filled == 2 then
			directors_label = directors_label .. "..."
		else
			directors_label = directors_label
		end

		kernel.setactortext( "director", directors_label )

		-- Format Movie/Game Genre
		local product_genre = nil
		local genre_count = 0
		if kernel.getculture() == "en-US" then
			for i,v in pairs( product.genres ) do
				if v == true then
					local genres = ProductGenreData[product_family.product_family_name].filter_genres[i]
					if genres ~= nil and product_genre == nil then
						product_genre = genres
						genre_count = genre_count + 1
					elseif genres ~= nil and product_genre ~= nil then
						product_genre = product_genre..", "..genres
					end
					if genre_count == 1 then break end
				end
			end
		elseif kernel.getculture() == "es-MX" then
			for i,v in pairs( product.genres ) do
				if v == true then
					local genres = ProductGenreData_MX[product_family.product_family_name].filter_genres[i]
					if genres ~= nil and product_genre == nil then
						product_genre = genres
						genre_count = genre_count + 1
					elseif genres ~= nil and product_genre ~= nil then
						product_genre = product_genre..", "..genres
					end
					if genre_count == 1 then break end
				end
			end
		end

		if product_genre ~= nil and string.len( product_genre ) > 25 then
			product_genre = string.sub(product_genre, 0, 25)
			product_genre = product_genre ..".."
			kernel.setactortext( "genre", product_genre )
		else
			kernel.setactortext( "genre", product_genre )
		end

		-- Format Movie/Game Pricing
		local product_pricing = StoreManager.PricingManager:get_product_price( product.product_id )
		local item_price = product_pricing.initial_night

		if item_price ~= nil then
			local price_string = string.format( "%s%.2f", strings.currency_symbol, item_price )
			if StoreManager.PricingManager:is_early_released( product ) == false then
				price_string = price_string .. " " .. strings.price_a_day
			end
			kernel.setactortext( "price", price_string )
		end

		-- Format Movie/Game Return Time
		-- *** Getting return date = (get date + 1 day), format: 9pm Tuesday, mm/dd/yy
		local temp_time = os.time( ) + (24*60*60)
		return_date = os.date( "9pm " .. "%A, %m/%d/%y", temp_time )
		if kernel.getculture() == "es-MX" then
			local english_day = os.date( "%A", temp_time )
			local spanish_day = strings[english_day]
			return_date = string.gsub( return_date, english_day, spanish_day )
		end
		kernel.setactortext( "return", return_date)

		-- Flip Cover attributes for Games:
		if product_family.product_family_name == "Games" then
			kernel.hideactor( "actors_label" )
			kernel.hideactor( "actors" )
			kernel.hideactor( "director_label" )
			kernel.hideactor( "director" )
			kernel.hideactor( "running_time_label" )
			kernel.hideactor( "running_time" )
			kernel.showactor( "rating_description" )
			kernel.setactory( "genre_label", 385 )
			kernel.setactory( "genre", 385 )
			kernel.setactory( "price_label", 410 )
			kernel.setactory( "price", 410 )
			kernel.setactory( "return_label", 435 )
			kernel.setactory( "return", 435 )
			kernel.setactory( "rating_label", 455 )
			kernel.setactory( "rating", 455 )
			kernel.setactory( "rating_description", 455 )
		else
			kernel.showactor( "actors_label" )
			kernel.showactor( "actors" )
			kernel.showactor( "director_label" )
			kernel.showactor( "director" )
			kernel.showactor( "running_time_label" )
			kernel.showactor( "running_time" )
			kernel.hideactor( "rating_description" )
			kernel.setactory( "genre_label", 385 )
			kernel.setactory( "genre", 385 )
			kernel.setactory( "actors_label", 410 )
			kernel.setactory( "actors", 410 )
			kernel.setactory( "director_label", 440 )
			kernel.setactory( "director", 440 )
			kernel.setactory( "running_time_label", 460 )
			kernel.setactory( "running_time", 460 )
			kernel.setactory( "price_label", 480 )
			kernel.setactory( "price", 480 )
			kernel.setactory( "return_label", 500 )
			kernel.setactory( "return", 500 )
			kernel.setactory( "rating_label", 520 )
			kernel.setactory( "rating", 520 )
		end

		kernel.hideactor( "add_to_cart" )
		kernel.hideactor( "plus_button" )
		kernel.showactor( "movie_flip" )
		kernel.showactor( "movie_flip_cancel" )
		kernel.showactor( "selected_product_title" )
		kernel.showactor( "genre_label" )
		kernel.showactor( "genre" )
		kernel.showactor( "selected_product_description" )
		kernel.showactor( "price_label" )
		kernel.setactortext( "price_label", strings.price_tag .. " " )
		kernel.showactor( "price" )
		kernel.setactorx( "price", 505 )
		kernel.showactor( "return_label" )
		kernel.showactor( "return" )
		kernel.showactor( "rating_label" )
		kernel.showactor( "rating" )
		kernel.showactor( "separator_1" )
		kernel.showactor( "separator_2" )

		if StoreManager.PricingManager:is_early_released( product ) == true then
			local extra_night_price = product_pricing.extra_night
			if extra_night_price ~= nil then
				kernel.setactortext( "price_label", strings.first_day_price .. " " )
				if kernel.getculture() == "es-MX" then
					kernel.setactorx( "price", 565 )
					kernel.setactorx( "price_extra", 565 )
				else
					kernel.setactorx( "price", 555 )
					kernel.setactorx( "price_extra", 555 )
				end

				kernel.showactor( "price_extra_label" )
				kernel.setactory( "price_extra_label", 497 )
				kernel.showactor( "price_extra" )
				kernel.setactory( "price_extra", 497 )
				local price_extra = string.format( "%s%.2f", strings.currency_symbol, extra_night_price )
				price_extra = price_extra .. " " .. strings.price_a_day
				kernel.setactortext( "price_extra", price_extra )

				kernel.setactory( "return_label", 514 )
				kernel.setactory( "return", 514 )

				kernel.setactory( "rating_label", 530 )
				kernel.setactory( "rating", 530 )
			end
		end
		--** End Formatting attributes specific to Movies and Games
	else
		kernel.showactor( "add_to_cart" )
		kernel.showactor( "plus_button" )
		kernel.hideactor( "movie_flip" )
		kernel.hideactor( "movie_flip_cancel" )
		kernel.hideactor( "running_time_label" )
		kernel.hideactor( "running_time" )
		kernel.hideactor( "selected_product_title" )
		kernel.hideactor( "national_street_date" )
		kernel.hideactor( "actors_label" )
		kernel.hideactor( "actors" )
		kernel.hideactor( "genre_label" )
		kernel.hideactor( "genre" )
		kernel.hideactor( "director_label" )
		kernel.hideactor( "director" )
		kernel.hideactor( "selected_product_description" )
		kernel.hideactor( "price_label" )
		kernel.hideactor( "price" )
		kernel.hideactor( "price_extra_label" )
		kernel.hideactor( "price_extra" )
		kernel.hideactor( "return_label" )
		kernel.hideactor( "return" )
		kernel.hideactor( "rating_label" )
		kernel.hideactor( "rating" )
		kernel.hideactor( "rating_description" )
		kernel.hideactor( "separator_1" )
		kernel.hideactor( "separator_2" )
		kernel.hideactor( "categories_label" )
		kernel.hideactor( "categories" )
		kernel.hideactor( "dates_label" )
		kernel.hideactor( "dates" )
		kernel.showactor( last_flipped_actor )
		kernel.timerstart( "carousel-user-timer" )
		user_on_flip_view = false
	end
end

start_idle_timer = newidletimer( "start-idle-timer" )
start_idle_timer:initialize(
		idle_timer_duration,
		function()
			if kernel.isactorvisible( "movie_flip" ) then
				kernel.hideactor( "movie_flip" )
				kernel.hideactor( "movie_flip_cancel" )
				kernel.hideactor( "running_time_label" )
				kernel.hideactor( "running_time" )
				kernel.hideactor( "selected_product_title" )
				kernel.hideactor( "national_street_date" )
				kernel.hideactor( "actors_label" )
				kernel.hideactor( "actors" )
				kernel.hideactor( "genre_label" )
				kernel.hideactor( "genre" )
				kernel.hideactor( "director_label" )
				kernel.hideactor( "director" )
				kernel.hideactor( "selected_product_description" )
				kernel.hideactor( "price_label" )
				kernel.hideactor( "price" )
				kernel.hideactor( "price_extra_label" )
				kernel.hideactor( "price_extra" )
				kernel.hideactor( "return_label" )
				kernel.hideactor( "return" )
				kernel.hideactor( "rating_label" )
				kernel.hideactor( "rating" )
				kernel.hideactor( "rating_description" )
				kernel.hideactor( "separator_1" )
				kernel.hideactor( "separator_2" )
				kernel.hideactor( "categories_label" )
				kernel.hideactor( "categories" )
				kernel.hideactor( "dates_label" )
				kernel.hideactor( "dates" )
				kernel.showactor( last_flipped_actor )
			end
			if kernel.peekview() == "start_view" then
				kernel.timerstop( "carousel-user-timer" )

				-- Now that analytics are being tracked on the carousel, the session has to be restarted on idle timeout
				if ShoppingCart ~= nil
				and kernel.analyticssessionexists( ShoppingCart.session_id ) == true then
					ShoppingCart:close_analytics( "True" )
					CloseAnalyticsOnStartView = false
					ShoppingCart:abandon( "Start View User Idle Timeout" )
					ShoppingCart = newshoppingcart()
					ShoppingCart:start_new()
					ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
				end

				next_user_motion = nil
				kernel.log( "Current Culture: ".. kernel.getculture().. " Default Culture: ".. DefaultCulture )
				if kernel.getculture() ~= DefaultCulture then
					localization_toggle_culture()
				end
				kernel.timerstart( "carousel-timer" )
			end
			start_idle_timer:reset()
		end )
start_idle_timer:start()

function GetUsedMemory()
	collectgarbage( "collect" )
end

-- timer for repeating carousel movement after every carousel_repeat_interval milli seconds
kernel.timercreate(
	"carousel-timer",
	carousel_repeat_interval,
	carousel_repeat_interval,
	function()
		kernel.timerstop( "carousel-user-timer" )
		next_motion = nil
		next_user_motion = nil
		move_direction( "right" )
	end
)

-- timer for repeating carousel movement after every carousel_user_repeat_interval
kernel.timercreate(
	"carousel-user-timer",
	carousel_user_repeat_interval,
	carousel_user_repeat_interval,
	function()
		kernel.timerstop( "carousel-timer" )
		if next_user_motion ~= nil then
			next_user_motion()
		else
			move_direction( "right" )
		end
	end
)

local function arrange_buttons_based_on_games_availability()
	local do_not_show_games_button = ( DisableGames or ( ProductCatalog:in_stock_games_count() < 1 ) )
	
	if do_not_show_games_button then
		kernel.hideactor( "games_button" )
		kernel.hideactor( "game_button_icon" )
		kernel.hideactor( "games_button_rent_label" )
		kernel.hideactor( "games_button_rent_label_2" )
		kernel.hideactor( "games_button_games_label" )
		kernel.setmacro( "StartView:AllMovies", strings.Alt_StartView_Movies )
	else
		kernel.showactor( "games_button" )
		kernel.showactor( "game_button_icon" )
		kernel.showactor( "games_button_rent_label" )
		kernel.showactor( "games_button_rent_label_2" )
		kernel.showactor( "games_button_games_label" )
		kernel.setmacro( "StartView:AllMovies", strings.Alt_StartView_Movies )
	end
end

local function on_banner_click(banner, value)
	view_analytics_start( Start_Over_View_Name, "BrowseVend" )
	view_analytics_event( "buttonPress", banner )

	if banner == "bluray_banner" then
		bluray_banner_pressed = true
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Blu-ray")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Blu-ray" )
		kernel.executescript( "browse_products" )
	elseif banner == "game_banner" then
		browse_filter_family_tracker.selected_family = "Games"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = nil			
		kernel.executescript( "browse_products" )
	elseif banner == "collections_banner" then
		if value ~= nil then
			local collection = nil
			if value ~= "ALL" then
				collection = kernel.rental_get_collection(value)
				if collection == nil then
					kernel.log("collection banner click: invalid collection id " .. tostring(value) .. ".  Showing all collections instead.")
				end
			end
			-- show the regular browse view first to properly initialize stuff
			view_show( "browse_view" )
			if collection ~= nil then
				show_collection_items(collection)
			else
				show_browse_view_collections()
			end
		end
	end
end

local function show_burn_in_view()
	showing_burn_in_view = true
	kernel.hideactor( "redbox_logo" )
	kernel.hideactor( "help_button" )
	kernel.hideactor( "espanol_button" )
	kernel.hideactor( "all_movies_button" )
	kernel.hideactor( "games_button" )
	kernel.hideactor( "game_button_icon" )
	kernel.hideactor( "movie_button_icon" )
	kernel.hideactor( "pickup_button_icon" )
	kernel.hideactor( "handicap_button_icon" )
	kernel.hideactor( "handicap_on_button" )
	kernel.hideactor( "abe_turned_on_label" )
	kernel.hideactor( "handicap_button" )
	kernel.hideactor( "return_button_icon" )
	kernel.hideactor( "navigation_side_bar" )
	kernel.hideactor( "return_button" )
	kernel.hideactor( "online_pickup_button" )
	kernel.hideactor( "online_red_box_label" )
	kernel.hideactor( "online_pickup_label" )
	kernel.hideactor( "logo_label" )
	kernel.hideactor( "back_ground_image" )
	kernel.hideactor( "movie_button_rent_label" )
	kernel.hideactor( "movie_button_movies_label" )
	kernel.hideactor( "games_button_rent_label" )
	kernel.hideactor( "games_button_rent_label_2" )
	kernel.hideactor( "games_button_games_label" )
	kernel.draw_start_view_banner(on_banner_click, true)
	kernel.showactor( "burn_in_view_label" )
	kernel.setactortext( "burn_in_view_label", strings.burn_in_view_label)
	kernel.timerstart( "burn-in-view-duration-timer" )
	burn_in_timer:stop()
end

local function show_normal_view()
	if (showing_burn_in_view ~= nil
	and showing_burn_in_view == true) then
		arrange_buttons_based_on_games_availability()
		kernel.showactor( "redbox_logo" )
		kernel.showactor( "help_button" )
		kernel.showactor( "espanol_button" )
		kernel.showactor( "all_movies_button" )
		kernel.showactor( "return_button" )
		kernel.showactor( "online_pickup_button" )
		kernel.showactor( "online_red_box_label" )
		kernel.showactor( "online_pickup_label" )
		kernel.showactor( "logo_label" )
		kernel.showactor( "back_ground_image" )
		if ABE_ON == true  then
			kernel.showactor( "abe_turned_on_label" )
			kernel.showactor( "handicap_on_button" )
		else
			kernel.showactor( "handicap_button" )
			kernel.showactor( "handicap_button_icon" )
		end
		kernel.showactor( "movie_button_icon" )
		kernel.showactor( "pickup_button_icon" )
		kernel.showactor( "return_button_icon" )
		kernel.showactor( "navigation_side_bar" )
		kernel.showactor( "movie_button_rent_label" )
		kernel.showactor( "movie_button_movies_label" )
		kernel.draw_start_view_banner(on_banner_click, false)
		kernel.hideactor( "burn_in_view_label" )
		kernel.timerstop( "burn-in-view-duration-timer" )
		burn_in_timer:start()
		showing_burn_in_view = false
	end
end

burn_in_timer = newidletimer( "burn-in-timer" )
burn_in_timer:initialize(
	BurnInTimeout,
	function()
		show_burn_in_view()
	end
)
burn_in_timer:start()

kernel.timercreate(
	"burn-in-view-duration-timer",
	BurnInViewDuration,
	BurnInViewDuration,
	function()
		show_normal_view()
	end
)

kernel.registeractorhitnotify(
	"burn_in_view_label",
	function()
		show_normal_view()
	end
)

function DefineTween()
	kernel.log( "*-- Carousel DefineTween" )

	Carousel_Direction = "left"

	xPositions = {-300, -300, -53, 186, 425, 734, 973, 1101, 1101, 1101, 1101 }
	yPositions = { 269,  269, 269, 269, 200, 269, 269,  269,  269,  269, 269 }
	oValues = { 0.00, 0.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.00, 0.00, 0.00, 0.00 }
	wValues = { 206, 206, 206,206, 276, 206, 206, 206, 206, 206, 206 }
	hValues = { 284, 284, 284, 284, 400, 284, 284, 284, 284, 284, 284 }

	function move_poster( inst )
		kernel.setactorx( inst.actor_name, inst.x )
		kernel.setactory( inst.actor_name, inst.y )
		kernel.setactoropacity( inst.actor_name, inst.opacity )
		kernel.setactorwidth( inst.actor_name, inst.width )
		kernel.setactorheight( inst.actor_name, inst.height )
	end

	function CalculateTweenValues( i, v )
		local actor_to_move = movie_positions[ i ]
		local next_pos = iif( Carousel_Direction == "left", i+1, i-1 )
		if next_pos == 0 then next_pos = #topten_list end
		if next_pos == #topten_list + 1 then next_pos = 1 end

		local new_x = xPositions[i] + ( ( ( xPositions[next_pos] - xPositions[i] ) / 10 ) * v )
		local new_y = yPositions[i] + ( ( ( yPositions[next_pos] - yPositions[i] ) / 10 ) * v )
		local new_opacity = oValues[i] + ( ( ( oValues[next_pos] - oValues[i] ) / 10 ) * v )
		local new_width = wValues[i] + ( ( ( wValues[next_pos] - wValues[i] ) / 10 ) * v )
		local new_height = hValues[i] + ( ( ( hValues[next_pos] - hValues[i] ) / 10 ) * v )

		-- Exception to keep last one from looping around the others, just jump it to its future location
		if next_pos == #topten_list or next_pos == 1 then
			new_x = xPositions[next_pos]
		end

		move_instructions = {
			actor_name = actor_to_move,
			x = new_x,
			y = new_y,
			opacity = new_opacity,
			width = new_width,
			height = new_height
		}
		return move_instructions
	end

	function Carousel_Move( n, t, v )
		for i = 1, #topten_list do
			local inst = CalculateTweenValues( i, v )
			move_poster( inst )
		end
	end

	function Actor_Step()
		local new_movie_positions = {}
		local step_incr = iif( Carousel_Direction == "left", -1, 1 )
		for i = 1, #topten_list do
			local old_pos = i + step_incr
			if old_pos == 0 then old_pos = #topten_list end
			if old_pos == #topten_list + 1 then old_pos = 1 end
			new_movie_positions[i] = movie_positions[ old_pos ]
		end
		movie_positions = new_movie_positions
	end

	function Carousel_Stop()
		--kernel.log( "*----------- Carousel_Stop" )
		Actor_Step()
		center_poster_product = movie_products[movie_positions[5]]
		kernel.setbordercolor("add_to_cart", {["r"] = 134, ["g"] = 12,["b"] = 19} )
		if kernel.getculture() == "es-MX" then
			kernel.setactortext( "add_to_cart", "     iAlquilelo Ahora!" )
		else
			kernel.setactortext( "add_to_cart", "    RENT IT NOW!" )
		end
		if center_poster_product.kiosk_product_type == 4 then
			kernel.setactortext( "add_to_cart", "    " .. strings.carousel_add_item_to_cart_button_for_collection )
		end
		if topten_list ~= nil and #topten_list >= min_carousel_items then
			kernel.showactor( "add_to_cart" )
			kernel.showactor( "plus_button" )
			kernel.showactor( "movie_3_position" )
			kernel.showactor( "movie_4_position" )
			kernel.showactor( "movie_5_position" )
			kernel.showactor( "movie_6_position" )
			kernel.showactor( "movie_7_position" )
			kernel.timerstart( "carousel-user-timer" )
		end
		GetUsedMemory()
	end

	kernel.tweencreate(
		"move-carousel",
		"Linear",
		0,
		10,
		10,
		Carousel_Move,
		Carousel_Stop
	)
end

function move_direction( direction )
	if kernel.peekview() ~= "start_view" then
		return
	end

	if topten_list == nil or #topten_list < min_carousel_items then
		return
	end
	
	if user_on_overlay then
		flip_overlay()
	end
	if kernel.isactorvisible( last_flipped_actor ) == false then
		flip_cover( last_flipped_actor )
	end
	
	kernel.hideactor( "add_to_cart" )
	kernel.hideactor( "plus_button" )
	kernel.hideactor( "movie_3_position" )
	kernel.hideactor( "movie_4_position" )
	kernel.hideactor( "movie_5_position" )
	kernel.hideactor( "movie_6_position" )
	kernel.hideactor( "movie_7_position" )

	Carousel_Direction = direction
	kernel.tweenstart( "move-carousel" )
end

get_product_family = function( product_type_id )
	local _ProductTypes = ProductTypes
	local _ProductFamilies = ProductFamilies
	local product_type
	local product_family

	product_type = _ProductTypes[product_type_id]
	return _ProductFamilies[product_type.product_family_id]
end

local function add_item_tocart()
	local _ProductTypes = ProductTypes
	local _ProductFamilies = ProductFamilies
	local product_type
	local product_family

	if selected_movie ~= nil then
		product_type = _ProductTypes[selected_movie.product_type_id]
	end
	if  selected_movie ~= nil
	and product_type ~= nil then
		product_family = _ProductFamilies[product_type.product_family_id]
	end

	if product_family.product_family_name == "Games" then
		browse_filter_family_tracker.selected_family = "Games"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = nil
		abe_product_family_macro = strings.browse_family_games
	elseif product_family.product_family_name == "Movies" then
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = ALL
		abe_product_family_macro = strings.browse_family_movies
	end
	if show_alert_message then
		show_alert_message = false
		if ABE_ON then
			kernel.setfilter( "ada_accessible", "True" )
			kernel.setmacro( "Browse:ProductFamily", abe_product_family_macro )
		end
		check_for_warning_popups( check_alert_table, selected_movie, i )
	end
end

local function add_movie()
	selected_movie = movie_products[movie_positions[5]]
	browse_selection_cart_action = "rental"
	if( StoreManager.PricingManager:is_early_released( selected_movie ) ) then
		price_set_item = true
		add_item_tocart()
	else
		show_alert_message = true
		add_item_tocart()
	end
end

go_to_collection_browse = function()
	from_carousel_collections = true
	-- show the regular browse view first to properly initialize stuff
	view_show( "browse_view" )
	show_browse_view_collections()
end

handle_center_poster_hit = function()
	kernel.timerstop( "carousel-timer" )
	kernel.timerstop( "carousel-user-timer" )
	browse_selection_selected_product = center_poster_product --used by product detail page

	product_family = center_poster_product.product_type_id ~= nil and get_product_family( center_poster_product.product_type_id ) or { product_family_name = "Movies" }
	
	view_analytics_start( Start_Over_View_Name, "BrowseVend" )
	view_analytics_event( "buttonPress", "carousel_art_button" )
	kernel.timerremove( "carousel-timer" )
	kernel.timerremove( "carousel-user-timer" )
	kernel.timerremove( "carousel-wait" )
	kernel.tweenstop( "move-carousel" )

	browse_filter_family_tracker.selected_family = product_family.product_family_name
	browse_filter_family_tracker:set_family_type( "ALL" )
	
	if ABE_ON then
		kernel.setfilter( "ada_accessible", "True" )
		kernel.setmacro( "Browse:ProductFamily", abe_product_family_string )
	end

	local abe_product_family_string = nil
	local browsing_family_string = nil

	if center_poster_product ~= nil and center_poster_product.kiosk_product_type == 4 then
		go_to_collection_browse()
		if center_poster_product.id > 0 then
			show_collection_items( center_poster_product )
		end
		return
	else
		if product_family.product_family_name == "Movies" then
			browse_filter_genre_selected = ALL
			abe_product_family_string = strings.browse_family_movies
			browsing_family_string = strings.Movies
		elseif product_family.product_family_name == "Games" then
			browse_filter_genre_selected = nil
			abe_product_family_string = strings.browse_family_games
			browsing_family_string = strings.Games
		end
	end
	
	kernel.setmacro( "TTS:BrowsingFamily", strings.Games )
	kernel.executescript( "browse_products" )
	product_detail_shown_from_view = kernel.peekview()
	from_carousel = true
	view_show( "product_detail_view" )
end

handle_add_to_cart_hit = function()
	
	product_family = center_poster_product.product_type_id ~= nil and get_product_family( center_poster_product.product_type_id ) or { product_family_name = "Movies" }
	
	if center_poster_product ~= nil and center_poster_product.kiosk_product_type == 4 then
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "add_to_cart" )
		go_to_collection_browse()
		if center_poster_product.id > 0 then
			show_collection_items( center_poster_product )
		end
	elseif product_family.product_family_name == "Movies" then
		flip_overlay()
	elseif product_family.product_family_name == "Games" then
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "add_to_cart" )
		kernel.timerstop( "carousel-timer" )
		kernel.timerstop( "carousel-user-timer" )
		added_from_carousel = true
		item_added_from_view = kernel.peekview()
		add_movie()
	end
end

go_to_movies_browse_view = function()
	view_analytics_start( Start_Over_View_Name, "BrowseVend" )
	view_analytics_event( "buttonPress", "all_movies_button" )
	kernel.timerremove( "carousel-timer" )
	kernel.timerremove( "carousel-user-timer" )
	kernel.timerremove( "carousel-wait" )
	browse_filter_family_tracker.selected_family = "Movies"
	browse_filter_family_tracker:set_family_type( "ALL" )
	browse_filter_genre_selected = ALL
	kernel.tweenstop( "move-carousel" )
	if ABE_ON then
		kernel.setfilter( "ada_accessible", "True" )
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
	end
	kernel.setmacro( "TTS:BrowsingFamily", strings.Movies )
	kernel.executescript( "browse_products" )
end


go_to_games_view = function()
	view_analytics_start( Start_Over_View_Name, "BrowseVend" )
	view_analytics_event( "buttonPress", "games_button" )
	kernel.timerremove( "carousel-timer" )
	kernel.timerremove( "carousel-user-timer" )
	kernel.timerremove( "carousel-wait" )
	browse_filter_family_tracker.selected_family = "Games"
	browse_filter_family_tracker:set_family_type( "ALL" )
	browse_filter_genre_selected = nil
	kernel.tweenstop( "move-carousel" )
	if ABE_ON then
		kernel.setfilter( "ada_accessible", "True" )
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_games )
	end
	kernel.setmacro( "TTS:BrowsingFamily", strings.Games )
end


go_to_games_browse_view = function()
	go_to_games_view()
	kernel.executescript( "browse_products" )
end


go_to_games_platform_selection_view = function()
	go_to_games_view()
	kernel.executescript( "select_platform" )
end

--function to get the image_type_id from type
function get_image_type_id( type )
	for k, v in ImageTypes:pairs() do
		if v.Value == type then return k end
	end
	return nil
end

-- function to load the top 10 movie posters to their initial positions
function show_images( difference )
	movie_positions = {}
	
	table.insert( movie_positions, 1, "stvw_movie_" .. tostring(#topten_list - 1) )
	table.insert( movie_positions, 2, "stvw_movie_" .. tostring(#topten_list) )

	for k = 1, #topten_list - 2 do
		local val = k + 2
		table.insert( movie_positions, val, "stvw_movie_" .. tostring(k) )
	end

	last_flipped_actor = "stvw_movie_3"

	kernel.registeractorhitnotify(
		"movie_3_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_3_position" )
			if movie_products[movie_positions[3]].image_file == "image_message_poster_background" then
				go_to_movies_browse_view()
			else
				kernel.timerstop( "carousel-timer" )
				next_motion = function() move_direction( "left" ) end
				next_user_motion = function() move_direction( "left" ) end
				move_direction( "left" )
			end
		end
	)

	kernel.registeractorhitnotify(
		"movie_4_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_4_position" )
			if movie_products[movie_positions[4]].image_file == "image_message_poster_background" then
				go_to_movies_browse_view()
			else
				kernel.timerstop( "carousel-timer" )
				next_motion = nil
				next_user_motion = function() move_direction( "left" ) end
				move_direction( "left" )
			end
		end
	)

	kernel.registeractorhitnotify(
		"movie_5_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_5_position" )
			if movie_products[movie_positions[5]].image_file == "image_message_poster_background" then
				go_to_movies_browse_view()
			else
				kernel.timerstop( "carousel-timer" )
				kernel.timerstop( "carousel-user-timer" )
				handle_center_poster_hit()
			end
		end
	)
	
	kernel.registeractorhitnotify(
		"movie_flip",
		function()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "poster_flip" )
			flip_cover( last_flipped_actor )
		end
	)

	kernel.registeractorhitnotify(
		"movie_flip_cancel",
		function()
			view_analytics_event( "buttonPress", "poster_flip_cancel" )
			flip_cover( last_flipped_actor )
		end
	)

	kernel.registeractorhitnotify(
		"movie_6_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_6_position" )
			if movie_products[movie_positions[6]].image_file == "image_message_poster_background" then
				go_to_movies_browse_view()
			else
				kernel.timerstop( "carousel-timer" )
				next_motion = nil
				next_user_motion = function() move_direction( "right" ) end
				move_direction( "right" )
			end
		end
	)

	kernel.registeractorhitnotify(
		"movie_7_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_7_position" )
			if movie_products[movie_positions[7]].image_file == "image_message_poster_background" then
				go_to_movies_browse_view()
			else
				kernel.timerstop( "carousel-timer" )
				next_motion = function() move_direction( "right" ) end
				next_user_motion = function() move_direction( "right" ) end
				move_direction( "right" )
			end
		end
	)

	local imageCount = 1

	startPoint = startPoint + difference

	--To make the list circular
	if startPoint < 1 then
		startPoint = #topten_list
	end

	if startPoint > #topten_list then
		startPoint = 1
	end

	list_position = startPoint
	movie_products = {}

	while imageCount <= #topten_list do
		product = {}
		
		product = topten_list[list_position]
		
		local product_type = nil
		movie_products["stvw_movie_"..imageCount] = product
		if product ~= nil then
			product_type = _ProductTypes[product.product_type_id]
			if product ~= nil
			and product.kiosk_product_type == 4 then
				kernel.setactorimage( "stvw_movie_".. imageCount, product.image_file )
			elseif  product ~= nil
			and product_type ~= nil then
				local product_family = _ProductFamilies[product_type.product_family_id]
				-- add in cache state variables...
				local cache = _ProductCatalog[product.product_id]
				if cache ~= nil then
					product.can_be_sold = cache.can_be_sold == nil and false or cache.can_be_sold
					--product.is_available = cache.is_available == nil and false or cache.is_available
					product.is_available = cache.is_available or false
				end
				local source_image_name = "missing_box_art-thumbnail"
				if product_family.product_family_name == "Movies" then
					source_image_name = "image_movie_missing_box_art"
					kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
				elseif product_family.product_family_name == "Games" then
					source_image_name = "image_game_missing_box_art"
					kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
				end
				
				local image_registered = kernel.dataserviceregisterimage( product.image_file, product.image_file )

				if image_registered then
					source_image_name = string.format( "cache:%s", product.image_file )
					kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
				end
			end
			kernel.setactortext( "stvw_movie_".. imageCount, "" )
		else
			kernel.hideactor( "stvw_movie_".. imageCount )
		end

		imageCount = imageCount + 1

		list_position = list_position + 1
		if list_position > #topten_list then
			list_position = 1
		end
	end
	center_poster_product = movie_products[movie_positions[5]]
end

local function load()
	kernel.rental_refresh_if_needed()
	topten_list = kernel.rental_get_carousel_products( kernel.rental_is_collections_enabled() == true )
end

load()

function refresh_start_view()
	if topten_list ~= nil and #topten_list >= min_carousel_items then
		show_images(0)
		kernel.hideactor( "loading_titles_label" )
		kernel.setactorimage( "back_ground_image", "image_kiosk_2_carousel_background" )
	else
		kernel.hideactor( "stvw_movie_1" )
		kernel.hideactor( "stvw_movie_2" )
		kernel.hideactor( "stvw_movie_3" )
		kernel.hideactor( "stvw_movie_4" )
		kernel.hideactor( "stvw_movie_5" )
		kernel.hideactor( "stvw_movie_6" )
		kernel.hideactor( "stvw_movie_7" )
		kernel.hideactor( "stvw_movie_8" )
		kernel.hideactor( "stvw_movie_9" )
		kernel.hideactor( "stvw_movie_10" )
		kernel.hideactor( "stvw_movie_11" )
		kernel.hideactor( "add_to_cart" )
		kernel.hideactor( "plus_button" )
		kernel.hideactor( "movie_3_position" )
		kernel.hideactor( "movie_4_position" )
		kernel.hideactor( "movie_5_position" )
		kernel.hideactor( "movie_6_position" )
		kernel.hideactor( "movie_7_position" )
		--kernel.showactor( "loading_titles_label" )
		kernel.setactorimage( "back_ground_image", "image_loading_titles_alt_carousel")
	end
end

function reload_carousel( force )
	if topten_list ~= nil and #topten_list ~= max_top_count or force == true then
		load()
		if topten_list ~= nil and #topten_list >= min_carousel_items then
			view_goback_safe( "reload carousel" )
		else
			kernel.log( "There are no sufficient movies to Display the carousel. Skipping Start-View refresh." )
		end
	end
end

refresh_start_view()
if Carousel_Direction == nil then
	DefineTween() -- This only needs to be loaded once
end
kernel.timerstart( "carousel-timer" )

kernel.registeractorhitnotify(
	"add_to_cart",
	function()
		handle_add_to_cart_hit()
	end
)

kernel.registeractorhitnotify(
	"plus_button",
	function()
		handle_add_to_cart_hit()
	end
)

kernel.registeractorhitnotify(
	"games_button",
	function()
		if EnablePlatformScreen == true and ABE_ON == false then
			go_to_games_platform_selection_view()
		else
			go_to_games_browse_view()
		end
	end
)

kernel.registeractorhitnotify(
	"all_movies_button",
	function()
		go_to_movies_browse_view()
	end
)

arrange_buttons_based_on_games_availability()
kernel.draw_start_view_banner(on_banner_click, false)