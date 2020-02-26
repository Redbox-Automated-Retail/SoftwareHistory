--
-- return-product.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if return_manager_error_message_view_manager == nil then
    return_manager_error_message_view_manager = newerrormessageviewmanager()
end

function return_manager_error_message_view_manager_timeout()
    -- make sure we are not stuck waiting for an on complete event from a HAL job that doesn't exist
    if last_return_job_id ~= nil and last_return_job_id ~= "in process" and not kernel.is_active_hardware_job(last_return_job_id) then
        kernel.log("return manager error message view: job id: " .. tostring(last_return_job_id) .. " is no longer active.  Starting new return.")
        last_return_job_id = nil
        return_manager_error_message_view_manager:hide( true )
        kernel.executescript("return_product")
    else
        kernel.log("return manager error message view waiting for completion of job id: " .. tostring(last_return_job_id))
        return_manager_error_message_view_manager:start_timer( return_manager_error_message_view_manager_timeout, 60000 )
    end
end

function show_hardware_busy_message() 
	if not in_maintenance_mode() then
        -- show please wait message
		return_manager_error_message_view_manager:show()
		return_manager_error_message_view_manager:set_message( strings.hardware_busy_message )
        return_manager_error_message_view_manager:start_timer( return_manager_error_message_view_manager_timeout, 60000 )
        -- the following add_session_event data isn't used anywhere so this has been commented out
        --self.shopping_cart:add_session_event(
        --  "Hardware",
        --	"HAL is busy running another job, user waiting for return." )
	end
end


-- only start a return job if there isn't one already running
if not in_maintenance_mode() then
    if last_return_job_id == nil then
        last_return_job_id = "in process"
        if return_pending == true then
            return_pending = false
            kernel.log("starting pending return")
        end
        -- start a return job
        local return_manager = newreturnmanager( ShoppingCart )
        return_manager:init()
        return_manager:begin_return( "standard_return" )
    else
        kernel.log("setting return_pending = true")
        return_pending = true
        show_hardware_busy_message()
    end
end

