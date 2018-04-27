--
-- start-view-enter.lua
-- Copyright (C) 2017 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- callback from the .net side now that the start_idle_time is in .net
function start_idle_timer_callback()
    kernel.log("start idle timer callback")
	-- Now that analytics are being tracked on the carousel, the session has to be restarted on idle timeout
	if ShoppingCart ~= nil
	and kernel.analyticssessionexists( ShoppingCart.session_id ) == true then
        kernel.log("start view idle timer: closing analytics and abandoning cart ")
		ShoppingCart:close_analytics( "True" )
		ShoppingCart:abandon( "Start View User Idle Timeout" )
		ShoppingCart = newshoppingcart()
		ShoppingCart:start_new()
		ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart
	end
end


kernel.configure_start_view(localization_reload_resources, start_idle_timer_callback)
