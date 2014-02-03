--
-- schedule-unknowns.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local progress_manager = newprogressmanager()
progress_manager:start( false )
local hardware = newhardwaremanager()

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

local result = hardware:get_inventory(
		function(inventory)
			local label = "sync-unknowns scheduled by " .. username
			local unknowns = {}

			for _,v in ipairs(inventory) do
				if ( v.item_id == nil or v.item_id == "UNKNOWN" or string.upper(  v.item_id ) == "REDBOX" ) and  
						deck_table[v.deck] ~= nil and 
						v.slot >= tonumber(starting_slot) and 
						v.slot <= tonumber(ending_slot) then
					table.insert(unknowns, { deck=v.deck, slot=v.slot })
				end
			end
			
			if #unknowns > 0 then
				table.sort(unknowns,
							function(left, right)
								if left.deck > right.deck then
									return true
								end
								
								if left.deck == right.deck then
									return left.slot > right.slot
								end
							end)

				hardware:hard_sync_by_location(
							label,
							schedule_time.backing,
							unknowns)
			else
				newmessageboxmanager():show("No unknowns were located on the deck(s) and slot(s) specified by schedule options")
			end
			
			job_grid:display(true)
			inventory_sync_reset()
			progress_manager:stop()
		end)

if not result then
	inventory_sync_reset()
	progress_manager:stop()
end
