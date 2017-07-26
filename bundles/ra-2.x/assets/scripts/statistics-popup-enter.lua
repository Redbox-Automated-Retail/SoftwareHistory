--
-- statistics-popup-enter.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local skipped = {}
skipped["product_id"] = true
skipped["stars"] = true
skipped["running_time"]= true
skipped["starring"] = true
skipped["long_name"] = true
skipped["sort_name"] = true
skipped["image_file"] = true
skipped["description"] = true
skipped["directors"] = true
skipped["genres"] = true

local requires_dump = {}

local format_date = {}
format_date["sold_out"] = true
format_date["created_date"] = true
format_date["last_max_ofr_date"] = true
format_date["merchandise_date"] = true
format_date["do_not_rent_date"] = true
format_date["sort_date"] = true

local DisplayRows = function (actorprefix, data, x, y)
	local key_text = nil
	local value_text = nil
	local first_value = true
	local original_y = y
	local count = 0
	for k, v in pairs(data) do
		should_skip = skipped[k]
		if (should_skip == nil) then
			count = count + 1

			key_text = (key_text ~= nil and key_text or "") .. tostring(not first_value and "\n" or "") .. tostring(string.upper(k))
			local text
			if format_date[k] == true then
				_,_,yr,m,d=string.find(v, "(%d%d%d%d)(%d%d)(%d%d)")
				text = m .. "/" .. d .. "/" .. yr
			elseif requires_dump[k] ~= nil then
				text = DataDumper( v, k, true )
			else
				text = v
			end

			value_text = (value_text ~= nil and value_text or "") .. tostring(not first_value and "\n" or "") .. tostring(text)

			if first_value then
				first_value = false
			end

			y =	y + 17
		end
	end

	local actor_height = 20 * count
	kernel.createactor(
		actorprefix .. "_key",
		false,
		x,
		original_y,
		200,
		actor_height,
		key_text,
		"font_akzidenz_regular_tiny",
		nil,
		"Gray",
		"Black",
		nil,
		nil,
		nil,
		nil,
		"Text",
		"None")

	kernel.createactor(
		actorprefix .. "_value",
		false,
		x+200,
		original_y,
		300,
		actor_height,
		tostring(value_text),
		"font_akzidenz_regular_tiny",
		nil,
		"Gray",
		"Black",
		nil,
		nil,
		nil,
		nil,
		"Text",
		"None")

	return x, y
end


idle_timer:suspend()
local product_type = ProductTypes[statistics_popup_view_product.product_type_id]
local product_family = ProductFamilies[product_type.product_family_id]

local function CreateSectionHeader( x, y, name, title )
	kernel.createactor(
		name,
		false,
		x,
		y,
		250,
		40,
		title,
		"font_target_smaller_overlays",
		nil,
		"White",
		"181,18,27",
		nil,
		nil,
		nil,
		nil,
		"Text",
		"None")
end

local function CreateMasterProductCatalogSection(x, y)
	CreateSectionHeader(20, y, "master_catalog_label", "Master Product Catalog")
	y = y + 23

	-- Display data stored in the Master Product Catalog
	local master_details = MasterProductCatalog[ statistics_popup_view_product.product_id ]
	if master_details ~= nil and master_details.sort_name ~= nil then
		kernel.setmacro( "StatisticsPopupView:TitleArea", "  " .. tostring( statistics_popup_view_product.product_id ) .. " - ".. statistics_popup_view_product.sort_name )
		kernel.markactordirty( "title_area" )
	end
	x, y = DisplayRows("master_catalog", master_details, x, y )
	return x, y
end

local function CreatePricingSection(x, y)
	CreateSectionHeader(20, y, "pricing_label", "Pricing")
	y = y + 23
	-- Display Pricing Details
	local pricing_table = StoreManager.PricingManager:get_product_price( statistics_popup_view_product.product_id )
	pricing_table.is_early_released = StoreManager.PricingManager:is_early_released( statistics_popup_view_product )
	pricing_table.is_alternate_priced = StoreManager.PricingManager:is_alternate_priced( statistics_popup_view_product.product_id )
	x, y = DisplayRows( "pricing", pricing_table, 40, y )
	return x, y
end

local function CreateGenresSection(x, y)
	CreateSectionHeader(20, y, "genres_label", "Genres")
	y = y + 25

	local genre_list = {}
	for i,v in pairs(statistics_popup_view_product.genres) do
		local genres = ProductGenreData[product_family.product_family_name].filter_genres[i]
		genre_list[tostring(i) .. ": " .. tostring(genres ~= nil and genres or "")] = v
	end
	x, y = DisplayRows("genres", genre_list, 40, y )
	return x, y
end

local function CreateLocalCatalogSection(x, y)
	CreateSectionHeader(380, 60, "local_catalog_label", "Local Product Catalog")
	y = y + 25

	-- Display local Product Catalog data
	local local_product = ProductCatalog[ statistics_popup_view_product.product_id ]
	if (local_product ~= nil) then
		x, y = DisplayRows("local_catalog", local_product, x, y )
	end
	return x, y
end

local function CreateProductHistorySection(x, y)
	CreateSectionHeader(380, y, "product_history_label", "Product History")
	y = y + 23

	-- Display data stored in Product History table
	local history = ProductHistory[ statistics_popup_view_product.product_id ]
	if (history ~= nil) then
		x, y = DisplayRows("product_history", history, x, y )
	end
	return x, y
end 

local function CreateKioskProductSection(x, y)
	CreateSectionHeader(380, y, "kiosk_product_label", "Kiosk Product")
	y = y + 25

	local kiosk_product = {}
	local kiosk_product_type = "None"
	if statistics_popup_view_product.kiosk_product_type == 0 then kiosk_product_type = "None"
	elseif	statistics_popup_view_product.kiosk_product_type == 1 then kiosk_product_type = "Title"
	elseif	statistics_popup_view_product.kiosk_product_type == 2 then kiosk_product_type = "MDV"
	elseif	statistics_popup_view_product.kiosk_product_type == 3 then kiosk_product_type = "TitleRollup"
	elseif	statistics_popup_view_product.kiosk_product_type == 4 then kiosk_product_type = "DynamicBrowse"
	end	
	kiosk_product["Type"] = kiosk_product_type
	kiosk_product["Show out of Stock"] = statistics_popup_view_product.show_out_of_stock
	x, y = DisplayRows("kiosk_product", kiosk_product, x, y )
	return x, y
end

local function CreateBarCodeSection(x, y)
	CreateSectionHeader(380, y, "barcode_label", "Barcodes")
	y = y + 25

	local barcodes = ProductToBarcode[statistics_popup_view_product.product_id]
	if barcodes ~= nil then
		for position_key, position_info in BarcodePosition:pairs() do
			for k, v in pairs(barcodes) do
				if position_info.barcode == k then
					barcodes[k].location = position_key
					barcodes[k].location_status = position_info.status
				end
			end
		end
		local total_count = 0
		local shown_count = 0
		local temp_y = y
		local barcode_list = {}
		for k, v in pairs(barcodes) do
			if temp_y < 630 then
				barcode_list[k .. " " .. (v.status or "nil") .. " ".. (v.location or "nil") .. " " ..
						string.sub( (v.location_status or " "), 1, 1 )] = ""
				temp_y = temp_y + 17
				shown_count = shown_count + 1
			end
			total_count = total_count + 1
		end
		x, y = DisplayRows("barcode", barcode_list, x, y )
		if shown_count ~= total_count then
			kernel.setactortext( "barcode_label", "First (" .. tostring(shown_count) .. " of " .. tostring(total_count) ..") Barcodes" )
		end
	end
end

local function CreateSections()
	-- Set starting location of first data actors
	local x = 40
	local y = 60
	x, y = CreateMasterProductCatalogSection(x, y)
	x, y = CreatePricingSection(x, y)
	CreateGenresSection(x, y)
	x = 400
	y = 60
	x, y = CreateLocalCatalogSection(x, y)
	x, y = CreateProductHistorySection(x, y)
	x, y = CreateKioskProductSection(x, y)
	x, y = CreateBarCodeSection(x, y)
end

local function RemoveActor( actor )
	kernel.removeactor(actor)
	kernel.markactordirty(actor)
end

local function RemoveActors (section)
	kernel.removeactor(section .. "_label")
	kernel.removeactor(section .. "_key")
	kernel.removeactor(section .. "_value")
end

local function RemoveSections()
	RemoveActors("master_catalog")
	RemoveActors("pricing")
	RemoveActors("genres")
	RemoveActors("local_catalog")
	RemoveActors("product_history")
	RemoveActors("kiosk_product")
	RemoveActors("barcode")
end

local function SetImage()
	-- Set the image
	if product_family.product_family_name == "Movies" then
		source_image_name = "image_movie_missing_box_art"
	elseif product_family.product_family_name == "Games" then
		source_image_name = "image_game_missing_box_art"
	end
	local image_registered = kernel.dataserviceregisterimage(	statistics_popup_view_product.image_file, statistics_popup_view_product.image_file )

	if image_registered then
		source_image_name = string.format("cache:%s", statistics_popup_view_product.image_file)
	end
	kernel.setactorimage("product_image", source_image_name)
	kernel.markactordirty( "product_image" )
end

local function PopulateUI()
	CreateSections()
	SetImage()
end

PopulateUI()


local bluray_product = nil
local dvd_product = nil
local title_rollup_product = nil
if statistics_popup_view_product.kiosk_product_type == 3 and statistics_popup_view_product.kiosk_products ~= nil then 
	title_rollup_product = statistics_popup_view_product
	dvd_product = statistics_popup_view_product.kiosk_products[statistics_popup_view_product.dvd_index]
	bluray_product = statistics_popup_view_product.kiosk_products[statistics_popup_view_product.blu_ray_index]

	kernel.showactor("title_rollup_button")
	kernel.disableactor("title_rollup_button")

	if dvd_product ~= nil then
		kernel.showactor("dvd_button")
		kernel.enableactor("dvd_button")
	else		
		kernel.hideactor("dvd_button")
		kernel.disableactor("dvd_button")
	end

	if bluray_product ~= nil then
		kernel.showactor("bluray_button")
		kernel.enableactor("bluray_button")
	else		
		kernel.hideactor("bluray_button")
		kernel.disableactor("bluray_button")
	end

else
	kernel.hideactor("title_rollup_button")
	kernel.hideactor("dvd_button")
	kernel.hideactor("bluray_button")
end

local function UpdateButtonStatus()
	if title_rollup_product == nil or statistics_popup_view_product == title_rollup_product  then
		kernel.disableactor("title_rollup_button")
	else
		kernel.enableactor("title_rollup_button")
	end

	if dvd_product == nil or statistics_popup_view_product == dvd_product  then
		kernel.disableactor("dvd_button")
	else
		kernel.enableactor("dvd_button")
	end

	if bluray_product == nil or statistics_popup_view_product == bluray_product  then
		kernel.disableactor("bluray_button")
	else
		kernel.enableactor("bluray_button")
	end
end

local function RefreshDisplay()
		RemoveSections()
		PopulateUI()
		UpdateButtonStatus()
end

kernel.registeractorhitnotify(
	"title_rollup_button",
	function()
		statistics_popup_view_product = title_rollup_product
		RefreshDisplay()
	end
)

kernel.registeractorhitnotify(
	"dvd_button",
	function()
		statistics_popup_view_product = dvd_product
		RefreshDisplay()
	end
)

kernel.registeractorhitnotify(
	"bluray_button",
	function()
		statistics_popup_view_product = bluray_product
		RefreshDisplay()
	end
)


