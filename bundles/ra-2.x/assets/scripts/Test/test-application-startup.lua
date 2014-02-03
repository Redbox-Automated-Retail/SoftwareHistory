--
-- test-application-startup.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

dofile( "../idle-timer-functions.lua" )
dofile( "../localization-functions.lua" )

TestApplicationStartup = {}

    function TestApplicationStartup:setUp()
    end

    function TestApplicationStartup:tearDown()
    end

--[[
    function TestApplicationStartup:test_001_verify_callback()
		--preconditions
		kernel = newluaunitmockkernel()
		kernel:storesvcs_shouldreboot_data( {} )
		kernel:registercanshutdown_data( {} )
		kernel:registershowmaintenanceview_data( {} )
		kernel:registerhardwarechange_data( {} )

		dofile("../application-startup.lua")
	end
]]

LuaUnit:run()









