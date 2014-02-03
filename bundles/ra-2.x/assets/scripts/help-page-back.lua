--
-- help-page-back.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "help-page-back," .. string.lower( kernel.getmacro( "Help:Title" )) )

help_image_page = help_image_page - 1

help_show_table()
