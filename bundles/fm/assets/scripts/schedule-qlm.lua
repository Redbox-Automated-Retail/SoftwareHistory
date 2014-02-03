--
-- schedule-qlm.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local progress_manager = newprogressmanager()

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

if not check_box_state["unload_checkbox"] and not check_box_state["thin_checkbox"] then
	local message = "Please select either Empty, Fill, or both"
	if VMZEnabledMachine == true then
		message = "Please select either Unload VMZ or Thin Kiosk"
	end
	newmessageboxmanager():show( message )
	return
end

progress_manager:start( false )
local hardware = newhardwaremanager()

kernel.callbackenqueue(
	function(params)
		local job_type = iif( VMZEnabledMachine, "VMZ", "QLM" )
		
		-- VMZ Unload handling
		if VMZEnabledMachine then
			if check_box_state["unload_checkbox"] then
				-- Set the number of decks to check
				if num_newrelease_decks > 0 then
					kernel.log("Marking Last Deck for Unload")
					result = hardware:executeimmediate("VMZ MARK-UNLOAD LAST-DECK="..num_newrelease_decks, false, nil)
					num_newrelease_decks = 0
					kernel.setmacro("keyboard:newrelease_decks", "" )
				else
					--failed... we should never reach this -- consider bailing out with popup
					kernel.log("VMZ disks not marked as unknown. Number of new release decks was not set.")
				end
			
				-- VMZ unloads can only be scheduled up to an hour
				if kernel.stringdatecompare(tostring((newdatetime(os.date("*t")) + "01:00:00")) , tostring(schedule_time) ) < 0 then
					schedule_time = newdatetime(os.date("*t")) + "01:00:00"
				end
				
				kernel.setactortext(
					"schedule_value",
					tostring( schedule_time ) )
				local label = job_type .. "-unload scheduled by " .. username
				hardware:qlm_unload(label, schedule_time.backing)
			end
		end
		
		if (not VMZEnabledMachine)
		and check_box_state["unload_checkbox"]
		and check_box_state["thin_checkbox"] then
			-- QLM Unload And Thin if both boxes are checked
			local label = job_type .. "-unload-and-thin scheduled by " .. username
			local qlm_unload_thin_schedule = schedule_time
			if schedule_time:is_within_minutes( newdatetime(), 10 ) then
				-- Don't use kiosk engine scheduler, just submit to HAL now
				if unload_checked then
					qlm_unload_thin_schedule = schedule_time + "00:01:00"
				end
				local barcodes = get_thin_barcodes()
				if #barcodes == 0 then
					kernel.log(">  There are no Thinned discs at this time, super job converted to unload ONLY.")
					hardware:qlm_unload( label, qlm_unload_thin_schedule.backing)
				else
					kernel.log(">  Found " ..tostring(#barcodes) .. " thinned discs")
					hardware:qlm_unload_and_thin( label, qlm_unload_thin_schedule.backing, barcodes)
				end
			else
				local jobname = "unload-thin-" .. tostring( qlm_unload_thin_schedule)
				local parameters = {}
				-- the p1_ and p2_ are to force an order on the parameters
				-- because the dictionary will sort them which will break
				-- the job entry
				parameters["p1_label"] = label
				parameters["p2_jobname"] = jobname
				scheduler_services.add_entry(
					jobname,
					label,
					nil,
					nil,
					qlm_unload_thin_schedule:to_cron_expression(),
					"run_qlm_unload_thin_job",
					"unload-thin",
					"FireOnceNow",
					parameters
				)
				kernel.log( "Job " .. jobname .. " added to Kiosk Engine scheduler." )
			end
		else
			-- QLM Unload if box is checked
			if not VMZEnabledMachine then
				if check_box_state["unload_checkbox"] then
					local label = job_type .. "-unload scheduled by " .. username
					hardware:qlm_unload(label, schedule_time.backing)
				end
			end
			
			-- Execute HAL Thin job if box is checked
			if check_box_state["thin_checkbox"] then
				local label = job_type .. "-thin scheduled by " .. username
				local thin_schedule = schedule_time
				if schedule_time:is_within_minutes( newdatetime(), 10 ) then

					-- Don't use kiosk engine scheduler, just submit to HAL now
					if unload_checked then
						thin_schedule = schedule_time + "00:01:00"
					end
					if job_type == "VMZ" then
						local barcodesToBin, barcodesToVmz = get_vmz_thin_barcodes()
						if #barcodesToBin == 0
						and #barcodesToVmz == 0 then
							kernel.log(">  There are no Thinned discs at this time, thin request cancelled.")
						else
							kernel.log(">  Found " ..tostring(#barcodesToBin) .. " thinned discs to be sent to Bin.")
							kernel.log(">  Found " ..tostring(#barcodesToVmz) .. " thinned discs to be sent to VMZ.")
							hardware:vmz_thin( label, thin_schedule.backing, barcodesToBin, barcodesToVmz )
						end
					else
						local barcodes = get_thin_barcodes()
						if #barcodes == 0 then
							kernel.log(">  There are no Thinned discs at this time, thin request cancelled.")
						else
							kernel.log(">  Found " ..tostring(#barcodes) .. " thinned discs")
							hardware:qlm_thin( label, thin_schedule.backing, barcodes )
						end
					end
				else
					local jobname = "thin-" .. tostring( thin_schedule)
					local run_job = iif( VMZEnabledMachine, "run_thin_vmz_job", "run_thin_job" )
					local parameters = {}
					-- the p1_ and p2_ are to force an order on the parameters
					-- because the dictionary will sort them which will break
					-- the job entry
					parameters["p1_label"] = label
					parameters["p2_jobname"] = jobname
					scheduler_services.add_entry(
						jobname,
						label,
						nil,
						nil,
						thin_schedule:to_cron_expression(),
						run_job,
						"thin",
						"FireOnceNow",
						parameters
					)
					kernel.log( "Job " .. jobname .. " added to Kiosk Engine scheduler." )
				end
			end
		end

		-- Display running / pending / errored jobs
		job_grid:display( true )

		-- Clear check marks
		check_box_state["thin_checkbox"] = nil
		kernel.setactorimage("thin_checkbox", "check_box")
		check_box_state["unload_checkbox"] = nil
		kernel.setactorimage("unload_checkbox", "check_box")

		schedule_time = newdatetime()
		kernel.setactortext(
			"schedule_value",
			tostring( schedule_time ) )
		progress_manager:stop()
	end,
	{})

