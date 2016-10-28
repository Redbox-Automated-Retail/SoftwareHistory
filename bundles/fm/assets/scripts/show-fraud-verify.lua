--
-- show-fraud-verify.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )


local hardware_manager = newhardwaremanager()
local progress_manager = newprogressmanager()

local set_text = function(text)
	kernel.setactortext( "insert_fraud_verify_status", text )
	kernel.setactortextalignx( "insert_fraud_verify_status", "center" )
	kernel.setactortextaligny( "insert_fraud_verify_status", "center" )
end

local register_handlers = function()

    hardware_manager.hardware_event_responders["MachineFull"] = function() set_text("Machine is Full.") end
    hardware_manager.hardware_event_responders["ItemStuckInGripper"] = function() set_text("Disc is stuck.") end
    hardware_manager.hardware_event_responders["MachineError"] = function() set_text("An Error Occurred.  Look in logs for more details.") end
    hardware_manager.hardware_event_responders["NoReceivedItem"] = function() set_text("No item inserted.") end
    hardware_manager.hardware_event_responders["CameraCaptureFailure"] = function() set_text("Camera Failure.") end
    hardware_manager.hardware_event_responders["ItemReturnedToUser"] = function() set_text("Returning item through vend door.") end
    hardware_manager.hardware_event_responders["ItemReturned"] = function() set_text("Disc filed away successfully.") end
    hardware_manager.hardware_event_responders["PutFailureMaxAttempts"] = function() set_text("Failed to locate a place to file disc away after 4 attempts.") end

end

register_handlers()

progress_manager:add_background()

idle_timer:stop()
kernel.createactor(
        "insert_fraud_verify_status",
        false,
        62,
        408,
        900,
        91,
        nil,
        "medium_font",
        "progress_background",
        nil,
        "black",
        nil,
        nil,
        nil,
        nil,
        "Image, Text",
        "Obstruction")

hardware_manager:field_insert_fraud_with_check(
	--ready
    function()
		set_text( "Please insert fraud disc to verify now..." )
	end,
    --complete
	function( job_result )
        local success = false
        local error_text = nil

        if job_result ~= nil then
			for _, v in ipairs( job_result.results ) do
                if v.code == "ItemReturned" and v.item_id ~= nil and v.item_id ~= "UNKNOWN" then
                    success = true
		            kernel.log( v )
			        set_text( tostring(v.item_id) .. " has been verified and successfully put back in inventory." )
                    
                    if DiscActionLogging == true then
                        local barcode_list = {}
                        barcode_list[1] = { v.item_id, "0" } -- action 0=Inserted  1=Removed
                        kernel.fmadiscaction(
							            StoreID,
							            username,
							            "ClearFraud",
							            v.time_stamp24,
							            barcode_list)
                        BarcodeStatus[v.item_id] = { barcode = tostring( v.item_id ), status = "Known" }
                        kernel.log( "barcode = " .. tostring( v.item_id ) .. ", status = Known inserted into BarcodeStatus" )
                    end
                    reload_inventory()
                    break
                elseif v.code == "MachineFull" then
                    error_text = (error_text or "") .. "\nMachine is Full."
                elseif v.code == "ItemStuckInGripper" then
                    error_text = (error_text or "") .. "\nDisc is stuck."
                elseif v.code == "MachineError" then
                    error_text = (error_text or "") .. "\nAn Error Occurred.  Look in logs for more details."
                elseif v.code == "NoReceivedItem" then
                     error_text = (error_text or "") .. "\nNo item inserted."
                elseif v.code == "CameraCaptureFailure" then
                     error_text = (error_text or "") .. "\nCamera Failure."
                elseif v.code == "ItemReturnedToUser" then
                     error_text = (error_text or "") .. "\nReturning item through vend door."
                elseif v.code == "PutFailureMaxAttempts" then
                     error_text = (error_text or "") .. "\nFailed to locate a place to file disc away after 4 attempts."
                end
            end
        end

        if success == false then
            set_text( "Disc was not verified." .. error_text or "" )
        end

		kernel.timercreate(
			"insert_fraud_verify_complete",
			5000,
			5000,
			function()
				kernel.timerstop("insert_fraud_verify_complete")
				kernel.callbackenqueue(
					function( r )
						kernel.removeactor( "insert_fraud_verify_status" )
						progress_manager:remove_background()
						idle_timer:start()
					end,
					{} )
			end
		)
		kernel.timerstart( "insert_fraud_verify_complete" )
	end,
    --busy
	function()
		set_text( "Please Wait..." )
	end
)