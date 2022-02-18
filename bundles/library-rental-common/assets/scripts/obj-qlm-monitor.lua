--
-- obj-qlm-monitor.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local function get_counts()
	local empty = 0
	local thinned = 0
    local undefined = 0
	for pos, disc in BarcodePosition:pairs() do
		local fields = {}
		string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields,values) end )
		if ( qlm_deck == nil or fields[1] ~= tostring( qlm_deck ) )
		and fields[1] ~= nil and fields[2] ~= nil
		and tostring(fields[1]) ~= "0" and tostring(fields[2]) ~= "0"
		and is_slot_excluded( fields[1], fields[2] ) == false then
			if disc["barcode"] == "EMPTY" then
				empty = empty + 1
			end
            if disc["barcode"] == "UNDEFINED" then
                undefined = undefined + 1
            end
			if disc["status"] == "Thinned" or disc["status"] == "Redeployment" or disc["status"] == "Rebalancing" then
				thinned = thinned + 1
			end
		end
	end
	return empty, thinned, undefined
end

local function send_thin_rebalance_information()
	kernel.log( "Preparing to Send Thin/Rebalance Statistics" )
	
	local get_inside_outside_vmz_counts = function( barcode_list, on_complete )
		local temp_list = {}
		for _,v in ipairs( barcode_list ) do
			local row = {}
			row.barcode = v["Barcode"] or v["barcode"]
			table.insert( temp_list, row )
		end
		
		if #temp_list == 0 then
			kernel.log( ">>obj-qlm-monitor.send_thin_information -> get_inside_outside_vmz_counts received bad information.  Bailing Computation" )
			on_complete( nil )
			return
		end
		local hardware_manager = newhardwaremanager()
		hardware_manager:get_barcode_vmz_position( 
			temp_list, 
			function( results )
				local inside = 0
				local outside = 0
				for _,v in ipairs( results ) do
					if ( v.code == "BarcodeInVMZ" ) then
						inside = inside + 1
					elseif ( v.code == "BarcodeOutsideVMZ" ) then
						outside = outside + 1
					end
				end
				local result_list = {}
				result_list["inside"] = inside
				result_list["outside"] = outside
				on_complete( result_list )
			end)
	end
	
	local thins = get_thin_barcodes()
	if thins ~= nil and #thins > 0 then
		get_inside_outside_vmz_counts( thins, 
			function( result_list )
					kernel.log( "ThinsInVMZ: " .. tostring( result_list ~= nil and result_list.inside or 0 ) )
					kernel.log( "ThinsOutsideVMZ: " .. tostring( result_list ~= nil and result_list.outside or 0 ) )
					kernel.healthsvcs_enqueuestatistic( "ThinsInVMZ", result_list ~= nil and result_list.inside or 0 )
					kernel.healthsvcs_enqueuestatistic( "ThinsOutsideVMZ", result_list ~= nil and result_list.outside or 0 )
			end)
	else
		kernel.healthsvcs_enqueuestatistic( "ThinsInVMZ", 0 )
		kernel.healthsvcs_enqueuestatistic( "ThinsOutsideVMZ", 0 )
	end
	
	--local rebalancesoutpath = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceOut.data.inuse"
	--local rebalances = kernel.tablefromfileserializedlistdictionary( rebalancesoutpath, StoreManager ~= nil and StoreManager:get_number() or nil )
	local rebalances = kernel.rental_inventory_get_rapidstatus_by_status("Rebalancing")	or {}
	
	local count = 0
	for k, v in pairs( rebalances ) do
		count = count + 1
	end
	
	if rebalances ~= nil and count > 0 then
		get_inside_outside_vmz_counts( rebalances, 
			function( result_list )
					kernel.log( "RebalancesInVMZ: " .. tostring( result_list ~= nil and result_list.inside or 0 ) )
					kernel.log( "RebalancesOutsideVMZ: " .. tostring( result_list ~= nil and result_list.outside or 0 ) )
					kernel.healthsvcs_enqueuestatistic( "RebalancesInVMZ", result_list ~= nil and result_list.inside or 0 )
					kernel.healthsvcs_enqueuestatistic( "RebalancesOutsideVMZ", result_list ~= nil and result_list.outside or 0 )
			end)
	else
		kernel.healthsvcs_enqueuestatistic( "RebalancesInVMZ", 0 )
		kernel.healthsvcs_enqueuestatistic( "RebalancesOutsideVMZ", 0 )
	end
end

local function format_errors( errors )
	local error_count = 0
	local error_msg = ""
	if errors ~= nil then
		for k, v in pairs(errors) do
			error_count = error_count + 1
			error_msg = error_msg .. "&nbsp;&nbsp;&nbsp;&nbsp;" .. k .. "&nbsp;&nbsp;" .. v
		end
	end
	return error_count, error_msg
end

local function log_result( result )
	kernel.log("id: " .. tostring( result.id  )
		.. " code: " .. tostring( result.code )
		.. " deck: " ..  tostring( result.deck )
		.. " slot: .. " .. tostring( result.slot )
		.. " item_id:  " .. tostring( result.item_id )
		.. " message: " .. tostring( result.message )
		.. " time_stamp: " .. tostring( result.time_stamp )
	)
end

local function qlm_unload_msg( job, result_table )
	kernel.log("QLMMonitor.on_timer: Creating QLM Unload message" )
	local empty, thinned, undefined = get_counts()
	local results = result_table["results"]
	local errors = result_table["errors"]
	local msg = "Unload " .. job.status .. "<br>HAL job: " .. job.id
	msg = msg .. "<br><br>Remaining Empty Slots in Kiosk: " .. empty .. "<br>"
	local error_count, error_msg = format_errors( errors )
	local result_msg = "RESULTS: <br>"
	local putaway = 0
	local unknownputaway = 0

	-- build barcode_list for DiscAction messgage
	barcode_list = { }
	time_stamp = os.date("%Y%m%d%H%M") -- YearMonthDayHourMin ie: 201312242359
				
	for _, v in ipairs(results) do
		log_result( v )
		if v.code == "QlmUnloadPutIntoDrum" then
			kernel.log("Item-ID" .. v.item_id)
			if (v.item_id == "UNKNOWN" or v.item_id == nil or v.item_id == "") then
				unknownputaway = unknownputaway + 1
			else
				putaway = putaway + 1
			end
			--Need to update inventory just ADDED
			sync_inventory( v )
		end

		if v.code == "QlmUnloadBarcodeFromQlm" then
			time_stamp = v.time_stamp24
			if v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
				barcode_list[#barcode_list + 1] = { v.item_id, "0" } -- action 0=Inserted  1=Removed
			end
		end

		result_msg = result_msg .. "&nbsp;&nbsp;&nbsp;&nbsp;" .. v.message
		if v.deck ~= 0 and v.slot ~= 0 then
			result_msg = result_msg .. " Deck: " .. v.deck .. " Slot: " .. v.slot
			if v.item_id ~= nil
			and v.code ~= "QlmUnloadQlmSlotEmpty" then
				result_msg = result_msg .. " Barcode: " .. v.item_id
			end
		end
		result_msg = result_msg .. "<br><br>"
	end
	msg = msg .. "<br>Discs Read:  " .. putaway
	msg = msg .. "<br>Discs Read as Unknown:  " .. unknownputaway
	msg = msg .. "<br>Total Discs Read:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" .. putaway + unknownputaway .. "<br><br>"
	
	
	if error_count > 0 then
		error_msg =  "JOB ERRORS: " .. error_count .. "<br>" .. error_msg
		msg = msg .. error_msg .. "<br><br>"
	end
	msg = msg .. result_msg

	-- Send DiscAction Message
	if ( #barcode_list > 0 ) then
		-- Get the job name
		local job_name = job.program
			
		-- VMZ machines use the same qlm-unload job
		-- So for VMZ machines, change the job name to the fictitious job name
		local VMZEnabledMachine = ( qlm_deck == nil or qlm_deck == 0 )
		if VMZEnabledMachine == true then
			job_name = "vmz-unload"
		end
			
		kernel.fmadiscaction(
			kernel.getmachinevalueordefault("Store","ID","N/A"),
			tostring(extractUsernameFromLabel( job.label ) ),
			job_name,
			time_stamp,
			barcode_list)
	end
	
	return msg
end

local function qlm_thin_msg( job, result_table )
    kernel.log("QLMMonitor.on_timer: Creating Thin message" )
	local results = result_table["results"]
	local errors = result_table["errors"]
	local msg = "Thin " .. job.status .. "<br>HAL job: " .. job.id
	local error_count, error_msg = format_errors( errors )
	local result_msg = "RESULTS: <br>"
	local putaway = 0

	-- build barcode_list for DiscAction messgage
	barcode_list = { }
	time_stamp = os.date("%Y%m%d%H%M") -- YearMonthDayHourMin ie: 201312242359
	
	for _, v in ipairs(results) do
		log_result( v )
		if v.code == "ThinTransferSuccessful" then
			putaway = putaway + 1
			--Need to update inventory REMOVED
			sync_inventory( v )
			
			time_stamp = v.time_stamp24
			if  v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
				barcode_list[#barcode_list + 1] = { v.item_id, "1" } -- action 0=Inserted  1=Removed
			end
		end
		result_msg = result_msg .. "&nbsp;&nbsp;&nbsp;&nbsp;" .. v.message
		if v.item_id ~= nil and v.code == "ThinDiskFromDrum" then
			result_msg = result_msg .. " Barcode: " .. v.item_id
		end
		if v.deck ~= 0 and v.slot ~= 0 then
			result_msg = result_msg .. " Deck: " .. v.deck .. " Slot: " .. v.slot
		elseif v.code == "ThinDiskFromDrum" then
			result_msg = result_msg .. " Location: DumpSlot"
		end
		result_msg = result_msg .. "<br>"
	end
	msg = msg .. "<br>Thinned discs transfered to QLM: " .. putaway .. "<br><br>"
	if error_count > 0 then
		error_msg =  "JOB ERRORS: " .. error_count .. "<br>" .. error_msg
		msg = msg .. error_msg .. "<br><br>"
	end
	local empty, thinned, undefined = get_counts() --Must happen after sync_inventory() calls
	msg = msg .. "<br><br>Remaining Empty Slots in Kiosk: " .. empty .. " (" .. undefined .. " slots marked UNDEFINED)"
	msg = msg .. "<br>Remaining Thinned Discs in Kiosk: " .. thinned .. "<br><br>"

	msg = msg .. result_msg
	
	-- Send DiscAction Message
	if ( #barcode_list > 0 ) then
		kernel.fmadiscaction(
			kernel.getmachinevalueordefault("Store","ID","N/A"),
			tostring(extractUsernameFromLabel( job.label ) ),
			job.program,
			time_stamp,
			barcode_list)
	end

	return msg
end

local function qlm_unload_thin_msg( job, result_table )
	kernel.log("QLMMonitor.on_timer: Creating QLM Unload_thin message" )
	local empty, thinned, undefined = get_counts()
	local results = result_table["results"]
	local errors = result_table["errors"]
	local msg = "QLM Unload_thin " .. job.status .. "<br>HAL job: " .. job.id
	msg = msg .. "<br><br>Remaining Empty Slots in Kiosk: " .. empty .. " (" .. undefined .. " slots marked UNDEFINED)"
	msg = msg .. "<br>Remaining Thinned Discs in Kiosk: " .. thinned .. "<br><br>"
	local error_count, error_msg = format_errors( errors )
	local result_msg = "RESULTS: <br>"
	local putaway_unload = 0
	local putaway_thin  = 0

	-- build barcode_list for DiscAction messgage
	barcode_list = { }
	time_stamp = os.date("%Y%m%d%H%M") -- YearMonthDatHourMin ie: 201312242359
	
	for _, v in ipairs(results) do
	log_result( v )
	if v.code == "QlmUnloadPutIntoDrum" then
		putaway_unload = putaway_unload + 1
		--Need to update inventory just ADDED
		sync_inventory( v )

		time_stamp = v.time_stamp24
		if v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
			barcode_list[#barcode_list + 1] = { v.item_id, "0" } -- action 0=Inserted  1=Removed
		end

	end
	if v.code ==  "ThinTransferSuccessful" then
		putaway_thin = putaway_thin + 1
		--Need to update inventory just ADDED
		sync_inventory( v )

		time_stamp = v.time_stamp24
		if v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
			barcode_list[#barcode_list + 1] = { v.item_id, "1" } -- action 0=Inserted  1=Removed
		end

	end
	result_msg = result_msg .. "&nbsp;&nbsp;&nbsp;&nbsp;" .. v.message
	if v.deck ~= 0
	and v.slot ~= 0 then
		result_msg = result_msg .. " Deck: " .. v.deck .. " Slot: " .. v.slot
		if v.item_id ~= nil
		and v.code ~= "QlmUnloadQlmSlotEmpty" then
			result_msg = result_msg .. " Barcode: " .. v.item_id
		end
	end
		result_msg = result_msg .. "<br><br>"
	end
	msg = msg .. "<br>Unloaded discs from QLM: " .. putaway_unload .. "<br><br>"
	msg = msg .. "<br>Thinned discs transfered to QLM: " .. putaway_thin .. "<br><br>"
	if error_count > 0 then
		error_msg =  "JOB ERRORS: " .. error_count .. "<br>" .. error_msg
		msg = msg .. error_msg .. "<br><br>"
	end
		msg = msg .. result_msg
		
	-- Send DiscAction Message
	if ( #barcode_list > 0 ) then
		kernel.fmadiscaction(
			kernel.getmachinevalueordefault("Store","ID","N/A"),
			tostring(extractUsernameFromLabel( job.label ) ),
			job.program,
			time_stamp,
			barcode_list)
	end

	return msg
end

local function clean_vmz_msg( job, result_table )
	kernel.log( "QLMMonitor.on_timer: Creating Clean VMZ message" )
	local results = result_table["results"]
	local msg = "Clean VMZ " .. job.status .. "<br>HAL Job: " .. job.id .. "<br><br>"
	local transferred_disk_count = 0
	local thin_count_in_zone = 0
	local high_thin_location = 0

	for _, v in ipairs( results ) do
		if v.code == "TransferredDiskCount" then
			transferred_disk_count = v.message
		end
		if v.code == "ThinCountInZone" then
			thin_count_in_zone = v.message
		end
		if v.code == "HighThinLocation" then
			high_thin_location = v.message
		end
	end
	
	msg = msg .. "Disks Transferred: " .. tostring( transferred_disk_count ) .. "<br>"
	msg = msg .. "Thins in VMZ: " .. tostring( thin_count_in_zone ) .. "<br>"
	msg = msg .. "Last Thin Location: " .. tostring( high_thin_location ) .. "<br><br>"
	msg = msg .. "The clean VMZ run completed."
	
	send_thin_rebalance_information()
	
	if job.status == "Completed" and tonumber( transferred_disk_count ) == 0 then
		msg = nil
	end
	
	return msg
end

local function vmz_thin_msg( job, result_table )
    kernel.log("QLMMonitor.on_timer: Creating VMZ Thin message" )
	local results = result_table["results"]
	local errors = result_table["errors"]
	local msg = "Thin " .. job.status .. "<br>HAL job: " .. job.id
	local error_count, error_msg = format_errors( errors )
	local result_msg = "RESULTS: <br>"
	local putaway = 0
	local dumpbin = 0

	-- build barcode_list for DiscAction messgage
	barcode_list = { }
	time_stamp = os.date("%Y%m%d%H%M") -- YearMonthDatHourMin ie: 201312242359
	
	for _, v in ipairs(results) do
		log_result( v )
		if v.code == "ThinTransferSuccessful" then
			if v.deck == 0 and v.slot == 0 then
				dumpbin = dumpbin + 1
				
				time_stamp = v.time_stamp24
				if v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
					barcode_list[#barcode_list + 1] = { v.item_id, "1" } -- action 0=Inserted  1=Removed
				end

			else
				putaway = putaway + 1
			end
			--Need to update inventory REMOVED
			sync_inventory( v )
		elseif v.code == "DiskTransferSuccessful" then
			local substring = "The item was transferred from storage to the VMZ."
			if string.find( v.message, substring ) ~= nil then
				putaway = putaway + 1
			end
			--Need to update inventory REMOVED
			sync_inventory( v )
		end
		result_msg = result_msg .. "&nbsp;&nbsp;&nbsp;&nbsp;" .. v.message
		if v.item_id ~= nil and v.code == "ThinDiskFromDrum" then
			result_msg = result_msg .. " Barcode: " .. v.item_id
		end
		if v.deck ~= 0 and v.slot ~= 0 then
			result_msg = result_msg .. " Deck: " .. v.deck .. " Slot: " .. v.slot
		elseif v.code == "ThinDiskFromDrum" then
			result_msg = result_msg .. " Location: DumpSlot"
		end
		result_msg = result_msg .. "<br>"
	end
	msg = msg .. "<br>Thinned discs transfered to VMZ: " .. putaway .. "<br><br>"
	msg = msg .. "<br>Thinned discs transfered to dump bin: " .. dumpbin .. "<br><br>"
	if error_count > 0 then
		error_msg =  "JOB ERRORS: " .. error_count .. "<br>" .. error_msg
		msg = msg .. error_msg .. "<br><br>"
	end
	local empty, thinned, undefined = get_counts() --Must happen after sync_inventory() calls
	msg = msg .. "<br><br>Remaining Empty Slots in Kiosk: " .. empty .. " (" .. undefined .. " slots marked UNDEFINED)"
	msg = msg .. "<br>Remaining Thinned Discs in Kiosk: " .. thinned .. "<br><br>"

	msg = msg .. result_msg
	
	-- Send DiscAction Message
	if ( #barcode_list > 0 ) then
		kernel.fmadiscaction(
			kernel.getmachinevalueordefault("Store","ID","N/A"),
			tostring(extractUsernameFromLabel( job.label ) ),
			job.program,
			time_stamp,
			barcode_list)
	end
		
	send_thin_rebalance_information()
	
	return msg
end

function stringtrim (s)
  return (string.gsub(s, "^%s*(.-)%s*$", "%1"))
end

function extractUsernameFromLabel( jobLabel )
	local username = nil
	if ( string.find( string.lower(tostring(jobLabel)), "scheduled by") ~= nil ) then
		--split and grab username
		local splitLabel = split(jobLabel, "scheduled by")
		if ( splitLabel ~= nil and #splitLabel == 2 ) then
			--trim and verify username is not empty or nil
			username = stringtrim( splitLabel[2] )
		end
	end
	return username
end

function newqlmmonitor()
	local t = {}
	t.start = function( self, start, frequency )
		kernel.timercreate(
			"qlm-monitor",
			start or kernel.random(60000,300000),
			frequency or 300000,
			self.on_timer )
		kernel.timerstart( "qlm-monitor" )
	end
	
	t.stop = function( self )
		kernel.log( "qlm-monitor:stop()" )
        kernel.timerstop( "qlm-monitor" )
	end

	t.on_timer = function()
        kernel.timerstop( "qlm-monitor" )
        kernel.log("QLMMonitor.on_timer: Executing")
        local hardwaremgr = newhardwaremanager()
        local hardwarejobs = hardwaremgr:get_hardware_jobs_by_program(
			{"qlm-unload", "thin", "unload-thin", "clean-vmz", "thin-vmz" },
            {"Pending", "Running", "Suspended", "Garbage" } )
		local msg_creators = { ["unload-thin"]= qlm_unload_thin_msg, ["qlm-unload"]= qlm_unload_msg, ["thin"] = qlm_thin_msg, ["clean-vmz"] = clean_vmz_msg, ["thin-vmz"] = vmz_thin_msg }
		local job_count = table_count( hardwarejobs )
		kernel.log("QLMMonitor.on_timer: Total Qlm jobs either stopped or errored: " .. tostring( job_count) )
		if job_count > 0 then
			hardwaremgr:soft_sync(
				function( soft_sync_result )
					for _, v in ipairs( hardwarejobs ) do
						-- job id, status, label, program, starttime, executiontime, priority
						local job_name_label = string.upper( v.program )
						if job_name_label == "THIN-VMZ" then
							job_name_label = "THIN"
							if v.label == "Automated Rebalance" then
								kernel.log( "QLMMonitor Force Clean VMZ")
								--shedule clean-vmz
								hardwaremgr:clean_vmz( 
									function()
										kernel.log("QLMMonitor Clean VMZ Finished.")
									end)
							end
						end
						local func = msg_creators[ v.program ]
						if func ~= nil then
							local result_table = kernel.hardwaregetprogramresults( v.id )
							local alert_type = "QLM"
							if v.program == "clean-vmz" then
								alert_type = "TitleEmpty"
							end
							local store = kernel.getmachinevalueordefault(
								"Store",
								"ID",
								"N/A" )
							local msg = func( v, result_table )
							if msg ~= nil then
								kernel.healthsvcs_sendalert2(
									store,
									alert_type,
									job_name_label,
									msg
								)
							end
							kernel.hardwaretrashjob( v.id )
						end
					end

					-- This only needs to be done once no matter how many hardware jobs were found
					force_server_snapshot( soft_sync_result.results )

					empty = nil
					kernel.timerstart("qlm-monitor")
					kernel.log("QLMMonitor.on_timer:  Complete" )
				end )
		else
			kernel.log("QLMMonitor.on_timer:  Complete" )
			empty = nil
			if hal_comm_failure == false then
				kernel.timerstart( "qlm-monitor" )
			else
				kernel.log( "QLMMonitor is unable to communicate with HAL, qlm-monitor timer will not be resumed until rental app reload." )
			end
		end
	end
	return t
end
