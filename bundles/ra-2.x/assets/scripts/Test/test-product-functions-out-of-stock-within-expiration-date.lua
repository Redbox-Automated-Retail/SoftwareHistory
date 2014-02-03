--
-- test-product-functions-out-of-stock-within-expiration-date.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

dofile( "../data-store-functions.lua" )
dofile( "../inventory-functions.lua" )
dofile( "../product-functions.lua" )

TestProductFunctionOutOfStockWithinExpirationDate = {}

function TestProductFunctionOutOfStockWithinExpirationDate:setUp()
	kernel = newluaunitmockkernel()
end

function TestProductFunctionOutOfStockWithinExpirationDate:tearDown()
	ProductHistory = nil
	kernel = nil
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_nil_product_id()
	try(
		function()
			ProductCatalog:out_of_stock_within_expiration_date()
			assert_fail( "should not get here" )
		end,
		function( e )
			assert_pattern_matches( e, "../product-functions.lua:%d%d%d: product_id cannot be nil!" )
		end
	)
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_nil_history_returns_false()
	assert_false( ProductCatalog:out_of_stock_within_expiration_date( 1 ) )
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_no_on_hand_count_returns_false()
	assert_false( ProductCatalog:out_of_stock_within_expiration_date( 1, {} ) )
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_no_last_date_out_of_stock_returns_false()
	assert_false( ProductCatalog:out_of_stock_within_expiration_date( 1, { on_hand_count = 0 } ) )
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_last_date_out_of_stock_within_25_days_returns_true()
	get_current_date_string = function() return "20100106" end
	kernel:getelapseddays_data( { start_date_string = "20100104", end_date_string = "20100106", response = 2 } )
	assert_true( ProductCatalog:out_of_stock_within_expiration_date( 1, { on_hand_count = 0, last_date_out_of_stock = "20100104" } ) )
end

function TestProductFunctionOutOfStockWithinExpirationDate:test_last_date_out_of_stock_outside_25_days_returns_false()
	get_current_date_string = function() return "20100206" end
	kernel:getelapseddays_data( { start_date_string = "20100104", end_date_string = "20100206", response = 32 } )
	assert_false( ProductCatalog:out_of_stock_within_expiration_date( 1, { on_hand_count = 0, last_date_out_of_stock = "20100104" } ) )
end

LuaUnit:run()

