--
-- lib-statistics-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


wide_slots = {
	["6"] = true,
	["12"] = true,
	["18"] = true,
	["24"] = true,
	["30"] = true,
	["36"] = true,
	["42"] = true,
	["48"] = true,
	["54"] = true,
	["60"] = true,
	["66"] = true,
	["72"] = true
}

local is_qlm_deck = function( deck ) 
	if  qlm_deck == nil or qlm_deck == 0 then
		return false
	end
	return tostring( qlm_deck ) == tostring( deck ) 
end

is_slot_excluded = function(deck, slot)
	if ExcludedSlots == nil then
		return false
	end

	for k, v in ipairs(ExcludedSlots.results) do
		if tostring(v.deck) == tostring(deck) and tostring(v.slot) == tostring(slot) then
			kernel.log("found excluded slot - D:"..v.deck.." S:"..v.slot)
			return true
		end
	end
	return false
end

get_kiosk_capacity = function()
	kiosk_capacity = 0
	for pos, disc in BarcodePosition:pairs() do
		local fields = {}
		string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields, values) end)

		if false == is_qlm_deck ( fields[1] ) then
			local is_wide_slot = wide_slots[fields[2]]

			if (tostring(fields[1]) ~= "0" or tostring(fields[2]) ~= "0")
			and is_slot_excluded(fields[1], fields[2]) == false then
				if max_slots == 72 then
					if fields[1] ~= "7" then
						kiosk_capacity = kiosk_capacity + 1
					else
						if (is_wide_slot == nil) then
							kiosk_capacity = kiosk_capacity + 1
						end
					end
				else
					kiosk_capacity = kiosk_capacity + 1
				end
			end
		end
	end
	kernel.healthsvcs_enqueuestatistic(
		"KioskCapacity",
		kiosk_capacity )
end

get_inventory_statistics = function()
	local empty_slots =0
	local red_unknowns = 0
	local white_unknowns = 0
	local thin_discs = 0
	local damaged = 0
	local wrongtitles = 0
	local total_disks = 0
	local unmatched = 0
	
	local all_unmatched_multi_disc_barcodes = kernel.get_all_unmatched_multi_disc_barcodes()
	local function is_unmatched( barcode )
		for _, unmatched_barcode in ipairs( all_unmatched_multi_disc_barcodes ) do
			if barcode == unmatched_barcode then
				return true
			end
		end
		return false
	end
	
	for pos, disc in BarcodePosition:pairs() do
		local fields = {}
		string.gsub(pos, "@*(%w+)@*", function(values) table.insert(fields,values) end )
		
		if false == is_qlm_deck( fields[1] ) then
			local barcode = disc["barcode"]
			local status = disc["status"]
			local is_wide_slot = wide_slots[ fields[2] ]
			if barcode == "EMPTY" then
				if is_slot_excluded(fields[1], fields[2]) == false
				and fields[1] ~= nil and fields[2] ~= nil
				and fields[1] ~= "0" and fields[2] ~= "0"
				and ( ( max_slots == 72 and fields[1] == "7" and is_wide_slot ~= nil ) == false ) then
					empty_slots = empty_slots + 1
				end
			else
				total_disks = total_disks + 1
			end
			if barcode == nil or barcode == "UNKNOWN" or string.upper( barcode ) == "REDBOX" then
				red_unknowns = red_unknowns + 1
			end
			if status == "Unknown Title" then
				white_unknowns = white_unknowns + 1
			end
			if status == "Thinned" or status == "PR1" or status == "PR2" or status == "Redeployment" or status == "Rebalancing" then
			    thin_discs = thin_discs + 1
			end
			if status == "Damaged" then
			    damaged = damaged + 1
			end
			if status == "Wrong Title" then
			    wrongtitles = wrongtitles + 1
			end
			if status ~= "Damaged" and status ~= "DNR" and status ~= "Fraud" and status ~= "Reserved" and is_unmatched( barcode ) == true then
				unmatched = unmatched + 1
			end
		end
	end
	kernel.healthsvcs_enqueuestatistic(
		"EmptySlots",
		empty_slots )
	kernel.healthsvcs_enqueuestatistic(
		"UnknownDiscs",
		red_unknowns )
	kernel.healthsvcs_enqueuestatistic(
		"UnknownTitle",
		white_unknowns )
	kernel.healthsvcs_enqueuestatistic(
		"ThinDiscs",
		thin_discs )
	kernel.healthsvcs_enqueuestatistic(
		"Damaged",
		damaged )
	kernel.healthsvcs_enqueuestatistic(
		"WrongTitle",
		wrongtitles )
    kernel.healthsvcs_enqueuestatistic(
		"TotalDiscs",
		total_disks )
    kernel.healthsvcs_enqueuestatistic(
		"UnmatchedInKiosk",
		unmatched )
end

update_profile_inventory_stats = function()
	-- Send up the data file dates
	local profile_data_version = kernel.get_profile_version()
	if profile_data_version ~= nil and profile_data_version.origin_machine ~= nil and profile_data_version.generated_on ~= nil then
		local gen_date = tostring(profile_data_version.generated_on)
		kernel.healthsvcs_enqueuestatistic(
			"ProfileData",
			string.sub(gen_date, 1, 4) .. "/" ..
			string.sub(gen_date, 5, 6) .. "/" ..
			string.sub(gen_date, 7, 8) .. " " ..
			string.sub(gen_date, 9, 10) .. ":" ..
			string.sub(gen_date, 11, 12) .. ":" ..
			string.sub(gen_date, 13, 14) )
	end
	if InventoryVersion ~= nil
	and InventoryVersion.origin_machine ~= nil
	and InventoryVersion.generated_on ~= nil then
		local gen_date = InventoryVersion.generated_on.date
		if string.len(gen_date) == 14 and tonumber(gen_date) ~= nil then
			kernel.healthsvcs_enqueuestatistic(
				"InventoryData",
				string.sub(gen_date, 1, 4) .. "/" ..
				string.sub(gen_date, 5, 6) .. "/" ..
				string.sub(gen_date, 7, 8) .. " " ..
				string.sub(gen_date, 9, 10) .. ":" ..
				string.sub(gen_date, 11, 12) .. ":" ..
				string.sub(gen_date, 13, 14) )
		end
	end
end

update_vmz_inventory_statistics = function()
	local hardware_manager = newhardwaremanager()
	hardware_manager:get_inventory_stats(
		function( job_result )
			local dump_bin_count = 0
			for k, v in ipairs( job_result ) do
				if v.code == "DumpBinCount" then
					dump_bin_count = tonumber( v.message )
				end
			end
			kernel.healthsvcs_enqueuestatistic( "DumpBin", dump_bin_count )
		end
	)
	
	get_rebalance_barcodes( true, true,
		function( available_rebalances )
			local count = 0
			for k, v in pairs( available_rebalances ) do
				count = count + 1
			end		
		
			kernel.log( "update_vmz_inventory_statistics - available_rebalances insideVmz and outsideVmz: " .. tostring( count ) )
			kernel.healthsvcs_enqueuestatistic( "RebalancesInKiosk", tostring( count ) )
		end
	)
end

get_version_statistics = function()
	kernel.healthsvcs_enqueuestatistic( "OSVersion", kernel.osversion() )
	local filepaths = kernel.config_getvalidconfigdata( "versionsneeded", "FullPath" ) or {}

	for _, v in ipairs( filepaths ) do
		local version = kernel.fileversion( v.value )
		if version ~= nil and v.setting_name ~= nil then
			kernel.healthsvcs_enqueuestatistic( v.setting_name, version )
		else
			kernel.log( "Couldn't locate version for " .. tostring( v.setting_name ) )
		end
	end
end
