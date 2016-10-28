--
-- lib-inventory-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


local sync_inventory_functors = {}

local function find_or_build_product_catalog_entry( product_id )
	local product = ProductCatalog[product_id]
	if product ~= nil then return product end
	local master = MasterProductCatalog[product_id]
	if master ~= nil then return { product_id = master.product_id } end
	return nil
end

local function product_to_barcode_entry_count( barcodes )
	local count = 0
	for k, v in pairs( barcodes ) do
		if v ~= nil 
		and (v.status == "Known" or v.status == "Thinned" or v.status == "Redeployment" or v.status == "Rebalancing")
		then 
			count = count + 1 
		end
	end
	return count
end

local function make_barcode_position_key( job_result )
	return tostring( job_result.deck ) .. "@" .. tostring( job_result.slot )
end

local function remove_barcode_position_barcode( slot, vended_barcode )
	if  slot ~= nil
	and slot.barcode ~= nil
	and slot.barcode ~= vended_barcode
	and slot.barcode ~= "EMPTY" then
		remove_barcode( slot.barcode )
	end
end

local function reset_barcode_position_status( barcode, job_result, status )
	for _, account_reservations in Reservations:pairs() do
		for _, reservation in pairs(account_reservations) do
			for _, barcode_list in pairs( reservation.barcodes ) do
				for _, resbarcode in ipairs( barcode_list ) do
					if( tostring( barcode ) == tostring( resbarcode ) ) then
						return "Reserved"
					end
				end
			end
		end
	end
	
	return status
end

local function create_or_update_product_to_barcode_entry( product_id, barcode, status )
	local barcodes = ProductToBarcode[product_id] or {}
    if type(barcodes) ~= "table" then
        kernel.log("Error in create_or_update_product_to_barcode_entry.  ProductToBarcode entry is not a table.  product_id:" .. tostring(product_id) .. " barcode: " .. tostring(barcode) .. " status: " ..tostring(status))
        barcodes = {}
    end
	if barcodes[barcode] == nil then barcodes[barcode] = {} end
	if barcodes[barcode].status == nil then barcodes[barcode].status = status end
	if barcodes[barcode].status ~= "Reserved" and barcodes[barcode].status ~= status then barcodes[barcode].status = status end
	if barcodes[barcode].total_rental_count ~= nil then barcodes[barcode].total_rental_count = nil end
	ProductToBarcode[product_id] = barcodes
	return barcodes
end

function update_game_has_content_registry_keys( product_type )
	if product_type ~= nil 
	and product_type.product_type_name ~= nil then
		if product_type.product_type_name == 'PS3' and GameEnablePS3 == true and GameHasContentPS3 == false then
			kernel.log( "Found Content PS3" )
			kernel.setmachinevalue( "Store", "GameHasContentPS3", "true" )
			GameHasContentPS3 = true
		elseif  product_type.product_type_name == 'PS4' and GameEnablePS4 == true and GameHasContentPS4 == false then
			kernel.log( "Found Content PS4" )
			kernel.setmachinevalue( "Store", "GameHasContentPS4", "true" )
			GameHasContentPS4 = true
		elseif product_type.product_type_name == 'Wii' and GameEnableWii == true and GameHasContentWii == false then
			kernel.log( "Found Content Wii" )
			kernel.setmachinevalue( "Store", "GameHasContentWii", "true" )
			GameHasContentWii = true
		elseif product_type.product_type_name == 'Wii U' and GameEnableWiiU == true and GameHasContentWiiU == false then
			kernel.log( "Found Content Wii U" )
			kernel.setmachinevalue( "Store", "GameHasContentWiiU", "true" )
			GameHasContentWiiU = true
		elseif product_type.product_type_name == 'Xbox 360' and GameEnableXbox360 == true and GameHasContentXbox360 == false then 
			kernel.log( "Found Content Xbox 360" )
			kernel.setmachinevalue( "Store", "GameHasContentXbox360", "true" )
			GameHasContentXbox360 = true
		elseif product_type.product_type_name == 'Xbox One' and GameEnableXboxOne == true and GameHasContentXboxOne == false then
			kernel.log( "Found Content Xbox One" )
			kernel.setmachinevalue( "Store", "GameHasContentXboxOne", "true" )
			GameHasContentXboxOne = true
		end
	end 
end

function is_multi_disc( product_id )
	return product_id ~= nil and MultiDiscXREF[product_id] ~= nil and MultiDiscXREF[product_id].product_group_id ~= nil and ProductGroup[MultiDiscXREF[product_id].product_group_id] ~= nil and ProductGroup[MultiDiscXREF[product_id].product_group_id].product_id_list ~= nil and #ProductGroup[MultiDiscXREF[product_id].product_group_id].product_id_list > 0
end

function get_all_unmatched_multi_disc_barcodes()
	local all_unmatched_multi_disc_barcodes = {}
	
	local product_group_ids = {}
	for _, v in MultiDiscXREF:pairs() do
		if v.product_group_id ~= nil and product_group_ids[v.product_group_id] == nil then
			product_group_ids[v.product_group_id] = v.product_group_id
		end
	end
	
	for _, product_group_id in pairs( product_group_ids ) do
		if ProductGroup[product_group_id] ~= nil and ProductGroup[product_group_id].product_id_list ~= nil then
			local product_ids_and_barcodes = {}
			
			local product_id_list = ProductGroup[product_group_id].product_id_list
			for _, product_id in pairs( product_id_list ) do
				local barcodes_table = {}
				
				local barcodes = ProductToBarcode[product_id]
				if barcodes ~= nil then
					for barcode, _ in pairs( barcodes ) do
						table.insert( barcodes_table, barcode )
					end
				end
				
				table.sort(
					barcodes_table,
					function( a, b )
						local a_total_rental_count = 0
						local b_total_rental_count = 0
						
						if BarcodeToProduct[a] ~= nil and BarcodeToProduct[a].total_rental_count ~= nil then
							a_total_rental_count = BarcodeToProduct[a].total_rental_count
						end
						
						if BarcodeToProduct[b] ~= nil and BarcodeToProduct[b].total_rental_count ~= nil then
							b_total_rental_count = BarcodeToProduct[b].total_rental_count
						end
						
						return a_total_rental_count < b_total_rental_count
					end
				)
				
				table.insert( product_ids_and_barcodes, { product_id = product_id, barcodes = barcodes_table } )
			end
			
			local min_number_of_barcodes = 0
			if product_ids_and_barcodes[1] ~= nil and product_ids_and_barcodes[1].barcodes ~= nil then
				min_number_of_barcodes = #product_ids_and_barcodes[1].barcodes
			end
			
			local max_number_of_barcodes = 0
			
			for _, product_id_and_barcodes in ipairs( product_ids_and_barcodes ) do
				if #product_id_and_barcodes.barcodes < min_number_of_barcodes then
					min_number_of_barcodes = #product_id_and_barcodes.barcodes
				end
				if #product_id_and_barcodes.barcodes > max_number_of_barcodes then
					max_number_of_barcodes = #product_id_and_barcodes.barcodes
				end
			end
			
			if min_number_of_barcodes < max_number_of_barcodes then
				for i = min_number_of_barcodes + 1, max_number_of_barcodes do
					for _, product_id_and_barcodes in ipairs( product_ids_and_barcodes ) do
						if product_id_and_barcodes.barcodes ~= nil and product_id_and_barcodes.barcodes[i] ~= nil then
							table.insert( all_unmatched_multi_disc_barcodes, product_id_and_barcodes.barcodes[i] )
						end
					end
				end
			end
		end
	end
	return all_unmatched_multi_disc_barcodes
end

function create_or_update_product_catalog_entry( product_id, history )
	local master = MasterProductCatalog[product_id]
	
	local function in_stock( product_id, history )
		if is_multi_disc( product_id ) == true then
			for k, v in pairs( ProductGroup[MultiDiscXREF[product_id].product_group_id].product_id_list ) do
				local current_history = ProductHistory[v]
				if current_history == nil or current_history.on_hand_count == nil or current_history.on_hand_count <= 0 then
					return false
				end
			end
			return true
		else
			return history.on_hand_count > 0
		end
	end
	local function product_is_available()
		if master == nil then return false end
		return get_current_date_string( "%4d%02d%02d%02d%02d" ) > master.merchandise_date or StoreManager.PricingManager:is_early_released( master )
	end
	local function get_type_name()
		if master == nil then return nil end
		local master_type = ProductTypes[ master.product_type_id ]
		if master_type == nil then return nil end
		return master_type.product_type_name
	end
	local function get_family_name()
		if master == nil then return nil end
		local master_type = ProductTypes[ master.product_type_id ]
		if master_type == nil or master_type.product_family_id == nil then return nil end
		local master_family = ProductFamilies[ master_type.product_family_id ]
		if master_family == nil then return nil end
		return master_family.product_family_name
	end

	local product_catalog = ProductCatalog[product_id] or { product_id = product_id }
	product_catalog.in_stock = in_stock( product_id, history )
	product_catalog.is_available = product_is_available()
	product_catalog.product_type_name = get_type_name()
	product_catalog.product_family_name = get_family_name()
	
	ProductCatalog[product_id] = product_catalog
end

local function create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )
	local history = ProductHistory[product_id]
	if history == nil then history = { product_id = product_id } end
	if history.ofr_count == nil then history.ofr_count = 0 end
	history.on_hand_count = product_to_barcode_entry_count( barcodes )
	if history.on_hand_count > 0 then history.thinned_out = nil end
	if history.last_max_ofr == nil then history.last_max_ofr = 1 end
	if history.last_max_ofr_date == nil then history.last_max_ofr_date = get_current_date_string( "%4d%02d%02d" ) end
	if history.current_max_ofr == nil then history.current_max_ofr = 0 end
	if history.created_date == nil then history.created_date = get_current_date_string( "%4d%02d%02d" ) end
	ProductHistory[product_id] = history --forces commit
	return history
end

local function item_status_based_on_barcode_to_product_and_barcode( barcode_to_product, barcode )
	local status = nil
	
	if barcode == nil or barcode == "UNKNOWN" or barcode == "EMPTY" or string.upper( barcode ) == "REDBOX" then 
		status = "Unknown"
	elseif barcode_to_product == nil then
		status = "Unknown Title"
	else
		if barcode_to_product.product_id ~= nil and MasterProductCatalog[barcode_to_product.product_id] ~= nil then
			status = get_rapid_status( barcode, nil )
            if status == nil then
                if barcode_to_product.status ~= nil  and ( barcode_to_product.status == "Rebalancing" or barcode_to_product.status == "Redeployment" or barcode_to_product.status == "Thinned")   then		
				    status = "Known"
			    else
				    status = barcode_to_product.status
			    end
            end
		else
			status = "Unknown Title"
		end	
		if VerboseLogging then
		  kernel.log("Barcode_to_product_and_barcode: " .. barcode .. " BarcodeToProduct status: " .. tostring(barcode_to_product.status) .. " return status: " .. status )
		end		
	end

	return status
end

local function clear_product_history_sold_out_after_seven_days( history, current_date )
	if history.sold_out == nil then return end
	if current_date == nil then current_date = get_current_date_string( "%4d%02d%02d" ) end
	local days_elapsed = kernel.getelapseddays( history.sold_out, current_date )
	if days_elapsed >= 7 then history.sold_out = nil end
	ProductHistory[history.product_id] = history --forces commit
end

local function set_last_max_ofr_fields_if_after_seven_days( history, current_date )
	if current_date == nil then 
		current_date = get_current_date_string( "%4d%02d%02d" ) 
	end
	if history.last_max_ofr_date == nil then 
		history.last_max_ofr_date = current_date 
	end
	local days_elapsed = kernel.getelapseddays( history.last_max_ofr_date, current_date )
	if days_elapsed >= 7 then
		history.last_max_ofr = history.current_max_ofr or 0
		history.last_max_ofr_date = current_date
		history.current_max_ofr = 0
		history.ofr_count = 0
	end
	ProductHistory[history.product_id] = history --forces commit
end

local function sync_inventory_item_returned( job_result )
	if job_result ~= nil then
		kernel.log( "Sync Inventory - ItemReturned. JobResult: " )
		kernel.log( job_result )
	end
	
	local barcode = job_result.item_id

	local key = make_barcode_position_key( job_result )
	local barcode_position = BarcodePosition[key]
	remove_barcode_position_barcode( barcode_position, barcode )

	local barcode_to_product = BarcodeToProduct[barcode]

	local new_status = barcodeisreserved( barcode ) and "Reserved" or item_status_based_on_barcode_to_product_and_barcode( barcode_to_product, barcode )
	BarcodePosition[key] = { ["barcode"] = barcode, ["status"] = new_status }

	-- if couldn't read barcode or barcode not in barcodetoproduct
	-- (master list of all barcodes) then just fix the deck/slot
	if barcode_to_product == nil then return end

	local product_id = barcode_to_product.product_id

	local barcodes = create_or_update_product_to_barcode_entry( product_id, barcode, new_status )

	local history = create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )
	if history.ofr_count > 0 then history.ofr_count = history.ofr_count - 1 end

	if history.on_hand_count == 0 then 
		local current_date = get_current_date_string( "%4d%02d%02d" )
		if history.sold_out == nil then history.sold_out = current_date end
		if history.last_date_out_of_stock == nil then history.last_date_out_of_stock = current_date end
	end

	if RemovedQueue[ barcode ] ~= nil then
		kernel.log( ">  " .. barcode .. " removed from RemovedQueue" )
		RemovedQueue:remove( barcode )
	end

	create_or_update_product_catalog_entry( product_id, history )
	ProductHistory[product_id] = history --forces commit
end

local function sync_inventory_item_vended( job_result )
	if job_result ~= nil then
		kernel.log( "Sync Inventory - ItemVended. JobResult: " )
		kernel.log( job_result )
	end
	
	local barcode = job_result.item_id

	job_result.item_id = "EMPTY"

	local key = make_barcode_position_key( job_result )
	local barcode_position = BarcodePosition[key]
	remove_barcode_position_barcode( barcode_position, "EMPTY" )
    BarcodePosition[key] = { barcode = "EMPTY", status = "Unknown" }

	local barcode_to_product = BarcodeToProduct[barcode]

	if job_result.code == "ThinTransferSuccessful"
	or job_result.code == "NonThinRemoved" then
		-- This needs to occur even if we can't match the barcode to a product
		RemovedQueue[ barcode ] = {["barcode"] = barcode, ["status"] = "Success" }
	end

	if barcode_to_product ~= nil then 

		local product_id = barcode_to_product.product_id

		local barcodes = ProductToBarcode[product_id] or {}
		if barcodes[barcode] ~= nil then
			remove_barcode( barcode )
			barcodes = ProductToBarcode[product_id] or {}
		end

		local history = create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )

		if history.ofr_count == nil then history.ofr_count = 0 end
		if job_result.code == "ItemVended" then
			history.ofr_count = history.ofr_count + 1
		end

		if history.current_max_ofr == nil then 
			history.current_max_ofr = history.ofr_count 
		end
		if history.ofr_count > history.current_max_ofr then 
			history.current_max_ofr = history.ofr_count	
		end

		if history.on_hand_count == 0 then 
			local current_date = get_current_date_string( "%4d%02d%02d" )
			history.sold_out = current_date 
			history.last_date_out_of_stock = current_date
			-- Titles which go to zero due to thinning/FMA removal are not considered "sold out"
			if ( job_result.code == "ThinTransferSuccessful" or job_result.code == "NonThinRemoved" )
			and history.thinned_out ~= true then
				history.thinned_out = true
			end
		end

		create_or_update_product_catalog_entry( product_id, history )
		ProductHistory[product_id] = history --forces commit

	end

	job_result.item_id = barcode --This needs to get reset for the QLM Alert

end

local function sync_inventory_soft_sync( job_result )
	local barcode = job_result.item_id
	local metadata = job_result.meta_data
	

	local barcode_to_product = BarcodeToProduct[barcode]
	local key = make_barcode_position_key( job_result )
	local barcode_position = BarcodePosition[key]
	
	
	
		--kernel.log("> soft sync item; barcode = " .. tostring( barcode ) .. " at " .. tostring( key ) .. ", metadata=" .. tostring(metadata) )
	if barcode ~= nil and  metadata ~= nil and string.find( tostring(metadata), "DUPLICATE" ) ~= nil then
		-- HAL believes it has two copies of the same disc
		DuplicateBarcodes:clear()
		kernel.log("> DUPLICATE reported by HAL: " .. barcode .. " at " ..tostring( key) )
		for prev_pos, disc in BarcodePosition:pairs() do
			if disc["barcode"] == barcode and tostring( key ) ~= tostring( prev_pos ) and DuplicateBarcodes[barcode] == nil then
				kernel.log( "Found duplicate barcode: " .. tostring( barcode ) .. "original position: " .. tostring( prev_pos ) ..
					" duplicate position: " .. tostring ( key ) )
				DuplicateBarcodes[barcode] = { ["original_key"] = prev_pos, ["duplicate_key"] = key }
			end
		end
		--fix_duplicate_barcodes( barcode, key )
		return  
	end

	remove_barcode_position_barcode( barcode_position, barcode )
	
	local new_status = barcodeisreserved( barcode ) and "Reserved" or item_status_based_on_barcode_to_product_and_barcode( barcode_to_product, barcode )
	local reset_status = reset_barcode_position_status( barcode, job_result, new_status )
	BarcodePosition[key] = { ["barcode"] = barcode, ["status"] = reset_status }

	-- if couldn't read barcode or barcode not in barcodetoproduct
	-- (master list of all barcodes) then just fix the deck/slot
	if barcode_to_product == nil then return end

	local product_id = barcode_to_product.product_id

	local barcodes = create_or_update_product_to_barcode_entry( product_id, barcode, reset_status )

	local history = create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )
	clear_product_history_sold_out_after_seven_days( history, current_date )
	set_last_max_ofr_fields_if_after_seven_days( history, current_date )

	if history.on_hand_count == 0 then 
		local current_date = get_current_date_string( "%4d%02d%02d" )
		if history.sold_out == nil then history.sold_out = current_date end
		if history.last_date_out_of_stock == nil then history.last_date_out_of_stock = current_date end
	end

	create_or_update_product_catalog_entry( product_id, history )
	ProductHistory[product_id] = history --forces commit
end

function get_barcodes_for_products( products, exclude_list )
	kernel.log( "Assigning barcodes to products in cart" )
	local barcodes_to_vend = {}
	local used_barcodes = {}
	products = products or {}
	exclude_list = exclude_list or {}
	for _, v in ipairs( exclude_list ) do
		used_barcodes[v] = true
	end
	
	for k, v in ipairs( products ) do
		if v.barcode == nil then
			local work_table_known = {}
			local work_table_thinned = {}
			local work_table_redeployment = {}
			local work_table_rebalancing = {}
			
			local in_kiosk_and_not_available = false
			local barcodes_table = ProductToBarcode[v.product_id] or {}

			for each_barcode, cached_info in pairs( barcodes_table ) do
				if cached_info ~= nil then
					local disc_info = BarcodeToProduct[each_barcode] or { total_rental_count = 0 }
					if used_barcodes[each_barcode] == nil then
						if cached_info.status == "Known" then
							table.insert( work_table_known, { barcode = each_barcode, turns = disc_info.total_rental_count } )
						elseif cached_info.status == "Thinned" then
							table.insert( work_table_thinned, { barcode = each_barcode, turns = disc_info.total_rental_count } )
						elseif cached_info.status == "Redeployment" then
							table.insert( work_table_redeployment, { barcode = each_barcode, turns = disc_info.total_rental_count } )
						elseif cached_info.status == "Rebalancing" then
							table.insert( work_table_rebalancing, { barcode = each_barcode, turns = disc_info.total_rental_count } )
						end
					else
						in_kiosk_and_not_available = true
					end
				else
					kernel.log( ">  WARNING: Missing disc info in ProductToBarcode for barcode: " .. each_barcode )
				end
			end
			
			local barcode = ""
			if #work_table_known > 0 then
				table.sort(work_table_known, function( a, b ) return a.turns < b.turns end )
				barcode = work_table_known[iif( v.action == "purchase", #work_table_known, 1 )].barcode
			elseif #work_table_thinned > 0 then
				table.sort(work_table_thinned, function( a, b ) return a.turns < b.turns end )
				barcode = work_table_thinned[iif( v.action == "purchase", #work_table_thinned, 1 )].barcode
			elseif #work_table_redeployment > 0 then
				table.sort(work_table_redeployment, function( a, b ) return a.turns < b.turns end )
				barcode = work_table_redeployment[iif( v.action == "purchase", #work_table_redeployment, 1 )].barcode
			elseif #work_table_rebalancing > 0 then
				table.sort(work_table_rebalancing, function( a, b ) return a.turns < b.turns end )
				barcode = work_table_rebalancing[iif( v.action == "purchase", #work_table_rebalancing, 1 )].barcode
			elseif in_kiosk_and_not_available then
				kernel.log( ">  WARNING: Last Barcode Not Reservable: " .. tostring( v.product_id ) )
				v.status = "ProductSelected"
			else
				kernel.log( ">  WARNING: No vendable barcodes found for product: " .. tostring( v.product_id ) )
				v.status = "InventoryError"
			end

			if barcode ~= "" then
				table.insert( barcodes_to_vend,	barcode )
				v.barcode = barcode
				v.status = "ProductSelected"
				used_barcodes[barcode] = true
				kernel.log( ">  Barcode " .. barcode .. " assigned to product " .. tostring( v.product_id ) )
			end
		else
			table.insert( barcodes_to_vend, v.barcode )
			kernel.log( ">  Barcode " .. v.barcode .. " was ALREADY assigned to product " .. tostring( v.product_id ) )
		end
	end
	table.sort(
		barcodes_to_vend,
		function( a, b ) return a > b end )
	return barcodes_to_vend
end

sync_inventory_functors["ItemReturned"] = sync_inventory_item_returned
sync_inventory_functors["ItemVended"] = sync_inventory_item_vended
sync_inventory_functors["SoftSync"] = sync_inventory_soft_sync
sync_inventory_functors["InventoryState"] = sync_inventory_soft_sync
sync_inventory_functors["NonThinRemoved"] = sync_inventory_item_vended
sync_inventory_functors["ThinTransferSuccessful"] = sync_inventory_item_vended
sync_inventory_functors["QlmUnloadPutIntoDrum"] = sync_inventory_item_returned

function sync_inventory( job_result )
	if job_result.deck == 0 or job_result.slot == 0 then 
		if job_result.code ~= "ThinTransferSuccessful" then 
			return 
		end
	end
	
	if sync_inventory_functors[job_result.code] == nil then
		kernel.log(
			"Unhandled job result code: " ..
			tostring(job_result.code) ..
			" passed to sync_inventory!" )
	else
		sync_inventory_functors[job_result.code]( job_result )
	end
end

function add_title_without_barcodes( product_id )
	local barcodes = ProductToBarcode[product_id] or {}
	local history = ProductHistory[product_id] or create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )
	create_or_update_product_catalog_entry( product_id, history )
	ProductHistory[product_id] = history --forces commit
end

function remove_barcode( barcode )
	local barcode_to_product = BarcodeToProduct[barcode]
	if barcode_to_product == nil then return end

	local product_id = barcode_to_product.product_id
	local barcodes = ProductToBarcode[product_id] or {}

	barcodes[barcode] = nil

	local history = create_missing_product_history_set_on_hand_and_ofr_counts( product_id, barcodes )

	create_or_update_product_catalog_entry( product_id, history )
	ProductHistory[product_id] = history --forces commit

	if table_count( barcodes ) == 0 then
		ProductToBarcode:remove( product_id )
	else
		ProductToBarcode[product_id] = barcodes
	end
end

function update_barcode_status( barcodes, status )
	BarcodePosition:reset_keys_cache()
	local product_ids = {}
	for _, v in ipairs( barcodes ) do
		local barcode_info = BarcodeToProduct[v]
		if barcode_info ~= nil then
			table.insert( product_ids, barcode_info.product_id )
		end
	end
	for _, product_id in ipairs( product_ids ) do
		local product_info = ProductToBarcode[product_id]
		if product_info ~= nil then
			local rentable_barcode_count = 0
			for _, v in pairs( product_info ) do
				if v.status == "Known" or v.status == "Thinned" or v.status == "Redeployment" or v.status == "Rebalancing" then
					rentable_barcode_count = rentable_barcode_count + 1
				end
			end
			for _, v in ipairs( barcodes ) do
				local barcode_info = product_info[v]
				if status == "Known" then
					-- Known is passed generically for expired reservations, use the CORRECT status instead
                    status = get_rapid_status( v, nil )
					if status == nil then
						if BarcodeToProduct[v] ~= nil then
							status = BarcodeToProduct[v].status
						else
                            status = "Known"
                        end
					end
				end
				if barcode_info ~= nil then
					barcode_info.status = status
					if status == "Reserved" and rentable_barcode_count == 1 and carousel_contains_item( product_id ) then
						need_to_reload_carousel = true
						kernel.log( string.format( "Carousel will be refreshed after reservation call is completed, since the last copy of product_id: %s was reserved online on this kiosk.", product_id ) )
					end
				end
			end
			ProductToBarcode[product_id] = product_info
		end
		local all_product_barcodes = ProductToBarcode[product_id] or {}
		local history = create_missing_product_history_set_on_hand_and_ofr_counts( product_id, all_product_barcodes )
		create_or_update_product_catalog_entry( product_id, history )
		ProductHistory[product_id] = history --forces commit
	end

	for position_key, position_info in BarcodePosition:pairs() do
		for _, v in ipairs( barcodes ) do
			if position_info.barcode == v then
				position_info.status = status
				BarcodePosition[position_key] = position_info
			end
		end
	end
end

function fix_duplicate_barcodes( new_barcode, new_location_key )
	if new_barcode == nil or new_location_key == nil then return end
	local hardware_manager = newhardwaremanager()
	local job_label = "CLEAR DUPLICATE " .. new_barcode

	-- Limit the number of attempts in case there is a hardare problem
	local prior_job = DuplicateSlotFixes[ new_barcode ]
	local current_date = get_current_date_string( "%4d%02d%02d%02d%02d" )
	if prior_job == nil then
		prior_job = { barcode=new_barcode, last_update=current_date, attempts=0 }
	end
	prior_job.attempts = prior_job.attempts + 1
	if prior_job.attempts >= 4 then
		kernel.log( ">  " .. new_barcode .. " DUP fix max attempts reached, sync will NOT be scheduled." )
		return
	else
		prior_job.last_update = current_date
	end
	DuplicateSlotFixes[ new_barcode ] = prior_job

	local duplicate_locations = {}
	-- do not create the job if it already exists
	local hardwarejobs = hardware_manager:get_hardware_jobs_by_program(
		{"sync-locations" },
		{"Errored", "Garbage" } )
	local job_count = table_count( hardwarejobs )
	if job_count > 0 then
		for _, v in ipairs( hardwarejobs ) do
			--kernel.log( tostring(v.program) .. " " .. tostring( v.label) ) --REMOVE ME
			if v.label ~= nil and string.find( v.label, job_label ) ~= nil then
				kernel.log( "> " .. job_label .. " already queued in HAL" )
				return
			end
		end
	end

	-- add the new location to the list [HAL stack works in reverse, this will get done last]
	local fields2 = {}
	string.gsub(new_location_key, "@*(%w+)@*", function(values) table.insert(fields2,values) end )
	local new_deck = fields2[1]
	local new_slot = fields2[2]
	table.insert(duplicate_locations, { deck=new_deck, slot=new_slot })

	-- discover the old location [this will get done FIRST, which should clear the DUPLICATE condition]
	local old_pos = "(not found)"
	for pos, disc in BarcodePosition:pairs() do
		local barcode = disc["barcode"]
		if new_barcode == barcode and pos ~= new_location_key then
			old_pos = pos
			local fields = {}
			string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields,values) end )
			local old_deck = fields[1]
			local old_slot = fields[2]
			table.insert(duplicate_locations, { deck=old_deck, slot=old_slot })

			break
		end
	end

	-- schedule one job to fix both slots
	local sync_schedule_time = newdatetime()
	sync_schedule_time = sync_schedule_time - "00:05:00"
	job_label = job_label .. " ORIG:" .. old_pos .. " NEW:" .. new_location_key
	kernel.log( "> " .. job_label .. " submitted to HAL")
	hardware_manager:hard_sync_by_location( job_label, sync_schedule_time.backing, duplicate_locations )

end

function getdistincttitlescount()
	local count = 0
	for _, v in ProductCatalog:pairs() do
		if v.in_stock ~= nil and v.in_stock == true then
			count = count + 1
		end
	end
	return count
end

function getreservationscount()
	local count = 0
	for _, account_reservations in Reservations:pairs() do
		for _, reservation in pairs(account_reservations) do
			for _, barcode_list in pairs( reservation.barcodes ) do
				for _, barcode in ipairs( barcode_list ) do
					count = count + 1
				end
			end
		end
	end
	kernel.log("Reserved Count total discs = ".. tostring(count))
	return count
end

function barcodeisreserved( barcode )
	local check_barcode = tostring( barcode )
	for _, account_reservations in Reservations:pairs() do
		for _, reservation in pairs(account_reservations) do
			for _, barcode_list in pairs( reservation.barcodes ) do
				for _, reserved_barcode in ipairs( barcode_list ) do
					if check_barcode == tostring( reserved_barcode) then
						return true
					end
				end
			end
		end
	end
	return false
end

function get_rebalance_barcodes( inside_vmz, outside_vmz, on_complete )
	--local rebalancesoutpath = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceOut.data.inuse"
	--local rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesoutpath, StoreManager ~= nil and StoreManager:get_number() or nil )
	local rebalance_out = kernel.rental_inventory_get_rapidstatus_by_status("Rebalancing")
	barcodes_vmz_status ( inside_vmz, outside_vmz, rebalance_out, on_complete )
end

function get_rebalance_barcodes_raw()
	--local rebalancesoutpath = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceOut.data.inuse"
	--local rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesoutpath, StoreManager ~= nil and StoreManager:get_number() or nil )
	local rebalance_out = kernel.rental_inventory_get_rapidstatus_by_status("Rebalancing")	
	local raw_data = {}

	if rebalance_out ~= nil then
		for k, v in pairs(rebalance_out) do
			raw_data[ tostring(v.Barcode) ] = true
		end
	end
	
	return raw_data
end

function get_redeploy_barcodes_raw()
	local redeploy_out = kernel.rental_inventory_get_rapidstatus_by_status("Redeployment")	
	local raw_data = {}

	if redeploy_out ~= nil then
		for k, v in pairs(redeploy_out) do
			raw_data[ tostring(v.Barcode) ] = true
		end
	end
	
	return raw_data
end

function barcodes_vmz_status ( inside_vmz, outside_vmz, barcode_list, on_complete )
	local hardware_manager = newhardwaremanager()
	local result_list = {}
	if on_complete ~= nil
	and barcode_list ~= nil then
		local indexed_barcodes = {}
		for _,v in pairs( barcode_list ) do
			local row = {}
			row.barcode = v["Barcode"] or v["barcode"]
			table.insert( indexed_barcodes, row )
		end
		if #indexed_barcodes > 0 then
			hardware_manager:get_barcode_vmz_position( indexed_barcodes, 
				function( results )
					local rebalance_count = 0
					for _,v in ipairs( results ) do
						if (inside_vmz == true  and v.code == "BarcodeInVMZ")
						or (outside_vmz == true and v.code == "BarcodeOutsideVMZ") then
							if not barcodeisreserved( v.item_id ) then
								table.insert( result_list, v )
							end
						end
					end
					on_complete( result_list )
				end)
		else
			on_complete( result_list )
		end
	elseif on_complete ~= nil then
		on_complete( result_list )
	end
end

function upgrade_product( product ) 
    if product.title_rollup_product_group_id ~= nil then
	    local group_prod = kernel.rental_get_title_rollup( product.title_rollup_product_group_id )
	    if group_prod ~= nil then
		    return group_prod
	    end
    end
    return product
end

function get_rapid_status( barcode, default_status )
    local barcode_status = BarcodeStatus[barcode]
    if barcode_status ~= nil then
        return barcode_status.status
    end
    local rapid_status = kernel.rental_inventory_get_rapidstatus( barcode )
    if rapid_status ~= nil then
        return rapid_status.status
    end
    return default_status
end