--
-- inventory-filter-onhit.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

check_box_state = {}
enable_filters = false
last_hit_filter_tab = kernel.getlasthitactorname()
last_hit_filter_tag = kernel.getactortag(last_hit_filter_tab)

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

InventoryViewFilter = kernel.getlasthitactorname()
kernel.setactortext( "select_button", "Select All")
kernel.setactortag( "select_button", "all")
kernel.setactorforegroundcolor( "remove_disc_button", "gray" )
kernel.disableactor( "remove_disc_button" )

if kernel.getlasthitactortag() == "Unknown" then
	ShowSelectAllButton()
	kernel.disableactor("select_button")
	kernel.setactorforegroundcolor( "select_button", "gray" )
elseif kernel.getlasthitactortag() == "ALL" then
	HideSelectAllButton()
else
	ShowSelectAllButton()
	kernel.setactorforegroundcolor( "select_button", "white" )
end


showinventory()

if kernel.getlasthitactortag() == "Unknown" or kernel.getlasthitactortag() == "Unknown Title" then
   kernel.hideactor( "letter_navigation_button" )
else
	kernel.showactor( "letter_navigation_button" )
end