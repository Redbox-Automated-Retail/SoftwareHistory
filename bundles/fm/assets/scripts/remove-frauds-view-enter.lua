--
-- remove-frauds-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved

--the number of items from output_log that can be displayed and not the actual lines in the output window.
number_of_log_items_visible = 14
max_characters_per_line = 135
output_log = {}
scroll_step_size = 1
scroll_index = 1

function get_output_text( start_index )
	if start_index == nil then
		start_index = 1
	end

	local display_offset = number_of_log_items_visible - 1

	if #output_log < number_of_log_items_visible then
		display_offset = #output_log - 1
	end

	local end_index = start_index + display_offset

	local output_text = ""

	for i = start_index, end_index do
		accomodated_text = tostring( #output_log - i + 1 ).. ": ".. output_log[i]
		while #accomodated_text > max_characters_per_line do
			accomodated_text = string.sub( accomodated_text, 0, string.len( accomodated_text ) - 5 ).. "..."
		end
		output_text = output_text.. accomodated_text.. "\n"
	end

	return output_text
end

function fit_output_text( output_text )
	kernel.setactortext( "output_window_textbox", get_output_text() )
end

function add_to_output( str )
	table.insert( output_log, 1, str )
	fit_output_text( get_output_text() )
	scroll_index = 1
end

function scroll_up( step )
	if scroll_index == 1 then
		return
	end

	if step == nil
	or step == 0 then
		step = 1
	end

	scroll_index = scroll_index - step
	if scroll_index < 1 then
		scroll_index = 1
	end

	kernel.setactortext( "output_window_textbox", get_output_text( scroll_index ) )
end

function scroll_down( step )
	if scroll_index == #output_log - number_of_log_items_visible + 1
	or #output_log <= number_of_log_items_visible then
		return
	end

	if step == nil
	or step == 0 then
		step = 1
	end

	scroll_index = scroll_index + step
	if scroll_index + number_of_log_items_visible > #output_log then
		scroll_index = #output_log - number_of_log_items_visible + 1
	end

	kernel.setactortext( "output_window_textbox", get_output_text( scroll_index ) )
end

kernel.registeractorhitnotify(
	"image_up_arrow",
	function()
		idle_timer:reset()
		scroll_up( scroll_step_size )
	end
)

kernel.registeractorhitnotify(
	"up_arrow_background",
	function()
		idle_timer:reset()
		scroll_up( scroll_step_size )
	end
)

kernel.registeractorhitnotify(
	"image_down_arrow",
	function()
		idle_timer:reset()
		scroll_down( scroll_step_size )
	end
)

kernel.registeractorhitnotify(
	"down_arrow_background",
	function()
		idle_timer:reset()
		scroll_down( scroll_step_size )
	end
)

progress_manager:stop()
FraudAlertManager = newalertmanager()

local resume_result = nil
--shutdown the scheduler service to prevent scheduled syncs, thin, etc from starting
scheduler_services.shutdown()

local hardware_manager = newhardwaremanager()
local overlay_progress_manager = newprogressmanager()

local get_frauds = function()
	local frauds = {}
	for _, v in ipairs( Inventory_All_Table ) do
		if v.status == "Fraud" then
			table.insert( frauds, { deck=v.deck, slot=v.slot } )
			kernel.log( tostring( v.deck ) .. "@" .. tostring ( v.slot ) .. " has been inserted into frauds table." )
		end
	end
	return frauds
end

local frauds = get_frauds()
local init_fraud_count = #frauds
local failed_vend_locations = {}

local update_frauds_remaining_count = function()
	kernel.log( "#frauds: " .. tostring( #frauds ) )
	kernel.setactortext( "frauds_remaining_count", tostring( ( #frauds ) ) )
	
	if #frauds == 0 then
		add_to_output( "There are no Fraud discs remaining in the Kiosk." )
	end
end
update_frauds_remaining_count()

if #frauds == 0 then
	kernel.disableactor( "pull_button" )
	kernel.setactorforegroundcolor( "pull_button", "gray" )
end
kernel.disableactor( "pause_button" )
kernel.setactorforegroundcolor( "pause_button", "gray" )

local vend_successful = function( results )
	for _, v in ipairs( results ) do
		if v.code == "ItemVended" then
			return true
		end
	end
	return false
end

local return_successful = function( results )
	for _, v in ipairs( results ) do
		if v.code == "ItemReturned" then
			return true
		end
	end
	return false
end

local peek_timeout = function( results )
	for _, v in ipairs( results ) do
		if v.code == "GetAfterPeekShowsFull" then
			return true
		end
	end
	return false
end

local soft_sync_and_resume_services = function()
	overlay_progress_manager:start( false )
	hardware_manager:soft_sync(
		function()
			reload_inventory()
			overlay_progress_manager:stop()
			view_goback()
		end
	)
	if not maintenance_mode then
		resume_result = kernel.hardwaremaintenancemoderesumejobs()
		if not resume_result.success then
			newmessageboxmanager():show( "Error resuming suspended jobs." )
		end
	end
	scheduler_services.start()
end

local is_failed_vend = function( location )
	for k, v in ipairs( failed_vend_locations ) do
		if location.deck == v.deck and location.slot == v.slot then
			return true
		end
	end
	return false
end

local get_next_fraud_location = function()
	for _, l in ipairs( frauds ) do
		local location = { ["deck"] = l.deck, ["slot"] = l.slot }
		if is_failed_vend( location ) == false then
			return location
		end
	end
	return nil
end

function remove_fraud_discs()
	kernel.log( frauds )
	--Using vend_locations to remove fraud disc
	local fraud_location = get_next_fraud_location()

	if fraud_location ~= nil then
		local locations = { [1] = fraud_location }
		local result = hardware_manager:vend_locations(
			locations,
			function( status, job_result )
				kernel.log( job_result )
				if vend_successful( job_result.results ) == true then
					for _, v in ipairs( job_result.results ) do
						if v.code == "ItemVended" then
							local deck_slot_key = tostring( fraud_location.deck ).. "@".. tostring( fraud_location.slot )
							BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }
							kernel.log( "> " .. v.item_id .. " added to RemovedQueue." )
							RemovedQueue[ v.item_id ] = { ["barcode"] = v.item_id, ["status"] = "Success" }
							
							remove_barcode( v.item_id )
							
							kernel.healthsvcs_sendauditevent(
								StoreID,
								"FMA_FraudPulled",
								username,
								"FMA Bundle",
								deck_slot_key.. " ".. v.item_id
							)
							
							-- Send DiscAction Message
							if DiscActionLogging == true and v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
								barcode_list = { }
								barcode_list[1] = { tostring(v.item_id), "1" } -- action 0=Inserted  1=Removed
								kernel.fmadiscaction(
									StoreID,
									username,
									"fraud_vended",
									v.time_stamp24,
									barcode_list)
							end

							add_to_output( "ITEM VEND: ".. v.item_id.. " has been successfully pulled from ".. deck_slot_key.. "." )
							
							reload_inventory()
							frauds = get_frauds()
							update_frauds_remaining_count()
						else
							add_to_output( "ITEM VEND: ".. tostring( v.code ).. ": ".. tostring( v.message ) )
							table.insert( failed_vend_locations, fraud_location )
						end
					end
					if #frauds > 0
					and PauseFraudRemoval == false then
						remove_fraud_discs()
					else
						if PauseFraudRemoval == true then
							add_to_output( "Fraud Removal Paused." )
							PauseFraudRemoval = false
						end
						if #frauds > 0 then
							kernel.enableactor( "pull_button" )
							kernel.setactorforegroundcolor( "pull_button", "white" )
						end
						kernel.disableactor( "pause_button" )
						kernel.setactorforegroundcolor( "pause_button", "gray" )
						kernel.enableactor( "close_area" )
						kernel.enableactor( "close_button" )
						kernel.setactorforegroundcolor( "close_button", "white" )
						kernel.hideactor( "title_warning_message" )
						--overlay_progress_manager:stop()
					end
				else
					for _, v in ipairs( job_result.results ) do
						local deck_slot_key = tostring( fraud_location.deck ) .. "@" .. tostring( fraud_location.slot )
						add_to_output( "ITEM VEND: " .. tostring( v.code ) .. ": " .. tostring( v.message ) )
						table.insert( failed_vend_locations, fraud_location )
						reload_inventory()
						frauds = get_frauds()
						update_frauds_remaining_count()
					end
					if PauseFraudRemoval == true then
						add_to_output( "Fraud Removal Paused." )
						PauseFraudRemoval = false
					end
					if #frauds > 0 then
						kernel.enableactor( "pull_button" )
						kernel.setactorforegroundcolor( "pull_button", "white" )
					end
					kernel.disableactor( "pause_button" )
					kernel.setactorforegroundcolor( "pause_button", "gray" )
					kernel.enableactor( "close_area" )
					kernel.enableactor( "close_button" )
					kernel.setactorforegroundcolor( "close_button", "white" )
					kernel.hideactor( "title_warning_message" )
				end
			end,
			nil,
			nil
		)
	else
		if #frauds > 0
		and #failed_vend_locations > 0 then
			add_to_output( "There were errors while vending from certain vend locations." )
		end
		PauseFraudRemoval = false
		if #frauds > 0 then
			kernel.enableactor( "pull_button" )
			kernel.setactorforegroundcolor( "pull_button", "white" )
		end
		kernel.disableactor( "pause_button" )
		kernel.setactorforegroundcolor( "pause_button", "gray" )
		kernel.enableactor( "close_area" )
		kernel.enableactor( "close_button" )
		kernel.setactorforegroundcolor( "close_button", "white" )
		kernel.hideactor( "title_warning_message" )
	end
end

kernel.registeractorhitnotify(
	"pause_button",
	function()
		kernel.log( "User hit Pause button." )
		add_to_output( "Fraud removal will pause after current disc." )
		PauseFraudRemoval = true
		kernel.disableactor( "pause_button" )
		kernel.setactorforegroundcolor( "pause_button", "gray" )
	end
)

kernel.registeractorhitnotify(
	"pull_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Frauds: User hits 'Pull Frauds'" )
		idle_timer:reset()
		
		kernel.disableactor( "pull_button" )
		kernel.setactorforegroundcolor( "pull_button", "gray" )
		kernel.enableactor( "pause_button" )
		kernel.setactorforegroundcolor( "pause_button", "white" )
		kernel.disableactor( "close_area" )
		kernel.disableactor( "close_button" )
		kernel.setactorforegroundcolor( "close_button", "gray" )
		kernel.showactor( "title_warning_message" )
		PauseFraudRemoval = false
		failed_vend_locations = {}
		
		remove_fraud_discs()
	end
)

kernel.registeractorhitnotify(
	"close_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Frauds: User hits 'Close'" )
		idle_timer:reset()
		soft_sync_and_resume_services()
	end
)

kernel.registeractorhitnotify(
	"close_area",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Frauds: User hits 'X'" )
		idle_timer:reset()
		soft_sync_and_resume_services()
	end
)