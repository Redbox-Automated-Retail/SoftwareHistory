--
-- resume-job.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local errored_jobs = {}
local resumed_jobs = {}

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

for k,v in pairs(check_box_state) do
	if k ~= "unload_checkbox" and k ~= "thin_checkbox" then
		if not kernel.hardwareresumejob(v) then
			table.insert( errored_jobs, v )
		else
			table.insert( resumed_jobs, v )
			check_box_state[k] = nil
		end
	end
end

if #errored_jobs > 0 then
	newmessageboxmanager():show( "The following " .. kernel.getlasthitactortag() .. " jobs could not be resumed:\r\n" .. table.concat(errored_jobs, ", "))
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .."_Resumed",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs could not be resumed: " .. table.concat( errored_jobs, ", "))
end

if #resumed_jobs > 0 then
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .."_Resumed",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs were resumed: " .. table.concat( resumed_jobs, ", "))
end

job_grid:display(true)
