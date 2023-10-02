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
	kernel.setmacro( "KIOSK_ID", store_id )

	local store = kernel.get_store(store_id)
	if store ~= nil	and store.zip ~= nil then
		local zip = string.split(store.zip, "-", 1)
		kernel.setmacro("MaintenanceView:ZipCode", tostring(zip[1]))
	end
end

MaintenanceModeTimer:start( 100, 600000 )
kernel.hardwarenotifymaintmodestate( true )
