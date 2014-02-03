--
-- tickets-find-tickets-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local show_seat_selection_screen_based_on_availability = function()
	kernel.popmovie()
	kernel.log( "|Tickets| GetTicketsResponse.success = ".. tostring( GetTicketsResponse.success ) )
	if GetTicketsResponse.success == true then
		ShoppingCart.TicketsCart:set_ticket_groups_from_response( GetTicketsResponse )
		ShoppingCart.TicketsCart:set_lock_guids()
		ShoppingCart.TicketsCart.PriceCodeManager:save_working_price_codes()
		kernel.log( "|Tickets| Showing Seat Selection Screen." )
		view_show( "tickets_seat_selection_view" )
	else
		ShoppingCart.TicketsCart.get_tickets_call_made = false
		local TicketsMessageView = newticketsmessageviewmanager()
		TicketsMessageView:show()

		local show_generic_tickets_transaction_error_message = function()
			view_analytics_event( "context", "connectionIssue" )
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			TicketsMessageView:set_message( tickets_strings.offline_or_fatal_transaction_message )
			TicketsMessageView:show_second_button(
				"OK",
				function()
					view_analytics_event( "buttonPress", "OK_button" )
					kernel.executescript( "start_over" )
				end
			)
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "TicketsMessageView: Timeout." )
					kernel.executescript( "start_over" )
				end
			)
		end

		if GetTicketsResponse.properties.response == "TicketsNotAvailable" then
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			local message_to_display = tickets_strings.no_tickets_found_message
			view_analytics_event( "context", "allTicketsLocked" )
			if #tickets_production_selection_productions > 1 then
				message_to_display = message_to_display.. " ".. tickets_strings.please_choose_another_date
				TicketsMessageView:show_second_button(
					"CHANGE DATE",
					function()
						view_analytics_event( "buttonPress", "change_date_button" )
						view_unwindto( "tickets_production_selection_view", true, "TicketsMessageView: User hit Change Date button." )
					end
				)
			end
			TicketsMessageView:set_message( message_to_display )
			TicketsMessageView:show_first_button(
				tickets_strings.browse_button_with_arrow_text,
				function()
					view_analytics_event( "buttonPress", "browse_button" )
					view_unwindto( "tickets_browse_view", true, "TicketsMessageView: User hit browse button." )
				end
			)
			TicketsMessageView:show_first_button_back_arrow()
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "TicketsMessageView: Timeout." )
					kernel.executescript( "start_over" )
				end
			)
		elseif GetTicketsResponse.properties.response == "NoContiguousSeats" then
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			view_analytics_event( "context", "seatsNotTogether" )
			local message_to_display = tickets_strings.no_contigious_seats_message
			if #tickets_production_selection_productions > 1 then
				message_to_display = message_to_display.. " ".. tickets_strings.please_choose_another_date
				TicketsMessageView:show_second_button(
					"CHANGE DATE",
					function()
						view_analytics_event( "buttonPress", "change_dates_button" )
						view_unwindto( "tickets_production_selection_view", true, "TicketsMessageView: User hit Change Date button." )
					end
				)
			end
			TicketsMessageView:set_message( message_to_display )
			TicketsMessageView:show_first_button(
				tickets_strings.browse_button_with_arrow_text,
				function()
					view_analytics_event( "buttonPress", "browse_button" )
					view_unwindto( "tickets_browse_view", true, "TicketsMessageView: User hit browse button." )
				end
			)
			TicketsMessageView:show_first_button_back_arrow()
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "TicketsMessageView: Timeout." )
					kernel.executescript( "start_over" )
				end
			)
		elseif GetTicketsResponse.properties.response == "TicketsAreSoldOut" then
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			view_analytics_event( "context", "outOfStockSeats" )
			local message_to_display = tickets_strings.out_of_stock_message
			if #tickets_production_selection_productions > 1 then
				message_to_display = message_to_display.. " ".. tickets_strings.please_choose_another_date
				TicketsMessageView:show_second_button(
					"CHANGE DATE",
					function()
						view_analytics_event( "buttonPress", "change_dates_button" )
						view_unwindto( "tickets_production_selection_view", true, "TicketsMessageView: User hit Change Date button." )
					end
				)
			end
			TicketsMessageView:set_message( message_to_display )
			TicketsMessageView:show_first_button(
				tickets_strings.browse_button_with_arrow_text,
				function()
					view_analytics_event( "buttonPress", "browse_button" )
					view_unwindto( "tickets_browse_view", true, "TicketsMessageView: User hit browse button." )
				end
			)
			TicketsMessageView:show_first_button_back_arrow()
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "TicketsMessageView: Timeout." )
					kernel.executescript( "start_over" )
				end
			)
			kernel.ticketsvcs_excludeproduction( ShoppingCart.TicketsCart.production.production_id )
			ShoppingCart.TicketsCart.production = nil
			refresh_views()
		else
			show_generic_tickets_transaction_error_message()
		end
	end
end

local _15_sec_counter = 0
kernel.timercreate(
	"15_sec_counter",
	1000,
	1000,
	function()
		_15_sec_counter = _15_sec_counter + 1
		if GetTicketsResponse ~= nil then
			kernel.timerstop( "15_sec_counter" )
			kernel.timerremove( "15_sec_counter" )
			show_seat_selection_screen_based_on_availability()
		end
		if _15_sec_counter == 15 then
			kernel.timerstop( "15_sec_counter" )
			kernel.timerremove( "15_sec_counter" )

			local TicketsMessageView = newticketsmessageviewmanager()
			TicketsMessageView:show()
			TicketsMessageView:set_message_header( tickets_strings.sorry )
			TicketsMessageView:set_message( tickets_strings.cannot_process_tickets_transaction_message )
			TicketsMessageView:show_second_button(
				"OK",
				function()
					view_analytics_event( "buttonPress", "OK_button" )
					kernel.executescript( "start_over" )
				end
			)
			TicketsMessageView:start_timer(
				function()
					view_analytics_event( "context", "TicketsMessageView: Timeout." )
					kernel.executescript( "start_over" )
				end
			)
		end
	end
)

kernel.timercreate(
	"wait_2_sec_timer",
	2000,
	nil,
	function()
		kernel.log( "|Tickets| 2 sec wait timer completed." )
		kernel.timerstop( "wait_2_sec_timer" )
		kernel.timerremove( "wait_2_sec_timer" )

		if GetTicketsResponse == nil then
			kernel.log( "|Tickets| Starting 15 sec counter." )
			kernel.timerstart( "15_sec_counter" )
			return
		end
		show_seat_selection_screen_based_on_availability()
	end
)

if VerboseLogging then
	kernel.log( "old_working_price_codes:" )
	kernel.log( ShoppingCart.TicketsCart.PriceCodeManager.old_working_price_codes )
end

if VerboseLogging then
	kernel.log( "|Tickets| get_tickets_call_made : ".. tostring( ShoppingCart.TicketsCart.get_tickets_call_made ) )
	kernel.log( "|Tickets| has_working_price_codes_quantity_changed : ".. tostring( ShoppingCart.TicketsCart.PriceCodeManager:has_working_price_codes_quantity_changed() ) )
end

if ( ShoppingCart.TicketsCart.get_tickets_call_made and ShoppingCart.TicketsCart.PriceCodeManager:has_working_price_codes_quantity_changed() )
or not ShoppingCart.TicketsCart.get_tickets_call_made then
	kernel.pushmovie( "movie_finding_tickets", { x = 0, y = 0, width = 1024, height = 768 } )
	view_analytics_event( "context", "findingTickets" )
	kernel.log( "|Tickets| Making GetTickets call to Ticket Controller." )
	GetTicketsResponse = nil

	if VerboseLogging then
		kernel.log( "|Tickets| Working_Price_Codes and Lock_Guids : " )
		kernel.log( ShoppingCart.TicketsCart.PriceCodeManager.working_price_codes )
		kernel.log( ShoppingCart.TicketsCart.lock_guids )
	end
	kernel.ticketsvcs_gettickets(
		StoreManager:get_number(),
		ShoppingCart.TicketsCart.production.production_id,
		ShoppingCart.TicketsCart.PriceCodeManager:select_valid_price_codes(),
		ShoppingCart.TicketsCart.lock_guids,
		function( result )
			kernel.log( "|Tickets| Received Response from Tickets Controller." )
			GetTicketsResponse = result
			kernel.log( GetTicketsResponse )
		end
	)
	ShoppingCart.TicketsCart.lock_guids = {}
	ShoppingCart.TicketsCart.get_tickets_call_made = true

	kernel.timerstart( "wait_2_sec_timer" )
else
	kernel.log( "|Tickets| Skipping GetTickets call to Ticket Controller. Taking customer to Seat Selection." )
	view_show( "tickets_seat_selection_view" )
end
