--
-- test-datetime.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )

dofile( "../lib-datetime.lua" )


-- Do not test functions which are basically wrappers for Kernel functions such as __add, __sub, __lt, __rt
-- Kiosk Engine should have its own unit tests which validate that those functions work on the C# side

TestDateTime = {}

function TestDateTime:setUp()
	DateTimeObject = newdatetime()
end

function TestDateTime:tearDown()
	DateTimeObject = nil
end

function TestDateTime:test_init_defaults()
	print( tostring( DateTimeObject ))
	assert_true( tostring( DateTimeObject ) ~= nil )
	print( ">  __tostring() TESTED" )
	
	assert_true( DateTimeObject.backing ~= nil )
	print( ">  .backing not null TESTED" )
	
	-- The default date assigned upon init() should be current OS time rounded up to nearest 5 minutes
	-- Don't try to compare too low, the rounding and rollovers will cause the test to fail
	local compTime = os.date("*t")
	local result = 
		DateTimeObject.backing.day == compTime.day
		and DateTimeObject.backing.month == compTime.month
		and DateTimeObject.backing.year == compTime.year
	assert_true( result )
	print( ">  default date = os.date(*t) TESTED" )
end

function TestDateTime:test_cron_expression()
	print( DateTimeObject:to_cron_expression() )
	local cron_expression =  "0 " .. 
			tostring( DateTimeObject.backing.min ) .. " "  ..
			tostring( DateTimeObject.backing.hour ) .. " " ..
			tostring( DateTimeObject.backing.day ) .. " " ..
			tostring( DateTimeObject.backing.month ) .. " ? " ..
			tostring( DateTimeObject.backing.year )
	assert_true( cron_expression == DateTimeObject:to_cron_expression() )
	print( ">  :to_cron_expression() TESTED" )
end

function TestDateTime:test_set_from_cron_expression()
	local cron1 = "0 30 18 23 7 ? 2010"
	DateTimeObject:set_from_cron_expression( cron1 )
	assert_true( DateTimeObject.backing.year == 2010 )
	assert_true( DateTimeObject.backing.month == 7 )
	assert_true( DateTimeObject.backing.day == 23 )
	assert_true( DateTimeObject.backing.hour == 18 )
	assert_true( DateTimeObject.backing.min == 30 )
	local cron2 = "0 4 3 1 12 ? 1987"
	DateTimeObject:set_from_cron_expression( cron2 )
	assert_true( DateTimeObject.backing.year == 1987 )
	assert_true( DateTimeObject.backing.month == 12 )
	assert_true( DateTimeObject.backing.day == 1 )
	assert_true( DateTimeObject.backing.hour == 3 )
	assert_true( DateTimeObject.backing.min == 4 )
	print( ">  :set_from_cron_expression() TESTED" )
end

function TestDateTime:test_is_within_minutes()
	-- The class is written to only test comp times in the future, not the past
	-- as the application does not allow setting dates into the past for scheduled jobs
	print( "> force minutes to 15 for next test" )
	DateTimeObject.backing.min = 15
	local compTime = newdatetime()
	compTime.backing.min =  17
	assert_true( DateTimeObject:is_within_minutes( compTime, 5 ) )
	print( "> :is_within_minutes() TESTED positive case A" )
	compTime.backing.min =  8
	assert_true( DateTimeObject:is_within_minutes( compTime, 10 ) )
	print( "> :is_within_minutes() TESTED positive case B" )
	compTime.backing.min =  59
	assert_true( DateTimeObject:is_within_minutes( compTime, 100 ) )
	print( "> :is_within_minutes() TESTED positive case C" )
	compTime.backing.min =  3
	assert_false( DateTimeObject:is_within_minutes( compTime, 5 ) )
	print( "> :is_within_minutes() TESTED negative case A" )
	compTime.backing.min =  38
	assert_false( DateTimeObject:is_within_minutes( compTime, 10 ) )
	print( "> :is_within_minutes() TESTED negative case B" )
	compTime.backing.min =  13
	assert_false( DateTimeObject:is_within_minutes( compTime, 1 ) )
	print( "> :is_within_minutes() TESTED negative case C" )
	
	print( "> force 1 hour difference for next test" )
	DateTimeObject.backing.hour = 8
	DateTimeObject.backing.min = 55
	compTime.backing.hour = 9
	compTime.backing.min =  5
	assert_true( DateTimeObject:is_within_minutes( compTime, 10 ) )
	print( "> :is_within_minutes() TESTED hour rollover positive" )
	compTime.backing.min =  15
	assert_false( DateTimeObject:is_within_minutes( compTime, 10 ) )
	print( "> :is_within_minutes() TESTED hour rollover negative" )

end


LuaUnit:run()