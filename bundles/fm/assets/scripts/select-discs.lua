--
-- select-discs.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if #Inventory_Table <= 0 then
	return
end

local current = kernel.getactortag("select_button")
check_box_state = {}
if current == "all" then
	for _,v in ipairs(Inventory_Table) do
		local deck = v["deck"] or " "
		local slot = v["slot"] or " "
		local barcode = v["barcode"] or " "
		local check_box_name = "checkbox_" .. barcode .. "_" .. deck .. "_" .. slot
		
		-- Disable the check box only if removal of unknowns is only enabled through the new 'Unknowns' UI.
		if barcode ~= "UNKNOWN" then
			check_box_state[check_box_name] = v
		end
	end
	kernel.setactortext( "select_button", "Clear All")
	kernel.setactortag( "select_button", "none")
	kernel.setactorforegroundcolor( "remove_disc_button", "White" )
	kernel.enableactor( "remove_disc_button" )
else
	kernel.setactortext( "select_button", "Select All")
	kernel.setactortag( "select_button", "all")
	kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
	kernel.disableactor( "remove_disc_button" )
end

showinventory()
