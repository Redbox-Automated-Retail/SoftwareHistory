--
-- obj-alert-helper.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newalertmanager()
	local t = {}

	local validate = function( value, name, validator )
		if not validator(value) then
			local message = "AlertManager: invalid parameter value: " .. name .. " = " .. tostring(value)
			kernel.log( message )
		end
	end

	t.format_jobresult = function( jobresult )
		local not_nil = function(o) return o ~= nil end
		validate( jobresult, "Jobresult", not_nil )
		validate( jobresult.code, "Jobresult code", not_nil )
		return
			"<p>Code: " .. jobresult.code ..
			", Deck: " .. tostring( jobresult.deck ) ..
			", Slot: " .. tostring( jobresult.slot ) ..
			", Item ID: " .. tostring( jobresult.item_id ) ..
			", Message: " .. tostring( jobresult.message ) ..
			"</p>"
	end

    t.format_jobresults = function( jobid )
        local table = kernel.hardwaregetprogramresults( jobid )
        local results = table["results"]
        local message = ""
        for _, v in ipairs(results) do
            message = message .. t.format_jobresult( v )
        end
        return message
    end

	t.format_sync_jobresult = function( job_result )
		local not_nil = function(o) return o ~= nil end
		validate( job_result, "Jobresult", not_nil )
		validate( job_result.code, "Jobresult code", not_nil )

		local format =
			"<p>D: " .. tostring( job_result.deck ) ..
			" S: " .. tostring( job_result.slot )

		local job_result_item = job_result.item_id

		-- [Vaibhav Pingle] While testing this on the kiosk, it was found that
		-- the job result contained the field meta_data instead
		-- of field item_id to handle this I added the below check.
		if job_result_item == nil then
			local temp = job_result.meta_data
			local index = (string.find(temp, ",") or 0) - 1
			if index == 0 then
				if #temp == 1 then
					job_result_item = "UNKNOWN"
				else
					job_result_item = string.sub(temp, index + 2, #temp)
				end
			elseif index < 0 then
				job_result_item = temp
			elseif index > 0 then
				job_result_item = string.sub(temp, 1, index)
			end
		end

		if job_result_item == nil or job_result_item == "" or job_result_item == "UNDEFINED" then
			format = format.. " UNDEFINED"
			format = format.. "<b> | Error - ".. job_result.code.. " - ".. job_result.message.. "</b>"
		elseif job_result_item == "UNKNOWN" then
			format = format.. " UNKNOWN"

			if job_result.code ~= "InventoryState"
			and job_result.code ~= "ItemReturned" then
				format = format.. "<b> | Error - ".. job_result.code.. " - ".. job_result.message.. "</b>"
			end
		elseif job_result_item == "EMPTY" then
			format = format.. " EMPTY"
		else
			format = format.. " Barcode:".. job_result_item
			if job_result.code == "ItemReturned" then
				format = format.. "<b> | ".. job_result.code.. " - ".. job_result.message.. "</b>"
		end
		end
		format = format.. "</p>"

		return format
	end

	t.get_jobresult_summary = function( jobresults )
		kernel.log( jobresults )
	
		local result_summary = { 
			["count_undefined"] = 0, 
			["count_unknown"] = 0, 
			["count_empty"] = 0, 
			["count_success"] = 0, 
			["count_excluded"] = 0 
		}

		for _, v in ipairs( jobresults ) do
			local job_result_item = v.item_id

			-- [Vaibhav Pingle] While testing this on the kiosk, it was found that
			-- the job result contained the field meta_data instead
			-- of field item_id to handle this I added the below check.
			if job_result_item == nil then
				local temp = v.meta_data
				local index = ( string.find( temp, "," ) or 0 ) - 1
				if index == 0 then
					if #temp == 1 then
						job_result_item = "UNKNOWN"
					else
						job_result_item = string.sub( temp, index + 2, #temp )
					end
				elseif index < 0 then
					job_result_item = temp
				elseif index > 0 then
					job_result_item = string.sub( temp, 1, index )
				end
			end

			if job_result_item ~= nil then kernel.log( "Found Item:".. job_result_item ) end

			if v.code == "ExcludedSlotSkipped" then
				result_summary["count_excluded"] = result_summary["count_excluded"] + 1
			elseif job_result_item == nil or job_result_item == "" or job_result_item == "UNDEFINED" then
				result_summary["count_undefined"] = result_summary["count_undefined"] + 1
			elseif job_result_item == "UNKNOWN" or string.upper(job_result_item) == "REDBOX" then
				result_summary["count_unknown"] = result_summary["count_unknown"] + 1
			elseif job_result_item == "EMPTY" then
				result_summary["count_empty"] = result_summary["count_empty"] + 1
			else
				result_summary["count_success"] = result_summary["count_success"] + 1
			end
		end
		
		kernel.log( result_summary )
		return result_summary
	end

	t.format_error = function( error )
		return
			"<p>" ..
			tostring( error.code ) ..
			" " ..
			tostring( error.description ) ..
			"<br>Details: " ..
			tostring( error.details ) ..
			"</p>"
	end

	t.format_errors = function( jobid )
		local validstring = function(o) return jobid ~= nil and type( jobid ) == "string" end
		validate( jobid, "Jobid", validstring )

        local job_errors = kernel.hardwaregetjoberrors( jobid )
		local errors = job_errors["results"]
        local message = ""
		if #errors > 0 then
		    message = message .. "<p>The job ended in an Errored state; errors follow:</p>"
			for _, v in pairs( errors ) do
			    message = message .. t.format_error( v )
			end
        end
        return message
    end

    t.send_job_alert = function( class, source, description, jobid, sub_type )
        local formatted_job_results = nil
        local formatted_job_errors = nil
        if jobid ~= nil then
			description = "HAL Job " .. jobid .. "<br>" .. description
            formatted_job_results = t.format_jobresults( jobid )
            formatted_job_errors = t.format_errors( jobid )
        end
        t.send_alert(
            class,
            source,
            description,
            formatted_job_results,
            formatted_job_errors,
			sub_type )
    end

    t.send_alert = function( class, source, description, jobresults, errors, sub_type )
		local validstring = function(v) return type(v) == "string" end

		local nilorvalidstring = function(v) return type(v) == "nil" or type(v) == "string" end

		validate( class, "Class", validstring )
		validate( source, "Source", validstring )
		validate( description, "Description", validstring )
		validate( jobresults, "Jobresults", nilorvalidstring )
		validate( errors, "Errors", nilorvalidstring )
		validate( sub_type, "Sub-Type", nilorvalidstring )

        local buffer =
            "<h2>" ..
            source ..
            "</h2>" ..
            "<p>" ..
            description ..
            "</p>"

		if jobresults ~= nil then
			buffer = buffer .. jobresults
			end

        if errors ~= nil then
			buffer = buffer .. errors
		end

		kernel.log( "AlertManager.send_alert: " .. buffer )

		kernel.healthsvcs_sendalert2(
			kernel.getmachinevalueordefault(
				"Store",
				"ID",
				"N/A" ),
		    class,
			sub_type,
            buffer )
    end

	t.send_alert2 = function (type_of_sync, jobid)
		kernel.log("AlertManager.set_alert2:Start")
		local validstring = function(v) return type(v) == "string" end
		local nilorvalidstring = function(v) return type(v) == "nil" or type(v) == "string" end

		validate( type_of_sync, "Type of Sync", validstring )
		validate( jobid, "Job Id", validstring )
		validate( errors, "Errors", nilorvalidstring )


		local job_result_table = kernel.hardwaregetprogramresults( jobid )
        local results = job_result_table["results"]
		kernel.log(results)

		local job_success = true

		for k, v in ipairs(results) do
			if v.code ~= "InventoryState"
			and v.code ~= "ItemReturned" 
			and v.code ~= "ExcludedSlotSkipped" then
				job_success = false
			end
		end

		local job_errors_table = kernel.hardwaregetjoberrors( jobid )
		local errors = job_errors_table["results"]
		kernel.log(errors)

		local buffer = "<h2>".. type_of_sync

		if (errors ~= nil and #errors ~= 0)
		or job_success ~= true then
			kernel.log("Errored")
			buffer = buffer.. " Errored".. "</h2>"
		else
			buffer = buffer.. " Successful".. "</h2>"
		end

		buffer = buffer.. "<br> HAL Job: " .. jobid .. "<br>"
		buffer = buffer.. "<br> Results:<br>"

		kernel.log( "Result Count:" .. #results )
		kernel.log( "Error Count:" .. #errors )
		if #results ~= 0 then
			local result_summary = t.get_jobresult_summary( results )

			buffer = buffer.. "<table border='0'>"
			buffer = buffer.. "<tr><td>Undefined slots AFTER sync</td><td>:</td><td>".. result_summary["count_undefined"].. "</td></tr>"
			buffer = buffer.. "<tr><td>Unknown slots AFTER sync</td><td>:</td><td>".. result_summary["count_unknown"].. "</td></tr>"
			buffer = buffer.. "<tr><td>Excluded slots</td><td>:</td><td>".. result_summary["count_excluded"].. "</td></tr>"
			buffer = buffer.. "<tr><td>Empty slots AFTER sync</td><td>:</td><td>".. result_summary["count_empty"].. "</td></tr>"
			buffer = buffer.. "<tr><td>Slots Successfully Synced</td><td>:</td><td>".. result_summary["count_success"].. "</td></tr>"
			buffer = buffer.. "<tr><td><b>Total slots covered in Sync</b></td><td><b>:</b></td><td><b>".. #results.. "</b></td></tr>"
			buffer = buffer.. "</table>"

			buffer = buffer.. "<br>Extended Results of ".. type_of_sync.. "<br>Starting Inventory Sync"

			if results ~= nil and #results ~= 0 then
				for _, v in ipairs(results) do
					buffer = buffer.. t.format_sync_jobresult( v )
				end
			end

			buffer = buffer.. "<br>The ".. type_of_sync.. " Completed."
		end

		if errors ~= nil then
			buffer = buffer.. t.format_errors( jobid )
		end

		kernel.healthsvcs_sendalert2(
			kernel.getmachinevalueordefault(
				"Store",
				"ID",
				"N/A" ),
			"TitleEmpty",
			type_of_sync,
			buffer)

		kernel.log( "AlertManager.send_alert2: " .. buffer )
		kernel.log( "AlertManager.send_alert2:Finish" )
	end

	return t
end
