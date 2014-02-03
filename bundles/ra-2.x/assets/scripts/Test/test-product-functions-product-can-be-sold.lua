--
-- test-sync-inventory.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

--~ function getworktable() return nil end
function getprofiletable( file, data_table ) return newluaunitmockworktable() end
ProductCatalog = newluaunitmockworktable()

strings = {}
strings.esrb_earlychildhood = nil
strings.esrb_everyone = nil
strings.esrb_everyone_10_plus = nil
strings.esrb_teen = nil
strings.esrb_mature = nil
strings.esrb_ratings_pending = nil
strings.esrb_adults_only = nil

dofile( "../extension-functions.lua" )
dofile( "../product-functions.lua" )

TestProductFunctionsProductCanBeSold = {}

function TestProductFunctionsProductCanBeSold:setUp()
	kernel = newluaunitmockkernel()

	function get_current_date_string() return "20090601" end
end

function TestProductFunctionsProductCanBeSold:tearDown()
	kernel = nil
	ProductFamilies = nil
end

--[[
function TestProductFunctionsProductCanBeSold:test_sellthru_false_returns_false()
	local product = { sell_thru = false }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_only_sell_dvd_product_family()
	local product = { sell_thru = true, product_type_id = 2 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 3 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 4 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 5 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 6 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 7 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 8 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
	local product = { sell_thru = true, product_type_id = 9 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )

	local product = { sell_thru = true, product_type_id = 1 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_nil_release_date_returns_false()
	local product = { sell_thru = true, product_type_id = 1 }
	assert_false( ProductCatalog:product_can_be_sold( product ) )

	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	local product = { sell_thru = true, product_type_id = 1, release_date = "20080601" }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_missing_product_history_record_returns_false()
	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	ProductHistory = newluaunitmockworktable()
	local product = { sell_thru = true, product_type_id = 1, release_date = "20080601" }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_nil_product_history_last_max_ofr_returns_false()
	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { }
	local product = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080601" }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_nonnil_product_history_sold_out_returns_false()
	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { last_max_ofr = 0, sold_out = "20080701" }
	local product = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080601" }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_product_history_on_hand_count_less_than_last_max_ofr_returns_false()
	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { on_hand_count = 2, last_max_ofr = 3  }
	local product = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080601", sold_out = "20080701" }
	assert_false( ProductCatalog:product_can_be_sold( product ) )
end

function TestProductFunctionsProductCanBeSold:test_product_history_on_hand_count_greater_than_last_max_ofr_returns_true()
	kernel:getelapseddays_data( { start_date_string = "20080601", end_date_string = "20090601", response = 365 } )
	ProductHistory = newluaunitmockworktable()
	ProductHistory[1] = { on_hand_count = 3, last_max_ofr = 2 }
	local product = { product_id = 1, sell_thru = true, product_type_id = 1, release_date = "20080601", sold_out = "20080701" }
	assert_true( ProductCatalog:product_can_be_sold( product ) )
end
]]

LuaUnit:run()

