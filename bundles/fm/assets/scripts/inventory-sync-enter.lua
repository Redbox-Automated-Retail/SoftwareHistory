--
-- inventory-sync-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local count = getinventorycount("Unknown")
if count <= 0 then
	kernel.disableactor("schedule_unknowns_button")
	kernel.setactorforegroundcolor("schedule_unknowns_button", "gray")
else
	kernel.enableactor("schedule_unknowns_button")
	kernel.setactorforegroundcolor("schedule_unknowns_button", "white")
end

reset_slots = function()
	starting_slot = "1"
	kernel.setactortext("start_slot_value", starting_slot)
	ending_slot = tostring(max_slots)
	kernel.setactortext("end_slot_value", ending_slot)
end

inventory_sync_reset = function()
	reset_slots()
	deck_table = {}
	for deck = 1, (max_decks or 0) do
		table.insert(deck_table, deck)
	end
	kernel.setactortext("deck_value", "All")
	schedule_time = newdatetime()
	kernel.setactortext("schedule_value", tostring(schedule_time) )
end

KeyPadController.on_show =function()
	active_slot_actor = kernel.getlasthitactorname()
end

function CheckIndividualDeckSelectionStatus()

	local status
	local singledecks={}
	
	if kernel.getactortext("deck_value") == "All" then
		status = false
	elseif #deck_table == 1 then
		status = true
		table.insert(singledecks,deck_table[1])
	else
		status = false

		local previous_deck
		local iteration = 1
		local deck_set = {}
		local sets={}
		
		-- Generate an array of 'count of consecutive decks' within the deck selection.
		for k, v in ipairs(deck_table) do
			if iteration > 1 then
				if (previous_deck + 1) ~= v then
					table.insert(deck_set, sets)
					sets={}
				end
			end
			
			table.insert(sets,v)
			if iteration == #deck_table then
				table.insert(deck_set, sets)
			end
			iteration = iteration + 1
			previous_deck = v
		end
		-- if there are more than one deck_sets then run a loop to check if there are any individual decks selected
		if #deck_set > 1 then
			-- if any of the deck_set has count 1 then
			for k, v in ipairs(deck_set) do
				if #v == 1 then
					status = true
					table.insert(singledecks,v[1])
				end
			end
		elseif #deck_set == 1 then
			if #deck_set[1] == 1 then
				status = true
				table.insert(singledecks,deck_set[1])
			end
		end
	end
	return status,singledecks
end

KeyPadController.on_validate = function(selected_slot)
	-- check if the slot value is greater than 0 and less than the max_slots (73 or 90) value
	if tonumber(selected_slot) < 1 
	or tonumber(selected_slot) > max_slots then
		newmessageboxmanager():show("Slot must be greater than 0 and less than ".. (tonumber(max_slots)+1).. ".")
		return false
	end

	local isIndividualDeckSelected
	local singledecks = {}
	
	isIndividualDeckSelected,singledecks = CheckIndividualDeckSelectionStatus()
	if active_slot_actor == "start_slot" then
		if (tonumber(selected_slot) > tonumber(ending_slot)
			and isIndividualDeckSelected == true
			and kernel.getactortext("deck_value") ~= "All")
		then
			newmessageboxmanager():show("You have selected an invalid deck/slot combination. Please remove decks ".. table.concat(singledecks, ", ").. " or change the slot range selection.")
			return false
		end
		starting_slot = selected_slot
	else
		if (tonumber(selected_slot) < tonumber(starting_slot)
			and isIndividualDeckSelected == true
			and kernel.getactortext("deck_value") ~= "All")
		then
			newmessageboxmanager():show("You have selected an invalid deck/slot combination. Please remove decks ".. table.concat(singledecks, ", ").. " or change the slot range selection.")
			return false
		end
		ending_slot = selected_slot
	end
	return true
end

if starting_slot == nil then
	starting_slot = "1"
end
kernel.setactortext("start_slot_value", starting_slot)

if ending_slot == nil then
	ending_slot = tostring(max_slots)
end
kernel.setactortext("end_slot_value", ending_slot)

if deck_table == nil or #deck_table == 0 or #deck_table == max_decks then
	deck_table = {}
	for deck = 1, (max_decks or 0) do
		table.insert(deck_table, deck)
	end
	kernel.setactortext("deck_value", "All")
else
	kernel.setactortext("deck_value", table.concat(deck_table, ", "))
end

if schedule_time == nil then
	schedule_time = newdatetime()
end

kernel.setactortext("schedule_value", tostring(schedule_time) )

if job_grid == nil then
	job_grid = newjobgrid({ "sync", "sync-locations" }, { "Garbage" })

	job_grid.job_pager.next_changed =
		function( enabled )
			if enabled == true then
				kernel.setactorimage( "right_arrow_button", "right_arrow" )
				kernel.setactorforegroundcolor( "right_arrow_button", "White" )
				kernel.enableactor( "right_arrow_button" )
			else
				kernel.setactorimage( "right_arrow_button", "right_arrow_disabled" )
				kernel.setactorforegroundcolor( "right_arrow_button", "Gray" )
				kernel.disableactor( "right_arrow_button" )
			end
		end

	job_grid.job_pager.prev_changed =
		function( enabled )
			if enabled == true then
				kernel.setactorimage( "left_arrow_button", "left_arrow" )
				kernel.setactorforegroundcolor( "left_arrow_button", "White" )
				kernel.enableactor( "left_arrow_button" )
			else
				kernel.setactorimage( "left_arrow_button", "left_arrow_disabled" )
				kernel.setactorforegroundcolor( "left_arrow_button", "Gray" )
				kernel.disableactor( "left_arrow_button" )
			end
		end
	job_grid:load()
end

job_grid:display()
