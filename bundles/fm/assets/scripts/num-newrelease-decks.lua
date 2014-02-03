--
-- num-newrlease-decks.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
if kernel.getlasthitactorname() == nil then
	return
end

KeyPadController.on_show =function()
	active_checkbox_actor = kernel.getlasthitactorname()
end

KeyPadController.on_validate = function(num_decks)

	-- check if the slot value is greater than 0 and less than the max dec value
	if tonumber(num_decks) < 1 or tonumber(num_decks) > max_decks then
		newmessageboxmanager():show("deck number is invalid" ) 
		return false
	end
	num_newrelease_decks = tonumber(num_decks)

	check_box_state[active_checkbox_actor] = kernel.getactortag( active_checkbox_actor ) or true
	kernel.setactorimage( 
		active_checkbox_actor,
		"check_box_checked")

	-- for VMZ machines, ensure only one of the two check boxes are selected
	if VMZEnabledMachine then
		check_box_state["thin_checkbox"] = nil
		kernel.setactorimage("thin_checkbox", "check_box")
	end

	-- reset the keypad title
	kernel.setmacro("keyboard:title_area", "   Enter a Slot")
	return true
end

-- Determine if the checkbox should be unchecked or launch the keypad
if check_box_state["unload_checkbox"] then
	-- Currently checked, clear the checkbox and deck count
	num_newrelease_decks = 0
	kernel.setmacro("keyboard:newrelease_decks", "" )
	check_box_state["unload_checkbox"] = nil
	kernel.setactorimage( 
		"unload_checkbox",
		"check_box")
else
	-- Launch the keypad to get deck count
	local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
	kernel.logto( "FieldMaintenanceActionLog", log_value )
	KeyPadController.set_display_target("slot_textbox",2)
	KeyPadController.show()
	kernel.setmacro("keyboard:title_area", "   # of Last Deck with New Releases")
end
