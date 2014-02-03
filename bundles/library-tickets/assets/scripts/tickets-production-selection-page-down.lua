--
-- tickets-production-selection-page-down.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "production-page-down" )
tickets_production_pager:page_next()
tickets_show_production_selection_grid()