--
-- multi-disc-vend-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.setactortext( "discs_image_number_text", tostring( kernel.current_rental_shopping_cart_item_count() ) )

kernel.timercreate(
	"multi_disc_vend_view_timer",
	3000,
	3000,
	function()
		finish_vend()
	end
)
kernel.timerstart( "multi_disc_vend_view_timer" )