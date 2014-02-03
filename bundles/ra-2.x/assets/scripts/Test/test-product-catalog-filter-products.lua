--
-- test-product-catalog-filter-products.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockkernel" )

dofile( "../data-store-functions.lua" )
dofile( "../inventory-functions.lua" )

strings = {}
strings.esrb_earlychildhood = nil
strings.esrb_everyone = nil
strings.esrb_everyone_10_plus = nil
strings.esrb_teen = nil
strings.esrb_mature = nil
strings.esrb_ratings_pending = nil
strings.esrb_adults_only = nil

dofile( "../product-functions.lua" )

TestProductCatalogFilterProducts = {}

function TestProductCatalogFilterProducts:setUp()
	kernel = newluaunitmockkernel()
end

function TestProductCatalogFilterProducts:tearDown()
	kernel = nil
end

function TestProductCatalogFilterProducts:test_()
	-- TODO: finish out this test
	kernel:datastoregetkeys_data( { store = "cache", name = "ProductCatalog", response = {} } )
	kernel:log_data( { message = "loading cache keys from database for table ProductCatalog" } )
	kernel:log_data( { message = "table ProductCatalog has #cache.keys = 0" } )
	ProductCatalog:filter_products()
end

LuaUnit:run()

