--
-- browse-filter-alpha-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.setmacro( "CartfullWarning:Header", strings.cart_full_header )
kernel.setmacro( "CartfullWarning:Message", strings.cart_full_body )
kernel.setactortext( "cart_full_continue_button", strings.continue_button )
browse_is_cart_full_open = true
shopping_cart_full_message_shown = true

kernel.registeractorhitnotify(
	"cart_full_continue_button",
	function()
		browse_is_cart_full_open = false
		view_goback_safe( "cartfull_message_view", pop_only )
		hide_shopping_cart_full()
	end
)