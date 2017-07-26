--
-- browse-filter-alpha-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.setmacro( "CartfullWarning:Header", strings.cart_full_header )
kernel.setmacro( "CartfullWarning:Message", strings.cart_full_body )
kernel.setactortext( "cart_full_continue_button", strings.continue_button )
kernel.set_session_value("BrowseState.IsShoppingCartFullMessagesOpen", true)
kernel.set_session_value("BrowseState.ShoppingCartFullMessagesShown", true)

kernel.registeractorhitnotify(
	"cart_full_continue_button",
	function()
		kernel.set_session_value("BrowseState.IsShoppingCartFullMessagesOpen", false)
		view_goback_safe( "cartfull_message_view", pop_only )
		hide_shopping_cart_full()
	end
)