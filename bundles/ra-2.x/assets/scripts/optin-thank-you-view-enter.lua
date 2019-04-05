--
-- optin-thank-you-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
if ShoppingCart.vend_manager.vend_job_completed  then
	view_goback_safe("optin_thank_you_view vend job already completed", true)
else
	optin_thank_you_timer = newidletimer("optin_thank_you_timer")
	optin_thank_you_timer:initialize(
		8000,
		function()
			-- Display the appropriate additional opt-in, vend, Coming Soon, or VSM screen
			view_goback_safe( "optin_thank_you_view timer", false )
			continue_vend()
		end )
	optin_thank_you_timer:start()
end
