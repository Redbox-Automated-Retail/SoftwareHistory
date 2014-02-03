--
-- application-startup.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


--************************************************
--
-- Do NOT allow the app to run if OutOfOrder mode is set
--
--************************************************
if kernel.getmachinevalueordefault( "Store", "OutOfOrder", "false" ) == "true" then
	kernel.applyunsafeshutdown()
end


kernel.setmarket( kernel.getmachinevalue( "Store", "Market" ) )
kernel.setvendor( kernel.getmachinevalue( "Store", "Banner" ) )

Start_Over_View_Name = "start_view" --Used to abstract certain view functions like goback()


DefaultCulture = kernel.getmachinevalueordefault("Store","DefaultCulture","en-US" )
kernel.log( "DefaultCulture is set to: ".. DefaultCulture )
if kernel.isculturevalid( DefaultCulture ) == false then
	kernel.log( "Invalid culture set in Registry: ".. DefaultCulture.. ". Setting DefaultCulture to en-US.")
	DefaultCulture = "en-US"
end
if DefaultCulture == "en-US" then
	OtherCulture = "es-MX"
else
	OtherCulture = "en-US"
end
kernel.log( "OtherCulture is set to: ".. OtherCulture )

user_on_flip_view = false

if DataAccessAlreadySent == true then
	return
end

--************************************************
--
-- Define function calls used later in the script
--
--************************************************
function handle_disconnected_usb_card_reader()
	kernel.setmacro( "MaintenanceView:ErrorCode", "0072CDRR" )
	kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "0072CDRR" ] or "image_red_diagonal_stripes" )
	view_show( "maintenance_view" )
	idle_timer:suspend()
	--[[AlertManager.send_alert(
		"HardwareError",
		"application-startup.lua",
		"USB card swipe has been disconnected, entering maintenance mode.",
		nil,
		nil,
		"mm:USBCardReaderOffline, 0072CDRR"
	)]]
	kernel.timerstart( "disconnected_usb_card_reader_timer" )

end

local function initialize_scheduled_tasks()
	QLMMonitor = newqlmmonitor()
	QLMMonitor:start()

	ReservationManager = newreservationmanager()
	ReservationManager:init()
	ReservationManager:sync_reservations()

	start_sync_timers()
	MaintenanceModeTimer = newmaintenancemodetimer()
	MaintenanceModeTimer.clear()

	UnplayableExpireTimer = newunplayableexpiretimer()
	UnplayableExpireTimer:start()

	if quick_return_is_running == true then
		QuickReturnTimer = newquickreturntimer()
	end
end

local function quick_return_daemon_running()
	local is_running = false
	local qr_status = kernel.hardwaregetquickreturnstatus()
	if qr_status["status"] == "ACTIVE" then
		is_running = true
	end
	kernel.log( "Quick Return running = " .. tostring( is_running ))
	return ( is_running )
end

function idle_timer_expire_session()
	kernel.log( "idle_timer:expire_session()" )
	if ShoppingCart.vend_manager ~= nil
	and ShoppingCart.vend_manager.vending == true then
		kernel.log( "> prepos_vend_abort() will be called" )
		ShoppingCart:prepos_vend_abort()
	end
	if ShoppingCart.TicketsCart ~= nil then
		ShoppingCart.TicketsCart:reset_cart()
	end

	ShoppingCart:close_analytics( "True" )
	ShoppingCart:abandon( "Idle timer expired." )
	if DefaultCulture ~= nil and DefaultCulture ~= "" then
		kernel.setculture( DefaultCulture )
	else
		kernel.setculture( "en-US" )
	end
	localization_reload_resources()
	ABE_ON = false
	view_unwindtobottom_safe( "application_startup::idle_timer_expire_session" )
end

function idle_timer_callback()
	idle_timer_show_prompt(idle_timer_expire_session)
end

function idle_timer_show_prompt(expire_session)
	local message_view_manager = newmessageviewmanager()
	message_view_manager:show()
	view_analytics_event( "context", "idleTimer" )

	message_view_manager:hide_message()
	message_view_manager:set_idle_timer_title( strings.more_time_prompt_message )
	message_view_manager:show_first_button( strings.no_thanks_button, expire_session )
	message_view_manager:show_second_button(
		strings.yes_button,
		function() view_goback_safe( "need_more_time:second_button", false ) end
	)
	message_view_manager:start_timer( expire_session )
end

function idle_timer_show_prompt_english(expire_session)
	local message_view_manager = newmessageviewmanager()
	message_view_manager:show()
	view_analytics_event( "context", "idleTimer" )

	message_view_manager:hide_message()
	message_view_manager:set_idle_timer_title("Need more time?")
	message_view_manager:show_first_button("NO THANKS", expire_session )
	message_view_manager:show_second_button(
		"YES",
		function() view_goback_safe( "need_more_time:second_button", false ) end
	)
	message_view_manager:start_timer( expire_session )
end

local function start_the_application()
	progress_manager:stop()
	--kernel.setculture("en-US")
	if not run_initial_sync() then
		initialize_scheduled_tasks()
		if not kernel.hasvalidcardreader() then
			--kernel.pushview replaced with view_show function which will inturn call the kernel.pushview
			view_show( Start_Over_View_Name )
			handle_disconnected_usb_card_reader()
		else
			view_show( Start_Over_View_Name )
		end
	end

	-- Start the user idle timer
	idle_timer:initialize(
		40000,
		idle_timer_callback
	)

	-- Send up the machine model information
	local qlm_count = 0
	if qlm_deck ~= nil then
		qlm_count = 1
		kernel.healthsvcs_enqueuestatistic( "QLMDeckNumber", tostring( qlm_deck ) )
	else
		kernel.healthsvcs_enqueuestatistic( "QLMDeckNumber", "0" )
	end
	if max_decks ~= nil and max_slots ~= nil then
		for i = 1, max_decks do
			kernel.healthsvcs_enqueuestatistic( "Deck" .. tostring( i ), tostring( max_slots ) )
		end
	end

	-- This is the kiosk Engine jobs timer (separate from HAL job list)
	scheduler_services.start()
	configure_validate_stats_timers()

	-- This is required for Kiosk Analytics to control unusual view paths which are difficult to track
	CloseAnalyticsOnStartView = false
end

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

local function get_hardware_info_then_start_application()
	-- Close the Vend Door if it happens to be open
	HardwareManager:executeimmediate(
		"VENDDOOR CLOSE",
		true,
		nil )

	--used to notify HAL to ensure Quick Return is on if turned off through maintenance mode and not properly restarted
	if not in_maintenance_mode() then
		kernel.hardwarenotifymaintmodestate( false )
	end

	-- Do not check any sooner as the HAL will not be ready yet
	quick_return_is_running = quick_return_daemon_running()

	-- Ask HAL for deck/slot config if not already set; otherwise go ahead and start the application
	local current = os.date( "*t" )
	local current_string = string.format( "%4d%02d%02d", current.year, current.month, current.day )
	local last_hardware_check = kernel.getmachinevalueordefault( "Store", "LastHardwareCheck", "" )

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
				merch_type = nil
				buffer_slots = 1
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
					elseif v.code == "BufferSlot" then
						buffer_slots = v.message
					elseif v.code == "MerchType" then
						merch_type = v.message
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

				if merch_type == nil then
					kernel.log("Infering MerchType from qlm deck existence. " .. tostring( qlm_deck ) )
					merch_type = qlm_deck ~= nil and "QLM" or "VMZ1"
				end
				kernel.healthsvcs_enqueuestatistic( "MerchType", tostring( merch_type ) )
				
				if buffer_slots == nil then
					kernel.log("Use default buffer slot value.")
					buffer_slots = 1
				end
				kernel.healthsvcs_enqueuestatistic( "BufferSlot", tostring( buffer_slots ) )

				get_excluded_slots()
				start_the_application()
			end
		)
	end
end

local function load_registry_key_values()
	-- Load registry values into global variables
	EnforceShoppingCartItemLimit = kernel.getmachinevalueordefault(
		"Remote Services\\Merchant Services",
		"EnforceShoppingCartItemLimit",
		"True" ) == "True"
	EnableOfflineBadCardList = kernel.getmachinevalueordefault(
		"Remote Services\\Merchant Services",
		"EnableOfflineBadCardList",
		"True" ) == "True"
	MaximumShoppingCartItems = tonumber( kernel.getmachinevalueordefault(
		"Remote Services\\Merchant Services",
		"MaximumShoppingCartItems",
		"3" ) )
	EnableOutOfStockDisplay = kernel.getmachinevalueordefault(
		"Remote Services\\Inventory Services",
		"EnableOutOfStockDisplay",
		"False" ) == "True"
	EnableQueueBasedRentalLimit = kernel.getmachinevalueordefault(
		"Remote Services\\Merchant Services",
		"EnableQueueBasedRentalLimit",
		"True" ) == "True"
	EnableSellThruOfUsed = kernel.getmachinevalueordefault(
		"Remote Services\\Inventory Services",
		"EnableSellThruOfUsed",
		"True" ) == "True"
	--[[DisableGames = kernel.getmachinevalueordefault(
		"Remote Services\\Inventory Services",
		"DisableGames",
		"False" ) == "True"]]
	DisableGames = false
	--[[EnableUnplayableReturn = kernel.getmachinevalueordefault(
		"Remote Services\\Inventory Services",
		"EnableUnplayableReturn",
		"False" ) == "True"]]
	EnableUnplayableReturn = false
	BurnInTimeout = kernel.config_getvalue(
		"system",
		"General",
		"BurnInTimeout" ) or 1800000
	BurnInViewDuration = kernel.config_getvalue(
		"system",
		"General",
		"BurnInViewDuration" ) or 30000
	PricingReloadTimeout = tonumber(kernel.getmachinevalueordefault(
		"Store",
		"PricingReloadTimeout",
		"300000" ) )
	VerboseLogging = kernel.getmachinevalueordefault(
		"Store",
		"VerboseLogging",
		"False" ) == "True"
	--[[DisplayModifiedShoppingCart = kernel.getmachinevalueordefault(
		"Store",
		"DisplayModifiedShoppingCart",
		"False" ) == "True"]]
	DisplayModifiedShoppingCart = true
	EnableCreditsButton = kernel.config_getvalue(
		"system",
		"General",
		"EnableCreditsButton" ) == true
	HideCreditsButtonOnPromoUse = kernel.config_getvalue(
		"system",
		"General",
		"HideCreditsButtonOnPromoUse" ) == true
	PromptForCredits = kernel.config_getvalue(
		"system",
		"General",
		"PromptForCredits" ) == true
	EnableHiddenDigitalCheckout = kernel.config_getvalue(
		"system",
		"General",
		"EnableHiddenDigitalCheckout" ) == true
	ShowOptinForNormalCheckout = kernel.config_getvalue(
		"system",
		"General",
		"ShowOptinForNormalCheckout" ) == true
	ShowOptinForCreditsCheckout = kernel.config_getvalue(
		"system",
		"General",
		"ShowOptinForCreditsCheckout" ) == true
	FMAMaintenanceMode = kernel.getmachinevalueordefault(
		"Field Maintenance",
		"MaintenanceMode",
		"False" ) == "true"
	ComingSoonOptIn = kernel.config_getvalue(
		"system",
		"General",
		"ComingSoonOptIn" ) == true
	GamesSellThru = kernel.config_getvalue(
		"system",
		"General",
		"GamesSellThru" ) == true
	SellTickets = kernel.config_getvalue(
		"system",
		"General",
		"SellTickets" ) == true
	ShowFreeTrial = kernel.config_getvalue(
		"system",
		"General",
		"ShowFreeTrial" ) == true
	HowItWorksToggle = kernel.config_getvalue(
		"system",
		"General",
		"HowItWorksToggle" )
	if HowItWorksToggle == nil then
		HowItWorksToggle = true
	end
	MakeRoomBuffer = tonumber( kernel.config_getvalue(
		"system",
		"General",
		"MakeRoomBuffer" ) or 5 )
	DiscActionLogging = kernel.config_getvalue(
		"system",
		"General",
		"DiscAction" ) == true
	MarketingOptInEnabled = kernel.config_getvalue(
		"system",
		"General",
		"MarketingOptIn" ) == true
	RecommendationOnPickup = kernel.config_getvalue(
		"system",
		"General",
		"RecommendationOnPickup" ) == true
	GiftCardsEnabled = kernel.config_getvalue(
		"system",
		"General",
		"GiftCardsEnabled" ) == true
	GameEnableXbox360 = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnableXbox360" ) == true
	GameHasContentXbox360 = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentXbox360",
		"false" ) == "true"
	GameEnableXboxOne = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnableXboxOne" ) == true
	GameHasContentXboxOne = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentXboxOne",
		"false" ) == "true"
	GameEnablePS3 = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnablePS3" ) == true
	GameHasContentPS3 = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentPS3",
		"false" ) == "true"
	GameEnablePS4 = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnablePS4" ) == true
	GameHasContentPS4 = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentPS4",
		"false" ) == "true"
	GameEnableWii = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnableWii" ) == true
	GameHasContentWii = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentWii",
		"false" ) == "true"
	GameEnableWiiU = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnableWiiU" ) == true
	GameHasContentWiiU = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentWiiU",
		"false" ) == "true"
	EnablePlatformScreen = kernel.config_getvalue(
		"system",
		"Games",
		"EnablePlatformScreen" ) == true
	ForcedAirExchangerConfigured = kernel.config_getvalue(
		"system",
		"General",
		"ForceAirExchangerConfiguration" ) == true
		
end

local function create_coming_soon_cache_tables()
	local coming_soon_updated = FilteredProductList["last_updated"]
	if coming_soon_updated == nil
	or ( ProfileVersion.generated_on ~= nil and tostring( coming_soon_updated ) < ProfileVersion.generated_on.date )
	then
		local coming_soon_generator = newcomingsoongenerator()
		-- First build the titles used during Vend 'slot machine' effect
		-- 1000 is the rating used when none can be determined for an item in the cart
		coming_soon_generator:build_cache( "game", 16, 100 )
		coming_soon_generator:build_cache( "game", 20, 100 )
		coming_soon_generator:build_cache( "game", 1000, 100 )
		coming_soon_generator:build_cache( "bluray", 5, 3 )
		coming_soon_generator:build_cache( "bluray", 6, 3 )
		coming_soon_generator:build_cache( "bluray", 7, 3 )
		coming_soon_generator:build_cache( "bluray", 8, 3 )
		coming_soon_generator:build_cache( "bluray", 10, 3 )
		coming_soon_generator:build_cache( "bluray", 1000, 3 )
		coming_soon_generator:build_cache( "dvd", 5, 3 )
		coming_soon_generator:build_cache( "dvd", 6, 3 )
		coming_soon_generator:build_cache( "dvd", 7, 3 )
		coming_soon_generator:build_cache( "dvd", 8, 3 )
		coming_soon_generator:build_cache( "dvd", 10, 3 )
		coming_soon_generator:build_cache( "dvd", 1000, 3 )
		-- Create browsable products in the local ProductCatalog
		coming_soon_generator:create_browse_titles()
		-- Update the last modified timestamp
		local current = os.date( "*t" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )
		FilteredProductList["last_updated"] = current_string
	else
		kernel.log( "Coming soon is up to date, skipping the cache build" )
	end
end

local function register_default_hardware_handlers()
	-- Hook up default hardware event handlers
	kernel.storesvcs_shouldreboot() --prime registry value; does not actually reboot the PC
	kernel.registercanshutdown(
		function()
			kernel.log( "Kiosk Engine is requesting a shut-down" )
			return kernel.peekview() == Start_Over_View_Name or kernel.peekview() == "maintenance_view"
		end )
	kernel.registercanrestart(
		function()
			kernel.log( "Kiosk Engine is requesting a restart" )
			return kernel.peekview() == Start_Over_View_Name
		end )
	kernel.registershowmaintenanceview(
		function()
			kernel.setmacro( "MaintenanceView:ErrorCode", "0096KRNL" )
			kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "0096KRNL" ] or "image_red_diagonal_stripes" )
			view_show( "maintenance_view" )
			return true
		end )
	kernel.registerhardwarechange(
		function( component, action, risk_level )
			if  component == "Card Reader" then
				if action == "Disconnected" then
					kernel.log( "The USB card reader was disconnected, going into maintenance mode." )
					handle_disconnected_usb_card_reader()
				elseif action == "Connected" then
					kernel.log( "The USB card reader was connected, resuming operation." )
					view_unwindtobottom( "card_reader_connected" )
					idle_timer:resume()
					--[[AlertManager.send_alert(
						"HardwareError",
						"application-startup.lua",
						"USB card swipe has been connected, leaving maintenance mode.",
						nil,
						nil,
						"USBCardReaderOnline"
					)]]
					kernel.logto( "CardReaderLog", "Card Reader Re-Connected" )
				end
			end
		end )
	kernel.registerconfigurationcallback(
		"watch-config",
		function( store, action, path, key, value )
			if store == "machine"
			and action == "set" then
				if  path == "Store" and key == "ID" then
					kernel.log( "Configuration value changed for Store ID: " .. value )
					StoreManager:set_number( value )
					ReservationManager:init()
					ReservationManager:sync_reservations()
				elseif path == "Remote Services\\Merchant Services" and key == "EnforeShoppingCartItemLimit" then
					kernel.log( "Configuration value changed for EnforceShoppingCartItemLimit: " .. value )
					EnforceShoppingCartItemLimit = value == "True"
				elseif path == "Remote Services\\Merchant Services" and key == "MaximumShoppingCartItems" then
					kernel.log( "Configuration value changed for MaximumShoppingCartItems: " .. value )
					MaximumShoppingCartItems = tonumber( value )
				elseif path == "Remote Services\\Merchant Services" and key == "EnableOfflineBadCardList" then
					kernel.log( "Configuration value changed for EnableOfflineBadCardList: " .. value )
					EnableOfflineBadCardList = value == "True"
				elseif path == "Remote Services\\Inventory Services" and key == "EnableOutOfStockDisplay" then
					kernel.log( "Configuration value changed for EnableOutOfStockDisplay: " .. value )
					EnableOutOfStockDisplay = value == "True"
					kernel.reenterview()
				elseif path == "Remote Services\\Merchant Services" and key == "EnableQueueBasedRentalLimit" then
					kernel.log( "Configuration value changed for EnableQueueBasedRentalLimit: " .. value )
					EnableQueueBasedRentalLimit = value == "True"
				elseif path == "Remote Services\\Inventory Services" and key == "EnableSellThruOfUsed" then
					kernel.log( "Configuration value changed for EnableSellThruOfUsed: " .. value )
					EnableSellThruOfUsed = value == "True"
					kernel.reenterview()
				end
			end
		end )

	register_initial_sync_config_callback()
end


local function verify_hardware_status_before_launching_application()
	schedule_time = newdatetime()
	HardwareManager:get_hardware_status(
		schedule_time.backing,
		function( job_result )
			kernel.log( job_result )
			local hardware_status_ok = true
			for k, v in ipairs( job_result.results ) do
				if v.code ~= "HardwareStatusOk" then
					hardware_status_ok = false
				end
			end
			if hardware_status_ok then
				kernel.log( "Hardware Status: OK... Starting Rental Application." )
				-- THIS STARTS THE RENTAL APPLICATION
				get_hardware_info_then_start_application()
			else
				kernel.log( "Hardware Status: Failed... Putting kiosk in Maintenance Mode." )
				if not in_maintenance_mode() then
					kernel.setmacro( "MaintenanceView:ErrorCode", "HALSTS-043" )
					kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "HALSTS-043" ] or "image_red_diagonal_stripes" )
					view_show( "maintenance_view" )
				end

				-- Create the message to be sent as a part of the Alert if the Hardware Status is Failure.
				local alert_code_table = {
					[1] = { alert_text = "Gripper Clear", alert_code = "GripperObstructed"},
					[2] = { alert_text = "Vend Door Close", alert_code = "VendDoorOpen"},
					[3] = { alert_text = "Communicate to Arcus", alert_code = "MotionControllerCommunicationError"}
				}

				local contains_code = function( result_code )
					for k, v in ipairs( job_result.results ) do
						if v.code == result_code then
							return true
						end
					end
					return false
				end

				local hardware_status_message = "HAL Job: ".. job_result.job_id.. "<br><br>Hardware-Status Error<br>"
				for k, v in ipairs( alert_code_table ) do
					if contains_code( v.alert_code ) then
						hardware_status_message = hardware_status_message.. "<br>".. v.alert_text.. ": <b>Error</b>."
					else
						hardware_status_message = hardware_status_message.. "<br>".. v.alert_text.. ": True."
					end
				end
				hardware_status_message = hardware_status_message.. "<br><br>Placing the kiosk into maintenance mode."

				AlertManager.send_alert(
					"HardwareError",
					"hardware-status.hs returns MachineError",
					hardware_status_message,
					nil,
					nil,
					"mm:Hardware-Status, HALSTS-043"
				)
			end
		end
	)
end


--************************************************
--
-- Create Global class instances required for application function
--
--************************************************
AlertManager = newalertmanager()
StoreManager = newstoremanager()
StoreManager:init()
StoreManager:reload_pricing_data( "_v2" )
StoreManager:check_product_catalog_for_games_platforms()
StoreManager:refresh_rapid_thins()
ShoppingCartList = {}
HardwareManager = newhardwaremanager()
idle_timer = newidletimer( "idle_timer" )


--************************************************
--
-- Set Global variable values
--
--************************************************
MaximumOfflineGameRentalLimit = 1
quick_return_is_running = false

-- Nothing will work correctly if these values are not accurate
qlm_deck = tonumber( kernel.getmachinevalueordefault( "Store", "QlmDeck", "0" ) )
if qlm_deck == 0 then qlm_deck = nil end
max_decks = tonumber( kernel.getmachinevalueordefault( "Store", "MaxDecks", "0" ) )
if max_decks == 0 then max_decks = nil end
max_slots = tonumber( kernel.getmachinevalueordefault( "Store", "MaxSlots",	"0" ) )
if max_slots == 0 then max_slots = nil end

max_reco_items = 6
max_reco_cart_items = 5 -- MaximumShoppingCartItems (Should we use this? Reco Cart Assets are restricted to 5)


--************************************************
--
-- Pre-load lookup tables and other key data elements
--
--************************************************
load_registry_key_values()
localization_init()
SetESRBRatings()
config = kernel.getdictionary( "config" )
create_coming_soon_cache_tables()
register_default_hardware_handlers()
if kernel.ticketsvcs_selltickets() == true then
	create_tickets_cache_tables()
end

-- JVMHardTake FAQ tables - files are only in english so its ok to be in app-startup
free_trial_billing = kernel.gettable( "free_trial_billing" )
free_trial_faq = kernel.gettable( "free_trial_faqs" )
free_trial_privacy = kernel.gettable( "free_trial_privacy" )
free_trial_terms = kernel.gettable( "free_trial_tos" )	


-- If the application has crashed from file access errors, do not continue
if DataAccessAlreadySent == true then
	return
end

-- [Vaibhav Pingle] Moved this section of the code from the initialize_scheduled_tasks() method
-- in order to be able to monitor the inventory sync jobs scheduled through the FMA, even though if
-- the Initial Sync job is running on the kiosk.
SyncAlertManager = newalertmanager()
HardSyncMonitor = newhardsyncmonitor()
HardSyncMonitor:start(10000, 30000)

-- timer for new unplugged usb card reader, started in handle_disconnected_usb_card_reader()
kernel.timercreate(
	"disconnected_usb_card_reader_timer",
	10000,
	10000,
	function()
		kernel.timerstop( "disconnected_usb_card_reader_timer" )
		kernel.log( "> disconnected_usb_card_reader_timer executing" )
		if kernel.peekview() == "maintenance_view" then
			local current = os.date( "*t" )
			local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )
			AlertManager.send_alert(
				"HardwareError",
				"KioskID: " .. StoreManager:get_number() .. " Credit Card Reader Communications Error &#150; Machine Down",
				current_string .. " - Machine in maintenance mode.",
				nil,
				nil,
				"mm:USBCardReaderOffline, 0072CDRR"
			)
			kernel.logto( "CardReaderLog", "Card Reader Disconnected, Alert Sent" )
		else
			kernel.logto( "CardReaderLog", "Card Reader Disconnected, Replug Event Occured Within Threshold, Alert Not Sent" )
		end
	end )


-- Run immediate thin job if secret <redacted> key is set
if kernel.getmachinevalueordefault( "Store", "RemoteThinRequested", "False" ) == "True" then
	kernel.log( "Remote Thin Request found in Store registry" )
	if qlm_deck == nil then
		--vmz
		local jobname = "vmzthin-" .. tostring( newdatetime() )
		run_thin_vmz_job( "Remote thin job requested", jobname )
	else
		--qlm
		local jobname = "qlmthin-" .. tostring( newdatetime() )
		run_thin_job( "Remote thin job requested", jobname )
	end
	kernel.setmachinevalue( "Store", "RemoteThinRequested", "False" )
end

if #newhardwaremanager():get_hardware_jobs_by_label_contains_text( "maintenance sync by ", { "Stopped", "Errored", "Garbage", "Suspended", "Completed" } ) > 0 then
	view_show( "maintenance_sync_view" )
	return
end

progress_manager = newprogressmanager()
if not in_maintenance_mode() then
	progress_manager:start()
end

kernel.timercreate(
	"hal_init_status_check_timer",
	500,
	1000,
	function()
		local initstatus = kernel.hardwaregetinitstatus()
		kernel.log( "HAL init Running in state " .. tostring( initstatus["status"] ))
		if initstatus["status"] == "COMPLETED" or initstatus["status"] == "ERRORED" then
			kernel.log( "HAL init Completed, resuming Rental Application launch." )
			kernel.timerremove( "hal_init_status_check_timer" )
			if kernel.peekview() == "maintenance_view" then
				view_popview( "remove maintenance_view from the stack." ) --won't actually change the view since there is nothing underneath it to render yet, but when start view is pushed it'll refresh
			end
			verify_hardware_status_before_launching_application()
		elseif initstatus["status"] == nil then
			if not in_maintenance_mode() then
				kernel.setmacro( "MaintenanceView:ErrorCode", "HALSVCNR" )
				kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "HALSVCNR" ] or "image_red_diagonal_stripes" )
				view_show( "maintenance_view" )
			end
		end
	end )
kernel.timerstart("hal_init_status_check_timer")

kernel.timercreate(
	"pricing-reload-timer",
	PricingReloadTimeout,
	PricingReloadTimeout,
	function()
		if StoreManager ~= nil
		and kernel.peekview() == "start_view"
		and user_on_flip_view == false then
			kernel.log( "Reloading Pricing information..." )
			StoreManager:reload_pricing_data( "_v2" )
			StoreManager:check_product_catalog_for_games_platforms()
			if reload_carousel ~= nil then
				reload_carousel()
			end
		else
			kernel.log( "Cannot Reload Pricing since User is on the Flip View or the bundle is Not on Start View." )
		end
	end
)
kernel.timerstart("pricing-reload-timer")

kernel.timercreate(
	"rapid-thin-fresh-timer",
	600000,
	600000,
	function()
		if StoreManager ~= nil then
			kernel.log( "Refresh rapid thin data..." )
			kernel.datastoreclose( "InventoryThins" )
			-- This same code runs at app startup from the load-tables script
			-- To refresh cached inventory files they must be closed first, then the global Lua variable can be re-assigned to the newly opened copies
			RapidThinVersion = getfulltable( "InventoryThins", "Version")
			RapidThinBarcodes = getfulltable( "InventoryThins", "BarcodeToProduct" )
			kernel.log( "RapidThinBarcodes file contains " .. tostring( RapidThinBarcodes:getsize()) .. " barcodes."  )
			StoreManager:refresh_rapid_thins()
		end
	end
)
kernel.timerstart("rapid-thin-fresh-timer")

kernel.timercreate(
	"vmz-file-check-timer",
	30000,
	900000,
	function()
		local path = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\"
		local rebalancesin = path .. "RebalanceIn.json"
		local rebalancesout = path .. "RebalanceOut.json"
		local rebalancesunthin = path .. "RebalanceUnthin.json"
		local rebalancesversion = path .. "RebalanceVersion.json"
		local hwmgr = newhardwaremanager()
		
		local do_clean = false
		local do_thin = false
		local requires_soft_sync = false
		
		kernel.log( "Checking for VMZ files" )
	
		local compare_date_same_day = function( date1, date2 )
			kernel.log( "Date1: " .. tostring( date1 ) )
			kernel.log( "Date2: " .. tostring( date2 ) )
			if date1 == nil
			or date2 == nil then
				kernel.log("One of the dates was nil " .. tostring( date1 ~= nil ) .. ", " .. tostring( date2 ~= nil ) )
				return false
			end
			
			if date1.backing == nil
			or date2.backing == nil then
				kernel.log("One of the dates backing was nil " .. tostring( date1.backing ~= nil ) .. ", " .. tostring( date2.backing ~= nil ) )
				return false
			end
			
			if date1.backing.year ~= date2.backing.year
			or date1.backing.month ~= date2.backing.month
			or date1.backing.day ~= date2.backing.day then
				return false
			end
			return true
		end
		
		local unmark_discs = function( barcode_list, on_complete )
			if barcode_list ~= nil and #barcode_list > 0 then
				local barcodes = {}
				for _,v in ipairs( barcode_list ) do
					local row = {}
					row.barcode = v["Barcode"]
					table.insert( barcodes, row )
				end
			
				hwmgr:clear_merch_status( 
					barcodes,
					on_complete )
				do_clean = true
			end
		end
		
		local convert_unthin_to_global_table = function( barcode_list )
			if barcode_list ~= nil then
				local temp_list = {}
				for _,v in ipairs( barcode_list ) do
					temp_list[ v["Barcode"] ] = "Known"
				end
				RebalancesUnThinInuse = temp_list
			end
		end
		
		--*********START SETUP*************
		local version = kernel.tablefromfileserializeddictionary( rebalancesversion )
		
		local rebalance_out_data = nil
		local rebalance_out_inuse_data = nil
		local rebalance_in_data = nil
		local rebalance_unthin_data = nil

		--get new version date
		local version_date = nil
		if version ~= nil then
			version_date_file = newdatetime( kernel.datetimeparsefromstringdate( version.VersionTimeStampUTC, "yyyyMMddHHmmss" ) )
			version_date = newdatetime( kernel.datetimeparsefromstringdate( 
												kernel.tolocaltime( version.VersionTimeStampUTC, "yyyyMMddHHmmss" ) ) )
			if version_date_file ~= nil then
				local date_string = version_date_file.backing.year .. "/" 
										.. string.format( "%02d", version_date_file.backing.month ) .. "/"
										.. string.format( "%02d", version_date_file.backing.day ) .. " "
										.. string.format( "%02d", version_date_file.backing.hour ) .. ":"
										.. string.format( "%02d", version_date_file.backing.min ) .. ":"
										.. string.format( "%02d", version_date_file.backing.sec )
				kernel.healthsvcs_enqueuestatistic( "RebalanceData", date_string ) --Check Name -- Test it hits db
			end
		end
		
		local current_date = newdatetime( os.date( "*t" ) )
		--*********END SETUP*************
		
		kernel.log( "Check for Unthin." )
		rebalance_unthin_data = kernel.tablefromfileserializedlistdictionary( rebalancesunthin, StoreManager ~= nil and StoreManager:get_number() or nil )
		rebalance_unthin_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesunthin .. ".inuse", StoreManager ~= nil and StoreManager:get_number() or nil )
		
		if rebalance_unthin_data ~= nil and rebalance_unthin_inuse_data ~= nil then
			kernel.log( "UnProcess UnThin Inuse File." )
			RebalancesUnThinInuse = {}
			kernel.deletefile( rebalancesunthin .. ".inuse" )
		end
		if rebalance_unthin_data ~= nil then
			kernel.log( "Process UnThin File." )
			os.rename( rebalancesunthin, rebalancesunthin .. ".inuse" )
			unmark_discs( rebalance_unthin_data, nil )
			convert_unthin_to_global_table( rebalance_unthin_data )
			--TODO should I mark the disks here?
			requires_soft_sync = true -- a cheat?
		end

		
		--*******Check for outdated files
		if version ~= nil then
			if compare_date_same_day( current_date, version_date ) == false then 
				--old data
				kernel.log( "Remove Old Vmz Files." )
				rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesout .. ".inuse", StoreManager ~= nil and StoreManager:get_number() or nil )
				if rebalance_out_inuse_data ~= nil then
					unmark_discs( rebalance_out_inuse_data, nil )
					kernel.deletefile( rebalancesout .. ".inuse" )
				end
				
				kernel.deletefile( rebalancesout )
				kernel.deletefile( rebalancesin )
				kernel.deletefile( rebalancesversion )
			else 
				kernel.log( "Processing New Vmz Files." )

				rebalance_out_data = kernel.tablefromfileserializedlistdictionary( rebalancesout, StoreManager ~= nil and StoreManager:get_number() or nil )
				rebalance_out_inuse_data = kernel.tablefromfileserializedlistdictionary( rebalancesout .. ".inuse", StoreManager ~= nil and StoreManager:get_number() or nil )

				if rebalance_out_data ~= nil and rebalance_out_inuse_data ~= nil then
					unmark_discs( rebalance_out_inuse_data, nil )
					kernel.deletefile( rebalancesout .. ".inuse" )
				end
				
				if rebalance_out_data ~= nil then
					os.rename( rebalancesout, rebalancesout .. ".inuse" )
					--terminate clean-vmz jobs
					local currentjobs = hwmgr:get_hardware_jobs_by_program({ "clean-vmz" }, { "Garbage", "Stopped", "Completed", "Errored" })
					for k,v in ipairs( currentjobs ) do
						kernel.hardwareterminatejob( v.id )
					end
					do_thin = true
				end
			end
			--finalize
			local finalize = function()
				if do_thin == true then
					local rebalances = {}
					for _,v in ipairs( rebalance_out_data ) do
						local row = {}
						row.barcode = v["Barcode"]
						table.insert( rebalances, row )
					end
					run_thin_vmz_job2( "Automated Rebalance", "vmzthin-" .. tostring( newdatetime() ), rebalances, rebalance_unthin_data )
				elseif do_clean == true then
					hwmgr:clean_vmz( nil )
				end
			end
			
			if requires_soft_sync == true then
				run_soft_sync( finalize )
			else
				finalize()
			end
		end
	end
)
if qlm_deck == nil then
	kernel.timerstart("vmz-file-check-timer")
end

--Check if Air Exchanger is Configured
HardwareManager:executeimmediate(
	"AIRXCHGR ISCONFIGURED",
	false,
	function( command, success, stack )
		if success then
			kernel.log( "Air Exchanger Configured:".. tostring( stack[1] ) )
			if ForcedAirExchangerConfigured then
				kernel.log( "Forcing Air Exchanger to be Configured." )
			end
			if string.lower(tostring(stack[1])) == "true"
			or ForcedAirExchangerConfigured == true then
				HardwareManager:executeimmediate("AIRXCHGR FANON", false, nil)
				kernel.log( "Air Exchanger has been turned ON" )
			end
		end
	end
)
