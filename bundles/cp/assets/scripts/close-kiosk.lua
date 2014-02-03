--
-- close-kiosk.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

update_inventory = function()
	view_show( "close_kiosk_popup_view" )
	kernel.setactortext( "header_text", "Updating Inventory..." )
	kernel.hideactor( "message_text" )
	kernel.hideactor( "button_1" )
	kernel.hideactor( "button_2" )
	kernel.showactor( "spinner" )
	kernel.timercreate(
		"spinner_timer",
		0,
		75,
		function()
			kernel.setactorframe( "spinner" )
		end )
	kernel.timerstart( "spinner_timer" )
	local items = {}
	kernel.hardwaresoftsync(
		nil,
		function( jobid, status )
			if status == "Completed" then
				local job_result = kernel.hardwaregetprogramresults( jobid )
				for _, v in ipairs( job_result.results ) do
					if v.item_id ~= nil and v.item_id ~= "" and string.lower( v.item_id ) ~= "unknown" and string.lower( v.item_id ) ~= "redbox" and string.lower( v.item_id ) ~= "empty" and string.lower( v.item_id ) ~= "undefined" and v.deck ~= 0 and v.slot ~= 0 then
						table.insert( items, { barcode = v.item_id, deck = v.deck, slot = v.slot, status = "Removed" } )
					end
				end
				kernel.invsvcs_snapshotimmediate(
					StoreNumber,
					items,
					function( result )
						if result.success == true then
							kernel.hardwareexecuteimmediate(
								"INVENTORY CLEARALL",
								function()
									kernel.deletefilefromrelativepath( "data\\cache.data" )
									kernel.deletefilefromrelativepath( "data\\local.data" )
									kernel.setactortext( "header_text", "Inventory Removed" )
									kernel.showactor( "message_text" )
									kernel.setactortext( "message_text", "Inventory successfully marked as removed for this kiosk.\n\n" .. tostring( #items ) .. " Inventory Record(s) Updated.\n\nYou may continue the process of closing the kiosk." )
									kernel.showactor( "button_2" )
									kernel.setactortext( "button_2", "Continue" )
									if kernel.timerexists( "spinner_timer" ) then
										kernel.timerstop( "spinner_timer" )
										kernel.timerremove( "spinner_timer" )
									end
									kernel.hideactor( "spinner" )
									kernel.registeractorhitnotify(
										"button_2",
										function()
											kernel.healthsvcs_sendauditevent(
												StoreNumber,
												"CP_CloseKiosk",
												CurrentUser.id,
												"CP Bundle",
												"Marked " .. tostring( #items ) .. " barcode(s) as removed."
											)
											view_goback()
										end
									)
								end,
								false
							)
						else
							kernel.setactortext( "header_text", "Error" )
							kernel.showactor( "message_text" )
							kernel.setactortext( "message_text", "An error occurred while trying to update the inventory.\n\nWould you like to try again?" )
							kernel.showactor( "button_1" )
							kernel.setactortext( "button_1", "No" )
							kernel.showactor( "button_2" )
							kernel.setactortext( "button_2", "Yes" )
							if kernel.timerexists( "spinner_timer" ) then
								kernel.timerstop( "spinner_timer" )
								kernel.timerremove( "spinner_timer" )
							end
							kernel.hideactor( "spinner" )
							kernel.registeractorhitnotify(
								"button_1",
								function()
									view_goback()
								end
							)
							kernel.registeractorhitnotify(
								"button_2",
								function()
									view_goback()
									update_inventory()
								end
							)
						end
					end
				)
			end
		end
	)
end
KeyNavigationManager.stop()
view_show( "close_kiosk_popup_view" )
kernel.setactortext( "header_text", "Warning!" )
kernel.setactortext( "message_text", "Clicking Yes will mark ALL known kiosk inventory as removed.\n\nAre you sure you want to continue?" )
kernel.setactortext( "button_1", "No" )
kernel.setactortext( "button_2", "Yes" )
kernel.hideactor( "confirm_textbox" )
kernel.registeractorhitnotify(
	"button_1",
	function()
		view_goback()
	end
)
kernel.registeractorhitnotify(
	"button_2",
	function()
		view_goback()
		KeyNavigationManager.stop()
		view_show( "close_kiosk_popup_view" )
		kernel.setactortext( "header_text", "Are you sure?" )
		kernel.setactortext( "message_text", "Clicking Yes again will mark ALL known kiosk inventory as removed. This action is not reversible.\n\nTo continue, enter 'close' into the textbox below." )
		kernel.setactortext( "button_1", "No" )
		kernel.setactortext( "button_2", "Yes" )
		kernel.disableactor( "button_2" )
		kernel.setactorforegroundcolor( "button_2", "gray" )
		kernel.setmacro( "CloseKiosk:ConfirmText", "" )
		kernel.showactor( "confirm_textbox" )
		kernel.registeractorhitnotify(
			"button_1",
			function()
				view_goback()
			end
		)
		kernel.registeractorhitnotify(
			"button_2",
			function()
				view_goback()
				update_inventory()
			end
		)
		kernel.registerkeypressnotify(
			"close_kiosk_key_handler",
			function( keystroke )
				local confirm_string = kernel.getmacro( "CloseKiosk:ConfirmText" ) or ""
				if keystroke == "\b" then
					if string.len( confirm_string ) > 0 then
						confirm_string = string.sub( confirm_string, 1, string.len( confirm_string ) - 1 )
					end
				elseif keystroke == " " or keystroke == "\r" then
					confirm_string = confirm_string.. ""
				else
					confirm_string = confirm_string.. keystroke
				end
				
				kernel.log( confirm_string )
				if #confirm_string < 6 then
					kernel.setmacro( "CloseKiosk:ConfirmText", confirm_string )
				end
				if kernel.getmacro( "CloseKiosk:ConfirmText" ) == "close" then
					kernel.setactorforegroundcolor( "button_2", "White" )
					kernel.enableactor( "button_2" )
				else
					kernel.disableactor( "button_2" )
					kernel.setactorforegroundcolor( "button_2", "gray" )
				end
			end
		)
	end
)

