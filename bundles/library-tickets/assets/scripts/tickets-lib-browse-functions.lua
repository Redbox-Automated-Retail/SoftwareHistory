--
-- tickets-lib-browse-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function get_min_max_price( min_price, max_price)
	if min_price ~= nil and max_price ~= nil then
		if min_price ~= max_price then
			return "$" .. tostring( math.floor(min_price) ) .. " - $" .. tostring( math.ceil(max_price) )
		else
			return "$" .. tostring( math.floor(min_price) )
		end
	elseif min_price ~= nil then
		return "$" .. tostring( math.floor(min_price) )
	elseif max_price ~= nil then
		return "$" .. tostring( math.ceil(max_price) )
	end
end

function tickets_show_selection_grid()
	local function is_today( event )
		if EventToProductions[event.event_id].is_open_ended == true then
			return true
		end
		local _Productions = Productions
		local production_ids = EventToProductions[event.event_id].production_ids
		local current_datetime = os.date("*t")
		local current_datetime_string = string.format( "%4d%02d%02d", current_datetime.year, current_datetime.month, current_datetime.day )
		for k, v in ipairs( production_ids ) do
			local production = _Productions[v]
			if production.production_start_date_time_local == nil then
				local elapsed_days = kernel.getelapseddays( current_datetime_string, production.production_end_date_time_local )
				if elapsed_days >= 0 and elapsed_days < 1 then
					return true
				end
			else
				local current_datetime_with_all_string = string.format( "%4d%02d%02d%02d%02d%02d", current_datetime.year, current_datetime.month, current_datetime.day, current_datetime.hour, current_datetime.min, current_datetime.sec )
				if current_datetime_with_all_string >= production.production_start_date_time_local and current_datetime_with_all_string <= production.production_end_date_time_local then
					return true
				end
			end
		end
		return false
	end
	
	local function is_last_chance( event )
		if EventToProductions[event.event_id].is_open_ended == true then
			return true
		end
		local _Productions = Productions
		local production_ids = EventToProductions[event.event_id].production_ids
		local latest_production_production_end_date_time_local = _Productions[production_ids[1]].production_end_date_time_local
		for k, v in ipairs( production_ids ) do
			local production = _Productions[v]
			if production.production_end_date_time_local > latest_production_production_end_date_time_local then
				latest_production_production_end_date_time_local = production.production_end_date_time_local
			end
		end
		
		local current_datetime = os.date("*t")
		local current_datetime_string = string.format( "%4d%02d%02d", current_datetime.year, current_datetime.month, current_datetime.day )
		local elapsed_days = kernel.getelapseddays( current_datetime_string, latest_production_production_end_date_time_local )
		if elapsed_days >= 0 and elapsed_days < 3 then
			return true
		end
		return false
	end
	
	local function get_browse_event_poster( event )
		local event_poster = kernel.ticketsvcs_registerimagefromcache( 1, 1, event.event_id )
		if event_poster ~= nil then
			return string.format( "cache:%s", event_poster )
		end
		return "image_tickets_no_image_available"
	end
	
	local _tickets_browse_filtered_products = tickets_browse_filtered_products
	local _tickets_selection_grid_scene_manager = tickets_selection_grid_scene_manager
	local _tickets_browse_pager = tickets_browse_pager

	_tickets_selection_grid_scene_manager:clear()
	local x = 67
	local y = 115
	local actorIndex = 1
	local events_shown = 0

	if _tickets_browse_pager.is_valid == false then
		_tickets_browse_pager:reset()
	end

	local total_pages = math.ceil( #_tickets_browse_filtered_products / 8 )
	if kernel.peekview() == "tickets_browse_view" then
		view_analytics_event( "pagesShown", tostring( total_pages ) )
	end

	if total_pages > 0 then
		while _tickets_browse_pager:page_number() > total_pages do
			_tickets_browse_pager:page_back()
		end
	end

	if total_pages == 0 then
		kernel.hideactor( "page_label" )
	else
		kernel.setmacro( "Page", tostring( _tickets_browse_pager:page_number() ) )
		kernel.setmacro( "TotalPages", total_pages )
		kernel.showactor( "page_label" )
	end

	_tickets_browse_pager:mark_current_page()

	if #_tickets_browse_filtered_products == 0 then
		_tickets_selection_grid_scene_manager:add_actor(
			"inventory_message",
			0,
			302,
			1024,
			32,
			{	
				font = "font_akzidenz_32_bold",
				renderOptions = "Text, TextShadow",
				foregroundColor = { r = "224", g = "38", b = "44" },
				verticalAlignment = "center",
				horizontalAlignment = "center",
				text = "Sorry, there are no events"
			}
		)
	end
	
	for k, _ in ipairs( _tickets_browse_filtered_products ) do
		if _tickets_browse_pager:get_index() > #_tickets_browse_filtered_products then
			break
		end

		local catalog_entry = _tickets_browse_filtered_products[_tickets_browse_pager:get_index()]
		local product_name = "product_name_" .. actorIndex
		
		--render the browse flags
		if is_today( catalog_entry ) == true then
			_tickets_selection_grid_scene_manager:add_actor(
				"flag_today_" .. product_name,
				x-16,
				y-18,
				76,
				34,
				{
					renderOptions = "Image",
					image = "image_flag_today",
					addToFront = true
				}
			)
		elseif is_last_chance( catalog_entry ) == true then
			_tickets_selection_grid_scene_manager:add_actor(
				"flag_last_chance_" .. product_name,
				x-13,
				y-18,
				105,
				34,
				{
					renderOptions = "Image",
					image = "image_flag_last_chance",
					addToFront = true
				}
			)
		end
		
		--render the product image/name
		_tickets_selection_grid_scene_manager:add_actor(
			product_name,
			x,
			y,
			204,
			150,
			{
				hitOptions = "Enabled",
				foregroundColor = "black",
				font = "font_akzidenz_10_bold",
				horizontalAlignment = "center",
				text = catalog_entry.sort_name,
				renderOptions = "Image, DrawBorder, Text, ElideText",
				image = get_browse_event_poster( catalog_entry ),
				borderColor = { r = "60", g = "56", b = "57" },
				textRegion = { x = 0, y = 158, width = 204, height = 40 },
				addToFront = true
			}
		)
		
		--show oep/vb background tab
		if EventToProductions[catalog_entry.event_id].is_oep_or_vb == true then
			local production_ids = {}
			for k, v in ipairs( EventToProductions[catalog_entry.event_id].production_ids ) do
				table.insert( production_ids, v )
			end
			table.sort(
				production_ids,
				function( a, b )
					if Productions[a].production_end_date_time_utc == Productions[b].production_end_date_time_utc then
						return Productions[a].rb_offsale_utc < Productions[b].rb_offsale_utc
					else
						return tostring( Productions[a].production_end_date_time_utc ) < tostring( Productions[b].production_end_date_time_utc )
					end
				end
			)
			local first_production = Productions[production_ids[1]]
			
			_tickets_selection_grid_scene_manager:add_actor(
				"oep_vb_background_tab_" .. product_name,
				x-4,
				y-5,
				222,
				162,
				{
					image = "image_oep_vb_background_tab",
					text = ShoppingCart.TicketsCart:get_production_datetime_display( first_production ),
					foregroundColor = "black",
					font = "font_arial_7_regular",
					textRegion = { x = 0, y = 0, width = 87, height = 18 },
					textRotationAngle = 90,
					textTranslationPoint = { x = x + 216, y = y + 28 },
					horizontalAlignment = "center",
					renderOptions = "Image, Text",
					addToFront = true
				}
			)
		end
		
		local text_info = kernel.measureactortext( product_name )
		local min_price_max_price = ""
		min_price_max_price = get_min_max_price( catalog_entry.min_price, catalog_entry.max_price )
		
		_tickets_selection_grid_scene_manager:add_actor(
			product_name .. "_price",
			x,
			y + text_info["height"] + 158,
			204,
			25,
			{
				foregroundColor = "black",
				font = "font_akzidenz_10_regular",
				horizontalAlignment = "center",
				text = min_price_max_price,
				renderOptions = "Text",
				addToFront = true
			}
		)
		kernel.registeractorhitnotify( product_name,
			function()
				view_analytics_event( "buttonPress", "eventSelected," .. tostring(  catalog_entry.event_id ) )
				ShoppingCart.TicketsCart.event = catalog_entry
				view_show( "tickets_production_selection_view" )
			end
		)
		
		--render the previous top image
		if actorIndex == 1 and _tickets_browse_filtered_products[_tickets_browse_pager:get_index() - 5] ~= nil then
			local previous_top_catalog_entry = _tickets_browse_filtered_products[_tickets_browse_pager:get_index() - 5]
			_tickets_selection_grid_scene_manager:add_actor(
				"previous_top",
				-162,
				115,
				204,
				150,
				{
					renderOptions = "Image, DrawBorder",
					image = get_browse_event_poster( previous_top_catalog_entry ),
					borderColor = { r = "60", g = "56", b = "57" },
					addToFront = true
				}
			)
		end
		--render the previous bottom image
		if actorIndex == 5 and _tickets_browse_filtered_products[_tickets_browse_pager:get_index() - 5] ~= nil then
			local previous_bottom_catalog_entry = _tickets_browse_filtered_products[_tickets_browse_pager:get_index() - 5]
			_tickets_selection_grid_scene_manager:add_actor(
				"previous_bottom",
				-162,
				354,
				204,
				150,
				{
					renderOptions = "Image, DrawBorder",
					image = get_browse_event_poster( previous_bottom_catalog_entry ),
					borderColor = { r = "60", g = "56", b = "57" },
					addToFront = true
				}
			)
		end
		--render the next top image
		if actorIndex == 4 and _tickets_browse_filtered_products[_tickets_browse_pager:get_index() + 5] ~= nil then
			local next_top_catalog_entry = _tickets_browse_filtered_products[_tickets_browse_pager:get_index() + 5]
			_tickets_selection_grid_scene_manager:add_actor(
				"next_top",
				983,
				115,
				204,
				150,
				{
					renderOptions = "Image, DrawBorder",
					image = get_browse_event_poster( next_top_catalog_entry ),
					borderColor = { r = "60", g = "56", b = "57" },
					addToFront = true
				}
			)
		end
		--render the next bottom image
		if actorIndex == 8 and _tickets_browse_filtered_products[_tickets_browse_pager:get_index() + 5] ~= nil then
			local next_bottom_catalog_entry = _tickets_browse_filtered_products[_tickets_browse_pager:get_index() + 5]
			_tickets_selection_grid_scene_manager:add_actor(
				"next_bottom",
				983,
				354,
				204,
				150,
				{
					renderOptions = "Image, DrawBorder",
					image = get_browse_event_poster( next_bottom_catalog_entry ),
					borderColor = { r = "60", g = "56", b = "57" },
					addToFront = true
				}
			)
		end
		
		if (actorIndex % 4) == 0 then
			y = y + 239
			x = 67
		else
			x = x + 229
		end
		
		events_shown = actorIndex
		
		if actorIndex == 8 then
			break
		end
		
		actorIndex = actorIndex + 1
		_tickets_browse_pager:increment_index()
	end
	
	_tickets_browse_pager:update_paging( #_tickets_browse_filtered_products )
	view_analytics_event( "eventsShown", tostring( events_shown ) )
end

function tickets_show_filtered_grid()
	refresh_views()
	tickets_show_selection_grid()
end