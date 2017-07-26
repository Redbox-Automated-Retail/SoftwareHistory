--
-- select-platform.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

browse_rent_or_sellthru = "rent"

if kernel.get_application_state_value("IsADAAccessible") == true then
	browse_view_name = "browse_view_ada_accessible"
else
	browse_view_name = "browse_view"
end
view_show_safe( "platform_selection_view" )