--
-- set-slot.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local selected_slot = kernel.getmacro("keyboard:slot_textbox")
if selected_slot == "" then
	newmessageboxmanager():show("Please enter a slot")
	return
end

if active_slot_actor == "start_slot" then
	if tonumber(selected_slot) < 1 or tonumber(selected_slot) > tonumber(ending_slot) then
		newmessageboxmanager():show("Slot must be greater than 0 and less than " .. ending_slot)
		return
	end
	starting_slot = selected_slot
else
	if tonumber(selected_slot) < 1 or tonumber(selected_slot) < tonumber(starting_slot) then
		newmessageboxmanager():show("Slot must be greater than " .. starting_slot)
		return
	end
	ending_slot = selected_slot
end

view_goback()
