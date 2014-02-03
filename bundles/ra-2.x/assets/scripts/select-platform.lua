--
-- select-platform.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

total_products_shown = nil
browse_filter_alpha_selected = nil
browse_rent_or_sellthru = "rent"

if kernel.getfilter( "ada_accessible" ) == "True" then
	browse_view_name = "browse_view_ada_accessible"
else
	browse_view_name = "platform_selection_view"
end
view_show_safe( browse_view_name )
