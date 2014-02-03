--
-- unknown-discs-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved

progress_manager:stop()
UnknownsAlertManager = newalertmanager()

local resume_result = nil
--shutdown the scheduler service to prevent scheduled syncs, thin, etc from starting
scheduler_services.shutdown()

local hardware_manager = newhardwaremanager()
local overlay_progress_manager = newprogressmanager()

local excluded_unknowns = {}
local is_excluded = function( deck, slot )
	for _, v in ipairs( excluded_unknowns ) do
		if v.deck == deck and v.slot == slot then
			kernel.log( tostring( deck ) .. "@" .. tostring ( slot ) .. " is excluded." )
			return true
		end
	end
	kernel.log( tostring( deck ) .. "@" .. tostring ( slot ) .. " is not excluded." )
	return false
end

local possible_stuck_discs = {}
local is_possible_stuck_disc = function( deck, slot )
	for _, v in ipairs( possible_stuck_discs ) do
		if v.deck == deck and v.slot == slot then
			kernel.log( tostring( deck ) .. "@" .. tostring ( slot ) .. " is a possible stuck disc." )
			return true
		end
	end
	kernel.log( tostring( deck ) .. "@" .. tostring ( slot ) .. " is not a possible stuck disc." )
	return false
end

local get_unknowns = function()
	local unknowns = {}
	for _, v in ipairs( Inventory_All_Table ) do
		if v.barcode == "UNKNOWN" and is_excluded( v.deck, v.slot ) == false and is_possible_stuck_disc( v.deck, v.slot ) == false then
			table.insert( unknowns, { deck=v.deck, slot=v.slot } )
			kernel.log( tostring( v.deck ) .. "@" .. tostring ( v.slot ) .. " has been inserted into unknowns table." )
		end
	end
	return unknowns
end
local unknowns = get_unknowns()
local init_unknown_count = #unknowns

local update_unknowns_remaining_count = function()
	kernel.log( "#unknowns: " .. tostring( #unknowns ) .. " and #excluded_unknowns: " .. tostring ( #excluded_unknowns ) )
	kernel.setactortext( "unknowns_remaining_count", tostring( ( #unknowns + #excluded_unknowns ) ) )

	if #unknowns == 0 and ( #excluded_unknowns > 0 or #possible_stuck_discs > 0 ) and kernel.isactorenabled( "yes_button" ) == false and kernel.isactorenabled( "no_button" ) == false then
		add_to_output( "All remaining Unknowns have errored, please close this window and manually check remaining Unknowns." )
	end

	if #unknowns ~= 0 then
		kernel.disableactor( "confirm_button" )
		kernel.setactorforegroundcolor( "confirm_button", "gray" )
	else
		kernel.disableactor( "pull_button" )
		kernel.setactorforegroundcolor( "pull_button", "gray" )
		
		if kernel.isactorenabled( "yes_button" ) == false and kernel.isactorenabled( "no_button" ) == false and #excluded_unknowns == 0 then
			kernel.enableactor( "confirm_button" )
			kernel.setactorforegroundcolor( "confirm_button", "white" )
		end
	end
end
update_unknowns_remaining_count()

local update_possible_stuck_discs_count = function()
	kernel.log( "#possible_stuck_discs: " .. tostring( #possible_stuck_discs ) )
	kernel.setactortext( "possible_stuck_discs_count", tostring( ( #possible_stuck_discs ) ) )
end
update_possible_stuck_discs_count()

local current_unknown_return_time = "NONE"
local current_possible_stuck_disc = false

kernel.enableactor( "pull_button" )
kernel.setactorforegroundcolor( "pull_button", "white" )
kernel.disableactor( "yes_button" )
kernel.setactorforegroundcolor( "yes_button", "gray" )
kernel.disableactor( "no_button" )
kernel.setactorforegroundcolor( "no_button", "gray" )
kernel.disableactor( "hardware_issue_button" )
kernel.setactorforegroundcolor( "hardware_issue_button", "gray" )

local vend_successful = function( results )
	for _, v in ipairs( results ) do
		if v.code == "ItemVended" or v.code == "SlotEmpty" then
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

local completion_summary = { 
	["user"] = tostring( username ),
	["begin_count"] = tostring( init_unknown_count ),
	["pulled"] = {},
	["reinserted"] = {},
	["notreinserted"] = {},
	["hardware_issues"] = {},
	["exit_method"] = "",
	["errors"] = ""
}
	
local send_summary_alert = function()
	local valid_pull_count = 0
	local valid_return_count = 0
	local notreinserted_count = 0
	local hardware_issue_count = 0
	
	for _, v in ipairs( completion_summary.pulled ) do
		if v.status == "Successful pull." then
			valid_pull_count = valid_pull_count + 1
		end
	end
	
	for _, v in ipairs( completion_summary.reinserted ) do
		if v.status == "Successful return." then
			valid_return_count = valid_return_count + 1
		end
	end
	
	notreinserted_count = #completion_summary.notreinserted
	
	hardware_issue_count = #completion_summary.hardware_issues

	local buffer = 
	"<h2>" .. "Summary of Remove Unknown process" .. "</h2>" ..
	"<p>" .. 
	"Remove unknown process initiated by: " .. tostring( completion_summary.user ) ..
	"<br>" .. "Number of unknowns at the beginning of process: " .. tostring( completion_summary.begin_count ) ..
	"<br>" .. "Number of unknowns removed: " .. tostring( valid_pull_count ) ..
	"<br>" .. "Number of unknowns reinserted: " .. tostring( valid_return_count ) ..
	"<br>" .. "Number of unknowns not reinserted: " .. tostring( notreinserted_count ) ..
	"<br>" .. "Number of hardware issues: " .. tostring( hardware_issue_count ) ..
	"<br>" .. "Method of exiting: " .. tostring( completion_summary.exit_method ) ..
	"</p><br>" ..
	
	"<h2>" .. "Pulls" .. "</h2>"
	for _, v in ipairs( completion_summary.pulled ) do
		buffer = buffer ..
		"Status: " .. tostring( v.status ) ..
		"<br>" .. "Deck@Slot: " .. tostring( v.key ) ..
		"<br>"
	end
	
	buffer = buffer ..
	"<h2>" .. "Returns" .. "</h2>"
	for _, v in ipairs( completion_summary.reinserted ) do
		buffer = buffer ..
		"Status: " .. tostring( v.status ) ..
		"<br>" .. "Deck@Slot: " .. tostring( v.key ) ..
		"<br>" .. "Barcode: " .. tostring( v.barcode ) ..
		"<br>"
	end
	
	buffer = buffer ..
	"<h2>" .. "Hardware Issues" .. "</h2>"
	for _, v in ipairs( completion_summary.hardware_issues ) do
		buffer = buffer ..
		"Status: " .. tostring( v.status ) ..
		"<br>" .. "Deck@Slot: " .. tostring( v.key ) ..
		"<br>"
	end
	
	UnknownsAlertManager.send_alert(
		"TitleEmpty",
		"FMA: Remove Unknowns",
		buffer,
		nil,
		tostring( completion_summary.errors ),
		"FMA: Remove Unknowns"
	)
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

kernel.registeractorhitnotify(
	"pull_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'Pull Unknown'" )
		idle_timer:reset()

		overlay_progress_manager:start( false )

		local result = hardware_manager:vend_unknown(
			unknowns[1].deck,
			unknowns[1].slot,
			function( job_result )	
				overlay_progress_manager:stop()
				local errors = ""
				if vend_successful( job_result.results ) == false then
					if peek_timeout( job_result.results ) == false then
						for _, v in ipairs( job_result.results ) do
							if v.code ~= "ItemVended" then
								add_to_output( "ITEM VEND: " .. tostring( v.code ) .. ": " .. tostring( v.message ) )
								errors = errors .. tostring( v.code ) .. ": " .. tostring( v.message ) .. ". "
							end
						end

						table.insert( excluded_unknowns, { deck=unknowns[1].deck, slot=unknowns[1].slot } )
						table.insert( completion_summary.pulled, {key=tostring( unknowns[1].deck ) .. "@" .. tostring( unknowns[1].slot ), status="Failed pull: " .. errors } )
						kernel.log( tostring( unknowns[1].deck ) .. "@" .. tostring ( unknowns[1].slot ) .. " has been inserted into excluded_unknowns table." )

						reload_inventory()				
						unknowns = get_unknowns()
						update_unknowns_remaining_count()
					else
						local deck_slot_key = tostring( unknowns[1].deck ) .. "@" .. tostring( unknowns[1].slot )
						add_to_output( "ITEM VEND: SlotProblem: Possible stuck disc at " .. deck_slot_key .. ". Please inspect the slot manually." )
						
						kernel.disableactor( "close_area" )
						kernel.disableactor( "pull_button" )
						kernel.setactorforegroundcolor( "pull_button", "gray" )
						kernel.enableactor( "yes_button" )
						kernel.setactorforegroundcolor( "yes_button", "white" )
						kernel.enableactor( "no_button" )
						kernel.setactorforegroundcolor( "no_button", "white" )
						kernel.enableactor( "hardware_issue_button" )
						kernel.setactorforegroundcolor( "hardware_issue_button", "white" )
						
						current_unknown_return_time = "NONE"
						for _, v in ipairs( job_result.results ) do
							if v.return_time ~= nil then
								current_unknown_return_time = v.return_time
							end
						end
						
						for _, v in ipairs( job_result.results ) do
							if v.code == "MoveOffsetOk" then
								add_to_output( tostring( v.message ) )
							end
						end
						
						current_possible_stuck_disc = true
						table.insert( possible_stuck_discs, { deck=unknowns[1].deck, slot=unknowns[1].slot } )
						update_possible_stuck_discs_count()

						reload_inventory()
						unknowns = get_unknowns()
						update_unknowns_remaining_count()
					end
				else
					for _, v in ipairs( job_result.results ) do
						if v.code ~= "ItemVended" and v.code ~= "SlotEmpty" then
							add_to_output( "ITEM VEND: " .. tostring( v.code ) .. ": " .. tostring( v.message ) )
						end

						if v.code == "ItemVended" then
							local deck_slot_key = tostring( unknowns[1].deck ) .. "@" .. tostring( unknowns[1].slot )
							BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }
						
							kernel.healthsvcs_sendauditevent(
								StoreID,
								"FMA_UnknownPulled",
								username,
								"FMA Bundle",
								deck_slot_key .. " UNKNOWN"
							)

							-- Send DiscAction Message
							if DiscActionLogging == true and v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
								barcode_list = { }
								barcode_list[1] = { tostring(v.item_id), "1" } -- action 0=Inserted  1=Removed
								kernel.fmadiscaction(
									StoreID,
									username,
									"unknown-vended",
									v.time_stamp24,
									barcode_list)
							end
						
							current_unknown_return_time = "NONE"
							if v.return_time ~= nil then
								current_unknown_return_time = v.return_time
							end
							add_to_output( "ITEM VEND: UNKNOWN has been successfully pulled from " .. deck_slot_key .. "." )
							table.insert( completion_summary.pulled, { key=deck_slot_key, return_time=current_unknown_return_time, status="Successful pull." } )
							kernel.disableactor( "close_area" )
							kernel.disableactor( "pull_button" )
							kernel.setactorforegroundcolor( "pull_button", "gray" )
							kernel.enableactor( "yes_button" )
							kernel.setactorforegroundcolor( "yes_button", "white" )
							kernel.enableactor( "no_button" )
							kernel.setactorforegroundcolor( "no_button", "white" )
						
							reload_inventory()
							unknowns = get_unknowns()
							update_unknowns_remaining_count()
						end

						if v.code == "SlotEmpty" then
							local deck_slot_key = tostring( unknowns[1].deck ) .. "@" .. tostring( unknowns[1].slot )
							BarcodePosition[deck_slot_key] = { ["barcode"] = "EMPTY", ["status"] = "EMPTY" }
							add_to_output( "ITEM VEND: SlotEmpty: The slot at " .. deck_slot_key .. " is empty." )
							table.insert( completion_summary.pulled, { key=deck_slot_key, return_time="NONE", status="Failed pull, slot empty." } )
							reload_inventory()
							unknowns = get_unknowns()
							update_unknowns_remaining_count()
						end
					end
				end

				if job_result.status == "Errored" then
					completion_summary.errors = completion_summary.errors .. UnknownsAlertManager.format_errors( job_result.job_id )
				end
			end,
			nil,
			nil
		)
	end
)

kernel.registeractorhitnotify(
	"yes_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'Yes'" )
		idle_timer:reset()

		overlay_progress_manager:start( false )

		local result = hardware_manager:return_unknown(
			current_unknown_return_time,
			nil,
			function( job_result )
				overlay_progress_manager:stop()
				
				if return_successful( job_result.results ) == false then
					table.insert( completion_summary.reinserted, { key="NONE", status="Failed return." } )
				end
				
				for _, v in ipairs( job_result.results ) do
					if v.code ~= "BarcodeRead" then
						if v.code ~= "ItemReturned" then
							add_to_output( "ITEM RETURN: " .. tostring( v.code ) .. ": " .. tostring( v.message ) )
						else
							add_to_output( "ITEM RETURN: " .. tostring( v.message ) )
						end
					end					
					
					if v.code == "ItemReturned"
					and v.item_id ~= nil
					and trim( v.item_id ) ~= ""
					and string.lower( v.item_id ) ~= "unknown"
					and string.lower( v.item_id ) ~= "redbox" then
						local deck_slot_key = tostring( v.deck ) .. "@" .. tostring( v.slot )

						kernel.invsvcs_return(
							StoreID,
							"BackwardsDiscReturn",
							v.item_id,
							v.deck,
							v.slot,
							current_unknown_return_time
						)
						table.insert( completion_summary.reinserted, { key=deck_slot_key, barcode=v.item_id, return_time=current_unknown_return_time, status="Successful return." } )

						kernel.healthsvcs_sendauditevent(
							StoreID,
							"FMA_UnknownReinserted",
							username,
							"FMA Bundle",
							deck_slot_key .. " " .. tostring( v.item_id )
						)
						
						-- Send DiscAction Message
						if DiscActionLogging == true and v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
							barcode_list = { }
							barcode_list[1] = { tostring(v.item_id), "0" } -- action 0=Inserted  1=Removed
							kernel.fmadiscaction(
								StoreID,
								username,
								"unknown-reinserted",
								v.time_stamp24,
								barcode_list)
						end
						
						--copied from inventory-insert-disc.lua	
						local product = BarcodeToProduct[v.item_id]
						if product ~= nil then
							local master_product = MasterProductCatalog[product.product_id]
							if master_product ~= nil then 
								product_name = MasterProductCatalog[product.product_id].long_name 
							end
							local history = ProductHistory[product.product_id]
							if history ~= nil then
								if ( history.ofr_count or 0 ) > 0 then history.ofr_count = (history.ofr_count or 0 ) - 1 end
								history.on_hand_count = (history.on_hand_count or 0 ) + 1
								ProductHistory[product.product_id] = history
							end
						end
						--

						kernel.enableactor( "close_area" )
						kernel.enableactor( "pull_button" )
						kernel.setactorforegroundcolor( "pull_button", "white" )
						kernel.disableactor( "yes_button" )
						kernel.setactorforegroundcolor( "yes_button", "gray" )
						kernel.disableactor( "no_button" )
						kernel.setactorforegroundcolor( "no_button", "gray" )
						kernel.disableactor( "hardware_issue_button" )
						kernel.setactorforegroundcolor( "hardware_issue_button", "gray" )

						if current_possible_stuck_disc == true then
							overlay_progress_manager:start( false )
							
							hardware_manager:executeimmediate(
								"LOC RESET DECK=" .. possible_stuck_discs[#possible_stuck_discs].deck .. " SLOT=" .. possible_stuck_discs[#possible_stuck_discs].slot,
								false,
								function()
									hardware_manager:soft_sync(
										function()
											current_possible_stuck_disc = false
											table.remove( possible_stuck_discs, #possible_stuck_discs )
											update_possible_stuck_discs_count()
											
											reload_inventory()
											unknowns = get_unknowns()
											update_unknowns_remaining_count()
											
											overlay_progress_manager:stop()
										end
									)
								end
							)
						else
							reload_inventory()
							unknowns = get_unknowns()
							update_unknowns_remaining_count()
						end
					elseif v.code == "ItemReturned" then
						local deck_slot_key = tostring( v.deck ) .. "@" .. tostring( v.slot )
						BarcodePosition[deck_slot_key] = { ["barcode"] = "UNKNOWN", ["status"] = "Unknown Title" }

						kernel.healthsvcs_sendauditevent(
							StoreID,
							"FMA_UnknownReinserted",
							username,
							"FMA Bundle",
							deck_slot_key .. " UNKNOWN"
						)

						table.insert( completion_summary.reinserted, { key=deck_slot_key, barcode="UNKNOWN", return_time=current_unknown_return_time, status="Successful return." } )
						kernel.enableactor( "close_area" )
						kernel.enableactor( "pull_button" )
						kernel.setactorforegroundcolor( "pull_button", "white" )
						kernel.disableactor( "yes_button" )
						kernel.setactorforegroundcolor( "yes_button", "gray" )
						kernel.disableactor( "no_button" )
						kernel.setactorforegroundcolor( "no_button", "gray" )
						kernel.disableactor( "hardware_issue_button" )
						kernel.setactorforegroundcolor( "hardware_issue_button", "gray" )

						if current_possible_stuck_disc == true then
							overlay_progress_manager:start( false )
							
							hardware_manager:executeimmediate(
								"LOC RESET DECK=" .. possible_stuck_discs[#possible_stuck_discs].deck .. " SLOT=" .. possible_stuck_discs[#possible_stuck_discs].slot,
								false,
								function()
									hardware_manager:soft_sync(
										function()
											current_possible_stuck_disc = false
											table.remove( possible_stuck_discs, #possible_stuck_discs )
											update_possible_stuck_discs_count()
											
											reload_inventory()
											unknowns = get_unknowns()
											update_unknowns_remaining_count()
											
											overlay_progress_manager:stop()
										end
									)
								end
							)
						else
							reload_inventory()
							unknowns = get_unknowns()
							update_unknowns_remaining_count()
						end
					end
				end
				
				if job_result.status == "Errored" then
					completion_summary.errors = completion_summary.errors .. UnknownsAlertManager.format_errors( job_result.job_id )
				end
			end,
			nil
		)
	end
)
kernel.registeractorhitnotify(
	"no_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'No'" )
		idle_timer:reset()

		kernel.healthsvcs_sendauditevent(
			StoreID,
			"FMA_UnknownNotReinserted",
			username,
			"FMA Bundle",
			"UNKNOWN"
		)
		table.insert( completion_summary.notreinserted, { status="Not reinserted." } )

		add_to_output( "ITEM RETURN: The item was not reinserted." )

		kernel.enableactor( "close_area" )
		kernel.enableactor( "pull_button" )
		kernel.setactorforegroundcolor( "pull_button", "white" )
		kernel.disableactor( "yes_button" )
		kernel.setactorforegroundcolor( "yes_button", "gray" )
		kernel.disableactor( "no_button" )
		kernel.setactorforegroundcolor( "no_button", "gray" )
		kernel.disableactor( "hardware_issue_button" )
		kernel.setactorforegroundcolor( "hardware_issue_button", "gray" )

		if current_possible_stuck_disc == true then
			overlay_progress_manager:start( false )
			
			hardware_manager:executeimmediate(
				"GRIPPER CLEAR",
				false,
				function()
					hardware_manager:executeimmediate(
						"LOC RESET DECK=" .. possible_stuck_discs[#possible_stuck_discs].deck .. " SLOT=" .. possible_stuck_discs[#possible_stuck_discs].slot,
						false,
						function()
							hardware_manager:soft_sync(
								function()
									current_possible_stuck_disc = false
									table.remove( possible_stuck_discs, #possible_stuck_discs )
									update_possible_stuck_discs_count()
									
									reload_inventory()
									unknowns = get_unknowns()
									update_unknowns_remaining_count()
									
									overlay_progress_manager:stop()
								end
							)
						end
					)
				end
			)
		else
			reload_inventory()
			unknowns = get_unknowns()
			update_unknowns_remaining_count()
		end
	end
)
kernel.registeractorhitnotify(
	"hardware_issue_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'Hardware Issue'" )
		idle_timer:reset()

		local popup_check_box_value = nil
		local hardware_issue_scene_manager = newscenemanager()
		
		hardware_issue_scene_manager:add_actor(
			"popup_opacity_background",
			0,
			0,
			1024,
			768,
			{ backgroundColor = "Black", opacity = 0.5, hitOptions = "Enabled", renderOptions = "Background" }
		)
		
		hardware_issue_scene_manager:add_actor(
			"popup_background",
			337,
			60,
			350,
			500,
			{ backgroundColor = { r = "165", g = "165", b = "165" }, cornerSize = { width = 10, height = 10 }, borderColor = { r = "192", g = "192", b = "192" }, strokeWeight = 3.0, renderOptions = "Background, DrawBorder" }
		)
		
		hardware_issue_scene_manager:add_actor(
			"popup_title_area",
			339,
			62,
			346,
			50,
			{ backgroundColor = "DarkRed", foregroundColor = "White", text = "  Issues", font = "medium_larger_font", verticalAlignment = "center", cornerSize = { width = 10, height = 10 }, renderOptions = "Background, Text, TextShadow" }
		)
		
		hardware_issue_scene_manager:add_actor(
			"popup_close_area_button",
			611,
			60,
			77,
			52,
			{ backgroundColor = { r = "192", g = "192", b = "192" }, foregroundColor = "White", text = "X", font = "medium_larger_font", verticalAlignment = "center", horizontalAlignment = "center", cornerSize = { width = 10, height = 10 }, hitOptions = "Enabled", renderOptions = "Background, Text" },
			function()
				hardware_issue_scene_manager:clear()
			end
		)
		
		local check_boxes = { "Slot Data", "Warped Deck", "No Kiosk Access", "Other" }
		for k, v in ipairs( check_boxes ) do
			hardware_issue_scene_manager:add_actor(
				"popup_check_box_" .. tostring( k ),
				353,
				100 + ( k * 65 ),
				51,
				47,
				{ image = "check_box", hitOptions = "Enabled", renderOptions = "Image" },
				function()
					idle_timer:reset()
					
					for key, _ in ipairs( check_boxes ) do
						kernel.setactorimage( "popup_check_box_" .. tostring( key ), "check_box" )
					end
					kernel.setactorimage( "popup_check_box_" .. tostring( k ), "check_box_checked" )
					popup_check_box_value = v
					
					kernel.enableactor( "popup_ok_button" )
					kernel.setactorforegroundcolor( "popup_ok_button", "White" )
				end
			)
			hardware_issue_scene_manager:add_actor(
				"popup_check_box_label_" .. tostring( k ),
				423,
				100 + ( k * 65 ),
				255,
				47,
				{ foregroundColor = "Black", text = v, font = "medium_font", verticalAlignment = "center", renderOptions = "Text" }
			)
		end
		
		hardware_issue_scene_manager:add_actor(
			"popup_ok_button",
			510,
			485,
			164,
			62,
			{ backgroundColor = "DarkRed", foregroundColor = "White", text = "OK", font = "medium_font", verticalAlignment = "center", horizontalAlignment = "center", cornerSize = { width = 10, height = 10 }, borderColor = "White", strokeWeight = 2.0, hitOptions = "Enabled", renderOptions = "Background, Text, TextShadow, DrawBorder" },
			function()
				local deck_slot_key = tostring( possible_stuck_discs[#possible_stuck_discs].deck ) .. "@" .. tostring( possible_stuck_discs[#possible_stuck_discs].slot )
				
				local output_string = "ITEM RETURN: Slot " .. deck_slot_key .. " marked as HARDWARE-ISSUE: " .. popup_check_box_value .. "."
				kernel.logto( "FieldMaintenanceActionLog", output_string )
				add_to_output( output_string )
				
				table.insert( completion_summary.hardware_issues, { key=deck_slot_key, status="Hardware Issue: " .. popup_check_box_value } )
				
				kernel.enableactor( "close_area" )
				kernel.enableactor( "pull_button" )
				kernel.setactorforegroundcolor( "pull_button", "white" )
				kernel.disableactor( "yes_button" )
				kernel.setactorforegroundcolor( "yes_button", "gray" )
				kernel.disableactor( "no_button" )
				kernel.setactorforegroundcolor( "no_button", "gray" )
				kernel.disableactor( "hardware_issue_button" )
				kernel.setactorforegroundcolor( "hardware_issue_button", "gray" )
				
				hardware_manager:executeimmediate(
					"GRIPPER CLEAR",
					false,
					function()
						current_possible_stuck_disc = false
						
						reload_inventory()
						unknowns = get_unknowns()
						update_unknowns_remaining_count()
						
						hardware_issue_scene_manager:clear()
					end
				)
			end
		)
		kernel.disableactor( "popup_ok_button" )
		kernel.setactorforegroundcolor( "popup_ok_button", "Gray" )
	end
)

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

kernel.registeractorhitnotify(
	"confirm_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'Confirm Unknown Task Completed'" )
		idle_timer:reset()
		
		if #unknowns == 0 then
			completion_summary.exit_method = "User pressed the Confirm Unknown Task Completed button."
			send_summary_alert()
			soft_sync_and_resume_services()
		end
	end
)

kernel.registeractorhitnotify(
	"close_area",
	function()
		kernel.logto( "FieldMaintenanceActionLog", "Remove Unknowns: User hits 'X'" )
		idle_timer:reset()
		completion_summary.exit_method = "User pressed close[X] button."

		if #unknowns ~= 0 then
			local messagebox = newmessageboxmanager()
			callbacks = {}
			callbacks[messagebox.ButtonYES] = function()
				send_summary_alert()
				soft_sync_and_resume_services()
			end

			local message_text = "There are still Unknowns remaining in the kiosk. Do you want to exit?"
			messagebox:show(
				message_text,
				messagebox.ButtonTypeYESNO,
				messagebox.IconTypeQuestion,
				callbacks )
		else
			send_summary_alert()
			soft_sync_and_resume_services()
		end
	end
)