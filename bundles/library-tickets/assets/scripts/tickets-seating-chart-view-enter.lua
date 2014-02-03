--
-- tickets-seating-chart-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local venue_image_to_draw = "image_tickets_no_image_available_large"
local venue_poster = kernel.ticketsvcs_registerimagefromcache( 2, 2, ShoppingCart.TicketsCart.production.production_id )
if venue_poster ~= nil then
	venue_image_to_draw = string.format( "cache:%s", venue_poster )
end

kernel.setactorimage( "seating_view", venue_image_to_draw )
show_ga_overlay_text_if_applicable()

kernel.registeractorhitnotify(
	"option_1_button",
	function()
		kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
		kernel.setactorimage( "option_2_checkmark", "image_gray_checkmark" )
		ShoppingCart.TicketsCart.selected_ticket_group = ShoppingCart.TicketsCart.ticket_groups[1]
		view_analytics_event( "buttonPress", "tickets-seating-chart-view-option-1-button" )
	end
)
if ( ShoppingCart.TicketsCart.ticket_groups[1].Zone == nil and ShoppingCart.TicketsCart.ticket_groups[1].Level == nil )
or ( ShoppingCart.TicketsCart.ticket_groups[1].Zone == "" and ShoppingCart.TicketsCart.ticket_groups[1].Level == "" ) then
	kernel.hideactor( "option_1_zone_level" )
	kernel.setactory( "option_1_prices", kernel.getactory( "option_1_section_row" ) )
	kernel.setactory( "option_1_section_row", kernel.getactory( "option_1_zone_level" ) )
end
if ShoppingCart.TicketsCart.ticket_groups[1].SuggestionTypeId == "3" then
	kernel.hideactor( "option_1_section_row" )
	kernel.setactory( "option_1_prices", kernel.getactory( "option_1_section_row" ) )
end

if ShoppingCart.TicketsCart.ticket_groups[2] ~= nil then
	kernel.registeractorhitnotify(
		"option_2_button",
		function()
			kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
			kernel.setactorimage( "option_1_checkmark", "image_gray_checkmark" )
			ShoppingCart.TicketsCart.selected_ticket_group = ShoppingCart.TicketsCart.ticket_groups[2]
			view_analytics_event( "buttonPress", "tickets-seating-chart-view-option-2-button" )
		end
	)
	if ( ShoppingCart.TicketsCart.ticket_groups[2].Zone == nil and ShoppingCart.TicketsCart.ticket_groups[2].Level == nil )
	or ( ShoppingCart.TicketsCart.ticket_groups[2].Zone == "" and ShoppingCart.TicketsCart.ticket_groups[2].Level == "" ) then
		kernel.hideactor( "option_2_zone_level" )
		kernel.setactory( "option_2_prices", kernel.getactory( "option_2_section_row" ) )
		kernel.setactory( "option_2_section_row", kernel.getactory( "option_2_zone_level" ) )
	end
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

if ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId == ShoppingCart.TicketsCart.ticket_groups[1].TicketGroupId then
	kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
elseif ShoppingCart.TicketsCart.ticket_groups[2] ~= nil and ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId == ShoppingCart.TicketsCart.ticket_groups[2].TicketGroupId then
	kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
end

kernel.registeractorhitnotify(
	"unzoom_button",
	function()
		view_analytics_event( "buttonPress", "tickets-seating-chart-view-unzoom-button" )
		view_unwindto( "tickets_seat_selection_view", true, "SeatingChart: User hit Unzoom button." )
	end
)

kernel.registeractorhitnotify(
	"buy_button",
	function()
		view_analytics_event( "context", string.format( "TicketGroup %s-%s selected.", ShoppingCart.TicketsCart.selected_ticket_group.TicketGroupId, ShoppingCart.TicketsCart.selected_ticket_group.SuggestionTypeDisplayName or nil ) )
		view_analytics_event( "buttonPress", "tickets-seating-chart-view-buy-button" )
		view_show( "tickets_confirm_delivery_method_view" )
	end
)