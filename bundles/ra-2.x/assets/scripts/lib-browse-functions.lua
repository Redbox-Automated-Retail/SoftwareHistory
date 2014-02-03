--
-- lib-browse-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

browse_filtered_products_table = {}
movie_product_map = {}
local x=80
local y=80
local max_set_number
local total_images_count
local alpha_opacity = 0.2
local browse_opacity = 0.4
local rated_movie_in_cart = false
local blu_ray_movie_in_cart = false
local rated_game_in_cart = false
local subplatform_game_in_cart = false
local purchase_product_in_cart = false
local subplatform_message = ""
local customer_selected_product_id

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

function remove_movie_from_selected_product_map( actor_name )
	local movie_to_remove_index = string.sub( actor_name, string.find( actor_name, "%d" ), #actor_name )
	local i = movie_to_remove_index + 0
	while ( i <= #selected_movies_list ) do
		movie_product_map[ "selectedmovie_" .. i ] = movie_product_map[ "selectedmovie_" .. i + 1 ]
		i = i + 1
	end
	movie_product_map[ "selectedmovie_" .. i-1 ] = nil
	movie_product_map[ "selectedmovie_" .. i ] = nil
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

function set_actors_opacity( value )
	kernel.setactoropacity( "movie_11", value )
	kernel.setactoropacity( "movie_12", value )
	kernel.setactoropacity( "movie_13", value )
	kernel.setactoropacity( "movie_14", value )
end

function show_browse_view( total_count )
	highlight_sort_button( browse_selected_sort )
	total_images_count = total_count
	ProductCatalog:show_filtered_grid()
end

highlight_tab = function( tab_number )
	for i = 1, 7 do
		if tab_number == i then
			kernel.setactorbackgroundcolor("tab_" .. tostring( i ), {["r"] = 143, ["g"] = 10, ["b"] = 10})
			kernel.setactorgradienttargetcolor("tab_" .. tostring( i ), {["r"] = 170, ["g"] = 37, ["b"] = 37})
		else
			kernel.setactorbackgroundcolor("tab_" .. tostring( i ), {["r"] = 88, ["g"] = 85, ["b"] = 85})
			kernel.setactorgradienttargetcolor("tab_" .. tostring( i ), {["r"] = 51, ["g"] = 48, ["b"] = 48})
		end
	end
end

load_shopping_cart = function()
	if #ShoppingCart.items > 0 then
		selected_products_list = {}
		local count = total_images_count
		if count == 18 then
			count = count + 2
		end

		for k, v in ipairs( ShoppingCart.items ) do
			local product = MasterProductCatalog[ v.product_id ]
			if product ~= nil then
				table.insert( selected_products_list, product )
				movie_product_map[ "selectedmovie_" .. k ] = product
				draw_image( product, "selectedmovie_" .. k )
				kernel.showactor( "selectedmovie_" .. k .. "_border" )
				--Display purchase flag on mini-cart
				if v.action == "purchase" then
					kernel.showactor( "selectedmovie_" .. k .. "_flag" )
				else
					kernel.hideactor( "selectedmovie_" .. k .. "_flag" )
				end

				-- Pre-offer titles cannot be removed from the Title Marketing screen
				if ShoppingCart.in_cart_title_marketing == false
				or v.add_sequence == "post_offer"
				then
					kernel.showactor( "cancel_" .. k )
				end

				for i = 1, count do
					local browse_item = movie_product_map[ "movie_".. tostring( i ) ]
					if browse_item ~= nil and browse_item.product_id == product.product_id then
						local product_history = ProductHistory[ product.product_id ]

						if product.product_id ~= 1139
						or ( product.product_id == 1139 and ProductCatalog:in_stock( product.product_id, product_history ) == false )
						then
							if browse_selected_sort == "alpha" then
								kernel.setactoropacity("movie_" .. tostring( i ), alpha_opacity )
							else
								kernel.setactoropacity("movie_" .. tostring( i ), browse_opacity )
							end
							kernel.disableactor( "flag_" .. tostring( i ) )
							kernel.disableactor( "banner_" .. tostring( i ) )
							kernel.hideactor( "add_movie_" .. tostring( i ) )
							kernel.disableactor( "add_movie_lower_" .. tostring( i ) )
						end
						break
					end
				end
			end
		end

		if #selected_products_list  == 5 then
			for i = 1, count do
				if browse_selected_sort == "alpha" then
					kernel.setactoropacity( "movie_" .. tostring( i ), alpha_opacity )
				else
					if  browse_selected_sort ~= "coming-soon" then
						kernel.setactoropacity( "movie_" .. tostring( i ), browse_opacity )
					end
				end
				kernel.disableactor( "flag_" .. tostring( i ) )
				kernel.disableactor( "banner_" .. tostring( i ) )
				kernel.hideactor( "add_movie_" .. tostring( i ) )
				kernel.disableactor( "add_movie_lower_" .. tostring( i ) )
			end
		end

		if kernel.getculture() == "en-US" then
			kernel.setactortext( "my_rentals_button", "CHECK OUT (" .. #ShoppingCart.items .. ")" )
		elseif kernel.getculture() == "es-MX" then
			kernel.setactortext( "my_rentals_button", "CAJA (" .. #ShoppingCart.items .. ")" )
		end

		kernel.hideactor( "add_to_cart_label" )
		kernel.hideactor( "add_to_cart_label_image" )
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

ProductCatalog.filter_products_title_marketing = function( self )
	--cache globals to locals for improved performance
	local _ProductTypes = ProductTypes
	local _ProductFamilies = ProductFamilies
	local _History = ProductHistory
	local _MasterProductCatalog = MasterProductCatalog

	local products = {}
	filtered_product_all_family = {}

	local AddValidProductToResult = function( product_id )
		local product_type = nil
		local product = _MasterProductCatalog[ product_id ]
		if product == nil then
			return
		end

		local history = _History[ product_id ]
		local do_not_rent_flag = false
		product_type = _ProductTypes[product.product_type_id]
		if product.do_not_rent_date ~= nil then
			do_not_rent_flag = product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
		end
		local cache = self[ product.product_id ]
		if cache ~= nil then
			product.can_be_sold = false -- Customers should not be making purchases from the Title Marketing view
			product.is_available = cache.is_available == nil and false or cache.is_available
		end
		if self:in_stock( product.product_id, history )
		and product.is_available == true
		and do_not_rent_flag == false
		and ( product_type.product_family_id ~= 2 or game_allowed_to_be_shown( product_type ) == true )
		then
			table.insert( products, product )
			table.insert( filtered_product_all_family, product )
		end
	end

	local _in_cart = GetInCartPromoAction()
	if _in_cart ~= nil then
		if _in_cart.product_id_list ~= nil then
			local title_list = string.split( _in_cart.product_id_list, "," )
			for _, v in ipairs( title_list ) do
				AddValidProductToResult( v )
			end
		elseif _in_cart.exclude_id_list ~= nil
		and #_in_cart.exclude_id_list > 0 then
			for k, v in self:pairs() do
				if inlist( _in_cart.exclude_id_list, v.product_id, "," ) == false then
					AddValidProductToResult( v.product_id )
				end
			end
		end
	end

	return products

end

ProductCatalog.filter_products = function( self )
	--cache globals to locals for improved performance
	local _ProductTypes = ProductTypes
	local _ShoppingCart = ShoppingCart
	local _ProductFamilies = ProductFamilies
	local _History = ProductHistory
	local _MasterProductCatalog = MasterProductCatalog
	local products = {}
	filtered_product_all_family = {}
	-- Check in-cart include title and exclude title lists no longer limited to use in Title Marketing browse mode
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


	for k, v in self:pairs() do
		local product_type = nil
		local product = _MasterProductCatalog[ v.product_id ]
		local history = _History[ v.product_id ]
		local do_not_rent_flag = false
		if product ~= nil then
			product_type = _ProductTypes[product.product_type_id]
		end
		if product ~= nil and product.do_not_rent_date ~= nil then
			do_not_rent_flag = product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
		end
		if  product ~= nil
		and product_type ~= nil
		and ( product_type.product_family_id ~= 2 or game_allowed_to_be_shown( product_type ) == true )
		and do_not_rent_flag == false
		and history ~= nil then
			local product_family = _ProductFamilies[product_type.product_family_id]

			--add in cache state variables...
			local cache = self[ product.product_id ]
			if cache ~= nil then
				product.can_be_sold = cache.can_be_sold == nil and false or cache.can_be_sold
				product.is_available = cache.is_available == nil and false or cache.is_available
				product.is_coming_soon = cache.is_coming_soon or false
			end

			if   browse_filter_family_tracker:is_match( product_family.product_family_name, product_type.product_type_name )
			and (browse_filter_genre_selected == nil or browse_filter_genre_selected == "ALL" or product.genres[browse_filter_genre_selected] == true)
			and (browse_filter_alpha_selected == nil  or browse_filter_alpha_selected==string.sub( product.sort_name, 1, 1) or browse_filter_alpha_selected==string.upper(string.sub( product.sort_name, 1, 1)) or (browse_filter_alpha_selected == "0-9" and ( AlphaRanges[8].ranges[tonumber(string.sub( product.sort_name, 1, 1 ))])))
			and (browse_rent_or_sellthru == "rent" or (browse_rent_or_sellthru == "sellthru" and product.can_be_sold) )
			and product.sell_thru_new ~= true
			and browse_filter_mode ~= "for-sale"
			then
				if browse_filter_mode == "coming-soon" and ( product_type.product_family_id == 1 or product_type.product_family_id == 2 ) then
						if product.is_coming_soon == true and v.product_id ~= 1139 then
							table.insert(products, product)
						end
				else
					if (self:in_stock( v.product_id, history)
					or self:out_of_stock_within_expiration_date( v.product_id, history ) == true
					or is_product_id_selected(v.product_id) == true)
					and product.is_available == true
					and ( _incart_include_titles == nil or _incart_include_titles[v.product_id] ~= nil )
					and ( _incart_exclude_titles == nil or _incart_exclude_titles[v.product_id] == nil )
					then
						table.insert(products, product)
					end
				end
			end

			if   (browse_filter_genre_selected == nil or browse_filter_genre_selected == "ALL" or product.genres[browse_filter_genre_selected] == true)
			and  (browse_filter_alpha_selected == nil or browse_filter_alpha_selected==string.sub( product.sort_name, 1, 1 ))
			and product.sell_thru_new ~= true
			then
				if browse_filter_mode == "coming-soon" and ( product_type.product_family_id == 1 or product_type.product_family_id == 2 ) then
						if product.is_coming_soon == true and v.product_id ~= 1139 then
							table.insert(filtered_product_all_family, product)
						end
				else
					if (self:in_stock( v.product_id, history)
					or self:out_of_stock_within_expiration_date( v.product_id, history ) == true
					or is_product_id_selected(v.product_id) == true)
					and product.is_available == true
					and ( _incart_include_titles == nil or _incart_include_titles[v.product_id] ~= nil )
					and ( _incart_exclude_titles == nil or _incart_exclude_titles[v.product_id] == nil )
					then
						if browse_filter_mode == "for-sale" then
							if  browse_filter_family_tracker.selected_family == "Movies" and product_family.product_family_name == "Movies"
							then
								local price_purchase = StoreManager:get_purchase_price()
								if price_purchase ~= nil
								and price_purchase > 0
								and product.sell_thru == true
								and product.can_be_sold then
									table.insert( products, product )
								end
							elseif browse_filter_family_tracker.selected_family == "Games" and product_family.product_family_name == "Games"
							then
								local price_purchase = StoreManager.PricingManager:get_adjusted_price( product.sellthru_price )
								if price_purchase ~= nil
								and price_purchase > 0
								and product.sell_thru == true
								and product.sellthru_date ~= nil
								and product.sellthru_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
								and product.can_be_sold  then
									table.insert( products, product )
								end
							end
						else
							table.insert(filtered_product_all_family, product)
						end
					end
				end
			end
		else
			--kernel.log( "WARNING! ProductCatalog.filter_products couldn't find: v.product_id = " .. tostring( v.product_id ) )
		end
	end

	table.sort(
		products,
		function( a, b )
			if browse_selected_sort == "alpha" then
				return tostring( string.lower( a.sort_name ) ) < tostring( string.lower( b.sort_name ) )
			else
				if browse_selected_sort == "coming-soon" then
					if tostring( a.release_date ) ~= tostring( b.release_date ) then
						-- Coming soonest before those coming later
						return tostring( a.release_date ) < tostring( b.release_date )
					end
				else
					if tostring( a.sort_date ) ~= tostring( b.sort_date ) then
						-- Newest to Oldest
						return tostring( a.sort_date ) > tostring( b.sort_date )
					end
				end
				if tonumber( a.box_office_gross ) ~= tonumber( b.box_office_gross ) then
					return tonumber( a.box_office_gross ) > tonumber( b.box_office_gross )
				else
					return tostring( string.lower( a.sort_name ) ) < tostring( string.lower( b.sort_name ) )
				end
			end
		end
	)

	if #products == 0 then
		-- No products available for the sort, display an appropriate message
		if browse_filter_family_tracker.selected_family == "Games" then 
			kernel.setactortext( "filter_empty_text", strings.filter_games_empty )
		else
			kernel.setactortext( "filter_empty_text", strings.filter_movies_empty )
		end

		if DisableGames
		or ProductCatalog:in_stock_games_count() == 0 then
			kernel.setactorimage( "image_filter_empty_background", "image_filter_empty_background_no_notch" )
		else
			kernel.setactorimage( "image_filter_empty_background", "image_filter_empty_background" )
		end
		kernel.showactor( "image_filter_empty_background" )
		kernel.showactor( "filter_empty_text" )
	else
		-- ensure the filter empty assets are hidden
		kernel.hideactor( "image_filter_empty_background" )
		kernel.hideactor( "filter_empty_text" )
	end
	
	return products
end

-- adjust the product list to bring the dvd editions appear before the blu-ray editions of a title.
ProductCatalog.filter_grid = function( self )

	if ShoppingCart.in_cart_title_marketing == true then
		browse_filtered_products = self:filter_products_title_marketing()
	else
		browse_filtered_products = self:filter_products()
	end

	for i, v in ipairs( browse_filtered_products ) do
		if v.product_type_id == 2 then
			for j, k in ipairs( browse_filtered_products ) do
			if k.long_name ~= nil then
				local current_long_name = string.sub( v.long_name, 0, string.len( v.long_name ) - 10 )
					if k.long_name == current_long_name then
						if j > i then
							table.insert( browse_filtered_products, j + 1, v )
							table.remove( browse_filtered_products, i )
						elseif j < i then
							table.insert( browse_filtered_products, j + 1, v )
							table.remove( browse_filtered_products, i + 1 )
						end
						break
					end
				end
			end
		end
	end

	if browse_selected_sort ~= "alpha" then
		local In_stock_products = {}
		local Out_of_stock_products = {}
		for i, v in ipairs( browse_filtered_products ) do
			local product_history = ProductHistory[ v.product_id ]
			if ProductCatalog:in_stock( v.product_id, product_history ) == false
			and is_product_selected( v )==false
			and v.is_available == true
			then
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
	kernel.setactory( "left_page_shadow", 0 )
	kernel.setactorheight( "left_page_shadow", 570 )
	kernel.setactory( "right_page_shadow", 0 )
	kernel.setactorheight( "right_page_shadow", 570 )
	kernel.setactortext( "title_marketing_h1", kernel.getmacro( "InCartPromoHeaderMessage" ) )
	kernel.showactor( "title_marketing_h1" )

	-- Hide all the tabs at the top
	kernel.hideactor( "tab_1" )
	kernel.hideactor( "tab_2" )
	kernel.hideactor( "tab_2_text_a" )
	kernel.hideactor( "tab_2_text_b" )
	kernel.hideactor( "tab_3" )
	kernel.hideactor( "tab_4" )
	kernel.hideactor( "tab_5" )
	kernel.hideactor( "tab_6" )
	kernel.hideactor( "tab_7" )
	kernel.hideactor( "tab_switch_product_family_border" )
	kernel.hideactor( "tab_switch_product_family" )
	kernel.hideactor( "image_next_white_arrow" )

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

draw_pagination_dots = function()
	if set_number > 0 and max_set_number > 0 then
		kernel.showactor( "browse_pagination_text" )
		kernel.showactor( "browse_pagination_L_image" )
		kernel.showactor( "browse_pagination_of_text" )
		kernel.showactor( "browse_pagination_R_image" )
		kernel.setactortext( "browse_pagination_L_image", set_number )
		kernel.setactortext( "browse_pagination_R_image", max_set_number )
	end
end

remove_pagination_dots = function()
	kernel.hideactor( "browse_pagination_text" )
	kernel.hideactor( "browse_pagination_L_image" )
	kernel.hideactor( "browse_pagination_of_text" )
	kernel.hideactor( "browse_pagination_R_image" )
end

function refresh_barriers()
	for i=1,20,1 do
		kernel.removeactor("barrier_"..i)
		kernel.removeactor("barrier_label_"..i)
		kernel.removeactor("movie_"..i)
		kernel.removeactor("movie_"..i .."_title")
		kernel.removeactor("add_movie_"..i)
	end

	for i=1,21,1 do
		kernel.removeactor("separator_"..i)
	end
	x=80
	y=80
end

function draw_barriers()
local x1=70
local y1=60
	for i=1,21,1 do
		if ( i == 8 ) then
			x1=370
			y1=60
		end
		if ( i == 15 ) then
			x1=670
			y1=60
		end

		aplha_scene_manager:add_actor(
			"separator_" ..i,
			x1,
			y1+70,
			280,
			1,
			{
				renderOptions = "Background",
				backgroundColor= {["r"] = 163, ["g"] = 163, ["b"] = 163}
			}
		)
		y1=y1+70
	end
end

move_left = function( total_count )
	set_number = set_number - 1
	total_images_count = total_count
	if browse_selected_sort=="alpha" then
		draw_images_set( browse_filtered_products_table )
	else
		draw_images_set( alpha_filtered_products )
	end
	draw_pagination_dots()
end

move_right = function( total_count )
	set_number = set_number + 1
	total_images_count = total_count
	if browse_selected_sort=="alpha" then
		draw_images_set( browse_filtered_products_table )
	else
		draw_images_set( alpha_filtered_products )
	end
	draw_pagination_dots()
end

draw_image = function( product_to_draw, control_to_draw )
	if browse_filter_family_tracker.selected_family == "Movies" then
		if kernel.getculture() == "es-MX" then
			source_image_name = "image_movie_missing_box_art_es_MX"
		else
			source_image_name = "image_movie_missing_box_art"
		end
	elseif browse_filter_family_tracker.selected_family == "Games" then
		if kernel.getculture() == "es-MX" then
			source_image_name = "image_game_missing_box_art_es_MX"
		else
			source_image_name = "image_game_missing_box_art"
		end
	end

	if product_to_draw ~= nil then
		local image_registered = kernel.registerimagefromcache(	product_to_draw.image_file,	product_to_draw.image_file )
		if image_registered then
			source_image_name = string.format( "cache:%s", product_to_draw.image_file )
		else
			if ProductTypes[product_to_draw.product_type_id].product_family_id == 1 then
				if kernel.getculture() == "es-MX" then
					source_image_name = "image_movie_missing_box_art_es_MX"
				else
					source_image_name = "image_movie_missing_box_art"
				end
			elseif ProductTypes[product_to_draw.product_type_id].product_family_id == 2 then
				if kernel.getculture() == "es-MX" then
					source_image_name = "image_game_missing_box_art_es_MX"
				else
					source_image_name = "image_game_missing_box_art"
				end
			end
		end
		kernel.showactor( control_to_draw )
		kernel.setactorimage( control_to_draw, source_image_name )
		kernel.setactortext( control_to_draw, source_image_name )
	end
end

find_empty_index = function()
	local index = 1
	while index < 6 do
		if movie_product_map[ "selectedmovie_" .. tostring(index) ] ~= nil then
			index = index + 1
		else
			return index
		end
	end
	return 0
end

is_product_selected = function( product )
	if product == nil then
		return false
	end
	local is_selected = false
	for k, v in ipairs( ShoppingCart.items ) do
		if v.product_id == product.product_id then
			is_selected = true
			break
		end
	end
	return is_selected
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

	highlight_sort_button( browse_selected_sort )
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

		local function enable_games()
			kernel.setactortext( "tab_switch_product_family", strings.To_Games )
		end

		local function disable_games()
			kernel.showactor( "tab_no_switch_product_family" )
			kernel.hideactor( "tab_switch_product_family" )
			kernel.hideactor( "tab_switch_product_family_border" )
			kernel.hideactor( "image_next_white_arrow" )
		end

		if not DisableGames then
			kernel.log( "Games are enabled." )
			if ProductCatalog.in_stock_games_count() == 0 then
				kernel.log( "Not Enough games in kiosk. Disabling Games tab." )
				disable_games()
			else
				enable_games()
			end
		else
			kernel.log( "Games are disabled. Disabling Games tab." )
			disable_games()
		end

		-- TOP 20 had top be split so we can increase the size of the 20
		-- The original tab_2 text has to be cleared as it is used by the Games Tabs
		kernel.setactortext( "tab_2", "" )
		kernel.showactor( "tab_2_text_a" )
		kernel.showactor( "tab_2_text_b" )
		kernel.setactortext( "tab_2_text_a", strings.genre_1093a )
		kernel.setactortext( "tab_2_text_b", strings.genre_1093b )
		
		kernel.setactortext( "tab_1", strings.Movies_ALL )
		kernel.setactortext( "tab_3", strings.genre_1000 )
		kernel.setactortext( "tab_4", strings.genre_1005 )
		kernel.setactortext( "tab_5", strings.genre_1009 )
		kernel.setactortext( "tab_6", strings.genre_1004 )
		kernel.setactortext( "tab_7", strings.genre_1006 )

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

		if browse_selected_sort == "alpha" then
			kernel.setactorbackgroundcolor( "bg_box_top", "black" )
		else
			kernel.setactorbackgroundcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorbackgroundcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactory( "bg_box_bottom", 318 )
			kernel.hideactor( "left_page_shadow" )
			kernel.hideactor( "right_page_shadow" )
			kernel.showactor( "paddle_shadow" )
			kernel.setactory( "center_gray_background", 563 )
			kernel.setactorforegroundcolor( "browse_pagination_L_image", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
			kernel.setactorforegroundcolor( "browse_pagination_of_text", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
			kernel.setactorforegroundcolor( "browse_pagination_R_image", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
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
		
		kernel.setactortext("tab_switch_product_family", strings.To_Movies)
		kernel.setactortext("tab_1", strings.Games_ALL)
		kernel.hideactor( "tab_2_text_a" )
		kernel.hideactor( "tab_2_text_b" )
		kernel.setactortext("tab_2", strings.genre_1026)
		kernel.setactortext("tab_3", strings.genre_1006)
		kernel.setactortext("tab_4", strings.genre_1028)
		kernel.setactortext("tab_5", strings.genre_1030)
		kernel.setactortext("tab_6", strings.genre_1034)
		kernel.setactortext("tab_7", strings.genre_1036)

		--if browse_selected_sort == "alpha" then
		--	kernel.setactorbackgroundcolor( "bg_box_top", "black" )
		--else
			kernel.setactorbackgroundcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorbackgroundcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_top", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactorgradienttargetcolor( "bg_box_bottom", {["r"] = 229, ["g"] = 229, ["b"] = 229} )
			kernel.setactory( "bg_box_bottom", 318 )
			kernel.hideactor( "left_page_shadow" )
			kernel.hideactor( "right_page_shadow" )
			kernel.showactor( "paddle_shadow" )
			kernel.setactory( "center_gray_background", 563 )
			kernel.setactorforegroundcolor( "browse_pagination_L_image", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
			kernel.setactorforegroundcolor( "browse_pagination_of_text", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
			kernel.setactorforegroundcolor( "browse_pagination_R_image", {["r"] = 60, ["g"] = 57, ["b"] = 57} )
		--end
	end
	highlight_sort_button( browse_selected_sort )
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

function is_product_thinned( product_id )
	product_barcodes = ProductToBarcode[ product_id ]
	if product_barcodes ~= nil then
		for k, v in pairs( product_barcodes ) do
			if v.status == "Thinned" then
				return true
			end
		end
	end
	return false
end

local function display_flag( product, index )
	if product == nil then
		return
	end

	if ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_title_marketing == true
	then
		-- Mingle #3981 do not show flags in special title marketing mode
		return
	end

	local releaseDate = string.sub( product.release_date, 1, 8 )
	local calculatedSortDate = string.sub( product.sort_date, 1, 8 )
	local days = kernel.getelapseddays( releaseDate, get_current_date_string( "%4d%02d%02d" ) )
	local daysByCalculatedSortDate = kernel.getelapseddays( calculatedSortDate, get_current_date_string( "%4d%02d%02d" ) )
	local product_history = ProductHistory[ product.product_id ]
	local is_cart_empty = find_empty_index()

	local show_flag_data = function( index, image, text, show_add_button )
		kernel.showactor( "flag_" .. tostring( index ) )
		kernel.setactorimage( "flag_" .. tostring( index ), image )
		kernel.setactortext( "flag_" .. tostring( index ), text )
		if show_add_button == true then
			kernel.showactor("add_movie_" .. tostring(index))
			kernel.enableactor("add_movie_lower_" .. tostring(index))
		else
			kernel.hideactor( "add_movie_" .. tostring(index ) )
			kernel.disableactor( "add_movie_lower_" .. tostring(index ) )
		end
	end

	-- Coming Soon
	if product.is_available == false then
		show_flag_data( index, "image_flag_blue_small", strings.flag_coming_soon, false )
		return
	end

	-- Out of Stock
	if ProductCatalog:in_stock( product.product_id, product_history ) == false
	and is_product_selected( product ) == false
	and product.is_available == true
	then
		show_flag_data( index, "image_flag_gray_small", strings.flag_out_of_stock, false )
		return
	end

	-- Award Winner
	if product.genres[1002] ~= nil
	and not is_product_selected( product ) then
		show_flag_data( index, "image_flag_gold_small", strings.flag_award_winner, true  )
		return
	end

	-- Early Release Titles
	if StoreManager.PricingManager:is_early_released( product )
	and StoreManager.PricingManager:is_alternate_priced( product.product_id )
	and not is_product_selected( product ) then
		local product_pricing = StoreManager.PricingManager:get_product_price( product.product_id )
		local first_night_string = "$3"
		if product_pricing.initial_night ~= nil then
			first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
		end
		show_flag_data( index, "image_flag_red_small", string.gsub( strings.flag_price_test, "{0}", first_night_string ), true )
		return
	end

	-- Back Again Titles
	if daysByCalculatedSortDate <= 14
	and product.genres[1025] ~= nil
	and browse_filter_family_tracker.selected_family ~= "Games"
	and not is_product_selected( product ) then
		show_flag_data( index, "image_flag_red_small", strings.flag_back_again, true )
		return
	end

	-- Classics
	if daysByCalculatedSortDate <= 14
	and product.genres[1095] ~= nil
	and browse_filter_family_tracker.selected_family ~= "Movies"
	and not is_product_selected( product ) then
		show_flag_data( index, "image_flag_red_small", strings.flag_classics, true )
		return
	end

	-- New Releases
	if days <= 28
	and not is_product_selected( product ) then
	--and browse_filter_family_tracker.selected_family ~= "Games" then -- Per Holly Joshi 9-Sep-2011 Email
		show_flag_data( index, "image_flag_red_small", strings.flag_new, true )
		return
	end

	-- Empty Case
	if product.product_id == 1139 then
		if ProductCatalog:in_stock(product.product_id, product_history ) == false then
			show_flag_data( index, "image_flag_gray_small", strings.flag_out_of_stock, false )
		else
			kernel.hideactor( "flag_" .. tostring( index ) )
			kernel.showactor( "add_movie_" .. tostring( index ) )
			kernel.enableactor( "add_movie_lower_" .. tostring( index ) )
		end
		return
	end

	-- Last Chance
	if days >= 105
	and not is_product_selected( product )
	and browse_filter_family_tracker.selected_family ~= "Games"
	and is_product_thinned( product.product_id ) == true
	then
		show_flag_data( index, "image_flag_orange_small", strings.flag_last_chance, true )
		return
	end

	-- Default is to hide the flag and allow user to add to cart unless cart is full
	kernel.hideactor( "flag_" .. tostring( index ) )
	if is_cart_empty < 0 or is_cart_empty > 5 then
		kernel.showactor( "add_movie_" .. tostring( index ) )
		kernel.enableactor( "add_movie_lower_" .. tostring( index ) )
	else
		kernel.hideactor( "add_movie_" .. tostring( index ) )
		kernel.disableactor( "add_movie_lower_" .. tostring( index ) )
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

function add_actor_to_alphaview(type,position,x,y)
	if type == "barrier_label" then
		aplha_scene_manager:add_actor(
			"barrier_label_" ..position,
			x,
			y+72,
			30,
			30,
			{
				renderOptions = "Text",
				foregroundColor ="black",
				horizontalAlignment = "center",
				verticalAlignment ="center",
				font = "font_akzidenz_bold_small"
			}
		)
	elseif type == "movie" then
		aplha_scene_manager:add_actor(
			"movie_" ..tostring( position ),
			x,
			y+57,
			40,
			56,
			{
				renderOptions = "Image, DrawBorder, Background",
				borderColor = { 60,56,57 },
				backgroundColor = "0,0,0",
				hotSpot = "0,0,230,60",
				strokeWeight = 1,
				hitOptions="Enabled"
			}
		)
	elseif type == "title" then
		aplha_scene_manager:add_actor(
			"movie_" ..tostring( position ) .."_title",
			x+60,
			y+75,
			190,
			30,
			{
				font = "font_akzidenz_regular_smaller_13",
				foregroundColor = "black",
				optionFlags = "Text",
				hitOptions="Enabled"
			}
		)
	elseif type == "add_movie" then
		aplha_scene_manager:add_actor(
			"add_movie_" ..tostring( position ),
			x+235,
			y+65,
			43,
			43,
			{
				hotSpot = "-10, -10, 60, 60",
				image ="image_add_to_cart",
				hitOptions="Enabled"
			}
		)
	end
end

local function register_add_movie_hit()
	-- Add movie Plus signs
	for i = 1, 20 do
			if i ~= 7 or i ~= 13 then
				kernel.registeractorhitnotify(
					"add_movie_" .. tostring( i ),
					function()
						view_analytics_event( "buttonPress", "shopping-cart-add-item" )
						selected_movie = "movie_" .. tostring( i )
						if browse_filter_mode == "for-sale" then
							browse_selection_cart_action = "purchase"
						else
							browse_selection_cart_action = "rental"
						end
						item_added_from_view = kernel.peekview()
						add_item_tocart( 21 )
					end
				)
			end
		end
end

draw_images_set = function( filtered_products_list )
	if browse_selected_sort == "alpha" 
	and #filtered_products_list > 0 then
		draw_barriers()
	end

	max_set_number = math.ceil( #filtered_products_list / total_images_count )
	kernel.enableactor( "left_arrow_button" )
	kernel.enableactor( "right_arrow_button" )
	x=80
	y=80
	if set_number ~= 0 then
		if max_set_number > 0 then
			kernel.showactor( "left_arrow_button" )
			kernel.showactor( "right_arrow_button" )
		else
			kernel.hideactor( "left_arrow_button" )
			kernel.hideactor( "right_arrow_button" )
		end

		local start_image_number = ( set_number * total_images_count ) - ( total_images_count - 1 )
		local end_image_number = set_number * total_images_count
		local left_upper_image_number = 0
		local left_bottom_image_number = 0
		local right_upper_image_number = 0
		local right_bottom_image_number = 0
		if browse_selected_sort ~= "alpha" then
			left_upper_image_number = ( set_number * 10 ) - 15
			left_bottom_image_number = ( set_number * 10 ) - 10
			right_upper_image_number = ( set_number * 10 ) + 1
			right_bottom_image_number = ( set_number * 10 ) + 6
		end
		local required_image_list = {}

		for i = 1, 10 do
			movie_product_map[ "movie_".. i ] = nil
		end

		local i = 1
		for k, v in ipairs( filtered_products_list ) do
			if k >= start_image_number and k <= end_image_number and browse_selected_sort ~= "alpha" then
				table.insert( required_image_list, v)
				if browse_selected_sort == "alpha" then
					if i == 7 or i == 14 then i = i + 1 end
				end
				movie_product_map[ "movie_"..i ] = v
				i = i + 1
			end
		end

		if browse_selected_sort=="alpha" then
			for k, v in ipairs( filtered_products_list ) do
				if k >= start_image_number and k <= end_image_number then
					if v == "" then
						if i == 7 or i == 14 then i = i + 1 end
						table.insert( required_image_list, "" )
						movie_product_map[ "movie_"..i ] = v
						i = i + 1
					else
						table.insert( required_image_list, v )
						if i == 7 or i == 14 then i = i + 1 end
						movie_product_map[ "movie_"..i ] = v
						i = i + 1
					end
				end
			end
		end
		if browse_selected_sort == "alpha" and i < 20 then
			while i <= 20 do
				if browse_selected_sort == "alpha" then
					if i == 7 or i == 14 then i = i + 1 end
				end
				movie_product_map[ "movie_" .. i ] = nil
				i = i + 1
			end
		elseif i < 10 then
			while i <= 10 do
				movie_product_map[ "movie_" .. i ] = nil
				i = i + 1
			end
		end

		--draw the image that are in visible region
		local images_count = #required_image_list
		local position = 1
		local headers_counted = 0
		local displayed_barrier_tags = 0

		for k, v in ipairs( required_image_list ) do
			local is_cart_empty = find_empty_index()
			if ( k == 7 ) then
				x=380
				y=80
			end
			if ( k == 13 ) then
				x=675
				y=80
			end
			if ( k == 7 or k == 13 ) and browse_selected_sort == "alpha" then
				position = position + 1
				headers_counted = headers_counted + 1
			end

			if v == ""  then
				kernel.hideactor( "add_movie_" .. position )
				kernel.disableactor( "add_movie_lower_" .. position )
				kernel.hideactor( "movie_" .. position )
				kernel.hideactor( "movie_" .. position .. "_title" )
				kernel.showactor( "barrier_" .. position )
				kernel.showactor( "barrier_label_" .. position )
				local displaycharacter
				if k < #required_image_list then
					displaycharacter = string.upper( string.sub( required_image_list[ k + 1 ].sort_name, 0, 1) )
					if AlphaRanges[8].ranges[ tonumber( displaycharacter ) ] ~= nil then
						displaycharacter = "#"
					end
					if browse_selected_sort == "alpha" then
						add_actor_to_alphaview( "barrier_label", position, x, y )
						y=y+70
						kernel.setactorbackgroundcolor("separator_" ..position + 1, {["r"] = 0, ["g"] = 0, ["b"] = 0})
						kernel.setactorheight("separator_" ..position + 1, 2)

					end
					displayed_barrier_tags = displayed_barrier_tags + 1
					kernel.setactortext( "barrier_label_" .. position, displaycharacter )
				else
					for i, j in pairs( browse_filtered_products ) do
						if required_image_list[ k - 1 ].sort_name == j.sort_name then
							local label_name=string.upper( string.sub( browse_filtered_products[ i + 1 ].sort_name, 0, 1 ) )
							if browse_selected_sort == "alpha" then
								add_actor_to_alphaview( "barrier_label", position, x, y )
								y=y+70
							end
							kernel.setactortext( "barrier_label_" .. position, label_name )
							displayed_barrier_tags = displayed_barrier_tags + 1
							break
						end
					end
				end
				kernel.registeractorhitnotify(
					"movie_" .. tostring( position ),
					function()
					end
				)
			else
				if browse_selected_sort == "alpha" then
					add_actor_to_alphaview( "movie", position, x, y )
				else
					kernel.showactor( "add_movie_" .. position )
					kernel.enableactor( "add_movie_lower_" .. position )
					kernel.showactor( "movie_" .. position )
					kernel.showactor( "movie_" .. position .. "_title" )
				end
				is_selected = is_product_selected( v )
				draw_image( v, "movie_" .. tostring( position ) )
				kernel.registeractorhitnotify( "movie_" .. tostring( position ),
					function()
						browse_selection_selected_product = v
						product_detail_shown_from_view = kernel.peekview()
						view_show( "product_detail_view" )
					end
				)
			end
			if v ~= "" then
				if browse_selected_sort == "alpha" then
					add_actor_to_alphaview( "title", position, x, y )
					local movie_title = required_image_list[ k ].long_name
					if string.len( movie_title ) > 18 then
						local index = 1
						local indexoflastspace = 0
						movie_title  = string.sub( movie_title, 0, 19 )
						while indexoflastspace < 19 do
							local j = string.find( movie_title, "%s", index )
							if j == nil then
								break
							end
							index = j + 1
							indexoflastspace = j
						end
						movie_title  = string.sub( movie_title, 0, indexoflastspace - 1 ) .. "..."
					else
						movie_title  = string.sub( movie_title, 0, 18 )
					end
					kernel.setactortext( "movie_" ..tostring( position ) .."_title", movie_title )
					kernel.registeractorhitnotify( "movie_" ..tostring( position ) .."_title",
						function()
							browse_selection_selected_product = v
							product_detail_shown_from_view = kernel.peekview()
							view_show( "product_detail_view" )
						end
					)
					add_actor_to_alphaview( "add_movie", position, x, y )
					kernel.registeractorhitnotify(
						"add_movie_" .. tostring( position ),
						function()
							view_analytics_event( "buttonPress", "shopping-cart-add-item" )
							selected_movie = "movie_" .. tostring( i )
							browse_selection_cart_action = "rental"
							item_added_from_view = kernel.peekview()
							add_item_tocart( 21 )
						end
					)
					y=y+70
				else
					--display banner
					local product_type = ProductTypes[ v.product_type_id ]
					if browse_filter_mode ~= "coming-soon" then
						kernel.hideactor( "banner_coming_soon_" .. tostring( position ) )
						kernel.hideactor( "button_email_reminder_" .. tostring( position ) )
						kernel.hideactor( "button_email_reminder_set_" .. tostring( position ) )
						display_banner( product_type.product_type_name, product_type.product_family_id, position )
						--display flag
						display_flag( v, position )
					end
				end
			end

			position = position + 1

			if v.product_id ~= 1139 then
				--check if the movie is already selected or out of stock
				if is_selected == true or ProductCatalog:in_stock( v.product_id, product_history ) == false or v.is_available == false then
					kernel.hideactor( "add_movie_" .. tostring( position - 1 ) )
					kernel.disableactor( "add_movie_lower_" .. tostring( position - 1 ) )
					if browse_selected_sort == "alpha" then
						kernel.setactoropacity( "movie_" .. tostring( position - 1 ), alpha_opacity )
					else
						kernel.setactoropacity( "movie_" .. tostring( position - 1 ), browse_opacity )
					end
				else
					kernel.showactor( "add_movie_" .. tostring( position - 1 ) )
					kernel.setactoropacity( "movie_" .. tostring( position - 1 ), 1 )
					if is_cart_empty < 1 or is_cart_empty > 5 then
						kernel.hideactor( "add_movie_" .. tostring( position - 1 ) )
						kernel.disableactor( "add_movie_lower_" .. tostring( position - 1 ) )
						if browse_selected_sort == "alpha" then
							kernel.setactoropacity( "movie_" .. tostring( position - 1 ), alpha_opacity )
						else
							kernel.setactoropacity( "movie_" .. tostring( position - 1 ), browse_opacity )
						end
					else
						kernel.showactor( "add_movie_" .. tostring( position - 1 ) )
						kernel.enableactor( "add_movie_lower_" .. tostring( position - 1 ) )
						kernel.setactoropacity( "movie_" .. tostring( position - 1 ), 1 )
					end
				end
				if browse_filter_mode == "coming-soon" then
					local coming_soon_release_date = StoreManager.PricingManager:get_early_release_date( v )
					local release_date = getmonth( string.sub( coming_soon_release_date, 5, 6 ) ) .. " " .. string.sub( coming_soon_release_date, 7, 8 )
					kernel.setactoropacity( "movie_" .. tostring( position - 1 ), 1)
					kernel.hideactor( "add_movie_" .. tostring( position - 1 ) )
					kernel.disableactor( "add_movie_lower_" .. tostring( position - 1 ) )
					kernel.hideactor( "flag_" .. tostring( position - 1 ) )
					kernel.showactor( "banner_" .. tostring( position - 1 ) )
					kernel.setactortext( "banner_" .. tostring( position - 1 ), strings.coming_label_Upper .. " " .. release_date )
					kernel.setactorimage( "banner_" .. tostring( position - 1 ), "image_banner_coming_soon" )
					if ComingSoonOptIn == true then
						local pos = position - 1
						kernel.registeractorhitnotify( "button_email_reminder_" .. tostring( position - 1 ),
							function()
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
													item_id = tostring( v.product_id ),
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
											item_id = tostring( v.product_id ),
											item_type = "KioskRentalTitleID",
											optin_method_type = "EmailAddress",
											optin_method_data = ShoppingCart.coming_soon_optin.email_address,
											template_name = "EmailTemplate",
											campaign_name = "ComingSoonRentalCampaign"
										}
									)
									kernel.hideactor( "button_email_reminder_" .. tostring( pos ) )
									kernel.showactor( "button_email_reminder_set_" .. tostring( pos ) )
								end
							end
						)
						kernel.registeractorhitnotify( "button_email_reminder_set_" .. tostring( position - 1 ),
							function()
								table.remove( ShoppingCart.coming_soon_optin.optin_items, ShoppingCart.coming_soon_optin:indexof_optin_item( v.product_id ) )
								kernel.showactor( "button_email_reminder_" .. tostring( pos ) )
								kernel.hideactor( "button_email_reminder_set_" .. tostring( pos ) )
							end
						)
						kernel.hideactor( "banner_" .. tostring( position - 1 ) )
						kernel.showactor( "banner_coming_soon_" .. tostring( position - 1 ) )
						if ShoppingCart.coming_soon_optin:indexof_optin_item( v.product_id ) > 0 then
							kernel.hideactor( "button_email_reminder_" .. tostring( position - 1 ) )
							kernel.showactor( "button_email_reminder_set_" .. tostring( position - 1 ) )
						else
							kernel.showactor( "button_email_reminder_" .. tostring( position - 1 ) )
							kernel.hideactor( "button_email_reminder_set_" .. tostring( position - 1 ) )
						end
						release_date = string.sub( coming_soon_release_date, 5, 6 ) .. "/" .. string.sub( coming_soon_release_date, 7, 8 )
						if string.sub( release_date, 1, 1 ) == "0" then
							release_date = string.sub( release_date, 2 )
						end
						kernel.setactortext( "banner_coming_soon_" .. tostring( position - 1 ), strings.coming_label .. " " .. release_date )
						kernel.setactorimage( "banner_coming_soon_" .. tostring( position - 1 ), "image_banner_coming_soon_grey" )
					end
				end

			elseif v.product_id == 1139 then
				local product_history = ProductHistory[ v.product_id ]
				if ProductCatalog:in_stock( v.product_id, product_history ) == false then
					if browse_selected_sort == "alpha" then
						kernel.setactoropacity( "movie_" .. tostring( position - 1 ), alpha_opacity )
					else
						kernel.setactoropacity( "movie_" .. tostring( position - 1 ), browse_opacity )
					end
					kernel.disableactor( "flag_" .. tostring( position - 1 ) )
					kernel.disableactor( "banner_" .. tostring( position - 1 ) )
					kernel.hideactor( "add_movie_" .. tostring( position - 1 ) )
					kernel.disableactor( "add_movie_lower_" .. tostring( position - 1 ) )
				else
					if is_cart_empty < 1 or is_cart_empty > 5 then
						kernel.hideactor( "add_movie_" .. tostring( position - 1 ) )
						kernel.disableactor( "add_movie_lower_" .. tostring( position - 1 ) )
						if browse_selected_sort == "alpha" then
							kernel.setactoropacity( "movie_" .. tostring( position - 1 ), alpha_opacity )
						else
							kernel.setactoropacity( "movie_" .. tostring( position - 1 ), browse_opacity )
						end
					else
						kernel.showactor( "add_movie_" .. tostring( position - 1 ) )
						kernel.enableactor( "add_movie_lower_" .. tostring( position - 1 ) )
						kernel.setactoropacity ("movie_" .. tostring( position - 1 ), 1 )
					end
				end
			end
		end

		if browse_selected_sort == "alpha" then
			total_products_shown = position - headers_counted - displayed_barrier_tags - 1
			if ( total_products_shown < 0 ) then
				total_products_shown = 0
			end
		else
			total_products_shown = position - 1
		end
		view_analytics_event( "productsShown", tostring( total_products_shown ) )

		if images_count < total_images_count then
			local images_left = images_count + 1
			local count = total_images_count
			if count == 18 then
				count = count + 2
			end
			if ( images_count > 6 and images_count <= 12 ) and browse_selected_sort == "alpha" then
				images_left = images_left + 1
			elseif( images_count > 12 and images_count <= 18 ) and browse_selected_sort == "alpha" then
				images_left = images_left + 2
			end
			while images_left <= count do
				kernel.clearactorimage( "movie_" .. tostring( images_left ) )
				kernel.hideactor( "movie_" .. tostring( images_left ) )
				kernel.hideactor( "add_movie_" .. tostring( images_left ) )
				kernel.disableactor( "add_movie_lower_" .. tostring( images_left ) )
				kernel.hideactor( "banner_" .. tostring( images_left ) )
				kernel.hideactor( "banner_coming_soon_" .. tostring( images_left ) )
				kernel.hideactor( "button_email_reminder_" .. tostring( images_left ) )
				kernel.hideactor( "button_email_reminder_set_" .. tostring( images_left ) )
				kernel.hideactor( "flag_" .. tostring( images_left ) )
				kernel.setactortext( "movie_" .. tostring( images_left ), "" )
				if browse_selected_sort == "alpha" then
					kernel.setactortext( "movie_" .. tostring( images_left ) .. "_title", "" )
				end
				kernel.setactorbackgroundcolor( "separator_" .. tostring( images_left ), "dimgray" )
				kernel.registeractorhitnotify(
					"movie_" .. tostring( images_left ),
					function()
					end
				)
				images_left = images_left + 1
			end
		end

		if browse_selected_sort ~= "alpha" then
			if #filtered_products_list > 0 then
				--draw the border images
				draw_image( filtered_products_list[ left_upper_image_number ], "movie_11" )
				draw_image( filtered_products_list[ left_bottom_image_number ], "movie_12" )
				draw_image( filtered_products_list[ right_upper_image_number ], "movie_13" )
				draw_image( filtered_products_list[ right_bottom_image_number ], "movie_14" )
			else
				kernel.clearactorimage( "movie_11" )
				kernel.clearactorimage( "movie_12" )
				kernel.clearactorimage( "movie_13" )
				kernel.clearactorimage( "movie_14" )
				kernel.hideactor( "movie_11" )
				kernel.hideactor( "movie_12" )
				kernel.hideactor( "movie_13" )
				kernel.hideactor( "movie_14" )
			end
		end
	end

	if set_number == 1 then
		kernel.hideactor( "left_arrow_button" )
		if browse_selected_sort ~= "alpha" then
			kernel.clearactorimage( "movie_11" )
			kernel.clearactorimage( "movie_12" )
			kernel.hideactor( "movie_11" )
			kernel.hideactor( "movie_12" )
		end
	end
	if set_number == max_set_number then
		kernel.hideactor( "right_arrow_button" )
		if browse_selected_sort ~= "alpha" then
			kernel.clearactorimage( "movie_13" )
			kernel.clearactorimage( "movie_14" )
			kernel.hideactor( "movie_13" )
			kernel.hideactor( "movie_14" )
		end
	end
	if browse_selected_sort == "alpha" then
		register_add_movie_hit()
	end
end

ProductCatalog.show_selection_grid = function( self )
	-- cache globals to locals for improved performance
	local _ProductTypes = ProductTypes
	local _ProductCatalog = ProductCatalog
	local _ProductFamilies = ProductFamilies
	local _ProductHistory = ProductHistory
	local _ProductTypesMetaData = ProductTypesMetaData
	local _browse_pager = browse_pager
	local _selection_grid_scene_manager = selection_grid_scene_manager
	local _browse_filtered_products = browse_filtered_products
	remove_pagination_dots()
	if browse_selected_sort == "alpha" then
		browse_filtered_products_table = alpha_filter_list( browse_filtered_products )
		draw_images_set( browse_filtered_products_table )
	else
		draw_images_set( _browse_filtered_products )
	end
	draw_pagination_dots()
end

get_product_from_actor = function( actor_name )
	return movie_product_map[ actor_name ]
end

--The full cart popup will be shown after a delay of 50 ms so that the browse screen completes rendering and all the movies in the mini cart are displayed.
fullcart_popup_timer = newidletimer( "fullcart_popup_timer" )
fullcart_popup_timer:initialize(
	50,
	function()
		disable_backgroundactors()
		view_show_safe( "cartfull_message_view" )
		fullcart_popup_timer:stop()
	end
)
show_shopping_cart_full = function()
	fullcart_popup_timer:start()
end

--show_shopping_cart_full = function()
--	disable_backgroundactors()
--	view_show_safe( "cartfull_message_view" )
--end

hide_shopping_cart_full = function()
	kernel.hideactor( "cart_full_background" )
	kernel.hideactor( "cart_full_label_border" )
	kernel.hideactor( "cart_full_label" )
	kernel.hideactor( "cart_full_label_text" )
	kernel.hideactor( "cart_full_image" )
	kernel.hideactor( "cart_full_cancel_button" )
	enable_backgroundactors()
	highlight_sort_button( browse_selected_sort )
	browse_is_cart_full_open = false
end

add_movie = function( total_count )
	local selected_index = string.sub( selected_movie, 7 )
	local selected_product = get_product_from_actor( selected_movie )
	if (selected_product.product_id ~= customer_selected_product_id) then
		return
	end
	browse_selection_selected_product = selected_product
	--If Redbox empty case setting action as purchase
	if selected_product.product_id == 1139 or browse_filter_mode == "for-sale" then
		browse_selection_cart_action = "purchase"
	else
		browse_selection_cart_action = "rental"
	end

	kernel.executescript( "add_to_cart" )

	local empty_index = find_empty_index()
	if empty_index ~= 0 then
		selected_movies_list[ empty_index ] = selected_product
		movie_product_map[ "selectedmovie_".. empty_index ] = selected_product
		draw_image( selected_product, "selectedmovie_" .. empty_index )
		kernel.showactor( "selectedmovie_" .. empty_index .. "_border" )
		kernel.showactor( "cancel_" .. empty_index )
		
		if browse_selection_cart_action == "purchase" then
			kernel.showactor( "selectedmovie_" .. empty_index .. "_flag" )
		end

		--Checking if the selected product is redbox empty case and not hiding the add button
		if selected_product.product_id ~= 1139 then
			if browse_selected_sort == "alpha" then
				kernel.setactoropacity( selected_movie, alpha_opacity )
			else
				kernel.setactoropacity( selected_movie, browse_opacity )
			end

			kernel.disableactor( "flag_" .. selected_index )
			kernel.disableactor( "banner_" .. selected_index )
			kernel.hideactor( "add_movie_" .. selected_index )
			kernel.disableactor( "add_movie_lower_" .. selected_index )
		elseif selected_product.product_id == 1139 then
			local product_history = ProductHistory[ selected_product.product_id ]

			if ProductCatalog:in_stock( selected_product.product_id, product_history ) == false then
				if browse_selected_sort == "alpha" then
					kernel.setactoropacity( "movie_" .. selected_index, alpha_opacity )
				else
					kernel.setactoropacity( "movie_" .. selected_index, browse_opacity )
				end
				kernel.disableactor( "flag_" .. selected_index )
				kernel.disableactor( "banner_" .. selected_index )
				kernel.hideactor( "add_movie_".. selected_index )
				kernel.disableactor( "add_movie_lower_".. selected_index )
			end
		end

		--display flag
		display_flag( selected_product, selected_index )

		if kernel.getculture() == "en-US" then
			kernel.setactortext( "my_rentals_button", "CHECK OUT (" .. #ShoppingCart.items .. ")" )
		elseif kernel.getculture() == "es-MX" then
			kernel.setactortext( "my_rentals_button", "CAJA (" .. #ShoppingCart.items .. ")" )
		end

		kernel.hideactor( "add_to_cart_label" )
		kernel.hideactor( "add_to_cart_label_image" )
	end

	local index = 1
	while index < 6 do
		if movie_product_map[ "selectedmovie_" .. index ] ~= nil then
			index = index + 1
		else
			return
		end
	end

	local count = 1
	if index == 6 then
		while count < total_count do
			if count ~= 7 or count ~= 13 then
				kernel.hideactor( "add_movie_".. count )
				kernel.disableactor( "add_movie_lower_".. count )
				if browse_selected_sort == "alpha" then
					kernel.setactoropacity( "movie_".. count, alpha_opacity )
				else
					kernel.setactoropacity( "movie_".. count, browse_opacity )
				end
			end
			count = count + 1
		end
	end
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

remove_movie = function( total_count )
	local selected_index = string.sub( selected_movie, 15 )
	local selected_product = get_product_from_actor( selected_movie )

	--removing movie from cart
	for k, v in ipairs( ShoppingCart.items ) do
		if selected_product.product_id == v.product_id then
			ShoppingCart:remove_item( k )
			break
		end
	end

	local is_present = false
	local desired_actor_count = 1
	while( desired_actor_count <= total_count ) do
		if movie_product_map[ "movie_" .. desired_actor_count ] == selected_product then
			is_present = true
			break
		end
		desired_actor_count = desired_actor_count + 1
	end

	if is_present then
		kernel.enableactor( "movie_" .. desired_actor_count )
		kernel.setactoropacity( "movie_" .. desired_actor_count, 1 )
		kernel.enableactor( "flag_" .. desired_actor_count )
		kernel.showactor( "add_movie_" .. desired_actor_count )
		kernel.enableactor( "add_movie_lower_" .. desired_actor_count )
		kernel.registeractorhitnotify(
			"movie_" .. tostring( desired_actor_count ),
			function()
				browse_selection_selected_product = selected_product
				product_detail_shown_from_view = kernel.peekview()
				view_show( "product_detail_view" )
			end
		)

		--display flag
		display_flag( selected_product, desired_actor_count )
	end

	selected_index = selected_index + 1
	while( selected_index < 6 ) do
		if movie_product_map[ "selectedmovie_" .. selected_index ] ~= nil then
			selected_movie = "selectedmovie_" .. selected_index
			local desired_product = get_product_from_actor( selected_movie )
			movie_product_map[ "selectedmovie_" .. selected_index - 1 ] = desired_product
			if ShoppingCart.items[selected_index - 1].action == "purchase" then
				kernel.showactor( "selectedmovie_" .. selected_index - 1 .. "_flag")
			else
				kernel.hideactor( "selectedmovie_" .. selected_index - 1 .. "_flag")
			end
			draw_image( desired_product, "selectedmovie_" .. selected_index - 1 )
			selected_index = selected_index + 1
		else
			break
		end
	end
	movie_product_map[ "selectedmovie_".. selected_index - 1 ] = nil
	kernel.hideactor( "selectedmovie_" .. selected_index - 1 .. "_border" )
	kernel.hideactor( "cancel_" .. selected_index - 1 )
	kernel.clearactorimage( "selectedmovie_" .. selected_index -1 )
	kernel.hideactor( "selectedmovie_" .. selected_index - 1 .. "_flag")

	local empty_index = find_empty_index()

	if empty_index == 0 then
		kernel.showactor( "add_to_cart_label" )
		kernel.showactor( "add_to_cart_label_image" )
	end

	for k,v in ipairs( selected_movies_list ) do
		if selected_product.product_id == v.product_id then
			table.remove( selected_movies_list, k )
			break
		end
	end

	if kernel.getculture() == "en-US" then
		kernel.setactortext( "my_rentals_button", "CHECK OUT (" .. #ShoppingCart.items .. ")" )
	elseif kernel.getculture() == "es-MX" then
		kernel.setactortext( "my_rentals_button", "CAJA (" .. #ShoppingCart.items .. ")" )
	end

	--add + button to other images in the screen except the selected images
	local listed_images_count = 1
	while listed_images_count <= total_count do
		local prod = get_product_from_actor( "movie_" .. listed_images_count )
		if prod ~= nil and prod.product_id ~= 1139 then
			local is_selected = is_product_selected( prod )
			local product_history = ProductHistory[ prod.product_id ]
			if is_selected == false
			and movie_product_map[ "movie_" .. listed_images_count ] ~= nil then
				if ProductCatalog:in_stock( prod.product_id, product_history ) == true
				and prod.is_available == true then
					kernel.showactor( "add_movie_" .. listed_images_count )
					kernel.enableactor( "add_movie_lower_" .. listed_images_count )
					kernel.setactoropacity( "movie_" .. listed_images_count, 1 )
					kernel.registeractorhitnotify(
						"movie_" .. tostring( listed_images_count ),
						function()
							browse_selection_selected_product = prod
							view_analytics_event( "buttonPress", "productSelected," .. tostring( prod.product_id ) )
							product_detail_shown_from_view = kernel.peekview()
							view_show( "product_detail_view" )
						end
					)
				end
			end
		elseif prod ~= nil and prod.product_id == 1139 then
			local product_history = ProductHistory[ prod.product_id ]
			if ProductCatalog:in_stock( prod.product_id, product_history ) == false then
				if browse_selected_sort == "alpha" then
					kernel.setactoropacity( "movie_" .. listed_images_count, alpha_opacity )
				else
					kernel.setactoropacity( "movie_" .. listed_images_count, browse_opacity )
				end
				kernel.disableactor( "flag_" .. listed_images_count )
				kernel.disableactor( "banner_" .. listed_images_count )
				kernel.hideactor( "add_movie_" .. listed_images_count )
				kernel.disableactor( "add_movie_lower_" .. listed_images_count )
			else
				kernel.enableactor( "movie_" .. listed_images_count )
				kernel.setactoropacity( "movie_" .. listed_images_count, 1)
				kernel.hideactor( "flag_" .. listed_images_count )
				kernel.enableactor( "banner_" .. listed_images_count )
				kernel.showactor( "add_movie_" .. listed_images_count )
				kernel.enableactor( "add_movie_lower_" .. listed_images_count )
			end
		end
		listed_images_count = listed_images_count + 1
	end

	if empty_index == 1 then
		kernel.showactor( "add_to_cart_label" )
		kernel.showactor( "add_to_cart_label_image" )
	end
	hide_shopping_cart_full()
	highlight_sort_button( browse_selected_sort )

	if browse_selected_sort ~= "alpha" then
		set_actors_opacity(1)
	end
end

function add_item_tocart( i )
	local selected_product = get_product_from_actor( selected_movie )
	customer_selected_product_id = selected_product.product_id
	if selected_movie ~= nil
	and StoreManager.PricingManager:is_early_released( get_product_from_actor( selected_movie ) ) then

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
		check_for_warning_popups( check_alert_table, selected_movie, i )
	end
end

warning_popup_timer = newidletimer("warning_popup_timer")
warning_popup_timer:initialize(
	15000,
	function()
		hide_warning_popup()
	end )

function check_for_warning_popups( check_alert_table, selected_movie, i )

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

	if i == nil then
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
	show_warning_alert( check_alert_table, selected_movie, i )
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

function show_warning_alert( check_alert_table, selected_movie, i )
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
					show_blu_ray_alert( check_alert_table, selected_movie, i )
					kernel.setactoropacity( "bluray_scene_background" , 0 )
				elseif subplatform_game_in_cart == true then
					show_sub_platform_warning( check_alert_table, selected_movie, i )
				elseif purchase_product_in_cart == true then
					show_purchase_warning( check_alert_table, selected_movie, i )
					kernel.setactoropacity( "purchase_scene_background" , 0 )
				else
					warning_popup_timer:stop()
					add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action )
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
		show_blu_ray_alert( check_alert_table, selected_movie, i )
	elseif subplatform_game_in_cart == true then
		show_sub_platform_warning( check_alert_table, selected_movie, i )
	elseif purchase_product_in_cart == true then
		show_purchase_warning( check_alert_table, selected_movie, i )
	else
		add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action )
	end
end

function hide_warning_popup()
	view_unwindto_safe( item_added_from_view, true, "Message view Timed out." )
	warning_popup_timer:stop()

	if item_added_from_view ~= "product_detail_view"
	and item_added_from_view ~= Start_Over_View_Name then
		if #ShoppingCart.items == 5 then
			disable_backgroundactors()
		else
			enable_backgroundactors()
			highlight_sort_button( browse_selected_sort )
		end
		if browse_selected_sort == "alpha" then
			refresh_barriers()
			draw_images_set( browse_filtered_products_table )
		end
	end
end

function show_warning_popup()
	warning_popup_timer:start()
	for i = 1,5,1 do
		kernel.disableactor( "cancel_" .. i)
	end
	disable_backgroundactors()
	kernel.showactor( "scene_background" )
	kernel.showactor( "center_background" )
	kernel.showactor( "rating_label" )
	kernel.showactor( "message_label" )
	kernel.showactor( "mature_rating_back_button" )
	kernel.showactor( "mature_rating_back_button_arrow" )
	kernel.showactor( "continue_button" )
end

function show_blu_ray_alert( check_alert_table,selected_movie, i )
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
				show_purchase_warning( check_alert_table, selected_movie, i )
				kernel.setactoropacity( "purchase_scene_background" , 0 )
					-- Need to bail here or else lower part of routine will hide the puchase warning before user sees it
				return
			else
				add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action )
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

function show_sub_platform_warning( check_alert_table,selected_movie,i )
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
				show_purchase_warning( check_alert_table, selected_movie, i )
			else
				add_item_to_shopping_cart( selected_movie, i, browse_selection_cart_action )
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

function show_purchase_warning( check_alert_table, selected_movie, i )
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
			add_item_to_shopping_cart(selected_movie,  i, browse_selection_cart_action )
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

function add_item_to_shopping_cart( selected_movie, i, action )
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
		add_movie( i )
		view_unwindto_safe( item_added_from_view, true, "Item added" )
	end
	browse_selection_cart_action = "rental"
end

function disable_backgroundactors()
	kernel.disableactor( "left_arrow_button" )
	kernel.disableactor( "right_arrow_button" )
	kernel.disableactor( "tab_switch_product_family" )
	kernel.disableactor( "startover_button" )
	kernel.disableactor( "my_rentals_button" )
	kernel.hideactor( "newest_button" )
	kernel.hideactor( "abc_button" )
	kernel.hideactor( "coming_soon_button" )
	kernel.hideactor( "selected_button" )
	if browse_filter_family_tracker.selected_family == "Movies" then
		kernel.hideactor( "blu_ray_button" )
		kernel.hideactor( "paddle_shadow" )
	elseif browse_filter_family_tracker.selected_family == "Games" then
		kernel.hideactor( "xbox_360_button" )
		kernel.hideactor( "xbox_one_button" )
		kernel.hideactor( "ps3_button" )
		kernel.hideactor( "ps4_button" )
		kernel.hideactor( "wii_button" )
		kernel.hideactor( "wii_u_button" )
	end
	for i = 1, 7 do
		kernel.disableactor( "tab_" .. tostring( i ) )
	end

	if browse_selected_sort == "alpha" then
		for k, v in ipairs( alpha_table ) do
			kernel.disableactor( v .. "_label" )
			kernel.setactorforegroundcolor( v .. "_label", "gray" )
		end
		for i = 1, 20, 1 do
			kernel.disableactor( "movie_" .. tostring( i ) )
			kernel.disableactor( "add_movie_" .. tostring( i ) )
			if i == 7 or i == 14 then
				i = i + 1
			end
		end
	else
		for i = 1, 14, 1 do
			kernel.disableactor( "movie_" .. tostring( i ) )
			kernel.disableactor( "add_movie_" .. tostring( i ) )
			kernel.hideactor( "add_movie_lower_" .. tostring( i ) )
		end
	end

	kernel.disableactor( "left_arrow_button" )
	kernel.disableactor( "right_arrow_button" )
end

function enable_backgroundactors()
	kernel.enableactor( "left_arrow_button" )
	kernel.enableactor( "right_arrow_button" )
	kernel.enableactor( "tab_switch_product_family" )
	kernel.enableactor( "startover_button" )
	kernel.enableactor( "my_rentals_button" )
	if ShoppingCart.in_cart_title_marketing == false then
		kernel.showactor( "newest_button" )
		kernel.showactor( "coming_soon_button" )
		kernel.showactor( "selected_button" )
		if browse_filter_family_tracker.selected_family == "Movies" then
			kernel.showactor( "abc_button" )
			kernel.showactor( "blu_ray_button" )
			kernel.showactor( "paddle_shadow" )
			kernel.setbordercolor( "startover_button", "white" )
		elseif browse_filter_family_tracker.selected_family == "Games" then
			--kernel.showactor( "xbox_360_button" )
			--kernel.showactor( "xbox_one_button" )
			--kernel.showactor( "ps3_button" )
			--kernel.showactor( "ps4_button" )
			--kernel.showactor( "wii_button" )
			--kernel.showactor( "wii_u_button" )
		end
	end

	for i = 1, 7 do
		kernel.enableactor( "tab_" .. tostring( i ) )
	end

	if browse_selected_sort == "alpha" and browse_filter_family_tracker.selected_family ~= "Games" then
		for k, v in ipairs( alpha_table ) do
			kernel.enableactor( v .. "_label" )
		end
		for i = 1, 20, 1 do
			kernel.enableactor( "movie_" .. tostring( i ) )
			kernel.enableactor( "add_movie_" .. tostring( i ) )
			if i == 7 or i == 14 then
				i = i + 1
			end
			i = i + 1
		end
		highlight_alphabets()
	else
		for i = 1, 14, 1 do
			kernel.enableactor( "movie_" .. tostring( i ) )
			kernel.enableactor( "add_movie_" .. tostring( i ) )
			kernel.showactor( "add_movie_lower_" .. tostring( i ) )
		end
	end

	kernel.enableactor( "left_arrow_button" )
	kernel.enableactor( "right_arrow_button" )
end

function display_alpha_screen_layout()
	kernel.hideactor( "selected_label" )
	kernel.showactor( "alpha_back_ground_image" )
	kernel.hideactor( "bg_box_top" )
	for k,v in ipairs( alpha_table ) do
		kernel.setactorbackgroundcolor( v .. "_label", "black" )
	end
	for i= 1, 20, 1 do
		kernel.setactorforegroundcolor( "movie_" .. i .. "_title", "black" )
		kernel.setactorbackgroundcolor( "barrier_"..i,"black" )
		kernel.setactorbackgroundcolor( "barrier_label_"..i, "black" )
	end
end

is_dnr_disc = function( product )
	local do_not_rent_flag = false
	if product ~= nil and product.do_not_rent_date ~= nil then
		do_not_rent_flag = product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
	end
	return do_not_rent_flag
end
