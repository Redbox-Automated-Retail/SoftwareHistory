--
-- application-startup.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if DataAccessAlreadySent == true then
	return
end

strings = kernel.getdictionary( "strings" )

SyncAlertManager = newalertmanager()

result_summary = { ["count_undefined"] = 0, ["count_unknown"] = 0, ["count_empty"] = 0, ["count_success"] = 0, ["count_excluded"] = 0 }

StoreID = kernel.getmachinevalueordefault( "Store", "ID", "Not Configured" )
StoreManager = newstoremanager()
StoreManager:init()
StoreManager:reload_pricing_data()
DiscActionLogging = kernel.config_getvalue(
	"system",
	"General",
	"DiscAction" ) == true

VMZMerchFlowTimeOut = kernel.config_getvalue(
	"system",
	"General",
	"VMZMerchFlowTimeOut" ) or 900000 -- 15 minutes represented in seconds
VMZMerchFlowEnabled = true

VerboseLogging = kernel.getmachinevalueordefault( "Store", "VerboseLogging", "False" ) == "True"

UnloadDelayMinutes = tonumber( kernel.config_getvalue(
	"system",
	"General",
	"UnloadDelayMinutes" ) or 60 )		
kernel.log("UnloadDelayMinutes: "..UnloadDelayMinutes)


kernel.registerconfigurationcallback(
	"Watch-StoreID",
	function( store, action, path, key, value )
		if store == "machine"
		and action == "set"
		and path == "Store"
		and key == "ID" then
			kernel.log( "Configuration value changed for StoreNumber: " .. value )
			StoreID = value
		end
	end )

kernel.registercanshutdown(
	function()
		kernel.log( "Field Maintenance Application" )
		kernel.log( ">  Kiosk Engine is requesting a shut-down" )
		return false
	end )

idle_timer = newidletimer()
idle_timer:initialize(
	20000,
	function()
		idle_timer:stop()
		kernel.healthsvcs_sendauditevent(
			StoreID,
			"FMA_Timeout",
			username,
			"FMA Bundle",
			"Field Maintenance Application bundle session abandoned." )
		kernel.executescript("activate_rental")
	end )


-- Nothing will work correctly if these values are not accurate
qlm_deck = tonumber( kernel.getmachinevalueordefault( "Store", "QlmDeck", "0" ) )
if qlm_deck == 0 then qlm_deck = nil end
max_decks = tonumber( kernel.getmachinevalueordefault( "Store", "MaxDecks", "0" ) )
if max_decks == 0 then max_decks = nil end
max_slots = tonumber( kernel.getmachinevalueordefault( "Store", "MaxSlots",	"0" ) )
if max_slots == 0 then max_slots = nil end
local last_hardware_check = kernel.getmachinevalueordefault( "Store", "LastHardwareCheck", "" )
num_newrelease_decks = 0

local function start_the_application()
	progress_manager:stop()
	KeyPadController = newkeypadcontroller()
	kernel.setmacro("keyboard:title_area", "   Enter a Slot")

	VMZEnabledMachine = ( qlm_deck == nil or qlm_deck == 0 )

	-- Attempt to open one (small) table from each .data file, so that if they fail the app won't be in the middle of endless loop later
	InventoryVersion:prime_cache_keys()
	Stores:prime_cache_keys()
	BadCardList:prime_cache_keys()
	ProductHistory:prime_cache_keys()

	if DataAccessAlreadySent ~= true then
		view_show( "login_view" )
		idle_timer:start()
		EmptySlots = nil
		RentedDiscs = nil
		QlmMonitor = newqlmmonitor()
		QlmMonitor:start( 5000 )
		HardSyncMonitor = newhardsyncmonitor()
		HardSyncMonitor:start( 10000, 30000 )
		-- This is the kiosk Engine jobs timer (separate from HAL job list)
		scheduler_services.start()
	end
	
end

HardwareManager = newhardwaremanager()
local function get_excluded_slots()	
	kernel.log("Getting excluded slots")
	HardwareManager:schedule_job(
		"get-excluded-locations",
		"Excluded Slots",
		false,
		"Immediate",
		nil,
		nil,
		nil,
		function( results )
			kernel.log( results )
			ExcludedSlots = results
			kernel.log("Obtainted Excluded slots")
		end,
		nil,
		false
	)
end

progress_manager = newprogressmanager()
progress_manager:start(false)

--Check if Door Sensors are Configured
HardwareManager:executeimmediate(
	"DOORSENSORS ISCONFIGURED",
	false,
	function( command, success, stack )
		if success then
			kernel.log( "Door Sensors Status:".. tostring( stack[1] ) )
			if stack[1] == "True" then
				IsDoorSensorConfigured = true
			else
				IsDoorSensorConfigured = false
			end
		else
			IsDoorSensorConfigured = false
		end
	end
)

-- Ask HAL for deck/slot config if not already set; otherwise go ahead and start the application
local current = os.date( "*t" )
local current_string = string.format( "%4d%02d%02d", current.year, current.month, current.day )

if max_decks ~= nil 
and max_slots ~= nil 
and last_hardware_check == current_string 
then
	get_excluded_slots()
	start_the_application()
else
	HardwareManager:get_machine_info(
		function( results )
			qlm_deck = nil
			max_decks = nil
			max_slots = nil
			for _, v in ipairs( results ) do
				if v.code == "DeckInfo" then
					max_slots = v.slot
					if max_decks == nil then
						max_decks = v.deck
					else
						if v.deck > max_decks then
							max_decks = v.deck
						end
					end
				elseif v.code == "QlmInfo" then
					qlm_deck = v.deck
				end
			end
			if qlm_deck ~= nil then
				max_decks = qlm_deck - 1
			end

			-- Save these to the registry, they are not likely to ever change again on this machine
			kernel.setmachinevalue( "Store", "QlmDeck", tostring( qlm_deck ))
			kernel.setmachinevalue( "Store", "MaxDecks", tostring( max_decks ))
			kernel.setmachinevalue( "Store", "MaxSlots", tostring( max_slots ))
			
			kernel.setmachinevalue( "Store", "LastHardwareCheck", current_string )

			get_excluded_slots()
			start_the_application()
		end
	)
end

