--
-- suspend-job.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local errored_jobs = {}
local suspended_jobs = {}
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
for k,v in pairs(check_box_state) do
	if k ~= "unload_checkbox" and k ~= "thin_checkbox" then
		if not kernel.hardwaresuspendjob(v) then
			table.insert( errored_jobs, v )
		else
			table.insert( suspended_jobs, v )
			check_box_state[k] = nil
		end
	end
end

if #errored_jobs > 0 then
	newmessageboxmanager():show( "The following " .. kernel.getlasthitactortag() .. " jobs could not be suspended:\r\n" .. table.concat(errored_jobs, ", "))
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .."_Suspended",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs could not be suspended: " .. table.concat( errored_jobs, ", "))
end

if #suspended_jobs > 0 then
	kernel.healthsvcs_sendauditevent(
				StoreID,
				"FMA_".. kernel.getlasthitactortag() .."_Suspended",
				username,
				"FMA Bundle",
				"The following " .. kernel.getlasthitactortag() .. " jobs were suspended: " .. table.concat( suspended_jobs, ", "))
end

job_grid:display(true)
