--
-- maintenance-view-leave.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

MaintenanceModeTimer.stop( true )
kernel.hardwarenotifymaintmodestate( false )
kernel.refresh_hardware_state()