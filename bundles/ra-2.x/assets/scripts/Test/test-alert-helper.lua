--
-- test-alert-helper.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )

kernel = {}
function kernel:log( value ) end -- eat the call

StoreManager = {}
function StoreManager:get_number() return 5916 end

dofile( "../alert-helper.lua" )

TestAlertHelper = {}

    function TestAlertHelper:setUp()
    end

    function TestAlertHelper:tearDown()
    end

    function TestAlertHelper:test_001_format_jobresult_nil_jobresult()
		try(
			function()
				AlertManager.format_jobresult( nil )
			end,
			function(e)
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Jobresult value: nil is not valid!" )
			end
		)
	end

    function TestAlertHelper:test_002_format_jobresult_nil_jobresult_code()
		local jobresult = {}
		try(
			function()
				AlertManager.format_jobresult( jobresult )
			end,
			function(e)
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Jobresult code value: nil is not valid!" )
			end
		)
	end

	function TestAlertHelper:test_003_format_errors_nil_jobid_raises_error()
		try(
			function()
				AlertManager.format_errors( nil )
			end,
			function(e)
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Jobid value: nil is not valid!" )
			end
		)
	end

    function TestAlertHelper:test_004_send_alert_class_nil_raises_error()
		try(
			function()
				AlertManager.send_alert( nil, nil, nil, nil, nil )
				assert_fail( "o.send_alert should error..." )
			end,
            function( e )
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Class value: nil is not valid!" )
            end
		)
	end

    function TestAlertHelper:test_005_send_alert_source_nil_raises_error()
		try(
			function()
				AlertManager.send_alert( "testclass", nil, nil, nil, nil )
				assert_fail( "o.send_alert should error..." )
			end,
            function( e )
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Source value: nil is not valid!" )
            end
		)
	end

    function TestAlertHelper:test_006_send_alert_description_nil_raises_error()
		try(
			function()
				AlertManager.send_alert( "testclass", "testsource", nil, nil, nil )
				assert_fail( "o.send_alert should error..." )
			end,
            function( e )
				assert_pattern_matches( e, "../alert-helper.lua:%d%d: Description value: nil is not valid!" )
            end
		)
	end

    function TestAlertHelper:test_007_send_alert_no_jobresults_no_errors_formats_correctly()

		kernel.healthsvcs_sendalert = function( store_number, class, buffer )
			assert_equals( store_number, 5916 )
			assert_equals( class, "testclass" )
			assert_equals( buffer, "<h2>testsource</h2><p>testdescription</p>" )
		end

		AlertManager.send_alert( "testclass", "testsource", "testdescription", nil, nil )
	end

    function TestAlertHelper:test_008_send_alert_with_jobresults_formats_correctly()

		kernel.healthsvcs_sendalert = function( store_number, class, buffer )
			assert_equals( store_number, 5916 )
			assert_equals( class, "testclass" )
			assert_equals( buffer, "<h2>testsource</h2><p>testdescription</p><p>Code: testcode, Deck: testdeck, Slot: testslot, Item ID: testitemid, Message: testmessage</p>" )
		end

		AlertManager.send_alert(
			"testclass",
			"testsource",
			"testdescription",
			AlertManager.format_jobresult(
				{	["code"] = "testcode",
					["deck"] = "testdeck",
					["slot"] = "testslot",
					["item_id"] = "testitemid",
					["message"] = "testmessage"
				} ),
			nil )
	end

    function TestAlertHelper:test_009_send_alert_with_errors_formats_correctly()
		kernel.hardwaregetjoberrors = function( jobid )
			assert_equals( jobid, "ABC" )
			return {
				["results"] = {
					{ ["code"] = "testcode1", ["description"] = "testdescription1", ["details"] = "testdetails1" },
					{ ["code"] = "testcode2", ["description"] = "testdescription2", ["details"] = "testdetails2" },
				}
			}
		end

		kernel.healthsvcs_sendalert = function( store_number, class, buffer )
			assert_equals( store_number, 5916 )
			assert_equals( class, "testclass" )
			assert_equals( buffer, "<h2>testsource</h2><p>testdescription</p><p>The job ended in an Errored state; errors follow:</p><p>testcode1 testdescription1</br>Details: testdetails1</p><p>testcode2 testdescription2</br>Details: testdetails2</p>" )
		end

		AlertManager.send_alert(
			"testclass",
			"testsource",
			"testdescription",
			nil,
			AlertManager.format_errors( "ABC" ) )
	end

    function TestAlertHelper:test_010_send_alert_with_jobresults_and_errors_formats_correctly()
		kernel.hardwaregetjoberrors = function( jobid )
			assert_equals( jobid, "ABC" )
			return {
				["results"] = {
					{ ["code"] = "testcode1", ["description"] = "testdescription1", ["details"] = "testdetails1" },
					{ ["code"] = "testcode2", ["description"] = "testdescription2", ["details"] = "testdetails2" },
				}
			}
		end

		kernel.healthsvcs_sendalert = function( store_number, class, buffer )
			assert_equals( store_number, 5916 )
			assert_equals( class, "testclass" )
			assert_equals( buffer, "<h2>testsource</h2><p>testdescription</p><p>Code: testcode, Deck: testdeck, Slot: testslot, Item ID: testitemid, Message: testmessage</p><p>The job ended in an Errored state; errors follow:</p><p>testcode1 testdescription1</br>Details: testdetails1</p><p>testcode2 testdescription2</br>Details: testdetails2</p>" )
		end

		AlertManager.send_alert(
			"testclass",
			"testsource",
			"testdescription",
			AlertManager.format_jobresult(
				{	["code"] = "testcode",
					["deck"] = "testdeck",
					["slot"] = "testslot",
					["item_id"] = "testitemid",
					["message"] = "testmessage"
				}),
			AlertManager.format_errors( "ABC" ) )
	end

    function TestAlertHelper:test_011_send_job_alert_formats_correctly()
		kernel.hardwaregetprogramresults = function( jobid )
			assert_equals( jobid, "ABC" )
			return {
				["results"] = {
					{ ["code"] = "testcode", ["deck"] = "testdeck", ["slot"] = "testslot", ["item_id"] = "testitemid", ["message"] = "testmessage" }
				}
			}
		end

		kernel.hardwaregetjoberrors = function( jobid )
			assert_equals( jobid, "ABC" )
			return {
				["results"] = {
					{ ["code"] = "testcode1", ["description"] = "testdescription1", ["details"] = "testdetails1" },
					{ ["code"] = "testcode2", ["description"] = "testdescription2", ["details"] = "testdetails2" },
				}
			}
		end

		kernel.healthsvcs_sendalert = function( store_number, class, buffer )
			assert_equals( store_number, 5916 )
			assert_equals( class, "testclass" )
			assert_equals( buffer, "<h2>testsource</h2><p>testdescription</p><p>Code: testcode, Deck: testdeck, Slot: testslot, Item ID: testitemid, Message: testmessage</p><p>The job ended in an Errored state; errors follow:</p><p>testcode1 testdescription1</br>Details: testdetails1</p><p>testcode2 testdescription2</br>Details: testdetails2</p>" )
		end

		AlertManager.send_job_alert(
			"testclass",
			"testsource",
			"testdescription",
			"ABC" )
	end

LuaUnit:run()
