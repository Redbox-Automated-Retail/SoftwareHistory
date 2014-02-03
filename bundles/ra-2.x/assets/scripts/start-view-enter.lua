--
-- start-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if SellTickets == true and kernel.ticketsvcs_refreshifneeded() == true then
	kernel.log( "|Tickets| TicketsRefreshIfNeeded() returned true, reloading tickets data" )
	create_tickets_cache_tables()
end

kernel.timerstart( "reservation-expire-timer" )

idle_timer:stop()
topten_list = {}
ticketsforcarousel = {}
local startPoint = 1
max_top_count = 10
price_set_item = false
show_alert_message = false
user_on_flip_view = false

HasDigitalSignupOffer = false
AcceptedDigitalSignupOffer = false

if SellTickets == true then
	tickets_selection_grid_scene_manager = nil
	tickets_browse_pager = nil
	tickets_browse_filter_selected = nil
	tickets_browse_sort_filter_selected = nil
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
selected_movies_list = {}
strings = kernel.getdictionary( "strings" )

check_alert_table = {
	movie_mature_warning_shown = false,
	blu_ray_warning_shown = false,
	mature_game_warning_shown = false,
	sub_platform_warning_shown = false
}

carousel_contains_item = function( item_id, item_type )
	for k, v in ipairs( topten_list ) do
		if item_type == "TICKET" then
			if v.event ~= nil and v.event.event_id == item_id then
				return true
			end
		else
			if v.product_id == item_id then
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
			local release_date = string.sub(product.release_date, 1, 12)

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
browse_filter_mode = "all"
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

kernel.log( "Determining if new session should start - language_switch:" .. tostring(language_switch) )
if language_switch == nil or language_switch == false then
	ShoppingCart = newshoppingcart()
	ShoppingCart:start_new()
	ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
	kernel.log( "New ShoppingCart created." )
else
	kernel.log( "language_switch is set, keeping existing session and setting language_switch to false." )
	language_switch = false
end

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

local flip_cover = function ( actor_name )
	--This should remove the artwork and replace it with the right panel of the product detail, minus buttons and banners
	if kernel.isactorvisible( actor_name ) then

		user_on_flip_view = true
		last_flipped_actor = actor_name

		kernel.hideactor( actor_name )

		local product = movie_products[actor_name]
		local ticket = nil
		if product.event ~= nil then
			ticket = movie_products[actor_name]
		end

		-- Format Movie/Game/Ticket Title
		local title_label = product.long_name
		if ticket ~= nil then
			title_label = ticket.event.event_name
		end
		kernel.setactortext( "selected_product_title", title_label )

		local title_size = kernel.measureactortext( "selected_product_title" )
		while title_size.lines_filled > 2 do
			title_label = string.sub( title_label, 0, string.len( title_label ) - 5 ).. "..."
			kernel.setactortext( "selected_product_title", title_label )
			title_size = kernel.measureactortext( "selected_product_title" )
		end

		-- Format Movie/Game/Ticket Description
		local description = product.description
		if ticket ~= nil then
			description = ticket.event.short_note
		end
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

		if ticket == nil then
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
					kernel.setactory( "national_street_date", 155 )
					kernel.setactory( "selected_product_description", 190)
					kernel.setmacro( "NationalStreetDate", string.sub( national_street_year, 0, 4 ) )
					local size = kernel.measureactortext( "selected_product_title" )
					if size.lines_filled > 1 then
						kernel.setactory( "national_street_date", 180 )
						kernel.setactory( "selected_product_description", 200 )
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
				kernel.setactory( "genre_label", 305 )
				kernel.setactory( "genre", 305 )
				kernel.setactory( "price_label", 330 )
				kernel.setactory( "price", 330 )
				kernel.setactory( "return_label", 355 )
				kernel.setactory( "return", 355 )
				kernel.setactory( "rating_label", 375 )
				kernel.setactory( "rating", 375 )
				kernel.setactory( "rating_description", 375 )
			else
				kernel.showactor( "actors_label" )
				kernel.showactor( "actors" )
				kernel.showactor( "director_label" )
				kernel.showactor( "director" )
				kernel.showactor( "running_time_label" )
				kernel.showactor( "running_time" )
				kernel.hideactor( "rating_description" )
				kernel.setactory( "genre_label", 305 )
				kernel.setactory( "genre", 305 )
				kernel.setactory( "actors_label", 330 )
				kernel.setactory( "actors", 330 )
				kernel.setactory( "director_label", 360 )
				kernel.setactory( "director", 360 )
				kernel.setactory( "running_time_label", 380 )
				kernel.setactory( "running_time", 380 )
				kernel.setactory( "price_label", 400 )
				kernel.setactory( "price", 400 )
				kernel.setactory( "return_label", 420 )
				kernel.setactory( "return", 420 )
				kernel.setactory( "rating_label", 440 )
				kernel.setactory( "rating", 440 )
			end

			kernel.showactor( "movie_flip" )
			kernel.showactor( "movie_flip_cancel" )
			kernel.showactor( "selected_product_title" )
			kernel.showactor( "genre_label" )
			kernel.showactor( "genre" )
			kernel.showactor( "selected_product_description" )
			kernel.showactor( "price_label" )
			kernel.setactortext( "price_label", strings.price_tag .. " " )
			kernel.showactor( "price" )
			kernel.setactorx( "price", 460 )
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
						kernel.setactorx( "price", 520 )
						kernel.setactorx( "price_extra", 520 )
					else
						kernel.setactorx( "price", 510 )
						kernel.setactorx( "price_extra", 510 )
					end

					kernel.showactor( "price_extra_label" )
					kernel.setactory( "price_extra_label", 417 )
					kernel.showactor( "price_extra" )
					kernel.setactory( "price_extra", 417 )
					local price_extra = string.format( "%s%.2f", strings.currency_symbol, extra_night_price )
					price_extra = price_extra .. " " .. strings.price_a_day
					kernel.setactortext( "price_extra", price_extra )

					kernel.setactory( "return_label", 434 )
					kernel.setactory( "return", 434 )

					kernel.setactory( "rating_label", 450 )
					kernel.setactory( "rating", 450 )
				end
			end
			--** End Formatting attributes specific to Movies and Games
		else
			--** Format attributes specific to Tickets
			local get_event_categories_string = function( event )
				local product_genre = nil
				local genre_count = 0
				local event_genres = {}
				-- creating event_genres table manually till we get it from the Event record
				-- in the database after Kevin's code brings it down from the update service.
				if event.primary_category_id ~= nil then
					event_genres[event.primary_category_id] = true
				end
				for i, v in pairs( event_genres ) do
					if v == true then
						local genres = Categories[i].category_name
						if genres ~= nil and product_genre == nil then
							product_genre = genres
							genre_count = genre_count + 1
						elseif genres ~= nil and product_genre ~= nil then
							product_genre = product_genre..", "..genres
						end
						if genre_count == 1 then break end
					end
				end
				if product_genre ~= nil and string.len( product_genre ) > 25 then
					product_genre = string.sub(product_genre, 0, 25)
					product_genre = product_genre ..".."
				end
				return product_genre
			end

			local get_event_dates_string = function( event )
				local event_productions = get_event_productions( event )
				local dates = {}
				local _Productions = Productions
				for _, v in ipairs( event_productions ) do
					table.insert( dates, _Productions[v].production_end_date_time_local )
				end
				table.sort(
					dates,
					function( d1, d2 )
						return d1 < d2
					end
				)
				local date_string = ""
				local tempmonth = ""
				local tempday = ""
				local counter = 0
				for _, v in ipairs( dates ) do
					local month_added = false
					local month = MonthMap[string.sub( v, 5, 6 )].short_name
					if tempmonth ~= month then
						tempday = ""
					end
					if month ~= nil and month ~= tempmonth then
						if counter > 0 then
							date_string = date_string.. " "
						end
						date_string = date_string.. month.. " "
						month_added = true
					end
					local day = string.sub( v, 7, 8 )
					if day ~= nil and day ~= tempday then
						if month_added == false then
							date_string = date_string.. ","
						end
						date_string = date_string.. day
						counter = counter + 1
					end
					if counter == 5 then break end
					tempmonth = month
					tempday = day
				end
				return date_string
			end

			local get_event_price_string = function( event )
				if event.min_price ~= nil
				and event.max_price ~= nil then
					if event.min_price ~= event.max_price then
						return string.format("%s%s - %s%s", strings.currency_symbol, tostring( math.floor(event.min_price) ) , strings.currency_symbol, tostring( math.ceil(event.max_price) ) )
					else
						return string.format("%s%s", strings.currency_symbol, tostring( math.floor(event.min_price) ) )
					end
				end
				if event.min_price ~= nil then
					return string.format("%s%s", strings.currency_symbol, tostring( math.floor(event.min_price) ) )
				end
				if event.max_price ~= nil then
					return string.format("%s%s", strings.currency_symbol, tostring( math.ceil(event.max_price) ) )
				end
			end

			kernel.showactor( "movie_flip" )
			kernel.showactor( "movie_flip_cancel" )
			kernel.showactor( "selected_product_title" )
			kernel.showactor( "selected_product_description" )
			kernel.showactor( "separator_1" )
			kernel.showactor( "separator_2" )
			kernel.setactory( "genre_label", 305 )
			kernel.showactor( "genre_label" )
			kernel.setactory( "genre", 305 )
			kernel.setactortext( "genre", get_event_categories_string( ticket.event ) )
			kernel.showactor( "genre" )
			kernel.showactor( "dates_label" )
			kernel.setactortext( "dates", get_event_dates_string( ticket.event ) )
			kernel.showactor( "dates" )
			kernel.setactortext( "price_label", strings.price_tag .. " " )
			kernel.setactory( "price_label", 355 )
			kernel.showactor( "price_label" )
			kernel.setactortext( "price", get_event_price_string( ticket.event ) )
			kernel.setactorx( "price", 460 )
			kernel.setactory( "price", 355 )
			kernel.showactor( "price" )
		end

	else
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
	--local count = collectgarbage( "count" )
	--kernel.log( ">>  Memory Count: " .. count)
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

local function arrange_buttons_based_on_games_tickets_availability()
	local do_not_show_games_button = ( DisableGames or ( ProductCatalog:in_stock_games_count() < 1 ) )

	local do_not_show_tickets_button = ( ( SellTickets == false ) or ( #ticketsforcarousel == 0 ) )

	if do_not_show_games_button
	and do_not_show_tickets_button then
		kernel.setactorx( "online_pickup_button", 179 )
		kernel.setactorx( "online_red_box_label", 179 )
		kernel.setactorx( "online_pickup_label", 179 )
		kernel.setactorx( "return_button", 386 )
		kernel.hideactor( "tickets_button" )
		kernel.hideactor( "games_button" )
		kernel.setactorwidth( "all_movies_button", 253 )
		kernel.setactorx( "all_movies_button", 593 )
		kernel.setmacro( "StartView:AllMovies", strings.StartView_Movies_ALL )
	elseif do_not_show_games_button then
		kernel.setactorx( "online_pickup_button", 75 )
		kernel.setactorx( "online_red_box_label", 75 )
		kernel.setactorx( "online_pickup_label", 75 )
		kernel.setactorx( "return_button", 282 )
		kernel.setactorx( "tickets_button", 489 )
		kernel.showactor( "tickets_button" )
		kernel.hideactor( "games_button" )
		kernel.setactorwidth( "all_movies_button", 253 )
		kernel.setactorx( "all_movies_button", 696 )
		kernel.setmacro( "StartView:AllMovies", strings.StartView_Movies_ALL )
	elseif do_not_show_tickets_button then
		kernel.setactorx( "online_pickup_button", 75 )
		kernel.setactorx( "online_red_box_label", 75 )
		kernel.setactorx( "online_pickup_label", 75 )
		kernel.setactorx( "return_button", 282 )
		kernel.hideactor( "tickets_button" )
		kernel.setactorx( "games_button", 489 )
		kernel.showactor( "games_button" )
		kernel.setactorwidth( "all_movies_button", 253 )
		kernel.setactorx( "all_movies_button", 696 )
		kernel.setmacro( "StartView:AllMovies", strings.StartView_Movies_ALL )
	else
		kernel.setactorx( "online_pickup_button", 24 )
		kernel.setactorx( "online_red_box_label", 24 )
		kernel.setactorx( "online_pickup_label", 24 )
		kernel.setactorx( "return_button", 221 )
		kernel.setactorx( "tickets_button", 418 )
		kernel.showactor( "tickets_button" )
		kernel.setactorx( "games_button", 615 )
		kernel.showactor( "games_button" )
		kernel.setactorwidth( "all_movies_button", 187 )
		kernel.setactorx( "all_movies_button", 812 )
		kernel.setmacro( "StartView:AllMovies", strings.StartView_Movies )
	end
end

local function show_burn_in_view()
	showing_burn_in_view = true
	kernel.hideactor( "redbox_logo" )
	kernel.hideactor( "help_button" )
	kernel.hideactor( "espanol_button" )
	kernel.hideactor( "right_arrow_button" )
	kernel.hideactor( "left_arrow_button" )
	kernel.hideactor( "all_movies_button" )
	kernel.hideactor( "tickets_button" )
	kernel.hideactor( "games_button" )
	kernel.hideactor( "return_button" )
	kernel.hideactor( "online_pickup_button" )
	kernel.hideactor( "online_red_box_label" )
	kernel.hideactor( "online_pickup_label" )
	kernel.hideactor( "logo_label" )
	kernel.hideactor( "back_ground_image" )
	kernel.hideactor( "handicap_button" )
	kernel.showactor( "burn_in_view_label" )
	kernel.setactortext( "burn_in_view_label", strings.burn_in_view_label)
	kernel.timerstart( "burn-in-view-duration-timer" )
	burn_in_timer:stop()
end

local function show_normal_view()
	if (showing_burn_in_view ~= nil
	and showing_burn_in_view == true) then
		arrange_buttons_based_on_games_tickets_availability()
		kernel.showactor( "redbox_logo" )
		kernel.showactor( "help_button" )
		kernel.showactor( "espanol_button" )
		if #topten_list == max_top_count then
			kernel.showactor( "right_arrow_button" )
			kernel.showactor( "left_arrow_button" )
		end
		kernel.showactor( "all_movies_button" )
		kernel.showactor( "return_button" )
		kernel.showactor( "online_pickup_button" )
		kernel.showactor( "online_red_box_label" )
		kernel.showactor( "online_pickup_label" )
		kernel.showactor( "logo_label" )
		kernel.showactor( "back_ground_image" )
		kernel.showactor( "handicap_button" )
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

-- Function to get the product associated with a particular actor
function get_movie_product( movie_actor_name )
	local movie_index_to_fetch = 0
	movie_index_to_fetch = string.sub( movie_actor_name, string.find( movie_actor_name, "%d" ), #movie_actor_name )
	return movie_products[movie_index_to_fetch]
end

function DefineTween()
	kernel.log( "*-- Carousel DefineTween" )

	Carousel_Direction = "left"

	xPositions = {-300, -300, -62, 159, 380, 648, 869, 1071, 1071, 1071 }
	yPositions = { 150,  150, 150, 150, 120, 150, 150,  150,  150,  150 }
	oValues = { 0.00, 0.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.00, 0.00, 0.00 }
	wValues = { 221, 221, 221, 221, 268, 221, 221, 221, 221, 221 }
	hValues = { 318, 318, 318, 318, 388, 318, 318, 318, 318, 318 }

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
		if next_pos == 0 then next_pos = 10 end
		if next_pos == 11 then next_pos = 1 end

		local new_x = xPositions[i] + ( ( ( xPositions[next_pos] - xPositions[i] ) / 10 ) * v )
		local new_y = yPositions[i] + ( ( ( yPositions[next_pos] - yPositions[i] ) / 10 ) * v )
		local new_opacity = oValues[i] + ( ( ( oValues[next_pos] - oValues[i] ) / 10 ) * v )
		local new_width = wValues[i] + ( ( ( wValues[next_pos] - wValues[i] ) / 10 ) * v )
		local new_height = hValues[i] + ( ( ( hValues[next_pos] - hValues[i] ) / 10 ) * v )

		-- Exception to keep last one from looping around the others, just jump it to its future location
		if next_pos == 10 or next_pos == 1 then
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
		for i = 1, 10 do
			local inst = CalculateTweenValues( i, v )
			move_poster( inst )
		end
	end

	function Actor_Step()
		local new_movie_positions = {}
		local step_incr = iif( Carousel_Direction == "left", -1, 1 )
		for i = 1, 10 do
			local old_pos = i + step_incr
			if old_pos == 0 then old_pos = 10 end
			if old_pos == 11 then old_pos = 1 end
			new_movie_positions[i] = movie_positions[ old_pos ]
		end
		movie_positions = new_movie_positions
	end

	function Carousel_Stop()
		--kernel.log( "*----------- Carousel_Stop" )
		Actor_Step()
		if movie_positions ~= nil
		and #movie_positions > 4
		and movie_products[movie_positions[5]].event ~= nil then
			kernel.setbordercolor("add_to_cart","white")
			kernel.setactortext( "add_to_cart", "    BUY TICKETS!" )
			kernel.showactor( "ticket_5_position" )
		else
			kernel.setbordercolor("add_to_cart", {["r"] = 134, ["g"] = 12,["b"] = 19} )
			if kernel.getculture() == "es-MX" then
				kernel.setactortext( "add_to_cart", "     iAlquilelo Ahora!" )
			else
				kernel.setactortext( "add_to_cart", "    RENT IT NOW!" )
			end
			kernel.showactor( "movie_5_position" )
		end
		kernel.showactor( "add_to_cart" )
		kernel.showactor( "plus_button" )
		kernel.showactor( "shadow_movie_1" )
		kernel.showactor( "shadow_movie_2" )
		kernel.showactor( "shadow_movie_4" )
		kernel.showactor( "shadow_movie_5" )
		kernel.showactor( "movie_3_position" )
		kernel.showactor( "movie_4_position" )
		kernel.showactor( "movie_6_position" )
		kernel.showactor( "movie_7_position" )
		kernel.enableactor( "right_arrow_button" )
		kernel.enableactor( "left_arrow_button" )
		kernel.timerstart( "carousel-user-timer" )
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
	--kernel.log( "*----------- move_direction" )
	if kernel.peekview() ~= "start_view" then
		return
	end
	if kernel.isactorvisible( last_flipped_actor ) == false then
		flip_cover( last_flipped_actor )
	end
	if #topten_list < max_top_count then
		return
	end
	kernel.hideactor( "movie_5_position" )
	kernel.hideactor( "ticket_5_position" )
	kernel.disableactor( "right_arrow_button" )
	kernel.disableactor( "left_arrow_button" )
	kernel.hideactor( "add_to_cart" )
	kernel.hideactor( "plus_button" )
	kernel.hideactor( "shadow_movie_1" )
	kernel.hideactor( "shadow_movie_2" )
	kernel.hideactor( "shadow_movie_4" )
	kernel.hideactor( "shadow_movie_5" )
	kernel.hideactor( "movie_3_position" )
	kernel.hideactor( "movie_4_position" )
	kernel.hideactor( "movie_6_position" )
	kernel.hideactor( "movie_7_position" )

	Carousel_Direction = direction
	kernel.tweenstart( "move-carousel" )
end

-- function to go to the browse view
function browse_all_handle()
	kernel.timerremove( "carousel-timer" )
	kernel.timerremove( "carousel-user-timer" )
	kernel.timerremove( "carousel-wait" )
	browse_filter_family_tracker.selected_family = "Movies"
	browse_filter_family_tracker:set_family_type( "ALL" )
	browse_filter_genre_selected = ALL
	kernel.tweenstop( "move-carousel" )
	kernel.executescript( "browse_products" )
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
	table.insert( movie_positions, 1, "stvw_movie_9" )	--Set movie 9 at position 1
	table.insert( movie_positions, 2, "stvw_movie_10" )	--Set movie 10 at position 2
	table.insert( movie_positions, 3, "stvw_movie_1" )	--Set movie 1 at position 3
	table.insert( movie_positions, 4, "stvw_movie_2" )	--Set movie 2 at position 4
	table.insert( movie_positions, 5, "stvw_movie_3" )	--Set movie 3 at position 5
	table.insert( movie_positions, 6, "stvw_movie_4" )	--Set movie 4 at position 6
	table.insert( movie_positions, 7, "stvw_movie_5" )	--Set movie 5 at position 7
	table.insert( movie_positions, 8, "stvw_movie_6" )	--Set movie 6 at position 8
	table.insert( movie_positions, 9, "stvw_movie_7" )	--Set movie 7 at position 9
	table.insert( movie_positions, 10, "stvw_movie_8" )	--Set movie 8 at position 10

	last_flipped_actor = "stvw_movie_3"

	kernel.registeractorhitnotify(
		"movie_3_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "movie_3_position" )
			if movie_products[movie_positions[3]].image_file == "image_message_poster_background" then
				browse_all_handle()
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
				browse_all_handle()
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
				browse_all_handle()
			else
				kernel.timerstop( "carousel-timer" )
				kernel.timerstop( "carousel-user-timer" )
				flip_cover( movie_positions[5] )
			end
		end
	)

	kernel.registeractorhitnotify(
		"ticket_5_position",
		function()
			show_normal_view()
			view_analytics_start( Start_Over_View_Name, "BrowseVend" )
			view_analytics_event( "buttonPress", "ticket_5_position" )
			kernel.timerstop( "carousel-timer" )
			kernel.timerstop( "carousel-user-timer" )
			flip_cover( movie_positions[5] )
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
				browse_all_handle()
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
				browse_all_handle()
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
		local ticket = {}

		product = topten_list[list_position]
		if product.event ~= nil then
			ticket = product
			movie_products["stvw_movie_".. imageCount] = ticket
			base_image_name = "image_ticket_design"
			local ticket_image = "image_tickets_no_image_available"
			if product.image_file ~= nil then
				ticket_image = product.image_file
			end
			source_image_name = "ticket-".. ticket_image
			kernel.registerimagefromcache(
				ticket_image,
				ticket_image
			)
			kernel.drawimageonimage(
				ticket_image,
				base_image_name,
				source_image_name,
				{ X = 5, Y = 106, Width = 269, Height = 225 }
			)

			kernel.setactorimage( "stvw_movie_".. imageCount, string.format( "cache:%s", source_image_name ) )
		else
			local product_type = nil
			movie_products["stvw_movie_"..imageCount] = product
			if product ~= nil then
				product_type = _ProductTypes[product.product_type_id]
				if  product ~= nil
				and product_type ~= nil then
					local product_family = _ProductFamilies[product_type.product_family_id]
					-- add in cache state variables...
					local cache = _ProductCatalog[product.product_id]
					if cache ~= nil then
						product.can_be_sold = cache.can_be_sold == nil and false or cache.can_be_sold
						product.is_available = cache.is_available == nil and false or cache.is_available
					end
					if product_family.product_family_name == "Movies" then
						if kernel.getculture() == "es-MX" then
							source_image_name = "image_movie_missing_box_art_es_MX"
						else
							source_image_name = "image_movie_missing_box_art"
						end
						kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
					elseif product_family.product_family_name == "Games" then
						if kernel.getculture() == "es-MX" then
							source_image_name = "image_game_missing_box_art_es_MX"
						else
							source_image_name = "image_game_missing_box_art"
						end
						kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
					end
					local thumbnail_name = "missing_box_art-thumbnail"
					local image_registered = kernel.registerimagefromcache(	product.image_file,	product.image_file )

					if image_registered then
						source_image_name = string.format( "cache:%s", product.image_file )
						kernel.setactorimage( "stvw_movie_".. imageCount, source_image_name )
					end
				end
				kernel.setactortext( "stvw_movie_".. imageCount, "" )
			else
				kernel.hideactor( "stvw_movie_".. imageCount )
				kernel.hideactor( "shadow_movie_".. imageCount )
			end
		end

		imageCount = imageCount + 1

		list_position = list_position + 1
		if list_position > #topten_list then
			list_position = 1
		end
	end
	kernel.showactor( "movie_5_position" )
end

local function load()
	local topten_list_contains = function( product )
		for k, v in ipairs( topten_list ) do
			if v.product_id == product.product_id then
				return true
			end
		end
		return false
	end

	local temp_count = 0
	local temp_ProductCatalog = ProductCatalog:get_copy_with_release_date()
	table.sort( temp_ProductCatalog,
		function( a, b )
			if a.release_date == nil
			or b.release_date == nil
			then
				return false
			end

			if a.release_date ~= b.release_date then
				return a.release_date > b.release_date
			end
		end
	)

	local game_allowed_to_be_shown = function( product_type )
		if product_type ~= nil 
		and (( GameEnablePS3 == false
		and product_type.product_type_name == 'PS3' ) 
		or ( GameEnablePS4 == false
		and product_type.product_type_name == 'PS4' ) 
		or ( GameEnableWii == false
		and product_type.product_type_name == 'Wii' ) 
		or ( GameEnableWiiU == false
		and product_type.product_type_name == 'Wii U' ) 
		or ( GameEnableXbox360 == false
		and product_type.product_type_name == 'Xbox 360' ) 
		or ( GameEnableXboxOne == false
		and product_type.product_type_name == 'Xbox One' )) then
			return false
		end
		return true
	end

	-- Populate 'Early Release' titles first
	for k, v in ipairs( temp_ProductCatalog ) do
		local masterproduct = MasterProductCatalog[ v.product_id ]
		if temp_count >= max_top_count  then
			break
		else
			if masterproduct ~= nil
			and k ~= "1139"
			and v.in_stock ~= nil
			and _ProductCatalog:in_stock( v.product_id ) == true
			and ( masterproduct.product_type_id == 1
				or masterproduct.product_type_id == 2
			)
			and masterproduct.sell_thru_new ~= true
			and StoreManager.PricingManager:is_early_released( masterproduct ) == true
			and topten_list_contains( v ) == false
			and is_dnr_disc( masterproduct ) == false
			then
				temp_count = temp_count + 1
				table.insert( topten_list, masterproduct )
			end
		end
	end
	kernel.log( "Carousel movie count after Early Release: ".. tostring( #topten_list ) )

	-- Populate 'Hit Movies' (genre == 1020) by default
	if #topten_list ~=	max_top_count then
		for k, v in ipairs( temp_ProductCatalog ) do
			local masterproduct = MasterProductCatalog[v.product_id]
			if temp_count >= max_top_count  then
				break
			else
				if masterproduct ~= nil
				and k ~= "1139"
				and v.is_available ~= nil
				and v.is_available == true
				and v.in_stock ~= nil
				and (masterproduct.genres ~= nil
					and masterproduct.genres[1020] ~= nil
				)
				and _ProductCatalog:in_stock( v.product_id ) == true
				and ( masterproduct.product_type_id == 1
					or masterproduct.product_type_id == 2
				)
				and masterproduct.sell_thru_new ~= true
				and topten_list_contains( v ) == false
				and is_dnr_disc( masterproduct ) == false
				then
					temp_count = temp_count + 1
					table.insert( topten_list, masterproduct )
				end
			end
		end
	end
	kernel.log( "Carousel movie count after Hit Movies: ".. tostring( #topten_list ) )

	-- If there are not enough 'Hit Movies' in top ten, then add 'Top 20' (genre == 1093) movies.
	if #topten_list ~= max_top_count then
		for k, v in ipairs( temp_ProductCatalog ) do
			local masterproduct = MasterProductCatalog[v.product_id]
			if temp_count >= max_top_count  then
				break
			else
				if masterproduct ~= nil
				and k ~= "1139"
				and v.is_available ~= nil
				and v.is_available == true
				and v.in_stock ~= nil
				and ( masterproduct.genres ~= nil
					and masterproduct.genres[1093] ~= nil
					and masterproduct.genres[1020] == nil
				)
				and _ProductCatalog:in_stock( v.product_id ) == true
				and ( masterproduct.product_type_id == 1
					or masterproduct.product_type_id == 2
				)
				and masterproduct.sell_thru_new ~= true
				and topten_list_contains( v ) == false
				and is_dnr_disc( masterproduct ) == false
				then
					temp_count = temp_count + 1
					table.insert( topten_list, masterproduct )
				end
			end
		end
	end
	kernel.log( "Carousel movie count after Top 20 movies: ".. tostring( #topten_list ) )

	-- If 'Top 20' do not suffice required top 10 movies, then add 'Action' (genre == 1000) movies.
	if #topten_list ~= max_top_count then
		for k, v in ipairs( temp_ProductCatalog ) do
			local masterproduct = MasterProductCatalog[v.product_id]
			if temp_count >= max_top_count  then
				break
			else
				if masterproduct ~= nil
				and k ~= "1139"
				and v.is_available ~= nil
				and v.is_available == true
				and v.in_stock ~= nil
				and ( masterproduct.genres ~= nil
					and masterproduct.genres[1000] ~= nil
					and masterproduct.genres[1093] == nil
					and masterproduct.genres[1020] == nil
				)
				and _ProductCatalog:in_stock(v.product_id) == true
				and ( masterproduct.product_type_id == 1
					or masterproduct.product_type_id == 2
				)
				and masterproduct.sell_thru_new ~= true
				and topten_list_contains( v ) == false
				and is_dnr_disc( masterproduct ) == false
				then
					temp_count = temp_count + 1
					table.insert( topten_list, masterproduct )
				end
			end
		end
	end
	kernel.log( "Carousel movie count after Action movies: ".. tostring( #topten_list ) )

	-- Add DVD count  poster.
	--[[for k, v in ipairs(topten_list) do
		if k == 4 then
			temp = {}
			temp.long_name = "image_message_poster_background"
			temp.image_file = "image_message_poster_background"
			table.insert(topten_list, 4, temp)
		end
	end]]

	-- Tickets Logic
	if kernel.ticketsvcs_selltickets() then
		local carousel_events = kernel.ticketsvcs_getcarouselevents()
		local _Productions = Productions
		local _Events = Events

		for k, v in ipairs( carousel_events ) do
			local soonest_production = _Productions[EventToProductions[tostring(v.event_id)].soonest_production_not_expiring_in_2_hours]
			local event_poster = kernel.ticketsvcs_registerimagefromcache( 1, 1, v.event_id )
			local ticket = { ["event"] = v, ["production"] = soonest_production, ["image_file"] = event_poster }
			table.insert( ticketsforcarousel, ticket )
		end

		get_event_productions = function( event )
			local productions = EventToProductions[event.event_id].production_ids
			-- First sort the productions by production_end_date_time_utc
			table.sort(
				productions,
				function( p1, p2 )
					-- If production_end_date_time_utc is null, sort first 
					if ( _Productions[p1].production_end_date_time_utc == nil ) then
						return true
					end
					
					if ( _Productions[p2].production_end_date_time_utc ~= nil ) then
						return _Productions[p1].production_end_date_time_utc < _Productions[p2].production_end_date_time_utc
					end
					
					return false
				end
			)
			return productions
		end

		-- Sort tickets based on featured first, then category, then production_end_date_time and then alphabetically.
		--if VerboseLogging then
		--	kernel.log( "|Tickets| Tickets in Carousel before sorting Events" )
		--	kernel.log( ticketsforcarousel )
		--end
		table.sort(
			ticketsforcarousel,
			function( t1, t2 )
				if t1 ~= nil and t2 ~= nil then
					local e1 = t1.event
					local e2 = t2.event
					local p1 = t1.production
					local p2 = t2.production

					-- top-picks before non top-picks
					if e1.featured == 1
					and e2.featured == 0 then
						-- Sorting e1 ahead of e2 due to being a top-pick
						return true
					end

					-- if both events are top-picks
					if e1.featured == 1
					and e2.featured == 1 then
						-- If production_end_date_time_utc is null, sort first 
						if ( p1.production_end_date_time_utc == nil ) then
							-- Sorting e1 ahead of e2 due to production_end_date_time_utc is null
							return true
						elseif (p2.production_end_date_time_utc == nil ) then
							-- keep e2 ahead of e1 due to production_end_date_time_utc is null
							return false
						end
					
						-- if eventdates are different sort soonest first						
						if ( p2.production_end_date_time_utc ~= nil ) then
							if p1.production_end_date_time_utc ~= p2.production_end_date_time_utc then
								-- Sorting e1 and e2 by production_end_date_time_utc
								return p1.production_end_date_time_utc <= p2.production_end_date_time_utc
							end
						end
						--else sort by sort_name
						return e1.sort_name <= e2.sort_name
					end

					-- Sports before Music
					if e1.primary_category_id == 2
					and e2.primary_categoryId == 1 then
						return true
					end

					-- Music before Arts
					if e1.primary_category_id == 1
					and e2.primary_categoryId == 3 then
						return true
					end

					-- Arts before Other
					if e1.primary_category_id == 3
					and e2.primary_categoryId == 5 then
						return true
					end

					-- Other before Family
					if e1.primary_category_id == 5
					and e2.primary_categoryId == 4 then
						return true
					end

					-- if both events belong to same category.
					if e1.primary_category_id == e2.primary_category_id then
						-- If production_end_date_time_utc is null, sort first 
						if ( p1.production_end_date_time_utc == nil ) then
							-- Sorting ahead of e2 due to production_end_date_time_utc is null
							return true
						elseif (p2.production_end_date_time_utc == nil ) then
							-- Sorting keeping e2 ahead of e1 due to production_end_date_time_utc is null
							return false
						end

						-- if eventdates are different sort soonest first
						if ( p2.production_end_date_time_utc ~= nil ) then
							if p1.production_end_date_time_utc ~= p2.production_end_date_time_utc then
								return p1.production_end_date_time_utc <= p2.production_end_date_time_utc
							end
						end
						--else sort by sort_name
						return e1.sort_name <= e2.sort_name
					end
				end
			end
		)
		--if VerboseLogging then
		--	kernel.log( "|Tickets| Tickets in Carousel after sorting Events:" )
		--	kernel.log( ticketsforcarousel )
		--end

		if #ticketsforcarousel > 0 then
			-- Place the Ticket in positions 2 in the carousel.
			if ticketsforcarousel[1] ~= nil then
				table.insert( topten_list, 2, ticketsforcarousel[1] )
				if #topten_list > max_top_count then
					table.remove( topten_list )
				end
				kernel.log( "|Tickets| Carousel movie count after Ticket at position 2: ".. tostring( #topten_list ) )
			end

			-- Place the Ticket in positions 5 in the carousel.
			if ticketsforcarousel[2] ~= nil then
				table.insert( topten_list, 5, ticketsforcarousel[2] )
				if #topten_list > max_top_count then
					table.remove( topten_list )
				end
				kernel.log( "|Tickets| Carousel movie count after Ticket at position 5: ".. tostring( #topten_list ) )
			end

			-- Place the Ticket in positions 9 in the carousel.
			if ticketsforcarousel[3] ~= nil then
				table.insert( topten_list, 9, ticketsforcarousel[3] )
				if #topten_list > max_top_count then
					table.remove( topten_list )
				end
				kernel.log( "|Tickets| Carousel movie count after Ticket at position 9: ".. tostring( #topten_list ) )
			end
		end

		function format_fee_per_ticket( fee_per_ticket )
			if (math.fmod( fee_per_ticket, 1 ) == 0 ) then
				return string.format( "%d", fee_per_ticket )
			else
				return string.format( "%.2f", fee_per_ticket )
			end
		end

		if TicketsProfilePricing["Fee"] ~= nil then
			tickets_fee_for_display = format_fee_per_ticket( TicketsProfilePricing["Fee"].value )
		else
			tickets_fee_for_display = "1"
		end
	end

	--Tickets logic ends here.

	-- Move logic for loading the game after the tickets are loaded.
	if not DisableGames and ProductCatalog:in_stock_games_count() >=3 then
		-- Add 1 game in top 10.
		for k, v in ipairs( temp_ProductCatalog ) do
			local toptengameproduct = MasterProductCatalog[v.product_id]
			if toptengameproduct ~= nil
			and ProductTypes[toptengameproduct.product_type_id].product_family_id == 2
			and game_allowed_to_be_shown( ProductTypes[toptengameproduct.product_type_id] )
			and v.is_available ~= nil and v.is_available == true
			and v.in_stock ~= nil and v.in_stock == true
			and toptengameproduct.rating_id <= 17
			and topten_list_contains( v ) == false
			and is_dnr_disc( toptengameproduct ) == false
			then
				if #topten_list >= 6 then
					table.insert( topten_list, 6, toptengameproduct )
					if #topten_list > max_top_count then
						table.remove( topten_list, 7 )
					end
				end
				break
			end
		end
	end
	kernel.log( "Carousel movie count after Games: ".. tostring( #topten_list ) )

	--if VerboseLogging then
	--	kernel.log( "List of Movies in Carousel:" )
	--	kernel.log( topten_list )
	--end
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
	elseif product_family.product_family_name == "Movies" then
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = ALL
	end
	--kernel.tweenstop( "move-carousel" )
	--kernel.executescript( "browse_products" )
	if show_alert_message then
		show_alert_message = false
		check_for_warning_popups( check_alert_table, selected_movie, i )
	end
end

local function add_movie()
	selected_movie = movie_products[movie_positions[5]]
	selected_movies_list[1] = selected_movie
	browse_selection_cart_action = "rental"
	if( StoreManager.PricingManager:is_early_released( selected_movie ) ) then
		price_set_item = true
		add_item_tocart()
	else
		show_alert_message = true
		add_item_tocart()
	end
end

load()

function refresh_start_view()
	if #topten_list == max_top_count then
		show_images(0)
		kernel.hideactor( "loading_titles_label" )
	else
		kernel.hideactor( "stvw_movie_1" )
		kernel.hideactor( "shadow_movie_1" )
		kernel.hideactor( "stvw_movie_2" )
		kernel.hideactor( "shadow_movie_2" )
		kernel.hideactor( "stvw_movie_3" )
		kernel.hideactor( "stvw_movie_4" )
		kernel.hideactor( "shadow_movie_4" )
		kernel.hideactor( "stvw_movie_5" )
		kernel.hideactor( "shadow_movie_5" )
		kernel.hideactor( "add_to_cart" )
		kernel.hideactor( "plus_button" )
		kernel.hideactor( "movie_3_position" )
		kernel.hideactor( "movie_4_position" )
		kernel.hideactor( "movie_6_position" )
		kernel.hideactor( "movie_7_position" )
		kernel.hideactor( "left_page_shadow" )
		kernel.hideactor( "left_arrow_button" )
		kernel.hideactor( "right_page_shadow" )
		kernel.hideactor( "right_arrow_button" )
		kernel.showactor( "loading_titles_label" )
		--[[AlertManager.send_alert(
			"TitleEmpty",
			"Carousel title selection failed.",
			"This could be caused by low kiosk inventory or from corrupt data files. Kiosk is displaying loading titles message.",
			nil,
			nil,
			"Alert from kiosk ".. StoreManager:get_number()
		)]]
	end
	if SellTickets then
		kernel.setactortext( "logo_label", strings.start_view_title )
	end
end

function reload_carousel( force )
	if #topten_list ~= max_top_count or force == true then
		load()
		if #topten_list >= max_top_count then
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
	"left_arrow_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "left_arrow_button" )
		kernel.timerstop( "carousel-timer" )
		next_motion = nil
		next_user_motion = function() move_direction( "left" ) end
		move_direction( "left" )
	end
)

kernel.registeractorhitnotify(
	"right_arrow_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "right_arrow_button" )
		kernel.timerstop( "carousel-timer" )
		next_motion = nil
		next_user_motion = function() move_direction( "right" ) end
		move_direction( "right" )
	end
)

kernel.registeractorhitnotify(
	"add_to_cart",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "add_to_cart" )
		kernel.timerstop( "carousel-timer" )
		kernel.timerstop( "carousel-user-timer" )
		if movie_products[movie_positions[5]].event ~= nil then
			ShoppingCart.TicketsCart = newticketscart()
			if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_browse_products' " ) end
			kernel.executescript( "tickets_browse_products" )
			ShoppingCart.TicketsCart.event = movie_products[movie_positions[5]].event
			if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_production_selection_view' " ) end
			view_show( "tickets_production_selection_view" )
		else
			added_from_carousel = true
			item_added_from_view = kernel.peekview()
			add_movie()
		end
	end
)

kernel.registeractorhitnotify(
	"plus_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "plus_button" )
		kernel.timerstop( "carousel-timer" )
		kernel.timerstop( "carousel-user-timer" )
		if movie_products[movie_positions[5]].event ~= nil then
			ShoppingCart.TicketsCart = newticketscart()
			if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_browse_products' " ) end
			kernel.executescript( "tickets_browse_products" )
			ShoppingCart.TicketsCart.event = movie_products[movie_positions[5]].event
			if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_production_selection_view' " ) end
			view_show( "tickets_production_selection_view" )
		else
			added_from_carousel = true
			item_added_from_view = kernel.peekview()
			add_movie()
		end
	end
)

kernel.registeractorhitnotify(
	"tickets_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "tickets_button" )
		kernel.timerremove( "carousel-timer" )
		kernel.timerremove( "carousel-user-timer" )
		kernel.timerremove( "carousel-wait" )
		kernel.tweenstop( "move-carousel" )
		if kernel.getculture() ~= "en-US" then localization_toggle_culture() end
		kernel.setlasthitactorname( "tickets_button" )
		ShoppingCart.TicketsCart = newticketscart()
		if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_show_how_it_works_view' " ) end
		kernel.executescript( "tickets_show_how_it_works_view" )
	end
)

kernel.registeractorhitnotify(
	"games_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "games_button" )
		kernel.timerremove( "carousel-timer" )
		kernel.timerremove( "carousel-user-timer" )
		kernel.timerremove( "carousel-wait" )
		browse_filter_family_tracker.selected_family = "Games"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = nil
		kernel.tweenstop( "move-carousel" )
		kernel.executescript( "browse_products" )
	end
)

kernel.registeractorhitnotify(
	"all_movies_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "all_movies_button" )
		kernel.timerremove( "carousel-timer" )
		kernel.timerremove( "carousel-user-timer" )
		kernel.timerremove( "carousel-wait" )
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = ALL
		kernel.tweenstop( "move-carousel" )
		kernel.executescript( "browse_products" )
	end
)

arrange_buttons_based_on_games_tickets_availability()