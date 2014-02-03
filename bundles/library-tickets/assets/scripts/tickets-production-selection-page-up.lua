--
-- tickets-production-selection-page-up.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "production-page-up" )
tickets_production_pager:page_back()
tickets_show_production_selection_grid()