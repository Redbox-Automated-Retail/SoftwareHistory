--
-- browse-filter-alpha-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()
aplha_scene_manager = newscenemanager()

alpha_table = {
	[1] = "#",
	[2] = "A",
	[3] = "B",
	[4] = "C",
	[5] = "D",
	[6] = "E",
	[7] = "F",
	[8] = "G",
	[9] = "H",
	[10] = "I",
	[11] = "J",
	[12] = "K",
	[13] = "L",
	[14] = "M",
	[15] = "N",
	[16] = "O",
	[17] = "P",
	[18] = "Q",
	[19] = "R",
	[20] = "S",
	[21] = "T",
	[22] = "U",
	[23] = "V",
	[24] = "W",
	[25] = "X",
	[26] = "Y",
	[27] = "Z"
}

display_alpha_screen_layout()
kernel.hideactor("selected_label")

function highlight_selected_alphabets(character)
	for i = 1, 27 do
		if kernel.getactortag( alpha_table[i] .. "_label" ) == "black" then
			kernel.setactortag( alpha_table[i] .. "_label", "" )
			kernel.setactorbackgroundcolor( alpha_table[i] .. "_label", "black" )
			kernel.setactorforegroundcolor( alpha_table[i] .. "_label", "white" )
		end
		if character ~= nil then
			if alpha_table[i] .. "_label" == character .. "_label" then
				kernel.setactorx( "selected_label" , tonumber(kernel.getactorx(character .. "_label")-4))
				kernel.setactory( "selected_label" , tonumber(kernel.getactory(character .. "_label")-6))
				kernel.setactortext( "selected_label", alpha_table[i] )
				kernel.showactor( "selected_label" )
				kernel.setactortag( character .. "_label", "black" )
				kernel.setactorbackgroundcolor( character .. "_label", "silver" )
				kernel.setactorforegroundcolor( character .. "_label", "black" )
			end
		end
	end
end

highlight_alphabets = function()
	for k,v in ipairs(alpha_table) do
		kernel.setactorforegroundcolor(v .. "_label", "gray")
		kernel.setactorbackgroundcolor( v .. "_label", "black" )
		kernel.registeractorhitnotify(v .. "_label", function() end)
	end
	if #browse_filtered_products ~= 0 then
		for k, v in ipairs(browse_filtered_products) do
			local start_character =string.upper(string.sub(v.sort_name, 0, 1))
			if AlphaRanges[8].ranges[tonumber(start_character)] ~= nil then
				kernel.setactorbackgroundcolor( "#_label", "black" )
				kernel.setactorforegroundcolor("#_label", "white")
				kernel.registeractorhitnotify(
					"#_label",
					function()
						browse_filter_alpha_selected ="0-9"
						browse_filter_family_tracker:set_family_type("ALL")
						local character = "#"
						browse_goto_alpha = character
						draw_images_set(browse_filtered_products_table)
						highlight_selected_alphabets(character)
					end
				)
			else
				kernel.setactorbackgroundcolor( start_character .. "_label", "black" )
				kernel.setactorforegroundcolor(start_character .. "_label", "white")
				kernel.registeractorhitnotify(
					start_character .. "_label",
					function()
						browse_filter_alpha_selected = start_character
						browse_goto_alpha = start_character
						browse_filter_family_tracker:set_family_type("ALL")
						draw_images_set(browse_filtered_products_table)
						highlight_selected_alphabets(start_character)
					end
				)
			end
		end
	else
		for k,v in ipairs(alpha_table) do
			kernel.setactorforegroundcolor(v .. "_label", "gray")
			kernel.registeractorhitnotify(
				v .. "_label",
				function()
				end
			)
		end
	end
	highlight_selected_alphabets( browse_filter_alpha_selected )
end

kernel.registeractorhitnotify(
	"abc_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,alpha" )
		set_number = 1
		browse_filter_alpha_selected = nil
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "alpha")
		show_browse_view(18)
		highlight_selected_alphabets(nil)
	end
)
kernel.registeractorhitnotify(
	"newest_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Newest" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected= nil
		browse_filter_family_tracker:set_family_type( "ALL" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"for_sale_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,for-sale" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")

		browse_filter_alpha_selected= nil
		browse_filter_family_tracker:set_family_type( "for-sale" )
		kernel.executescript( "browse_products" )
	end
)
kernel.registeractorhitnotify(
	"coming_soon_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Coming-soon" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "coming-soon")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "ComingSoon")
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
		kernel.executescript("browse_products")
		check_coming_soon_collections_popup()
	end
)
kernel.registeractorhitnotify(
	"blu_ray_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Blu-ray" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Blu-ray")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Blu-ray" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"xbox_360_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox 360" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Xbox 360")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Xbox 360" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"xbox_one_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Xbox One" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Xbox One")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Xbox One" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"ps3_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS3" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "PS3")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "PS3" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"ps4_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,PS4" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "PS4")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "PS4" )
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"wii_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Wii")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Wii" )
		kernel.executescript("browse_products")
	end
)

kernel.registeractorhitnotify(
	"wii_u_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Wii U" )
		set_number = 1
        kernel.set_session_value("BrowseState.BrowseSelectedSort", "Wii U")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Wii U" )
		kernel.executescript("browse_products")
	end
)

show_browse_view(18)
highlight_alphabets()

idle_timer:start()

if #ShoppingCart.items == 5
and browse_is_cart_full_open == false
and shopping_cart_full_message_shown == false then
	show_shopping_cart_full()
end

draw_action_bar_and_mini_cart(kernel.peekview())
draw_top_menu()

