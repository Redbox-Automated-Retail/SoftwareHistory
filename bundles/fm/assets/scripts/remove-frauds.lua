--
-- show-unknown-discs.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

progress_manager:start( false )

--suspend all jobs upon entering the "remove frauds" screen, resume upon exit
local suspend_result = kernel.hardwaremaintenancemodesuspendjobs()
if not suspend_result.success then
	newmessageboxmanager():show( "Error suspending running jobs." )
end

local all_jobs_suspended = function()
	local suspended_jobs = kernel.hardwaregetsuspendedmaintenancemodejobs()
	
	local hardware_jobs = kernel.hardwaregetjobs()
	local get_job_status = function( job_id )
		for k, v in ipairs( hardware_jobs.results ) do
			if v.id == job_id then
				return v.status
			end
		end
		return nil
	end
	
	for k, v in ipairs( suspended_jobs.results ) do
		local job_status = get_job_status( v.id )
		kernel.log( "Job: ".. v.id..  " - Status: ".. job_status )
		if job_status == "Running" or job_status == "Pending" then
			return false
		end
	end
	return true
end

kernel.timercreate(
	"suspended_jobs_checker",
	2000,
	2000,
	function()
		if all_jobs_suspended() then
			kernel.timerstop( "suspended_jobs_checker" )
			view_show( "remove_frauds_view" )
		end
	end
)
kernel.timerstart( "suspended_jobs_checker" )
