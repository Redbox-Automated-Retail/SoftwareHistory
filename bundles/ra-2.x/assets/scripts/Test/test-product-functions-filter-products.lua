--
-- test-product-functions-filter-products.lua
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

TestProductFunctionsFilterProducts = {}

function TestProductFunctionsFilterProducts:setUp()
	kernel = newluaunitmockkernel()
end

function TestProductFunctionsFilterProducts:tearDown()
--~ 	ProductHistory = nil
	kernel = nil
end

function TestProductFunctionsFilterProducts:test_empty_product_catalog_returns_empty_table()
	kernel:datastoregetkeys_data( { store = "cache", name = "ProductCatalog", response = {} } )
	kernel:log_data( { message = "loading cache keys from database for table ProductCatalog" } )
	kernel:log_data( { message = "table ProductCatalog has #cache.keys = 0" } )
	assert_table_count( ProductCatalog:filter_products(), 0 )
end

LuaUnit:run()

