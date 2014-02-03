--
-- tickets-choose-quantity-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local function haspricecodes()
	return ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId ~= 0
end

local function hideticket2()
	kernel.hideactor( "ticket_2" )
	kernel.hideactor( "content_rating_short_note_label_2" )
	kernel.hideactor( "supplier_price_label_2" )
	kernel.hideactor( "content_rating_type_name_label_2" )
	kernel.hideactor( "ticket_quantity_field_2" )
	kernel.hideactor( "plus_button_2" )
	kernel.hideactor( "minus_button_2" )
end

local function showticket2()
	kernel.showactor( "ticket_2" )
	kernel.showactor( "content_rating_short_note_label_2" )
	kernel.showactor( "supplier_price_label_2" )
	kernel.showactor( "content_rating_type_name_label_2" )
	kernel.showactor( "ticket_quantity_field_2" )
	kernel.showactor( "plus_button_2" )
	kernel.showactor( "minus_button_2" )
end

local production_venue = Venues[ShoppingCart.TicketsCart.production.venue_id]

kernel.setactortext( "ticket_quantity_field_1", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId ) ) )
if ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2] ~= nil then
	kernel.setactortext( "ticket_quantity_field_2", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2].PriceCodeId ) ) )
end

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

	kernel.setactory( "price_label", 172 )
	kernel.setactory( "price_text", 172 )
	
	kernel.setactory( "venue_label", 197 )
	kernel.setactory( "venue_text", 197 )
end

local price_text = get_min_max_price( ShoppingCart.TicketsCart.production.min_price, ShoppingCart.TicketsCart.production.max_price )
kernel.setmacro( "ChooseQuantity:Price", price_text )

local venue_text = production_venue.venue_name .. "\n" .. production_venue.address1 .. " " .. production_venue.city .. ", " .. production_venue.state
kernel.setmacro( "ChooseQuantity:Venue", venue_text )

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionShortNote" ) ~= nil then
	kernel.showactor( "production_short_note" )
	kernel.setactortext( "production_short_note", ShoppingCart.TicketsCart.production.short_note )
end

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

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionVenueBundle" ) ~= nil then
	local production_venue_bundle_scene_manager = ShoppingCart.TicketsCart:get_production_venue_bundle_as_scene_manager( ShoppingCart.TicketsCart.production.production_id, 523, 121, 410, 160 )
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideProductionDetails" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideVenueDetails" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowEventShortNote" ) ~= nil then
	kernel.showactor( "event_short_note" )
	kernel.setactortext( "event_short_note", ShoppingCart.TicketsCart.event.short_note )
end

if haspricecodes() == false then
	local price_code = ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1]
	local production_content_rating = ContentRating[ShoppingCart.TicketsCart.production.content_rating_type_id]

	--content rating short note
	local rating_short_note_text = ""
	if production_content_rating.content_rating_short_note ~= nil and production_content_rating.content_rating_short_note ~= "" then
		rating_short_note_text = production_content_rating.content_rating_short_note .. ":"
	end
	kernel.setmacro( "ChooseQuantity:ContentRatingShortNote1", rating_short_note_text )

	--min - max
	kernel.setmacro( "ChooseQuantity:SupplierPrice1", ShoppingCart.TicketsCart.PriceCodeManager:get_min_max_price_range_text( price_code.MinSupplierPricePerTicket, price_code.MaxSupplierPricePerTicket ) )

	--content rating type name
	local content_rating_type_name_text = ""
	if production_content_rating.content_rating_type_name ~= nil and production_content_rating.content_rating_type_name ~= "" then
		content_rating_type_name_text = "(" .. production_content_rating.content_rating_type_name .. ")"
	end
	kernel.setmacro( "ChooseQuantity:ContentRatingTypeName1", content_rating_type_name_text )
else
	local price_codes = ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes

	--ticket 1 price code name
	kernel.setmacro( "ChooseQuantity:ContentRatingShortNote1", price_codes[1].PriceCodeName .. ":" )

	--ticket 1 min - max
	kernel.setmacro( "ChooseQuantity:SupplierPrice1", ShoppingCart.TicketsCart.PriceCodeManager:get_min_max_price_range_text( price_codes[1].MinSupplierPricePerTicket, price_codes[1].MaxSupplierPricePerTicket ) )

	--ticket 1 price code short note
	local price_code_short_note_text = ""
	if price_codes[1].PriceCodeShortNote ~= nil	and price_codes[1].PriceCodeShortNote ~= "" then
		price_code_short_note_text = "(" .. price_codes[1].PriceCodeShortNote .. ")"
	end
	kernel.setmacro( "ChooseQuantity:ContentRatingTypeName1", price_code_short_note_text )

	if price_codes[2] ~= nil then
		showticket2()

		--ticket 2 price code name
		kernel.setmacro( "ChooseQuantity:ContentRatingShortNote2", price_codes[2].PriceCodeName .. ":" )

		--ticket 2 min - max
		kernel.setmacro( "ChooseQuantity:SupplierPrice2", ShoppingCart.TicketsCart.PriceCodeManager:get_min_max_price_range_text( price_codes[2].MinSupplierPricePerTicket, price_codes[2].MaxSupplierPricePerTicket ) )

		--ticket 2 price code short note
		local price_code_short_note_text = ""
		if price_codes[2].PriceCodeShortNote ~= nil	and price_codes[2].PriceCodeShortNote ~= "" then
			price_code_short_note_text = "(" .. price_codes[2].PriceCodeShortNote .. ")"
		end
		kernel.setmacro( "ChooseQuantity:ContentRatingTypeName2", price_code_short_note_text )
	else
		hideticket2()
	end
end

kernel.registeractorhitnotify(
	"plus_button_1",
	function()
		view_analytics_event( "buttonPress", "qty_plus_button" )
		ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId, 1, true )
		kernel.markactordirty( "ticket_quantity_field_1" )
		kernel.setactortext( "ticket_quantity_field_1", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId ) ) )
	end
)

kernel.registeractorhitnotify(
	"minus_button_1",
	function()
		view_analytics_event( "buttonPress", "qty_minus_button" )
		if ( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2] == nil and ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId ) > 1 ) or ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2] ~= nil then
			ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId, -1 )
			kernel.markactordirty( "ticket_quantity_field_1" )
			kernel.setactortext( "ticket_quantity_field_1", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[1].PriceCodeId ) ) )
		end
	end
)

if ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2] ~= nil then
	kernel.registeractorhitnotify(
		"plus_button_2",
		function()
			view_analytics_event( "buttonPress", "qty_plus_button" )
			ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2].PriceCodeId, 1, true )
			kernel.markactordirty( "ticket_quantity_field_2" )
			kernel.setactortext( "ticket_quantity_field_2", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2].PriceCodeId ) ) )
		end
	)

	kernel.registeractorhitnotify(
		"minus_button_2",
		function()
			view_analytics_event( "buttonPress", "qty_minus_button" )
			ShoppingCart.TicketsCart.PriceCodeManager:add_to_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2].PriceCodeId, -1 )
			kernel.markactordirty( "ticket_quantity_field_2" )
			kernel.setactortext( "ticket_quantity_field_2", tostring( ShoppingCart.TicketsCart.PriceCodeManager:get_working_price_code_quantity( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes[2].PriceCodeId ) ) )
		end
	)
end

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

local function choose_seats_button_handler()
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
		MessageView:start_timer(
			function()
				MessageView:hide()
			end
		)
		return
	end
	view_show( "tickets_find_tickets_view" )
end

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