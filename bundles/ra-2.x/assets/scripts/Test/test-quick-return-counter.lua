--
-- test-quick-return-counter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )
require( "luaunitmockworktable" )
require( "luaunitmockkernel" )

function getworktable( store, name, read_only )
	return newluaunitmockworktable()
end

function get_current_date_string( mask )
	return "200912091634"
end

TestQuickReturnCounter = {}

function TestQuickReturnCounter:setUp()
	kernel = newluaunitmockkernel()
	QuickReturnCounter = nil
end

function TestQuickReturnCounter:tearDown()
	QuickReturnCounter = nil
	kernel = nil
end

function TestQuickReturnCounter:test_quick_return_count_initial_values()
	dofile( "../quick-return-counter.lua" )
	assert_is_not_nil( QuickReturnCounter )
	assert_equals( QuickReturnCounter:value(), 0 )
end

function TestQuickReturnCounter:test_quick_return_count_increment()
	dofile( "../quick-return-counter.lua" )
	assert_equals( QuickReturnCounter:value(), 0 )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 1 )
end

function TestQuickReturnCounter:test_quick_return_count_increment_and_reset()
	dofile( "../quick-return-counter.lua" )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 1 )
	QuickReturnCounter:reset()
	assert_equals( QuickReturnCounter:value(), 0 )
end

function TestQuickReturnCounter:test_quick_return_count_increment_x_3_reset()
	dofile( "../quick-return-counter.lua" )
	assert_equals( QuickReturnCounter:value(), 0 )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 1 )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 2 )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 3 )
	QuickReturnCounter:reset()
	assert_equals( QuickReturnCounter:value(), 0 )
end

function TestQuickReturnCounter:test_quick_return_increment_submit()
	dofile( "../quick-return-counter.lua" )
	assert_equals( QuickReturnCounter:value(), 0 )
	QuickReturnCounter:increment()
	assert_equals( QuickReturnCounter:value(), 1 )
	kernel:healthsvcs_enqueuestatistic_data( { name = "quick_returns_as_of_200912091634", value = 1 } )
	QuickReturnCounter:submit()
	assert_equals( QuickReturnCounter:value(), 0 )
end

LuaUnit:run()
