--
-- test-obj-email-validation.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

require( "luaunit" )
require( "luaunit_extensions" )

dofile( "../obj-email-validation.lua" )


TestObjEmailValidation = {}

function TestObjEmailValidation:setUp()
	EmailValidator = newemailvalidator()
end

function TestObjEmailValidation:tearDown()
	EmailValidator = nil
end


--Domain Rules:

function TestObjEmailValidation:test_valid_domains()
	local isValid = EmailValidator.validate_email( "test@AB_123.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "test@123_ABC.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "test@a1b_1c_3b.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "joebob@q.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_valid_sub_domains()
	local isValid = EmailValidator.validate_email( "test@AB_123.oakley.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "test@123_ABC.genie_score.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "test@jam.a1b_1c_3b.nowhere.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@QA.ADMIN.CHEETAH.com" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "test_123@admin.cheetah.com" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@ADMIN.CHEETAH.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_invalid_domains()
	local isValid = EmailValidator.validate_email( "test@_1Abcd23.COM" )
	assert_false( isValid )
	isValid = EmailValidator.validate_email( "test@_1cd23-.COM" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_invalid_sub_domains()
	local isValid = EmailValidator.validate_email( "test@_bob.1Abcd23.COM" )
	assert_false( isValid )
	isValid = EmailValidator.validate_email( "test@bob._1Abcd23.COM" )
	assert_false( isValid )
	isValid = EmailValidator.validate_email( "test@bob.1Abcd23_.COM" )
	assert_false( isValid )
	isValid = EmailValidator.validate_email( "test@bob_.1Abcd23.COM" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_valid_email()
	
	local isValid = EmailValidator.validate_email( "TEST@GMAIL.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "tEsT@Gmail.com" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "lower.case@gmail.com" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "lower.case_weSawIt@raggmail.com" )
	assert_true( isValid )
	
end

function TestObjEmailValidation:test_one_at()
	local isValid = EmailValidator.validate_email( "TEST123@GMAIL.COM" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_multiple_at()
	local isValid = EmailValidator.validate_email( "TEST@123@GMAIL.COM" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_contiguous_periods_handle()
	local isValid = EmailValidator.validate_email( "TEST..TEST@GMAIL.COM" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_contiguous_periods_domain()
	local isValid = EmailValidator.validate_email( "TEST.TEST@GMAIL..COM" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_No_contiguous_periods()
	local isValid = EmailValidator.validate_email( "TEST.123@GMAIL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_periods_as_first_character()
	local isValid = EmailValidator.validate_email( ".TEST@GMAIL.com." )
	assert_false( isValid )
end

--AOL Rules:

function TestObjEmailValidation:test_AOL_first_char_letter()
	local isValid = EmailValidator.validate_email( "TEST@AOL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_AOL_first_char_number()
	local isValid = EmailValidator.validate_email( "123TEST@AOL.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_AOL_min_length_3_chars()
	local isValid = EmailValidator.validate_email( "TE@AOL.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_AOL_max_length_16_chars()
	local isValid = EmailValidator.validate_email( "ABCDEFGHIJKLMNOPQR@AOL.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_AOL_contain_special_chars()
	local isValid = EmailValidator.validate_email( "JEO.TEST_123@AOL.com" )
	assert_false( isValid )
end

--YAHOO Rules:

function TestObjEmailValidation:test_Yahoo_first_char_letter()
	local isValid = EmailValidator.validate_email( "TEST@YAHOO.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_Yahoo_first_char_number()
	local isValid = EmailValidator.validate_email( "123TEST@YAHOO.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_Yahoo_first_special_chars()
	local isValid = EmailValidator.validate_email( "-TEST.123@YAHOO.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_Yahoo_valid_speacial_chars()
	local isValid = EmailValidator.validate_email( "QC-TEST_12.3@YAHOO.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_Yahoo_invalid_speacial_chars()
	local isValid = EmailValidator.validate_email( "TEST*123@YAHOO.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_YAHOO_max_length_32_chars()
	local isValid = EmailValidator.validate_email( "ABCDEFGHIJKLMNOPQR_ABCDEFGHIJKLMNOPQR@YAHOO.com" )
	assert_false( isValid )
end

--HOTMAIL Rules:

function TestObjEmailValidation:test_HOTMAIL_first_char_letter()
	local isValid = EmailValidator.validate_email( "TEST@HOTMAIL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_HOTMAIL_first_char_number()
	local isValid = EmailValidator.validate_email( "123TEST@HOTMAIL.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_HOTMAIL_first_special_chars()
	local isValid = EmailValidator.validate_email( "-TEST.123@HOTMAIL.com" )
	assert_false( isValid )
end

function TestObjEmailValidation:test_HOTMAIL_valid_speacial_chars()
	local isValid = EmailValidator.validate_email( "QC-TEST_12.3@HOTMAIL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_HOTMAIL_invalid_speacial_chars()
	local isValid = EmailValidator.validate_email( "TEST*123@HOTMAIL.com" )
	assert_false( isValid )
end

--MSN Rules:

function TestObjEmailValidation:test_MSN_valid_email()
	local isValid = EmailValidator.validate_email( "_TEST.123@MSN.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_MSN_valid_speacial_chars()
	local isValid = EmailValidator.validate_email( "QC-TEST_12.3@MSN.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_MSN_invalid_speacial_chars()
	local isValid = EmailValidator.validate_email( "TEST*123@MSN.com" )
	assert_false( isValid )
end

--GMAIL Rules:

function TestObjEmailValidation:test_GMAIL_valid_email()
	local isValid = EmailValidator.validate_email( "_TEST.123@GMAIL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_GMAIL_valid_speacial_chars()
	local isValid = EmailValidator.validate_email( "QC-TEST_12.3@GMAIL.com" )
	assert_true( isValid )
end

function TestObjEmailValidation:test_GMAIL_invalid_speacial_chars()
	local isValid = EmailValidator.validate_email( "TEST*123@GMAIL.com" )
	assert_false( isValid )
end

-- top Level Domains { COM, EDU, GOV, INFO, INT, JOBS, MIL, NET, ORG }


function TestObjEmailValidation:test_valid_TLDs()
	local isValid = EmailValidator.validate_email( "TEST_123@ADMIN.GMAIL.COM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.EDU" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.GOV" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.INFO" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.INT" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@QA.JOBS" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST123@CHEETAHMAIL.MIL" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "ABC_123@CHEETAHMAIL.NET" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@CHEETAHMAIL.ORG" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@gov.us" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@YOURTEST.biz" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@YOURTEST.MX" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@YOUR.TEST.CC" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@alexander.mobi" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_QA@joe.coop" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST.PERSON@JoLiEt.ca" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.PH" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.AERO" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.INT" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.MUSEUM" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.TRAVEL" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.TEL" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@CHEETAHMAIL.PRO" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@somoa.ws" )
	assert_true( isValid )
	isValid = EmailValidator.validate_email( "TEST_123@fastmail.fm" )
	assert_true( isValid )
end


--Domain other than the above given set

function TestObjEmailValidation:test_valid_domain_ORG()
	local isValid = EmailValidator.validate_email( "TEST_QA@CHEETAHMAIL.IN" )
	assert_false( isValid )
end

LuaUnit:run()
