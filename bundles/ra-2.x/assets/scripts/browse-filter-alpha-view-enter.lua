--
-- browse-filter-alpha-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()

show_browse_view()

if #ShoppingCart.items == 5
and kernel.get_session_value("BrowseState.IsShoppingCartFullMessagesOpen") == false
and kernel.get_session_value("BrowseState.ShoppingCartFullMessagesShown") == false then
	show_shopping_cart_full()
end

kernel.draw_browse_filter_bar(
    kernel.peekview(),
    function() 
        kernel.executescript( "start_over" )
    end,
    show_browse_view
)

if (kernel.get_application_state_value("IsADAAccessible") == true) then
	draw_action_bar()
	draw_ada_control()
else
    draw_action_bar_and_mini_cart(kernel.peekview())
    draw_top_menu()
end



