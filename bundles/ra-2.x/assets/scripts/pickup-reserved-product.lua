--
-- pickup-reserved-product.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Stop the reservation expire timer, so reservations can't expire while in the pickup flow
kernel.timerstop( "reservation-expire-timer" )

kernel.disableactor( "pickup_reservation_button" )

ShoppingCart = newshoppingcart()
ShoppingCart:start_new()
ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart

-- Start an analytics session if it does not already exist and capture button press on start_view
kernel.view_analytics_start("PickupReservation", "OnlinePickup")
view_analytics_event( "buttonPress", "pickup_reservation_button" )
kernel.analyticsserviceaddbuttonpressevent("StartPickUp")

swipe_view_cancel_func = function()
	kernel.analyticsserviceaddbuttonpressevent("PickUpBack")
	kernel.close_open_session_and_start_new_rental_session("pick_up_back")
end

kernel.set_swipe_view_parameters(swipe_view_cancel_func, swipe_view_next_func, "ReservationPickup")
view_show_safe( "swipe_view" )
view_analytics_event( "context", "onlinePickup" )
