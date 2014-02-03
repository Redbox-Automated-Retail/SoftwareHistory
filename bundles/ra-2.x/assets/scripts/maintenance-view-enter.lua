--
-- maintenance-view-enter.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
if MaintenanceModeTimer == nil then
	MaintenanceModeTimer = newmaintenancemodetimer()
	MaintenanceModeTimer.clear()
end

if DataAccessAlreadySent ~= true or AllowEntry ~= nil and AllowEntry == true then
	AllowEntry = false
	local store_id = kernel.getmachinevalueordefault( "Store", "ID", "Not Configured" )
	if Stores ~= nil
	and Stores[store_id] ~= nil
	and Stores[store_id].zip ~= nil
	then
		local zip = string.split(Stores[store_id].zip, "-", 1)
		kernel.setmacro("MaintenanceView:ZipCode", tostring(zip[1]))
	end
end

MaintenanceModeTimer:start( 100, 600000 )
kernel.hardwarenotifymaintmodestate( true )
