--
-- tickets-promotional-rules-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local function get_promotions_as_pages()
	local promotions_as_pages = {}
	for k, v in ipairs( ShoppingCart.TicketsCart.promotions ) do
		local header = v.promotion_header_text
		local message = v.promotion_rule_text
		local promotion_as_pages = { header = header, pages = {} }
		
		kernel.log( "start - #message = " .. tostring( #message ) )
		while #message > 0 do
			kernel.setactortext( "tickets_message", message )
			
			local actor_text_info = kernel.measureactortext( "tickets_message" )
			
			local page_text = string.sub( message, 1, actor_text_info["characters_fitted"] )
			message = string.sub( message, actor_text_info["characters_fitted"] + 1 )
			
			kernel.log( "characters_fitted = " .. tostring( actor_text_info["characters_fitted"] ) )
			kernel.log( "#page_text = " .. tostring( #page_text ) )
			kernel.log( "#message = " .. tostring( #message ) )
			
			table.insert( promotion_as_pages.pages, page_text )
			kernel.setactortext( "tickets_message", "" )
		end
		kernel.log( "end - #message = " .. tostring( #message ) )
		
		table.insert( promotions_as_pages, promotion_as_pages )
	end
	return promotions_as_pages
end

local promotions_as_pages = get_promotions_as_pages()

local current_rules_set = 1
local current_page = 1

local function draw_view()
	kernel.setactortext( "tickets_message_header", promotions_as_pages[current_rules_set].header )
	
	kernel.setactortext( "tickets_message", promotions_as_pages[current_rules_set].pages[current_page] )
	
	if current_rules_set == #promotions_as_pages and current_page == #promotions_as_pages[current_rules_set].pages then
		kernel.hideactor( "first_button" )
		kernel.hideactor( "first_button_back_arrow" )
		
		kernel.hideactor( "second_button_next_arrow" )
		kernel.setactortext( "second_button", "OK" )
	else
		kernel.showactor( "first_button" )
		kernel.showactor( "first_button_back_arrow" )
		kernel.setactortext( "first_button", "BACK" )
		
		kernel.showactor( "second_button_next_arrow" )
		kernel.setactortext( "second_button", "NEXT" )
	end
end

draw_view()

kernel.registeractorhitnotify(
	"first_button",
	function()
		if kernel.getactortext( "first_button" ) == "BACK" then
			view_analytics_event( "buttonPress", "back button" )
			view_goback_safe( "tickets-promotional-rules-view", false )
		else
			view_analytics_event( "buttonPress", "back button" )
			if current_page - 1 >= 1 then
				current_page = current_page - 1
			elseif current_rules_set - 1 >= 1 then
				current_rules_set = current_rules_set - 1
				current_page = #promotions_as_pages[current_rules_set].pages
			end
			
			draw_view()
		end
	end
)

kernel.registeractorhitnotify(
	"second_button",
	function()
		if kernel.getactortext( "second_button" ) == "OK" then
			view_analytics_event( "buttonPress", "ok button" )
			view_goback_safe( "tickets-promotional-rules-view", false )
		else
			view_analytics_event( "buttonPress", "next button" )
			if current_page + 1 <= #promotions_as_pages[current_rules_set].pages then
				current_page = current_page + 1
			elseif current_rules_set + 1 <= #promotions_as_pages then
				current_rules_set = current_rules_set + 1
				current_page = 1
			end
			
			draw_view()
		end
	end
)

promotional_rules_view_idle_timer = newidletimer( "promotional_rules_view_idle_timer" )
promotional_rules_view_idle_timer:initialize(
	40000,
	function()
		view_goback_safe( "tickets-promotional-rules-view", false )
	end
)
promotional_rules_view_idle_timer:start()
idle_timer:suspend()