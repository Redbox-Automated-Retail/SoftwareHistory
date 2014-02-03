--
-- trash-job.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local errored_jobs = {}
local trashed_jobs = {}

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

for k,v in pairs( check_box_state ) do
	if k ~= "unload_checkbox" and k ~= "thin_checkbox" then
		if string.sub( v, 1, 3 ) == "KE-" then
			-- Kiosk Engine future scheduled jobs
			kernel.log( "Request trash KE job " .. v )
			local job_trashed = false
			local realjobname = kiosk_engine_jobs[ v ]
			if realjobname ~= nil then
				job_trashed = scheduler_services.remove_entry( realjobname )
			end
			if job_trashed == true then
				kernel.log( ">  Trashed KE job " .. realjobname .. " : " .. v )
				check_box_state[k] = nil
			else
				kernel.log( ">  Unable to trash KE job " .. tostring( realjobname ) .. " : " .. v )
			end
		else
			local hardware_jobs = {}
			local job_hardware_manager = newhardwaremanager()
			local excludelist={"Garbage"}
			hardware_jobs = job_hardware_manager:get_hardware_jobs_by_status(excludelist)
			for j,k in pairs(hardware_jobs) do
				local trash=false
				if v == k.id then
					if (k.program == "thin" or k.program == "unload-thin")
					and k.status ~= "Pending" then
						trash=false
					else
						trash=true	
					end 
				end

				if trash==true then
					if not kernel.hardwaretrashjob(v) then
						table.insert( errored_jobs, v )
					else
						table.insert( trashed_jobs, v )
						check_box_state[k] = nil
					end
				end
			end
		end
	end
end

if #errored_jobs > 0 then
	newmessageboxmanager():show("The following " .. kernel.getlasthitactortag() .. " jobs could not be trashed:\r\n" .. table.concat(errored_jobs, ", "))
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .. "_Trashed",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs could not be trashed: " .. table.concat( errored_jobs, ", "))
end

if #trashed_jobs > 0 then
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .."_Trashed",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs were trashed: " .. table.concat( trashed_jobs, ", "))
end
job_grid:display(true)
