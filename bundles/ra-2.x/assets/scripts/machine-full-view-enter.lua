--
-- machine-full-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:stop()

machine_full_idle_timer = newidletimer("machine_full_idle_timer")
machine_full_idle_timer:initialize(
	15000,
	function()
		view_analytics_event( "timeout", "machine_full_idle_timer" )
		kernel.executescript( "start_over" )
	end
)
machine_full_idle_timer:start()

kernel.registeractorhitnotify(
	"machine_full_back_button",
	function()
		view_analytics_event( "buttonPress", "machine_full_back_button" )
		kernel.executescript( "start_over" )
	end )

kernel.registeractorhitnotify(
	"machine_full_rent_button",
	function()
		view_analytics_event( "buttonPress", "machine_full_rent_button" )
		CloseAnalyticsOnStartView = true
		if ShoppingCart ~= nil then
			ShoppingCart:close_analytics( "True" )
			ShoppingCart:abandon( "User chose to abandon session." )
			ShoppingCart = newshoppingcart()
			ShoppingCart:start_new()
			ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
		end
		view_analytics_start( Start_Over_View_Name, "BrowseVend" )
		kernel.executescript( "browse_products" )
	end )
