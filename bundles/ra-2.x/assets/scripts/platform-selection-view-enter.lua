--
-- platform-selection-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()

kernel.hideactor("ps3_button")
kernel.hideactor("ps4_button")
kernel.hideactor("wii_button")
kernel.hideactor("wii_u_button")
kernel.hideactor("xbox_one_button")
kernel.hideactor("xbox_360_button")

if GameEnablePS3 and GameHasContentPS3 then
	kernel.showactor("ps3_button")
end

if GameEnablePS4 and GameHasContentPS4 then
	kernel.showactor("ps4_button")
end

if GameEnableWii and GameHasContentWii then
	kernel.showactor("wii_button")
end

if GameEnableWiiU and GameHasContentWiiU then
	kernel.showactor("wii_u_button")
end

if GameEnableXbox360 and GameHasContentXbox360 then
	kernel.showactor("xbox_360_button")
end

if GameEnableXboxOne and GameHasContentXboxOne then
	kernel.showactor("xbox_one_button")
end

kernel.registeractorhitnotify(
	"ps3_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS3" )
		set_number = 1
		browse_selected_sort = "PS3"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "PS3" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"ps4_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS4" )
		set_number = 1
		browse_selected_sort = "PS4"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "PS4" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"wii_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii" )
		set_number = 1
		browse_selected_sort = "Wii"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Wii" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"wii_u_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii U" )
		set_number = 1
		browse_selected_sort = "Wii U"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Wii U" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"xbox_360_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox 360" )
		set_number = 1
		browse_selected_sort = "Xbox 360"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Xbox 360" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"xbox_one_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox One" )
		set_number = 1
		browse_selected_sort = "Xbox One"
		browse_filter_mode = "all"
		browse_filter_family_tracker:set_family_type( "Xbox One" )
		view_show_safe( "browse_view")
		-- view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
kernel.registeractorhitnotify(
	"view_all_button",
	function()
		view_show_safe( "browse_view" )
		--view_analytics_event( "productsShown", tostring( total_products_shown ) )
	end
)
