--
-- tickets-browse-products.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.analyticsaddelement( "session", "tickets", "True", ShoppingCart.session_id )

view_show( "tickets_browse_view" )