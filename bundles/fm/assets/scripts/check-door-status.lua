--
-- check-door-status.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.disableactor( "door_status_button" )
kernel.setactortext( "door_status_button", "Door Status:\nChecking..." )

verify_door_status()