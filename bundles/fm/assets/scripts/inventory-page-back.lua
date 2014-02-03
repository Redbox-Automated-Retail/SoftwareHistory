--
-- inventory-page-back.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

Inventory_Pager:page_back()

show_table(
      Inventory_Scene_Manager,
	  Inventory_Pager,
      Inventory_Table,
      Inventory_Config)
