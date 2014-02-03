--
-- inventory-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local progress_manager = newprogressmanager()
progress_manager:remove_background()

if StoreID == "" or nil then
	kernel.setmacro("StoreNumber", "Unconfigured")
else
	kernel.setmacro("StoreNumber", StoreID)
end
kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
kernel.disableactor( "remove_disc_button" )
kernel.setactortext( "select_button", "Select All")
kernel.setactortag( "select_button", "all")
check_box_state={}

if alpha_table ~= nil then
	if #alpha_table > 0 then
		if last_hit_filter_tab == nil then
			InventoryViewFilter = "all_filter"
		else
			InventoryViewFilter = last_hit_filter_tab
		end
	else
		if last_hit_filter_tab ~= nil then
			InventoryViewFilter = last_hit_filter_tab
		else
			InventoryViewFilter = "all_filter"
			kernel.setactortag( "select_button", "all")
		end
	end
end

if kernel.getlasthitactorname() == "close_area" then
	enable_filters = false
end

if last_hit_filter_tab == nil or InventoryViewFilter == "all_filter" then
	HideSelectAllButton()
else
	ShowSelectAllButton()
end

if sort_columns == nil then
	showinventory("new")
else
	showinventory()
end


