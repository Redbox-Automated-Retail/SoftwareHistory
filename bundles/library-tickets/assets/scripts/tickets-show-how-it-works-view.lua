--
-- tickets-show-how-it-works-view.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.analyticsaddelement( "session", "tickets", "True", ShoppingCart.session_id )

if TicketsProfilePricing["Fee"] ~= nil then
	tickets_fee_for_display = format_fee_per_ticket( TicketsProfilePricing["Fee"].value )
else
	tickets_fee_for_display = "1"
end

if HowItWorksToggle == true then
	view_show( "tickets_how_it_works_view" )
else
	view_show( "tickets_browse_view" )
end