--
-- tickets-purchase-confirmation-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.setactortext( "header", tickets_strings.confirmation_header )

if ShoppingCart.TicketsCart.selected_delivery_method_id == "1" then
	kernel.setactortext( "body_1", tickets_strings.confirmation_body_1_will_call )
elseif ShoppingCart.TicketsCart.selected_delivery_method_id == "2" then
	kernel.setactortext( "body_1", tickets_strings.confirmation_body_1_email )
end
