--
-- tickets-browse-page-back.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "browse-page-back" )
tickets_browse_pager:page_back()
tickets_show_selection_grid()