--
-- remove-disc-popup-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local displayed = {}
local item_index = 1
local top_index = 1
local tab_stops = { 350,120,50,50 }
local count = 0
local view_frame = kernel.getviewframe()

if view_frame == nil then
	return
end

local display_stack = newstack()
local items_to_remove = {}
local removed_items = {}
local hardware_manager = newhardwaremanager()
local progress_manager = newprogressmanager()

local close_view = function()
	kernel.callbackenqueue(
		function( r )
				check_box_state={}
				reload_inventory()
				if #removed_items > 0 then
					kernel.healthsvcs_sendauditevent(
						StoreID,
						"FMA_RemoveDiscs",
						username,
						"FMA Bundle",
						table.concat( removed_items, ",") )
						idle_timer:start()
				end

				-- build barcode_list from removed_items
				if DiscActionLogging == true then
					barcode_list = { }
					local numBarcodes = 1
					for i = 1, #removed_items, 1 do
						local item = removed_items[i] --> 1@1 111222333
						local item_split = split(item, " ") --> {"1@1", "111222333"}
						if item_split[2] ~= nil and item_split[2] ~= "UNKNOWN" and item_split[2] ~= "" then
							barcode_list[numBarcodes] = { tostring(item_split[2]), "1" } -- action 0=Inserted  1=Removed
							numBarcodes = numBarcodes + 1
						end
					end
					
					local job_name = "vend"
					if remove_disc_button_pressed_from_tab == "Unmatched" then
						job_name = "MDV_Vend_Unmatched"
					end
					
					-- Send DiscAction Message
					if ( #barcode_list > 0 ) then
						kernel.fmadiscaction(
							StoreID,
							username,
							job_name,
							os.date("%Y%m%d%H%M"), -- YearMonthDatHourMin ie: 201312242359
							barcode_list)
					end
				end
				view_goback()
		end,
		{} )
	end

--helper function for painting items on the panel
local display_items = function( x, y )
  local function get_disc_number( barcode )
     if barcode ~= nil and BarcodeToProduct[barcode] ~= nil and BarcodeToProduct[barcode].product_id ~= nil and MasterProductCatalog[BarcodeToProduct[barcode].product_id] ~= nil and MasterProductCatalog[BarcodeToProduct[barcode].product_id].disc_number ~= nil then
       return MasterProductCatalog[BarcodeToProduct[barcode].product_id].disc_number
     end
     return 1
  end
	
  local stop_y = 522
  local formatted_text
  local name
  local item
  local barcode
  local title
   x = x + 30
  while true do
    item = display_stack:pop()
    if( item == nil ) then
		break
    end
	
	if item.barcode == nil
	or item.barcode == "" then
		barcode = "Unknown"
	else
		barcode = item.barcode
	end
	
    if item.title == nil
    or item.title == "" then
		title = item.status
	else
		title = item.title
	end
	
	if remove_disc_button_pressed_from_tab == "Unmatched" then
		title = tostring( title ) .. " (disc " .. tostring( get_disc_number( barcode ) ) .. ")"
	end
	
	if string.len(title) > 35 then
		title = string.sub(title, 1, 35 ) .. ".."
	end

    formatted_text = title .. "\t".. barcode  .. "\t" ..item.deck .. "\t" .. item.slot .. "\t"
    name = "remove_" .. barcode .. item.deck .. item.slot
    kernel.createactor(
      name,
      false,
      x,
      y,
      708,
      20,
      formatted_text,
      "medium_font",
      nil,
      nil,
      "black",
      nil,
      nil,
      nil,
      nil,
      "Text",
      nil )
    kernel.setactortag( name, formatted_text )
    kernel.setactortabstops(
		name,
		tab_stops )
    y = y + 50
    table.insert(
		displayed,
		{name, barcode, item.deck, item.slot})
    if y >= stop_y then
      break
    end
  end
end

local function product_on_hand_count( barcodes )
	local count = 0
	for _, v in pairs( barcodes ) do
		if v ~= nil and (v.status == "Known" or v.status == "Thinned") then 
			count = count + 1 
		end
	end
	return count
end

local function create_or_update_product_catalog_entry( product_id, history )
	local function can_be_sold( product_id, history )
		local master = MasterProductCatalog[product_id]
		if master == nil then return false end
		if not master.sell_thru then return false end
		-- Currently may only sell DVDs, not blu-ray or games, safety catch for hive data used above
		if master.product_type_id ~= 1 then return false end
		if master.merchandise_date == nil then return false end

		-- Disc must be 30 days past release date
		local now = get_current_date_string( "%4d%02d%02d" )

		local since_release = kernel.getelapseddays( master.merchandise_date, now )
		if since_release ~= nil and since_release < 30 then return false end

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
	local function is_available( product_id )
		local product = MasterProductCatalog[product_id]
		if product == nil then return false end
		return get_current_date_string( "%4d%02d%02d%02d%02d" ) > product.merchandise_date
	end

	local product_catalog = ProductCatalog[product_id] or { product_id = product_id }
	product_catalog.in_stock = history.on_hand_count > 0
	product_catalog.can_be_sold = can_be_sold( product_id, history )
	product_catalog.is_available = is_available( product_id )
	ProductCatalog[product_id] = product_catalog
end


local function remove_disc_from_inventory_tables( deck, slot, barcode )

	-- BarcodePosition
	local deck_slot_key = tostring(deck) .. "@" .. tostring(slot)
	BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }

	local data_row = BarcodeToProduct[barcode]
	if data_row == nil then 
		return 
	end
	local product_id = data_row.product_id

	-- ProductToBarcode
	local barcodes = ProductToBarcode[product_id]
	local on_hand_count = 0
	if barcodes ~= nil then
		barcodes[barcode] = nil
		ProductToBarcode[product_id] = barcodes
		on_hand_count = product_on_hand_count( barcodes )
	end

	-- ProductHistory
	local history = ProductHistory[product_id]
	if history ~= nil then
		history.on_hand_count = on_hand_count
		if on_hand_count == 0 then
			history.sold_out = get_current_date_string( "%4d%02d%02d" )
			history.last_date_out_of_stock = get_current_date_string( "%4d%02d%02d" )
			-- Titles which go to zero due to thinning/FMA removal are not considered "sold out"
			if data_row.status == "Thinned" then
				history.thinned_out = true
			end
		end
		ProductHistory[product_id] = history

		-- ProductCatalog
		create_or_update_product_catalog_entry( product_id, history )
	end
end
-- called by machine status functions defined below
local remove_complete = function( status )
	local current = displayed[item_index]
	local text = kernel.getactortag( current[1] )
	if( text ~= nil) then
		kernel.setactortext(
			current[1],
			text .. status )
	end
	if status == "Please Wait" then
		return
	end

	if status == "Machine Error" then
		return
	end
	local name = current[1]
	local deck = current[3]
	local slot = current[4]
	local barcode = current[2]

	kernel.log( ">  " .. barcode .. " added to RemovedQueue" )
	RemovedQueue[ barcode ] = {["barcode"] = barcode, ["status"] = status }

	if status == "Success" then
		removed_items[#removed_items + 1 ] = deck .. "@" .. slot .. " " .. current[2]
		remove_disc_from_inventory_tables( deck, slot, barcode )
		if name == "Unknown" then
		   kernel.logto("FieldMaintenanceActionLog","Removed :"..name)
		elseif barcode ~= nil then
		   kernel.logto("FieldMaintenanceActionLog","Removed Disc Barcode :"..barcode)
		end		
	end

	if item_index >= 6
	and display_stack:peek() ~= nil then
		local top = displayed[top_index]
		local last_y = kernel.getactory(top[1] )
		local current_y
		for i, _ in ipairs( displayed ) do
			if i > top_index then
				local each = displayed[i]
				current_y = kernel.getactory( each[1] )
				kernel.moveactory( each[1], last_y - current_y )
				last_y = current_y
			end
		end
		kernel.removeactor(top[1])
		top_index = top_index + 1
		display_items(
			0,
			last_y - view_frame["y"] )
	end
	item_index = item_index + 1
	if item_index <= count then
		kernel.setactortext( "progress_label", "Removing " .. item_index .. " of " .. count )
	end
end

idle_timer:stop()
local checked  = {}
for _,v in pairs(check_box_state) do
	checked[#checked + 1] = v
end

table.sort(
	checked,
	function (first,second)
		local firstposition = (1000 * tonumber(first["deck"])) + tonumber(first["slot"])
		local secondposition = (1000 * tonumber(second["deck"])) + tonumber(second["slot"])
		return firstposition > secondposition
	end
)

for k, v in pairs( checked ) do
	count = count + 1
	items_to_remove[count] = {["deck"]=v.deck, ["slot"]=v.slot }
	display_stack:push(v)
end

hardware_manager.hardware_event_responders.ItemNotFound = function()  remove_complete( "Not Found") end
hardware_manager.hardware_event_responders.MachineError = function() remove_complete( "Machine Error" ) end
hardware_manager.hardware_event_responders.ItemVended = function() remove_complete( "Success" ) end
hardware_manager.hardware_event_responders.GripperTimedOut = function() remove_complete( "Hardware Error" ) end
display_items( 0, 125 )
local result = hardware_manager:vend_locations(
				items_to_remove,
				function( status )
					kernel.log( "Remove Disc Popup: job has completed" )
					local current_view = kernel.peekview()
					if  current_view == "remove_disc_popup_view" then
						close_view()
					end
				end,
				function()
					kernel.log( "Remove Disc Popup: job starting" )
					remove_complete( "Please Wait" )
				end)
				
kernel.registeractorhitnotify(
	"cancel_button",
	function()
		kernel.log( "Remove Disc Popup: User pressed cancel" )
		kernel.callbackenqueue(
			function( job_id )
				kernel.hardwareterminatejob( job_id )
			end,
			{ result.job_id } )
		end )

kernel.registeractorhitnotify(
	"close_area_button",
	function()
		kernel.log( "Remove Disc Popup: User pressed close[X] button" )
		kernel.callbackenqueue(
			function( job_id )
				kernel.hardwareterminatejob( job_id )
			end,
			{ result.job_id } )
		end )

kernel.setactortext("progress_label", "Removing " .. item_index .. " of " .. count )

