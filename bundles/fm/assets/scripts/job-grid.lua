--
-- job-grid.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newjobgrid( programs, status_to_exclude )
	local t = {}
	t.job_pager = newpager()
	t.job_scene_mgr = newscenemanager()
	t.job_hardware_manager = newhardwaremanager()
	t.hardware_jobs = {}
	t.job_programs = programs
	t.excluded_status = status_to_exclude

	t.load =  function( self )
		self.hardware_jobs = self.job_hardware_manager:get_hardware_jobs_by_program2( self.job_programs, self.excluded_status )
	end

	t.display = function( self, refresh_job_list )
		local index = 1
		local y = 115
		self.job_scene_mgr:clear()
		self.job_pager:mark_current_page()

		if refresh_job_list then
			self:load()
		end

		if self.hardware_jobs ~= nil and #self.hardware_jobs > 0 then
			shellsort(
				self.hardware_jobs,
				function ( v1, v2 )
					return kernel.stringdatecompare( v1.starttime, v2.starttime ) < 0
				end)
			while true do
				local v = self.hardware_jobs[ self.job_pager:get_index() ]

				if v == nil and self.job_pager:has_previous_pages() then
					self.job_pager:page_back()
					v = self.hardware_jobs[ self.job_pager:get_index() ]
				end
				local job_id = v.id
				if kernel.getlasthitactorname() == "schedule_button"
				or kernel.getlasthitactorname() == "schedule_unknowns_button" then
					local job_exist = false
					for i,k in ipairs(sync_job_ids) do
						if k == v.id then
						   job_exist = true
						end
					end
					if job_exist == false then
					    table.insert(sync_job_ids,v.id)
						kernel.logto( "FieldMaintenanceActionLog",v.id)
					end
				else
				    table.insert(sync_job_ids,v.id)
				end
				local back_color = { r="239", g="239", b="239" }
				if index % 2 == 0 then
					back_color = "lightgray"
				end

				local actor_name = "background_" .. tostring( index )
				local params = { backgroundColor = back_color, renderOptions = "Background", hitOptions = "Enabled" }
				self.job_scene_mgr:add_actor( actor_name, 17, y, 990, 51, params )
				local check_box_name = "checkbox_" .. job_id
				params = { renderOptions = "Image", hitOptions = "Enabled", tag = v.id }

				if check_box_state[ check_box_name ] ~= nil then
					params.image = "check_box_checked"
				else
					params.image = "check_box"
				end

				self.job_scene_mgr:add_actor(
					check_box_name,
					25,
					y+2,
					51,
					47,
					params,
					function()
						kernel.setlasthitactorname( check_box_name )
						kernel.executescript( "check_box_hit" )
						kernel.setactorforegroundcolor( "delete_button", "white" )
						kernel.enableactor( "delete_button" )
						for i,v in pairs(self.hardware_jobs) do
							local jobid="checkbox_"..tostring(v.id)
							if (v.program == "thin" or v.program == "thin-vmz" or v.program == "unload-thin")
							and v.status ~= "Pending" then
								if check_box_state[jobid]== v.id then
									kernel.setactorforegroundcolor( "delete_button", "gray" )
									kernel.disableactor( "delete_button" )
								end
							end
						end
					end
				)

				actor_name = "job_" .. tostring(index)
				params = { font = "medium_font",
						renderOptions = "Text",
						hitOptions = "Enabled",
						foregroundColor = "black",
						horizontalAlignment = "near",
						verticalAlignment = "center",
						text = v.id }
				self.job_scene_mgr:add_actor( actor_name, 90, y, 120, 51, params )
				actor_name = "label_" .. index
				local label_text = v.label
				if v.label == "(not labeled)" then
					label_text = v.program
					if VMZEnabledMachine == true and v.program == "qlm-unload" then
						label_text = "VMZ-unload"
					end
				else
					label_text = string.gsub( label_text, "range", "\r\nrange" )
				end
				params = { font = "medium_font",
						renderOptions = "Text",
						hitOptions = "Enabled",
						foregroundColor = "black",
						horizontalAlignment = "near",
						verticalAlignment = "center",
						text = label_text }
				self.job_scene_mgr:add_actor( actor_name, 180, y, 310, 51, params )

				actor_name = "status_" .. index
				params = { font = "medium_font",
						renderOptions = "Text",
						hitOptions = "Enabled",
						foregroundColor = "black",
						horizontalAlignment = "near",
						verticalAlignment = "center",
						text = v.status }
				self.job_scene_mgr:add_actor( actor_name, 500, y, 100, 51, params )

				actor_name = "start_time_" .. index
				params = { font = "medium_font",
						renderOptions = "Text",
						hitOptions = "Enabled",
						foregroundColor = "black",
						horizontalAlignment = "near",
						verticalAlignment = "center",
						text = v.starttime or "N/A" }
				self.job_scene_mgr:add_actor( actor_name, 615, y, 350, 51, params )

				actor_name = "execution_time_" .. index
				params = { font = "medium_font",
						renderOptions = "Text",
						hitOptions = "Enabled",
						foregroundColor = "black",
						horizontalAlignment = "near",
						verticalAlignment = "center",
						text = v.executiontime or "00:00:00.000000" }
				self.job_scene_mgr:add_actor( actor_name, 836, y, 150, 51, params )
				y = y + 51
				if index == 5 then
					break
				end

				index = index + 1
				self.job_pager:increment_index()

				if self.job_pager:get_index() > #self.hardware_jobs then
					break
				end
			end
			self.job_pager:update_paging( #self.hardware_jobs )
		end
	end

	return t
end
