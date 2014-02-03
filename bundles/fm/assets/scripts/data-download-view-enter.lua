--
-- data-download-view-enter.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newdownloadmonitor()
	local t = {}
	
	--start timer
	t.start = function(
		self,
		start,
		frequency )
		kernel.timercreate(
			"download-monitor",
			start or kernel.random(60000,300000),
			frequency or 300000,
			self.on_timer )
		kernel.timerstart( "download-monitor" )
	end

	-- timer tick
	t.on_timer = function()
		kernel.timerstop( "download-monitor" )
		get_downloads()
		if selected_row_id ~= nil then
			highlight_row(selected_row_id, total_row_count)
		end
		kernel.timerstart("download-monitor")
	end
	
	-- stop timer
	t.stop = function()
		kernel.timerstop( "download-monitor" )
	end
	
	return t
end

function newcheckupdatestatetimer()
	local t= {}
	t.start = function( self )
		kernel.timercreate( "check_update_state_timer", 3000, 5000, self.on_timer )
		kernel.timerstart("check_update_state_timer")
	end
	t.on_timer = function()
		kernel.timerstop("check_update_state_timer")
		check_data_state( 
			function( result )
				if result.state == "Idle" then
					-- Idle means the job has finished and the data file is ready for download
					force_schedule_event()
					kernel.timerremove("check_update_state_timer")
					return
				end
			end
		)
		kernel.timerstart("check_update_state_timer")
	end
	t.stop = function()
		kernel.timerstop("check_update_state_timer")
	end
	return t
end

force_subscription = function()
	-- call force subscription publish method of update manager service
	kernel.log( "updmgrsvc_forcesubscriptionpublish() called" )
	local result = kernel.updmgrsvc_forcesubscriptionpublish()
	if result ~= nil then
		kernel.log( ">> result.success = " .. tostring( result.success ) )
		if result.success then
			kernel.disableactor( "force_data_updates_button" )
			check_state_timer = newcheckupdatestatetimer()
			check_state_timer:start()
		else
			MessageBoxManager:show( 
				"The force subscription could not be started.", 
				MessageBoxManager.ButtonTypeOK, 
				MessageBoxManager._icon_type_error
			)
		end
	end
end

-- These are the hard-coded user options in the drop-down of the store services tab of Kiosk Engine
-- Values found in ManageBundleFiltersform.Designer.cs of Redbox.KioskEngine.Bootstrap namespace
local update_commands = {}
update_commands["Development"] = "tree-update"
update_commands["QA"] = "tree-update"
update_commands["UAT"] = "tree-update"
update_commands["Production"] = "tree-update-1"

force_schedule_event = function()
	-- call force scheduled event with correct parameter for Production vs. QA
	local environment = kernel.getmachinevalueordefault(
			"Store",
			"Environment",
			"Production" )
	if update_commands[environment] == nil then
		MessageBoxManager:show( 
			"The schedule event for " .. environment .. " is not configured.", 
			MessageBoxManager.ButtonTypeOK, 
			MessageBoxManager._icon_type_error
		)
	else
		kernel.log( "updmgrsvc_forcescheduleevent( " .. update_commands[environment] .. " ) called for " .. environment )
		local result = kernel.updmgrsvc_forcescheduleevent( update_commands[environment] )
		if result ~= nil then
			kernel.log( ">> result.success = " .. tostring( result.success ) )
			if result.success then
				kernel.disableactor("check_for_updates_button")
			else
				MessageBoxManager:show( 
					"The check for updates failed.", 
					MessageBoxManager.ButtonTypeOK, 
					MessageBoxManager._icon_type_error
				)
			end
		end
	end
end


check_data_state = function( on_success )
	-- call get subscription state method of update manager service
	kernel.log( "updmgrsvc_getsubscriptionstate() called" )
	local result = kernel.updmgrsvc_getsubscriptionstate()
	--result = { ["success"] = true, ["last_run"] = "2/12/2010 13:45PM", ["state"] = "Idle"}
	if result ~= nil then
		if result.success then
			kernel.setactortext("last_server_data_update_value", result.last_run .. " " .. result.state)
			if on_success ~= nil then
				on_success( result )
			end
		else
			kernel.setactortext("last_server_data_update_value", "Error getting subscription state")
			kernel.log( ">> errors: " .. DataDumper( result.errors ) )
		end
	end
end

function highlight_row( row_id, total_count )
	if total_count ~= nil then
		if last_set_number == set_number then
			kernel.setbordercolor( "row_" .. row_id, "black")
			kernel.markactordirty( "row_" .. row_id)
			kernel.enableactor( "show_errors_button" )
			kernel.setactorforegroundcolor( "show_errors_button", "white" )
		else
			kernel.disableactor( "show_errors_button" )
			kernel.setactorforegroundcolor( "show_errors_button", "gray" )
		end
		for i = 1, total_count do
			if i ~= row_id then
				kernel.setbordercolor( "row_" .. tostring(i), "white" )
			end
		end
	end
end

-- gets the list of items being downloaded from the update manager service
get_downloads = function()
	-- call get downloads method of update manager service
	kernel.log( "updmgrsvc_getdownloads() called" )
	local result = kernel.updmgrsvc_getdownloads()
	if result ~= nil then
		if result.success then
			show_download_data( result )
		else
			kernel.log( ">> errors: " .. DataDumper( result.errors ) )

			-- TODO: Create a new message which displays in the center of the download area

		end
	end
	
--[[	-- This part is to be removed 
	result = { ["success"] = true, ["downloads"] = {}, ["errors"] = { ["error_number"] = "1234",  
											  ["error_description"] = "Error Description", 
											  ["error_detail"] = "Error Details" 
											} }
	result.downloads = {	[1] = {	["name"] = "abcd",
								["status"] = "In Progress",
								["job_type"] = "Download",
								["start_time"] = "2/18/2010 6:23:41",
								["finish_time"] = "",
								["total_bytes_transferred"] = "780",
								["total_bytes"] = "1217"
								},
							[2] = {	["name"] = "efgh",
									["status"] = "Transferred",
									["job_type"] = "Download",
									["start_time"] = "2/10/2010 7:18:09",
									["finish_time"] = "2/10/2010 7:24:13",
									["total_bytes_transferred"] = "8545823",
									["total_bytes"] = "8545823"
								},
							[3] = {	["name"] = "ijkl",
									["status"] = "Not Started",
									["job_type"] = "Download",
									["start_time"] = "",
									["finish_time"] = "",
									["total_bytes_transferred"] = "0",
									["total_bytes"] = "2571831",
								},
							[4] = {	["name"] = "mnop",
									["status"] = "In Progress",
									["job_type"] = "Download",
									["start_time"] = "2/19/2010 4:15:03",
									["finish_time"] = "",
									["total_bytes_transferred"] = "22780",
									["total_bytes"] = "32807"
								},
							[5] = {	["name"] = "qrst",
								["status"] = "In Progress",
								["job_type"] = "Download",
								["start_time"] = "2/18/2010 6:23:41",
								["finish_time"] = "",
								["total_bytes_transferred"] = "780",
								["total_bytes"] = "1217"
								},
							[6] = {	["name"] = "uvwx",
									["status"] = "Transferred",
									["job_type"] = "Download",
									["start_time"] = "2/10/2010 7:18:09",
									["finish_time"] = "2/10/2010 7:24:13",
									["total_bytes_transferred"] = "8545823",
									["total_bytes"] = "8545823"
								},
							[7] = {	["name"] = "yz01",
									["status"] = "Not Started",
									["job_type"] = "Download",
									["start_time"] = "",
									["finish_time"] = "",
									["total_bytes_transferred"] = "0",
									["total_bytes"] = "2571831",
								},
							[8] = {	["name"] = "2345",
									["status"] = "In Progress",
									["job_type"] = "Download",
									["start_time"] = "2/19/2010 4:15:03",
									["finish_time"] = "",
									["total_bytes_transferred"] = "22780",
									["total_bytes"] = "32807"
								},
							[9] = {	["name"] = "6789",
								["status"] = "In Progress",
								["job_type"] = "Download",
								["start_time"] = "2/18/2010 6:23:41",
								["finish_time"] = "",
								["total_bytes_transferred"] = "780",
								["total_bytes"] = "1217"
								}
				}
	-- Till here
]]	

	--show_download_data(result)
	--highlight_row(selected_row_id, total_row_count)
end

-- display data in the grid
function show_download_data( result )
	if result == nil or result.success ~= true then
		selected_row_id = nil
		return
	end
	
	download_scene_manager:clear()
	local index = 1
	local y = 120
	local traverseCount = 2
	local _data_download_pager = data_download_pager

	local textHeight = 0			
	_data_download_pager:mark_current_page()

	for k,v in ipairs(result.downloads) do
		local current_row = result.downloads[_data_download_pager:get_index()]
		local current_row_name = current_row["name"]
		local current_row_status = current_row["status"]
		local current_row_job_type = current_row["job_type"]
		local current_row_start_time = current_row["start_time"]
		local current_row_finish_time = current_row["finish_time"]
		local current_row_total_bytes = current_row["total_bytes"]
		local current_row_progress = (current_row["total_bytes_transferred"]/current_row["total_bytes"]) * 135
		
		download_scene_manager:add_actor("row_bg" .. tostring(index), 
		22, 
		y, 
		935, 
		50, 
		{ font = "medium_font", 
		  backgroundColor = "white",
		  renderOptions = "Text, Image, Background", 
		  hitOptions = "Enabled"})
		
		if k == traverseCount then
			kernel.setactorbackgroundcolor("row_bg" .. tostring(index), "lightgray")
			traverseCount = traverseCount + 2
		end
		
		download_scene_manager:add_actor("job_name_" .. tostring(index), 
		30, 
		y, 
		165, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = current_row_name })				  
		
		download_scene_manager:add_actor("status_" .. tostring(index), 
		225, 
		y, 
		80, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = current_row_status })
		
		download_scene_manager:add_actor("job_type_" .. tostring(index), 
		330, 
		y, 
		80, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = current_row_job_type })
		
		download_scene_manager:add_actor("started_" .. tostring(index), 
		415, 
		y, 
		130, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = current_row_start_time })  
		
		download_scene_manager:add_actor("finished_" .. tostring(index), 
		565, 
		y, 
		130, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = current_row_finish_time })
		
		download_scene_manager:add_actor("total_bytes_" .. tostring(index), 
		710, 
		y, 
		80, 
		50, 
		{ font = "small_font", 
		  renderOptions = "Text", 
		  hitOptions = "Enabled", 
		  foregroundColor = "black",
		  horizontalAlignment = "near",
		  verticalAlignment = "center",
		  text = kernel.formatasbytesize( tonumber(current_row_total_bytes) ) })
		  
		download_scene_manager:add_actor("progress_background_" .. tostring(index), 
		805, 
		y + 20, 
		135, 
		15, 
		{ font = "small_font", 
		  renderOptions = "Image", 
		  hitOptions = "Enabled", 
		  image = "progress_background" })
		
		download_scene_manager:add_actor("progress_value_" .. tostring(index), 
		805, 
		y + 20, 
		current_row_progress, 
		15, 
		{ font = "medium_font", 
		  renderOptions = "Image", 
		  hitOptions = "Enabled", 
		  image = "image_green_progress_bar" })
		
		download_scene_manager:add_actor("row_" .. tostring(index), 
		22, 
		y, 
		935, 
		50, 
		{ font = "medium_font", 
		  renderOptions = "Text, Image, DrawBorder", 
		  hitOptions = "Enabled"},
		  function()
			last_set_number = set_number
			selected_download_item = result
			selected_row_id = k
			total_row_count = index
			kernel.enableactor("show_errors_button")
			kernel.setactorforegroundcolor("show_errors_button", "white")
			highlight_row(k, index)
		  end)
		  kernel.setbordercolor("row_" .. tostring(index),"white")
		
		index = index + 1
		y = y + 51
		
		if y >= 420 then	
			break
		end			
		_data_download_pager:increment_index()
		
		if _data_download_pager:get_index() > #result.downloads then
			break
		end
					
		_data_download_pager:update_paging( #result.downloads )
	end
end

-- show errors related to particular item
show_errors = function()
	kernel.executescript("show_error_view")
end



if set_number == nil then
	set_number = 1
end
if last_set_number == nil then
	last_set_number = 1
end

kernel.disableactor("show_errors_button")
kernel.disableactor("image_up_arrow")
kernel.disableactor("image_down_arrow")

MessageBoxManager = newmessageboxmanager()
if download_scene_manager ==null then
	download_scene_manager = newscenemanager()
end

if data_download_pager==null then
	data_download_pager = newpager()
end

data_download_pager.next_changed = function( enabled )
	if enabled == true then				
		kernel.enableactor( "image_down_arrow" )		

	else				
		kernel.disableactor( "image_down_arrow" )
	end
	--set_number = set_number + 1
	--highlight_row(0,total_row_count)
end

data_download_pager.prev_changed = function( enabled )
	if enabled == true then			
		kernel.enableactor( "image_up_arrow" )		
	else				
		kernel.disableactor( "image_up_arrow" )
	end
	--set_number = set_number - 1
	--highlight_row(0,total_row_count)
end

check_data_state()
download_monitor = newdownloadmonitor()
download_monitor:start(100, 5000)

kernel.registeractorhitnotify( "force_data_updates_button", 
							function()
								local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
								kernel.logto( "FieldMaintenanceActionLog", log_value )
								force_subscription()
							end)
kernel.registeractorhitnotify( "check_for_updates_button",
							function()
								local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
								kernel.logto( "FieldMaintenanceActionLog", log_value )
								force_schedule_event()
							end)
kernel.registeractorhitnotify( "show_errors_button", 
							function()
								local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
								kernel.logto( "FieldMaintenanceActionLog", log_value )
								show_errors()
							end)

