--
-- lib-browse-functions.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local x=80
local y=80
local rated_movie_in_cart = false
local blu_ray_movie_in_cart = false
local rated_game_in_cart = false
local subplatform_game_in_cart = false
local purchase_product_in_cart = false
local subplatform_message = ""


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

--SubPlatformIndicatorData = {
--	[1] = { sub_platform_id = 1,
--			sub_platform_name = "Wii Motion Plus Compatible",
--			sub_platform_image = "image_wii_motion_plus_compatible" },
--	[2] = { sub_platform_id = 2,
--			sub_platform_name = "Wii Motion Plus Required",
--			sub_platform_image = "image_wii_motion_plus_required" },
--	[3] = { sub_platform_id = 3,
--			sub_platform_name = "Wii Fit Compatible",
--			sub_platform_image = "image_wii_fit_compatible" },
--	[4] = { sub_platform_id = 4,
--			sub_platform_name = "Wii Fit Required",
--			sub_platform_image = "image_wii_fit_required" },
--	[5] = { sub_platform_id = 5,
--			sub_platform_name = "Move Compatible",
--			sub_platform_image = "image_move_compatible" },
--	[6] = { sub_platform_id = 6,
--			sub_platform_name = "Move Required",
--			sub_platform_image = "image_move_required" },
--	[7] = { sub_platform_id = 7,
--			sub_platform_name = "Kinect Compatible",
--			sub_platform_image = "image_kinect_compatible" },
--	[8] = { sub_platform_id = 8,
--			sub_platform_name = "Kinect Required",
--			sub_platform_image = "image_kinect_required" },
--	[9] = { sub_platform_id = 9,
--			sub_platform_name = "Nunchuck for Wii U",
--			sub_platform_image = "image_nunchuk_required" },
--	[10] = { sub_platform_id = 10,
--			sub_platform_name = "Wiimote for Wii U",
--			sub_platform_image = "image_wii_mote_required" },
--	[11] = { sub_platform_id = 11,
--			sub_platform_name = "PS4 Move Compatible",
--			sub_platform_image = "image_ps4_move_compatible" },
--	[12] = { sub_platform_id = 12,
--			sub_platform_name = "PS4 Move Required",
--			sub_platform_image = "image_ps4_move_required" },
--	[13] = { sub_platform_id = 13,
--			sub_platform_name = "PS4 Camera Required",
--			sub_platform_image = "image_ps4_camera_required" },
--	[14] = { sub_platform_id = 14,
--			sub_platform_name = "Xbox One Kinect Compatible",
--			sub_platform_image = "image_kinect_compatible_xbox_one" },
--	[15] = { sub_platform_id = 15,
--			sub_platform_name = "Xbox One Kinect Required",
--			sub_platform_image = "image_kinect_required_xbox_one" }
--}

--function show_browse_view()
--	if title_rollup_overlay ~= nil then
--		title_rollup_overlay:hide()
--	end

--    -- make sure we are on the correct view
--    if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" and kernel.peekview() ~= "browse_filter_alpha_view" then
--        kernel.log("switching view to A-Z")
--        view_show_safe( "browse_filter_alpha_view" )
--    else 
--        if kernel.get_session_value("BrowseState.BrowseSelectedSort") ~= "alpha" and kernel.peekview() == "browse_filter_alpha_view" then
--            kernel.log("Switching view to regular browse")
--            kernel.executescript("browse_products")
--        else
--            kernel.draw_browse_filter_bar(
--                kernel.peekview(),
--                function() 
--                    kernel.executescript( "start_over" )
--                end,
--                show_browse_view
--            )
--	        ProductCatalog:show_filtered_grid()

--	        if (kernel.get_application_state_value("IsADAAccessible") == true) then
--		        draw_ada_control()
--	        end
--        end
--    end
--end

-- this was moved to BrowseViewController.GetBrowseKioskProducts()
--ProductCatalog.filter_products = function(self)
--	local _in_cart = GetInCartPromoAction()
--	local _incart_include_titles = {}
--	local _incart_exclude_titles = {}
--	if kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true
--	and kernel.rental_shopping_cart_get_in_cart_offer_cancel( ShoppingCart.session_id ) == false
--	and _in_cart ~= nil then
--		if _in_cart.product_id_list ~= nil then
--			-- Build in-cart include ONLY these titles list
--			local title_list = string.split( _in_cart.product_id_list, "," )
--			for _, v in ipairs( title_list ) do
--				if tonumber(v) ~= nil then
--					_incart_include_titles[ tonumber( v ) ] = true
--				end
--			end
--		else
--			_incart_include_titles = nil
--		end
--		if _in_cart.exclude_id_list ~= nil
--		and #_in_cart.exclude_id_list > 0 then
--			-- Build in-cart exclude these titles list
--			local title_list = string.split( _in_cart.exclude_id_list, "," )
--			for _, v in ipairs( title_list ) do
--				if tonumber(v) ~= nil then
--					_incart_exclude_titles[ tonumber( v ) ] = true
--				end
--			end
--		else
--			_incart_exclude_titles = nil
--		end
--	else
--		_incart_include_titles = nil
--		_incart_exclude_titles = nil
--	end

--	return kernel.rental_get_products(
--		kernel.get_session_value("BrowseState.BrowseFilterMode"),
--        kernel.get_session_value("BrowseState.BrowseTitleFamily"),
--        kernel.get_session_value("BrowseState.BrowseTitleType"),
--        kernel.get_session_value("BrowseState.BrowseSelectedSort"),
--        kernel.get_session_value("BrowseState.BrowseGenre"),
--		_incart_include_titles,
--		_incart_exclude_titles,
--		kernel.get_application_state_value("IsADAAccessible") == true)

--end

-- this was moved to BrowseViewController.GetBrowseKioskProducts()
--ProductCatalog.filter_grid = function( self )

--	browse_filtered_products = self:filter_products()
--    if kernel.get_session_value("BrowseState.BrowseSelectedSort") ~= "alpha" then
--        -- move out of stock products to the end of the list
--		local In_stock_products = {}
--		local Out_of_stock_products = {}
--		for i, v in ipairs( browse_filtered_products ) do
--			if (v.in_stock == false and v.show_out_of_stock == true) then
--				table.insert( Out_of_stock_products, v )
--			else
--				table.insert( In_stock_products, v )
--			end
--		end

--		for i, v in ipairs( Out_of_stock_products ) do
--			table.insert( In_stock_products, v )
--		end
--		browse_filtered_products = In_stock_products
--	end

--	return browse_filtered_products
--end

ProductCatalog.get_default_family = function( self )
	local many_families, default_family = self:has_many_families()
	if many_families == false then
        kernel.set_session_value("BrowseState.BrowseTitleFamily", default_family or kernel.get_session_value("BrowseState.BrowseTitleFamily"))
	end
end

--ProductCatalog.show_filtered_grid = function( self )
--	if kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true
--	and kernel.rental_shopping_cart_get_in_cart_title_marketing( ShoppingCart.session_id ) == true
--	then
--		self:set_title_marketing_view()
--	end
--    self:filter_grid()
--    draw_images_set( browse_filtered_products )
--end

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
	kernel.hideactor( "nintendo_switch_button" )
    kernel.hideactor( "top_20_filter_button" )
    kernel.hideactor( "action_filter_button" )
    kernel.hideactor( "comedy_filter_button" )
    kernel.hideactor( "drama_filter_button" )
    kernel.hideactor( "family_filter_button" )
    kernel.hideactor( "horror_filter_button" )

	-- Hide the start over button
	kernel.hideactor( "startover_button" )
	kernel.hideactor( "startover_arrow" )
end

draw_image = function( product_to_draw, control_to_draw )
    if kernel.get_session_value("BrowseState.BrowseTitleFamily") == "Movies" then
		source_image_name = "image_movie_missing_box_art"
    elseif kernel.get_session_value("BrowseState.BrowseTitleFamily") == "Games" then
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

--is_product_selected = function( product )
--    -- determine if the shopping cart has all the rollup titles
--	if product ~= nil and product.kiosk_product_type == 3 and product.is_movie == true then
--		local dvd_product = nil
--		local blu_ray_product = nil

--		if product.dvd_index > 0 then
--			dvd_product = product.kiosk_products[product.dvd_index]
--		end
--		if  product.blu_ray_index > 0 then
--			blu_ray_product = product.kiosk_products[product.blu_ray_index]
--		end

--		if (dvd_product == nil and blu_ray_product ~= nil) then
--			return kernel.current_rental_shopping_cart_has_item( blu_ray_product.product_id ) == true
--		end
--		if (blu_ray_product == nil and dvd_product ~= nil) then
--			return kernel.current_rental_shopping_cart_has_item( dvd_product.product_id ) == true
--		end
--		if (blu_ray_product ~= nil and dvd_product ~= nil) then
--			-- make sure product has stock or this function doesn't work correctly
--			return (  (blu_ray_product.in_stock == false) or (kernel.current_rental_shopping_cart_has_item( blu_ray_product.product_id ) == true) ) and ( (dvd_product.in_stock == false) or  (kernel.current_rental_shopping_cart_has_item( dvd_product.product_id ) == true) )
--		end

--		return false
--	else
--		return product ~= nil and kernel.current_rental_shopping_cart_has_item( product.product_id ) == true
--	end
--end

function display_banner( type_name, productfamilytype, actor_count ) --Matt Will there be banners? no longer used?
	if ( type_name == "Blu-ray"
	or type_name == "Xbox 360"
	or type_name == "PS2"
	or type_name == "PS3"
	or type_name == "Wii"
	or type_name == "Nintendo DS"
	or type_name == "DigitalCode" )
	and productfamilytype == 1 then
		kernel.showactor( "banner_" .. tostring( actor_count ) )
		kernel.setactortext( "banner_" .. tostring( actor_count ), type_name )
		if type_name == "Blu-ray" then
			type_name = "blu_ray"
		elseif type_name == "Xbox 360" then
			type_name = "xbox"
		elseif type_name == "Nintendo DS" then
			type_name = "nintendo_ds"
		elseif type_name == "DigitalCode" then
			kernel.setactortext( "banner_" .. tostring( actor_count ), strings.digital_code )
			type_name = "digital_code"
		else
			type_name = string.lower( type_name )
		end
		kernel.setactorimage( "banner_" .. tostring( actor_count ), "image_banner_" .. type_name )
	else
		kernel.hideactor( "banner_" .. tostring( actor_count ) )
	end
end

--show_product_detail_view = function(product)
--    kernel.set_session_value("BrowseState.BrowseSelectionSelectedProductLuaTable", product)
--    kernel.set_session_value("BrowseState.ProductDetailShownFromView", kernel.peekview())
--	view_show( "product_detail_view" )
--end

--local page_number_changed = function(new_page_number, page_change_source)
--	if title_rollup_overlay ~= nil then
--		title_rollup_overlay:hide()
--	end
--	if new_page_number < kernel.get_session_value("BrowseState.PageNumber") then
--		view_analytics_event( "pageChange", "browse-page-back" )
--	else
--		view_analytics_event( "pageChange", "browse-page-next" )
--	end
--	local page_number_is_different = kernel.get_session_value("BrowseState.PageNumber") ~= new_page_number
--    kernel.set_session_value("BrowseState.PageNumber", new_page_number)
--	if kernel.get_application_state_value("IsADAAccessible") == true then
--		draw_ada_control()
--	end

--    -- increment paddle count if the page number changed but don't include A-Z character selections
--	if page_number_is_different and page_change_source == "PaddleButton" then
--		increment_paddle_click_count()
--	end

--    if kernel.ttsenabled() then
--        kernel.ttsrepeat()
--    end
--end

--increment_paddle_click_count = function ()
--	-- collection popup browse paddle click counter logic
--    local marketing_config = kernel.get_marketing_config()
--	if marketing_config ~= nil and marketing_config.collections_popup_paddle_clicks ~= nil and marketing_config.collections_popup_paddle_clicks > 0 then
--		local paddle_click_count = kernel.add_to_session_value("BrowseState.PaddleClickCount", 1) 
--		if paddle_click_count ~= nil and paddle_click_count >= marketing_config.collections_popup_paddle_clicks then
--			kernel.set_session_value("BrowseState.PaddleClickCount", 0)
--			kernel.check_collections_popup("interaction")
--		end
--	end
--end

--is_product_in_stock = function (product)
--	return ProductCatalog:in_stock( product.product_id, ProductHistory[ product.product_id ] )
--end

--update_products_shown = function( products_shown )
--	view_analytics_event( "productsShown", tostring( products_shown ) )
--end

--get_early_release_date2 = function (product)
--	return StoreManager.PricingManager:get_early_release_date( product )
--end

--is_title_marketing_mode = function ()
--	return kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true and kernel.rental_shopping_cart_get_in_cart_title_marketing( ShoppingCart.session_id ) == true
--end

--is_coming_soon_item_registered = function (product)
--	return ShoppingCart.coming_soon_optin:indexof_optin_item( product.product_id ) > 0
--end

--is_coming_soon_optin_enabled = function()
--	return ComingSoonOptIn
--end


get_email_address_and_register_email_reminder = function(product_id)
        -- after the keyboard manager is moved to .net this method should also be moved to .net
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
                kernel.current_rental_shopping_cart_coming_soon_optin_set_email_address(coming_soon_optin_keyboard_manager:get_value())
                kernel.current_rental_shopping_cart_coming_soon_optin_add_product_reminder(tostring( product_id ))
                view_unwindto_safe( browse_view_name, true, "coming soon email optin email entry screen - next button" )
			end
		)
		coming_soon_optin_keyboard_manager:show( strings.email_tag )
end

--register_email_reminder = function (product)
--	if ShoppingCart.coming_soon_optin.email_address == nil then
--		local coming_soon_optin_keyboard_manager = newkeyboardmanager()
--		coming_soon_optin_keyboard_manager:init( 64 )
--		coming_soon_optin_keyboard_manager:set_message( strings.coming_soon_email_optin_email_prompt_message )
--		coming_soon_optin_keyboard_manager:set_marketing( strings.coming_soon_email_optin_email_marketing_message )
--		show_email_confirmation = false
--		coming_soon_optin_keyboard_manager:set_validate_event(
--			function( text )
--				local coming_soon_optin_email_validator = newemailvalidator()
--				if coming_soon_optin_email_validator.validate_email( text ) == false then
--					return strings.email_error_message
--				end
--				return nil
--			end
--		)
--		coming_soon_optin_keyboard_manager:set_enter_event(
--			function()
--				ShoppingCart.coming_soon_optin.email_address = coming_soon_optin_keyboard_manager:get_value()
--				table.insert( ShoppingCart.coming_soon_optin.optin_items,
--					{
--						item_id = tostring( product.product_id ),
--						item_type = "KioskRentalTitleID",
--						optin_method_type = "EmailAddress",
--						optin_method_data = ShoppingCart.coming_soon_optin.email_address,
--						template_name = "EmailTemplate",
--						campaign_name = "ComingSoonRentalCampaign"
--					}
--				)
--				view_goback_safe( "coming soon email optin email entry screen - next button" )
--			end
--		)
--		coming_soon_optin_keyboard_manager:show( strings.email_tag )
--	else
--		table.insert( ShoppingCart.coming_soon_optin.optin_items,
--			{
--				item_id = tostring( product.product_id ),
--				item_type = "KioskRentalTitleID",
--				optin_method_type = "EmailAddress",
--				optin_method_data = ShoppingCart.coming_soon_optin.email_address,
--				template_name = "EmailTemplate",
--				campaign_name = "ComingSoonRentalCampaign"
--			}
--		)
--		draw_images_set( browse_filtered_products )
--	end
--end

--unregister_email_reminder = function(product)
--	table.remove( ShoppingCart.coming_soon_optin.optin_items, ShoppingCart.coming_soon_optin:indexof_optin_item( product.product_id ) )
--	draw_images_set( browse_filtered_products )
--end

--draw_images_set = function( filtered_products_list )
--	kernel.draw_images_set(
--		"Main",
--        kernel.get_session_value("BrowseState.BrowseTitleFamily"),
--		filtered_products_list,
--        kernel.get_session_value("BrowseState.PageNumber"),
--		page_number_changed,
--		update_products_shown, 
--		show_product_detail_view,
--		add_movie_on_hit,
--		nil,
--		is_product_selected,
--		is_product_in_stock,
--		get_early_release_date2,
--		is_title_marketing_mode,
--		is_coming_soon_optin_enabled,
--		is_coming_soon_item_registered,
--		register_email_reminder,
--		unregister_email_reminder)

--    if kernel.ttsenabled() then
--        kernel.ttsrepeat()
--    end
--end 

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
    local render_type = kernel.getviewrendertype();
    if render_type == "WPF" then
        kernel.show_cart_full_message()
    else 
	    fullcart_popup_timer:start()
    end
end

hide_shopping_cart_full = function()
	kernel.set_session_value("BrowseState.IsShoppingCartFullMessagesOpen", false)
end

--function getmonth( month )
--	local months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" }
--	return months[ tonumber( month ) ]
--end

--refresh_current_page = function()
----	if kernel.peekview() == "browse_view_collection" then
----		draw_collection_view()
----	elseif kernel.peekview() == "browse_view_collections" then
----		draw_collections_view()
----	elseif kernel.peekview() == "browse_view_ada_accessible" then
----		draw_images_set( browse_filtered_products )
----		draw_action_bar()
----		draw_ada_control()
----    elseif kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
----		draw_images_set( browse_filtered_products )
----        if (kernel.get_application_state_value("IsADAAccessible") == true) then
----		    draw_action_bar()
----		    draw_ada_control()
----        else
----		    draw_action_bar_and_mini_cart("browse_filter_alpha_view")
----        end
----	else
----		draw_images_set( browse_filtered_products )
----		draw_action_bar_and_mini_cart("browse_view")
----	end
--end

--remove_movie = function( selected_product )
--	--removing movie from cart
--    kernel.rental_shopping_cart_remove_item_product_id(ShoppingCart.session_id, selected_product, true)
--	refresh_current_page()
--	hide_shopping_cart_full()
--end

--function add_item_to_cart( selected_product, action )
--	check_for_warning_popups( selected_movie, true, selected_product, action )
--end

warning_popup_timer = newidletimer("warning_popup_timer")
warning_popup_timer:initialize(
	15000,
	function()
		hide_warning_popup()
	end )

function set_timeout_for_warning_timer( )
    local warning_popup_timeout = kernel.ttsgettimeout( "medium", 15000 )
    kernel.set_timer_period_and_duetime( "warning_popup_timer", warning_popup_timeout, warning_popup_timeout )
end

set_timeout_for_warning_timer()

function check_for_warning_popups( selected_movie, is_added_from_browse, product_to_check, action )
    -- the only this left that calls this is recomendation on pickup
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
	elseif is_added_from_browse ~= true then
		selected_product = selected_movie
	else
        -- this was only used from the browse which is now in the .net side.
		--selected_product = kernel.get_product_from_actor( selected_movie )
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
                            kernel.setmacro( "MatureWarning:TTSMessage", string.gsub(movie_mature_message, "OK", "Enter" ) )
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
                            kernel.setmacro( "MatureWarning:TTSMessage", string.gsub(game_mature_message, "OK", "Enter" ) )
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
	if action == "purchase" and selected_product.product_id ~= 1139 then
		purchase_product_in_cart = true
	end
	show_warning_alert( selected_movie, is_added_from_browse, product_to_check, action )
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

function show_warning_alert( selected_movie, is_added_from_browse, product_to_check, action )
	if ( kernel.get_session_value("BrowseState.MovieMatureWarningShown") == false and rated_movie_in_cart == true )
	or ( kernel.get_session_value("BrowseState.MatureGameWarningShown") == false and rated_game_in_cart == true ) then
		if product_family ~= nil and product_family.product_family_name ~= nil then
			view_analytics_event( "context", tostring(product_family.product_family_name) )
		end
		view_analytics_event( "context", "matureWarning" )
	    local render_type = kernel.getviewrendertype();
		if render_type == "WPF" then
			kernel.show_mature_warning(
				-- time out
				function()
            		view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Message view Timed out." )
				end,
				-- back button
				function()
					view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "rrated_message_view:hide()" )
				end,
				-- ok button
				function()
					if product_family.product_family_name == "Movies" then
						kernel.set_session_value("BrowseState.MovieMatureWarningShown", true)
					elseif product_family.product_family_name == "Games" then
						kernel.set_session_value("BrowseState.MatureGameWarningShown", true)
					end
					if blu_ray_movie_in_cart == true and kernel.get_session_value("BrowseState.BluRayWarningShown") == false then
						show_blu_ray_alert( selected_movie, is_added_from_browse, product_to_check, action )
					elseif subplatform_game_in_cart == true then
						show_sub_platform_warning( selected_movie, is_added_from_browse, product_to_check, action )
					elseif purchase_product_in_cart == true then
						show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
					else
						add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
					end
				end
			)
		else
			local analytics_view_name = "rrated_message_view"
			view_show_safe(analytics_view_name)
			show_warning_header()
			set_background_opacity("rrated_scene_background")
			set_timeout_for_warning_timer()
			warning_popup_timer:start()
			kernel.registeractorhitnotify(
				"warning_continue_button",
				function()
					if product_family.product_family_name == "Movies" then
						kernel.set_session_value("BrowseState.MovieMatureWarningShown", true)
					elseif product_family.product_family_name == "Games" then
						kernel.set_session_value("BrowseState.MatureGameWarningShown", true)
					end
					if blu_ray_movie_in_cart == true and kernel.get_session_value("BrowseState.BluRayWarningShown") == false then
						show_blu_ray_alert( selected_movie, is_added_from_browse, product_to_check, action )
					elseif subplatform_game_in_cart == true then
						show_sub_platform_warning( selected_movie, is_added_from_browse, product_to_check, action )
					elseif purchase_product_in_cart == true then
						show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
					else
						warning_popup_timer:stop()
						add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
					end
				end
			)
			kernel.registeractorhitnotify(
				"warning_back_button",
				function()
					warning_popup_timer:stop()
					view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, analytics_view_name .. ":hide()" )
				end
			)
		end
	elseif blu_ray_movie_in_cart == true and kernel.get_session_value("BrowseState.BluRayWarningShown") == false then
		show_blu_ray_alert( selected_movie, is_added_from_browse, product_to_check, action )
	elseif subplatform_game_in_cart == true then
		show_sub_platform_warning( selected_movie, is_added_from_browse, product_to_check, action )
	elseif purchase_product_in_cart == true then
		show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
	else
		add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
	end
end

function hide_warning_popup()
	view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Message view Timed out." )
	warning_popup_timer:stop()
end

function set_background_opacity(background_actor_name)
    -- if the prior view is another popup message then don't gray the background again because it will turn a dark shade of gray
    if (kernel.peekpriorview() ~= kernel.get_session_value("BrowseState.ItemAddedFromView")) then
        kernel.setactoropacity( background_actor_name , 0 )
    end
end

function show_blu_ray_alert( selected_movie, is_added_from_browse, product_to_check, action )
    view_analytics_event( "context", "blurayWarning" )
	kernel.setmacro( "MatureWarning:Header",strings.blu_ray_alert_title )
	kernel.setmacro( "Bluray:Message", strings.blu_ray_alert )

    local render_type = kernel.getviewrendertype();
    if render_type == "WPF" then
        kernel.show_bluray_alert(
            -- time out
            function()
            	view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Message view Timed out." )
            end,
            -- back button
		    function()
			    view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "bluray_message_view:hide()" )
		    end,
            -- ok button
		    function()
			    kernel.set_session_value("BrowseState.BluRayWarningShown", true)
			    if purchase_product_in_cart == true then
				    show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
					    -- Need to bail here or else lower part of routine will hide the puchase warning before user sees it
				    return
			    else
				    add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
			    end
		    end
        )
    else
        set_timeout_for_warning_timer()
	    warning_popup_timer:start()
	    view_show_safe( "bluray_message_view" )
	    warning_popup_timer:start()
	    show_warning_header()
        set_background_opacity("bluray_scene_background")


	    kernel.registeractorhitnotify(
		    "warning_continue_button",
		    function()
			    warning_popup_timer:stop()
			    kernel.set_session_value("BrowseState.BluRayWarningShown", true)
			    if purchase_product_in_cart == true then
				    show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
					    -- Need to bail here or else lower part of routine will hide the puchase warning before user sees it
				    return
			    else
				    add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
			    end
		    end
	    )
	    kernel.registeractorhitnotify(
		    "warning_back_button",
		    function()
			    warning_popup_timer:stop()
			    view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "bluray_message_view:hide()" )
		    end
	    )
    end
end

function show_sub_platform_warning( selected_movie, is_added_from_browse, product_to_check, action )
	view_analytics_event( "context", "subplatformWarning" )
	kernel.setmacro( "Subplatform:Header", strings.subplatform_warning_title )
	kernel.setmacro( "Subplatform:Message", string.gsub( strings.subplatform_warning, "{0}", subplatform_message ) )

    local render_type = kernel.getviewrendertype();
    if render_type == "WPF" then
        kernel.show_sub_platform_warning(
            -- time out
            function()
            	view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Message view Timed out." )
            end,
            -- back button
		    function()
			    view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "subplatform_message_view:hide()" )
		    end,
            -- ok button
		    function()
				if purchase_product_in_cart == true then
					show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
				else
					add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
				end
		    end
        )
    else
		view_show_safe( "subplatform_message_view" )
		show_warning_header()
		set_timeout_for_warning_timer()
		warning_popup_timer:start()

		kernel.registeractorhitnotify(
			"warning_continue_button",
			function()
				warning_popup_timer:stop()
				if purchase_product_in_cart == true then
					show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
				else
					add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
				end
			end
		)
		kernel.registeractorhitnotify(
			"warning_back_button",
			function()
				warning_popup_timer:stop()
				view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "subplatform_message_view:hide()" )
			end
		)
		kernel.registeractorhitnotify(
			"warning_backback_button_arrow",
			function()
				warning_popup_timer:stop()
				view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "subplatform_message_view:hide()" )
			end
		)
	end
end

function show_purchase_warning( selected_movie, is_added_from_browse, product_to_check, action )
	kernel.setmacro( "PurchaseWarning:Header",strings.products_used )
	kernel.setmacro( "PurchaseWarning:Message",strings.purchase_warning )

	view_analytics_event( "context", "purchaseWarning" )

    local render_type = kernel.getviewrendertype();
    if render_type == "WPF" then
        kernel.show_purchase_warning(
            -- time out
            function()
            	view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Message view Timed out." )
            end,
            -- back button
		    function()
			    view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "purchase_message_view:hide()" )
		    end,
            -- ok button
		    function()
				add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
		    end
        )
    else
		view_show_safe( "purchase_message_view" )
		set_timeout_for_warning_timer()
		warning_popup_timer:start()
		show_warning_header()
		set_background_opacity("purchase_scene_background")

		kernel.registeractorhitnotify(
			"warning_continue_button",
			function()
				warning_popup_timer:stop()
				add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_check )
			end
		)
		kernel.registeractorhitnotify(
			"warning_back_button",
			function()
				warning_popup_timer:stop()
				view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "purchase_message_view:hide()" )
			end
		)
	end
end

function add_item_to_shopping_cart( selected_movie, is_added_from_browse, action, product_to_add )
	if kernel.rental_shopping_cart_get_pickup_recommendations_shown( ShoppingCart.session_id ) == true then
		-- Add directly to the cart then unwind  ('add_movie' has screen specific toggling)
		ShoppingCart:add_item( selected_movie, 1, action, "Recommendation" )
		view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Item added" )
	elseif is_added_from_browse ~= true then
		ShoppingCart:add_item( selected_movie, 1, action )
		if kernel.get_session_value("BrowseState.ProductAddedFromCarousel") == false then
			view_unwindto_safe( kernel.get_session_value("BrowseState.ProductDetailShownFromView"), true, "Item added from Product Detail view" )
		else
			kernel.set_session_value("BrowseState.ProductAddedFromCarousel", false)
			kernel.executescript( "browse_products" )
		end
	else
        -- this is never called from the lua side.  Browse is done in .net now.
--        local selected_product = kernel.get_product_from_actor( selected_movie )
--        if product_to_add ~= nil then
--	        selected_product = product_to_add
--        end

--        kernel.set_session_value("BrowseState.BrowseSelectionSelectedProductLuaTable", selected_product)
--        ShoppingCart:add_item( selected_product, 1, action )
--        --refresh_current_page()

--        if kernel.current_rental_shopping_cart_is_full() and kernel.get_session_value("BrowseState.IsShoppingCartFullMessagesOpen") == false and kernel.get_session_value("BrowseState.ShoppingCartFullMessagesShown") == false then
--	        show_shopping_cart_full()
--        end

		--view_unwindto_safe( kernel.get_session_value("BrowseState.ItemAddedFromView"), true, "Item added" )
	end
end

is_dnr_disc = function( product )
	local do_not_rent_flag = false
	if product ~= nil and product.do_not_rent_date ~= nil then
		do_not_rent_flag = product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" )
	end
	return do_not_rent_flag
end

--function newtitlerollupoverlay()
--	local t = {}

--	t.scene_manager = newscenemanager()

--	t.show = function( self, x, y, width, height, modal, dvd_product, dvd_button_on_hit_function, blu_ray_product, blu_ray_button_on_hit_function, cancel_button_on_hit_function, timeout_function, purchase )
--		if x == nil or x < 0 or x > 1024
--		or y == nil or y < 0 or y > 768
--		or width == nil or width < 75 or width > 1024
--		or height == nil or height < 35 or height > 768
--		then
--			return
--		end

--		local button_width = width / 1.25
--		local button_height = height / 4
--		local button_x = x + ( ( width - button_width ) / 2 )
--		local button_y = y + ( ( height - ( button_height * 2 ) ) / 2 )

--		local cancel_button_x = x + width - 21
--		local cancel_button_y = y + height - 20

--		local function get_button_info( product, type )
--			local button_font = "font_bryant_bold_22"
--			if button_width < 200 then
--				button_font = "font_bryant_bold_13"
--			end
--			local button_text = strings.title_rollup_overlay_unavailable_text
--			local button_background_color = { r = 90, g = 90, b = 90 }
--			local button_gradient_target_color = { r = 30, g = 30, b = 30 }
--			local button_hit_options = "None"
--			local button_visible = false

--			if product ~= nil and product.product_id ~= nil and ShoppingCart:has_item( product.product_id ) == true then
--				button_text = string.gsub( strings.title_rollup_overlay_cart_text, "{0}", type )
--				button_visible = true
--			elseif product ~= nil and product.is_available == true and product.product_id ~= nil and ProductCatalog:in_stock( product.product_id ) == false then
--				if button_width < 200 then
--					button_font = "font_bryant_bold_12"
--				end
--				button_text = string.gsub( strings.title_rollup_overlay_oos_text, "{0}", type )
--				button_visible = true
--			elseif purchase == true and product ~= nil then
--				local purchase_price = product.purchase_price
--				if purchase_price ~= nil and purchase_price > 0 then
--					if kernel.getculture() == "es-MX" then
--						if button_width < 200 then
--							button_font = "font_bryant_bold_10"
--						else
--							button_font = "font_bryant_bold_18"
--						end
--					end
--					button_text = string.gsub( strings.title_rollup_overlay_buy_text, "%S%S%S", { ["{0}"] = type, ["{1}"] = strings.currency_symbol, ["{2}"] = string.format( "%.2f", purchase_price ) } )
--					button_hit_options = "Enabled"
--					button_visible = true
--					if type == "BLU-RAY" then
--						button_background_color = { r = 29, g = 155, b = 229 }
--						button_gradient_target_color = { r = 23, g = 127, b = 188 }
--					else
--						button_background_color = { r = 181, g = 18, b = 27 }
--						button_gradient_target_color = { r = 143, g = 14, b = 21 }
--					end
--				end
--			elseif product ~= nil and product.is_available == true and product.product_id ~= nil and ProductCatalog:in_stock( product.product_id ) == true then
--				if button_width < 200 then
--					button_font = "font_bryant_bold_15"
--				end
--				button_text = string.gsub( strings.title_rollup_overlay_add_text, "{0}", type )
--				button_hit_options = "Enabled"
--				button_visible = true
--				if type == "BLU-RAY" then
--					button_background_color = { r = 29, g = 155, b = 229 }
--					button_gradient_target_color = { r = 23, g = 127, b = 188 }
--				else
--					button_background_color = { r = 181, g = 18, b = 27 }
--					button_gradient_target_color = { r = 143, g = 14, b = 21 }
--				end
--			end

--			return button_font, button_text, button_background_color, button_gradient_target_color, button_hit_options, button_visible
--		end

--		if modal == true then
--			self.scene_manager:add_actor(
--				"overlay_modal_background",
--				0,
--				0,
--				1024,
--				768,
--				{
--					backgroundColor = "black",
--					opacity = 0.8,
--					hitOptions = "Enabled",
--					renderOptions = "Background"
--				}
--			)

--			self.scene_manager:add_actor(
--				"overlay_background",
--				x,
--				y,
--				width,
--				height,
--				{
--					backgroundColor = "white",
--					borderColor = { r = 151, g = 14, b = 22 },
--					strokeWeight = 4,
--					cornerSize = { width = 5, height = 5 },
--					renderOptions = "Background, DrawBorder"
--				}
--			)
--		else
--			self.scene_manager:add_actor(
--				"overlay_background",
--				x,
--				y,
--				width,
--				height,
--				{
--					backgroundColor = "black",
--					opacity = 0.8,
--					borderColor = { r = 151, g = 14, b = 22 },
--					strokeWeight = 4,
--					cornerSize = { width = 5, height = 5 },
--					hitOptions = "Enabled",
--					renderOptions = "Background, DrawBorder"
--				}
--			)	
--		end

--		local dvd_button_font, dvd_button_text, dvd_button_background_color, dvd_button_gradient_target_color, dvd_button_hit_options, dvd_button_visible = get_button_info( dvd_product, "DVD" )
--		self.scene_manager:add_actor(
--			"overlay_dvd_button",
--			button_x,
--			button_y - 15,
--			button_width,
--			button_height,
--			{
--				backgroundColor = dvd_button_background_color,
--				gradientTargetColor = dvd_button_gradient_target_color,
--				gradientAngle = 90,
--				cornerSize = { width = 90, height = 90 },
--				foregroundColor = "white",
--				text = dvd_button_text,
--				font = dvd_button_font,
--				horizontalAlignment = "center",
--				verticalAlignment = "center",
--				hitOptions = dvd_button_hit_options,
--				visible = dvd_button_visible,
--				renderOptions = "BackgroundGradient, Text"
--			},
--			function()
--				self:hide()
--				if dvd_button_on_hit_function ~= nil then
--					dvd_button_on_hit_function()
--				end
--			end
--		)

--		local blu_ray_button_font, blu_ray_button_text, blu_ray_button_background_color, blu_ray_button_gradient_target_color, blu_ray_button_hit_options, blu_ray_button_visible = get_button_info( blu_ray_product, "BLU-RAY" )
--		self.scene_manager:add_actor(
--			"overlay_blu_ray_button",
--			button_x,
--			button_y + button_height + 15,
--			button_width,
--			button_height,
--			{
--				backgroundColor = blu_ray_button_background_color,
--				gradientTargetColor = blu_ray_button_gradient_target_color,
--				gradientAngle = 90,
--				cornerSize = { width = 90, height = 90 },
--				foregroundColor = "white",
--				text = blu_ray_button_text,
--				font = blu_ray_button_font,
--				horizontalAlignment = "center",
--				verticalAlignment = "center",
--				hitOptions = blu_ray_button_hit_options,
--				visible = blu_ray_button_visible,
--				renderOptions = "BackgroundGradient, Text"
--			},
--			function()
--				self:hide()
--				if blu_ray_button_on_hit_function ~= nil then
--					blu_ray_button_on_hit_function()
--				end
--			end
--		)

--		self.scene_manager:add_actor(
--			"overlay_cancel_button",
--			cancel_button_x,
--			cancel_button_y,
--			43,
--			43,
--			{
--				backgroundColor = { r = 80, g = 80, b = 80 },
--				gradientTargetColor = { r = 0, g = 0, b = 0 },
--				gradientAngle = 90,
--				borderColor = { r = 151, g = 14, b = 22 },
--				strokeWeight = 2,
--				cornerSize = { width = 21, height = 21 },
--				foregroundColor = "white",
--				text = "X",
--				font = "font_akzidenz_bold_small",
--				horizontalAlignment = "center",
--				verticalAlignment = "center",
--				hitOptions = "Enabled",
--				renderOptions = "BackgroundGradient, DrawBorder, Text"
--			},
--			function()
--				self:hide()
--				if cancel_button_on_hit_function ~= nil then
--					cancel_button_on_hit_function()
--				end
--			end
--		)
--        if kernel.ttsenabled() == true then
--            kernel.setmacro( "Overlay:DVDButtonText", dvd_button_text )
--            kernel.setmacro( "Overlay:BlurayButtonText", blu_ray_button_text )
--            kernel.ttsrepeat()
--        end

--        local timeout = kernel.ttsgettimeout( "long", 10000 )

--		kernel.timercreate(
--			"overlay_scene_manager_timer",
--			timeout,
--			timeout,
--			function()
--				self:hide()
--				if timeout_function ~= nil then
--					timeout_function()
--				end
--			end
--		)
--		kernel.timerstart( "overlay_scene_manager_timer" )
--	end

--	t.hide = function( self )
--		kernel.timerstop( "overlay_scene_manager_timer" )
--		kernel.timerremove( "overlay_scene_manager_timer" )
--		self.scene_manager:clear()
--	end

--	return t
--end

--function get_product_ui_info ( product )
--	local sell_through_enabled = StoreManager:is_sell_thru_enabled()
--	local current_date_string = get_current_date_string( "%4d%02d%02d%02d%02d" )
--	local product_type = nil
--	local product_family = nil
--	if (product ~= nil) then
--		product_type = ProductTypes[product.product_type_id]
--		product_family = ProductFamilies[product_type.product_family_id]
--	end

--	local product_info_result = {}
--    product_info_result["product"] = product
--    product_info_result["product_is_case"] = product ~= nil and product.product_id == 1139
--    product_info_result["available"] = product ~= nil and product.is_available
--    product_info_result["available_and_instock"] = product_info_result["available"] and ProductCatalog:in_stock( product.product_id )
--    product_info_result["already_in_cart"] = product ~= nil and ShoppingCart:count_for_item( product.product_id ) > 0

--    -- purchase related properties
--    local is_purchasable_value = false
--    local show_buy_button_value = false
--    local show_in_purchase_overlay_value = false
--    if product ~= nil then
--        product_info_result["purchase_price"] = product.purchase_price
--	    if product_info_result["product_is_case"] then
--			is_purchasable_value = product_info_result["available_and_instock"]
--	    elseif product_family.product_family_name == "Movies" then
--		    is_purchasable_value = sell_through_enabled and product.is_sellable
--		    show_buy_button_value = is_purchasable_value and product_info_result["available_and_instock"] and not product_info_result["already_in_cart"]
--		    show_in_purchase_overlay_value = product_info_result["available"] and is_purchasable_value
--		elseif product_family.product_family_name == "Games" then
--			is_purchasable_value = product_info_result["available_and_instock"] and product.is_sellable == true 
--		end
--	end

--	product_info_result["is_purchasable"] = is_purchasable_value
--	product_info_result["show_buy_button"] = show_buy_button_value
--	product_info_result["show_in_purchase_overlay"] = show_in_purchase_overlay_value

--	return product_info_result
--end

--show_collection_items = function(collection)
--	browse_collection = collection
--	if (collection ~= nil) then
--		browse_collection_products = collection.incart_available_kiosk_products
--	end
--	collection_set_number = 1
--    kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--	view_show( "browse_view_collection" )
--end

--remove_product_on_hit = function(product_to_remove)
--	if title_rollup_overlay ~= nil then
--		title_rollup_overlay:hide()
--	end
--	view_analytics_event( "buttonPress", "shopping-cart-remove-item" )
--	remove_movie(product_to_remove)
--end

--should_show_no_thanks_browse_button = function()
---- this was ported to .net and shouldn't be called from lua but is here so the app doesn't break
--return false

----return  kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true
----	and kernel.rental_shopping_cart_get_in_cart_title_marketing( ShoppingCart.session_id ) == true
----	and CartHasAnyPostOfferItems( ShoppingCart ) == false
--end

--on_checkout_button_click = function ()
--	kernel.executescript("view_shopping_cart")
--end

--draw_action_bar = function() 
--	kernel.draw_action_bar(
--		on_checkout_button_click,
--		should_show_no_thanks_browse_button())
--end

--draw_action_bar_and_mini_cart = function (browseSetName)
--	draw_action_bar()
--	kernel.draw_mini_cart(
--		browseSetName,
--        kernel.rental_shopping_cart_get_in_cart_title_marketing( ShoppingCart.session_id ),
--		remove_product_on_hit,
--		browseSetName ~= "Collections")
--end

--draw_ada_control = function ()
--	kernel.draw_accessibility_control(
--		"ada",
--		browse_filtered_products,
--		add_movie_on_hit,
--		switch_product_family_tab_function,
--        kernel.get_session_value("BrowseState.BrowseTitleFamily")
--		)
--end

--draw_top_menu = function()
----	kernel.draw_top_menu(
----		kernel.peekview(),
----        kernel.get_session_value("BrowseState.BrowseGenre"),
----        kernel.get_session_value("BrowseState.BrowseTitleFamily"),
----		false,
----		kernel.rental_is_collections_enabled(), 
----		change_genre_filter,
----		change_product_family,
----		click_collections_menu)
--end

--change_genre_filter = function(new_genre_filter_value, new_genre_filter_text)
----	if title_rollup_overlay ~= nil then
----		title_rollup_overlay:hide()
----	end
----	kernel.set_session_value("BrowseState.BrowseGenre", new_genre_filter_value)
----	view_analytics_event( "filterChange", "browse-filter-genre," .. new_genre_filter_text )
----    kernel.set_session_value("BrowseState.PageNumber", 1)
----	if (kernel.peekview() == "browse_view_collections") then
----		view_goback_safe()
----		if (kernel.peekview() ~= "browse_view" and kernel.peekview() ~= "browse_filter_alpha_view")  then
----			view_show_safe("browse_view")
----		end
----	else
----		show_browse_view()
----		draw_top_menu()
----	end
----	if kernel.peekview() == "browse_filter_alpha_view" then
----	end
--end

--click_collections_menu = function ()
--	view_analytics_event( "filterChange", "browse-filter-genre,COLLECTIONS" )
--	if (kernel.peekview() ~= "browse_view" and kernel.peekview() ~= "browse_filter_alpha_view") then
--		view_goback_safe()
--	end
--	show_browse_view_collections()
--end

--show_browse_view_collections = function()
--	if title_rollup_overlay ~= nil then
--		title_rollup_overlay:hide()
--	end
--	collections_set_number = 1
--    kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--	view_show("browse_view_collections")
--end

--function switch_product_family_tab_function()
--	if kernel.get_session_value("BrowseState.BrowseTitleFamily") == "Movies" then
--		change_product_family("Games")
--	else
--		change_product_family("Movies")
--	end
--end

--change_product_family = function (new_product_family) 
--	if title_rollup_overlay ~= nil then
--		title_rollup_overlay:hide()
--	end
--    kernel.set_session_value("BrowseState.BrowseTitleFamily", new_product_family)
--	if kernel.get_session_value("BrowseState.BrowseTitleFamily") == "Games" then
--		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_games )
--        kernel.set_session_value("BrowseState.BrowseGenre", nil)
--        if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Blu-ray" or  kernel.get_session_value("BrowseState.BrowseSelectedSort") == "alpha" then
--            kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--            kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--            kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
--        elseif kernel.get_session_value("BrowseState.BrowseFilterMode") =="ComingSoon" then
--            kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--            kernel.set_session_value("BrowseState.BrowseSelectedSort", "coming-soon")
--            kernel.set_session_value("BrowseState.BrowseFilterMode", "ComingSoon")
--        elseif kernel.get_session_value("BrowseState.BrowseFilterMode") =="ForSale" then
--			if kernel.get_can_sell_used_games() == true then
--                kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--                kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
--                kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")
--			else
--                kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--                kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
--			end
--        else
--            kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--            kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--            kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
--		end
--	else
--		kernel.setmacro( "Browse:ProductFamily", strings.browse_family_movies )
--        kernel.set_session_value("BrowseState.BrowseGenre", nil)
--        if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Xbox 360" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Xbox One" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS2" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS3" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "PS4" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Wii" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "NintendoDS" or kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Wii U" then
--            kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--            kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--		end
--        if kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" then
--			if kernel.get_can_sell_used_movies() == true  then
--                kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--                kernel.set_session_value("BrowseState.BrowseSelectedSort", "for-sale")
--                kernel.set_session_value("BrowseState.BrowseFilterMode", "ForSale")
--			else
--                kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--                kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
--                kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
--			end
--		end
--	end
--	view_analytics_event( "filterChange", "browse-filter," .. kernel.get_session_value("BrowseState.BrowseTitleFamily") )
--    if kernel.get_session_value("BrowseState.BrowseSelectedSort") == "Newest" then
--		kernel.set_session_value("BrowseState.BrowseTitleType", "All")
--	end

--    kernel.set_session_value("BrowseState.PageNumber", 1)
--	while (kernel.peekview() == "browse_view_collections" or kernel.peekview() == "browse_view_collection" or kernel.peekview() == "browse_filter_alpha_view") do
--		view_goback_safe()
--	end
--	if (kernel.peekview() == "browse_view_ada_accessible") then
--		show_browse_view()
--	elseif (kernel.peekview() == "browse_view") then
--		show_browse_view()
--		draw_top_menu()
--	else
--		view_show("browse_view")
--	end
--end


