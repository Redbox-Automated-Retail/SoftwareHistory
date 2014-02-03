--
-- test-sync-inventory.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

function getworktable() return nil end
function getprofiletable() return nil end

dofile( "../extension-functions.lua" )
dofile( "../inventory-functions.lua" )

TestSyncInventory = {}


function TestSyncInventory:setUp()
	kernel = newluaunitmockkernel()

	function get_current_date_string() return "20091111" end
end

function TestSyncInventory:tearDown()
	kernel = nil
	RemovedQueue         = nil
	ProductHistory       = nil
	ProfileVersion       = nil
	ProductCatalog       = nil
	InventoryVersion     = nil
	BarcodePosition      = nil
	ProductToBarcode     = nil
	BarcodeToProduct     = nil
	MasterProductCatalog = nil
end

function TestSyncInventory:test_ItemReturned_missing_barcode_to_product_known_barcode()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodeToProduct:count(), 0 )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown Title" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_ItemReturned_missing_barcode_to_product_unknown_barcode()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	MasterProductCatalog = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "UNKNOWN" }
	sync_inventory( job_result )

	assert_equals(BarcodeToProduct:count(), 0 )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "UNKNOWN" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_ItemVended_missing_barcode_to_product()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodeToProduct:count(), 0 )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_SoftSync_missing_barcode_to_product_known_barcode()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodeToProduct:count(), 0 )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown Title" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_SoftSync_existing_barcode_to_product_empty_barcode()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "EMPTY" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_SoftSync_missing_barcode_to_product_unknown_barcode()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()

	-- mutable
	BarcodePosition = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "UNKNOWN" }
	sync_inventory( job_result )

	assert_equals(BarcodeToProduct:count(), 0 )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "UNKNOWN" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
end

function TestSyncInventory:test_ItemReturned_all_mutables_empty_are_created()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	-- testing product_catalog::can_be_sold flag is working

	-- immutable
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemVended_all_mutables_empty_are_created()
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_SoftSync_all_mutables_empty_are_created()

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemReturned_BarcodePosition_exists()
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1"] = { barcode = "000000000001", status = "Known", total_rental_count = 111 }
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemVended_BarcodePosition_exists()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1"] = { barcode = "000000000001", status = "Known", total_rental_count = 111 }
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_SoftSync_BarcodePosition_exists()

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1"] = { barcode = "000000000001", status = "Known", total_rental_count = 111 }
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemReturned_ProductCatalog_exists()
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1, in_stock = true }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemVended_ProductCatalog_exists()
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1, in_stock = true }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_SoftSync_ProductCatalog_exists()

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1, in_stock = true }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemReturned_ProductToBarcode_exists()
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductToBarcode[1] = { ["000000000001"] = { status = "Known", total_rental_count = 111 } }
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0)
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemVended_ProductToBarcode_exists()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductToBarcode[1] = { ["000000000001"] = { status = "Known", total_rental_count = 111 } }
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_SoftSync_ProductToBarcode_exists()

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductToBarcode[1] = { ["000000000001"] = { status = "Known", total_rental_count = 111 } }
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

function TestSyncInventory:test_ItemReturned_ProductHistory_exists()

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { product_id = 1, on_hand_count = 5, ofr_count = 4 }

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 3 )
	assert_equals(ProductHistory[1].on_hand_count, 1 ) -- reset to producttobarcode count
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

function TestSyncInventory:test_ItemVended_ProductHistory_exists()
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091101", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )

	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { product_id = 1, on_hand_count = 3, ofr_count = 4, last_max_ofr = 6, last_max_ofr_date = "20091101" }

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 5 )
	assert_equals(ProductHistory[1].on_hand_count, 0 ) -- reset to producttobarcode count
	assert_equals(ProductHistory[1].last_max_ofr, 6 )
	assert_equals(ProductHistory[1].current_max_ofr, 5 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091101" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
end

function TestSyncInventory:test_SoftSync_ProductHistory_exists()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { product_id = 1, created_date = "20091001", on_hand_count = 3, ofr_count = 4, last_max_ofr = 6, last_max_ofr_date = "20091101" }

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091101", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091001", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091001", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091001" )
	assert_equals(ProductHistory[1].ofr_count, 4 )
	assert_equals(ProductHistory[1].on_hand_count, 1 ) -- reset to producttobarcode count
	assert_equals(ProductHistory[1].last_max_ofr, 6 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091101" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

function TestSyncInventory:test_ItemReturned_product_entry_in_product_catalog()
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1 }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_equals(BarcodeToProduct:count(), 1 )
	assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
	assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_ItemVended_product_entry_in_product_catalog()
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )

	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1 }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_equals(BarcodeToProduct:count(), 1 )
	assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
	assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_SoftSync_product_entry_in_product_catalog()
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1 }
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_equals(BarcodeToProduct:count(), 1 )
	assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
	assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

-------------------------------------------------------------------------------
-- workflow tests -------------------------------------------------------------
-------------------------------------------------------------------------------

function TestSyncInventory:test_cache_blank_soft_sync_three_barcodes_for_product()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 4, code = "SoftSync", item_id = "UNKNOWN" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 4 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "UNKNOWN" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 4 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "UNKNOWN" )
	assert_equals(BarcodePosition["1@4"].status, "Unknown" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_cache_blank_return_three_barcodes_for_product()
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )

	job_result = { deck = 1, slot = 3, code = "ItemReturned", item_id = "000000000003" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 4, code = "ItemReturned", item_id = "UNKNOWN" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 4 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "UNKNOWN" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 4 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "UNKNOWN" )
	assert_equals(BarcodePosition["1@4"].status, "Unknown" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_cache_synced_soft_sync_three_barcodes_for_product()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1"] = { barcode = "000000000001", status = "Known", total_rental_count = 111 }
	BarcodePosition["1@2"] = { barcode = "000000000002", status = "Known", total_rental_count = 222 }
	BarcodePosition["1@3"] = { barcode = "000000000003", status = "Known", total_rental_count = 333 }
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1, in_stock = true }
	ProductToBarcode = newluaunitmockworktable()
	ProductToBarcode[1] = {
		["000000000001"] = { status = "Known", total_rental_count = 111 },
		["000000000002"] = { status = "Known", total_rental_count = 222 },
		["000000000003"] = { status = "Known", total_rental_count = 333 }
	}
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { product_id = 1, on_hand_count = 3 }

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, 222 )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, 333 )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, 222 )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, 333 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, 333 )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, 333 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	local job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_cache_synced_vend_three_barcodes_for_product()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1"] = { barcode = "000000000001", status = "Known", total_rental_count = 111 }
	BarcodePosition["1@2"] = { barcode = "000000000002", status = "Known", total_rental_count = 222 }
	BarcodePosition["1@3"] = { barcode = "000000000003", status = "Known", total_rental_count = 333 }
	ProductCatalog = newluaunitmockworktable()
	ProductCatalog[1] = { product_id = 1, in_stock = true }
	ProductToBarcode = newluaunitmockworktable()
	ProductToBarcode[1] = {
		["000000000001"] = { status = "Known", total_rental_count = 111 },
		["000000000002"] = { status = "Known", total_rental_count = 222 },
		["000000000003"] = { status = "Known", total_rental_count = 333 }
	}
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { product_id = 1, on_hand_count = 3 }

	local job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, 222 )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, 333 )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, 222 )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, 333 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "ItemVended", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, 333 )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, 333 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	job_result = { deck = 1, slot = 3, code = "ItemVended", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	sync_inventory( job_result )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@3"].status, "Unknown" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 3 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 3 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091111" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091111" )
end

function TestSyncInventory:test_cache_blank_single_barcode_soft_sync_multiple_rent_return()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 1 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091112" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20091112", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365 } )
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091112" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091112" )

	function get_current_date_string() return "20091113" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091112", end_date_string = "20091113", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091113", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091112" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091112" )

	function get_current_date_string() return "20091114" end
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091114", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 3 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1)
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091114" )

	function get_current_date_string() return "20091115" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091115", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091115", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091114" )

	function get_current_date_string() return "20091116" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091116", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091116", end_date_string = "20091116", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091116", response = 5 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091116", response = 365 } )
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091116" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091116" )

	function get_current_date_string() return "20091117" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091116", end_date_string = "20091117", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091117", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091116" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091116" )

	function get_current_date_string() return "20091118" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091118", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091118", end_date_string = "20091118", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091118", response = 7 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091118", response = 365 } )
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, "20091118" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091118" )

	function get_current_date_string() return "20091119" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091118", end_date_string = "20091119", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091119", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, "20091118" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091118" )

	function get_current_date_string() return "20091120" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091120", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091120", end_date_string = "20091120", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091118", end_date_string = "20091120", response = 2 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091120", response = 365 } )
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, "20091120" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091120" )

	function get_current_date_string() return "20091121" end
	kernel:getelapseddays_data( { start_date_string = "20091120", end_date_string = "20091121", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091121", response = 365 } )
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, "20091120" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091120" )
end

function TestSyncInventory:test_cache_blank_three_barcode_soft_sync_multiple_rent_return()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	local job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	local job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091112" end

	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091113" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091113", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091113", response = 2 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091114" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 3 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 3 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 3 } )
	job_result = { deck = 1, slot = 2, code = "ItemVended", item_id = "000000000002" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091115" end
	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091115", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091115", response = 4 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091116" end
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091116", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091116", response = 5 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091116", response = 5 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091116", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091116", response = 5 } )
	job_result = { deck = 1, slot = 3, code = "ItemVended", item_id = "000000000003" }
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@3"].status, "Unknown" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091117" end
	job_result = { deck = 1, slot = 3, code = "ItemReturned", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091117", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091117", response = 6 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091118" end
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091118", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091118", response = 7 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091118", response = 7 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091118", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091118", response = 7 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091119" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091119", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091119", response = 8 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091120" end
	job_result = { deck = 1, slot = 2, code = "ItemVended", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091120", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091120", response = 9 } )
	kernel:getelapseddays_data( { start_date_string = "20091118", end_date_string = "20091120", response = 2 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091120", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091120", response = 9 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091121" end
	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091121", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091121", response = 10 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091118" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_all_mutables_empty_return_discs_to_kiosk_that_hasnt_seen_them()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 112 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 113 }
	BarcodeToProduct["000000000004"] = { product_id = 1, status = "Known", total_rental_count = 114 }
	BarcodeToProduct["000000000005"] = { product_id = 1, status = "Known", total_rental_count = 115 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 3, code = "ItemReturned", item_id = "000000000003" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 4, code = "ItemReturned", item_id = "000000000004" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 4 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "000000000004" )
	assert_equals(BarcodePosition["1@4"].status, "Known" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 4 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000004"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000004"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 4 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

function TestSyncInventory:test_cache_blank_three_barcode_soft_sync_rent_all_return_all()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 333 }
	local function assert_BarcodeToProduct()
		assert_equals(BarcodeToProduct:count(), 3 )
		assert_equals(BarcodeToProduct["000000000001"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000001"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000001"].total_rental_count, 111 )
		assert_equals(BarcodeToProduct["000000000002"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000002"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000002"].total_rental_count, 222 )
		assert_equals(BarcodeToProduct["000000000003"].product_id, 1 )
		assert_equals(BarcodeToProduct["000000000003"].status, "Known" )
		assert_equals(BarcodeToProduct["000000000003"].total_rental_count, 333 )
	end
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	local job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	local job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "SoftSync" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	function get_current_date_string() return "20091112" end
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365  } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365  } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1  } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	function get_current_date_string() return "20091113" end
	job_result = { deck = 1, slot = 2, code = "ItemVended", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091113", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091113", response = 3 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091113", response = 3 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091113", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091113", response = 2 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2)
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	function get_current_date_string() return "20091114" end
	job_result = { deck = 1, slot = 3, code = "ItemVended", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 5 } )
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091114", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091114", response = 5 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091114", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "ItemVended" )
	assert_equals( job_result.item_id, "EMPTY" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@3"].status, "Unknown" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 3 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 3 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091114" )

	function get_current_date_string() return "20091115" end
	job_result = { deck = 1, slot = 1, code = "ItemReturned", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091115", response = 1 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091115", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 1 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000001" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@3"].status, "Unknown" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 3 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091114" )

	function get_current_date_string() return "20091116" end
	job_result = { deck = 1, slot = 2, code = "ItemReturned", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091116", response = 2 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091116", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 2 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000002" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@3"].status, "Unknown" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 3 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )

	function get_current_date_string() return "20091117" end
	job_result = { deck = 1, slot = 3, code = "ItemReturned", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091114", end_date_string = "20091117", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091117", response = 365 } )
	sync_inventory( job_result )

	assert_equals( job_result.deck, 1 )
	assert_equals( job_result.slot, 3 )
	assert_equals( job_result.code, "ItemReturned" )
	assert_equals( job_result.item_id, "000000000003" )

	assert_BarcodeToProduct()
	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 3 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
 	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 3 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 3 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091114" )
	assert_equals(ProductHistory[1].last_date_out_of_stock, "20091114" )
end

function TestSyncInventory:test_SoftSync_all_mutables_coming_soon_release()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 0 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20100101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_SoftSync_ProductCatagory_record_tied_to_empty_slot_cleared_on_sync()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "EMPTY" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 0 )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_SoftSync_ProductCatagory_record_tied_to_different_barcode_slot_adjusted_on_sync()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 2, status = "Known", total_rental_count = 222 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }
	MasterProductCatalog[2] = { product_id = 2, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[2], 1 )
	assert_equals(ProductToBarcode[2]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 1 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_SoftSync_two_ProductCatagory_records_switch_slots_readjusts_cache_data()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 2, status = "Known", total_rental_count = 222 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }
	MasterProductCatalog[2] = { product_id = 2, release_date = "20080101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1)
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 2 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 1 )
	assert_equals(ProductToBarcode[2]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 1 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
--~ 	assert_table_count(ProductToBarcode[1], 1 )
--~ 	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
--~ 	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 1 )
	assert_equals(ProductToBarcode[2]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 1 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, false )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
--~ 	assert_table_count(ProductToBarcode[2], 1 )
--~ 	assert_equals(ProductToBarcode[2]["000000000002"].status, "Known" )
--~ 	assert_equals(ProductToBarcode[2]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 0 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_SoftSync_four_ProductCatagory_records_switch_slots_on_two_readjusts_cache_data()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 111 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 222 }
	BarcodeToProduct["000000000003"] = { product_id = 2, status = "Known", total_rental_count = 333 }
	BarcodeToProduct["000000000004"] = { product_id = 2, status = "Known", total_rental_count = 444 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101" }
	MasterProductCatalog[2] = { product_id = 2, release_date = "20080101" }

	-- mutable
	-- speed test since adding logic to run thru the positions and update locations for hand moved discs
	BarcodePosition = newluaunitmockworktable()
	BarcodePosition["1@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["1@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["2@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["3@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["4@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["5@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["6@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@1" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@2" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@3" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@4" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@5" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@6" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@7" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@8" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@9" ] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@10"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@11"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@12"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@13"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@14"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@15"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@16"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@17"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@18"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@19"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@20"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@21"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@22"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@23"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@24"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@25"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@26"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@27"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@28"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@29"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@30"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@31"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@32"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@33"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@34"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@35"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@36"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@37"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@38"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@39"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@40"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@41"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@42"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@43"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@44"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@45"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@46"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@47"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@48"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@49"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@50"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@51"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@52"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@53"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@54"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@55"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@56"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@57"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@58"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@59"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@60"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@61"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@62"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@63"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@64"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@65"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@66"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@67"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@68"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@69"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@70"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@71"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@72"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@73"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@74"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@75"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@76"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@77"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@78"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@79"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@80"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@81"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@82"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@83"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@84"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@85"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@86"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@87"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@88"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@89"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }
	BarcodePosition["7@90"] = { barcode = "UNKNONWN", status = "Unknown Title", total_rental_count = 0 }

	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1)
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductToBarcode:count(), 1 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 2 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 1 )
	assert_equals(ProductToBarcode[2]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000003"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 1 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )

	job_result = { deck = 1, slot = 4, code = "SoftSync", item_id = "000000000004" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "000000000004" )
	assert_equals(BarcodePosition["1@4"].status, "Known" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 2 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 2 )
	assert_equals(ProductToBarcode[2]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000003"].total_rental_count, nil )
	assert_equals(ProductToBarcode[2]["000000000004"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000004"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 2 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )

	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "000000000004" )
	assert_equals(BarcodePosition["1@4"].status, "Known" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 2 )
	assert_table_count(ProductToBarcode[1], 1)
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 2 )
	assert_equals(ProductToBarcode[2]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000003"].total_rental_count, nil )
	assert_equals(ProductToBarcode[2]["000000000004"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000004"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 2 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )

	local job_result = { deck = 1, slot = 3, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 630 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@4"].barcode, "000000000004" )
	assert_equals(BarcodePosition["1@4"].status, "Known" )
	assert_equals(BarcodePosition["1@4"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 2 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_equals(ProductCatalog[2].product_id, 2 )
	assert_equals(ProductCatalog[2].in_stock, true )
	assert_equals(ProductCatalog[2].can_be_sold, false )
	assert_equals(ProductCatalog[2].is_available, true )
	assert_equals(ProductToBarcode:count(), 2 )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_table_count(ProductToBarcode[2], 1 )
--~ 	assert_equals(ProductToBarcode[2]["000000000003"].status, "Known" )
--~ 	assert_equals(ProductToBarcode[2]["000000000003"].total_rental_count, nil )
	assert_equals(ProductToBarcode[2]["000000000004"].status, "Known" )
	assert_equals(ProductToBarcode[2]["000000000004"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 2 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
	assert_equals(ProductHistory[1].last_date_out_of_stock, nil )
	assert_equals(ProductHistory[2].product_id, 2 )
	assert_equals(ProductHistory[2].created_date, "20091111" )
	assert_equals(ProductHistory[2].ofr_count, 0 )
	assert_equals(ProductHistory[2].on_hand_count, 1 )
	assert_equals(ProductHistory[2].last_max_ofr, 1 )
	assert_equals(ProductHistory[2].current_max_ofr, 0 )
	assert_equals(ProductHistory[2].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[2].sold_out, nil )
	assert_equals(ProductHistory[2].last_date_out_of_stock, nil )
end

function TestSyncInventory:test_Fogbugz_2096829_rent_two_wait_seven_days_softsync_sold_out_flag_not_reset_until_product_comes_back()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 0 }
	BarcodeToProduct["000000000002"] = { product_id = 1, status = "Known", total_rental_count = 0 }
	BarcodeToProduct["000000000003"] = { product_id = 1, status = "Known", total_rental_count = 0 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20100101" }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	-- first disc in inventory
	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	-- second disc in inventory
	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_table_count(ProductToBarcode[1], 2 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 2 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	-- rent first disc
	function get_current_date_string() return "20091112" end
	job_result = { deck = 1, slot = 1, code = "ItemVended", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "000000000002" )
	assert_equals(BarcodePosition["1@2"].status, "Known" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000002"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000002"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 1 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	-- rent second disc
	job_result = { deck = 1, slot = 2, code = "ItemVended", item_id = "000000000002" }
	kernel:getelapseddays_data( { start_date_string = "20091112", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_is_nil(ProductToBarcode[1] )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091112" )

	-- softsync six days out
	function get_current_date_string() return "20091120" end
	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "EMPTY" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_is_nil(ProductToBarcode[1] )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091112" )

	job_result = { deck = 1, slot = 2, code = "SoftSync", item_id = "EMPTY" }
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 2 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, false )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_is_nil(ProductToBarcode[1] )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 2 )
	assert_equals(ProductHistory[1].on_hand_count, 0 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, "20091112" )

	-- return third value
	function get_current_date_string() return "20091121" end
	job_result = { deck = 1, slot = 3, code = "ItemReturned", item_id = "000000000003" }
	kernel:getelapseddays_data( { start_date_string = "20091112", end_date_string = "20091121", response = 9 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 3 )
	assert_equals(BarcodePosition["1@1"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@1"].status, "Unknown" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@2"].barcode, "EMPTY" )
	assert_equals(BarcodePosition["1@2"].status, "Unknown" )
	assert_equals(BarcodePosition["1@2"].total_rental_count, nil )
	assert_equals(BarcodePosition["1@3"].barcode, "000000000003" )
	assert_equals(BarcodePosition["1@3"].status, "Known" )
	assert_equals(BarcodePosition["1@3"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, false )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000003"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000003"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 1 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 2 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

function TestSyncInventory:test_Fogbugz_2096842_can_be_sold_goes_to_true_on_softsync_after_seven_days()
	-- immutable
	BarcodeToProduct = newluaunitmockworktable()
	BarcodeToProduct["000000000001"] = { product_id = 1, status = "Known", total_rental_count = 0 }
	MasterProductCatalog = newluaunitmockworktable()
	MasterProductCatalog[1] = { product_id = 1, release_date = "20080101", sell_thru = true, product_type_id = 1 }

	-- mutable
	BarcodePosition = newluaunitmockworktable()
	ProductCatalog = newluaunitmockworktable()
	ProductToBarcode = newluaunitmockworktable()
	ProductHistory = newluaunitmockworktable()

	-- disc in inventory
	local job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091111", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091111", response = 0 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	-- soft sync after one day
	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	function get_current_date_string() return "20091112" end
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 0 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091112", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091112", response = 1 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, true )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091111" )
	assert_equals(ProductHistory[1].sold_out, nil )

	-- soft sync after eight days
	job_result = { deck = 1, slot = 1, code = "SoftSync", item_id = "000000000001" }
	function get_current_date_string() return "20091120" end
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091120", response = 8 } )
	kernel:getelapseddays_data( { start_date_string = "20080101", end_date_string = "20091120", response = 365 } )
	kernel:getelapseddays_data( { start_date_string = "20091111", end_date_string = "20091120", response = 8 } )
	sync_inventory( job_result )

	assert_equals(BarcodePosition:count(), 1 )
	assert_equals(BarcodePosition["1@1"].barcode, "000000000001" )
	assert_equals(BarcodePosition["1@1"].status, "Known" )
	assert_equals(BarcodePosition["1@1"].total_rental_count, nil )
	assert_equals(ProductCatalog:count(), 1 )
	assert_equals(ProductCatalog[1].product_id, 1 )
	assert_equals(ProductCatalog[1].in_stock, true )
	assert_equals(ProductCatalog[1].can_be_sold, false )
	assert_equals(ProductCatalog[1].is_available, true )
	assert_table_count(ProductToBarcode[1], 1 )
	assert_equals(ProductToBarcode[1]["000000000001"].status, "Known" )
	assert_equals(ProductToBarcode[1]["000000000001"].total_rental_count, nil )
	assert_equals(ProductHistory:count(), 1 )
	assert_equals(ProductHistory[1].product_id, 1 )
	assert_equals(ProductHistory[1].created_date, "20091111" )
	assert_equals(ProductHistory[1].ofr_count, 0 )
	assert_equals(ProductHistory[1].on_hand_count, 1 )
	assert_equals(ProductHistory[1].last_max_ofr, 1 )
	assert_equals(ProductHistory[1].current_max_ofr, 0 )
	assert_equals(ProductHistory[1].last_max_ofr_date, "20091120" )
	assert_equals(ProductHistory[1].sold_out, nil )
end

LuaUnit:run()

