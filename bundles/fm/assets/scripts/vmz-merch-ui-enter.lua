--
-- vmz-merch-ui-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:stop()
local first_enter = kernel.getmacro("MerchVmzFirstEnter")
if first_enter == nil then
	kernel.healthsvcs_sendauditevent(
		StoreID,
		"FMA_MerchandizeVMZ",
		username,
		"FMA Bundle",
		"FMA: VMZ Management> Merchandize VMZ selected" )
		kernel.setmacro("MerchVmzFirstEnter", "visited" )
end

last_thin_text = "Last thin at Deck " .. qlm_info.thin_deck .. ", Slot " .. qlm_info.thin_slot
if qlm_info.last_thin_unavailable == true then
	last_thin_text = "Last thin information unavailable."
elseif qlm_info.thin_deck == 0 or qlm_info.thin_slot == 0 then
	last_thin_text = "No thins in the VMZ."
end
kernel.setactortext("pull_last_thin_label", last_thin_text)

local function send_vmz_removed_thins_alert()
	local hardware_jobs = HardwareManager:get_hardware_jobs_by_program( { "vmz-removed-thins" }, { "Pending", "Running", "Suspended", "Garbage" } )
	for k1, v1 in ipairs( hardware_jobs ) do
		local msg = "Remove thins from VMZ<br>"
		msg = msg .. "Remove " .. v1.status .. "<br>"
		msg = msg .. "HAL job: " .. v1.id .. "<br>"
		
		local results_msg = "<br>RESULTS:<br>"
		local thins_removed_count = 0
		local non_thins_removed_count = 0
		local empty_slot_count = 0
		
		local result_table = kernel.hardwaregetprogramresults( v1.id )
		local results = result_table["results"]
		for k2, v2 in ipairs( results ) do
			local barcode = v2.item_id or "UNKNOWN"
			if v2.code == "ThinTransferSuccessful" then
				thins_removed_count = thins_removed_count + 1
				results_msg = results_msg .. "Code: " .. v2.code .. " Deck: " .. v2.deck .. " Slot: " .. v2.slot .. " Barcode: " .. barcode .. "<br>"
			elseif v2.code == "NonThinRemoved" then
				non_thins_removed_count = non_thins_removed_count + 1
				results_msg = results_msg .. "Code: " .. v2.code .. " Deck: " .. v2.deck .. " Slot: " .. v2.slot .. " Barcode: " .. barcode .. "<br>"
			elseif v2.code == "MachineEmptySlots" then
				empty_slot_count = v2.message
			end
		end
		
		msg = msg .. "Remaining Empty Slots in Kiosk: " .. tostring( empty_slot_count ) .. "<br>"
		msg = msg .. "Thins Removed: " .. tostring( thins_removed_count ) .. "<br>"
		msg = msg .. "Non-thins Removed: " .. tostring( non_thins_removed_count ) .. "<br>"
		msg = msg .. results_msg
		
		kernel.healthsvcs_sendalert2(
			StoreID,
			"TitleEmpty",
			"VMZ-Thin",
			msg
		)
		
		kernel.hardwaretrashjob( v1.id )
	end
end

kernel.registeractorhitnotify( "reset_bin_counter_button",
	function()
		kernel.log( "Reset bin counter." )
		HardwareManager:executeimmediate( "DUMPBIN RESETCOUNTERMOVE", false, nil )
		kernel.disableactor( "reset_bin_counter_button" )
		kernel.healthsvcs_sendauditevent(
			StoreID,
			"FMA_Continue",
			username,
			"FMA Bundle",
			"FMA: Merchandize VMZ> Continue selected" )
	end
)

kernel.registeractorhitnotify( "close_screen_button",
    function()
        kernel.log( "Closing merchandizing UI screen." )
        local progress_mgr = newprogressmanager()
        progress_mgr:start( false )
        HardwareManager:schedule_job(
            "vmz-removed-thins",
            "VMZ Removed Thins",
            false,
            "Highest",
            nil,
            nil,
            nil,
            function()
                send_vmz_removed_thins_alert()
                kernel.healthsvcs_sendauditevent(
                    StoreID,
                    "FMA_CloseMerchandizeVMZ",
                    username,
                    "FMA Bundle",
                    "FMA: Merchandize VMZ> Close selected"
                )
                HardwareManager:executeimmediate(
                    "VMZ MARK-EMPTY",
                    false,
                    function()
                        kernel.log( "Refresh Inventory as last step of VMZ Close" )
                        HardwareManager:soft_sync(
                            function()
                                reload_inventory()
                                if progress_mgr ~= nil then
                                    progress_mgr:stop()
                                end
                                view_goback()
                                idle_timer:start()
                            end
                        )    
                    end
                )
            end,
            nil,
            false
        )
    end
)

kernel.registeractorhitnotify( "view_unload_diagram_button",
	function()
		kernel.setlasthitactortag( "unload" )
		kernel.logto( "FieldMaintenanceActionLog", "launch view diagram with unload argument" )
		view_show( "vmz_merch_popup_view" )
	end
)