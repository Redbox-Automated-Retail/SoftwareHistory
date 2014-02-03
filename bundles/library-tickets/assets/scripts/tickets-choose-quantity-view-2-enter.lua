--
-- tickets-choose-quantity-view-2-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local production_venue = Venues[ShoppingCart.TicketsCart.production.venue_id]

local event_text = ShoppingCart.TicketsCart.event.sort_name
kernel.setmacro( "ChooseQuantity:EventName", event_text )

local ticket_image_to_draw = "image_tickets_no_image_available"
local event_poster = kernel.ticketsvcs_registerimagefromcache( 1, 1, ShoppingCart.TicketsCart.event.event_id )
if event_poster ~= nil then
	ticket_image_to_draw = string.format( "cache:%s", event_poster )
end
kernel.setactorimage( "event_art", ticket_image_to_draw )

if ShoppingCart.TicketsCart.production.secondary_event_id ~= nil then
	kernel.setmacro( "ChooseQuantity:WithLabel", "With:" )
	local secondary_event = Events[ShoppingCart.TicketsCart.production.secondary_event_id]
	if secondary_event.primary_category_id == 2
	and ShoppingCart.TicketsCart.event.primary_category_id == 2 then
		kernel.setmacro( "ChooseQuantity:WithLabel", "VS.:" )
	end
	kernel.setmacro( "ChooseQuantity:WithText", secondary_event.sort_name )
	kernel.showactor( "with_label" )
	kernel.showactor( "with_text" )
else
	kernel.setactory( "date_label", 120 )
	kernel.setactory( "date_text", 120 )
	kernel.setactory( "time_label", 145 )
	kernel.setactory( "time_text", 145 )
	kernel.setactory( "price_label", 170 )
	kernel.setactory( "price_text", 170 )
	kernel.setactory( "venue_label", 195 )
	kernel.setactory( "venue_text", 195 )
end

if ShoppingCart.TicketsCart.production.production_end_date_time_local ~= nil then
	local event_date_time = parseDateTime( ShoppingCart.TicketsCart.production.production_end_date_time_local )

	local date_text = os.date( "%a. %m/%d", event_date_time )
	kernel.setmacro( "ChooseQuantity:Date", date_text )

	local time_text = os.date( "%I:%M %p", event_date_time )
	time_text = string.format( "%s %s", time_text, production_venue.timezone_name )
	kernel.setmacro( "ChooseQuantity:Time", time_text )
else
	kernel.setmacro( "ChooseQuantity:Date", "" )
	kernel.setmacro( "ChooseQuantity:Time", "" )
end

local attributes = ShoppingCart.TicketsCart:get_attributes( ShoppingCart.TicketsCart.production )

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil then
	kernel.setmacro( "ChooseQuantity:Date", ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value )
end

local production_property_data = ShoppingCart.TicketsCart:get_production_property_data( ShoppingCart.TicketsCart.production.production_id )
if ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "ProductionTimeDescriptor" ) ~= nil then
	kernel.setmacro( "ChooseQuantity:Time", ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "ProductionTimeDescriptor" ).value )
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideProductionTime" ) ~= nil then
	kernel.hideactor( "time_label" )
	kernel.setmacro( "ChooseQuantity:Time", "" )

	kernel.moveactory( "price_label", -25 )
	kernel.moveactory( "price_text", -25 )
	
	kernel.moveactory( "venue_label", -25 )
	kernel.moveactory( "venue_text", -25 )
end

local price_text = get_min_max_price( ShoppingCart.TicketsCart.production.min_price, ShoppingCart.TicketsCart.production.max_price )
kernel.setmacro( "ChooseQuantity:Price", price_text )

local venue_text = production_venue.venue_name .. "\n" .. production_venue.address1 .. " " .. production_venue.city .. ", " .. production_venue.state .. ", " ..production_venue.zip
kernel.setmacro( "ChooseQuantity:Venue", venue_text )

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideProductionDetails" ) ~= nil then
	kernel.hideactor( "with_label" )
	kernel.hideactor( "with_text" )
	kernel.hideactor( "date_label" )
	kernel.hideactor( "date_text" )
	kernel.hideactor( "time_label" )
	kernel.hideactor( "time_text" )
	kernel.hideactor( "price_label" )
	kernel.hideactor( "price_text" )
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideVenueDetails" ) ~= nil then
	kernel.hideactor( "venue_label" )
	kernel.hideactor( "venue_text" )
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideProductionDetails" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideVenueDetails" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowEventShortNote" ) ~= nil then
	kernel.showactor( "event_short_note" )
	kernel.setactortext( "event_short_note", ShoppingCart.TicketsCart.event.short_note )
end

local tickets_choose_quantity_scene_manager = newscenemanager()
draw_price_code_selector = function( x, y, pc )
	--draw selector background
	tickets_choose_quantity_scene_manager:add_actor(
		"selector_background_image_".. pc.PriceCodeId,
		x,
		y,
		175,
		277,
		{
			renderOptions = "Image",
			image = "image_price_code_selector_background"
		}
	)
	--draw pc_name
	tickets_choose_quantity_scene_manager:add_actor(
		"price_code_name_".. pc.PriceCodeId,
		x,
		y + 30,
		175,
		30,
		{
			renderOptions = "Text",
			text = string.upper( pc.PriceCodeName ).. ":",
			foregroundColor = { r = "60", g = "57", b = "57" },
			font = "font_target_22_regular",
			horizontalAlignment = "center",
			verticalAlignment = "center"
		}
	)
	--draw pc_price
	local price_text = ShoppingCart.TicketsCart.PriceCodeManager:get_min_max_price_range_text( pc.MinSupplierPricePerTicket, pc.MaxSupplierPricePerTicket )
	tickets_choose_quantity_scene_manager:add_actor(
		"price_code_value_".. pc.PriceCodeId,
		x,
		y + 55,
		175,
		30,
		{
			renderOptions = "Text",
			text = string.format( "%s", tostring( price_text ) ),
			foregroundColor = { r = "60", g = "57", b = "57" },
			font = "font_target_17_regular",
			horizontalAlignment = "center",
			verticalAlignment = "center"
		}
	)
	--draw pc_short_note
	if pc.PriceCodeShortNote ~= nil
	and pc.PriceCodeShortNote ~= "" then
		tickets_choose_quantity_scene_manager:add_actor(
			"price_code_short_note_".. pc.PriceCodeId,
			x + 5,
			y + 85,
			163,
			35,
			{
				renderOptions = "Text, ElideText",
				text = string.format( "(%s)", pc.PriceCodeShortNote ),
				foregroundColor = { r = "0", g = "0", b = "0" },
				font = "font_akzidenz_11_regular",
				horizontalAlignment = "center",
				verticalAlignment = "center"
			}
		)
	end
	--draw quantity
	tickets_choose_quantity_scene_manager:add_actor(
		"ticket_quantity_field_".. pc.PriceCodeId,
		x + 61,
		y + 125,
		57,
		48,
		{
			renderOptions = "Text",
			text = pc.Quantity,
			foregroundColor = { r = "0", g = "0", b = "0" },
			font = "font_target_21_regular",
			horizontalAlignment = "center",
			verticalAlignment = "center"
		}
	)
	--draw plus_button
	tickets_choose_quantity_scene_manager:add_actor(
		"plus_button_".. pc.PriceCodeId,
		x + 17,
		y + 190,
		53,
		53,
		{
			renderOptions = "Text, DrawBorder, BackgroundGradient",
			foregroundColor = { r = "255", g = "255", b = "255" },
			backgroundColor = { r = "0", g = "154", b = "51" },
			gradientTargetColor = { r = "0", g = "105", b = "35" },
			gradientAngle = 90.0,
			borderColor = { r = "255", g = "255", b = "255" },
			strokeWeight = 2.0,
			cornerSize = { width = "26", height = "26" },
			font = "font_target_35_regular",
			textRegion = { x = "12", y = "8", width = "30", height = "30" },
			horizontalAlignment = "center",
			verticalAlignment = "center",
			text="+",
			hitOptions = "Enabled"
		},
		function()
			view_analytics_event( "buttonPress", "qty_plus_button_".. pc.PriceCodeId )
			ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( pc.PriceCodeId, 1, true )
			kernel.setactortext( "ticket_quantity_field_".. pc.PriceCodeId, tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( pc.PriceCodeId ) ) )
		end
	)
	--draw minus_button
	tickets_choose_quantity_scene_manager:add_actor(
		"minus_button_".. pc.PriceCodeId,
		x + 105,
		y + 190,
		53,
		53,
		{
			renderOptions = "Text, DrawBorder, BackgroundGradient",
			foregroundColor = { r = "255", g = "255", b = "255" },
			backgroundColor = { r = "226", g = "120", b = "18" },
			gradientTargetColor = { r = "177", g = "77", b = "1" },
			gradientAngle = 90.0,
			borderColor = { r = "255", g = "255", b = "255" },
			strokeWeight = 2.0,
			cornerSize = { width = "26", height = "26" },
			font = "font_target_35_regular",
			textRegion = { x = "12", y = "8", width = "30", height = "30" },
			horizontalAlignment = "center",
			verticalAlignment = "center",
			text="-",
			hitOptions = "Enabled"
		},
		function()
			view_analytics_event( "buttonPress", "qty_minus_button_".. pc.PriceCodeId )
			ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( pc.PriceCodeId, -1 )
			kernel.setactortext( "ticket_quantity_field_".. pc.PriceCodeId, tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( pc.PriceCodeId ) ) )
		end
	)
end

if price_codes_pager == nil then
	price_code_pager = newpager()

	price_code_pager.prev_changed = function( enabled )
		if enabled == true then
			kernel.enableactor( "price_code_pager_up" )
		else
			kernel.disableactor( "price_code_pager_up" )
		end
	end

	price_code_pager.next_changed = function( enabled )
		if enabled == true then
			kernel.enableactor( "price_code_pager_down" )
		else
			kernel.disableactor( "price_code_pager_down" )
		end
	end
end

draw_production_price_codes = function()
	tickets_choose_quantity_scene_manager:clear()

	if price_code_pager.is_valid == false then
		price_code_pager:reset()
	end

	local total_pages = math.ceil( ShoppingCart.TicketsCart.PriceCodeManager:count() / 4 )

	if total_pages > 0 then
		while price_code_pager:page_number() > total_pages do
			price_code_pager:page_back()
		end
	end

	if total_pages < 2 then
		kernel.hideactor( "price_code_pager_background" )
		kernel.hideactor( "price_code_pager_up" )
		kernel.hideactor( "price_code_pager_down" )
		kernel.hideactor( "price_code_page_label" )
	else
		kernel.setmacro( "PriceCode:Page", tostring( price_code_pager:page_number() ) )
		kernel.setmacro( "PriceCode:TotalPages", total_pages )
		kernel.showactor( "price_code_pager_background" )
		kernel.showactor( "price_code_pager_up" )
		kernel.showactor( "price_code_pager_down" )
		kernel.showactor( "price_code_page_label" )
	end

	price_code_pager:mark_current_page()

	local xStart = 75
	local yStart = 353

	local count_items_to_draw_on_current_page = ShoppingCart.TicketsCart.PriceCodeManager:count() - price_code_pager.current_index

	x = xStart
	y = yStart

	local count = 1
	for k, pc in ipairs( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes ) do
		if price_code_pager:get_index() > ShoppingCart.TicketsCart.PriceCodeManager:count() then
			break
		end

		local current_price_code = ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[price_code_pager:get_index()]
		draw_price_code_selector( x, y, current_price_code )
		x = x + 190

		if count == 4 then
			break
		end
		count = count + 1

		price_code_pager:increment_index()
	end
	price_code_pager:update_paging( ShoppingCart.TicketsCart.PriceCodeManager:count() )
end

local choose_seats_button_handler = function()
	if ShoppingCart.TicketsCart.PriceCodeManager:get_total_tickets_quantity() < 1 then
		local MessageView = newmessageviewmanager()
		MessageView:show()
		MessageView:set_title( tickets_strings.min_quantity_message_title )
		MessageView:set_message( tickets_strings.min_quantity_message )
		MessageView:show_second_button(
			"OK",
			function()
				MessageView:hide()
			end
		)
		MessageView:start_timer( function() MessageView:hide() end )
		return
	end
	view_show( "tickets_find_tickets_view" )
end

kernel.registeractorhitnotify(
	"price_code_pager_up",
	function()
		price_code_pager:page_back()
		draw_production_price_codes()
	end
)

kernel.registeractorhitnotify(
	"price_code_pager_down",
	function()
		price_code_pager:page_next()
		draw_production_price_codes()
	end
)

kernel.registeractorhitnotify(
	"browse_button",
	function()
		view_analytics_event( "buttonPress", "browse_button" )
		view_unwindto( "tickets_browse_view", true, "ChooseQuantity: User hit browse button." )
	end
)

kernel.registeractorhitnotify(
	"browse_button_back_arrow",
	function()
		view_analytics_event( "buttonPress", "browse_button" )
		view_unwindto( "tickets_browse_view", true, "ChooseQuantity: User hit browse button." )
	end
)

kernel.registeractorhitnotify(
	"choose_seats_button",
	function()
		view_analytics_event( "buttonPress", "choose_seats_button" )
		choose_seats_button_handler()
	end
)

kernel.registeractorhitnotify(
	"choose_seats_button_next_arrow",
	function()
		view_analytics_event( "buttonPress", "choose_seats_button" )
		choose_seats_button_handler()
	end
)

kernel.registeractorhitnotify(
	"change_date_button",
	function()
		view_analytics_event( "buttonPress", "change_date_button" )
		view_unwindto( "tickets_production_selection_view", true, "ChooseQuantity: User hit Change Date button." )
	end
)

kernel.registeractorhitnotify(
	"venue_info_button",
	function()
		view_analytics_event( "buttonPress", "venue_info_button" )
		view_show( "tickets_venue_detail_view" )
	end
)

draw_production_price_codes()