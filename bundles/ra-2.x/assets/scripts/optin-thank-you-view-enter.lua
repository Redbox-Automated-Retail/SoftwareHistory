--
-- optin-thank-you-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

optin_thank_you_timer = newidletimer("optin_thank_you_timer")
optin_thank_you_timer:initialize(
	8000,
	function()
		-- Display the appropriate additional opt-in, vend, Coming Soon, or VSM screen
		continue_vend()
	end )
optin_thank_you_timer:start()
