--
-- toggle-maintenance-mode.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local result

maintenancemodestate = not maintenancemodestate

if maintenancemodestate then
   result = kernel.hardwaremaintenancemodesuspendjobs()
   if not result.success then
      newmessageboxmanager():show( "Error turning on maintenance mode." )
   else
      kernel.setactortext( "start_maintenance_mode", "Turn Maintenance Mode Off" )
   end
else
   result = kernel.hardwaremaintenancemoderesumejobs()
   if not result.success then
      newmessageboxmanager():show( "Error turning off maintenance mode." )
   else
      kernel.setactortext( "start_maintenance_mode", "Turn Maintenance Mode On" )
   end
end

kernel.markactordirty( "start_maintenance_mode" )
kernel.setmachinevalue("Field Maintenance", "MaintenanceMode", tostring(maintenancemodestate))
