--
-- collection-message-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.CollectionsPopupShown", true)

initialize_idle_timer()

idle_timer:suspend()

collections_message_view_timer = newidletimer( "collections_message_view_timer" )
collections_message_view_timer:initialize(
	15000,
	function()
		idle_timer_expire_session()
	end
)
collections_message_view_timer:start()


view_analytics_event( "collectionspopupreason", collection_message_reason )
kernel.draw_collections_popup_dialog(
	145, 
	125, 
	734, 
	525,
	function()  -- see all collections
		view_analytics_event( "buttonPress", "see-all-collections" )
		view_goback_safe("collection message view see all collections")
		show_browse_view_collections()
	end,
	function(collection) -- collection selected
		view_analytics_event( "buttonPress", "view-collection," .. collection.product_name  )
		view_goback_safe("collection message collection selected")
		show_collection_items(collection)
	end,
	function() -- collection popup cancelled
		view_analytics_event( "buttonPress", "cancel-collection-mesage-view" )
		view_goback_safe("collection message view cancelled")
		kernel.log("collection message view cancelled")
	end
	)
