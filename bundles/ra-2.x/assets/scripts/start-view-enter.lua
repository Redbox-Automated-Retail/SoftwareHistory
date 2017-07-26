--
-- start-view-enter.lua
-- Copyright (C) 2017 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
kernel.reset_browse_controls()
kernel.ttsinreturnflow(false)
kernel.timerstart( "reservation-expire-timer" )

idle_timer:stop()

kernel.initialize_session()

price_set_item = false  
-- idle timer needs to be reinitialized because of the collections popup
initialize_idle_timer()
overlay_scene_manager = newtitlerollupoverlay()  -- this can be removed when the browse view is converted to WPF

if kernel.ttsenabled() or kernel.get_application_state_value("IsADAAccessible") == true then
    kernel.set_application_state_value("IsADAAccessible", true)
end

kernel.setlasthitactorname( nil )

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

ProductGenreData = {
	["Movies"] = {
		filter_genres = {
			[1093] = strings.lower_genre_1093,
			[1000] = strings.lower_genre_1000,
			[1004] = strings.lower_genre_1004,
			[1005] = strings.lower_genre_1005,
			[1006] = strings.lower_genre_1006,
			[1009] = strings.lower_genre_1009
		}
	},
	["Games"] = {
		filter_genres = {
			[1026] = strings.lower_genre_1026,
			[1094] = strings.lower_genre_1006,
			[1028] = strings.lower_genre_1028,
			[1030] = strings.lower_genre_1030,
			[1034] = strings.lower_genre_1034,
			[1036] = strings.lower_genre_1036
		}
	}
}

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
		AlertManager.send_job_alert(
			"HardwareError",
			"HAL script: check-picker.hs sent message MaxPutAwayAttemptsExceeded",
			"Unable to put an item away after the configured number of slot retries. Placing the kiosk into maintenance mode",
			jobid,
			"mm:MaxPutAwayAttemptsExceeded, 005PMXA"
		)
		if not in_maintenance_mode() then
			kernel.setmacro( "MaintenanceView:ErrorCode", "005PMXA" )
			kernel.setmacro( "MaintenanceView:BackgroundImage", MAINTENANCE_MODE_BACKGROUND[ "005PMXA" ] or "red_diagonal_stripes" )
			view_show( "maintenance_view" )
		end
	end
	local jobs = hardware_manager:get_hardware_jobs_by_program(
		{ "check-picker" },
		{ "Stopped", "Completed", "Garbage", "Errored" } )
	if table_count( jobs ) == 0 then
		hardware_manager:check_picker()
	end
end

-- Pre-setting these values back to defaults here saves time
kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
kernel.set_session_value("BrowseState.BrowseGenre", nil)

kernel.set_session_value("BrowseState.IsShoppingCartFullMessagesOpen", false)
kernel.set_session_value("BrowseState.ShoppingCartFullMessagesShown", false)

-- Handles those edge cases where the user does not come from the standard UI path
kernel.log( "CloseAnalyticsOnStartView = " .. tostring( CloseAnalyticsOnStartView ))
if CloseAnalyticsOnStartView == true then
	ShoppingCart:close_analytics( "True" )
	CloseAnalyticsOnStartView = false
end

if language_switch == nil or language_switch == false then
	ShoppingCart = newshoppingcart()
	ShoppingCart:start_new()
	ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
else
	language_switch = false
end

kernel.set_session_value("BrowseState.BrowseTitleFamily", "Movies")
ProductCatalog:get_default_family()

-- callback from the .net side now that the start_idle_time is in .net
function start_idle_timer_callback()
    kernel.log("start idle timer callback")
	-- Now that analytics are being tracked on the carousel, the session has to be restarted on idle timeout
	if ShoppingCart ~= nil
	and kernel.analyticssessionexists( ShoppingCart.session_id ) == true then
		ShoppingCart:close_analytics( "True" )
		CloseAnalyticsOnStartView = false
		ShoppingCart:abandon( "Start View User Idle Timeout" )
		ShoppingCart = newshoppingcart()
		ShoppingCart:start_new()
		ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
	end
end

function GetUsedMemory()
	collectgarbage( "collect" )
end

GetUsedMemory()

function is_product_in_shoppingCart( product )
    local result = false
    if (product ~= nil) then
        result = ShoppingCart:has_item( product.product_id ) == true
        kernel.log("IsProductInShoppingCart " .. tostring(product.product_id) .. " : " .. tostring(result))
    end
    return result
end

function is_product_early_release( product )
    local result = false
    result = StoreManager.PricingManager:is_early_released( product ) 
    if (product ~= nil) then
        kernel.log("IsProductEarlyRelease " .. tostring(product.product_id) .. " : " .. tostring(result))
    end
    return result
end


kernel.configure_start_view(localization_reload_resources, start_idle_timer_callback, show_browse_view_collections, show_collection_items, check_for_warning_popups, is_product_in_shoppingCart, is_product_early_release)
