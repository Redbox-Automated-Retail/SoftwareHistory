--
-- inventory-remove-disks.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
kernel.disableactor( "remove_disc_button" )
kernel.setactortext( "select_button", "Select All")
kernel.setactortag( "select_button", "all")
kernel.setactortext( "selected_status", nil )
for k,_ in pairs(check_box_state) do
	kernel.setactorimage(k, "check_box")
end
local progress_manager = newprogressmanager()
progress_manager:add_background()
view_show("remove_disc_popup_view")	
	
  
	

