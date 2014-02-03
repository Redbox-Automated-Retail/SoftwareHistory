--
-- activate-fa.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart ~= nil then
	kernel.analyticsdisposesession( ShoppingCart.session_id )
end
kernel.activatebundle( "Field Maintenance Application", "*" )
