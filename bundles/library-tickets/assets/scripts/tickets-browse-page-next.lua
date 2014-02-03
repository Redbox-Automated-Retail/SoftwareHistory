--
-- tickets-browse-page-next.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "browse-page-next" )
tickets_browse_pager:page_next()
tickets_show_selection_grid()