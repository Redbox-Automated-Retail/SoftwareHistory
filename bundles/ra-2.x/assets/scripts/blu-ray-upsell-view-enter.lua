--
-- blu-ray-upsell-view-enter.lua
-- Copyright (C) 2017 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.suspend_idle_timer( "idle_timer" )

blu_ray_upsell_view_timer = newidletimer( "blu_ray_upsell_view_timer" )
blu_ray_upsell_view_timer:initialize(
	15000,
	function()
        kernel.current_rental_shopping_cart_reject_undecided_bluray_upsell_items()
		kernel.current_rental_shopping_cart_bluray_upsell_set_do_not_show_again( true) 
		view_show_safe( "shopping_cart_view" )
	end
)
blu_ray_upsell_view_timer:start()

kernel.current_rental_shopping_cart_bluray_upsell_add_missing_upsell_items()

local blu_ray_upsell_scene_manager = newscenemanager()
local x = 517 - ( kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( true ) * 74 )

local blurayUpsellItemCount = kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( false )

for k=1,blurayUpsellItemCount do
    local v = kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index( k )
    if v ~= nil then
	    if v.offer == nil then
		    local catalog_entry = MasterProductCatalog[v.blu_ray_product_id]
		
		    local image_name = "image_movie_missing_box_art"
		    if kernel.dataserviceregisterimage( catalog_entry.image_file, catalog_entry.image_file ) == true then
			    image_name = string.format( "cache:%s", catalog_entry.image_file )
		    end
		    blu_ray_upsell_scene_manager:add_actor(
			    "boxart_" .. k,
			    x,
			    236,
			    121,
			    198,
			    {
				    image = image_name,
				    renderOptions = "Image"
			    }
		    )
		
		    blu_ray_upsell_scene_manager:add_actor(
			    "banner_" .. k,
			    x,
			    408,
			    121,
			    29,
			    {
				    image = "image_banner_blu_ray",
				    text = "Blu-ray",
				    font = "font_target_small_medium",
				    forgroundColor = "white",
				    horizontalAlignment = "center",
				    verticalAlignment = "center",
				    renderOptions = "Image, Text"
			    }
		    )
		
		    blu_ray_upsell_scene_manager:add_actor(
			    "price_" .. k,
			    x,
			    448,
			    121,
			    29,
			    {
				    text = string.format( "$%.2f", StoreManager.PricingManager:get_product_price( v.blu_ray_product_id ).initial_night ),
				    font = "font_target_11_italic",
				    foregroundColor = "black",
				    horizontalAlignment = "center",
				    renderOptions = "Text"
			    }
		    )
		
		    blu_ray_upsell_scene_manager:add_actor(
			    "checkbox_" .. k,
			    x + 38,
			    480,
			    45,
			    45,
			    {
				    image = "image_unchecked_box",
				    tag = "unchecked",
				    hitOptions = "Enabled",
				    renderOptions = "Image"
			    }
		    )
		    kernel.registeractorhitnotify(
			    "checkbox_" .. k,
			    function()
				    if kernel.getactortag( "checkbox_" .. k ) == "unchecked" then
					    kernel.setactorimage( "checkbox_" .. k, "image_checked_box" )
					    kernel.setactortag( "checkbox_" .. k, "checked" )
				    elseif kernel.getactortag( "checkbox_" .. k ) == "checked" then
					    kernel.setactorimage( "checkbox_" .. k, "image_unchecked_box" )
					    kernel.setactortag( "checkbox_" .. k, "unchecked" )
				    end
			    end
		    )
		
		    x = x + 149
	    end
    end
end

if kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( true ) == 1 then
    local blurayUpsellItemCount = kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( false )
    for k=1,blurayUpsellItemCount do
        local v = kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index( k )
        if v ~= nil and v.offer == nil then
			kernel.hideactor( "checkbox_" .. k )
			kernel.setactortag( "checkbox_" .. k, "checked" )
	    end
    end
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_analytics_event( "buttonPress", "blu_ray_upsell_back_button" )
        kernel.current_rental_shopping_cart_remove_undecided_bluray_upsell_items()
		view_goback_safe( "blu_ray_upsell_view", false )
	end
)

kernel.registeractorhitnotify(
	"no_thanks_button",
	function()
		view_analytics_event( "buttonPress", "blu_ray_upsell_no_thanks_button" )
        kernel.current_rental_shopping_cart_reject_undecided_bluray_upsell_items()
		kernel.current_rental_shopping_cart_bluray_upsell_set_do_not_show_again( true ) 
		view_show_safe( "shopping_cart_view" )
	end
)

kernel.registeractorhitnotify(
	"upgrade_button",
	function()
		local function any_checked()
            local blurayUpsellItemCount = kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( false )
            for k=1,blurayUpsellItemCount do
                local v = kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index( k )
                if v ~= nil and v.offer == nil then
					if kernel.getactortag( "checkbox_" .. k ) == "checked" then
						return true
					end
				end
			end
			return false
		end
		
		if any_checked() == true then
			view_analytics_event( "buttonPress", "blu_ray_upsell_upgrade_button" )
            local blurayUpsellItemCount = kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( false )
            for k=1,blurayUpsellItemCount do
                local v = kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index( k )
                if v ~= nil and v.offer == nil then
					if kernel.getactortag( "checkbox_" .. k ) == "checked" then
                        kernel.current_rental_shopping_cart_bluray_upsell_set_offer_response(v.dvd_product_id, "accepted")
					else
                        kernel.current_rental_shopping_cart_bluray_upsell_set_offer_response(v.dvd_product_id, "rejected")
					end
				end
			end
			view_show_safe( "shopping_cart_view" )
		else
			local message_view_manager = newmessageviewmanager()
			
			local go_back = function()
				message_view_manager:hide( false )
			end
			
			message_view_manager:show()
			message_view_manager:set_message_header( strings.blu_ray_upsell_unselected_header )
			message_view_manager:set_message( strings.blu_ray_upsell_unselected_message )
			message_view_manager:hide_first_button()
			message_view_manager:show_second_button( strings.okay_button, go_back )
			message_view_manager:start_timer( go_back )
			
			view_analytics_event( "context", "selectMovieToUpgrade" )
		end
	end
)