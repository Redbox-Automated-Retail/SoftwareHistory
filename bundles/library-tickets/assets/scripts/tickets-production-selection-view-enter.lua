--
-- tickets-production-selection-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local production_venue_bundle_scene_manager = newscenemanager()

local function format_production_information( production )
	local function format_datetime_string( datetime_string, hide_time )
		local datetime = {
			year = string.sub( datetime_string, 1, 4 ),
			month = string.sub( datetime_string, 5, 6 ),
			day = string.sub( datetime_string, 7, 8 ),
			hour = string.sub( datetime_string, 9, 10 ),
			min = string.sub( datetime_string, 11, 12 ),
			sec = string.sub( datetime_string, 13, 14 )
		}
		if hide_time == true then
			return os.date( "%a. %m/%d", os.time( datetime ) )
		else
			return os.date( "%a. %m/%d %I:%M %p", os.time( datetime ) )
		end
	end

	local function get_secondary_event( production )
		local _Events = Events
		if production.secondary_event_id ~= nil then
			local article = "with"
			if _Events[production.event_id].primary_category_id == 2 and _Events[production.event_id].primary_category_id == _Events[production.secondary_event_id].primary_category_id then
				article = "vs."
			end
			return article .. " " .. _Events[production.secondary_event_id].sort_name
		end
		return ""
	end
	
	local _Venues = Venues
	local attributes = ShoppingCart.TicketsCart:get_attributes( production )
	local production_property_data = ShoppingCart.TicketsCart:get_production_property_data( production.production_id )
	
	local hide_production_time = false
	if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideProductionTime" ) ~= nil then
		hide_production_time = true
	end
	
	local datetime_text = ""
	if production.production_start_date_time_local ~= nil and production.production_end_date_time_local ~= nil then
		local current_datetime = os.date("*t")
		local current_datetime_string = string.format( "%4d%02d%02d%02d%02d%02d", current_datetime.year, current_datetime.month, current_datetime.day, current_datetime.hour, current_datetime.min, current_datetime.sec )
		if current_datetime_string >= production.production_start_date_time_local and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ).value ~= nil then
			datetime_text = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ).value .. " " .. format_datetime_string( production.production_end_date_time_local, hide_production_time )
			if hide_production_time == false then
				datetime_text = datetime_text .. " " .. _Venues[production.venue_id].timezone_name
			end
		elseif ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= nil then
			datetime_text = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value
		else
			datetime_text = format_datetime_string( production.production_start_date_time_local, hide_production_time )
			if hide_production_time == false then
				datetime_text = datetime_text .. " " .. _Venues[production.venue_id].timezone_name
			end
			datetime_text = datetime_text .. " - " .. format_datetime_string( production.production_end_date_time_local, hide_production_time )
			if hide_production_time == false then
				datetime_text = datetime_text .. " " .. _Venues[production.venue_id].timezone_name
			end
		end
	elseif production.production_start_date_time_local ~= nil then
		if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= nil then
			datetime_text = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value
		else
			datetime_text = format_datetime_string( production.production_start_date_time_local, hide_production_time )
			if hide_production_time == false then
				datetime_text = datetime_text .. " " .. _Venues[production.venue_id].timezone_name
			end
		end
	elseif production.production_end_date_time_local ~= nil then
		if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= nil then
			datetime_text = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value
		else
			datetime_text = format_datetime_string( production.production_end_date_time_local, hide_production_time )
			if hide_production_time == false then
				datetime_text = datetime_text .. " " .. _Venues[production.venue_id].timezone_name
			end
		end
	else
		if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= nil then
			datetime_text = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ).value
		end
	end
	
	if production.production_type_id == 3 and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ) ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ).value ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ).value ~= "" then
		datetime_text = datetime_text .. "*"
	end
	
	local with_text = get_secondary_event( production )
	if ( ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ) ~= nil or ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil ) and ( production.min_price ~= nil or production.max_price ~= nil ) and ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "ProductionTimeDescriptor" ) ~= nil then
		with_text = ""
	end
	
	local production_information = {}
	production_information["datetime"] = datetime_text
	production_information["price_range"] = get_min_max_price( production.min_price, production.max_price )
	production_information["venue"] = _Venues[production.venue_id].venue_name
	production_information["with"] = with_text
	
	if ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "ProductionTimeDescriptor" ) ~= nil then
		production_information["venue"] = ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "ProductionTimeDescriptor" ).value
		production_information["with"] = _Venues[production.venue_id].venue_name
	end
	
	return production_information
end

local arrange_short_note_and_content_rating = function( short_note, content_rating )
	kernel.setactortext( "event_short_note", short_note )
	if content_rating ~= nil and content_rating ~= "" then
		local short_note_text_dimensions = kernel.measureactortext( "event_short_note" )
		local short_note_text = short_note or ""
		if short_note_text_dimensions.lines_filled > 7 then
			while short_note_text_dimensions.lines_filled > 7 do
				short_note_text = string.sub( short_note_text, 0, string.len( short_note_text ) - 5 ) .. "..."
				kernel.setactortext( "event_short_note", short_note_text )
				short_note_text_dimensions = kernel.measureactortext( "event_short_note" )
			end
		end
		short_note_text = short_note_text .. "\n\n" .. content_rating
		kernel.setactortext( "event_short_note", short_note_text )
	end
end

function tickets_show_production_selection_grid()
	local _tickets_production_selection_productions = tickets_production_selection_productions
	local _tickets_production_grid_scene_manager = tickets_production_grid_scene_manager
	local _tickets_production_pager = tickets_production_pager

	_tickets_production_grid_scene_manager:clear()
	local x = 517
	local y = 196
	local actorIndex = 1

	if _tickets_production_pager.is_valid == false then
		_tickets_production_pager:reset()
	end

	local total_pages = math.ceil( #_tickets_production_selection_productions / 3 )
	if kernel.peekview() == "tickets_production_selection_view" then
		view_analytics_event( "productionsShown", tostring( #_tickets_production_selection_productions ))
	end

	if total_pages > 0 then
		while _tickets_production_pager:page_number() > total_pages do
			_tickets_production_pager:page_back()
		end
	end

	if total_pages == 0 then
		kernel.hideactor( "page_label" )
	else
		kernel.setmacro( "Page", tostring( _tickets_production_pager:page_number() ) )
		kernel.setmacro( "TotalPages", total_pages )
		kernel.showactor( "page_label" )
	end

	_tickets_production_pager:mark_current_page()

	if #_tickets_production_selection_productions == 0 then
		kernel.hideactor( "available_dates" )
		kernel.hideactor( "top_panel" )
		kernel.hideactor( "middle_panel" )
		kernel.hideactor( "bottom_panel" )
		kernel.hideactor( "up_arrow" )
		kernel.hideactor( "down_arrow" )
		kernel.hideactor( "page_label" )
		kernel.hideactor( "current_selection_heading" )
		kernel.hideactor( "next_button" )
		kernel.hideactor( "next_button_next_arrow" )
		kernel.hideactor( "email_info_button" )
		_tickets_production_grid_scene_manager:add_actor(
			"inventory_message",
			517,
			142,
			371,
			378,
			{
				font = "font_akzidenz_32_bold",
				renderOptions = "Text, TextShadow",
				foregroundColor = { r = "224", g = "38", b = "44" },
				text = "We're sorry, this event has sold out! Please touch the browse button to browse for other events"
			}
		)
	end

	local _Productions = Productions
	for k, _ in ipairs( _tickets_production_selection_productions ) do
		if _tickets_production_pager:get_index() > #_tickets_production_selection_productions then
			break
		end

		local current_production_id = _tickets_production_selection_productions[_tickets_production_pager:get_index()]
		local production = _Productions[current_production_id]

		local button_name = "button_" .. actorIndex
		_tickets_production_grid_scene_manager:add_actor(
			button_name,
			x,
			y,
			371,
			126,
			{
				hitOptions = "Enabled"
			}
		)

		local checkmark_image = "image_gray_checkmark"
		if ShoppingCart.TicketsCart.production ~= nil and ShoppingCart.TicketsCart.production.production_id == production.production_id then
			checkmark_image = "image_red_checkmark"
			if ShoppingCart.TicketsCart.production.content_rating_type_id > 1 then
				arrange_short_note_and_content_rating(
					ShoppingCart.TicketsCart.event.short_note,
					ContentRating[ShoppingCart.TicketsCart.production.content_rating_type_id].content_rating_type_name
				)
			end
		end
		local checkmark_name = "checkmark_" .. actorIndex
		_tickets_production_grid_scene_manager:add_actor(
			checkmark_name,
			x + 15,
			y + 25,
			55,
			55,
			{
				image = checkmark_image,
				renderOptions = "Image"
			}
		)

		kernel.registeractorhitnotify( button_name,
			function()
				view_analytics_event( "buttonPress", "productionSelected," .. tostring( production.production_id ) )
				for i = 1, 3 do
					kernel.setactorimage( "checkmark_" .. i, "image_gray_checkmark" )
				end
				kernel.setactorimage( checkmark_name, "image_red_checkmark" )
				ShoppingCart.TicketsCart.production = production
				ShoppingCart.TicketsCart.PriceCodeManager:load_working_price_codes_for_selected_production( ShoppingCart.TicketsCart.production )
				ShoppingCart.TicketsCart.get_tickets_call_made = false
				
				local production_information = format_production_information( production )
				kernel.setactortext( "current_selection_line_1", production_information["datetime"] )
				kernel.setactortext( "current_selection_line_2", production_information["price_range"] )
				kernel.setactortext( "current_selection_line_3", production_information["venue"] )
				kernel.setactortext( "current_selection_line_4", production_information["with"] )
				
				if ShoppingCart.TicketsCart.production.content_rating_type_id > 1 then
					arrange_short_note_and_content_rating(
						ShoppingCart.TicketsCart.event.short_note,
						ContentRating[ShoppingCart.TicketsCart.production.content_rating_type_id].content_rating_type_name
					)
				else
					arrange_short_note_and_content_rating( ShoppingCart.TicketsCart.event.short_note )
				end
				
				local attributes = ShoppingCart.TicketsCart:get_attributes( production )
				local production_property_data = ShoppingCart.TicketsCart:get_production_property_data( production.production_id )
				
				if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionShortNote" ) ~= nil and production.short_note ~= nil then
					kernel.hideactor( "event_short_note" )
					kernel.showactor( "production_short_note" )
					kernel.setactortext( "production_short_note", production.short_note )
				else
					kernel.showactor( "event_short_note" )
					kernel.hideactor( "production_short_note" )
				end
				
				local show_blackout_dates = true
				if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionVenueBundle" ) ~= nil then
					kernel.hideactor( "event_short_note" )
					production_venue_bundle_scene_manager:clear()
					production_venue_bundle_scene_manager, show_blackout_dates = ShoppingCart.TicketsCart:get_production_venue_bundle_as_scene_manager( production.production_id, 75, 442, 410, 160 )
				else
					kernel.showactor( "event_short_note" )
					production_venue_bundle_scene_manager:clear()
				end
				
				if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideMoreDatesText" ) ~= nil then
					kernel.hideactor( "event_future_dates" )
				else
					kernel.showactor( "event_future_dates" )
				end
				
				if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ) ~= nil then
					local production_selection_note = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ).value or ""
					if production.production_type_id == 3 and production_selection_note ~= "" then
						kernel.showactor( "event_future_dates" )
						kernel.setactortext( "event_future_dates", "*" .. production_selection_note )
					else
						kernel.showactor( "event_future_dates" )
						kernel.setactortext( "event_future_dates", production_selection_note )
					end
				else
					kernel.setactortext( "event_future_dates", tickets_strings.event_future_dates )
				end
				
				if ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "BlackoutDates" ) ~= nil and show_blackout_dates == true then
					kernel.setactortext( "blackout_dates_text", ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "BlackoutDates" ).value )
				else
					kernel.setactortext( "blackout_dates_text", "" )
				end
			end
		)

		local production_information = format_production_information( production )
		_tickets_production_grid_scene_manager:add_actor(
			"production_info_line_1_" .. actorIndex,
			x + 85,
			y + 25,
			275,
			22,
			{
				font = "font_akzidenz_14_bold",
				foregroundColor = "black",
				text = production_information["datetime"],
				renderOptions = "Text, ElideText"
			}
		)
		_tickets_production_grid_scene_manager:add_actor(
			"production_info_line_2_" .. actorIndex,
			x + 85,
			y + 47,
			275,
			22,
			{
				font = "font_akzidenz_14_regular",
				foregroundColor = "black",
				text = production_information["price_range"],
				renderOptions = "Text, ElideText"
			}
		)
		_tickets_production_grid_scene_manager:add_actor(
			"production_info_line_3_" .. actorIndex,
			x + 85,
			y + 69,
			275,
			22,
			{
				font = "font_akzidenz_14_regular",
				foregroundColor = "black",
				text = production_information["venue"],
				renderOptions = "Text, ElideText"
			}
		)
		_tickets_production_grid_scene_manager:add_actor(
			"production_info_line_4_" .. actorIndex,
			x + 85,
			y + 91,
			275,
			22,
			{
				font = "font_akzidenz_14_regular",
				foregroundColor = "black",
				text = production_information["with"],
				renderOptions = "Text, ElideText"
			}
		)
		y = y + 126

		if actorIndex == 3 then
			break
		end

		actorIndex = actorIndex + 1
		_tickets_production_pager:increment_index()
	end

	_tickets_production_pager:update_paging( #_tickets_production_selection_productions )
end

if ShoppingCart.TicketsCart.production == nil then
	tickets_production_grid_scene_manager = newscenemanager()

	tickets_production_pager = newpager()
	tickets_production_pager.prev_changed = function( enabled )
		if enabled == true then
			kernel.enableactor( "up_arrow" )
		else
			kernel.disableactor( "up_arrow" )
		end
	end
	tickets_production_pager.next_changed = function( enabled )
		if enabled == true then
			kernel.enableactor( "down_arrow" )
		else
			kernel.disableactor( "down_arrow" )
		end
	end
end

kernel.setactortext( "event_name", ShoppingCart.TicketsCart.event.sort_name )

local ticket_image_to_draw = "image_tickets_no_image_available"
local event_poster = kernel.ticketsvcs_registerimagefromcache( 1, 1, ShoppingCart.TicketsCart.event.event_id )
if event_poster ~= nil then
	ticket_image_to_draw = string.format( "cache:%s", event_poster )
end
kernel.setactorimage( "event_image", ticket_image_to_draw )

arrange_short_note_and_content_rating( ShoppingCart.TicketsCart.event.short_note )

kernel.registeractorhitnotify( "next_button",
	function()
		view_analytics_event( "buttonPress", "next_button" )
		local selected_product_rating = ShoppingCart.TicketsCart.production.content_rating_type_id
		if selected_product_rating > 1 then
			local TicketsMessageView = newticketsmessageviewmanager()
			TicketsMessageView:show()
			view_analytics_event( "context", "ageRestrictionWarning" )
			TicketsMessageView:set_message_header( ContentRating[selected_product_rating].content_rating_type_name .." Event")
			TicketsMessageView:set_message( string.format( tickets_strings.age_restrictions_text, ContentRating[selected_product_rating].content_rating_short_note, ContentRating[selected_product_rating].content_rating_type_name ) )
			TicketsMessageView:show_first_button(
				tickets_strings.CANCEL,
				function()
					view_analytics_event( "buttonPress", "age-restriction-view-cancel" )
					view_unwindto( "tickets_production_selection_view", true, "ProductionSelection:User hit browse button." )
				end
			)
			TicketsMessageView:show_second_button(
				tickets_strings.OK,
				function()
					view_analytics_event( "buttonPress", "age-restriction-view-continue" )
					view_show( choose_quantity_view_name )
				end
			)
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "age-restriction-view timeout" )
					view_unwindto( "tickets_production_selection_view", true, "ProductionSelection:User hit browse button." )
				end
			)
		else
			view_show( choose_quantity_view_name )
		end
	end
)

refresh_views()
tickets_production_selection_productions = {}
local _Productions = Productions

if EventToProductions[ShoppingCart.TicketsCart.event.event_id] == nil
or EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids == nil
or #EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids == 0 then
	kernel.hideactor( "available_dates" )
	kernel.hideactor( "top_panel" )
	kernel.hideactor( "middle_panel" )
	kernel.hideactor( "bottom_panel" )
	kernel.hideactor( "up_arrow" )
	kernel.hideactor( "down_arrow" )
	kernel.hideactor( "page_label" )
	kernel.hideactor( "current_selection_heading" )
	kernel.hideactor( "next_button" )
	kernel.hideactor( "next_button_next_arrow" )
	kernel.hideactor( "email_info_button" )
	tickets_production_grid_scene_manager:add_actor(
		"inventory_message",
		517,
		142,
		371,
		378,
		{
			font = "font_akzidenz_32_bold",
			renderOptions = "Text, TextShadow",
			foregroundColor = { r = "224", g = "38", b = "44" },
			text = "We're sorry, this event has sold out! Please touch the browse button to browse for other events"
		}
	)
else
	for k, v in ipairs( EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids ) do
		table.insert( tickets_production_selection_productions, v )
	end
end

table.sort(
	tickets_production_selection_productions,
	function( a, b )
		return tostring( _Productions[a].production_end_date_time_local ) < tostring( _Productions[b].production_end_date_time_local )
	end
)

if ShoppingCart.TicketsCart.production == nil and tickets_production_selection_productions[1] ~= nil then
	ShoppingCart.TicketsCart.production = _Productions[tickets_production_selection_productions[1]]
	ShoppingCart.TicketsCart.PriceCodeManager:load_working_price_codes_for_selected_production( ShoppingCart.TicketsCart.production )
	ShoppingCart.TicketsCart.get_tickets_call_made = false
end

if ShoppingCart.TicketsCart.production ~= nil then
	local production_information = format_production_information( ShoppingCart.TicketsCart.production )
	kernel.setactortext( "current_selection_line_1", production_information["datetime"] )
	kernel.setactortext( "current_selection_line_2", production_information["price_range"] )
	kernel.setactortext( "current_selection_line_3", production_information["venue"] )
	kernel.setactortext( "current_selection_line_4", production_information["with"] )
	
	local attributes = ShoppingCart.TicketsCart:get_attributes( ShoppingCart.TicketsCart.production )
	local production_property_data = ShoppingCart.TicketsCart:get_production_property_data( ShoppingCart.TicketsCart.production.production_id )
	
	if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionShortNote" ) ~= nil and ShoppingCart.TicketsCart.production.short_note ~= nil then
		kernel.hideactor( "event_short_note" )
		kernel.showactor( "production_short_note" )
		kernel.setactortext( "production_short_note", ShoppingCart.TicketsCart.production.short_note )
	end
	
	local show_blackout_dates = true
	if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ShowProductionVenueBundle" ) ~= nil then
		kernel.hideactor( "event_short_note" )
		production_venue_bundle_scene_manager, show_blackout_dates = ShoppingCart.TicketsCart:get_production_venue_bundle_as_scene_manager( ShoppingCart.TicketsCart.production.production_id, 75, 442, 410, 160 )
	end
	
	if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideMoreDatesText" ) ~= nil then
		kernel.hideactor( "event_future_dates" )
	end
	
	if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ) ~= nil then
		local production_selection_note = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "ProductionSelectionNote" ).value or ""
		if ShoppingCart.TicketsCart.production.production_type_id == 3 and production_selection_note ~= "" then
			kernel.showactor( "event_future_dates" )
			kernel.setactortext( "event_future_dates", "*" .. production_selection_note )
		else
			kernel.showactor( "event_future_dates" )
			kernel.setactortext( "event_future_dates", production_selection_note )
		end
	else
		kernel.setactortext( "event_future_dates", tickets_strings.event_future_dates )
	end
	
	if ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "BlackoutDates" ) ~= nil and show_blackout_dates == true then
		kernel.setactortext( "blackout_dates_text", ShoppingCart.TicketsCart:get_production_property_data_by_name( production_property_data, "BlackoutDates" ).value )
	end
end

if EventToProductions[ShoppingCart.TicketsCart.event.event_id] ~= nil
and EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids ~= nil
and #EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids > 0 then
	ShoppingCart.TicketsCart:set_promotions( EventToProductions[ShoppingCart.TicketsCart.event.event_id].production_ids )
	if #ShoppingCart.TicketsCart.promotions > 0 then
		kernel.hideactor( "email_info_button" )
		kernel.showactor( "email_info_small_button" )
		kernel.showactor( "rules_button" )
	end
end

if ShoppingCart.TicketsCart:is_event_in_sent_list() == true then
	if #ShoppingCart.TicketsCart.promotions > 0 then
		kernel.hideactor( "email_info_small_button" )
		kernel.disableactor( "email_sent_info_small_button" )
		kernel.showactor( "email_sent_info_small_button" )
	else
		kernel.hideactor( "email_info_button" )
		kernel.disableactor( "email_sent_info_button" )
		kernel.showactor( "email_sent_info_button" )
	end
end

kernel.registeractorhitnotify(
	"browse_button",
	function()
		view_analytics_event( "buttonPress", "browse_button" )
		view_unwindto( "tickets_browse_view", true, "ProductionSelection:User hit browse button." )
	end
)

kernel.registeractorhitnotify(
	"browse_button_back_arrow",
	function()
		view_analytics_event( "buttonPress", "browse_button" )
		view_unwindto( "tickets_browse_view", true, "ProductionSelection:User hit browse button." )
	end
)

kernel.registeractorhitnotify(
	"rules_button",
	function()
		view_analytics_event( "buttonPress", "rules_button" )
		view_show( "tickets_promotional_rules_view" )
	end
)

kernel.registerkeypressnotify(
	"tickets_production_selection_view",
	function( key, keyCode, modifier )
		if modifier == "Control" and keyCode == 4 then
			view_show_safe( "tickets_statistics_popup_view" )
		end
    end
)

tickets_show_production_selection_grid()