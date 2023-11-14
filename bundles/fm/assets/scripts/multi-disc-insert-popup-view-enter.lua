--
-- multi-disc-insert-popup-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local empty_slots = EmptySlots

inserted_discs = {}
local inserted_disc_starting_index = 1

local hardware_manager = newhardwaremanager()
local inserted_discs_scene_manager = newscenemanager()
local overlay_progress_manager = newprogressmanager()

local display_inserted_discs = function()
	inserted_discs_scene_manager:clear()
	local counter = 1

	local inserted_discs_reversed = {}
	for i = #inserted_discs, 1, -1 do
		table.insert( inserted_discs_reversed, { title = inserted_discs[i].title, barcode = inserted_discs[i].barcode, deck = inserted_discs[i].deck, slot = inserted_discs[i].slot } )
	end	

	for _, v in ipairs( inserted_discs_reversed ) do
		if counter >= inserted_disc_starting_index and ( counter - inserted_disc_starting_index ) < 10 then
			local title = v.title
			if string.len( title ) > 35 then
				title = string.sub( title, 1, 35 ) .. "..."
			end
			
			inserted_discs_scene_manager:add_actor(
				"multi_disc_insert_disc" .. counter,
				173,
				160 + ( 44 * ( counter - inserted_disc_starting_index ) ),
				726,
				20,
				{ text = tostring( ( #inserted_discs + 1 ) - counter ) .. ".\t" .. title .. "\t" .. v.barcode .. "\t" .. v.deck .. "\t" .. v.slot, font = "medium_font", foregroundColor = "Black", renderOptions = "Text", tabs = { 30, 340, 140, 75 } },
				nil
			)
		end
		
		counter = counter + 1
	end
end

local decrement_and_display_empty_slots = function()
	empty_slots = empty_slots - 1
	kernel.setmacro( "EmptySlotsValue", tostring( empty_slots ) )
end

local scroll_up = function()
	if inserted_disc_starting_index > 1 then
		inserted_disc_starting_index = inserted_disc_starting_index - 1
		display_inserted_discs()
	end
end

local scroll_down = function()
	if inserted_disc_starting_index < #inserted_discs then
		inserted_disc_starting_index = inserted_disc_starting_index + 1
		display_inserted_discs()
	end
end

insert_disc = function()
	idle_timer:reset()
	local result = hardware_manager:return_unknown(
		"FMA-INSERT",
		nil,
		function( job_result )
			for _, v in ipairs( job_result.results ) do
				if v.code == "ItemReturned"
				and v.item_id ~= nil
				and trim( v.item_id ) ~= ""
				and string.lower( v.item_id ) ~= "unknown"
				and string.lower( v.item_id ) ~= "redbox" then
					kernel.invsvcs_return(
						StoreID,
						"ServiceMenu",
						v.item_id,
						v.deck,
						v.slot,
						v.return_time
					)
					
					local deck_slot_string = " Deck: " .. tostring( v.deck ) .. " Slot: " .. tostring( v.slot )
					kernel.healthsvcs_sendauditevent(
						StoreID,
						"FMA_MultiDiscInsert",
						username,
						"FMA Bundle",
						"Barcode: " .. tostring( v.item_id ) .. deck_slot_string
					)
					
					-- Send DiscAction Message
					barcode_list = { }
					barcode_list[1] = { tostring(v.item_id), "0" } -- action 0=Inserted  1=Removed
					kernel.fmadiscaction(
						StoreID,
						username,
						"return-unknown",
						v.time_stamp24,
						barcode_list)

					local product_name = "Unknown Title"
					local product = BarcodeToProduct[v.item_id]
					if product ~= nil then
						local master_product = kernel.get_product_catalog_title(product.product_id)
						if master_product ~= nil then
							product_name = master_product.long_name
						end
				   end
											
					table.insert( inserted_discs, { title = product_name, barcode = tostring( v.item_id ), deck = tostring( v.deck ), slot = tostring( v.slot ) } )
					reload_inventory()
					decrement_and_display_empty_slots()
					display_inserted_discs()
					if #inserted_discs < 30 then
						insert_disc()
					end
				elseif v.code == "ItemReturned" then
					local deck_slot_string = " Deck: " .. tostring( v.deck ) .. " Slot: " .. tostring( v.slot )
					kernel.healthsvcs_sendauditevent(
						StoreID,
						"FMA_MultiDiscInsert",
						username,
						"FMA Bundle",
						"Barcode: UNKNOWN" .. deck_slot_string
					)
					
					table.insert( inserted_discs, { title = "UNKNOWN", barcode = "UNKNOWN", deck = tostring( v.deck ), slot = tostring( v.slot ) } )
					reload_inventory()
					decrement_and_display_empty_slots()
					display_inserted_discs()
					if #inserted_discs < 30 then
						insert_disc()
					end
				end
			end
		end,
		nil
	)
end

kernel.registeractorhitnotify(
	"up_arrow_background",
	function()
		idle_timer:reset()
		scroll_up()
	end
)
kernel.registeractorhitnotify(
	"image_up_arrow",
	function()
		idle_timer:reset()
		scroll_up()
	end
)
kernel.registeractorhitnotify(
	"down_arrow_background",
	function()
		idle_timer:reset()
		scroll_down()
	end
)
kernel.registeractorhitnotify(
	"image_down_arrow",
	function()
		idle_timer:reset()
		scroll_down()
	end
)
kernel.registeractorhitnotify(
	"close_area_button",
	function()
		if #hardware_manager:get_hardware_jobs_by_program( { "return-unknown" }, { "Stopped", "Completed", "Errored", "Garbage", "Suspended" } ) == 0 then
			idle_timer:reset()
			reload_inventory()
			view_goback()
		end
	end
)

kernel.setmacro( "EmptySlotsValue", tostring( empty_slots ) )
if #hardware_manager:get_hardware_jobs_by_program( { "return-unknown" }, { "Stopped", "Completed", "Errored", "Garbage", "Suspended" } ) == 0 then
	insert_disc()
end
