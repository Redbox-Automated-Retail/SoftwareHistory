--
-- abe-start-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local set_text_on_screen = function()
	kernel.setactortext( "abe_mode_description", strings.abe_mode_description )
	kernel.setactortext( "abe_mode_text", strings.abe_mode_text )
end

kernel.timercreate(
	"abe_start_view_timer",
	15000,
	15000,
	function()
		kernel.timerremove( "abe_start_view_timer" )
		if kernel.getculture() ~= DefaultCulture then
			localization_toggle_culture()
		end
		kernel.executescript( "start_over" )
	end
)

kernel.registeractorhitnotify(
	"abe_espanol_button",
	function()
		post_switch_language_action = function()
			set_text_on_screen()
		end
		kernel.executescript( "switch_language" )
	end
)

kernel.registeractorhitnotify(
	"abe_handicap_button",
	function()
		view_goback_safe()
	end
)

kernel.registeractorhitnotify(
	"abe_tickets_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "abe_tickets_button" )
		if kernel.getculture() ~= "en-US" then localization_toggle_culture() end
		kernel.setlasthitactorname( "abe_tickets_button" )
		ShoppingCart.TicketsCart = newticketscart()
		if VerboseLogging then kernel.log( "|Tickets| Making call to script 'tickets_show_how_it_works_view' " ) end
		kernel.executescript( "tickets_show_how_it_works_view" )
	end
)

kernel.registeractorhitnotify(
	"abe_games_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "abe_games_button" )
		browse_filter_family_tracker.selected_family = "Games"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = nil
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_games )
		kernel.executescript( "browse_products" )
	end
)

kernel.registeractorhitnotify(
	"abe_all_movies_button",
	function()
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		view_analytics_event( "buttonPress", "abe_all_movies_button" )
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = ALL
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
		kernel.executescript( "browse_products" )
	end
)

local do_not_show_games_button = ( DisableGames or ( ProductCatalog:in_stock_games_count() < 1 ) )

if do_not_show_games_button then
	kernel.hideactor( "abe_games_button" )
	kernel.setactorx( "abe_tickets_button", 177 )
	kernel.setactorx( "abe_all_movies_button", 534 )
end

kernel.setfilter( "ada_accessible", "True" )

set_text_on_screen()

if language_switch == nil or language_switch == false then
	-- Do nothing, we are still on the ABE screen - no session should be restarted.
else
	language_switch = false
end


kernel.timerstart( "abe_start_view_timer" )