--
-- help-document-view-leave.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart ~= nil and kernel.rental_shopping_cart_get_pickup_recommendations_shown( ShoppingCart.session_id ) == false and kernel.getlasthitactorname() ~= "play_pass_disclaimer_button" then
    -- reinitialize idle timer for TTS Help view timeout
    kernel.reset_application_idle_timer_timeout()
end

if kernel.getlasthitactorname() == "play_pass_disclaimer_button" then
    play_pass_help_view_timer:stop()
    kernel.resume_idle_timer( "idle_timer" )
end