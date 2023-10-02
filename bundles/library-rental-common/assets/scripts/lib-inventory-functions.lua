--
-- lib-inventory-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local sync_inventory_functors = {}

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

-- converted to InventoryHelper.MakeBarcodePositionKey
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

local function reset_barcode_position_status( barcode, status )
    if kernel.is_barcode_reserved(barcode) then
		return "Reserved"
    else	
	    return status
    end
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

function create_or_update_product_catalog_entry( product_id, history )
	local master = kernel.get_product_catalog_title(product_id)
	
	local function in_stock( product_id, history )
		return history.on_hand_count > 0
	end
	local function product_is_available()
		if master == nil then return false end
		return get_current_date_string( "%4d%02d%02d%02d%02d" ) > master.merchandise_date
	end

	local product_catalog = ProductCatalog[product_id] or { product_id = product_id }
	product_catalog.in_stock = in_stock( product_id, history )
	product_catalog.is_available = product_is_available()
	product_catalog.product_type_name = nil  -- product_type_name is never used
	product_catalog.product_family_name = nil  -- product_family_name is never used
	
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
		if barcode_to_product.product_id ~= nil and kernel.get_product_catalog_title(barcode_to_product.product_id) ~= nil then
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

	local new_status = kernel.is_barcode_reserved( barcode ) and "Reserved" or item_status_based_on_barcode_to_product_and_barcode( barcode_to_product, barcode )
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
	
	local new_status = kernel.is_barcode_reserved( barcode ) and "Reserved" or item_status_based_on_barcode_to_product_and_barcode( barcode_to_product, barcode )
	local reset_status = reset_barcode_position_status( barcode, new_status )
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

sync_inventory_functors["ItemReturned"] = sync_inventory_item_returned
sync_inventory_functors["ItemVended"] = sync_inventory_item_vended
sync_inventory_functors["SoftSync"] = sync_inventory_soft_sync
sync_inventory_functors["InventoryState"] = sync_inventory_soft_sync
sync_inventory_functors["NonThinRemoved"] = sync_inventory_item_vended
sync_inventory_functors["ThinTransferSuccessful"] = sync_inventory_item_vended
sync_inventory_functors["QlmUnloadPutIntoDrum"] = sync_inventory_item_returned

-- converted to InventoryHelper.SyncInventory()
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
					if status == "Reserved" and rentable_barcode_count == 1 and kernel.is_product_in_carousel( product_id ) then
						kernel.set_application_state_value("NeedToReloadCarousel", true)
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

kernel.register_update_barcode_status_callback(update_barcode_status)

function getdistincttitlescount()
	local count = 0
	for _, v in ProductCatalog:pairs() do
		if v.in_stock ~= nil and v.in_stock == true then
			count = count + 1
		end
	end
	return count
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
							if not kernel.is_barcode_reserved( v.item_id ) then
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