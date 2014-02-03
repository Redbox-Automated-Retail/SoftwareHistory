--
-- export-inventory.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

--if kernel.getenvironment() == "Development" then
	print(" ")
	print(" ")
	print("USE [RedboxServerDB-HAL]")
	print("GO")
	print("BEGIN")
	print("DECLARE @Disc_ID INT;")
	print(" ")
	for _, v in ipairs( Inventory_All_Table ) do
		if v.product_id ~= nil and v.barcode ~= nil then
			print( "EXEC RB_InsertDisk @Disc_ID, " .. tostring( v.product_id ) .. ", '" .. v.barcode .. "' ;" )
		end
	end
	print("END")
	print(" ")
	print(" ")
--end

