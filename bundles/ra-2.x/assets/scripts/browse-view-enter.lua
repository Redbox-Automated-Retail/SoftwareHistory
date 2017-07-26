--
-- browse-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- This screen uses the actual Start Over script instead
CloseAnalyticsOnStartView = false

browse_collection = nil

local title_rollup_overlay = newtitlerollupoverlay()

-- Add movie Plus signs
function add_movie_on_hit( actor_name )
	if title_rollup_overlay ~= nil then
		title_rollup_overlay:hide()
	end

	local function product_on_hit( product )
		selected_movie = actor_name
        local action = "rental"
        if kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and (kernel.peekview() == "browse_view" or kernel.peekview() == "browse_view_ada_accessible" or kernel.peekview() == "ada_purchase_message_view") then
            action = "purchase"
		end

        if ( ada_item_added_from_view ~= nil ) then
          kernel.set_session_value("BrowseState.ItemAddedFromView", ada_item_added_from_view)
          ada_item_added_from_view = nil
        else 
		    kernel.set_session_value("BrowseState.ItemAddedFromView", kernel.peekview())
        end

		add_item_to_cart( product, action )
	end
	
	local product_from_actor = get_product_from_actor( actor_name )
    local is_ada_purchase = kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and kernel.peekview() == "browse_view_ada_accessible" 
	if product_from_actor ~= nil 
        and product_from_actor.product_id ~= 1139 
        and product_from_actor.is_movie == true 
        and (product_from_actor.kiosk_product_type == 3 
            or ( is_ada_purchase ))
        then
		
        view_analytics_event( "buttonPress", "shopping-cart-modal-add-item" )
		
        local is_purchase = kernel.get_session_value("BrowseState.BrowseFilterMode") == "ForSale" and (kernel.peekview() == "browse_view" or kernel.peekview() == "browse_view_ada_accessible")
		
		local dvd_product = nil
		local blu_ray_product = nil

		if product_from_actor.dvd_index ~= nil and product_from_actor.dvd_index > 0 then
			dvd_product = product_from_actor.kiosk_products[product_from_actor.dvd_index]
        elseif product_from_actor.product_type_id == 1 then
            dvd_product = product_from_actor
		end
		if product_from_actor.blu_ray_index ~= nil and product_from_actor.blu_ray_index > 0 then
			blu_ray_product = product_from_actor.kiosk_products[product_from_actor.blu_ray_index]
        elseif product_from_actor.product_type_id == 2 then
            blu_ray_product = product_from_actor
		end
		
		local dvd_product_ui_info = get_product_ui_info(dvd_product)
		local blu_ray_product_ui_info = get_product_ui_info(blu_ray_product)

		-- if we are in "for sale" mode and the product is not purchasable then pass nil to the overlay
		local overlay_dvd_product = dvd_product
		if is_purchase and not dvd_product_ui_info.show_in_purchase_overlay then
			overlay_dvd_product = nil
		end

		local overlay_blu_ray_product = blu_ray_product
		if is_purchase and not blu_ray_product_ui_info.show_in_purchase_overlay then
			overlay_blu_ray_product = nil
		end


        if ( is_ada_purchase ) then
            -- use ada purchase message to show product price
            set_timeout_for_warning_timer()
	        warning_popup_timer:start()
            ada_item_added_from_view = kernel.peekview()
	        view_show_safe( "ada_purchase_message_view" )

	        warning_popup_timer:start()
	        view_analytics_event( "context", "blurayWarning" )
            -- set view text
            local product = overlay_dvd_product or overlay_blu_ray_product
            local type_of_product = strings.dvd
            if ( overlay_blu_ray_product ~= nil ) then
                type_of_product = strings.blu_ray
            end
            type_of_product = type_of_product:upper()
			local purchase_price = product.purchase_price
            local header_text = nil
            local abe_message = nil
			if purchase_price ~= nil and purchase_price > 0 then
				header_text = string.gsub( strings.ada_purchase_price_message, "%S%S%S", { ["{0}"] = type_of_product, ["{1}"] = strings.currency_symbol, ["{2}"] = string.format( "%.2f", purchase_price ) } )
				abe_message = string.gsub( strings.abe_purchase_price_message, "%S%S%S", { ["{0}"] = type_of_product, ["{1}"] = strings.currency_symbol, ["{2}"] = string.format( "%.2f", purchase_price ) } )
            end
	        kernel.setactortext( "ada_purchase_header_label", header_text )
	        kernel.setactortext( "ada_purchase_back_button", strings.back_button )
	        kernel.setactortext( "ada_purchase_ok_button", strings.okay_button )
            kernel.setmacro( "ABEPurchaseMessage:Header", nil )
            if kernel.ttsenabled() == true then
	            kernel.setmacro( "ABEPurchaseMessage:Header", abe_message )
                kernel.ttsrepeat()
            end

	        kernel.registeractorhitnotify(
		        "ada_purchase_ok_button",
		        function()
			        warning_popup_timer:stop()
                    product_on_hit( product )
		        end
	        )
	        kernel.registeractorhitnotify(
		        "ada_purchase_back_button",
		        function()
			        warning_popup_timer:stop()
                    kernel.setactoropacity( "ada_purchase_scene_background" , 0 )
			        view_unwindto_safe( ada_item_added_from_view, true, "ada_purchase_message_view:hide()" )
		        end
	        )
        else
		    title_rollup_overlay:show(
			    kernel.getactorx( actor_name ),
			    kernel.getactory( actor_name ),
			    kernel.getactorwidth( actor_name ),
			    kernel.getactorheight( actor_name ),
			    false,
			    overlay_dvd_product,
			    function()
				    if is_purchase == true and dvd_product ~= nil and dvd_product.product_id ~= nil then
					    view_analytics_event( "buttonPress", "buy," .. tostring( dvd_product.product_id ) )
				    else
					    if browse_collection ~= nil then
						    view_analytics_event( "buttonPress", "shopping-cart-add-item," .. browse_collection.product_name )
					    else
						    view_analytics_event( "buttonPress", "shopping-cart-add-item" )
					    end
				    end
				    product_on_hit( dvd_product )
			    end,
			    overlay_blu_ray_product,
			    function()
				    if is_purchase == true and blu_ray_product ~= nil and blu_ray_product.product_id ~= nil then
					    view_analytics_event( "buttonPress", "buy," .. tostring( blu_ray_product.product_id ) )
				    else
					    if browse_collection ~= nil then
						    view_analytics_event( "buttonPress", "shopping-cart-add-item," .. browse_collection.product_name )
					    else
						    view_analytics_event( "buttonPress", "shopping-cart-add-item" )
					    end
				    end
				    product_on_hit( blu_ray_product )
			    end,
			    function()
				    if is_purchase == true then
					    view_analytics_event( "buttonPress", "buy-cancel-add-item" )
				    else
					    view_analytics_event( "buttonPress", "shopping-cart-cancel-add-item" )
				    end
			    end,
			    nil,
			    is_purchase
		    )
        end
	elseif product_from_actor ~= nil then
		view_analytics_event( "buttonPress", "shopping-cart-add-item" )
		product_on_hit( product_from_actor )
	end
end



show_browse_view()

idle_timer:start()

priced_item_popup_timer = newidletimer("priced_item_popup_timer")
priced_item_popup_timer:initialize(
	40,
	function()
		priced_item_popup_timer:stop()
		price_set_item = false
		local message_view_manager = newmessageviewmanager()
		message_view_manager:hide( true )

		if selected_movie ~= nil and StoreManager.PricingManager:is_early_released( selected_movie ) then
			message_view_manager:show()
			view_analytics_event( "context", "$3 Firsts warning message" )
			message_view_manager:hide_title()

			-- The message needs to include the prices from the actual price file (and MD adjustment if needed)
			local product_pricing = StoreManager.PricingManager:get_product_price( selected_movie.product_id )
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
					ShoppingCart:add_item( selected_movie, 1, "rental" )
				end )
			message_view_manager:show_back_button_arrow(
				function()
					message_view_manager:hide_message()
				end )
			idle_timer:start()
		end
	end )

if price_set_item then
	price_set_item = false
	priced_item_popup_timer:start()
end

if #ShoppingCart.items == 5
and kernel.get_session_value("BrowseState.IsShoppingCartFullMessagesOpen") == false
and kernel.get_session_value("BrowseState.ShoppingCartFullMessagesShown") == false then
	show_shopping_cart_full()
end

kernel.draw_browse_filter_bar(
    kernel.peekview(),
    function() 
        kernel.executescript( "start_over" )
    end,
    show_browse_view
)

if (kernel.peekview() == "browse_view") then
	draw_action_bar_and_mini_cart(kernel.peekview())
	draw_top_menu()
elseif (kernel.peekview() == "browse_view_ada_accessible") then
	draw_action_bar()
	draw_ada_control()
end
