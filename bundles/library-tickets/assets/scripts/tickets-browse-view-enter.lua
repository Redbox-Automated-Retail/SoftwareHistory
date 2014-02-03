--
-- tickets-browse-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

ShoppingCart.TicketsCart:reset_cart()
kernel.setmacro( "get_local_tickets2_label:text", string.gsub(tickets_strings.tickets_browse_get_local_tickets2_label, "{0}", tickets_fee_for_display ) )

if ( tickets_browse_filter_selected == nil and tickets_browse_sort_filter_selected == nil ) then
	kernel.disableactor( "filter_tab_1" )
	tickets_browse_filter_selected = "all"
	kernel.disableactor( "sort_filter_tab_1" )
	tickets_browse_sort_filter_selected = "soonest"
else
	local selected_filter_tab = kernel.getactorsbytag( tickets_browse_filter_selected )
	kernel.disableactor( selected_filter_tab[1] )
	local selected_sort_filter_tab = kernel.getactorsbytag( tickets_browse_sort_filter_selected )
	kernel.disableactor( selected_sort_filter_tab[1] )
end

for i = 1, 7 do
	kernel.registeractorhitnotify( "filter_tab_" .. i,
		function()
			for j = 1, 7 do
				kernel.enableactor( "filter_tab_" .. j )
			end
			kernel.disableactor( "filter_tab_" .. i )
			
			tickets_browse_filter_selected = tostring( kernel.getactortag( "filter_tab_" .. i ) )
			local analytics_tickets_browse_filter_selected = string.lower( string.gsub( kernel.getactortext( "filter_tab_" .. i ), " ", "" ) )
			view_analytics_event( "filterChange", "browse-filter," .. analytics_tickets_browse_filter_selected )
			tickets_browse_pager:reset()
			tickets_show_filtered_grid()
		end
	)
end
for i = 1, 5 do
	kernel.registeractorhitnotify( "sort_filter_tab_" .. i,
		function()
			for j = 1, 5 do
				kernel.enableactor( "sort_filter_tab_" .. j )
			end
			kernel.disableactor( "sort_filter_tab_" .. i )
			
			tickets_browse_sort_filter_selected = tostring( kernel.getactortag( "sort_filter_tab_" .. i ) )
			view_analytics_event( "filterChange", "browse-filter-sort," .. tickets_browse_sort_filter_selected )
			tickets_browse_pager:reset()
			tickets_show_filtered_grid()
		end
	)
end

if tickets_selection_grid_scene_manager == nil then
	tickets_selection_grid_scene_manager = newscenemanager()
end

if tickets_browse_pager == nil then
	tickets_browse_pager = newpager()
	tickets_browse_pager.prev_changed = function( enabled )
		if enabled == true then
			kernel.setactorimage( "left_arrow", "image_left_navigation_arrow" )
			kernel.enableactor( "left_arrow" )
			kernel.showactor( "left_arrow" )
		else
			kernel.disableactor( "left_arrow" )
			kernel.hideactor( "left_arrow" )
		end
	end
	tickets_browse_pager.next_changed = function( enabled )
		if enabled == true then
			kernel.setactorimage( "right_arrow", "image_right_navigation_arrow" )
			kernel.enableactor( "right_arrow" )
			kernel.showactor( "right_arrow" )
		else
			kernel.disableactor( "right_arrow" )
			kernel.hideactor( "right_arrow" )
		end
	end
end

kernel.registeractorhitnotify(
	"how_it_works_button",
	function()
		view_analytics_event( "buttonPress", "how_it_works" )
		kernel.setlasthitactorname( "how_it_works_button" )
		view_show( "tickets_how_it_works_view" )
	end
)

kernel.registeractorhitnotify(
	"start_over_button",
	function()
		view_analytics_event( "buttonPress", "start_over_button" )
		kernel.setlasthitactorname( "start_over_button" )
		kernel.executescript( "start_over" )
	end
)

tickets_show_filtered_grid()
idle_timer:start()