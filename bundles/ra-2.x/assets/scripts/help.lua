--
-- help.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.disableactor( "help_button" )

-- Start an analytics session if it does not already exist and capture button press on start_view
kernel.view_analytics_start("BrowseVend")

view_analytics_event( "buttonPress", "help_button" )

kernel.set_session_value("HelpState.CurrentHelpDocument", "Faq")
kernel.set_session_value("HelpState.CurrentHelpDocumentPage", "1")

view_show_safe( "help_document_view" )

