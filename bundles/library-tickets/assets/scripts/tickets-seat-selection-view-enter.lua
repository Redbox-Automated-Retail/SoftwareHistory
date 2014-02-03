--
-- tickets-seat-selection-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

show_ga_overlay_text_if_applicable = function()
	if ( #ShoppingCart.TicketsCart.ticket_groups == 1 and ShoppingCart.TicketsCart.ticket_groups[1].SuggestionTypeId == "3" )
	or ( #ShoppingCart.TicketsCart.ticket_groups == 2 and ShoppingCart.TicketsCart.ticket_groups[1].SuggestionTypeId == "3" and ShoppingCart.TicketsCart.ticket_groups[2].SuggestionTypeId == "3" ) then
		if kernel.peekview() == "tickets_seat_selection_view" then
			kernel.setactorimage( "ga_overlay_text", "image_ga_overlay_text" )
			kernel.showactor( "ga_overlay_text" )
			kernel.registeractorhitnotify(
				"ga_overlay_text",
				function()
					view_analytics_event( "buttonPress", "seat-selection-seating-chart" )
					view_show( "tickets_seating_chart_view" )
				end
			)
		else
			kernel.setactorimage( "ga_overlay_text_large", "image_ga_overlay_text_large" )
			kernel.showactor( "ga_overlay_text_large" )
		end
	else
		kernel.hideactor( "ga_overlay_text" )
		kernel.hideactor( "ga_overlay_text_large" )
	end
end

local production_venue = Venues[ShoppingCart.TicketsCart.production.venue_id]
kernel.setmacro( "SeatSelection:VenueAddress", production_venue.address1 .. " " .. production_venue.city .. " " .. production_venue.state .. ", " .. production_venue.zip )

kernel.setmacro( "SeatSelection:EventName", ShoppingCart.TicketsCart.event.sort_name )
local venue_image_to_draw = "image_tickets_no_image_available"
local venue_poster = kernel.ticketsvcs_registerimagefromcache( 2, 2, ShoppingCart.TicketsCart.production.production_id )
if venue_poster ~= nil then
	venue_image_to_draw = string.format( "cache:%s", venue_poster )
end

kernel.setactorimage( "seating_chart", venue_image_to_draw )
show_ga_overlay_text_if_applicable()

kernel.registeractorhitnotify(
	"seating_chart",
	function()
		view_analytics_event( "buttonPress", "seat-selection-seating-chart" )
		view_show( "tickets_seating_chart_view" )
	end
)

table.sort(
	ShoppingCart.TicketsCart.ticket_groups,
	function( a, b )
		if a.DisplayOrder == nil
		or b.DisplayOrder == nil
		or a.DisplayOrder == b.DisplayOrder then
			local a_sub_total = ShoppingCart.TicketsCart:sub_total( a.PriceCodes ) + ShoppingCart.TicketsCart:fee_total( a.PriceCodes )
			local b_sub_total = ShoppingCart.TicketsCart:sub_total( b.PriceCodes ) + ShoppingCart.TicketsCart:fee_total( b.PriceCodes )
			return tostring( a_sub_total ) > tostring( b_sub_total )
		else
			return a.DisplayOrder < b.DisplayOrder
		end
	end
)

match_price_codes = function( price_codes )
	if table_count( price_codes ) ~= #ShoppingCart.TicketsCart.PriceCodeManager.select_price_codes then
		return false
	end

	local match_count = 0
	for i, v1 in ipairs( ShoppingCart.TicketsCart.PriceCodeManager.select_price_codes ) do
		for j, v2 in pairs( price_codes ) do
			if tonumber( v1.PriceCodeId ) == tonumber( v2.PriceCodeId ) then
				match_count = match_count + 1
			end
		end
	end
	return ( match_count == #ShoppingCart.TicketsCart.PriceCodeManager.select_price_codes )
end

set_zone_level = function( tg )
	if ( ShoppingCart.TicketsCart.ticket_groups[tg].Zone == nil and ShoppingCart.TicketsCart.ticket_groups[tg].Level == nil )
	or ( ShoppingCart.TicketsCart.ticket_groups[tg].Zone == "" and ShoppingCart.TicketsCart.ticket_groups[tg].Level == "" ) then
		kernel.hideactor( "option_".. tg.. "_zone_level" )
		kernel.setactory( "option_".. tg.. "_prices", kernel.getactory( "option_".. tg.. "_section_row" ) )
		kernel.setactory( "option_".. tg.. "_section_row", kernel.getactory( "option_".. tg.. "_zone_level" ) )
	else
		local zone_text = ""
		if ShoppingCart.TicketsCart.ticket_groups[tg].Zone ~= nil and ShoppingCart.TicketsCart.ticket_groups[tg].Zone ~= "" then
			zone_text = "Zone ".. ShoppingCart.TicketsCart.ticket_groups[tg].Zone
		end
		local level_text = ""
		if ShoppingCart.TicketsCart.ticket_groups[tg].Level ~= nil and ShoppingCart.TicketsCart.ticket_groups[tg].Level ~= "" then
			level_text = "Level ".. ShoppingCart.TicketsCart.ticket_groups[tg].Level
		end
		local zone_level_text = ""
		if zone_text ~= "" then
			zone_level_text = zone_text
		end
		if level_text ~= "" then
			if zone_text ~= "" then
				zone_level_text = zone_level_text.. ", "
			end
			zone_level_text = zone_level_text.. level_text
		end
		kernel.setmacro("SeatSelection:Option".. tg.. "ZoneLevel", zone_level_text )
	end
end

if match_price_codes( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes ) == false then
	kernel.showactor( "alternate_seats_label" )
end

Fee_Per_Ticket = TicketsProfilePricing["Fee"].value

local tg1_price_codes_count = table_count( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes )
local tg1_price_code = {}
if tg1_price_codes_count == 1 then
	for k, v in pairs( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes ) do
		tg1_price_code = v
	end
end

local sub_total = ShoppingCart.TicketsCart:sub_total( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes ) + ShoppingCart.TicketsCart:fee_total( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes )
local fee_per_ticket = format_fee_per_ticket( Fee_Per_Ticket )

kernel.setmacro( "SeatSelection:Option1SuggestionType", ShoppingCart.TicketsCart.ticket_groups[1].SuggestionTypeDisplayName )
kernel.setmacro( "SeatSelection:Option1SubTotal", "$" .. string.format( "%.2f", kernel.round( sub_total, 2 ) ) )
kernel.setmacro( "SeatSelection:Option1Section", ShoppingCart.TicketsCart.ticket_groups[1].Section )
kernel.setmacro( "SeatSelection:Option1Row", ShoppingCart.TicketsCart.ticket_groups[1].Row )
set_zone_level( 1 )
local price_text_option_1 = ""
if tg1_price_codes_count == 1 then
	price_text_option_1 = string.format( "%.2f Each", kernel.round( tg1_price_code.PricePerTicket, 2 ) )
else
	price_text_option_1 = string.format( "%.2f", kernel.round( ShoppingCart.TicketsCart:sub_total( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes ), 2 ) )
end
kernel.setmacro( "SeatSelection:Option1PricePerTicket", "$" .. price_text_option_1 )
kernel.setmacro( "SeatSelection:Option1FeePerTicket", "$" .. fee_per_ticket )
kernel.setmacro( "SeatSelection:Option1Quantity", ShoppingCart.TicketsCart:quantity_total( ShoppingCart.TicketsCart.ticket_groups[1].PriceCodes ) )
kernel.registeractorhitnotify(
	"option_1_button",
	function()
		kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
		kernel.setactorimage( "option_2_checkmark", "image_gray_checkmark" )
		ShoppingCart.TicketsCart.selected_ticket_group = ShoppingCart.TicketsCart.ticket_groups[1]
		view_analytics_event( "buttonPress", "seat-selection-option-1-button" )
	end
)
if ShoppingCart.TicketsCart.ticket_groups[1].SuggestionTypeId == "3" then
	kernel.hideactor( "option_1_section_row" )
	kernel.setactory( "option_1_prices", kernel.getactory( "option_1_section_row" ) )
end

if ShoppingCart.TicketsCart.ticket_groups[2] ~= nil then
	if kernel.isactorvisible( "alternate_seats_label" ) == false
	and match_price_codes( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes ) == false then
		kernel.showactor( "alternate_seats_label" )
	end
	local tg2_price_codes_count = table_count( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes )
	local tg2_price_code = {}
	if tg2_price_codes_count == 1 then
		for k, v in pairs( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes ) do
			tg2_price_code = v
		end
	end

	local sub_total = ShoppingCart.TicketsCart:sub_total( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes ) + ShoppingCart.TicketsCart:fee_total( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes )
	local fee_per_ticket = format_fee_per_ticket( Fee_Per_Ticket )
	kernel.setmacro( "SeatSelection:Option2SuggestionType", ShoppingCart.TicketsCart.ticket_groups[2].SuggestionTypeDisplayName )
	kernel.setmacro( "SeatSelection:Option2SubTotal", "$" .. string.format( "%.2f", kernel.round( sub_total, 2 ) ) )
	kernel.setmacro( "SeatSelection:Option2Section", ShoppingCart.TicketsCart.ticket_groups[2].Section )
	kernel.setmacro( "SeatSelection:Option2Row", ShoppingCart.TicketsCart.ticket_groups[2].Row )
	set_zone_level( 2 )
	local price_text_option_2 = ""
	if tg1_price_codes_count == 1 then
		price_text_option_2 = string.format( "%.2f Each", kernel.round( tg2_price_code.PricePerTicket, 2 ) )
	else
		price_text_option_2 = string.format( "%.2f", kernel.round( ShoppingCart.TicketsCart:sub_total( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes ), 2 ) )
	end
	kernel.setmacro( "SeatSelection:Option2PricePerTicket", "$" .. price_text_option_2 )
	kernel.setmacro( "SeatSelection:Option2FeePerTicket", "$" .. fee_per_ticket )
	kernel.setmacro( "SeatSelection:Option2Quantity", ShoppingCart.TicketsCart:quantity_total( ShoppingCart.TicketsCart.ticket_groups[2].PriceCodes ) )
	kernel.registeractorhitnotify(
		"option_2_button",
		function()
			kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
			kernel.setactorimage( "option_1_checkmark", "image_gray_checkmark" )
			ShoppingCart.TicketsCart.selected_ticket_group = ShoppingCart.TicketsCart.ticket_groups[2]
			view_analytics_event( "buttonPress", "seat-selection-option-2-button" )
		end
	)
	if ShoppingCart.TicketsCart.ticket_groups[2].SuggestionTypeId == "3" then
		kernel.hideactor( "option_2_section_row" )
		kernel.setactory( "option_2_prices", kernel.getactory( "option_2_section_row" ) )
	end
else
	kernel.hideactor( "option_2_button" )
	kernel.hideactor( "option_2_zone_level" )
	kernel.hideactor( "option_2_section_row" )
	kernel.hideactor( "option_2_prices" )
	kernel.hideactor( "option_2_checkmark" )
end

if ShoppingCart.TicketsCart.selected_ticket_group == nil then
	ShoppingCart.TicketsCart.selected_ticket_group = ShoppingCart.TicketsCart.ticket_groups[1]
end

if ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId == ShoppingCart.TicketsCart.ticket_groups[1].TicketGroupId then
	kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
elseif ShoppingCart.TicketsCart.ticket_groups[2] ~= nil and ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId == ShoppingCart.TicketsCart.ticket_groups[2].TicketGroupId then
	kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
end

kernel.setmacro( "SeatSelection:EmailConfirmationText", tickets_strings.email_confirmation_text )

local attributes = ShoppingCart.TicketsCart:get_attributes( ShoppingCart.TicketsCart.production )
if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideSeatSelectionNote" ) ~= nil then
	kernel.hideactor( "more_options_label_bold" )
	kernel.hideactor( "more_options_label" )
end

kernel.registeractorhitnotify(
	"browse_button",
	function()
		view_analytics_event( "buttonPress", "seat-selection-browse-button" )
		view_unwindto( "tickets_browse_view", true, "SeatSelection:User hit browse button." )
	end
)

kernel.registeractorhitnotify(
	"change_quantity_button",
	function()
		view_analytics_event( "buttonPress", "seat-selection-change-quantity-button" )
		ShoppingCart.TicketsCart.selected_ticket_group = nil
		view_unwindto( choose_quantity_view_name, true, "SeatSelection: User hit ChangeQuantity button." )
	end
)

kernel.registeractorhitnotify(
	"buy_button",
	function()
		view_analytics_event( "context", string.format( "TicketGroup %s-%s selected.", ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId, ShoppingCart.TicketsCart.selected_ticket_group.SuggestionTypeDisplayName or nil ) )
		view_analytics_event( "buttonPress", "seat-selection-buy-button" )
		view_show( "tickets_confirm_delivery_method_view" )
	end
)

kernel.registeractorhitnotify(
	"venue_info_button",
	function()
		view_analytics_event( "buttonPress", "venue_info_button" )
		view_show( "tickets_venue_detail_view" )
	end
)