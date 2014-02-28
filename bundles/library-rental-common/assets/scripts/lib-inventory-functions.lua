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
		and (v.status == "Known" or v.status == "Thinned")
		and UnplayableProduct[ k ] == nil
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

function create_or_update_product_catalog_entry( product_id, history )
	local master = MasterProductCatalog[product_id]
	local function can_be_sold( product_id, history )
		if master == nil then return false end
		if not master.sell_thru then return false end
		if master.release_date == nil then return false end

		local now = get_current_date_string( "%4d%02d%02d" )
		-- Disc must be 30 days past release date (Does not apply for Games)
		if master.product_type_id <= 2 then
			local since_release = kernel.getelapseddays( master.release_date, now )
			if since_release ~= nil and since_release < 30 then return false end
		end

		-- Mingle #291 special exception for last copy of a thinned disc
		if history.on_hand_count == 1 then
			local barcodes_table = ProductToBarcode[product_id] or {}
			for each_barcode, cached_info in pairs( barcodes_table ) do
				if cached_info ~= nil then
					if cached_info.status == "Thinned"
					and UnplayableProduct[each_barcode] == nil then
						return true
					end
				end
			end
		end

		-- Special Out for Rent (OFR) rules apply to ensure adequate rental stock
		if history == nil
		or history.last_max_ofr == nil
		or history.created_date == nil
		or history.sold_out ~= nil then
			return false
		end

		-- if the created date is less than seven days, then go ahead and let the sell go thru;
		-- if the created date is seven days or greater, then check on hand against last max ofr.
		if kernel.getelapseddays( history.created_date, now ) < 7 then return true end

		-- we do not have enough on hand to meet last week's demand
		-- rule is that we do not count actual out for rent copies, only on-hand copies
		if history.on_hand_count <= history.last_max_ofr then return false end

		return true
	end
	local function product_is_available()
		if master == nil then return false end
		return get_current_date_string( "%4d%02d%02d%02d%02d" ) > master.release_date or StoreManager.PricingManager:is_early_released( master )
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
	product_catalog.in_stock = history.on_hand_count > 0
	product_catalog.can_be_sold = can_be_sold( product_id, history )
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
		if barcode_to_product.status ~= nil and 
		(barcode_to_product.status == "Unknown Title" or barcode_to_product.status == "Known") then
			-- Known or Unknown get reset but DO NOT override Thinned, Damaged, or Reserved
			if barcode_to_product.product_id ~= nil and MasterProductCatalog[barcode_to_product.product_id] ~= nil then
				if use_rapid_thin_file() == true 
				and RapidThinBarcodes[ barcode ] ~= nil then
					status = RapidThinBarcodes[ barcode ].status
				else
					status = "Known"
				end
			else
				status = "Unknown Title"
			end
		else
			status = barcode_to_product.status
		end
		
		if status == "Thinned"
		and RebalancesUnThinInuse ~= nil 
		and RebalancesUnThinInuse[ barcode ] ~= nil then
			kernel.log(" > Unthin detected in item_status_based_on_barcode_to_product_and_barcode: " .. tostring( barcode ) )
			status = "Known"
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

	-- This is now required even on a return thanks to Unplayable Returns
	if history.on_hand_count == 0 then 
		local current_date = get_current_date_string( "%4d%02d%02d" )
		if history.sold_out == nil then history.sold_out = current_date end
		if history.last_date_out_of_stock == nil then history.last_date_out_of_stock = current_date end
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

	-- This is now required even on a return thanks to Unplayable Returns
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
	for k, v in UnplayableProduct:pairs() do
		used_barcodes[k] = true
	end
	
	-- Get a list of barcodes in the RebalanceOut list
	rebalances_raw = get_rebalance_barcodes_raw()
	
	for k, v in ipairs( products ) do
		if v.barcode == nil then
			local work_table = {}
			local in_kiosk_and_not_available = false
			local barcodes_table = ProductToBarcode[v.product_id] or {}
			local title_rebalances = {}
			local num_title_rebalances = 0
			for each_barcode, cached_info in pairs( barcodes_table ) do
				if cached_info ~= nil then
					if used_barcodes[each_barcode] == nil
					and (cached_info.status == "Known" or cached_info.status == "Thinned") then
						local disc_info = BarcodeToProduct[each_barcode] or { total_rental_count = 0 }
						table.insert(
							work_table,
							{ barcode = each_barcode, turns = disc_info.total_rental_count } )
						if rebalances_raw[ each_barcode ] == true then
							title_rebalances[ tostring(each_barcode) ] = true
							num_title_rebalances = num_title_rebalances + 1
						end
					else
						in_kiosk_and_not_available = true
					end
				else
					kernel.log( ">  WARNING: Missing disc info in ProductToBarcode for barcode: " .. each_barcode )
				end
			end
			if #work_table > 0 then
				table.sort(
					work_table,
					function( a, b ) return a.turns < b.turns end )
				local barcode = ""
				if v.action == "purchase" then
					barcode = work_table[#work_table].barcode
				else
					barcode = work_table[1].barcode

					-- if the selected barcode is a rebalance, try to assign a barcode not in the rebalance list
					if rebalances_raw[ barcode ] == true
					and num_title_rebalances < #work_table then
						for key, work_table_item in ipairs( work_table ) do
							if title_rebalances[ tostring(work_table_item.barcode) ] == nil then
								barcode = work_table_item.barcode
								break
							end
						end
					end
				end
				table.insert( barcodes_to_vend,	barcode )
				v.barcode = barcode
				v.status = "ProductSelected"
				used_barcodes[barcode] = true
				kernel.log( ">  Barcode " .. barcode .. " assigned to product " .. tostring( v.product_id ) )
			elseif in_kiosk_and_not_available then
				kernel.log( ">  WARNING: Last Barcode Not Reservable: " .. tostring( v.product_id ) )
				v.status = "ProductSelected"
			else
				kernel.log( ">  WARNING: No vendable barcodes found for product: " .. tostring( v.product_id ) )
				v.status = "InventoryError"
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
	local rapidthinsvalid = use_rapid_thin_file()
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
				if v.status == "Known" or v.status == "Thinned" then
					rentable_barcode_count = rentable_barcode_count + 1
				end
			end
			for _, v in ipairs( barcodes ) do
				local barcode_info = product_info[v]
				if status == "Known" then
					-- Known is passed generically for expired reservations, use the CORRECT status instead
					if rapidthinsvalid == true and RapidThinBarcodes[v] ~= nil then
						status = RapidThinBarcodes[v].status
					else
						if BarcodeToProduct[v] ~= nil then
							status = BarcodeToProduct[v].status
						end
					end
				end
				if status == "Thinned"
				and RebalancesUnThinInuse ~= nil 
				and RebalancesUnThinInuse[ v ] ~= nil then
					kernel.log(" > Unthin detected in update_barcode_status: " .. tostring( v ) )
					status = "Known"
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

function use_rapid_thin_file()
	if InventoryVersion ~= nil
	and InventoryVersion.generated_on.date ~= nil
	and RapidThinVersion ~= nil
	and RapidThinVersion.generated_on ~= nil 
	and kernel.datecompare( 
		kernel.datetimeparsefromstringdate( RapidThinVersion.generated_on.date ), 
		kernel.datetimeparsefromstringdate( InventoryVersion.generated_on.date ) 
		) > 0
	and RapidThinBarcodes:getsize() > 0 then
		return true
	else
		return false
	end
end

function get_rebalance_barcodes( inside_vmz, outside_vmz, on_complete )
	local rebalancesoutpath = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceOut.json.inuse"
	local rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesoutpath, StoreManager ~= nil and StoreManager:get_number() or nil )
	barcodes_vmz_status ( inside_vmz, outside_vmz, rebalance_out_inuse_data, on_complete )
end

function get_rebalance_barcodes_raw()
	local rebalancesoutpath = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceOut.json.inuse"
	local rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesoutpath, StoreManager ~= nil and StoreManager:get_number() or nil )
	local raw_data = {}

	if rebalance_out_inuse_data ~= nil then
		for k, v in ipairs(rebalance_out_inuse_data) do
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
		local all_rebalances = {}
		for _,v in ipairs( barcode_list ) do
			local row = {}
			row.barcode = v["Barcode"] or v["barcode"]
			table.insert( all_rebalances, row )
		end
		if #all_rebalances > 0 then
			hardware_manager:get_barcode_vmz_position( all_rebalances, 
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