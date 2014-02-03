--
-- select-all.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

for index = 1, (max_decks or 0) do
	local actor_name = "deck_button_" .. tostring(index)
	kernel.setactortag(actor_name, "selected")
	kernel.showactor(actor_name .. "_glow")
end
