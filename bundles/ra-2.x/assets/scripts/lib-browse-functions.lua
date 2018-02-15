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

ProductCatalog.get_default_family = function( self )
	local many_families, default_family = self:has_many_families()
	if many_families == false then
        kernel.set_session_value("BrowseState.BrowseTitleFamily", default_family or kernel.get_session_value("BrowseState.BrowseTitleFamily"))
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
