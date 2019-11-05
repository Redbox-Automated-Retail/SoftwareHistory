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

kernel.register_environment_change_callback()

if not in_maintenance_mode() then
    view_show("application_load_view")
end

kernel.setmarket( kernel.getmachinevalue( "Store", "Market" ) )
kernel.setvendor( kernel.getmachinevalue( "Store", "Banner" ) )

DefaultCulture = kernel.rental_get_default_culture()
OtherCulture = kernel.rental_get_alternate_culture()

kernel.log( "DefaultCulture is set to: ".. DefaultCulture )
kernel.log( "OtherCulture is set to: ".. OtherCulture )

if DataAccessAlreadySent == true then
	return
end

--************************************************
--
-- Define function calls used later in the script
--
--************************************************

local function initialize_scheduled_tasks()
	QLMMonitor = newqlmmonitor()
	QLMMonitor:start()

	ReservationManager = newreservationmanager()
	ReservationManager:init()
	kernel.sync_reservations()

	start_sync_timers()

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
	
	ShoppingCart:close_analytics( "True" )
	ShoppingCart:abandon( "Idle timer expired." )
	if DefaultCulture ~= nil and DefaultCulture ~= "" then
		kernel.setculture( DefaultCulture )
	else
		kernel.setculture( "en-US" )
	end
	localization_reload_resources()
	kernel.close_open_session_and_start_new_rental_session( "application_startup::idle_timer_expire_session" )
end

-- don't convert the idle_timer_callback to .net till all the views that use it are converted to .net
function idle_timer_callback()
	if kernel.is_collections_popup_timer_enabled() then
		kernel.check_collections_popup("time") 
	else
		idle_timer_show_prompt(idle_timer_expire_session)
	end
end

function idle_timer_show_prompt(expire_session)
    local render_type = kernel.getviewrendertype();
    if render_type == "WPF" then
	    if kernel.peekview() ~= "idle_timeout_message_view" then
		    view_show_safe( "idle_timeout_message_view" )
	    end
	    view_analytics_event( "context", "idleTimer" )
    else
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
end

local function start_the_application()
    if (kernel.peekview() == "application_load_view") then
        view_popview("remove startup wait view")
        -- Preload help view because if you are the first person to view help it occassionally shows a black screen 
        -- for up to 30 seconds and then shows the help screen. This issue should go away after all the views are WPF
        --view_show("help_document_view")
		kernel.show_full_help_view(false, false)
        view_popview("remove preloaded help document view")
    end
	if not run_initial_sync() then
		initialize_scheduled_tasks()
		if not kernel.hasvalidcardreader() then
			--kernel.pushview replaced with view_show function which will inturn call the kernel.pushview
            kernel.close_open_session_and_start_new_rental_session()
			kernel.handle_disconnected_usb_card_reader()
		else
			if not in_maintenance_mode() then
                kernel.close_open_session_and_start_new_rental_session()
			end
		end
	end

	-- initialize the user idle timer
    kernel.initialize_idle_timer("idle_timer", kernel.get_idle_timer_timeout(), idle_timer_callback)

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

	kernel.start_the_rental_application()
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
			-- This is the kiosk Engine jobs timer (separate from HAL job list)
			scheduler_services.start()
			configure_validate_stats_timers()
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
	EnableOutOfStockDisplay = kernel.getmachinevalueordefault(
		"Remote Services\\Inventory Services",
		"EnableOutOfStockDisplay",
		"False" ) == "True"
	PricingReloadTimeout = tonumber(kernel.getmachinevalueordefault(
		"Store",
		"PricingReloadTimeout",
		"300000" ) )
	VerboseLogging = kernel.getmachinevalueordefault(
		"Store",
		"VerboseLogging",
		"False" ) == "True"
	FMAMaintenanceMode = kernel.getmachinevalueordefault(
		"Field Maintenance",
		"MaintenanceMode",
		"False" ) == "true"
	ComingSoonOptIn = kernel.config_getvalue(
		"system",
		"General",
		"ComingSoonOptIn" ) == true
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
	GameEnableNintendoSwitch = kernel.config_getvalue(
		"system",
		"Games",
		"GameEnableNintendoSwitch" ) == true
	GameHasContentNintendoSwitch = kernel.getmachinevalueordefault(
		"Store",
		"GameHasContentNintendoSwitch",
		"false" ) == "true"
	ForcedAirExchangerConfigured = kernel.config_getvalue(
		"system",
		"General",
		"ForceAirExchangerConfiguration" ) == true
	BluRayUpsellEnabled = kernel.config_getvalue(
		"system",
		"General",
		"BluRayUpsellEnabled" ) == true
    SoftSyncTimer = kernel.config_getvalue(
		"system",
		"General",
		"SoftSyncTimer" ) or 3600000 -- one hour default
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

local function register_get_coming_soon_products_callback( )
    local get_coming_soon_products_callback = function ( )
        return coming_soon_product_list
    end

    kernel.register_get_coming_soon_products_callback( get_coming_soon_products_callback )
end


local function register_default_hardware_handlers()
	-- Hook up default hardware event handlers
	kernel.storesvcs_shouldreboot() --prime registry value; does not actually reboot the PC
	kernel.registerconfigurationcallback(
		"watch-config",
		function( store, action, path, key, value )
			if store == "machine"
			and action == "set" then
				if  path == "Store" and key == "ID" then
					kernel.log( "Configuration value changed for Store ID: " .. value )
					StoreManager:set_number( value )
					ReservationManager:init()
					kernel.sync_reservations()
				elseif path == "Remote Services\\Inventory Services" and key == "EnableOutOfStockDisplay" then
					kernel.log( "Configuration value changed for EnableOutOfStockDisplay: " .. value )
					EnableOutOfStockDisplay = value == "True"
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

            kernel.refresh_hardware_state()

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
				kernel.add_maintenance_mode_reason("HAL", "HALSTS-043")

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

				kernel.hardware_alert_manager_send_alert(
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

local function start_new_rental_session_callback() 
    kernel.log( "> Running start_new_rental_session_callback" )

    -- this code was formerly in start-view-enter.lua but was moved to so it can be called from .net
    -- commented out code is now in .net


    local _ProductCatalog = ProductCatalog
    strings = kernel.getdictionary( "strings" )

    -- Reload pricing in case the date has changed since last session
    if StoreManager ~= nil then
	    StoreManager:reload_pricing_data( "_v2" )
	    StoreManager:check_product_catalog_for_games_platforms()
    end
    kernel.log( "start-view-enter - started updating cache table")
    for k,v in _ProductCatalog:pairs() do
	    local product = MasterProductCatalog[v.product_id]
	    if product ~= nil then
			    local product_catalog = ProductCatalog[v.product_id]
			    local current_date = get_current_date_string( "%4d%02d%02d%02d%02d" )
			    local release_date = string.sub(product.merchandise_date, 1, 12)

                -- making changes to only update values when one changes, this loop is causing a performance problem when hitting the start over button
                local isAvailable = product_catalog.is_available
                local isComingSoon = product_catalog.is_coming_soon

			    local newIsAvailable = current_date > release_date or StoreManager.PricingManager:is_early_released( product )
			    local newIsComingSoon = not (release_date <= current_date	or kernel.getelapseddays( current_date, release_date ) > product.coming_soon_days)

                if ( (isAvailable ~= newIsAvailable) or (isComingSoon ~= newIsComingSoon)) then
			        product_catalog.is_available = newIsAvailable
			        product_catalog.is_coming_soon = newIsComingSoon
			        ProductCatalog[v.product_id] = product_catalog
                end
	    end
    end
    kernel.log( "start-view-enter - finished updating cache table")

    -- N.B. Added per Jim Polubinski for Quick Return integration
    --      If nothing was suspended with the companion function, this does nothing.
    --      JDP 20090903
    if FMAMaintenanceMode == false then
	    kernel.hardwaremaintenancemoderesumejobs()
    end

    if kernel.getlastshownview() == kernel.get_start_over_view_name() then
	    kernel.applysafeshutdown()
	    kernel.trimbitmapcache( 128 )
	    local hardware_manager = newhardwaremanager()
	    hardware_manager.hardware_event_responders.MaxPutAwayAttemptsExceeded = function( args, jobid )
		    kernel.hardware_alert_manager_send_alert_job(
			    "HardwareError",
			    "HAL script: check-picker.hs sent message MaxPutAwayAttemptsExceeded",
			    "Unable to put an item away after the configured number of slot retries. Placing the kiosk into maintenance mode",
			    jobid,
			    "mm:MaxPutAwayAttemptsExceeded, 005PMXA"
		    )
			kernel.add_maintenance_mode_reason("HAL", "005PMXA")
	    end
	    local jobs = hardware_manager:get_hardware_jobs_by_program(
		    { "check-picker" },
		    { "Stopped", "Completed", "Garbage", "Errored" } )
	    if table_count( jobs ) == 0 then
		    hardware_manager:check_picker()
	    end
    end

    if language_switch == nil or language_switch == false then
	    ShoppingCart = newshoppingcart()
	    ShoppingCart:start_new()
	    ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
    else
	    language_switch = false
    end

    ProductCatalog:get_default_family()


    function GetUsedMemory()
	    collectgarbage( "collect" )
    end

    GetUsedMemory()

end

-- callback from the .net side now that the start_idle_time is in .net
function start_idle_timer_callback()
    kernel.log("start idle timer callback")
	-- Now that analytics are being tracked on the carousel, the session has to be restarted on idle timeout
	if ShoppingCart ~= nil
	and ( kernel.analyticssessionexists( ShoppingCart.session_id ) == true or kernel.analyticsservicesessionexists( ShoppingCart.session_id ) == true ) then
        kernel.log("start view idle timer: closing analytics and abandoning cart ")
		ShoppingCart:close_analytics( "True" )
		ShoppingCart:abandon( "Start View User Idle Timeout" )
		ShoppingCart = newshoppingcart()
		ShoppingCart:start_new()
		ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
	end
end

--************************************************
--
-- Create Global class instances required for application function
--
--************************************************
StoreManager = newstoremanager()
kernel.register_pricing_service( 
    -- pricing set changed callback
    function() 
        if ReservationManager ~= nil then
            kernel.log("Pricing set changed. Initializing ReservationManager")
            ReservationManager:init()
        end
    end )
StoreManager:init()
StoreManager:reload_pricing_data( "_v2" )
StoreManager:check_product_catalog_for_games_platforms()
ShoppingCartList = {}
HardwareManager = newhardwaremanager()
kernel.create_idle_timer( "idle_timer" )
kernel.register_idle_timer_expire_session_callback(idle_timer_expire_session)
-- kernel.register_get_email_address_and_register_email_reminder_callback(get_email_address_and_register_email_reminder)
kernel.register_start_new_rental_session_callback(start_new_rental_session_callback)
kernel.register_start_idle_timer_callback(start_idle_timer_callback)
kernel.register_localization_reload_callback(localization_reload_resources)
register_get_coming_soon_products_callback()

--************************************************
--
-- Set Global variable values
--
--************************************************
quick_return_is_running = false

-- Nothing will work correctly if these values are not accurate
qlm_deck = tonumber( kernel.getmachinevalueordefault( "Store", "QlmDeck", "0" ) )
if qlm_deck == 0 then qlm_deck = nil end
max_decks = tonumber( kernel.getmachinevalueordefault( "Store", "MaxDecks", "0" ) )
if max_decks == 0 then max_decks = nil end
max_slots = tonumber( kernel.getmachinevalueordefault( "Store", "MaxSlots",	"0" ) )
if max_slots == 0 then max_slots = nil end

max_reco_items = 6
max_reco_cart_items = 5


--************************************************
--
-- Pre-load lookup tables and other key data elements
--
--************************************************
load_registry_key_values()
kernel.setculture( DefaultCulture )
localization_reload_resources()
SetESRBRatings()
config = kernel.getdictionary( "config" )
create_coming_soon_cache_tables()
register_default_hardware_handlers()

-- If the application has crashed from file access errors, do not continue
if DataAccessAlreadySent == true then
	return
end

-- [Vaibhav Pingle] Moved this section of the code from the initialize_scheduled_tasks() method
-- in order to be able to monitor the inventory sync jobs scheduled through the FMA, even though if
-- the Initial Sync job is running on the kiosk.
HardSyncMonitor = newhardsyncmonitor()
HardSyncMonitor:start(10000, 30000)

-- Run immediate thin job if secret <redacted> key is set (which includes rebalances)
if kernel.getmachinevalueordefault( "Store", "RemoteThinWithRebalRequested", "False" ) == "True" then
    kernel.timercreate(
	    "remote_thin_with_rebalances_timer",
	    5000,
	    nil,
	    function()
	        kernel.log( "Remote Thin Request found in Store registry" )
	        if qlm_deck == nil then
		        --vmz
		        local jobname = "vmzthin-with-rebalance-" .. tostring( newdatetime() )
		        run_thin_vmz_job( "Remote thin job with rebalance requested", jobname, nil, true )
	        else
		        --qlm
		        local jobname = "qlmthin-" .. tostring( newdatetime() )
		        run_thin_job( "Remote thin job requested", jobname )
	        end
	        kernel.setmachinevalue( "Store", "RemoteThinWithRebalRequested", "False" )
	        kernel.setmachinevalue( "Store", "RemoteThinRequested", "False" )
            kernel.timerremove("remote_thin_with_rebalances_timer")
        end )
    kernel.timerstart("remote_thin_with_rebalances_timer")
end

-- Run immediate thin job if secret <redacted> key is set
if kernel.getmachinevalueordefault( "Store", "RemoteThinRequested", "False" ) == "True" then
    kernel.timercreate(
	    "remote_thin_timer",
	    5000,
	    nil,
	    function()
	        kernel.log( "Remote Thin Request found in Store registry" )
	        if qlm_deck == nil then
		        --vmz
		        local jobname = "vmzthin-" .. tostring( schedule )
		        run_thin_vmz_job( "Remote thin job requested", jobname )
	        else
		        --qlm
		        local jobname = "qlmthin-" .. tostring( newdatetime() )
		        run_thin_job( "Remote thin job requested", jobname )
	        end
	        kernel.setmachinevalue( "Store", "RemoteThinRequested", "False" )
            kernel.timerremove("remote_thin_timer")
        end )
    kernel.timerstart("remote_thin_timer")
end

if #newhardwaremanager():get_hardware_jobs_by_label_contains_text( "maintenance sync by ", { "Stopped", "Errored", "Garbage", "Suspended", "Completed" } ) > 0 then
	view_show( "maintenance_sync_view" )
	return
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
			kernel.clear_hal_maintenance_mode()
			verify_hardware_status_before_launching_application()
		elseif initstatus["status"] == nil then
			kernel.add_maintenance_mode_reason("HAL", "HALSVCNR")
		end
	end )
kernel.timerstart("hal_init_status_check_timer")

kernel.timercreate(
	"pricing-reload-timer",
	PricingReloadTimeout,
	PricingReloadTimeout,
	function()
		if StoreManager ~= nil
		and kernel.is_rental_session_active() == false then
			kernel.log( "Reloading Pricing information..." )
			StoreManager:reload_pricing_data( "_v2" )
			StoreManager:check_product_catalog_for_games_platforms()
            kernel.reload_carousel(false)
		else
			kernel.log( "Cannot Reload Pricing since User is on the Flip View or the bundle is Not on Start View." )
		end
	end
)
kernel.timerstart("pricing-reload-timer")

kernel.timercreate("inventory-check-for-new-files-timer",
	26000,
	120000,
	function()
		kernel.rental_inventory_check_for_new_files(
		function()
			if qlm_deck == nil then
		
				if StoreManager ~= nil then
					kernel.log( "Refresh rapid status data..." )
					StoreManager:refresh_rapid_statuses()
				end		
			
				--terminate clean-vmz jobs
				kernel.log( "r> terminate any existing clean-vmz and thin-vmz jobs" )
				local hwmgr = newhardwaremanager()
				local currentjobs = hwmgr:get_hardware_jobs_by_program({ "clean-vmz","thin-vmz" }, { "Garbage", "Stopped", "Completed", "Errored" })
				for k, v in ipairs( currentjobs ) do
					kernel.hardwareterminatejob( v.id )
				end

				kernel.log("run thin-vmz job passing all parameters (thin-to-bin, rebalances, redeploys, thins)")
				local jobname = "vmzthin-" .. tostring( newdatetime() ) 
				run_thin_vmz_job( 
					"Automated Rebalance", 
					jobname, 
					nil, 
					true,
					function( jobid, status )
                        
						if status == "Completed" or status == "Errored" then
							kernel.log( "> hardwarevmzthin2 complete, now running clean_vmz")
							hwmgr:clean_vmz( 
								function()
									kernel.log("run soft sync")
									run_soft_sync( nil )
								end)
						end
					end )
			end
		end,
		nil )
		
		if StoreManager ~= nil then
			kernel.log( "Refresh rapid status data..." )
			StoreManager:refresh_rapid_statuses()
		end
	end
)
kernel.timerstart("inventory-check-for-new-files-timer")

kernel.timercreate("blocklist-check-timer",
	28000,
	3600000,
	function()
		kernel.rental_check_for_new_blocklist()
	end
)
kernel.timerstart("blocklist-check-timer")

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

kernel.queuekioskfunctioncheck()

kernel.marketing_ad_start_application()

-- force load of first xps help document to speed up load of help screen
kernel.load_xps_document("document-faqs")

-- force load of product images
kernel.cache_product_images()

kernel.create_schedule_fraud_sync_timer()

kernel.checkreadervalidation()

kernel.sync_unknowns(qlm_deck, false)