--
-- keypad-validate-alert.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
KeyPadController.keypad_validate_alert()
kernel.healthsvcs_sendauditevent(
	StoreID,
	"FMA_Send_Inventory_Alert",
	username,
	"FMA Bundle",
	"Inventory statistics alert sent. '" .. tostring( KeyPadController.last_slot_count ) .. "' NR's could not be loaded."
)

kernel.log( "empty slots=" .. tostring( EmptySlots ) .. ", thins=" .. tostring( getinventorycount( "Thinned" ) + getinventorycount( "DNR" ) ) ..
			", damaged=" .. tostring( getinventorycount( "Damaged" ) ) .. ", unknowns=" .. tostring( getinventorycount( "Unknown" ) ) ..
			", unknown titles=" .. tostring( getinventorycount( "Unknown Title" ) ) .. ", frauds=" .. tostring( getinventorycount( "Fraud" ) ) ..
			"; inventory alert was sent." )

local buffer =	"<b>Details (MANUAL FULL MACHINE)</b><br/><br/>"
buffer = buffer .. "<table>"
buffer = buffer .. "<tr><td>Username</td><td>:</td><td>" .. username .. "</td></tr>"
buffer = buffer .. "<tr><td>Kiosk ID</td><td>:</td><td>" .. StoreID .. "</td></tr>"
buffer = buffer .. "<tr><td>Generated At</td><td>:</td><td>" .. get_current_date_string( "%4d%02d%02d" ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Last Inv. Update</td><td>:</td><td>" .. kernel.tolocaltime( InventoryVersion.generated_on.date, "MMMM d h:m tt" ) .. "</td></tr>"
buffer = buffer .. "<tr><td>NR's Not Loaded</td><td>:</td><td>" .. tostring( KeyPadController.last_slot_count ) .. "</td></tr>"
buffer = buffer .. "</table><br/><br/>"
buffer = buffer .. "Inventory Statistics<br/><br/>"
buffer = buffer .. "<table>"
buffer = buffer .. "<tr><td>Empty Slots</td><td>:</td><td>" .. tostring( EmptySlots ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Thins</td><td>:</td><td>" .. tostring( getinventorycount( "Thinned" ) + getinventorycount( "DNR" ) ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Damaged</td><td>:</td><td>" .. tostring( getinventorycount( "Damaged" ) ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Unknowns</td><td>:</td><td>" .. tostring( getinventorycount( "Unknown" ) ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Unknown Titles</td><td>:</td><td>" .. tostring( getunknowntitlecount() ) .. "</td></tr>"
buffer = buffer .. "<tr><td>Frauds</td><td>:</td><td>" .. tostring( getinventorycount( "Fraud" ) ) .. "</td></tr>"
buffer = buffer .. "</table>"


kernel.healthsvcs_sendalert2(
	kernel.getmachinevalueordefault(
		"Store",
		"ID",
		"N/A" ),
	"TitleEmpty",
	"Inventory Alert",
	buffer
)