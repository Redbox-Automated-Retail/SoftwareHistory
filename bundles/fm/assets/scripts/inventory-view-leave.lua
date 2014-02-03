--
-- inventory-view-leave.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

InventoryViewFilter = "all_filter"
kernel.disableactor("close_area")

if kernel.getlasthitactorname() == "close_area" then
	sort_columns = nil
end