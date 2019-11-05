--
-- check-out.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function finish_vend()
	view_show_safe( "vend_view" )
	if quick_return_is_running == false then
		ShoppingCart.vend_manager:show_movie_or_coming_soon()
	end
	if in_maintenance_mode() then
		ShoppingCart:prepos_vend_abort()
	else
		ShoppingCart:prepos_vend_continue()
	end
end

kernel.register_finish_vend_callback(finish_vend)
kernel.register_preposition_vend_callback(
	function()
		ShoppingCart:prepos_vend()
	end
)

kernel.register_preposition_vend_abort_callback(
	function()
		ShoppingCart:prepos_vend_abort()
	end
)

view_show_safe( "swipe_view" )
view_analytics_event( "context", "checkout" )