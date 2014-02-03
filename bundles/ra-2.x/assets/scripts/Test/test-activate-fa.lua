--
-- test-activate-fa.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

TestActiviteFA = {}

    function TestActiviteFA:setUp()
    end

    function TestActiviteFA:tearDown()
    end

    function TestActiviteFA:test_001_verify_callback()
		--preconditions
		kernel = {}
		kernel.activatebundle = function( name, version )
			assert_equals( name, "Field Maintenance Application" )
			assert_equals( version, "*" )
		end

		dofile("../activate-fa.lua")
	end

LuaUnit:run()
