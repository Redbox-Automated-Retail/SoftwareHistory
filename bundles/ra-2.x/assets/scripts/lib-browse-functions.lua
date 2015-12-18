--
-- lib-browse-functions.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

browse_filtered_products_table = {}
local x=80
local y=80
local rated_movie_in_cart = false
local blu_ray_movie_in_cart = false
local rated_game_in_cart = false
local subplatform_game_in_cart = false
local purchase_product_in_cart = false
local subplatform_message = ""


AlphaRanges = {
	{ range_name = "A - D", ranges = { A = true, B = true, C = true, D = true } },
	{ range_name = "E - H", ranges = { E = true, F = true, G = true, H = true } },
	{ range_name = "I - L", ranges = { I = true, J = true, K = true, L = true } },
	{ range_name = "M - O", ranges = { M = true, N = true, O = true } },
	{ range_name = "P - S", ranges = { P = true, Q = true, R = true, S = true } },
	{ range_name = "T - W", ranges = { T = true, U = true, V = true, W = true } },
	{ range_name = "X - Z", ranges = { X = true, Y = true, Z = true } },
	{ range_name = "0 - 9", ranges = { [0] = true, [1] = true, [2] = true, [3] = true, [4] = true, [5] = true, [6] = true, [7] = true, [8] = true, [9] = true } }
}

-- TODO:  It might just be easier to put this is a table resource and localize the resource
function SetESRBRatings()
	ESRBRatings = {
		[14] = { rating_id = 14, name = "EC",   code = "EC", longname = strings.esrb_earlychildhood },
		[15] = { rating_id = 15, name = "E",    code = "E",  longname = strings.esrb_everyone },
		[16] = { rating_id = 16, name = "E 10+", code = "E10+",  longname = strings.esrb_everyone_10_plus },
		[17] = { rating_id = 17, name = "T",    code = "T",  longname = strings.esrb_teen },
		[18] = { rating_id = 18, name = "M (17+)", code = "M",  longname = strings.esrb_mature },
		[19] = { rating_id = 19, name = "RP", code = "RP", longname = strings.esrb_rating_pending },
		[20] = { rating_id = 20, name = "AO (18+)", code = "AO", longname = strings.esrb_adults_only }
	}
end

ProductTypesMetaData = {
	[1] = { is_filter = true,
			checkout_resource_name = "checkout_dvd" },

	[2] = { is_filter = true,
			checkout_resource_name = "checkout_blu_ray",
			banner_resource_name = "browse_banner_blu_ray",
			detail_banner_resource_name = "detail_banner_blu_ray"},

	[3] = { is_filter = true,
			checkout_resource_name = "checkout_xbox",
			banner_resource_name = "browse_banner_xbox",
			detail_banner_resource_name = "detail_banner_xbox" },

	[4] = { is_filter = true,
			checkout_resource_name = "checkout_ps2",
			banner_resource_name = "browse_banner_ps2",
			detail_banner_resource_name = "detail_banner_ps2" },

	[5] = { is_filter = true,
			checkout_resource_name = "checkout_ps3",
			banner_resource_name = "browse_banner_ps3",
			detail_banner_resource_name = "detail_banner_ps3" },

	-- TODO: These image resources are wrong
	[6] = { is_filter = false,
			checkout_resource_name = "checkout_ps3",
			banner_resource_name = "browse_banner_dvd",
			detail_banner_resource_name = "detail_banner_dvd" },

	[7] = { is_filter = true,
			checkout_resource_name = "checkout_wii",
			banner_resource_name = "browse_banner_wii",
			detail_banner_resource_name = "detail_banner_wii" },

	-- TODO: These image resources are wrong
	[8] = { is_filter = true,
			checkout_resource_name = "checkout_wii",
			banner_resource_name = "browse_banner_wii",
			detail_banner_resource_name = "detail_banner_wii" },

	-- TODO: These image resources are wrong
	[9] = { is_filter = false,
			checkout_resource_name = "checkout_xbox",
			banner_resource_name = "browse_banner_xbox",
			detail_banner_resource_name = "detail_banner_xbox" },

	[10] = { is_filter = true,
			checkout_resource_name = "checkout_wii_u",
			banner_resource_name = "browse_banner_wii_u",
			detail_banner_resource_name = "detail_banner_wii_u" },

	[11] = { is_filter = true,
			checkout_resource_name = "checkout_ps4",
			banner_resource_name = "browse_banner_ps4",
			detail_banner_resource_name = "detail_banner_ps4" },
			
	[12] = { is_filter = true,
			checkout_resource_name = "checkout_xbox_one",
			banner_resource_name = "browse_banner_xbox_one",
			detail_banner_resource_name = "detail_banner_xbox_one" }
	
}

ProductFamiliesMetaData = {
	["Movies"] = {
		filter_genres = {
			[1] = ALL,
			[2] = 1093,
			[3] = 1000,
			[4] = 1005,
			[5] = 1009,
			[6] = 1004,
			[7] = 1006
		}
	},
	["Games"] = {
		filter_genres = {
			[1] = nil,
			[2] = 1026,
			[3] = 1094,
			[4] = 1028,
			[5] = 1030,
			[6] = 1034,
			[7] = 1036
		}
	}
}

SubPlatformIndicatorData = {
	[1] = { sub_platform_id = 1,
			sub_platform_name = "Wii Motion Plus Compatible",
			sub_platform_image = "image_wii_motion_plus_compatible" },
	[2] = { sub_platform_id = 2,
			sub_platform_name = "Wii Motion Plus Required",
			sub_platform_image = "image_wii_motion_plus_required" },
	[3] = { sub_platform_id = 3,
			sub_platform_name = "Wii Fit Compatible",
			sub_platform_image = "image_wii_fit_compatible" },
	[4] = { sub_platform_id = 4,
			sub_platform_name = "Wii Fit Required",
			sub_platform_image = "image_wii_fit_required" },
	[5] = { sub_platform_id = 5,
			sub_platform_name = "Move Compatible",
			sub_platform_image = "image_move_compatible" },
	[6] = { sub_platform_id = 6,
			sub_platform_name = "Move Required",
			sub_platform_image = "image_move_required" },
	[7] = { sub_platform_id = 7,
			sub_platform_name = "Kinect Compatible",
			sub_platform_image = "image_kinect_compatible" },
	[8] = { sub_platform_id = 8,
			sub_platform_name = "Kinect Required",
			sub_platform_image = "image_kinect_required" },
	[9] = { sub_platform_id = 9,
			sub_platform_name = "Nunchuck for Wii U",
			sub_platform_image = "image_nunchuk_required" },
	[10] = { sub_platform_id = 10,
			sub_platform_name = "Wiimote for Wii U",
			sub_platform_image = "image_wii_mote_required" },
	[11] = { sub_platform_id = 11,
			sub_platform_name = "PS4 Move Compatible",
			sub_platform_image = "image_ps4_move_compatible" },
	[12] = { sub_platform_id = 12,
			sub_platform_name = "PS4 Move Required",
			sub_platform_image = "image_ps4_move_required" },
	[13] = { sub_platform_id = 13,
			sub_platform_name = "PS4 Camera Required",
			sub_platform_image = "image_ps4_camera_required" },
	[14] = { sub_platform_id = 14,
			sub_platform_name = "Xbox One Kinect Compatible",
			sub_platform_image = "image_kinect_compatible_xbox_one" },
	[15] = { sub_platform_id = 15,
			sub_platform_name = "Xbox One Kinect Required",
			sub_platform_image = "image_kinect_required_xbox_one" }
}

function show_or_hide_for_sale_button()
	if EnableSellThruOfUsed == true then
		if browse_filter_family_tracker.selected_family == "Movies" then
			kernel.showactor( "for_sale_button" )
			kernel.setactorx( "coming_soon_button", 727)
		else
			if GamesSellThru == true then
				kernel.showactor( "for_sale_button" )
			else
				kernel.hideactor( "for_sale_button" )
			end
		end
	else
		if browse_filter_family_tracker.selected_family == "Movies" then
			kernel.hideactor( "for_sale_button" )
			kernel.setactorx( "coming_soon_button", 617 )
		else
			kernel.hideactor( "for_sale_button" )
		end
	end
end

ProductFamilies.new_selection_tracker = function( self, default )
	local t = {
		selected_family = default or "Movies",

		get_family_type = function( self )
			return self.families[ self.selected_family ]
		end,

		set_family_type = function( self, v )
			self.families[ self.selected_family ] = v
		end,

		is_match = function( self, f, v )
			local family_type = self:get_family_type()
			return f == self.selected_family
				and ( family_type == "ALL" or family_type == v )
		end,

		families = {}
	}
	for k, v in self:pairs() do
		t.families[ v.product_family_name ] = "ALL"
	end
	return t
end

function show_browse_view( total_count )
    highlight_sort_button( kernel.get_session_value("BrowseState.BrowseSelectedSort") )
	ProductCatalog:show_filtered_grid()
	if (kernel.getfilter( "ada_accessible" ) == "True") then
		draw_ada_control()
	end
end

game_allowed_to_be_shown = function( product_type )
	if product_type ~= nil 
	and (( GameEnablePS3 == false
	and product_type.product_type_name == 'PS3' ) 
	or ( GameEnablePS4 == false
	and product_type.product_type_name == 'PS4' ) 
	or ( GameEnableWii == false
	and product_type.product_type_name == 'Wii' ) 
	or ( GameEnableWiiU == false
	and product_type.product_type_name == 'Wii U' ) 
	or ( GameEnableXbox360 == false
	and product_type.product_type_name == 'Xbox 360' ) 
	or ( GameEnableXboxOne == false
	and product_type.product_type_name == 'Xbox One' )) then
		return false
	end
	return true
end

ProductCatalog.filter_products = function(self)
	local _in_cart = GetInCartPromoAction()
	local _incart_include_titles = {}
	local _incart_exclude_titles = {}
	if ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_promo_offer_cancel == false
	and _in_cart ~= nil then
		if _in_cart.product_id_list ~= nil then
			-- Build in-cart include ONLY these titles list
			local title_list = string.split( _in_cart.product_id_list, "," )
			for _, v in ipairs( title_list ) do
				if tonumber(v) ~= nil then
					_incart_include_titles[ tonumber( v ) ] = true
				end
			end
		else
			_incart_include_titles = nil
		end
		if _in_cart.exclude_id_list ~= nil
		and #_in_cart.exclude_id_list > 0 then
			-- Build in-cart exclude these titles list
			local title_list = string.split( _in_cart.exclude_id_list, "," )
			for _, v in ipairs( title_list ) do
				if tonumber(v) ~= nil then
					_incart_exclude_titles[ tonumber( v ) ] = true
				end
			end
		else
			_incart_exclude_titles = nil
		end
	else
		_incart_include_titles = nil
		_incart_exclude_titles = nil
	end
	
	return kernel.rental_get_products(
		kernel.get_session_value("BrowseState.BrowseFilterMode"),
		browse_filter_family_tracker.selected_family,
		browse_filter_family_tracker:get_family_type(),
        kernel.get_session_value("BrowseState.BrowseSelectedSort"),
		browse_filter_genre_selected,
		_incart_include_titles,
		_incart_exclude_titles,
		kernel.getfilter( "ada_accessible" ) == "True")
		
end

ProductCatalog.filter_grid = function( self )

	browse_filtered_products = self:filter_products()
    if kernel.get_session_value("BrowseState.BrowseSelectedSort") ~= "alpha" then
		local In_stock_products = {}
		local Out_of_stock_products = {}
		for i, v in ipairs( browse_filtered_products ) do
			if (v.in_stock == false and v.show_out_of_stock == true) then
				table.insert( Out_of_stock_products, v )
			else
				table.insert( In_stock_products, v )
			end
		end

		for i, v in ipairs( Out_of_stock_products ) do
			table.insert( In_stock_products, v )
		end
		browse_filtered_products = In_stock_products
	end
	alpha_filtered_products = browse_filtered_products

	return browse_filtered_products
end

ProductCatalog.get_default_family = function( self )
	local many_families, default_family = self:has_many_families()
	if many_families == false then
		browse_filter_family_tracker.selected_family = default_family or browse_filter_family_tracker.selected_family
	end
end

ProductCatalog.show_filtered_grid = function( self )
	if ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_title_marketing == true
	then
		self:set_title_marketing_view()
		self:filter_grid()
		self:show_selection_grid()
	else
		self:filter_grid()
		self:show_sort_buttons()
		self:show_selection_grid()
	end
end

ProductCatalog.set_title_marketing_view = function( self )
	-- Change the look of the screen
	kernel.setactory( "bg_box_top", 0 )
	kernel.setactorheight( "bg_box_top", 567 )
	kernel.setactorgradienttargetcolor("bg_box_top", {["r"] = 143, ["g"] = 10, ["b"] = 10})
	kernel.setactorbackgroundcolor("bg_box_top", {["r"] = 0, ["g"] = 0, ["b"] = 0})
	kernel.hideactor( "bg_box_bottom" )
	kernel.setactortext( "title_marketing_h1", kernel.getmacro( "InCartPromoHeaderMessage" ) )
	kernel.showactor( "title_marketing_h1" )

	-- Hide all sort and filter buttons
	kernel.hideactor( "newest_button" )
	kernel.hideactor( "abc_button" )
	kernel.hideactor( "blu_ray_button" )
	kernel.hideactor( "coming_soon_button" )
	kernel.hideactor( "xbox_360_button" )
	kernel.hideactor( "xbox_one_button" )
	kernel.hideactor( "ps3_button" )
	kernel.hideactor( "ps4_button" )
	kernel.hideactor( "wii_button" )
	kernel.hideactor( "wii_u_button" )

	-- Hide the start over button
	kernel.hideactor( "startover_button" )
	kernel.hideactor( "startover_arrow" )
end

draw_image = function( product_to_draw, control_to_draw )
	if browse_filter_family_tracker.selected_family == "Movies" then
		source_image_name = "image_movie_missing_box_art"
	elseif browse_filter_family_tracker.selected_family == "Games" then
		source_image_name = "image_game_missing_box_art"
	end

	if product_to_draw ~= nil then
		local image_registered = kernel.dataserviceregisterimage(	product_to_draw.image_file,	product_to_draw.image_file )
		if image_registered then
			source_image_name = string.format( "cache:%s", product_to_draw.image_file )
		else
			if ProductTypes[product_to_draw.product_type_id].product_family_id == 1 then
				source_image_name = "image_movie_missing_box_art"
			elseif ProductTypes[product_to_draw.product_type_id].product_family_id == 2 then
				source_image_name = "image_game_missing_box_art"
			end
		end
		kernel.showactor( control_to_draw )
		kernel.setactorimage( control_to_draw, source_image_name )
		kernel.setactortext( control_to_draw, source_image_name )
	end
end

is_product_selected = function( product )
	return ShoppingCart:contains_all_title_rollup_movie_group( product )
end

is_product_id_selected = function( product_id )
	local is_selected = false
	for k, v in ipairs( ShoppingCart.items ) do
		if v.product_id == product_id then
			is_selected = true
			break
		end
	end
	return is_selected
end

function highlight_sort_button( sort_name )
	if TextToSpeechEnabled == true then
		kernel.setmacro( "TTS:BrowsingFilter", sort_name )
	end

	local width = kernel.getactorwidth( "selected_button" )
	show_or_hide_for_sale_button()
	kernel.setactorwidth( "selected_button", 94 )
	local _showselectedbutton = function( name )
		kernel.setmacro( "SortName", kernel.getactortext( name .. "_button" ))
		kernel.showactor( "selected_button" )
		kernel.hideactor( name .. "_button" )
	end

	if ShoppingCart.in_cart_title_marketing == true then
		-- These buttons are hidden in the Title Marketing view
		return
	end

	if sort_name == "alpha" then
		sort_name = "abc"
		kernel.setactorx( "selected_button", kernel.getactorx( "abc_button") )
		width = kernel.getactorwidth( "abc_button" )
		if browse_filter_family_tracker.selected_family == "Movies"  then
			kernel.showactor( "blu_ray_button" )
			kernel.showactor( "coming_soon_button" )
		end
	elseif sort_name == "Newest" then
		sort_name = "newest"
		kernel.setactorx( "selected_button", kernel.getactorx( "newest_button") )
		width = kernel.getactorwidth( "newest_button" )
		if browse_filter_family_tracker.selected_family == "Movies"  then
			if ProductCatalog:is_product_available( "Newest" ) then
				kernel.showactor( "blu_ray_button" )
				kernel.showactor( "coming_soon_button" )
			end
		end
	elseif sort_name == "Blu-ray" then
		sort_name = "blu_ray"
		kernel.setactorx( "selected_button", kernel.getactorx( "blu_ray_button" ) )
		width = kernel.getactorwidth( "blu_ray_button" )
		kernel.showactor( "newest_button" )
		kernel.showactor( "coming_soon_button" )
	elseif sort_name == "for-sale" then
		sort_name = "for_sale"
		kernel.setactorx( "selected_button", kernel.getactorx( "for_sale_button") )
		width = kernel.getactorwidth( "for_sale_button" )
		if browse_filter_family_tracker.selected_family == "Movies"  then
			kernel.showactor( "blu_ray_button" )
			kernel.showactor( "newest_button" )
			kernel.showactor( "coming_soon_button" )
		end
	elseif sort_name == "Xbox 360" then
		sort_name = "xbox_360"
		kernel.setactorx( "selected_button", kernel.getactorx( "xbox_360_button") )
		width = kernel.getactorwidth( "xbox_360_button" )
	elseif sort_name == "Xbox One" then
		sort_name = "xbox_one"
		kernel.setactorx( "selected_button", kernel.getactorx( "xbox_one_button") )
		width = kernel.getactorwidth( "xbox_one_button" )
	elseif sort_name == "PS3" then
		kernel.setactorx( "selected_button", kernel.getactorx( "ps3_button") )
		width = kernel.getactorwidth( "ps3_button" )
	elseif sort_name == "PS4" then
		kernel.setactorx( "selected_button", kernel.getactorx( "ps4_button") )
		width = kernel.getactorwidth( "ps4_button" )
	elseif sort_name == "Wii" then
		kernel.setactorx( "selected_button", kernel.getactorx( "wii_button") )
		width = kernel.getactorwidth( "wii_button" )
	elseif sort_name == "Wii U" then
		sort_name = "wii_u"
		kernel.setactorx( "selected_button", kernel.getactorx( "wii_u_button") )
		width = kernel.getactorwidth( "wii_u_button" )
	elseif sort_name == "coming-soon" then
		sort_name = "coming_soon"
		kernel.setactorx( "selected_button", kernel.getactorx( "coming_soon_button" ) )
		width = kernel.getactorwidth( "coming_soon_button" )
		if browse_filter_family_tracker.selected_family == "Movies" then
			kernel.showactor( "newest_button" )
			kernel.showactor( "blu_ray_button" )
		end
	else
		sort_name = string.lower(sort_name)
	end
	kernel.setactorwidth( "selected_button", width )
	_showselectedbutton( sort_name )

	local _highlight = function( name, foreground, background, gradient_target )
		kernel.setactorforegroundcolor( name, foreground )
		kernel.setactorbackgroundcolor( name, background )
		kernel.setactorgradienttargetcolor( name, gradient_target )
	end

	local bright_gray = {["r"] = 219, ["g"] = 219,["b"] = 219}
	local light_gray = {["r"] = 155, ["g"] = 155,["b"] = 155}
	local med_gray = {["r"] = 111, ["g"] = 111,["b"] = 111}
	local dark_gray = {["r"] = 70, ["g"] = 70,["b"] = 70}

	_highlight( "abc_button", "white", med_gray, dark_gray )
	_highlight( "coming_soon_button", "white",  med_gray, dark_gray )
	_highlight( "blu_ray_button", "white",  med_gray, dark_gray )
	_highlight( "newest_button", "white",  med_gray, dark_gray )
	_highlight( "for_sale_button", "white",  med_gray, dark_gray )
	_highlight( "xbox_360_button", "white",  med_gray, dark_gray )
	_highlight( "xbox_one_button", "white",  med_gray, dark_gray )
	_highlight( "ps3_button", "white",  med_gray, dark_gray )
	_highlight( "ps4_button", "white",  med_gray, dark_gray )
	_highlight( "wii_button", "white",  med_gray, dark_gray )
	_highlight( "wii_u_button", "white",  med_gray, dark_gray )

	kernel.setbordercolor( "startover_button", "white" )

	_highlight( sort_name .. "_button", "black", light_gray, bright_gray )
end

check_product_availability = function()

	if ShoppingCart.in_cart_title_marketing == true then
		-- These buttons are hidden in the Title Marketing view
		return
	end

    highlight_sort_button( kernel.get_session_value("BrowseState.BrowseSelectedSort") )
end

ProductCatalog.is_product_available = function( self, sort_name )
	--cache globals to locals for improved performance
	local _ProductTypes = ProductTypes
	local _ShoppingCart = ShoppingCart
	local _ProductFamilies = ProductFamilies
	local _History = ProductHistory
	local _MasterProductCatalog = MasterProductCatalog
	local products = {}
	for k, v in self:pairs() do
		local product_type = nil
		local product = _MasterProductCatalog[v.product_id]
		local history = _History[v.product_id]
		if product ~= nil then
			product_type = _ProductTypes[product.product_type_id]
		end
		if  product ~= nil
		and product_type ~= nil
		and history ~= nil then
			local product_family = _ProductFamilies[product_type.product_family_id]

			if    product_family.product_family_name == browse_filter_family_tracker.selected_family
			and  ( browse_filter_genre_selected == nil or browse_filter_genre_selected == "ALL" or product.genres[browse_filter_genre_selected] == true )
			and  ( browse_rent_or_sellthru == "rent" or ( browse_rent_or_sellthru == "sellthru" and product.can_be_sold) ) then
				if sort_name == "Newest" then
					return true
				elseif sort_name == "coming-soon" then
					if product.is_available == false then
						kernel.setbordercolor( "coming_soon_button", {["r"] = 157, ["g"] = 157,["b"] = 157} )
						return true
					end
				elseif sort_name == "for-sale" then
					if browse_filter_family_tracker.selected_family == "Movies" then
						local price_purchase = StoreManager:get_purchase_price()
						if 	price_purchase ~= nil
						and price_purchase > 0
						and product.sell_thru == true
						and product.can_be_sold  then
							return true
						end
					else
						local price_purchase = StoreManager.PricingManager:get_adjusted_price( product.sellthru_price )
						if 	price_purchase ~= nil
						and price_purchase > 0
						and product.sell_thru == true
						and product.sellthru_date ~= nil
						and product.sellthru_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
						and product.can_be_sold  then
							return true
						end
					end
				else
					if product_type.product_type_name == sort_name then
						return true
					end
				end
			end
		else
			--kernel.log( "WARNING! ProductCatalog.filter_products couldn't find: v.product_id = " .. tostring( v.product_id ) )
		end
	end

	return false

end

ProductCatalog.show_sort_buttons = function( self )

	if ShoppingCart.in_cart_title_marketing == true then
		-- These buttons are hidden in the Title Marketing view
		return
	end

	kernel.setactorbackgroundcolor( "center_gray_background", {["r"] = 167, ["g"] = 167, ["b"] = 167} )
	kernel.setactorgradienttargetcolor( "center_gray_background", {["r"] = 167, ["g"] = 167, ["b"] = 167} )
	
	if TextToSpeechEnabled == true then
		if browse_filter_family_tracker.selected_family == "Movies" then
			kernel.setmacro( "TTS:SwitchToFamily", strings.Games )
			kernel.setmacro( "TTS:BrowsingFamily", strings.Movies )
			kernel.setmacro( "TTS:BrowsingFamilySingular", "movie" )
		else
			kernel.setmacro( "TTS:SwitchToFamily", strings.Movies )
			kernel.setmacro( "TTS:BrowsingFamily", strings.Games )
			kernel.setmacro( "TTS:BrowsingFamilySingular", "game" )
		end
		kernel.setmacro( "TTS:FilterOptions", "Various Options" )
		kernel.ttsrepeat() -- assets have been set and toggled
	end
	
	if browse_filter_family_tracker.selected_family == "Movies" then
		kernel.showactor( "abc_button" )
		kernel.showactor( "blu_ray_button" )
		kernel.showactor( "coming_soon_button" )
		kernel.hideactor( "xbox_360_button" )
		kernel.hideactor( "xbox_one_button" )
		kernel.hideactor( "ps3_button" )
		kernel.hideactor( "ps4_button" )
		kernel.hideactor( "wii_button" )
		kernel.hideactor( "wii_u_button" )
		kernel.setbordercolor( "abc_button", "white" )

		if not DisableGames then
			kernel.log( "Games are enabled." )
			if ProductCatalog:in_stock_games_count() == 0 then
				kernel.log( "Not Enough games in kiosk. Disabling Games tab." )
				--disable_games()
			else
				--enable_games()
			end
		else
			kernel.log( "Games are disabled. Disabling Games tab." )
			--disable_games()
		end

		if TextToSpeechEnabled == true then
			kernel.setmacro( "TTS:TabName_2", strings.genre_1093a .. " " .. strings.genre_1093b )
			kernel.setmacro( "TTS:TabName_3", strings.genre_1000 )
			kernel.setmacro( "TTS:TabName_4", strings.genre_1005 )
			kernel.setmacro( "TTS:TabName_5", strings.genre_1009 )
			kernel.setmacro( "TTS:TabName_6", strings.genre_1004 )
			kernel.setmacro( "TTS:TabName_7", strings.genre_1006 )
		end

		if kernel.getfilter( "ada_accessible" ) == "True" then
			kernel.setactorx( "blu_ray_button", 342 )
			kernel.setactorx( "newest_button", 452 )
			kernel.setactorx( "for_sale_button", 562  )
			kernel.setactorx( "coming_soon_button", 662 )
		else
			kernel.setactorx( "blu_ray_button", 287 )
			kernel.setactorx( "newest_button", 397 )
			kernel.setactorx( "abc_button", 507 )
			kernel.setactorx( "for_sale_button", 617  )
			kernel.setactorx( "coming_soon_button", 727 )
		end

		kernel.setbordercolor( "abc_button", "white" )
		kernel.setbordercolor( "coming_soon_button", "white" )
		kernel.setbordercolor( "blu_ray_button", "white" )
		kernel.setbordercolor( "newest_button", "white" )
		kernel.setbordercolor( "for_sale_button", "white" )

        if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
			kernel.setactorbackgroundcolor( "bg_box_top", "black" )
		else
			kernel.setactorbackgroundcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorbackgroundcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactory( "bg_box_bottom", 318 )
			kernel.setactory( "center_gray_background", 563 )
		end
	else
		-- GAMES
		kernel.hideactor( "blu_ray_button" )
		kernel.hideactor( "abc_button" )

		local total_width = 0
		local padding = 8
		local set_position_array = {}
		
		local enable_filter_button = function( button_name )
			if button_name == nil or button_name == "" then
				return
			end
			kernel.showactor( button_name )
			kernel.setbordercolor( button_name, "white" )
			local width = kernel.getactorwidth( button_name )
			total_width = total_width + width + padding
			
			table.insert( set_position_array, 
							{ 
								name = button_name,
								button_width = width
							} )
		end
		
		local align_filter_buttons = function()

			local half_total_width = total_width / 2
			local center = 592  -- ( 1024 + 160 (offset to right) ) / 2
			local start_loc = center + half_total_width
			for _, button in ipairs( set_position_array ) do
				local name = button["name"]
				local item_width = button["button_width"]
				start_loc = start_loc - item_width - padding
				kernel.setactorx( name, start_loc )
			end
			
		end
		
		enable_filter_button( "coming_soon_button" )
		if EnableSellThruOfUsed == true and GamesSellThru == true then
			enable_filter_button( "for_sale_button" )
		else
			kernel.hideactor( "for_sale_button" )
		end
		if GameEnableWiiU == true and GameHasContentWiiU == true then
			enable_filter_button( "wii_u_button" )
		else
			kernel.hideactor( "wii_u_button" )
		end
		if GameEnableWii == true and GameHasContentWii == true then
			enable_filter_button( "wii_button" )
		else
			kernel.hideactor( "wii_button" )
		end
		if GameEnablePS4 == true and GameHasContentPS4 == true then
			enable_filter_button( "ps4_button" )
		else
			kernel.hideactor( "ps4_button" )
		end
		if GameEnablePS3 == true and GameHasContentPS3 == true then
			enable_filter_button( "ps3_button" )
		else
			kernel.hideactor( "ps3_button" )
		end
		if GameEnableXboxOne == true and GameHasContentXboxOne == true then
			enable_filter_button( "xbox_one_button" )
		else
			kernel.hideactor( "xbox_one_button" )
		end
		if GameEnableXbox360 == true and GameHasContentXbox360 == true then
			enable_filter_button( "xbox_360_button" )
		else
			kernel.hideactor( "xbox_360_button" )
		end
	
		enable_filter_button( "newest_button" )
		align_filter_buttons() --sets x coords in centered manner
		
		if TextToSpeechEnabled == true then
			kernel.setmacro( "TTS:TabName_2", strings.genre_1026 )
			kernel.setmacro( "TTS:TabName_3", strings.genre_1006 )
			kernel.setmacro( "TTS:TabName_4", strings.genre_1028 )
			kernel.setmacro( "TTS:TabName_5", strings.genre_1030 )
			kernel.setmacro( "TTS:TabName_6", strings.genre_1034 )
			kernel.setmacro( "TTS:TabName_7", strings.genre_1036 )
		end

		kernel.setactorbackgroundcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
		kernel.setactorbackgroundcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
		kernel.setactorgradienttargetcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
		kernel.setactorgradienttargetcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
		kernel.setactory( "bg_box_bottom", 318 )
		kernel.setactory( "center_gray_background", 563 )
	end
	
	if TextToSpeechEnabled == true then
		-- Set TTS:Filter buttons based on what are showing
		local filter_num = 1
		if kernel.isactorvisible( "blu_ray_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("blu_ray_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "blu_ray_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "newest_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("newest_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "newest_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "abc_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("abc_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "abc_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "for_sale_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("for_sale_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "for_sale_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "xbox_360_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("xbox_360_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "xbox_360_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "xbox_one_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("xbox_one_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "xbox_one_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "ps3_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("ps3_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "ps3_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "ps4_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("ps4_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "ps4_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "wii_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("wii_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "wii_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "wii_u_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("wii_u_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "wii_u_button" )
			filter_num = filter_num + 1
		end
		if kernel.isactorvisible( "coming_soon_button" ) then
			kernel.setmacro( "TTS:FilterName_" .. tostring(filter_num), kernel.getactortext("coming_soon_button") )
			kernel.setmacro( "TTS:FilterButton_" .. tostring(filter_num), "coming_soon_button" )
			filter_num = filter_num + 1
		end

		if TextToSpeechEnabled == true then
			kernel.ttsrepeat();  -- the titles and assests have been set and toggled
		end
	end
	
    highlight_sort_button( kernel.get_session_value("BrowseState.BrowseSelectedSort") )
end

function display_banner( type_name, productfamilytype, actor_count ) --Matt Will there be banners? no longer used?
	if ( type_name == "Blu-ray"
	or type_name == "Xbox 360"
	or type_name == "PS2"
	or type_name == "PS3"
	or type_name == "Wii"
	or type_name == "Nintendo DS" )
	and productfamilytype == 1 then
		kernel.showactor( "banner_" .. tostring( actor_count ) )
		kernel.setactortext( "banner_" .. tostring( actor_count ), type_name )
		if type_name == "Blu-ray" then
			type_name = "blu_ray"
		elseif type_name == "Xbox 360" then
			type_name = "xbox"
		elseif type_name == "Nintendo DS" then
			type_name = "nintendo_ds"
		else
			type_name = string.lower( type_name )
		end
		kernel.setactorimage( "banner_" .. tostring( actor_count ), "image_banner_" .. type_name )
	else
		kernel.hideactor( "banner_" .. tostring( actor_count ) )
	end
end

alpha_filter_list = function( browse_alpha_products )
	local alpha_final_list = {}
	local alpha_products = browse_alpha_products
	local position = 1
	for i, k in ipairs( alpha_products ) do
		if i < #alpha_products then
			if i == 1 then
				table.insert( alpha_final_list, position, "" )
				position = position + 1
			end
			if	AlphaRanges[8].ranges[ tonumber( string.sub( alpha_products[ i + 1 ].sort_name, 0, 1 ) ) ] == nil
			and string.upper( string.sub( k.sort_name, 0, 1 ) ) ~= string.upper( string.sub( alpha_products[ i + 1 ].sort_name, 0, 1 ) ) then
				table.insert( alpha_final_list,position, k )
				position = position + 1
				table.insert( alpha_final_list, position, "" )
			else
				table.insert( alpha_final_list, position, k )
			end
			position = position + 1
		end
		if i == #alpha_products then
			table.insert( alpha_final_list, position, k )
		end
	end
	return alpha_final_list
end

show_product_detail_view = function(product)
	browse_selection_selected_product = product
	product_detail_shown_from_view = kernel.peekview()
	view_show( "product_detail_view" )
end

local page_number_changed = function(new_page_number)
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	if new_page_number < set_number then
		view_analytics_event( "pageChange", "browse-page-back" )
	else
		view_analytics_event( "pageChange", "browse-page-next" )
	end
	local page_number_is_different = set_number ~= new_page_number
	set_number = new_page_number
	if kernel.getfilter( "ada_accessible" ) == "True" then
		draw_ada_control()
	end

	if page_number_is_different then
		increment_paddle_click_count()
	end
end

increment_paddle_click_count = function ()
	-- collection popup browse paddle click counter logic
	if marketing_config ~= nil and marketing_config.collections_popup_paddle_clicks ~= nil and marketing_config.collections_popup_paddle_clicks > 0 then
		local paddle_click_count = kernel.add_to_session_value("BrowseState.PaddleClickCount", 1) 
		if paddle_click_count ~= nil and paddle_click_count >= marketing_config.collections_popup_paddle_clicks then
			kernel.set_session_value("BrowseState.PaddleClickCount", 0)
			check_collections_popup("interaction")
		end
	end
end

is_product_in_stock = function (product)
	return ProductCatalog:in_stock( product.product_id, ProductHistory[ product.product_id ] )
end

is_shopping_cart_full = function ()
	return #ShoppingCart.items == 5
end

update_products_shown = function( products_shown )
	view_analytics_event( "productsShown", tostring( products_shown ) )
end

get_test_price = function (product) 
	local result = nil
	if StoreManager.PricingManager:is_early_released( product )	and StoreManager.PricingManager:is_alternate_priced( product.product_id ) then
		local product_pricing = StoreManager.PricingManager:get_product_price( product.product_id )
		local first_night_string = "$3"
		if product_pricing.initial_night ~= nil then
			first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
		end
		result = first_night_string
	end
	return result
end

get_early_release_date2 = function (product)
	return StoreManager.PricingManager:get_early_release_date( product )
end

is_title_marketing_mode = function ()
	return ShoppingCart.in_cart_promo_offer_made == true and ShoppingCart.in_cart_title_marketing == true
end

is_coming_soon_item_registered = function (product)
	return ShoppingCart.coming_soon_optin:indexof_optin_item( product.product_id ) > 0
end

is_coming_soon_optin_enabled = function()
	return ComingSoonOptIn
end

register_email_reminder = function (product)
	if ShoppingCart.coming_soon_optin.email_address == nil then
		local coming_soon_optin_keyboard_manager = newkeyboardmanager()
		coming_soon_optin_keyboard_manager:init( 64 )
		coming_soon_optin_keyboard_manager:set_message( strings.coming_soon_email_optin_email_prompt_message )
		coming_soon_optin_keyboard_manager:set_marketing( strings.coming_soon_email_optin_email_marketing_message )
		show_email_confirmation = false
		coming_soon_optin_keyboard_manager:set_validate_event(
			function( text )
				local coming_soon_optin_email_validator = newemailvalidator()
				if coming_soon_optin_email_validator.validate_email( text ) == false then
					return strings.email_error_message
				end
				return nil
			end
		)
		coming_soon_optin_keyboard_manager:set_enter_event(
			function()
				ShoppingCart.coming_soon_optin.email_address = coming_soon_optin_keyboard_manager:get_value()
				table.insert( ShoppingCart.coming_soon_optin.optin_items,
					{
						item_id = tostring( product.product_id ),
						item_type = "KioskRentalTitleID",
						optin_method_type = "EmailAddress",
						optin_method_data = ShoppingCart.coming_soon_optin.email_address,
						template_name = "EmailTemplate",
						campaign_name = "ComingSoonRentalCampaign"
					}
				)
				view_goback_safe( "coming soon email optin email entry screen - next button" )
			end
		)
		coming_soon_optin_keyboard_manager:show( strings.email_tag )
	else
		table.insert( ShoppingCart.coming_soon_optin.optin_items,
			{
				item_id = tostring( product.product_id ),
				item_type = "KioskRentalTitleID",
				optin_method_type = "EmailAddress",
				optin_method_data = ShoppingCart.coming_soon_optin.email_address,
				template_name = "EmailTemplate",
				campaign_name = "ComingSoonRentalCampaign"
			}
		)
		draw_images_set( browse_filtered_products )
	end
end

unregister_email_reminder = function(product)
	table.remove( ShoppingCart.coming_soon_optin.optin_items, ShoppingCart.coming_soon_optin:indexof_optin_item( product.product_id ) )
	draw_images_set( browse_filtered_products )
end

draw_images_set = function( filtered_products_list )
	kernel.draw_images_set(
		"Main",
		browse_filter_family_tracker.selected_family,
		filtered_products_list,
		set_number,
		browse_goto_alpha,
		browse_filter_alpha_selected,
		page_number_changed,
		update_products_shown, 
		show_product_detail_view,
		add_movie_on_hit,
		nil,
		is_product_selected,
		is_product_in_stock,
		is_shopping_cart_full,
		get_test_price,
		get_early_release_date2,
		is_title_marketing_mode,
		is_coming_soon_optin_enabled,
		is_coming_soon_item_registered,
		register_email_reminder,
		unregister_email_reminder)

	browse_goto_alpha = nil
end 

ProductCatalog.show_selection_grid = function( self )
    if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
		browse_filtered_products_table = alpha_filter_list( browse_filtered_products )
		draw_images_set( browse_filtered_products_table )
	else
		draw_images_set( browse_filtered_products )
	end
end

get_product_from_actor = function( actor_name )
	return kernel.get_product_from_actor(actor_name)
end

--The full cart popup will be shown after a delay of 50 ms so that the browse screen completes rendering and all the movies in the mini cart are displayed.
fullcart_popup_timer = newidletimer( "fullcart_popup_timer" )
fullcart_popup_timer:initialize(
	50,
	function()
		view_show_safe( "cartfull_message_view" )
		fullcart_popup_timer:stop()
	end
)
show_shopping_cart_full = function()
	fullcart_popup_timer:start()
end

hide_shopping_cart_full = function()
	kernel.hideactor( "cart_full_background" )
	kernel.hideactor( "cart_full_label_border" )
	kernel.hideactor( "cart_full_label" )
	kernel.hideactor( "cart_full_label_text" )
	kernel.hideactor( "cart_full_image" )
	kernel.hideactor( "cart_full_cancel_button" )
    highlight_sort_button( kernel.get_session_value("BrowseState.BrowseSelectedSort") )
	browse_is_cart_full_open = false
end

add_movie = function( product_to_add )
	local selected_product = get_product_from_actor( selected_movie )
	if product_to_add ~= nil then
		selected_product = product_to_add
	end

	browse_selection_selected_product = selected_product
	--If Redbox empty case setting action as purchase
    if selected_product.product_id == 1139 or (kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and (kernel.peekview() == "browse_view" or kernel.peekview() == "purchase_message_view" or kernel.peekview() == "browse_view_ada_accessible")) then
		browse_selection_cart_action = "purchase"
	else
		browse_selection_cart_action = "rental"
	end
	kernel.executescript( "add_to_cart" )
 	refresh_current_page()
		
	if #ShoppingCart.items == 5
	and browse_is_cart_full_open == false
	and shopping_cart_full_message_shown == false then
		show_shopping_cart_full()
	end

end


function getmonth( month )
	local months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" }
	return months[ tonumber( month ) ]
end

refresh_current_page = function()
	if kernel.peekview() == "browse_view_collection" then
		draw_collection_view()
	elseif kernel.peekview() == "browse_view_collections" then
		draw_collections_view()
	elseif kernel.peekview() == "browse_view_ada_accessible" then
		draw_images_set( alpha_filtered_products )
		draw_action_bar()
		draw_ada_control()
    elseif kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
		draw_images_set( browse_filtered_products_table )
		draw_action_bar_and_mini_cart("browse_filter_alpha_view")
		highlight_alphabets()
	else
		draw_images_set( alpha_filtered_products )
		draw_action_bar_and_mini_cart("browse_view")
	end
end

remove_movie = function( selected_product )
	--removing movie from cart
	if (selected_product ~= nil) then
		for k, v in ipairs( ShoppingCart.items ) do
			if selected_product.product_id == v.product_id then
				ShoppingCart:remove_item( k )
				break
			end
		end
	end

	refresh_current_page()
	hide_shopping_cart_full()
    highlight_sort_button( kernel.get_session_value("BrowseState.BrowseSelectedSort") )

end

function add_item_to_cart( selected_product )
	if selected_product ~= nil
	and StoreManager.PricingManager:is_early_released( selected_product ) then

		local message_view_manager = newmessageviewmanager()
		message_view_manager:hide( true )
		message_view_manager:show()
		view_analytics_event( "context", "$3 Firsts warning message" )
		message_view_manager:hide_title()

		-- The message needs to include the prices from the actual price file (and MD adjustment if needed)
		local product_pricing = StoreManager.PricingManager:get_product_price( selected_product.product_id )

		local first_night_string = "$3"
		if product_pricing.initial_night ~= nil then
			first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
		end
		local extra_night_string = "$1"
		if product_pricing.extra_night ~= nil then
			extra_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.extra_night )
		end

		local message_text = string.gsub( strings.nonstandered_priced_item, "{EXTRANIGHT}", extra_night_string )
		message_view_manager:set_message_header( string.gsub( strings.nonstandered_priced_item_header, "{0}", first_night_string ) )
		message_view_manager:set_message_text( string.gsub( message_text, "{0}", first_night_string ) )

		message_view_manager:show_first_button(
			strings.back_button,
			function()
				message_view_manager:hide_message()
			end )
		message_view_manager:show_third_button(
			strings.add_to_cart,
			function()
				message_view_manager:hide()
				check_for_warning_popups( check_alert_table, selected_movie, i )
			end )
		message_view_manager:show_back_button_arrow(
			function()
				message_view_manager:hide_message()
			end )
		idle_timer:start()
	else
		check_for_warning_popups( check_alert_table, selected_movie, 11, selected_product )
	end
end

warning_popup_timer = newidletimer("warning_popup_timer")
local warning_popup_timeout = 15000
if TextToSpeechEnabled == true then
	warning_popup_timeout = 60000
end
warning_popup_timer:initialize(
	warning_popup_timeout,
	function()
		hide_warning_popup()
	end )

function check_for_warning_popups( check_alert_table, selected_movie, i, product_to_check )

	local _MasterProductCatalog = MasterProductCatalog
	local _ProductFamilies = ProductFamilies
	local _ProductTypes = ProductTypes
	local _SubPlatform = SubPlatform
	local selected_product = nil
	local product_type = nil
	local rating = nil
	local game_mature_message
	subplatform_game_in_cart = false
	purchase_product_in_cart = false
	blu_ray_movie_in_cart = false
	rated_movie_in_cart = false
	rated_game_in_cart = false

	if product_to_check ~= nil then
		selected_product = product_to_check
	elseif i == nil then
		selected_product = selected_movie
	else
		selected_product = get_product_from_actor( selected_movie )
	end

	if ProductCatalog:has_mature_rating( selected_product ) then
		if selected_product ~= nil then
			product_type = _ProductTypes[selected_product.product_type_id]
			if product_type ~= nil then
				product_family = _ProductFamilies[product_type.product_family_id]
				if product_family ~= nil then
					if product_family.product_family_name == "Movies" then
						if selected_product.genres[1017] ~= nil then
							rating = TVRatings[selected_product.rating_id]
						else
							rating = Ratings[selected_product.rating_id]
						end
						if rating ~= nil then
							local movie_mature_message = string.gsub( strings.mature_movie_warning,"{0}", rating.name )
							kernel.setmacro( "MatureWarning:Header", string.gsub( strings.esrb_mature_name,"{0}", rating.name ))
							kernel.setmacro( "MatureWarning:Message", movie_mature_message )
							rated_movie_in_cart = true
						end
					elseif product_family.product_family_name == "Games" then
						rating = ESRBRatings[selected_product.rating_id]
						if rating ~= nil then
							if kernel.getculture() == "en-US" then
								game_mature_message = string.gsub( string.gsub( strings.mature_game_warning,"{0}", rating.name ),"M","Mature" )
							else
								game_mature_message = string.gsub( string.gsub( strings.mature_game_warning,"{0}", rating.name ),"M","Maduro" )
							end
							kernel.setmacro( "MatureWarning:Header", string.gsub( strings.esrb_mature_game_name,"{0}", rating.code ))
							kernel.setmacro( "MatureWarning:Message", game_mature_message )
							rated_game_in_cart = true
						end
					end
				end
			end
		end
	end

	local product = _MasterProductCatalog[selected_product.product_id]
	product_type = _ProductTypes[selected_product.product_type_id]
	if product.sub_platform_id ~= nil then
		local sub_platform_id = product.sub_platform_id
		for a,b in _SubPlatform:pairs() do
			if product.sub_platform_id == _SubPlatform[a].subplatform_id then
				has_subplatform = true
				subplatform = string.gsub( b.subplatform_name, "Compatible", "" )
				subplatform = string.gsub( subplatform, "Required", "")
				subplatform = string.gsub( subplatform, "XboxOne ", "")
				subplatform = string.gsub( subplatform, "PS4 ", "")
				subplatform = string.gsub( subplatform, "Move", "Playstation Move" )
				subplatform = string.gsub( subplatform, "Eye Camera", "Playstation Eye Camera" )
				subplatform_message = ""
				subplatform_message = "\n"..subplatform..strings.subplatform_for.." "..product_type.product_type_name
			end
		end
		subplatform_game_in_cart = true
	end

	if selected_product.product_type_id == 2 then
		blu_ray_movie_in_cart = true
	end
	if browse_selection_cart_action == "purchase" and selected_product.product_id ~= 1139 then
		purchase_product_in_cart = true
	end
	show_warning_alert( check_alert_table, selected_movie, i, product_to_check )
end

local function show_warning_header()
	kernel.setactortext( "warning_back_button", strings.back_button )
	kernel.setactortext( "warning_continue_button", strings.okay_button )
	if kernel.getculture() == "es-MX" then
		kernel.hideactor( "warning_header_label" )
		kernel.showactor( "warning_header_label_es-MX" )
	else
		kernel.showactor( "warning_header_label" )
		kernel.hideactor( "warning_header_label_es-MX" )
	end
end

function show_warning_alert( check_alert_table, selected_movie, i, product_to_check )
	if ( check_alert_table.movie_mature_warning_shown == false and rated_movie_in_cart == true )
	or ( check_alert_table.mature_game_warning_shown == false and rated_game_in_cart == true ) then
		local analytics_view_name = "rrated_message_view"

		view_show_safe(analytics_view_name)
		show_warning_header()
		warning_popup_timer:start()
		if product_family ~= nil and product_family.product_family_name ~= nil then
			view_analytics_event( "context", tostring(product_family.product_family_name) )
		end
		view_analytics_event( "context", "matureWarning" )
		kernel.registeractorhitnotify(
			"warning_continue_button",
			function()
				if product_family.product_family_name == "Movies" then
					check_alert_table.movie_mature_warning_shown = true
				elseif product_family.product_family_name == "Games" then
					check_alert_table.mature_game_warning_shown = true
				end
				if blu_ray_movie_in_cart == true and check_alert_table.blu_ray_warning_shown == false then
					show_blu_ray_alert( check_alert_table, selected_movie, i, product_to_check )
					kernel.setactoropacity( "bluray_scene_background" , 0 )
				elseif subplatform_game_in_cart == true then
					show_sub_platform_warning( check_alert_table, selected_movie, i, product_to_check )
				elseif purchase_product_in_cart == true then
					show_purchase_warning( check_alert_table, selected_movie, i, product_to_check )
					kernel.setactoropacity( "purchase_scene_background" , 0 )
				else
					warning_popup_timer:stop()
					add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action, product_to_check )
				end
			end
		)
		kernel.registeractorhitnotify(
			"warning_back_button",
			function()
				warning_popup_timer:stop()
				view_unwindto_safe( item_added_from_view, true, analytics_view_name .. ":hide()" )
			end
		)
		kernel.registeractorhitnotify(
			"warning_back_button_arrow",
			function()
				warning_popup_timer:stop()
				view_unwindto_safe( item_added_from_view, true, analytics_view_name .. ":hide()" )
			end
		)
	elseif blu_ray_movie_in_cart == true and check_alert_table.blu_ray_warning_shown == false then
		show_blu_ray_alert( check_alert_table, selected_movie, i, product_to_check )
	elseif subplatform_game_in_cart == true then
		show_sub_platform_warning( check_alert_table, selected_movie, i, product_to_check )
	elseif purchase_product_in_cart == true then
		show_purchase_warning( check_alert_table, selected_movie, i, product_to_check )
	else
		add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action, product_to_check )
	end
end

function hide_warning_popup()
	view_unwindto_safe( item_added_from_view, true, "Message view Timed out." )
	warning_popup_timer:stop()
end

function show_warning_popup()
	warning_popup_timer:start()
	kernel.showactor( "scene_background" )
	kernel.showactor( "center_background" )
	kernel.showactor( "rating_label" )
	kernel.showactor( "message_label" )
	kernel.showactor( "mature_rating_back_button" )
	kernel.showactor( "mature_rating_back_button_arrow" )
	kernel.showactor( "continue_button" )
end

function show_blu_ray_alert( check_alert_table, selected_movie, i, product_to_check )
	warning_popup_timer:start()
	view_show_safe( "bluray_message_view" )
	warning_popup_timer:start()
	view_analytics_event( "context", "blurayWarning" )
	show_warning_header()

	kernel.setmacro( "MatureWarning:Header",strings.blu_ray_alert_title )
	kernel.setmacro( "Bluray:Message", strings.bly_ray_alert )

	kernel.registeractorhitnotify(
		"warning_continue_button",
		function()
			warning_popup_timer:stop()
			check_alert_table.blu_ray_warning_shown = true
			if purchase_product_in_cart == true then
				show_purchase_warning( check_alert_table, selected_movie, i, product_to_check )
				kernel.setactoropacity( "purchase_scene_background" , 0 )
					-- Need to bail here or else lower part of routine will hide the puchase warning before user sees it
				return
			else
				add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action, product_to_check )
			end
		end
	)
	kernel.registeractorhitnotify(
		"warning_back_button",
		function()
			warning_popup_timer:stop()
			kernel.setactoropacity( "bluray_scene_background" , 0.6 )
			view_unwindto_safe( item_added_from_view, true, "bluray_message_view:hide()" )
		end
	)
	kernel.registeractorhitnotify(
		"warning_backback_button_arrow",
		function()
			warning_popup_timer:stop()
			kernel.setactoropacity( "bluray_scene_background" , 0.6 )
			view_unwindto_safe( item_added_from_view, true, "bluray_message_view:hide()" )
		end
	)
end

function show_sub_platform_warning( check_alert_table, selected_movie, i, product_to_check )
	view_show_safe( "subplatform_message_view" )
	show_warning_header()
	warning_popup_timer:start()
	view_analytics_event( "context", "subplatformWarning" )
	kernel.setmacro( "Subplatform:Header", strings.subplatform_warning_title )
	kernel.setmacro( "Subplatform:Message", string.gsub( strings.subplatform_warning, "{0}", subplatform_message ) )

	kernel.registeractorhitnotify(
		"warning_continue_button",
		function()
			warning_popup_timer:stop()
			check_alert_table.sub_platform_warning_shown = true
			if purchase_product_in_cart == true then
				show_purchase_warning( check_alert_table, selected_movie, i, product_to_check )
			else
				add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action, product_to_check )
			end
		end
	)
	kernel.registeractorhitnotify(
		"warning_back_button",
		function()
			warning_popup_timer:stop()
			view_unwindto_safe( item_added_from_view, true, "subplatform_message_view:hide()" )
		end
	)
	kernel.registeractorhitnotify(
		"warning_backback_button_arrow",
		function()
			warning_popup_timer:stop()
			view_unwindto_safe( item_added_from_view, true, "subplatform_message_view:hide()" )
		end
	)
end

function show_purchase_warning( check_alert_table, selected_movie, i, product_to_check )
	view_show_safe( "purchase_message_view" )
	warning_popup_timer:start()
	show_warning_header()
	if StoreManager:get_purchase_new_price() ~= nil
	and StoreManager:has_sell_thru_new_in_stock() then
		kernel.setmacro( "PurchaseWarning:Header", strings.products_new )
		kernel.setmacro( "PurchaseWarning:Message", strings.purchase_new_warning )
	else
		kernel.setmacro( "PurchaseWarning:Header",strings.products_used )
		kernel.setmacro( "PurchaseWarning:Message",strings.purchase_warning )
	end
	view_analytics_event( "context", "purchaseWarning" )

	kernel.registeractorhitnotify(
		"warning_continue_button",
		function()
			warning_popup_timer:stop()
			add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action, product_to_check )
		end
	)
	kernel.registeractorhitnotify(
		"warning_back_button",
		function()
			warning_popup_timer:stop()
			kernel.setactoropacity( "purchase_scene_background" , 0.6 )
			view_unwindto_safe( item_added_from_view, true, "purchase_message_view:hide()" )
		end
	)
	kernel.registeractorhitnotify(
		"warning_backback_button_arrow",
		function()
			warning_popup_timer:stop()
			kernel.setactoropacity( "purchase_scene_background" , 0.6 )
			view_unwindto_safe( item_added_from_view, true, "purchase_message_view:hide()" )
		end
	)
	view_analytics_event( "context", "purchaseWarning" )
end

function add_item_to_shopping_cart( selected_movie, i, action, product_to_add )
	if ShoppingCart.pickup_recommendations_shown == true then
		-- Add directly to the cart then unwind  ('add_movie' has screen specific toggling)
		ShoppingCart:add_item( selected_movie, 1, action, "Recommendation" )
		view_unwindto_safe( item_added_from_view, true, "Item added" )
	elseif i == nil then
		ShoppingCart:add_item( selected_movie, 1, action )
		if added_from_carousel == false then
			view_unwindto_safe( product_detail_shown_from_view, true, "Item added from Product Detail view" )
		else
			added_from_carousel = false
			kernel.executescript( "browse_products" )
		end
	else
		add_movie( product_to_add )
		view_unwindto_safe( item_added_from_view, true, "Item added" )
	end
	browse_selection_cart_action = "rental"
end

function display_alpha_screen_layout()
	kernel.hideactor( "selected_label" )
	kernel.showactor( "alpha_back_ground_image" )
	kernel.hideactor( "bg_box_top" )
	for k,v in ipairs( alpha_table ) do
		kernel.setactorbackgroundcolor( v .. "_label", "black" )
	end
end

is_dnr_disc = function( product )
	local do_not_rent_flag = false
	if product ~= nil and product.do_not_rent_date ~= nil then
		do_not_rent_flag = product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
	end
	return do_not_rent_flag
end

function newtitlerollupoverlay()
	local t = {}
	
	t.scene_manager = newscenemanager()
	
	t.show = function( self, x, y, width, height, modal, dvd_product, dvd_button_on_hit_function, blu_ray_product, blu_ray_button_on_hit_function, cancel_button_on_hit_function, timeout_function, purchase )
		if x == nil or x < 0 or x > 1024
		or y == nil or y < 0 or y > 768
		or width == nil or width < 75 or width > 1024
		or height == nil or height < 35 or height > 768
		then
			return
		end
		
		local button_width = width / 1.25
		local button_height = height / 4
		local button_x = x + ( ( width - button_width ) / 2 )
		local button_y = y + ( ( height - ( button_height * 2 ) ) / 2 )
		
		local cancel_button_x = x + width - 21
		local cancel_button_y = y + height - 20
		
		local function get_button_info( product, type )
			local button_font = "font_bryant_bold_22"
			if button_width < 200 then
				button_font = "font_bryant_bold_13"
			end
			local button_text = strings.title_rollup_overlay_unavailable_text
			local button_background_color = { r = 90, g = 90, b = 90 }
			local button_gradient_target_color = { r = 30, g = 30, b = 30 }
			local button_hit_options = "None"
			local button_visible = false
			
			if product ~= nil and product.product_id ~= nil and ShoppingCart:has_item( product.product_id ) == true then
				button_text = string.gsub( strings.title_rollup_overlay_cart_text, "{0}", type )
				button_visible = true
			elseif product ~= nil and product.is_available == true and product.product_id ~= nil and ProductCatalog:in_stock( product.product_id ) == false then
				if button_width < 200 then
					button_font = "font_bryant_bold_12"
				end
				button_text = string.gsub( strings.title_rollup_overlay_oos_text, "{0}", type )
				button_visible = true
			elseif purchase == true and product ~= nil then
				local purchase_price = StoreManager:get_purchase_price()
				if purchase_price ~= nil and purchase_price > 0 then
					if kernel.getculture() == "es-MX" then
						if button_width < 200 then
							button_font = "font_bryant_bold_10"
						else
							button_font = "font_bryant_bold_18"
						end
					end
					button_text = string.gsub( strings.title_rollup_overlay_buy_text, "%S%S%S", { ["{0}"] = type, ["{1}"] = strings.currency_symbol, ["{2}"] = string.format( "%.2f", purchase_price ) } )
					button_hit_options = "Enabled"
					button_visible = true
					if type == "BLU-RAY" then
						button_background_color = { r = 29, g = 155, b = 229 }
						button_gradient_target_color = { r = 23, g = 127, b = 188 }
					else
						button_background_color = { r = 181, g = 18, b = 27 }
						button_gradient_target_color = { r = 143, g = 14, b = 21 }
					end
				end
			elseif product ~= nil and product.is_available == true and product.product_id ~= nil and ProductCatalog:in_stock( product.product_id ) == true then
				if button_width < 200 then
					button_font = "font_bryant_bold_15"
				end
				button_text = string.gsub( strings.title_rollup_overlay_add_text, "{0}", type )
				button_hit_options = "Enabled"
				button_visible = true
				if type == "BLU-RAY" then
					button_background_color = { r = 29, g = 155, b = 229 }
					button_gradient_target_color = { r = 23, g = 127, b = 188 }
				else
					button_background_color = { r = 181, g = 18, b = 27 }
					button_gradient_target_color = { r = 143, g = 14, b = 21 }
				end
			end
			
			return button_font, button_text, button_background_color, button_gradient_target_color, button_hit_options, button_visible
		end
		
		if modal == true then
			self.scene_manager:add_actor(
				"overlay_modal_background",
				0,
				0,
				1024,
				768,
				{
					backgroundColor = "black",
					opacity = 0.8,
					hitOptions = "Enabled",
					renderOptions = "Background"
				}
			)
			
			self.scene_manager:add_actor(
				"overlay_background",
				x,
				y,
				width,
				height,
				{
					backgroundColor = "white",
					borderColor = { r = 151, g = 14, b = 22 },
					strokeWeight = 4,
					cornerSize = { width = 5, height = 5 },
					renderOptions = "Background, DrawBorder"
				}
			)
		else
			self.scene_manager:add_actor(
				"overlay_background",
				x,
				y,
				width,
				height,
				{
					backgroundColor = "black",
					opacity = 0.8,
					borderColor = { r = 151, g = 14, b = 22 },
					strokeWeight = 4,
					cornerSize = { width = 5, height = 5 },
					hitOptions = "Enabled",
					renderOptions = "Background, DrawBorder"
				}
			)	
		end
		
		local dvd_button_font, dvd_button_text, dvd_button_background_color, dvd_button_gradient_target_color, dvd_button_hit_options, dvd_button_visible = get_button_info( dvd_product, "DVD" )
		self.scene_manager:add_actor(
			"overlay_dvd_button",
			button_x,
			button_y - 15,
			button_width,
			button_height,
			{
				backgroundColor = dvd_button_background_color,
				gradientTargetColor = dvd_button_gradient_target_color,
				gradientAngle = 90,
				cornerSize = { width = 90, height = 90 },
				foregroundColor = "white",
				text = dvd_button_text,
				font = dvd_button_font,
				horizontalAlignment = "center",
				verticalAlignment = "center",
				hitOptions = dvd_button_hit_options,
				visible = dvd_button_visible,
				renderOptions = "BackgroundGradient, Text"
			},
			function()
				self:hide()
				if dvd_button_on_hit_function ~= nil then
					dvd_button_on_hit_function()
				end
			end
		)
		
		local blu_ray_button_font, blu_ray_button_text, blu_ray_button_background_color, blu_ray_button_gradient_target_color, blu_ray_button_hit_options, blu_ray_button_visible = get_button_info( blu_ray_product, "BLU-RAY" )
		self.scene_manager:add_actor(
			"overlay_blu_ray_button",
			button_x,
			button_y + button_height + 15,
			button_width,
			button_height,
			{
				backgroundColor = blu_ray_button_background_color,
				gradientTargetColor = blu_ray_button_gradient_target_color,
				gradientAngle = 90,
				cornerSize = { width = 90, height = 90 },
				foregroundColor = "white",
				text = blu_ray_button_text,
				font = blu_ray_button_font,
				horizontalAlignment = "center",
				verticalAlignment = "center",
				hitOptions = blu_ray_button_hit_options,
				visible = blu_ray_button_visible,
				renderOptions = "BackgroundGradient, Text"
			},
			function()
				self:hide()
				if blu_ray_button_on_hit_function ~= nil then
					blu_ray_button_on_hit_function()
				end
			end
		)
		
		self.scene_manager:add_actor(
			"overlay_cancel_button",
			cancel_button_x,
			cancel_button_y,
			43,
			43,
			{
				backgroundColor = { r = 80, g = 80, b = 80 },
				gradientTargetColor = { r = 0, g = 0, b = 0 },
				gradientAngle = 90,
				borderColor = { r = 151, g = 14, b = 22 },
				strokeWeight = 2,
				cornerSize = { width = 21, height = 21 },
				foregroundColor = "white",
				text = "X",
				font = "font_akzidenz_bold_small",
				horizontalAlignment = "center",
				verticalAlignment = "center",
				hitOptions = "Enabled",
				renderOptions = "BackgroundGradient, DrawBorder, Text"
			},
			function()
				self:hide()
				if cancel_button_on_hit_function ~= nil then
					cancel_button_on_hit_function()
				end
			end
		)
		
		kernel.timercreate(
			"overlay_scene_manager_timer",
			10000,
			10000,
			function()
				self:hide()
				if timeout_function ~= nil then
					timeout_function()
				end
			end
		)
		kernel.timerstart( "overlay_scene_manager_timer" )
	end
	
	t.hide = function( self )
		kernel.timerstop( "overlay_scene_manager_timer" )
		kernel.timerremove( "overlay_scene_manager_timer" )
		self.scene_manager:clear()
	end
	
	return t
end

function get_product_ui_info ( product )
	local movie_price_purchase = StoreManager:get_purchase_price()
	local sell_through_enabled = StoreManager:is_sell_thru_enabled()
	local current_date_string = get_current_date_string( "%4d%02d%02d%02d%02d" )
	local product_type = nil
	local product_family = nil
	local product_type_md = nil
	if (product ~= nil) then
		product_type = ProductTypes[product.product_type_id]
		product_family = ProductFamilies[product_type.product_family_id]
		product_type_md = ProductTypesMetaData[product_type.product_type_id]
	end

	local product_info_result = {}
    product_info_result["product"] = product
    product_info_result["product_is_case"] = product ~= nil and product.product_id == 1139
    product_info_result["available"] = product ~= nil and product.is_available
    product_info_result["available_and_instock"] = product_info_result["available"] and ProductCatalog:in_stock( product.product_id )
    product_info_result["already_in_cart"] = product ~= nil and ShoppingCart:count_for_item( product.product_id ) > 0

    -- purchase related properties
    local is_purchasable_value = false
    local show_buy_button_value = false
    local show_in_purchase_overlay_value = false
    if product ~= nil then
	    if product_info_result["product_is_case"] then
			is_purchasable_value = product_info_result["available_and_instock"]
	    elseif product_family.product_family_name == "Movies" then
		    is_purchasable_value = sell_through_enabled and movie_price_purchase ~= nil
					and movie_price_purchase > 0 and product ~= nil and product.sell_thru == true and product.can_be_sold
					and is_multi_disc( product.product_id ) == false 
		    show_buy_button_value = is_purchasable_value and product_info_result["available_and_instock"] and not product_info_result["already_in_cart"]
		    show_in_purchase_overlay_value = product_info_result["available"] and is_purchasable_value
		elseif product_family.product_family_name == "Games" then
			local game_price_purchase = nil
			if product ~= nil and product.sellthru_price ~= nil then
				game_price_purchase = StoreManager.PricingManager:get_adjusted_price( product.sellthru_price, "purchase" )
			end

			is_purchasable_value = product_info_result["available_and_instock"] and GamesSellThru == true and game_price_purchase ~= nil
					and game_price_purchase > 0	and product.sell_thru == true
					and product.sellthru_date ~= nil
					and product.sellthru_date <= current_date_string
					and product.can_be_sold
					and EnableSellThruOfUsed == true
					and is_multi_disc( product.product_id ) == false
		end
	end

	product_info_result["is_purchasable"] = is_purchasable_value
	product_info_result["show_buy_button"] = show_buy_button_value
	product_info_result["show_in_purchase_overlay"] = show_in_purchase_overlay_value

	return product_info_result
end

show_collection_items = function(collection)
	browse_collection = collection
	if (collection ~= nil) then
		browse_collection_products = collection.incart_available_kiosk_products
	end
	collection_set_number = 1
	view_show( "browse_view_collection" )
end

remove_product_on_hit = function(product_to_remove)
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	view_analytics_event( "buttonPress", "shopping-cart-remove-item" )
	remove_movie(product_to_remove)
end

should_show_no_thanks_browse_button = function()
return  ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_title_marketing == true
	and CartHasAnyPostOfferItems( ShoppingCart ) == false
end

on_checkout_button_click = function ()
	kernel.executescript("view_shopping_cart")
end

draw_action_bar = function() 
	kernel.draw_action_bar(
		#ShoppingCart.items,
		on_checkout_button_click,
		should_show_no_thanks_browse_button())
end

draw_action_bar_and_mini_cart = function (browseSetName)
	draw_action_bar()
	kernel.draw_mini_cart(
		browseSetName,
		ShoppingCart.items,
		ShoppingCart.in_cart_title_marketing,
		remove_product_on_hit,
		browseSetName ~= "Collections")
end

draw_ada_control = function ()
	kernel.draw_accessibility_control(
		"ada",
		alpha_filtered_products,
		add_movie_on_hit,
		switch_product_family_tab_function,
		browse_filter_family_tracker.selected_family
		)
end

draw_top_menu = function()
	kernel.draw_top_menu(
		kernel.peekview(),
		browse_filter_genre_selected,
		browse_filter_family_tracker.selected_family,
		DisableGames,
		kernel.rental_is_collections_enabled(), 
		change_genre_filter,
		change_product_family,
		click_collections_menu)
end

change_genre_filter = function(new_genre_filter_value, new_genre_filter_text)
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	browse_filter_genre_selected = new_genre_filter_value
	view_analytics_event( "filterChange", "browse-filter-genre," .. new_genre_filter_text )
	set_number = 1
	if (kernel.peekview() == "browse_view_collections") then
		view_goback_safe()
		if (kernel.peekview() ~= "browse_view" and kernel.peekview() ~= "browse_filter_alpha_view")  then
			view_show_safe("browse_view")
		end
	else
		show_browse_view(10)
		draw_top_menu()
		check_product_availability()
	end
	if kernel.peekview() == "browse_filter_alpha_view" then
		highlight_alphabets()
	end
end

click_collections_menu = function ()
	view_analytics_event( "filterChange", "browse-filter-genre,COLLECTIONS" )
	if (kernel.peekview() ~= "browse_view" and kernel.peekview() ~= "browse_filter_alpha_view") then
		view_goback_safe()
	end
	show_browse_view_collections()
end

show_browse_view_collections = function()
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	collections_set_number = 1
	view_show("browse_view_collections")
end

function switch_product_family_tab_function()
	if browse_filter_family_tracker.selected_family == "Movies" then
		change_product_family("Games")
	else
		change_product_family("Movies")
	end
end

change_product_family = function (new_product_family) 
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end
	browse_filter_family_tracker.selected_family = new_product_family
	if browse_filter_family_tracker.selected_family == "Games" then
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_games )
		browse_filter_genre_selected = nil
        if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Blu-ray" or  kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
			browse_filter_family_tracker:set_family_type( "ALL" )
            kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
            kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
        elseif kernel.get_session_value("BrowseState.BrowseFilterMode") =="ComingSoon" then
			browse_filter_family_tracker:set_family_type( "ALL" )
            kernel.set_session_value("BrowseState.BrowseSelectedSort", "coming-soon")
            kernel.set_session_value("BrowseState.BrowseFilterMode", "ComingSoon")
        elseif kernel.get_session_value("BrowseState.BrowseFilterMode") =="ForSale" then
			if ProductCatalog:is_product_available( "for-sale" )
			and EnableSellThruOfUsed == true
			and GamesSellThru == true then
				browse_filter_family_tracker:set_family_type( "for-sale" )
                kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
                kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")
			else
				browse_filter_family_tracker:set_family_type( "ALL" )
                kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
			end
		end
	else
		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
		browse_filter_genre_selected = ALL
        if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Xbox 360" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Xbox One" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS2" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS3" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS4" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Wii" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "NintendoDS" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Wii U" then
			browse_filter_family_tracker:set_family_type( "ALL" )
            kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
		end
        if kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" then
			if ProductCatalog:is_product_available( "for-sale" ) and EnableSellThruOfUsed == true then
				browse_filter_family_tracker:set_family_type( "for-sale" )
                kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
                kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")
			else
				browse_filter_family_tracker:set_family_type( "ALL" )
                kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
			end
		end
	end
	view_analytics_event( "filterChange", "browse-filter," .. browse_filter_family_tracker.selected_family )
    if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Newest" then
		browse_filter_family_tracker:set_family_type( "ALL" )
	end

	set_number = 1
	while (kernel.peekview() == "browse_view_collections" or kernel.peekview() == "browse_view_collection" or kernel.peekview() == "browse_filter_alpha_view") do
		view_goback_safe()
	end
	if (kernel.peekview() == "browse_view_ada_accessible") then
		show_browse_view(10)
	elseif (kernel.peekview() == "browse_view") then
		show_browse_view(10)
		draw_top_menu()
		check_product_availability()
	else
		view_show("browse_view")
	end
end

function check_collections_popup(reason) 
	if is_collections_popup_enabled() then
		collection_message_reason = reason
		view_show("collection_message_view")
	end
end

function check_coming_soon_collections_popup()
	if marketing_config ~= nil and marketing_config.show_collections_popup_on_coming_soon == true then
		check_collections_popup("coming soon")
	end
end

