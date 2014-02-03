--
-- quickreturn-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local TIMER_NAME = "quickreturn-timer"

function newquickreturntimer()
	kernel.timercreate(
		TIMER_NAME,
		30000,
		30000,
		function()
			kernel.log( "Quick Return Timer executing" )
			kernel.log( ">  stop timer" )
			kernel.timerstop( TIMER_NAME )
			local hardwarejobs =
				HardwareManager:get_hardware_jobs_by_program(
					{"quick-return" },
					{"Pending", "Running", "Suspended", "Garbage" } )
			if #hardwarejobs > 0 then
				for _, hardware_job in ipairs( hardwarejobs ) do
					kernel.log( "quickreturn_timer_callback: processing return with messaging job: " .. tostring(hardware_job.id) )
					local job_results = kernel.hardwaregetprogramresults( hardware_job.id )
					for _, v in ipairs( job_results.results ) do
						if  v.code == "ItemReturned"
						and v.item_id ~= nil then
							kernel.log( ">  Enqueue invsvcs_return message for QuickReturn" )
							kernel.invsvcs_return(
								StoreManager:get_number(),
								"QuickReturn",
								v.item_id,
								v.deck,
								v.slot,
								v.return_time )
							kernel.log( ">  Increment Quick Return counter" )
							QuickReturnCounter:increment()
						end
					end
					kernel.log( ">  Trash QuickReturn job " .. tostring(hardware_job.id) )
					kernel.hardwaretrashjob( hardware_job.id )
				end
			else
				kernel.log( ">  no completed quick return jobs found" )
			end
			if hal_comm_failure == false then
				kernel.log( ">  re-start QuickReturn timer" )
				kernel.timerstart( TIMER_NAME )
			else
				kernel.log( "Quick Return Timer is unable to communicate with HAL, quickreturn-timer timer will not be resumed until rental app reload." )
			end
		end
	)
	kernel.log( "Quick Return Timer initialized" )
	kernel.timerstart( TIMER_NAME )
end
