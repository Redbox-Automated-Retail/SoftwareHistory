--
-- show-keypad-popup.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
KeyPadController.set_display_target("slot_textbox",2)
KeyPadController.show()

