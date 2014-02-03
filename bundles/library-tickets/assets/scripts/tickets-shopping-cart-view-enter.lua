--
-- tickets-shopping-cart-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local terms_conditions_button = "tickets_terms_conditions_button"
ShoppingCart.TicketsCart:set_promotions( { [1] = ShoppingCart.TicketsCart.production.production_id } )
if #ShoppingCart.TicketsCart.promotions > 0 then
	terms_conditions_button = "tickets_terms_conditions_small_button"
	kernel.hideactor( "tickets_terms_conditions_button" )
	kernel.showactor( "tickets_terms_conditions_small_button" )
	kernel.showactor( "rules_button" )
end

local ticket_image_to_draw = "image_tickets_no_image_available"
local event_poster = kernel.ticketsvcs_registerimagefromcache( 1, 1, ShoppingCart.TicketsCart.event.event_id )
if event_poster ~= nil then
	ticket_image_to_draw = string.format( "cache:%s", event_poster )
end
kernel.setactorimage( "event_art", ticket_image_to_draw )

local sub_total = ShoppingCart.TicketsCart:sub_total( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes )
local fee_total = ShoppingCart.TicketsCart:fee_total( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes )
local fee_per_ticket = format_fee_per_ticket( Fee_Per_Ticket )
local tax_total = ShoppingCart.TicketsCart:tax_total( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes )
local total = sub_total + fee_total + tax_total

local price_codes_count = table_count( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes )
local price_code = {}
if price_codes_count == 1 then
	for k, v in pairs( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes ) do
		price_code = v
	end
end

kernel.setactortext( "event_price", "$" .. string.format( "%.2f", kernel.round( ( sub_total + fee_total ), 2 ) ) )
local breakdown_line_1 = ""
if price_codes_count == 1 then
	breakdown_line_1 = "$" .. string.format( "%.2f", kernel.round( price_code.PricePerTicket, 2 ) ) .. " x " .. price_code.Quantity .. " tickets = $" .. string.format( "%.2f", kernel.round( sub_total, 2 ) )
else
	breakdown_line_1 = "Ticket Subtotal = $".. string.format( "%.2f", kernel.round( sub_total, 2 ) )
end
local breakdown_line_2 = "Redbox $" .. fee_per_ticket .. " fee x " .. ShoppingCart.TicketsCart:quantity_total( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes ) .. " = $" .. string.format( "%.2f", kernel.round( fee_total, 2 ) )

local attributes = ShoppingCart.TicketsCart:get_attributes( ShoppingCart.TicketsCart.production )

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "AddTaxNote" ) ~= nil then
	local show_asterisk = ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "AddTaxNote" ).value ~= nil and ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "AddTaxNote" ).value ~= ""
	if show_asterisk == true then
		breakdown_line_1 = breakdown_line_1 .. "*"
		
		kernel.showactor( "tax_note" )
		kernel.setactortext( "tax_note", "*" .. ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "AddTaxNote" ).value )
	end
end

local HideSubTotalLine = false
if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideSubTotalLine" ) ~= nil then
	HideSubTotalLine = true
	kernel.hideactor( "label_sub_total" )
	kernel.hideactor( "sub_total_amount" )
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideTicketTaxLine" ) ~= nil then
	kernel.hideactor( "label_tax" )
	kernel.hideactor( "tax_amount" )
	kernel.setactory( "total_border", 681 )
	kernel.setactory( "label_total", 686 )
	kernel.setactory( "total_amount", 686 )
	
	if HideSubTotalLine == true then
		kernel.hideactor( "total_border" )
		kernel.setactory( "label_total", 681 )
		kernel.setactory( "total_amount", 681 )
	end
end

if ShoppingCart.TicketsCart:get_attribute_by_name( attributes, "HideTotalFromButton" ) ~= nil then
	kernel.setactortext( "pay_button", "PAY" )
end

kernel.setactortext( "event_price_breakdown", breakdown_line_1 .. "\n" .. breakdown_line_2 )
kernel.setmacro( "ShoppingCart:Total", "$" .. string.format( "%.2f", kernel.round( total, 2 ) ) )

kernel.setmacro( "ShoppingCart:SubTotal", "$" .. string.format( "%.2f", kernel.round( ( sub_total + fee_total ), 2 ) ) )
kernel.setmacro( "ShoppingCart:Tax", "$" .. string.format( "%.2f", kernel.round( tax_total, 2 ) ) )
local total = sub_total + fee_total + tax_total
kernel.setmacro( "ShoppingCart:Total", "$" .. string.format( "%.2f", kernel.round( total, 2 ) ) )

ShoppingCart.TicketsCart.order_header["Subtotal"] = string.format( "%.2f", kernel.round( sub_total, 2 ) )
ShoppingCart.TicketsCart.order_header["TotalFee"] = string.format( "%.2f", kernel.round( fee_total, 2 ) )
ShoppingCart.TicketsCart.order_header["TotalTax"] = string.format( "%.2f", kernel.round( tax_total, 2 ) )
ShoppingCart.TicketsCart.order_header["Total"] = string.format( "%.2f", kernel.round( total, 2 ) )

kernel.registeractorhitnotify(
	"remove_tickets_button",
	function()
		view_analytics_event( "buttonPress", "shopping-cart-remove-tickets-button" )

		local MessageView = newmessageviewmanager()
		MessageView:show()
		MessageView:set_title( tickets_strings.are_you_sure )
		MessageView:set_message( tickets_strings.shopping_cart_remove_tickets )
		MessageView:show_first_button( tickets_strings.CANCEL )
		MessageView:show_second_button(
			tickets_strings.REMOVE,
			function()
				ShoppingCart.TicketsCart:remove_item_from_cart()
				view_analytics_event( "buttonPress", "shopping-cart-remove-tickets-button,removed event id " .. tostring( ShoppingCart.TicketsCart.event.event_id ) )
				view_unwindto( "tickets_browse_view", true, "ReviewSummary:User hit remove button." )
			end
		)
		MessageView:start_timer()
	end
)

kernel.registeractorhitnotify(
	"start_over_button",
	function()
		view_analytics_event( "buttonPress", "shopping-cart-start-over-button" )

		local MessageView = newmessageviewmanager()
		MessageView:show()
		MessageView:set_title( tickets_strings.are_you_sure )
		MessageView:set_message( tickets_strings.shopping_cart_remove_tickets )
		MessageView:show_first_button( tickets_strings.CANCEL )
		MessageView:show_second_button(
			tickets_strings.REMOVE,
			function()
				ShoppingCart.TicketsCart:remove_item_from_cart()
				view_analytics_event( "buttonPress", "shopping-cart-start-over-button,removed event id " .. tostring( ShoppingCart.TicketsCart.event.event_id ) )
				kernel.executescript( "start_over" )
			end
		)
		MessageView:start_timer()
	end
)

kernel.registeractorhitnotify(
	terms_conditions_button,
	function()
		kernel.setlasthitactorname( terms_conditions_button )
		view_analytics_event( "buttonPress", "shopping-cart-terms-and-conditions-button" )
		kernel.executescript( "help" )
	end
)

kernel.registeractorhitnotify(
	"rules_button",
	function()
		view_analytics_event( "buttonPress", "rules_button" )
		view_show( "tickets_promotional_rules_view" )
	end
)

kernel.registeractorhitnotify(
	"pay_button",
	function()
		view_analytics_event( "buttonPress", "shopping-cart-pay-button" )
		kernel.executescript( "tickets_check_out" )
	end
)