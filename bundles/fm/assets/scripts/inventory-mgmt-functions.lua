--
-- inventory-mgmt-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
Inventory_Table = nil
Inventory_All_Table = nil
config = kernel.getdictionary( 'config' )
sort = {}

ESRBRatings = {
	[14] = { rating_id = 14, name = "EC" },
	[15] = { rating_id = 15, name = "E" },
	[16] = { rating_id = 16, name = "E 10+" },
	[17] = { rating_id = 17, name = "T" },
	[18] = { rating_id = 18, name = "M (17+)" },
	[19] = { rating_id = 19, name = "RP" },
	[20] = { rating_id = 20, name = "AO (18+)" }
}

Ratings = {
	[5] =  { rating_id = 5,  name = "G" },
	[6] =  { rating_id = 6,  name = "PG" },
	[7] =  { rating_id = 7,  name = "PG-13" },
	[8] =  { rating_id = 8,  name = "R" },
	[10] = { rating_id = 10, name = "NR" },
	[21] = { rating_id = 21, name = "TVMA" },
	[22] = { rating_id = 22, name = "TVPG" },
	[23] = { rating_id = 23, name = "TV14" },
	[24] = { rating_id = 24, name = "TVG" },
	[25] = { rating_id = 25, name = "TVY" },
	[26] = { rating_id = 26, name = "TVY7" }
}

sort_columns = nil
default_sort_column = "deck_slot"

function get_sort_order_for_column (col)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			return v.order
		end
	end
	return "A"
end

function set_sort_order_for_column(col, sort_order)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			v.order = sort_order
		end
	end
end

function get_sort_column_position(col)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			return k
		end
	end
	return nil
end

function is_column_selected_for_sort(col_name)
	for _, v in ipairs(sort_columns) do
		if col_name == "deck_slot" then
			col_name = "deck"
		end

		if v.name == col_name then
			return true
		end
	end
	return false
end

function enable_column_highlight(col)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			v.enable_highlight = true
		end
	end
end

function disable_column_highlight(col)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			v.enable_highlight = false
		end
	end
end

function is_column_highlight_enabled(col)
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			return v.enable_highlight
		end
	end
end

function move_to_last(col)
	local temp_col = nil
	for k, v in ipairs(sort_columns) do
		if v.name == col then
			temp_col = v
			table.remove(sort_columns, k)
			break
		end
	end
	table.insert(sort_columns, temp_col)
end

function add_column_to_sort(col)
	if #sort_columns == 2
	and is_column_selected_for_sort(default_sort_column)
	and col ~= "deck_slot" then
		table.remove(sort_columns, get_sort_column_position("deck"))
		table.remove(sort_columns, get_sort_column_position("slot"))
	end

	if col == "deck_slot" then
		col = "deck"
	end

	if is_column_selected_for_sort(col) then
		local order = get_sort_order_for_column(col)

		if col == "deck" then
			if order == "A" then
				if is_column_highlight_enabled("deck") == false then
					enable_column_highlight("deck")
					enable_column_highlight("slot")
				else
					set_sort_order_for_column("deck", "D")
					set_sort_order_for_column("slot", "D")
				end
			else
				disable_column_highlight("deck")
				disable_column_highlight("slot")
			end

			if get_sort_column_position("deck") ~= #sort_columns-1 then
				move_to_last("deck")
				move_to_last("slot")
			end
		else
			table.remove(sort_columns, get_sort_column_position(col))

			if order == "A" then
				table.insert(sort_columns, {name = col, order = "D", enable_highlight = true})
			end
		end
	else
		table.insert(sort_columns, {name = col, order = "A", enable_highlight = true})
		if col == "deck" then
			table.insert(sort_columns, {name = "slot", order = "A", enable_highlight = true})
		end
	end

	if is_column_selected_for_sort("deck")
	and is_column_highlight_enabled("deck") == false then
		table.remove(sort_columns, get_sort_column_position("deck"))
		table.remove(sort_columns, get_sort_column_position("slot"))
	end

	if #sort_columns == 0 then
		do_multi_sort_on(default_sort_column)
	elseif is_column_selected_for_sort("deck") == false then
		table.insert(sort_columns, {name = "deck", order = "A", enable_highlight = false})
		table.insert(sort_columns, {name = "slot", order = "A", enable_highlight = false})
	end
end

function get_column_order_string()
	local str = ""
	for k, v in ipairs(sort_columns) do
		local col_str = ""
		if v.name == "merchandise_date" then
			col_str = kernel.getactortext("releasedate_label")
		elseif v.name == "deck" then
			col_str = "Deck Slot"
		else
			col_str = kernel.getactortext(v.name.. "_label")
		end

		if v.name == "deck"
		and is_column_highlight_enabled("deck") == false then
			return str
		end

		if v.name ~= "slot" then
			if k>1 then str = str.. "|" end
			str = str.. col_str.. "(".. v.order.. ")"
		end
	end
	return str
end

function is_less_than(v1, v2)
	if (type(v1) ~= type(v2))
	or (type(v1) == "string" and type(v2) == "string") then
		return string.lower(tostring(v1)) < string.lower(tostring(v2))
	end
	return v1 < v2
end

function is_greater_than(v1, v2)
	if (type(v1) ~= type(v2))
	or (type(v1) == "string" and type(v2) == "string") then
		return string.lower(tostring(v1)) > string.lower(tostring(v2))
	end
	return v1 > v2
end

function default_sort(a, b)
	if a["deck"] ~= b["deck"] then
		return a["deck"] < b["deck"]
	else
		return a["slot"] < b["slot"]
	end
end

function multi_sort(a, b)
	for k, v in ipairs(sort_columns) do
		local v1 = a[v.name] or 0
		local v2 = b[v.name] or 0

		if string.lower(tostring(v1)) ~= string.lower(tostring(v2)) then
			if v.name == "rented"
			or v.name == "barcode"
			or v.name == "deck"
			or v.name == "slot"
			then
				v1 = tonumber(v1)
				v2 = tonumber(v2)
			end
			if v.order == "A" then
				return is_less_than(v1, v2)
			else
				return is_greater_than(v1, v2)
			end
		end
	end
end

function show_sorting_arrows()
	kernel.setactorforegroundcolor("title_label", "white")
	kernel.setactorforegroundcolor("rating_label", "white")
	kernel.setactorforegroundcolor("media_label", "white")
	kernel.setactorforegroundcolor("deck_label", "white")
	kernel.setactorforegroundcolor("slot_label", "white")
	kernel.setactorforegroundcolor("barcode_label", "white")
	kernel.setactorforegroundcolor("releasedate_label", "white")
	kernel.setactorforegroundcolor("rented_label", "white")
	kernel.setactorforegroundcolor("status_label", "white")

	kernel.hideactor("title_sort_arrow")
	kernel.hideactor("rating_sort_arrow")
	kernel.hideactor("media_sort_arrow")
	kernel.hideactor("deck_slot_sort_arrow")
	kernel.hideactor("barcode_sort_arrow")
	kernel.hideactor("releasedate_sort_arrow")
	kernel.hideactor("rented_sort_arrow")
	kernel.hideactor("status_sort_arrow")

	for _, v in ipairs(sort_columns) do
		if v.name
		and is_column_selected_for_sort(v.name) ~= "slot" then
			if v.name == "merchandise_date" then
				kernel.setactorforegroundcolor("releasedate_label", "red")
				kernel.showactor("releasedate_sort_arrow")
				if v.order == "A" then
					kernel.setactorimage("releasedate_sort_arrow", "sort_asc")
				else
					kernel.setactorimage("releasedate_sort_arrow", "sort_desc")
				end
			elseif v.name == "deck" or v.name == "slot" then
				if is_column_highlight_enabled("deck") then
					kernel.setactorforegroundcolor("deck_label", "red")
					kernel.setactorforegroundcolor("slot_label", "red")
					kernel.showactor("deck_slot_sort_arrow")
					if v.order == "A" then
						kernel.setactorimage("deck_slot_sort_arrow", "sort_asc")
					else
						kernel.setactorimage("deck_slot_sort_arrow", "sort_desc")
					end
				end
			else
				kernel.setactorforegroundcolor(v.name.. "_label", "red")
				kernel.showactor(v.name.. "_sort_arrow")
				if v.order == "A" then
					kernel.setactorimage(v.name.. "_sort_arrow", "sort_asc")
				else
					kernel.setactorimage(v.name.. "_sort_arrow", "sort_desc")
				end
			end
		end
	end
end

function do_multi_sort_on(col_name)
	if col_name ~= nil then
		add_column_to_sort(col_name)
		table.sort(Inventory_Table, multi_sort)
		if col_name == "deck_slot" then
			col_name = "deck"
		end
	else
		table.sort(Inventory_Table, multi_sort)
	end

	if #sort_columns == 2
	and is_column_selected_for_sort("deck")
	and col_name ~= "deck" then
		kernel.setactortext("sort_scheme", "Sorted by - Deck Slot(A)")
	else
		kernel.setactortext("sort_scheme", "Sorted by - ".. get_column_order_string())
	end

	show_table(
		Inventory_Scene_Manager,
		Inventory_Pager,
		Inventory_Table,
		Inventory_Config
	)
end

is_slot_excluded = function(deck, slot)
	if ExcludedSlots == nil then
		return false
	end

	for k, v in ipairs(ExcludedSlots.results) do
		if tostring(v.deck) == tostring(deck) and tostring(v.slot) == tostring(slot) then
			kernel.log("found excluded slot - D:"..v.deck.." S:"..v.slot)
			return true
		end
	end
	return false
end

reload_inventory = function()
	local function rental_count_for_barcode( barcode )
		local record = BarcodeToProduct[barcode]
		if record == nil then return "" end
		if record.total_rental_count == nil then return "" end
		return tostring( record.total_rental_count )
	end

	Inventory_Table = {}
	Inventory_All_Table = {}
	sort = {}
    local product_type_lookup = {}
    local product_family_lookup = {}
	local product_type = getprofiletable( "profile", "ProductType" )

	for _, v in product_type:pairs() do
		product_type_lookup[v["product_type_id"]] = v["product_type_name"]
		product_family_lookup[v["product_type_id"]] = v["product_family_id"]
	end

	EmptySlots = 0
	local wide_slots = {
		["6"] = true,
		["12"] = true,
		["18"] = true,
		["24"] = true,
		["30"] = true,
		["36"] = true,
		["42"] = true,
		["48"] = true,
		["54"] = true,
		["60"] = true,
		["66"] = true,
		["72"] = true
	}
	
	local all_unmatched_multi_disc_barcodes = get_all_unmatched_multi_disc_barcodes()
	local function is_unmatched( barcode )
		for _, unmatched_barcode in ipairs( all_unmatched_multi_disc_barcodes ) do
			if barcode == unmatched_barcode then
				return true
			end
		end
		return false
	end
	
	for pos, disc in BarcodePosition:pairs() do
		local fields = {}
		string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields,values) end )
		if fields[1] ~= nil
		and fields[1] ~= tostring( qlm_deck )
		and fields[2] ~= nil
		and fields[1] ~= "0"
		and fields[2] ~= "0" then
			local barcode = disc["barcode"]
			if  barcode ~= "UNDEFINED"
			and barcode ~= "EMPTY" then
				local row = {}
				row.deck = fields[1]
				row.slot = fields[2]
				row.barcode = disc["barcode"]
				row.rented = rental_count_for_barcode( barcode )
				row.status = disc["status"]
				row.title = ""
				row.media = ""
				row.unmatched = is_unmatched( barcode )
				if barcode ~= "UNKNOWN" then
					local product = BarcodeToProduct[barcode]
					if product ~= nil then
						row.product_id = product.product_id
						if product.status ~= nil and row.status ~= "Reserved" then
                            row.status = get_rapid_status( barcode, nil )
                            if row.status == nil then
								-- if the barcode is not in rapid status then revert to inventory status unless the inventory status is equal to Rebalance, Redeployment or Thinned
								-- this happens when a barcode is unthinned
								if product.status  == "Rebalancing" or product.status == "Redeployment" or product.status == "Thinned" then
									row.status = "Known"
								else
									row.status = product.status 
								end
							end
						end
						
						local catalog_item = MasterProductCatalog[product["product_id"]]
						if catalog_item ~= nil
						and catalog_item["do_not_rent_date"] ~= nil
						and catalog_item["do_not_rent_date"] ~= "" then
							local DoNotRentDate = string.sub(catalog_item["do_not_rent_date"], 1, 8)
							local days = tonumber(kernel.getelapseddays( DoNotRentDate, get_current_date_string( "%4d%02d%02d" ) ))
							if days >= 0 then
								row.status = "DNR"
							end
						end
						if catalog_item ~= nil then
							row.title = tostring(catalog_item["sort_name"])
							if row.status == "Unknown Title" then
								row.status = "Known" --Override the display while sync catches up, saying unknown title while showing the title makes us look bad
							end
							local product_type_id = catalog_item["product_type_id"]
							if product_type_id ~= nil then
								if product_type_id == 17 then
									row.media = "DC"
								elseif product_type_id == 18 then
									row.media = "Switch"
								else
									row.media = product_type_lookup[tonumber(product_type_id)]
								end
								row.rating = get_rating(product_family_lookup[tonumber(product_type_id)], catalog_item.rating_id)
							else
								row.media = ""
								row.rating = ""
							end
							local year = string.sub(catalog_item["merchandise_date"], 1, 4)
							local month = string.sub(catalog_item["merchandise_date"], 5, 6)
							local day = string.sub(catalog_item["merchandise_date"], 7, 8)
							row.released = string.format("%s-%s-%s", month, day, year)
							row.merchandise_date = catalog_item["merchandise_date"]
						end
					else
						if row.barcode == nil or trim( row.barcode ) == "" then
							row.barcode = "UNKNOWN"
						end
					end
				end
				
				if row.status == "Damaged" or row.status == "DNR" or row.status == "Fraud" or row.status == "Reserved" then
					row.unmatched = false
				end
				
				table.insert(Inventory_Table, row)
				table.insert(Inventory_All_Table, row)
			end
			local is_wide_slot = wide_slots[ fields[2] ]
			if barcode == "EMPTY"
			and fields[1] ~= qlm_deck
			and ( ( max_slots == 72 and fields[1] == "7" and is_wide_slot ~= nil ) == false )
			then
				if is_slot_excluded(fields[1], fields[2]) == false then
					EmptySlots = EmptySlots + 1
				end
			end
		end
	end
end

function get_rating(product_family_id, rating_id)
local rating = nil
	if product_family_id ~= nil and rating_id ~= nil then
		if product_family_id == 1 then
			if(Ratings[rating_id]~= nil) then
				rating = Ratings[rating_id].name
			end
		end
		if product_family_id == 2 then
			if(ESRBRatings[rating_id] ~= nil) then
				rating = ESRBRatings[rating_id].name
			end
		end
		return rating
	end
end

Inventory_Scene_Manager = newscenemanager()
Inventory_Pager = newpager()

Inventory_Config = {
  rectangle_x = config.inventory_rectangle_x,
  rectangle_y = config.inventory_rectangle_y,
  rectangle_width = config.inventory_rectangle_width,
  rectangle_height = config.inventory_rectangle_height
}

--wire up the call backs from the pager
Inventory_Pager.next_changed = function( enabled )
  if enabled == true then
    kernel.setactorimage( "right_arrow", "right_arrow" )
    kernel.setactorforegroundcolor( "right_arrow", "White" )
    kernel.enableactor( "right_arrow" )
  else
    kernel.setactorimage( "right_arrow", "right_arrow_disabled" )
    kernel.setactorforegroundcolor( "right_arrow", "Gray" )
    kernel.disableactor( "right_arrow" )
  end
end

Inventory_Pager.prev_changed = function( enabled )
  if enabled == true then
    kernel.setactorimage( "left_arrow", "left_arrow" )
    kernel.setactorforegroundcolor( "left_arrow", "White" )
    kernel.enableactor( "left_arrow" )
  else
    kernel.setactorimage( "left_arrow", "left_arrow_disabled" )
    kernel.setactorforegroundcolor( "left_arrow", "Gray" )
    kernel.disableactor( "left_arrow" )
  end
end

show_table = function(
  scene_manager,
  pager,
  table,
  table_config,
  empty_table_msg )

	scene_manager:clear()
	local x = table_config.rectangle_x
	x = x + 70
	local y = table_config.rectangle_y
	y = y + 70
	local actor_index = 1
	pager:mark_current_page()
	local tabStops = {275,100,55,65,115,130,70}
	kernel.hideactor("inventory_item_selected")

	if table == nil or
		#table <= 0 then
		kernel.setactortext(
			"no_inventory",
			empty_table_msg)
		kernel.showactor("no_inventory")
		kernel.centeractorx("no_inventory")
		kernel.centeractory("no_inventory")

		kernel.disableactor("title_label", "white")
		kernel.disableactor("rating_label", "white")
		kernel.disableactor("media_label", "white")
		kernel.disableactor("deck_label", "white")
		kernel.disableactor("slot_label", "white")
		kernel.disableactor("barcode_label", "white")
		kernel.disableactor("releasedate_label", "white")
		kernel.disableactor("rented_label", "white")
		kernel.disableactor("status_label", "white")
		return
	end
	kernel.hideactor("no_inventory")

	kernel.enableactor("title_label", "white")
	kernel.enableactor("rating_label", "white")
	kernel.enableactor("media_label", "white")
	kernel.enableactor("deck_label", "white")
	kernel.enableactor("slot_label", "white")
	kernel.enableactor("barcode_label", "white")
	kernel.enableactor("releasedate_label", "white")
	kernel.enableactor("rented_label", "white")
	kernel.enableactor("status_label", "white")
	
	local function get_disc_number( barcode )
		if barcode ~= nil and BarcodeToProduct[barcode] ~= nil and BarcodeToProduct[barcode].product_id ~= nil and MasterProductCatalog[BarcodeToProduct[barcode].product_id] ~= nil and MasterProductCatalog[BarcodeToProduct[barcode].product_id].disc_number ~= nil then
			return MasterProductCatalog[BarcodeToProduct[barcode].product_id].disc_number
		end
		return 1
	end
	
	while true do
		local textHeight = 0
		local columns = table[pager:get_index()]
		local inventoryItemActorName = "inventoryitem" .. actor_index
		
		local barcode = columns["barcode"] or " "
		
		local title = columns["title"] or " "
		if kernel.getactortag( InventoryViewFilter ) == "Unmatched" then
			title = tostring( title ) .. " (disc " .. tostring( get_disc_number( barcode ) ) .. ")"
		end
		
		local media = columns["media"] or " "
		local rating = columns["rating"] or " "
		local deck = columns["deck"] or " "
		local slot = columns["slot"] or " "
		local released = columns["released"] or " "
		local rented = columns["rented"] or " "
		local status = columns["status"] or " "
		local length = string.len(title)
		local params = {
				renderOptions = "Image",
				image = "check_box",
				hitOptions = "Enabled"
				}
		local check_box_name = "checkbox_" .. barcode .. "_" .. deck .. "_" .. slot

		if check_box_state[check_box_name] ~= nil then
			params.image = "check_box_checked"
		end

		scene_manager:add_actor(
			check_box_name,
			x - 65,
			y - 10,
			50,
			45,
			params,
			function()
				kernel.setlasthitactorname( check_box_name )
				kernel.executescript("check_box_hit")
				if check_box_state[check_box_name] ~= nil then
					check_box_state[check_box_name] = columns
				end
				-- disable the remove disc button if no discs
				local count = 0
				for i, v in pairs(check_box_state ) do
					count = count + 1
				end
				if count > 0 then
					kernel.setactorforegroundcolor( "remove_disc_button", "White" )
					kernel.enableactor( "remove_disc_button" )
					kernel.setactortext( "selected_status", "Selected: " .. count)
				else
					kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
					kernel.disableactor( "remove_disc_button" )
					kernel.setactortext( "selected_status", nil )
				end
			end )

		if status == "Fraud" or barcode == "UNKNOWN" then
			kernel.setactorimage( check_box_name, "check_box_disabled" )
			kernel.disableactor( check_box_name )
		end

		params = {
			text = title,
			font = "small_font",
			foregroundColor = "black",
			renderOptions = "Text, ElideText"
		}
		scene_manager:add_actor(
			"title_" .. inventoryItemActorName,
			90,
			y,
			255,
			20,
			params,
			nil )

        params.text = rating
		scene_manager:add_actor(
			"rating" .. inventoryItemActorName,
			365,
			y,
			90,
			20,
			params,
			nil )

		params.text = media
		scene_manager:add_actor(
			"media" .. inventoryItemActorName,
			434,
			y,
			90,
			20,
			params,
			nil )

		params.text = deck
		scene_manager:add_actor(
			"deck" .. inventoryItemActorName,
			500,
			y,
			35,
			20,
			params,
			nil )

		params.text = slot
		scene_manager:add_actor(
			"slot" .. inventoryItemActorName,
			550,
			y,
			35,
			20,
			params,
			nil )

		params.text = barcode
		scene_manager:add_actor(
			"barcode" .. inventoryItemActorName,
			583,
			y,
			100,
			20,
			params,
			nil )

		params.text = released
		scene_manager:add_actor(
			"released" .. inventoryItemActorName,
			699,
			y,
			100,
			20,
			params,
			nil )

		params.text = rented
		scene_manager:add_actor(
			"rented" .. inventoryItemActorName,
			830,
			y,
			60,
			20,
			params,
			nil )

		params.text = status
		scene_manager:add_actor(
			"status" .. inventoryItemActorName,
			899,
			y,
			104,
			20,
			params,
			nil )

		y = y + 60
		-- stop painting when the next row exceeds the container
		if y >= table_config.rectangle_height  + table_config.rectangle_y then
			break
		end

		actor_index = actor_index + 1
		pager:increment_index()
		if pager:get_index() > #table then
			break
		end
	end
	pager:update_paging( #table )

	show_sorting_arrows()
end

filter_by_title = function( value )
	local result = false
	for k,v in pairs(alpha_table) do
		if v ~= "0-9" then
			if string.lower(string.sub(value["title"], 0 ,1)) == string.lower(v)
			and (value["status"] ~= "Unknown"
			and value["status"] ~= "UnknownTitle")  then
				result = true
				break
			end
		else
			for i = 0,9 do
				if string.lower(string.sub(value["title"], 0 ,1)) == tostring(i)
				and (value["status"] ~= "Unknown"
				and value["status"] ~= "UnknownTitle") then
					result = true
					break
				end
			end
		end
	end
	return result
end

filter_by_tabs = function( value )
	local result = false
	if value["media"] ~= nil
	and filter_titles_tables.media[value["media"]] == true
	and filter_titles_tables.movie_rating[value["rating"]] == true then
		if last_hit_filter_tag == "Thinned" then
			if (value["status"] == last_hit_filter_tag or value[ "status" ] == "DNR") then
				result = true
			else
				result = false
			end
		elseif last_hit_filter_tag == "Unmatched" then
			result = value["unmatched"]
		else 
			if value["status"] == last_hit_filter_tag then
				result = true
			else
				result = false
			end
		end
	elseif (value["media"] == "Wii U" or value["media"] == "Wii" or value["media"] == "PC" or value["media"] == "DS" or value["media"] == "PSP" or value["media"] == "PS4" or value["media"] == "PS3" or value["media"] == "PS2" or value["media"] == "Xbox 360" or value["media"] == "Xbox One" or value["media"] == "Switch") 
	and filter_titles_tables.game_rating[ value["rating"]] 
	and filter_titles_tables.media["Games"] == true then
		if last_hit_filter_tag == "Thinned" then
			if (value["status"] == last_hit_filter_tag or value[ "status" ] == "DNR") then
				result = true
			else
				result = false
			end
		else 
			if value["status"] == last_hit_filter_tag then
				result = true
			else
				result = false
			end
		end
	else
		result = false
	end
	return result
end

filter_by_all_tab = function( value )
	local result = false
	if value["media"] ~= nil
	and filter_titles_tables.media[value["media"]] == true
	and filter_titles_tables.movie_rating[value["rating"]] == true then
		result = true
	elseif (value["media"] == "Wii U" or value["media"] == "Wii" or value["media"] == "PC" or value["media"] == "DS" or value["media"] == "PSP" or value["media"] == "PS4" or value["media"] == "PS3" or value["media"] == "PS2" or value["media"] == "Xbox 360" or value["media"] == "Xbox One" or value["media"] == "Switch") 
	and filter_titles_tables.game_rating[ value["rating"]] 
	and filter_titles_tables.media["Games"] == true then
		result = true
	else
		result = false
	end
	return result
end

showinventory = function(entry_point)
	local filter_criteria = kernel.getactortag( InventoryViewFilter )
	if not enable_filters then
		Inventory_Table = filtertable(
			Inventory_All_Table,
			function( value )
				local result = false
				if filter_criteria ~= "filter" and filter_criteria ~= "Thinned" and filter_criteria ~= "Unmatched" then
					if value["status"] == filter_criteria
					or filter_criteria == "ALL" then
						result = true
					end
				elseif filter_criteria == "Thinned" then
					local s = value["status"]
					if s ~= nil and
					(s == "Thinned" or s == "PR1" or s == "PR2" or s == "DNR") then
						result = true
					end
				elseif filter_criteria == "Unmatched" then
					result = value["unmatched"]
				else
					InventorySortBy = "sort_by_title"
					result = filter_by_title( value )
				end
				return result
			end)			
	else 
		Inventory_filtered_Table = filtertable(
			Inventory_All_Table,
			function( value )	
				checkalpharesult = false
				result = false
				if #alpha_table > 0 then
					checkalpharesult = false
					InventorySortBy = "sort_by_title"
					checkalpharesult = filter_by_title( value )
					if checkalpharesult == true and filter_criteria ~= "ALL"then
						result = filter_by_tabs( value )
					elseif checkalpharesult == true and filter_criteria == "ALL" then
						result = filter_by_all_tab( value )
					end
				else
					if filter_criteria ~= "ALL" then
						result = filter_by_tabs( value )
					else
						if select_all_tab then 
							result = true
						else
							result = filter_by_all_tab( value )
						end
					end
				end
				return result
			end)
		Inventory_Table = Inventory_filtered_Table
	end

	local missing_msg = nil
	local count_msg = nil
	if #Inventory_Table <= 0 then
		missing_msg = "Machine has no " .. filter_criteria .. " discs"
		if enable_filters then
			missing_msg = "No matching titles found"
		elseif filter_criteria == "ALL" then
			missing_msg = "Machine has no discs"
		end
	else
		count_msg = "Total " .. kernel.getactortext(InventoryViewFilter) .. " Discs: " .. #Inventory_Table
		if filter_criteria == "ALL" then
			count_msg = "Total Discs: " ..  #Inventory_Table
		end
	end
	local count = 0
	for i, v in pairs(check_box_state ) do
		count = count + 1
	end
	if count > 0 then
		kernel.setactorforegroundcolor( "remove_disc_button", "White" )
		kernel.enableactor( "remove_disc_button" )
		kernel.setactortext( "selected_status", "Selected: " .. count)
	else
		kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
		kernel.disableactor( "remove_disc_button" )
		kernel.setactortext( "selected_status", nil )
	end
	kernel.setactortext("count_label", count_msg)

	if filter_criteria ~= "filter" then
		local last_hit_x = kernel.getactorx(InventoryViewFilter)
		local last_hit_y = kernel.getactory(InventoryViewFilter)
		local last_hit_width = kernel.getactorwidth(InventoryViewFilter)
		local pointer_width = kernel.getactorwidth("inventory_filter")
		local pointer_height = kernel.getactorheight("inventory_filter")
		local x = (last_hit_width/2 + last_hit_x) - (pointer_width/2)
		local y = last_hit_y - pointer_height
		kernel.setactorlocation("inventory_filter", x, y )
	else
		local x = ((140/2) + 864) - (38/2)
		local y = 76 - 27
		kernel.setactorlocation("inventory_filter", x, y )
	end

	Inventory_Scene_Manager:clear()
	Inventory_Pager:reset()
	if entry_point ~= nil then
		sort_columns = {}
		do_multi_sort_on(default_sort_column)
	else
		do_multi_sort_on()
	end
	show_table(
		Inventory_Scene_Manager,
		Inventory_Pager,
		Inventory_Table,
		Inventory_Config,
		missing_msg)
end

getinventorycount = function( disc_type )
	local t = filtertable(
		Inventory_All_Table,
		function( value )
			local result = false
			if value["status"] == disc_type then
				result = true
			end
			return result
		end )
	return #t
end

getinventorycountbyproductid = function( product_id )
	local result = {}
	local product = ProductToBarcode[product_id]
	if product ~= nil then
		for i, v in pairs( product ) do
			result[#result+1] = v
		end
	end
	return #result
end

local is_qlm_deck = function( deck ) 
	if qlm_deck == nil or qlm_deck == 0 then
		return false
	end
	return tostring( qlm_deck ) == tostring( deck ) 
end

getunknowntitlecount = function()
	local unknown_title_count = 0
	for pos, disc in BarcodePosition:pairs() do
		local fields = {}
		string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields,values) end )
		if false == is_qlm_deck( fields[1] ) then
			if disc["status"] == "Unknown Title" then
				unknown_title_count = unknown_title_count + 1
			end
		end
	end
	return unknown_title_count
end

function HideSelectAllButton()
	kernel.hideactor( "select_button" )
	kernel.setactorx( "title_label", 90 )
	kernel.setactorx( "title_sort_arrow",128 )
end

function ShowSelectAllButton()
	kernel.showactor( "select_button" )
	kernel.enableactor( "select_button" )
	kernel.setactorx( "title_label", 197 )
	kernel.setactorx( "title_sort_arrow", 235 )
end