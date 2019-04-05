--
-- inventory-insert-disc.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local hardware_manager = newhardwaremanager()
local progress_manager = newprogressmanager()
local responders = {}
local set_text = function(text)
	kernel.setactortext( "insert_disc_status", text )
	kernel.setactortextalignx( "insert_disc_status", "center" )
	kernel.setactortextaligny( "insert_disc_status", "center" )
end

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

responders.MachineError = function(tokens)
	set_text( "Fatal machine error" )
end

responders.MachineFull =  function(tokens)
	set_text( "Machine is full" )
end

responders.ReturnRetry =  function(tokens)
	set_text( "Failed to read barcode" )
end

responders.ReturnEndOnError = function(tokens)
	set_text( "Insert failed" )
end
--no longer used?
responders.ReturnBarcodeDisplay = function(tokens)
	set_text( "Read barcode: " .. tokens[2] or "UNKNOWN")
end
--no longer used?
responders.ItemReceivedBackwards = function(tokens)
	set_text( "Disc was inserted backwards")
end

responders.ItemInsertError = function(tokens)
	set_text( "Disc was inserted incorrectly")
end

responders.PhotocopyBarcode = function(tokens)
    set_text( "Checking for fraud status" )
    local value = kernel.is_paper_fraud( tokens[2] ) and 'continue' or 'reject'
    local job_signal_command = 'JOB signal job: ' .. tostring( tokens[3] ) .. ' value: ' .. value
    kernel.log( job_signal_command )
    kernel.hardwareexecuteservicecommand( job_signal_command )
        --kernel.process_return_unknown_event( tokens[2], tokens[3] )  --barcode, jobId
end

responders.ItemPlacementDone = function(tokens)
end

hardware_manager.default_status_change_handler = function( jobid, timestamp, message )
	local i, _ = string.find( message, ">STATUS CHANGE<" )
	if i == nil then
		local tokens = {}
		string.gsub(
			message, "(%w+)",
			function(token) tokens[#tokens+1] = token end )
		if #tokens <= 0 then
			return
		end

        local responder = responders[tokens[1]]
		if responder == nil then
			return
		end

        table.insert(tokens, #tokens+1, jobid)
		responder( tokens )
	end
end

for k, _ in pairs(check_box_state) do
	kernel.setactorimage(k, "check_box")
end
check_box_state = {}
kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
kernel.disableactor( "remove_disc_button" )
kernel.setactortext( "select_button", "Select All")
kernel.setactortag( "select_button", "all")
kernel.setactortext( "selected_status", nil )
progress_manager:add_background()

idle_timer:stop()
kernel.createactor(
        "insert_disc_status",
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

--using return_unknown (not return_item) in order to pass in "NONE" as return time
local result = hardware_manager:return_unknown(
	"FMA-INSERT",
	function()
		set_text( "Please insert the disc now..." )
	end,
	function( job_result )
		if job_result ~= nil then
			for _, v in ipairs( job_result.results ) do
                if v.code == "NoFraudMarkersDetected" 
                and v.item_id ~= nil
                and v.item_id ~= ""
                and v.item_id ~= "UNKNOWN"
                and kernel.is_paper_fraud( v.item_id ) == true then
                    if kernel.rental_inventory_get_rapidstatus( v.item_id ) == nil then
                        BarcodeStatus[v.item_id] = { barcode = tostring( v.item_id ), status = "Fraud" }
                        kernel.log( "barcode = " .. tostring( v.item_id ) .. ", status = fraud inserted into barcodestatus" )
                    end
                    kernel.send_fraud_alert( v.item_id, true, v.time_stamp24 )
				elseif v.code == "ItemReturned" and v.item_id ~= nil and v.item_id ~= "UNKNOWN" then
					
					if v.item_id ~= nil and trim(v.item_id) ~= "" then
						kernel.invsvcs_return(
							StoreID,
							"ServiceMenu",
							v.item_id,
							v.deck,
							v.slot,
							v.return_time )
					end

					local deck_slot_string = " Deck: " .. tostring(v.deck) .. " Slot: " .. tostring(v.slot)
					kernel.healthsvcs_sendauditevent(
						StoreID,
						"FMA_InsertDisc",
						username,
						"FMA Bundle",
						"Barcode: " .. tostring(v.item_id) .. deck_slot_string )
					
					-- Send DiscAction Message
					if v.item_id ~= nil and v.item_id ~= "UNKNOWN" and v.item_id ~="" then
						barcode_list = { }
						barcode_list[1] = { tostring(v.item_id), "0" } -- action 0=Inserted  1=Removed
						kernel.fmadiscaction(
							StoreID,
							username,
							"return-unknown",
							v.time_stamp24,
							barcode_list)
					end
		
                    local product_name = "UNKNOWN"
                    local product = BarcodeToProduct[v.item_id]
                    if product ~= nil then
						local master_product = MasterProductCatalog[product.product_id]
						if master_product ~= nil then 
							product_name = MasterProductCatalog[product.product_id].long_name 
						end
                    end

					set_text( "'" .. product_name .. "' was added to inventory at" .. deck_slot_string )
					kernel.logto( "FieldMaintenanceActionLog","Inserted Disc Barcode :".. tostring(v.item_id) )
				elseif v.code == "ItemReturned" then
					local deck_slot_string = " Deck: " .. tostring(v.deck) .. " Slot: " .. tostring(v.slot)
					set_text( "Unknown Item was added to inventory at" .. deck_slot_string )
					kernel.logto( "FieldMaintenanceActionLog","Inserted Disc Barcode :".. tostring(v.item_id) )					
				else
					kernel.log(
						"Unhandled code of: " ..
						v.code ..
						" recieved from the return job!  Details are - " ..
						"Code: " .. tostring( v.code ) ..
						", Deck: " .. tostring( v.deck ) ..
						", Slot: " .. tostring( v.slot ) ..
						", Item ID: " .. tostring( v.item_id ) ..
						", Message: " .. tostring( v.message ) )
				end
			end
		end
		kernel.timercreate(
			"insert_disc_complete",
			5000,
			5000,
			function()
				kernel.timerstop("insert_disc_complete")
				kernel.callbackenqueue(
					function( r )
						kernel.removeactor( "insert_disc_status" )
						progress_manager:remove_background()
						check_box_state = {}
						reload_inventory()
						showinventory()
						idle_timer:start()
					end,
					{} )
			end )
		kernel.timerstart( "insert_disc_complete" )
	end,
	function()
		set_text( "Please Wait" )
	end )
