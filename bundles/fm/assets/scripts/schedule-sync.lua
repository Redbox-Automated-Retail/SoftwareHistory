--
-- schedule-sync.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local progress_manager = newprogressmanager()
progress_manager:start( false )
local hardware = newhardwaremanager()

local index = 0
local start_deck = 0
local last_deck = 0
local table_size = #deck_table
local label = "FMA-Sync scheduled by " .. username .. " range: "
local sync_label
table.sort(deck_table)
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

local deck_slot_valid
if (active_slot_actor == "start_slot") then
	deck_slot_valid = KeyPadController.on_validate(starting_slot)
else
	deck_slot_valid = KeyPadController.on_validate(ending_slot)
end

if deck_slot_valid == true then
	for k,v in ipairs(deck_table) do
		if index == 0 then
			start_deck = v
			last_deck = start_deck - 1
		end

		if last_deck + 1 ~= v then
			sync_label = label .. start_deck .. "," .. starting_slot .. " - " .. last_deck .. "," .. ending_slot
			hardware:hard_sync(
				sync_label,
				schedule_time.backing,
				start_deck,
				tonumber(starting_slot),
				last_deck,
				tonumber(ending_slot))
			start_deck = v
		end

		last_deck = v

		if index + 1 == table_size then
			sync_label = label .. start_deck .. "," .. starting_slot .. " - " .. last_deck .. "," .. ending_slot
			if tostring( start_deck ) == "1" and tostring( last_deck ) == tostring( max_decks ) and tostring( starting_slot ) == "1" and tostring( ending_slot ) == tostring( max_slots ) then
				sync_label = sync_label .. " - Full inventory sync."
			end
			hardware:hard_sync(
				sync_label,
				schedule_time.backing,
				start_deck,
				tonumber(starting_slot),
				last_deck,
				tonumber(ending_slot))
		end

		index = index + 1
	end
	job_grid:display(true)
	inventory_sync_reset()
end
progress_manager:stop()