--
-- coming-soon-vend-screen.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local vend_screen_thank_you_actor = "vend-screen-thank-you-actor"
local vend_screen_wait_actor = "vend-screen-wait-actor"
local vend_screen_box_actor = "vend-screen-box-actor"
local vend_screen_box_black_background = "vend-screen-box-black-background"
local vend_screen_box_gray_background = "vend-screen-box-gray-background"
local vend_screen_box_title_actor = "vend-screen-box-title-actor"
local vend_screen_tile1_actor = "vend-screen-tile1-actor"
local vend_screen_tile1_text_actor = "vend-screen-tile1-text-actor"
local vend_screen_tile2_actor = "vend-screen-tile2-actor"
local vend_screen_tile2_text_actor = "vend-screen-tile2-text-actor"
local vend_screen_tile3_actor = "vend-screen-tile3-actor"
local vend_screen_tile3_text_actor = "vend-screen-tile2-text-actor"
local vend_screen_tile1_image = "vend-screen-tile1-image"
local vend_screen_tile2_image = "vend-screen-tile2-image"
local vend_screen_tile3_image = "vend-screen-tile3-image"
local vend_screen_box_image_actor = "vend-screen-box-image-actor"
local vend_screen_rental_coming_soon_actor="vend-screen-rental-coming-soon-actor"

local vend_screen_tile_actor_mask = "vend-screen-tile%d-actor"
local vend_screen_tile_text_actor_mask = "vend-screen-tile%d-text-actor"
local vend_screen_tile_image_mask = "vend-screen-tile%d-image"

local thumbnail_width = 170
local thumbnail_height = 142

local calculate_starting_points = function( dimensions, width, height )
	local x = dimensions.x + math.modf((dimensions.width - width) / 2)
	local y = dimensions.y + math.modf((dimensions.height - height) / 2)
	return x, y, x, y + height
end

local get_box_dimensions = function( sections )
	kernel.log( "get_box_dimensions - sections: " .. sections )
	local x = kernel.getactorx( vend_screen_box_actor )
	local y = kernel.getactory( vend_screen_box_actor )
	local width = kernel.getactorwidth( vend_screen_box_actor )
	local height = kernel.getactorheight( vend_screen_box_actor )
	local section = math.modf(width / sections)
	kernel.log( "get_box_dimensions - section: " .. section )
	local mask = string.format( "{ x = %%d, y = %d, width = %d, height = %d }", y, section, height )
	kernel.log( "get_box_dimensions - mask: " .. mask )

	local split = false
	local stub = " return "

	for i = 1, sections do
		if split then stub = stub .. ", " end
		stub = stub .. string.format( mask, x, section )
		split = true
		x = x + section
	end
	kernel.log( "get_box_dimensions - stub: " .. stub )
	local functor = loadstring( stub )
	return functor()
end

local draw_one_tile = function( index, product, dimensions )
	if product.image_file == nil then
		error("Product image file cannot be nil!" )
	end
	if product.release_date == nil then
		error("Product release date cannot be nil!" )
	end
	if dimensions == nil then dimensions = get_box_dimensions( 1 ) end
	kernel.log( "client dimensions: " .. dimensions.x .. ", " .. dimensions.y .. ", " .. dimensions.width .. ", " .. dimensions.height )
	local x,y, tx, ty = calculate_starting_points(
		dimensions,
		thumbnail_width,
		thumbnail_height )
	kernel.log( "starting points: " .. x .. ", " .. y .. ", " .. tx .. ", " .. ty )

	-- The image must be registered in cache before it can be used from cache
	local source_image_name = "missing_box_art"
	local thumbnail_name = "missing_box_art-thumbnail"
	local image_registered = kernel.registerimagefromcache(
		product.image_file,
		product.image_file )
	if image_registered then
		source_image_name = string.format( "cache:%s", product.image_file )
		thumbnail_name = string.format( "%s-thumbnail", product.image_file )
	end
	kernel.createthumbnail(
		source_image_name,
		thumbnail_name,
		thumbnail_width,
		thumbnail_height )

	local tile_actor_name = string.format( vend_screen_tile_actor_mask, index )
	coming_soon_scene_manager:add_actor(
		tile_actor_name,
		x,
		y,
		thumbnail_width,
		thumbnail_height,
		{	image = source_image_name,
			renderOptions = "Image, DisableAntiAlias"
		} )
	
	local release_date = string.format(
		"%s/%s/%s",
		string.sub(product.release_date, 5, 6 ),
		string.sub(product.release_date, 7, 8 ),
		string.sub(product.release_date, 1, 4 ))
			
	coming_soon_scene_manager:add_actor(
		string.format( vend_screen_tile_text_actor_mask, index ),
		tx-20,
		ty,
		thumbnail_width+40,
		60,
		{   text = release_date,
			font = "font_message",
			renderOptions = "Text",
			foregroundColor = "black",
			horizontalAlignment = "center",
			verticalAlignment = "center" 
		} )
		
end

local draw_two_tiles = function( product1, product2 )
	local left, right = get_box_dimensions( 2 )
	draw_one_tile( 1, product1, left )
 	draw_one_tile( 2, product2, right )
end

local draw_three_tiles = function( product1, product2, product3 )
	local left, center, right = get_box_dimensions( 3 )
	draw_one_tile( 1, product1, left )
	draw_one_tile( 2, product2, center )
	draw_one_tile( 3, product3, right )
end

local create_titles_strip = function( products )
	local count = #products
	if count == 1 then
		draw_one_tile( 1, products[1] )
	elseif count == 2 then
		draw_two_tiles( products[1], products[2] )
	elseif count == 3 then
		draw_three_tiles( products[1], products[2], products[3] )
	else
		error(
			"Got strip count of " ..
			tostring(count) ..
			", can only handle values of 1, 2, or 3" )
	end

end

local function trim_spaces(s)
  local i1,i2 = string.find(s,'^%s*')
  if i2 >= i1 then s = string.sub(s,i2+1) end
  local i1,i2 = string.find(s,'%s*$')
  if i2 >= i1 then s = string.sub(s,1,i1-1) end
  return s
end


function draw_coming_soon_products(products)
	if #products == 1 then
		x = 427
	elseif #products == 2 then
		x = 322
	elseif #products == 3 then
		x = 217				
	end
	
	local coming_soon_productids = ""

	for k,v in ipairs(products) do
		
		local catalog_entry = products[k]					
		local imageName = "image_name_" .. tostring(k)
		coming_soon_productids = coming_soon_productids ..products[k].product_id ..","
		
		coming_soon_scene_manager:add_actor(
			imageName,
			x,
			248,
			167,
			243,
			{ renderOptions = "Image, DrawBorder", borderColor = { 165, 165, 165 }, strokeWeight = 2 } )
			
		draw_image(catalog_entry, imageName)
		
		local product_title = trim_spaces( catalog_entry.long_name )
		
		local title_imageName = "title_" .. tostring(k)
		
		coming_soon_scene_manager:add_actor(
			title_imageName,
			x + 10,
			495,
			150,
			70,
			{ 
				foregroundColor="black",
				font="font_akzidenz_bold_tiny",
				horizontalAlignment="center",
				verticalAlignment="near",
				renderOptions = "Text"
				} )
				
		kernel.setactortext(title_imageName, product_title )	
		
		local size = kernel.measureactortext( title_imageName )
		local height = 0
		if size ~= nil then
			height = math.ceil( size.height )
		end
				
		local title_release_date = "title_release_date" .. tostring(k)
			coming_soon_scene_manager:add_actor(
				title_release_date,
				x + 10,
				500 + height,
				150,
				20,
				{ 
					foregroundColor="black",
					font="font_smaller_italic",
					horizontalAlignment="center",
					verticalAlignment="near",
					renderOptions = "Text"
					} )
					
			local release_date = string.format(
			"%s/%s/%s",
			string.sub(catalog_entry.release_date, 5, 6 ),
			string.sub(catalog_entry.release_date, 7, 8 ),
			string.sub(catalog_entry.release_date, 1, 4 ))	
			
			kernel.setactortext(title_release_date,release_date)
				
			x= x + 210					
	end
	coming_soon_productids = string.sub(coming_soon_productids, 1, -2)
	kernel.setmacro( "ComingSoonTitleIDs", coming_soon_productids )
end

function show_third_party_vend_screen(third_party_product)

		if coming_soon_scene_manager == nil then
				coming_soon_scene_manager = newscenemanager()
		end
		
		coming_soon_scene_manager:clear()
			
		coming_soon_scene_manager:add_actor(
			"background_scene",
			0,
			0,
			1024,
			768,
			{	
			backgroundColor="Black",
			renderOptions = "Background"			
			} )
		coming_soon_scene_manager:add_actor(
				vend_screen_box_black_background,
				0,
				0,
				1024,
				475,
				{	renderOptions = "Background",
					backgroundColor="26,22,23"				
				} )
				
		coming_soon_scene_manager:add_actor(
				vend_screen_box_gray_background,
				0,
				475,
				1024,
				293,
				{	renderOptions = "Background",
					backgroundColor="Gray"
				} )
		coming_soon_scene_manager:add_actor(
			vend_screen_box_image_actor,
			122,
			31,
			791,
			462,
			{
				renderOptions = "Image"
			} )
			
		if third_party_product ~= nil then
			local catalog_entry = third_party_product[1]
			draw_image(catalog_entry, vend_screen_box_image_actor)
		end
			
		coming_soon_scene_manager:add_actor(
			vend_screen_rental_coming_soon_actor,
			0,
			630,
			1024,
			100,
			{	text = strings.rentals_coming_soon,
				horizontalAlignment = "center",
				foregroundColor = "black",
				font = "font_target_large",
				renderOptions = "Text"
			} )	
end


function show_coming_soon_products( coming_soon_products )
	if coming_soon_products == nil then
		local message = "show_coming_soon_vend_screen - coming_soon_products parameter cannot be nil!"
		kernel.log( message )
		error( message )
	end
	if coming_soon_scene_manager == nil then
		coming_soon_scene_manager = newscenemanager()
	end
	
	coming_soon_scene_manager:clear()
	
	coming_soon_scene_manager:add_actor(
		"background_scene",
		0,
		0,
		1024,
		768,
		{
		image="image_cart_background",
		renderOptions = "Image"			
		} )
	local coming_soon_font = "font_target_largest"
	if kernel.getculture() == "es-MX" then
		coming_soon_font = "font_target_larger"
	end
	
	coming_soon_scene_manager:add_actor(
		vend_screen_box_title_actor,
		0,
		100,
		1024,
		100,
		{	text = strings.coming_soon_coming_soon,
			horizontalAlignment = "center",
			foregroundColor = "black",
			font = coming_soon_font,
			renderOptions = "Text"
		} )
		
	coming_soon_scene_manager:add_actor(
		vend_screen_rental_coming_soon_actor,
		0,
		646,
		1024,
		100,
		{	text = strings.rentals_coming_soon,
			horizontalAlignment = "center",
			foregroundColor = "black",
			font = "font_target_large",
			renderOptions = "Text"
		} )

	draw_coming_soon_products( coming_soon_products )	
end

function show_coming_soon_vend_screen()

	if coming_soon_scene_manager == nil then
		coming_soon_scene_manager = newscenemanager()
	end
	coming_soon_scene_manager:clear()	
end

function clear_coming_soon_vend_screen_if_up( )
	if coming_soon_scene_manager ~= nil then
		coming_soon_scene_manager:clear()
		coming_soon_scene_manager = nil
	end
end
