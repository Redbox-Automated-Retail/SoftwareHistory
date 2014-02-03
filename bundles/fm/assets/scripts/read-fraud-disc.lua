--
-- read-fraud-disc.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

local hardware_manager = newhardwaremanager()
local progress_manager = newprogressmanager()

local set_text = function(text)
	kernel.setactortext( "read_fraud_status", text )
	kernel.setactortextalignx( "read_fraud_status", "center" )
	kernel.setactortextaligny( "read_fraud_status", "center" )
end

progress_manager:add_background()

idle_timer:stop()
kernel.createactor(
        "read_fraud_status",
        false,
        (1024-900)/2,
        (768-91)/2 + 70,
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

hardware_manager:read_fraud_disc(
	function()
		set_text( "Please insert Fraud disc now..." )
	end,
	function( job_result )
		kernel.log( job_result )
		fraud_disc = job_result.results[1]
		if fraud_disc.code == "FraudItem" then
			set_text( tostring(fraud_disc.item_id).. " will be marked as a Fraud disc." )
			kernel.updatefraudstatus(
				StoreID,
				fraud_disc.item_id,
				"true",
				fraud_disc.time_stamp,
				nil
			)
		end
		kernel.timercreate(
			"read_fraud_complete",
			5000,
			5000,
			function()
				kernel.timerstop("read_fraud_complete")
				kernel.callbackenqueue(
					function( r )
						kernel.removeactor( "read_fraud_status" )
						progress_manager:remove_background()
						idle_timer:start()
					end,
					{} )
			end
		)
		kernel.timerstart( "read_fraud_complete" )
	end,
	function()
		set_text( "Please Wait" )
	end
)