--
-- help-page-next.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

view_analytics_event( "pageChange", "free-trial-terms-and-privacy-view-next," .. string.lower( kernel.getmacro( "Help:Title" )) )

help_image_page = help_image_page + 1

free_trial_help_show_table()
