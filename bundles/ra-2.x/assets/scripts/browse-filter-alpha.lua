--
-- browse-filter-alpha.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
alpha_scene_manager = newscenemanager()
view_show_safe( "browse_filter_alpha_view" )
