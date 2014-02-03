--
-- schedule-maintenance-sync.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

function start_schedule_maintenance_sync()
	local progress_manager = newprogressmanager()
	progress_manager:start( false )
	local hardware = newhardwaremanager()

	local index = 0
	local start_deck = 0
	local last_deck = 0
	local table_size = #deck_table
	local label = "maintenance sync by " .. username .. " range: "
	local sync_label
	table.sort(deck_table)

	--forces job to run immediately
	schedule_time = newdatetime()
	schedule_time = schedule_time - "00:05:00"

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
	progress_manager:stop()
end


local messagebox = newmessageboxmanager()
callbacks = {}
callbacks[messagebox.ButtonYES] = function()
	start_schedule_maintenance_sync()
end

local message_text = "WARNING!\n\nThis kiosk will show OUT OF ORDER for the duration of the inventory sync AFTER exiting the field maintenance application.\n\n" ..
	"This inventory sync can only run IMMEDIATELY regardless of what date/time was entered.\n\n" ..
	"To run a BACKGROUND inventory sync please use the other inventory sync option.\n\nCONTINUE?\n"
messagebox:show( 
	message_text,
	messagebox.ButtonTypeYESNO,
	messagebox.IconTypeQuestion,
	callbacks )
	