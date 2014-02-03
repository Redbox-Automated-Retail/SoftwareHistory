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
format_date["release_date"] = true
format_date["do_not_rent_date"] = true
format_date["sort_date"] = true

local DisplayRows = function (data, x, y)
	for k, v in pairs(data) do
		should_skip = skipped[k]
		if (should_skip == nil) then

			local labelName = k .. "_label_key"
			kernel.createactor(
				labelName,
				false,
				x,
				y,
				200,
				20,
				tostring(string.upper(k)),
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

			x = x + 200
			local text
			if format_date[k] == true then
				_,_,yr,m,d=string.find(v, "(%d%d%d%d)(%d%d)(%d%d)")
				text = m .. "/" .. d .. "/" .. yr
			elseif requires_dump[k] ~= nil then
				text = DataDumper( v, k, true )
			else
				text = v
			end

			labelName = tostring(v) .. "_label_value"
			kernel.createactor(
				labelName,
				false,
				x,
				y,
				300,
				20,
				tostring(text),
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

			x = x - 200
			y =	y + 17
		end
	end
	return x, y
end

idle_timer:suspend()
culture_idle_timer:suspend()

kernel.createactor(
	"master_catalog_label",
	false,
	20,
	60,
	250,
	40,
	"Master Product Catalog",
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

-- Set starting location of first data actors
local x = 40
local y = 85

-- Display data stored in the Master Product Catalog
local master_details = MasterProductCatalog[ browse_selection_selected_product.product_id ]
if master_details ~= nil and master_details.sort_name ~= nil then
	kernel.setactortext( "title_area", "  " .. tostring( master_details.product_id ) .. " - ".. master_details.sort_name )
end
x, y = DisplayRows( master_details, x, y )

kernel.createactor(
	"pricing_label",
	false,
	20,
	y + 7,
	200,
	40,
	"Pricing",
	"font_target_smaller_overlays",
	nil,
	"White",
	"Black",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")

y = y + 30
-- Display Pricing Details
local pricing_table = StoreManager.PricingManager:get_product_price( browse_selection_selected_product.product_id )
pricing_table.is_early_released = StoreManager.PricingManager:is_early_released( browse_selection_selected_product )
pricing_table.is_alternate_priced = StoreManager.PricingManager:is_alternate_priced( browse_selection_selected_product.product_id )
x, y = DisplayRows( pricing_table, 40, y )

kernel.createactor(
	"genres_label",
	false,
	20,
	y + 7,
	200,
	40,
	"Genres",
	"font_target_smaller_overlays",
	nil,
	"White",
	"Black",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")
y = y + 30
x, y = DisplayRows( master_details.genres, 40, y )

kernel.createactor(
	"local_catalog_label",
	false,
	380,
	60,
	250,
	40,
	"Local Product Catalog",
	"font_target_smaller_overlays",
	nil,
	"White",
	"Black",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")
x = 400
y = 85

-- Display local Product Catalog data
local local_product = ProductCatalog[ browse_selection_selected_product.product_id ]
x, y = DisplayRows( local_product, x, y )

kernel.createactor(
	"product_history_label",
	false,
	380,
	y + 7,
	250,
	40,
	"Product History",
	"font_target_smaller_overlays",
	nil,
	"White",
	"Black",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")
y = y + 30

-- Display data stored in Product History table
local history = ProductHistory[ browse_selection_selected_product.product_id ]
x, y = DisplayRows( history, x, y )

-- Display barcodes for the title
kernel.createactor(
	"product_to_barcode_label",
	false,
	380,
	y + 7,
	200,
	40,
	"Barcodes",
	"font_target_smaller_overlays",
	nil,
	"White",
	"Black",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")
y = y + 30

local barcodes = ProductToBarcode[browse_selection_selected_product.product_id]
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
	for k, v in pairs(barcodes) do
		if y < 630 then
			kernel.createactor(
				k .."_barcode_label",
				false,
				x,
				y,
				220,
				20,
				k .. " " .. (v.status or "nil") .. " ".. (v.location or "nil") .. " " ..
					string.sub( (v.location_status or " "), 1, 1 ),
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
			y = y + 17
			shown_count = shown_count + 1
		end
		total_count = total_count + 1
	end
	if shown_count ~= total_count then
		kernel.setactortext( "product_to_barcode_label", "First (" .. tostring(shown_count) .. " of " .. tostring(total_count) ..") Barcodes" )
	end
end
