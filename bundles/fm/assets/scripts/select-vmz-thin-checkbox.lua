
if check_box_state["thin_checkbox"] then
	kernel.setactorimage( "thin_checkbox", "check_box")
	check_box_state["thin_checkbox"] = nil
else
	kernel.setactorimage( "thin_checkbox", "check_box_checked")
	check_box_state["thin_checkbox"] = true
	
	-- for VMZ machines, ensure only one of the two check boxes are selected
	if VMZEnabledMachine then
		check_box_state["unload_checkbox"] = nil
		kernel.setmacro("keyboard:newrelease_decks", "" )
		kernel.setactorimage("unload_checkbox", "check_box")
		num_newrelease_decks = 0
	end
end
