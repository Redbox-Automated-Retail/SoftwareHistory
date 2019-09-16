--
-- launch-vmz-merch-screen.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

local unknowns = getinventorycount( "Unknown" );

if unknowns == 0 then
	view_show( "vmz_merch_flow_view" )
elseif unknown_discs_attempted == true then
	if unknown_discs_not_completed == true then
		kernel.logto( "FieldMaintenanceActionLog", "Override Unknown Removal" )
		kernel.healthsvcs_sendauditevent(
								StoreID,
								"FMA_VMZMerchUnknownsIncomplete",
								username,
								"FMA Bundle",
								"Failure to complete unknown disc removal.  Remaining Unknowns: " .. tostring(unknowns)
							)
	end
	view_show( "vmz_merch_flow_view" )
else
	view_show( "vmz_merch_warning_view" )
end