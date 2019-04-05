--
-- lib-scheduler-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local is_valid_reboot_time = function( reboot_time )
	if string.len( reboot_time ) ~= 5 then
		return false
	end
	local hour = tonumber( string.sub( reboot_time, 1, 2 ))
	local minute = tonumber( string.sub( reboot_time, 4, 5 ))

	if hour >= 0 and hour <= 23 and minute >= 0 and minute <= 59 then
		return true
	end
	return false
end

configure_validate_stats_timers = function()

	local create_stats_timer = function( job_name, cron_expression, function_name )
		scheduler_services.add_entry(
			job_name,
			nil,
			nil,
			nil,
			cron_expression,
			function_name,
			nil,
			"DoNothing",
			nil
		)
		kernel.log( "Job " .. job_name .. " added to Kiosk Engine scheduler." )
	end

	local seed_hour = 0
	local seed_minute = 0
	local seed_seconds = kernel.random( 0, 59 )

	local stats_timer_seed = kernel.getmachinevalueordefault( "Store", "StatsTimerSeed", "NotSet" )
	if stats_timer_seed == "NotSet" then
		local reboot_time = kernel.getmachinevalueordefault( "Store", "RebootTime", "NotSet" )
		if reboot_time ~= "NotSet" and reboot_time ~= "  :" and is_valid_reboot_time( reboot_time ) == true then
			seed_hour = tonumber( string.sub( reboot_time, 1, 2 )) + 1
			if seed_hour >= 24 then
				seed_hour = seed_hour - 24
			end
			seed_minute = tonumber( string.sub( reboot_time, 4, 5 ))
		else
			seed_hour = kernel.random( 2, 4 )
			seed_minute = kernel.random( 0, 59 )
		end
		local new_seed_value = string.padr( tostring( seed_hour ), 2, "0") .. ":" .. string.padr( tostring( seed_minute ), 2, "0" )
		kernel.setmachinevalue( "Store", "StatsTimerSeed", new_seed_value )
	else
		seed_hour = tonumber( string.sub( stats_timer_seed, 1, 2 ))
		seed_minute = tonumber( string.sub( stats_timer_seed, 4, 5 ))
	end

	local _h = tostring( seed_hour )
	local _m = tostring( seed_minute )
	local _s = tostring( seed_seconds )

	local _medium_timer = scheduler_services.get_entry( "StatsMediumTimer" )
	if _medium_timer == nil then
		local _temp = kernel.random( 0, 2 )
		local cron_exp = _s .. " " .. _m .. " " .. tostring( _temp ) .. "-23/2 * * ? "
		create_stats_timer( "StatsMediumTimer", cron_exp, "stats_run_medium_timer" )
	end

	local _daily_timer = scheduler_services.get_entry( "StatsDailyTimer" )
	if _daily_timer == nil then
		local cron_exp = _s .. " " .. _m .. " " .. tostring( seed_hour + 1 ) .. " * * ? "
		create_stats_timer( "StatsDailyTimer", cron_exp, "stats_run_daily_timer" )
	end

end

stats_run_medium_timer = function()
	--Runs every 2 hours
	kernel.log( "STATS medium timer executing..." )
	--TotalDiscs
	--WrongTitle
	--Damaged
	--ThinDiscs
	--UnknownDiscs
	--UnknownTitle
	--EmptySlots
	--InventoryData
	--ProfileData
	get_inventory_statistics()
	update_profile_inventory_stats()
	update_vmz_inventory_statistics()
	kernel.log("Got inventory stats")
end

stats_run_daily_timer = function()
	--Runs once per day
	kernel.log( "STATS daily timer executing..." )
	
    kernel.clean_fraud_images()
    kernel.log( "clean old fraud images" )

	--Send Hal Properties
    kernel.enqueue_hardware_properties()
	kernel.log( "Include Hardware Properties" )

	--Kiosk Capacity
	get_kiosk_capacity()
	kernel.log("Got Kiosk Capacity")
	
	--VSMImpressions
	get_vsm_statistics()
	kernel.log("Got VSM stats")
	
	--Versions
	get_version_statistics()
	kernel.log( "Got Versions" )

    --Abe Check
    local has_abe_device = kernel.hasabedevice()
    kernel.healthsvcs_enqueueproperty( "ABEKeypad", has_abe_device )
    kernel.log( "Got Abe Keypad State" )
	
	-- IR Camera install Check
	local ir_camera_installed = kernel.irhardwareinstalled()
    kernel.log("Got IR Camera install date")
	if ir_camera_installed ~= nil then
		kernel.healthsvcs_enqueueproperty("IRCamera", ir_camera_installed)
		kernel.log("Sent IR Camera install date")
	end

	--Create duplicate barcode alert
	kernel.log( "Duplicate barcode daily timer executing..." )
	local total_dupes = 0
	for k, v in DuplicateBarcodes:pairs() do
		total_dupes = total_dupes + 1
	end
	
	if DuplicateBarcodes == nil or total_dupes < 1 then
		kernel.log( "> no duplicate barcodes found, alert will not be sent." )
	else
		local kiosk_id = StoreManager:get_number()
		local buffer =
		"<h2>" .. "Duplicate Bar code summary" .. "</h2>" ..
		"<br>"
		for barcode, slot in DuplicateBarcodes:pairs() do
			buffer = buffer .. "Barcode: " .. tostring( barcode ) .. ", " .. "original location " .. tostring( slot.original_key ) ..
			", " .. "duplicate location " .. tostring( slot.duplicate_key) ..
			"<br>"
		end
		kernel.hardware_alert_manager_send_alert(
			"TitleEmpty",
			"Alert from kiosk " .. tostring( kiosk_id ),
			buffer,
			nil,
			nil,
			"Alert from kiosk " .. tostring( kiosk_id )
		)
		kernel.log( "Sent duplicate barcode alert" )
		DuplicateBarcodes:clear()
	end
	
	--compare GetOpsMarket result to KaseyaD.ini
	kernel.storesvcs_getopsmarket(
		StoreManager:get_number(),
		function( result )
			local kaseya_path = kernel.storesvcs_getkaseyainipath()
			local kaseya_info = kernel.storesvcs_getopsmarketandkaseyaserverfromfile( kaseya_path )
			
			if kaseya_path == null then
				kernel.log( "Unable to locate KaseyaD.ini path, not reinstalling Kaseya." )
			elseif kaseya_info == null
			or ( kaseya_info ~= null and ( kaseya_info.ops_market == null or kaseya_info.kaseya_server == null ) ) then
				kernel.log( "Unable to parse KaseyaD.ini, not reinstalling Kaseya." )
			elseif result.properties.ops_market == "UnNamed" and result.properties.kaseya_server == "Kaseya6" then
				kernel.log( "GetOpsMarket returned UnNamed and Kaseya6, not reinstalling Kaseya." )			
			elseif string.lower( kaseya_info.ops_market ) ~= string.lower( result.properties.ops_market ) or string.lower( kaseya_info.kaseya_server ) ~= string.lower( result.properties.kaseya_server ) then
				kernel.log( "KaseyaD.ini and GetOpsMarket have different values, reinstalling Kaseya." )
				
				local path = "C:\\Program Files\\Redbox\\Reds\\Installers\\" .. result.properties.kaseya_server .. "Install.exe"
				local args = "/s /w /r /v /g=redbox." .. result.properties.ops_market .. " /m=redbox-" .. StoreManager:get_number()
				
				kernel.startprocesswaitforexit( path, args, 20000 )
			else
				kernel.log( "KaseyaD.ini and GetOpsMarket have the same values, not reinstalling Kaseya." )
			end
		end
	)

    --Update Mcafee
    kernel.update_mcafee()
end