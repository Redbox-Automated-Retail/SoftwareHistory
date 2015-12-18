--
-- qlm-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if schedule_time == nil then
	schedule_time = newdatetime()
end

kernel.setactortext("schedule_value", tostring(schedule_time) )
kernel.setmacro("MerchVmzFirstEnter", nil )

if VMZMerchFlowEnabled == false then
	kernel.disableactor( "launch_button" )
	kernel.setactorforegroundcolor( "launch_button", "gray" )
end

-- unload-vmz # release decks, update text field appropriately
if num_newrelease_decks < 1 then
	kernel.setmacro("keyboard:newrelease_decks", "" )
elseif num_newrelease_decks < 2 then
	kernel.setmacro("keyboard:newrelease_decks", tostring( num_newrelease_decks ) )
else
	kernel.setmacro("keyboard:newrelease_decks", "1-"..tostring( num_newrelease_decks ) )
end

if job_grid == nil then
	job_grid = newjobgrid({ "qlm-unload", "thin", "thin-vmz", "unload-thin", "clean-vmz" }, { "Garbage" })

	job_grid.load =
		function ( self )
			job_grid.hardware_jobs = job_grid.job_hardware_manager:get_hardware_jobs_by_program2( job_grid.job_programs, job_grid.excluded_status )
			
			-- if THIN or CLEAN-VMZ is running, ensure the "Merchandise VMZ" button is disabled
			
			-- thin-vmz jobs from scheduled jobs visible in this list
			local is_thin_vmz_running = #HardwareManager:get_hardware_jobs_by_program( { "thin-vmz" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
			local is_thin_running = #HardwareManager:get_hardware_jobs_by_program( { "thin" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
			
			-- thin-vmz jobs from immediate jobs visible in this list
			local is_thin_vmz_running2 = #HardwareManager:get_hardware_jobs_by_program2( { "thin-vmz" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
			local is_thin_running2 = #HardwareManager:get_hardware_jobs_by_program2( { "thin" }, { "Errored", "Stopped", "Garbage", "Completed" } ) > 0
			
			local is_clean_vmz_running = #HardwareManager:get_hardware_jobs_by_program( { "clean-vmz" }, { "Suspended", "Pending", "Errored", "Stopped", "Garbage", "Completed" } ) > 0

			if (is_thin_running == true)
			   or (is_thin_running2 == true)
			   or (is_thin_vmz_running == true)
			   or (is_thin_vmz_running2 == true)
			   or (is_clean_vmz_running == true)
			   or (VMZMerchFlowEnabled == false) then
				kernel.disableactor( "launch_button" )
				kernel.setactorforegroundcolor( "launch_button", "gray" )
			else
				kernel.enableactor( "launch_button" )
				kernel.setactorforegroundcolor( "launch_button", "white" )
			end
		end
	
	job_grid.job_pager.next_changed =
		function( enabled )
			if enabled == true then
				kernel.setactorimage( "right_arrow_button", "right_arrow" )
				kernel.setactorforegroundcolor( "right_arrow_button", "White" )
				kernel.enableactor( "right_arrow_button" )
			else
				kernel.setactorimage( "right_arrow_button", "right_arrow_disabled" )
				kernel.setactorforegroundcolor( "right_arrow_button", "Gray" )
				kernel.disableactor( "right_arrow_button" )
			end
		end

	job_grid.job_pager.prev_changed =
		function( enabled )
			if enabled == true then
				kernel.setactorimage( "left_arrow_button", "left_arrow" )
				kernel.setactorforegroundcolor( "left_arrow_button", "White" )
				kernel.enableactor( "left_arrow_button" )
			else
				kernel.setactorimage( "left_arrow_button", "left_arrow_disabled" )
				kernel.setactorforegroundcolor( "left_arrow_button", "Gray" )
				kernel.disableactor( "left_arrow_button" )
			end
		end
	job_grid:load()
end

if check_box_state["unload_checkbox"] then
	kernel.setactorimage("unload_checkbox", "check_box_checked")
end

if check_box_state["thin_checkbox"] then
	kernel.setactorimage("thin_checkbox", "check_box_checked")
end

job_grid:display( true )

if qlm_info.loaded == false then
	local progress_mgr = newprogressmanager()
	progress_mgr:start( false )
	qlm_info:load(progress_mgr)
else
	if VMZEnabledMachine == true then
		qlm_info:refresh()
	end
	kernel.setactortext("info_label", tostring(qlm_info))
end
