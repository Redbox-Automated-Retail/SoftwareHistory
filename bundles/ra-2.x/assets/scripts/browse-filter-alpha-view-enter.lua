--
-- browse-filter-alpha-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()
aplha_scene_manager = newscenemanager()

if set_number_offset == nil then
	set_number_offset = 0
end

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

local function highlight_barrier_label(browse_filter_alpha_selected)
	for i=1,20,1 do
		if kernel.getactortext("barrier_label_" .. i) == browse_filter_alpha_selected then
			kernel.setactorforegroundcolor("barrier_label_" .. i,  {["r"] = 153, ["g"] = 0, ["b"] = 0} )
		else
			kernel.setactorforegroundcolor("barrier_label_" .. i, "black")
		end
	end
end

refresh_barriers()

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

function load_alpha_products (start_character,alpha_filtered_products)
	local page_number =1
	for k, v in ipairs(alpha_filtered_products)  do
		if v ~="" then
			if string.upper(string.sub( v.sort_name, 0, 1)) == start_character then
				page_number = math.ceil(k/18)
				return page_number
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
						set_number = 1
						refresh_barriers()
						draw_images_set(browse_filtered_products_table)
						local character = "#"
						highlight_selected_alphabets(character)
						highlight_barrier_label(character)
					end
				)
			else
				kernel.setactorbackgroundcolor( start_character .. "_label", "black" )
				kernel.setactorforegroundcolor(start_character .. "_label", "white")
				kernel.registeractorhitnotify(
					start_character .. "_label",
					function()
						if browse_filter_alpha_selected ~= start_character then
							set_number_offset = 0
						end
						browse_filter_alpha_selected = start_character
						browse_filter_family_tracker:set_family_type("ALL")
						set_number = load_alpha_products(start_character,browse_filtered_products_table) + set_number_offset
						refresh_barriers()
						draw_images_set(browse_filtered_products_table)
						highlight_selected_alphabets(start_character)
						highlight_barrier_label(start_character)
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

-- Register All Actor Hit call-backs
-- Left and Right Arrows
kernel.registeractorhitnotify(
	"left_arrow_button",
	function()
		set_number_offset = set_number_offset - 1
		view_analytics_event( "pageChange", "browse-page-back" )
		browse_selected_sort = "alpha"
		refresh_barriers()
		move_left(18)
		highlight_barrier_label(browse_filter_alpha_selected)
	end
)
kernel.registeractorhitnotify(
	"right_arrow_button",
	function()
		set_number_offset = set_number_offset + 1
		view_analytics_event( "pageChange", "browse-page-next" )
		browse_selected_sort = "alpha"
		refresh_barriers()
		move_right(18)
		highlight_barrier_label(browse_filter_alpha_selected)
	end
)

function alpha_switch_product_family_tab_function()
	if browse_filter_family_tracker.selected_family == "Movies" then
		browse_filter_family_tracker.selected_family = "Games"
		set_number = 1
		browse_selected_sort = "Newest"
		browse_filter_mode = "all"
		browse_filter_genre_selected = nil
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "ALL" )
		view_analytics_event( "filterChange", "browse-filter," .. browse_filter_family_tracker.selected_family )
		kernel.executescript( "browse_products" )
		return
	else
		browse_filter_family_tracker.selected_family = "Movies"
		browse_filter_genre_selected = ALL
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
	end
	view_analytics_event( "filterChange", "browse-filter," .. browse_filter_family_tracker.selected_family )
	browse_filter_family_tracker:set_family_type( "ALL" )
	browse_selected_sort = "alpha"
	browse_filter_alpha_selected = nil
	set_number = 1
	display_alpha_screen_layout()
	show_browse_view(18)
	check_product_availability()
	highlight_alphabets()
	highlight_tab(1)
end

kernel.registeractorhitnotify(
	"tab_switch_product_family",
	function()
		refresh_barriers()
		alpha_switch_product_family_tab_function()
	end
)

kernel.registeractorhitnotify(
	"image_next_white_arrow",
	function()
		refresh_barriers()
		alpha_switch_product_family_tab_function()
	end
)

-- Tabs at the top
function RegisterProductFamilyTabs()
	for i = 1, 7 do
		kernel.registeractorhitnotify(
			"tab_" .. tostring( i ),
			function()
				browse_filter_genre_selected = ProductFamiliesMetaData[ browse_filter_family_tracker.selected_family ].filter_genres[i]
				local tab_text = kernel.getactortext( "tab_" .. tostring( i ) )
				if string.len(tab_text) == 0
					and i==2 then -- single out the Top 20 tab (2nd tab, ie i==2) that is currently split into 2 actors
					-- handles the tabs that has the text split between 2 new actors
					tab_text = kernel.getactortext( "tab_" .. tostring( i ) .. "_text_a")
						.. " "
						.. kernel.getactortext( "tab_" .. tostring( i ) .. "_text_b")
				end
				view_analytics_event( "filterChange", "browse-filter-genre," .. tab_text )
				browse_filter_alpha_selected = nil
				set_number = 1
				refresh_barriers()
				show_browse_view(18)
				check_product_availability()
				highlight_tab( i )
				highlight_selected_alphabets(nil)
				highlight_alphabets()
			end
		)
	end
end

-- Remove movie from cart X buttons
for i = 1, 5 do
	kernel.registeractorhitnotify(
		"cancel_" .. tostring( i ),
		function()
			view_analytics_event( "buttonPress", "shopping-cart-remove-item" )
			selected_movie = "selectedmovie_" .. tostring( i )
			remove_movie(20)
			highlight_alphabets()
			if browse_filter_alpha_selected ~= nil then
				kernel.setactorforegroundcolor(browse_filter_alpha_selected.."_label","black")
				kernel.setactorbackgroundcolor(browse_filter_alpha_selected.."_label","silver")
				kernel.setactortag(browse_filter_alpha_selected.."_label","black")
			end
		end
	)
end

kernel.registeractorhitnotify(
	"abc_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,alpha" )
		set_number = 1
		browse_filter_alpha_selected = nil
		browse_selected_sort = "alpha"
		refresh_barriers()
		show_browse_view(18)
		highlight_selected_alphabets(nil)
	end
)
kernel.registeractorhitnotify(
	"newest_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Newest" )
		set_number = 1
		browse_selected_sort = "Newest"
		browse_filter_mode = "all"
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
		browse_selected_sort = "for-sale"
		browse_filter_mode = "for-sale"
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
		browse_selected_sort = "coming-soon"
		browse_filter_mode = "coming-soon"
		browse_filter_family_tracker:set_family_type( "ALL" )
		show_browse_view(10)
		kernel.executescript("browse_products")
	end
)
kernel.registeractorhitnotify(
	"blu_ray_button",
	function()
		view_analytics_event( "filterChange", "browse-filter-family,Blu-ray" )
		set_number = 1
		browse_selected_sort = "Blu-ray"
		browse_filter_mode = "all"
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
		browse_selected_sort = "Xbox 360"
		browse_filter_mode = "all"
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
		browse_selected_sort = "Xbox One"
		browse_filter_mode = "all"
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
		browse_selected_sort = "PS3"
		browse_filter_mode = "all"
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
		browse_selected_sort = "PS4"
		browse_filter_mode = "all"
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
		browse_selected_sort = "Wii"
		browse_filter_mode = "all"
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
		browse_selected_sort = "Wii U"
		browse_filter_mode = "all"
		browse_filter_alpha_selected = nil
		browse_filter_family_tracker:set_family_type( "Wii U" )
		kernel.executescript("browse_products")
	end
)

local alpha_selected
if browse_filter_alpha_selected ~= nil then
	alpha_selected = browse_filter_alpha_selected
	browse_filter_alpha_selected = nil
end

show_browse_view(18)
check_product_availability()
if alpha_selected ~= nil then
	browse_filter_alpha_selected = alpha_selected
end

highlight_alphabets()
if browse_filter_alpha_selected ~= nil then
	if browse_filter_alpha_selected == "0-9" then
		kernel.executehitnotify("#_label")
	else
		kernel.executehitnotify(browse_filter_alpha_selected .."_label")
	end
end

load_shopping_cart()
idle_timer:start()

for i = 1, 7 do
	local genre = ProductFamiliesMetaData[ browse_filter_family_tracker.selected_family ].filter_genres[i]
	if browse_filter_genre_selected == genre then
		highlight_tab(i)
		break
	end
end

RegisterProductFamilyTabs()

if #ShoppingCart.items == 5
and browse_is_cart_full_open == false
and shopping_cart_full_message_shown == false then
	show_shopping_cart_full()
end