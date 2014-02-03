--
-- test-product-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )

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


kernel = {}
kernel.log = function( value ) end -- eat the call

kernel.ProductFamiliesData = { ["1"] = "{ product_family_name = 'Movies', product_family_id = 1}" }

kernel.datastoreget = function( data_store_name, table_name, key )
	if data_store_name == "profile" then
		if table_name == "ProductFamily" then
			ProductFamilies.cache = {}
			local result = kernel.ProductFamiliesData[key]
			ProductFamilies.cache = {}
			return result
		end
	end
end
kernel.datastoregetkeys = function( data_store_name, table_name, key ) return {} end -- stub out cache function

TestProductFunctions = {}

    function TestProductFunctions:setUp()
    end

    function TestProductFunctions:tearDown()
    end

--~     function TestProductFunctions:test_new_selection_tracker()
--~         local o = ProductFamilies:new_selection_tracker()
--~         assert_is_not_nil( o )

--~ 	end

	function TestProductFunctions:test_get_family()
        local o = ProductFamilies:get_family(1)
        assert_is_not_nil( o )
	end

LuaUnit:run()
