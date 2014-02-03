--
-- schedule-redeployment-sync.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
table.sort(deck_table)

function start_redeployment_sync()
	local progress_manager = newprogressmanager()
	progress_manager:start( false )
	local hardware = newhardwaremanager()

	local decks_to_change = {}
	for _,v in ipairs( deck_table ) do
		local row = {}
		row.deck = tostring( v )
		table.insert( decks_to_change, row )
	end

	local result = hardware:prep_for_redeploy( 
		decks_to_change,
		function( results )
			local unknowns = {}

			for _, v in ipairs( results ) do
				if v.code == "LocationChanged" then
					table.insert( unknowns, { deck=v.deck, slot=v.slot } )
				end
			end
			
			if #unknowns > 0 then
				local label = "redeployment sync by " .. username
				local run_immediately = newdatetime()
				run_immediately = run_immediately - "00:05:00"
				hardware:hard_sync_by_location( label, run_immediately.backing, unknowns )
				job_grid:display(true)
			end
			inventory_sync_reset()
			progress_manager:stop()
		end	)
	if result ~= true then
		kernel.log( "Failed to schedule redeploy." )
		inventory_sync_reset()
		progress_manager:stop()
	end	
end

local messagebox = newmessageboxmanager()
callbacks = {}
callbacks[messagebox.ButtonYES] = function()
	start_redeployment_sync()
end

local deck_string = ""
local first = true
for _,v in ipairs( deck_table ) do
	if first then
		first = false
		deck_string = deck_string .. tostring( v )
	else
		deck_string = deck_string .. ", " .. tostring( v )
	end
end

local header = "WARNING!\n\n"
local line1 = "All empty slots outside of the VMZ on Decks: " .. deck_string .. " are about to be marked as UNKNOWN. If this job does not complete for any reason, some slots will remain marked as UNKNOWN and another inventory sync will need to be scheduled.\n\n"
local line2 = "This inventory sync can only run IMMEDIATELY regardless of what date/time was entered.\n\n"
local line3 = "CONTINUE?\n"
local message_text = header .. line1 .. line2 .. line3

messagebox:show( 
	message_text,
	messagebox.ButtonTypeYESNO,
	messagebox.IconTypeQuestion,
	callbacks )
