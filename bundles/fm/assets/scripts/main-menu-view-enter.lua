--
-- main-menu-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

----vmz screen code
-- Clear num_newrelease_decks and display text
num_newrelease_decks = 0
kernel.setmacro("keyboard:newrelease_decks", "" )
----end vmzscreen code

enable_filters = false
last_hit_filter_tab = nil
kernel.timerremove( "idle_timer" )
idle_timer = newidletimer()
sync_job_ids = {}
idle_timer:initialize(
	300000,
	function()
		local ok_to_switch = true
		
		if ok_to_switch == true then
			idle_timer:stop()
			kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_Timeout",
				username,
				"FMA Bundle",
				"Field Maintenance Application bundle session abandoned." )
			kernel.logto( "UserLoginLog","Field Maintenance | Session timed out" )
			kernel.executescript( "activate_rental" )
		else 
			kernel.timerstart( "idle_timer" )
		end
	end )
idle_timer:start()

if Inventory_All_Table == nil then
	reload_inventory()
end

local Hardware_Manager = newhardwaremanager()
local latest = {}
local programs = {"sync", "qlm-unload", "thin"}
local hardwarejobs = Hardware_Manager:get_hardware_jobs_by_program2(programs)

local get_earliest = function(jobs)
	local last = nil
	for _, v in ipairs(jobs) do
		if v.starttime ~= nil then
			last = last or v.starttime
			if last > v.starttime then
				last = v.starttime
			end
		end
	end
	return last or "NA"
end

local filter_jobs = function(program)
	local jobs = {}
	for _, v in ipairs(hardwarejobs) do
		if program == v.program
			and v.status == "Pending" then
			jobs[#jobs+1] = v
		end
	end
	return jobs
end

if hardwarejobs then
  for _, v in ipairs(programs) do
    local jobs = filter_jobs(v)
    if #jobs <= 0  then
      latest[v] = "NA"
    else
      latest[v] = get_earliest(jobs)
    end
  end
end

kernel.invsvcs_getrentalcount(
	StoreID,
	function( result )
		if result.success == false then
			kernel.log( "Main Menu Enter: GetRentalCount call failed" )
			return
		end
		local count = result.properties["rental_count"]
		if count == nil then
			kernel.log( "Main Menu Enter: Rental count was not returned in the properties" )
			return
		end
		RentedDiscs = count
		kernel.setactortext(
			"rented_discs_value",
			tostring(count) )
end )

if maintenance_mode == nil then
	maintenance_mode = kernel.getmachinevalue( "Field Maintenance", "MaintenanceMode" ) == "true"
	kernel.setmachinevalue( "Field Maintenance", "MaintenanceMode", tostring(maintenance_mode) )
end

kernel.setactortext( "maintenance_mode_button", iif(maintenance_mode, "Turn Off\r\nMaintenance Mode", "Turn On\r\nMaintenance Mode") )
kernel.setactorforegroundcolor("inv_mgmt_button", iif(maintenance_mode, "gray", "white") )
kernel.setactorforegroundcolor("inv_sync_button", iif(maintenance_mode, "gray", "white") )
kernel.setactorforegroundcolor("inv_qlm_button", iif(maintenance_mode, "gray", "white") )

if maintenance_mode == true then
	 kernel.disableactor( "inv_mgmt_button" )
     kernel.disableactor( "inv_sync_button" )
     kernel.disableactor( "inv_qlm_button" )
elseif maintenance_mode == false then
	 kernel.enableactor( "inv_mgmt_button" )
     kernel.enableactor( "inv_sync_button" )
     kernel.enableactor( "inv_qlm_button" )
end

-- All stats values will be refreshed each time the page loads
kernel.setactortext( "rented_discs_value", tostring(RentedDiscs) )
kernel.setactortext( "empty_slots_value", tostring(EmptySlots) )
kernel.setactortext( "empty_case_count_value", tostring(getinventorycountbyproductid( 1139 )) )
kernel.setactortext( "unknown_count_value", tostring(getinventorycount( "Unknown" )) )
kernel.setactortext( "fraud_discs_count_value", tostring( getinventorycount( "Fraud" ) ) )
kernel.setactortext( "damaged_discs_count_value", tostring(getinventorycount( "Damaged" )) )
kernel.setactortext( "distinct_title_count_value", tostring(getdistincttitlescount()) )
kernel.setactortext( "reserved_disc_count_value", tostring(getreservationscount()) )
kernel.setactortext( "rebalance_disc_count_value", "..." )
kernel.setactortext( "thin_redeploys_outside_vmz_value", "..." )
kernel.setactortext( "thins_outside_vmz_value", "..." )
get_rebalance_barcodes( true, true, function(rebalance_barcodes)
	kernel.setactortext( "rebalance_disc_count_value", tostring(#rebalance_barcodes) )
end)

kernel.setactortext( "DNR_disc_count_value", tostring(getinventorycount( "DNR" )) )
kernel.setactortext( "qlm_unload_time_value", latest["qlm-unload"] )
kernel.setactortext( "qlm_thin_time_value", latest["thin"] )
kernel.setactortext( "inv_sync_time_value", latest["sync"] )

local unknowns_count = getinventorycount( "Unknown" )
kernel.log( "Unknowns in kiosk: " .. unknowns_count )
if unknowns_count == 0 then
	kernel.log( "Disabling 'remove_unknowns_button' button" )
	kernel.disableactor( "remove_unknowns_button" )
	kernel.setactorforegroundcolor( "remove_unknowns_button", "gray" )
else
	kernel.enableactor( "remove_unknowns_button" )
	kernel.setactorforegroundcolor( "remove_unknowns_button", "white" )
end

local frauds_count = getinventorycount( "Fraud" )
kernel.log( "Frauds in kiosk: " .. frauds_count )
if frauds_count == 0 then
	kernel.log( "Disabling 'remove_frauds_button' button" )
	kernel.disableactor( "remove_frauds_button" )
	kernel.setactorforegroundcolor( "remove_frauds_button", "gray" )
else
	kernel.enableactor( "remove_frauds_button" )
	kernel.setactorforegroundcolor( "remove_frauds_button", "white" )
end

-- Clean up globals
starting_slot = nil
ending_slot = nil
deck_table = nil
working_date = nil
schedule_time = nil
job_grid = nil
qlm_info = nil
check_box_state = nil
DeckPopupMode = nil
InventoryViewFilter = "all_filter"
InventorySortBy = "sort_by_deck_slot"
KeyPadController.last_selected_slot = nil
KeyPadController.on_validate = nil
KeyPadController.on_show = nil
LastReadBarcode = nil
sort = {}

ForcedAirExchangerConfigured = kernel.config_getvalue(
	"system",
	"General",
	"ForceAirExchangerConfiguration" ) == true

--Check if Air Exchanger is Configured
local air_exchanger_query = function()
	HardwareManager:executeimmediate(
		"AIRXCHGR ISCONFIGURED",
		false,
		function( command, success, stack )
			local IsAirExchangerConfigured = false
			if success then
				IsAirExchangerConfigured = false
				kernel.log( "Air Exchanger Callback stack[1]: ".. tostring( stack[1] ) )
				if string.lower(tostring(stack[1])) == "true" then
					IsAirExchangerConfigured = true
				end
			end

			if ForcedAirExchangerConfigured then
				kernel.log( "Forcing Air Exchanger to be Configured." )
				IsAirExchangerConfigured = true
			end

			if IsAirExchangerConfigured then
				kernel.enableactor( "air_exchanger_button" )
				local AirExchangerFanOn = true
				HardwareManager:executeimmediate("AIRXCHGR  FANSTATUS", false, function( command2, success2, stack2 )
					if success2 or ForcedAirExchangerConfigured then
						if IsDoorSensorConfigured then
							kernel.setactory( "air_exchanger_button", 550 )
						else
							kernel.setactory( "air_exchanger_button", 440 )
						end
						if string.lower(tostring(stack2[1])) == "on" then
							AirExchangerFanOn = true
							kernel.setactortext( "air_exchanger_button", "Turn Off\nAir Exchanger Fan" )
							kernel.setactorforegroundcolor("air_exchanger_button", "white" )
						else
							AirExchangerFanOn = false
							kernel.setactortext( "air_exchanger_button", "Turn On\nAir Exchanger Fan" )
							kernel.setactorforegroundcolor("air_exchanger_button", "white" )
						end
						
						kernel.showactor( "air_exchanger_button" )
						kernel.registeractorhitnotify( "air_exchanger_button", function()
							if AirExchangerFanOn then
								Hardware_Manager:executeimmediate( "AIRXCHGR FANOFF", false, function( command3, success3, stack3)
									kernel.setactortext( "air_exchanger_button", "Turn On\nAir Exchanger Fan" )
									kernel.setactorforegroundcolor( "air_exchanger_button", "white" )
									kernel.enableactor( "air_exchanger_button" )
									kernel.log( "Air Exchanger has been turned OFF" )
									AirExchangerFanOn = false
								end)
							else
								Hardware_Manager:executeimmediate( "AIRXCHGR FANON", false, function( command3, success3, stack3)
									kernel.setactortext( "air_exchanger_button", "Turn Off\nAir Exchanger Fan" )
									kernel.setactorforegroundcolor( "air_exchanger_button", "white" )
									kernel.enableactor( "air_exchanger_button" )
									kernel.log( "Air Exchanger has been turned ON" )
									AirExchangerFanOn = true
								end)
							end
						end)
					end
				end)
			end
		end)
end

function verify_door_status()
	Hardware_Manager:executeimmediate(
		"DOORSENSORS STATUS",
		false,
		function( command, success, stack )
			local DoorStatus = "Failed"
			if success then
				kernel.log( "DOORSENSORS STATUS:".. tostring( stack[1] ) )
				if stack[1] == "OK" then
					DoorStatus = "Closed"
				else
					DoorStatus = "Open"
				end
			end
			kernel.setactortext( "door_status_button", "Door Status:\nDoor ".. DoorStatus )
			kernel.enableactor( "door_status_button" )
			air_exchanger_query()
		end
	)
end

function begin_page_status_queries()
	if IsDoorSensorConfigured then
		kernel.showactor( "door_status_button" )
		verify_door_status()
	else
		air_exchanger_query()
	end
end

if VMZEnabledMachine == true then 
	qlm_info = newvmzinfo()
	qlm_info.on_load = function() 
		kernel.setactortext( "info_label", tostring( qlm_info ) )
		begin_page_status_queries()
	end
else
	qlm_info = newqlminfo()
	qlm_info.on_load = function()
		kernel.setactortext( "info_label", tostring( qlm_info ) )
		if qlm_info.bay_status == "ERROR" or qlm_info.bay_status == "EMPTY" then
			kernel.setactorforegroundcolor( "engage_button", "gray" )
			kernel.setactorforegroundcolor( "disengage_button", "gray" )
			kernel.disableactor( "engage_button" )
			kernel.disableactor( "disengage_button" )
		end
		begin_page_status_queries()
	end
end

if qlm_info.loaded == false then
	local progress_mgr = newprogressmanager()
	progress_mgr:start( false )
	qlm_info:load( progress_mgr )
else
	if VMZEnabledMachine == true then
		qlm_info:refresh()
	end
	kernel.setactortext( "info_label", tostring( qlm_info ) )
end

if EnableDiscSecurity == true then
    kernel.showactor( "verify_fraud_button" )
    kernel.setactory( "inv_mgmt_button", 550 )
else
    kernel.hideactor( "verify_fraud_button" )
    kernel.setactory( "inv_mgmt_button", 440 )
end

