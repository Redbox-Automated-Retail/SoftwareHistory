--
-- blu-ray-upsell-view-enter.lua
-- Copyright (C) 2014 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

blu_ray_upsell_view_timer = newidletimer( "blu_ray_upsell_view_timer" )
blu_ray_upsell_view_timer:initialize(
	15000,
	function()
		for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
			if v.offer == nil then
				v.offer = "rejected"
			end
		end
		ShoppingCart.blu_ray_upsell.do_not_show_again = true
		view_show_safe( "shopping_cart_view" )
	end
)
blu_ray_upsell_view_timer:start()

ShoppingCart.blu_ray_upsell:has_in_stock_and_available_blu_rays( true )

local function get_items_count( items )
	local count = 0
	for k, v in ipairs( items ) do
		if v.offer == nil then
			count = count + 1
		end
	end
	return count
end

local blu_ray_upsell_scene_manager = newscenemanager()
local x = 517 - ( get_items_count( ShoppingCart.blu_ray_upsell.items ) * 74 )

for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
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
				text = string.format( "$%.2f", StoreManager.PricingManager:get_product_price( v.blu_ray_product_id ).initial_night ) .. " " .. strings.a_day,
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

if get_items_count( ShoppingCart.blu_ray_upsell.items ) == 1 then
	for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
		if v.offer == nil then
			kernel.hideactor( "checkbox_" .. k )
			kernel.setactortag( "checkbox_" .. k, "checked" )
		end
	end
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		local function get_index( dvd_product_id )
			for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
				if v.dvd_product_id == dvd_product_id then
					return k
				end
			end
			return nil
		end
		
		view_analytics_event( "buttonPress", "blu_ray_upsell_back_button" )
		
		local dvd_product_ids_to_remove = {}
		for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
			if v.offer == nil then
				table.insert( dvd_product_ids_to_remove, v.dvd_product_id )
			end
		end
		for k, v in ipairs( dvd_product_ids_to_remove ) do
			local index_to_remove = get_index( v )
			if index_to_remove ~= nil then
				table.remove( ShoppingCart.blu_ray_upsell.items, index_to_remove )
			end
		end
		
		view_goback_safe( "blu_ray_upsell_view", false )
	end
)

kernel.registeractorhitnotify(
	"no_thanks_button",
	function()
		view_analytics_event( "buttonPress", "blu_ray_upsell_no_thanks_button" )
		for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
			if v.offer == nil then
				v.offer = "rejected"
			end
		end
		ShoppingCart.blu_ray_upsell.do_not_show_again = true
		view_show_safe( "shopping_cart_view" )
	end
)

kernel.registeractorhitnotify(
	"upgrade_button",
	function()
		local function any_checked()
			for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
				if v.offer == nil then
					if kernel.getactortag( "checkbox_" .. k ) == "checked" then
						return true
					end
				end
			end
			return false
		end
		
		local function get_shopping_cart_index_by_dvd_product_id( dvd_product_id )
			for k, v in ipairs( ShoppingCart.items ) do
				if v.product_id == dvd_product_id then
					return k
				end
			end
			return 0
		end
		
		if any_checked() == true then
			view_analytics_event( "buttonPress", "blu_ray_upsell_upgrade_button" )
			for k, v in ipairs( ShoppingCart.blu_ray_upsell.items ) do
				if v.offer == nil then
					if kernel.getactortag( "checkbox_" .. k ) == "checked" then
						v.offer = "accepted"
						ShoppingCart:remove_item( get_shopping_cart_index_by_dvd_product_id( v.dvd_product_id ), false )
						ShoppingCart:add_item( MasterProductCatalog[v.blu_ray_product_id], 1, "rental", "BlurayUpsell" )
					else
						v.offer = "rejected"
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