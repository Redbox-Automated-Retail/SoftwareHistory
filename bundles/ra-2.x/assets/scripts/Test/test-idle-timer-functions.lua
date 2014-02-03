--
-- test-shopping-cart-genre-categories.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )

--kernel = {}
--function kernel:log( value ) end -- eat the call

dofile( "../idle-timer-functions.lua" )

TestIdleTimerFunctions = {}

function TestIdleTimerFunctions:setUp()
end

function TestIdleTimerFunctions:tearDown()
end

function TestIdleTimerFunctions:test_001_construction_defaults()
	local o = newidletimer()
    assert_is_not_nil( o )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_002_initialize_nil_timeout()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 15000 )
		assert_equals( period, 15000 )
		assert_is_not_nil( handler )
	end

	local o = newidletimer()
	o:initialize( nil, nil )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "way_cool_funky_timer" )
		assert_equals( duetime, 15000 )
		assert_equals( period, 15000 )
		assert_is_not_nil( handler )
	end

	local o = newidletimer( "way_cool_funky_timer" )
	o:initialize( nil, nil )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_003_initialize_specified_timeout()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 3000 )
		assert_equals( period, 3000 )
		assert_is_not_nil( handler )
	end

	local o = newidletimer()
	o:initialize( 3000, nil )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

--TODO: shouldn't a nil handler passed into the timer be flagged as an error?
function TestIdleTimerFunctions:test_004_initialize_nil_handler()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:initialize( 3000, nil )
 			assert_fail("o:initialze( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: Callback should not be nil!" )
 		end
	)
 	assert_false( o.is_enabled )
 	assert_false( o.is_registered )
 	assert_false( o.is_suspended )
 	assert_false( o.is_timer_started )
end

--TODO: shouldn't start error if initialize hasn't been called?
function TestIdleTimerFunctions:test_005_start_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:start( )
 			assert_fail( "o:start( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before start()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_006_start_calls_correct_subfunctions()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_007_start_call_short_circuits_on_subsiquent_calls()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = nil
	kernel.registerclicknotify = nil
	kernel.registerkeypressnotify = nil
	kernel.registeridlenotify = nil

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

end

--TODO: shouldn't stop error if initialize hasn't been called?
function TestIdleTimerFunctions:test_008_stop_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:stop( )
 			assert_fail( "o:stop( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before stop()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_009_stop_called_without_start_short_circuits()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )

	o:stop( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_010_stop_called_without_start_short_circuits()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )

	o:stop( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_011_start_calls_correct_subfunctions()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' stopped." )
	end
	kernel.removeclicknotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removekeypressnotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removeidlenotify = function( name )
		assert_equals( name, "idle_timer" )
	end

	o:stop( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

end

--TODO: shouldn't start error if initialize hasn't been called?
function TestIdleTimerFunctions:test_012_suspend_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:suspend( )
 			assert_fail( "o:start( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before suspend()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_013_suspend_called_before_start_short_circuits()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	o:suspend( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_014_suspend_call_calls_correct_functions()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' stopped." )
	end
	kernel.removeclicknotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removekeypressnotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removeidlenotify = function( name )
		assert_equals( name, "idle_timer" )
	end

	o:suspend( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_true( o.is_suspended )
	assert_false( o.is_timer_started )
end

--TODO: shouldn't resume error if initialize hasn't been called?
function TestIdleTimerFunctions:test_015_resume_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:resume( )
 			assert_fail( "o:resume( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before resume()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_016_resume_called_before_suspend_short_circuits()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	o:resume( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_017_resume_call_calls_correct_functions()
	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

 	local o = newidletimer()
	o:initialize( 10000, function() end )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:start( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' stopped." )
	end
	kernel.removeclicknotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removekeypressnotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removeidlenotify = function( name )
		assert_equals( name, "idle_timer" )
	end

	o:suspend( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_true( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.removeclicknotify = nil
	kernel.removekeypressnotify = nil
	kernel.removeidlenotify = nil

	o:resume( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

--TODO: shouldn't reset error if initialize hasn't been called?
function TestIdleTimerFunctions:test_018_reset_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:reset( )
 			assert_fail( "o:resume( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before reset()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_019_initialize_reset_calls_correct_functions()
 	local o = newidletimer()

	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

	o:initialize( 10000, function() end )
	kernel.timercreate = nil
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	o:reset( )
	kernel.registeridlenotify = nil
	assert_false( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_020_initialize_start_reset_calls_correct_functions()
 	local o = newidletimer()

	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

	o:initialize( 10000, function() end )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	o:start( )
	kernel = nil

	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	o:reset( )
	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_021_initialize_start_stop_reset_calls_correct_functions()
 	local o = newidletimer()

	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 10000 )
		assert_equals( period, 10000 )
		assert_is_not_nil( handler )
	end

	o:initialize( 10000, function() end )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' started." )
	end
	kernel.registerclicknotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registerkeypressnotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end
	o:start( )
	kernel = nil

	assert_true( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.log = function( message )
		assert_equals( message, "idle timer 'idle_timer' stopped." )
	end
	kernel.removekeypressnotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removeclicknotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	kernel.removeidlenotify = function( name )
		assert_equals( name, "idle_timer" )
	end
	o:stop( )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.registeridlenotify = function( name, handler )
		assert_equals( name, "idle_timer" )
		assert_is_not_nil( handler )
	end

	o:reset( )
	assert_false( o.is_enabled )
	assert_true( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

--TODO: shouldn't start_idle_timer error if initialize hasn't been called?
function TestIdleTimerFunctions:test_022_reset_called_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:start_idle_timer( )
 			assert_fail( "o:idle_start_timer( ) should throw error" )
 		end,
 		function( e )
			assert_equals(
			  string.sub( e, 1, 38 ),
			  "../idle_timer_functions.lua:" )
			assert_equals(
				string.sub( e, 44 ),
				"initialize() should be called before reset()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_023_start_idle_timer_calls_correct_functions()
 	local o = newidletimer()
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.timerstart = function( name )
		assert_equals( name, "idle_timer" )
	end
	o:start_idle_timer( )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_true( o.is_timer_started )
end

function TestIdleTimerFunctions:test_024_start_idle_timer_start_idle_timer_short_circuits()
 	local o = newidletimer()
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.timerstart = function( name )
		assert_equals( name, "idle_timer" )
	end
	o:start_idle_timer( )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_true( o.is_timer_started )

	o:start_idle_timer( )

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_true( o.is_timer_started )
end

--TODO: shouldn't stop_idle_timer error if initialize hasn't been called?
function TestIdleTimerFunctions:test_025_stop_idle_timer_before_initialize_should_error()
	if true then
		local info = debug.getinfo(1, "nfSlu")
		print( ">>>>>>  Skipping function " .. info.name .. " at line: " .. info.linedefined )
		return
	end

 	local o = newidletimer()
 	try(
 		function( )
 			o:stop_idle_timer( )
 			assert_fail( "o:stop_idle_timer( ) should throw error" )
 		end,
 		function( e )
			assert_pattern_matches( e, "../idle_timer_functions.lua:%d%d%d: initialize() should be called before stop_idle_timer()!" )
		end
	)
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_026_stop_idle_timer_short_circuits()
 	local o = newidletimer()
 	o:stop_idle_timer( )
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end

function TestIdleTimerFunctions:test_027_start_idle_timer_stop_idle_timer_calls_correct_functions()
 	local o = newidletimer()

	kernel = {}
	kernel.timercreate = function( name, duetime, period, handler )
		assert_equals( name, "idle_timer" )
		assert_equals( duetime, 15000 )
		assert_equals( period, 15000 )
		assert_is_not_nil( handler )
	end
	o:initialize( )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )

	kernel = {}
	kernel.timerstart = function( name )
		assert_equals( name, "idle_timer" )
	end
 	o:start_idle_timer( )
	kernel = nil

	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_true( o.is_timer_started )

	kernel = {}
	kernel.timerstop = function( name )
		assert_equals( name, "idle_timer" )
	end
 	o:stop_idle_timer( )
	kernel = nil
	assert_false( o.is_enabled )
	assert_false( o.is_registered )
	assert_false( o.is_suspended )
	assert_false( o.is_timer_started )
end
LuaUnit:run()
