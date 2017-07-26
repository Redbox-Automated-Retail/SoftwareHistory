--
-- email-view-next.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- continue with the vend process
kernel.registeractorhitnotify(
	"email_view_next_button",
	function()
		kernel.timerremove( "email_next_timer" )
		marketing_opt_in_flow( ShoppingCart.marketing_opt_in )
	end
)

-- set a timer to just move on if NEXT is not pressed
local timeout = kernel.ttsgettimeout( "shorter", 8000 )
kernel.timercreate(
	"email_next_timer",
	timeout,
	timeout,
	function()
		kernel.timerremove( "email_next_timer" )
		marketing_opt_in_flow( ShoppingCart.marketing_opt_in )
	end )
kernel.timerstart( "email_next_timer" )

set_email_view_text = function( text )
	kernel.setmacro( "EmailText", text )
	kernel.markactordirty( "email_message" )
	if kernel.ttsenabled() then
		kernel.ttsrepeat();  -- assests have been set and toggled
	end
end