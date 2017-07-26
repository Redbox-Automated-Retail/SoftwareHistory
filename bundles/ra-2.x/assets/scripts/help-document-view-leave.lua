--
-- help-document-view-leave.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart ~= nil and ShoppingCart.pickup_recommendations_shown == false and kernel.getlasthitactorname() ~= "play_pass_disclaimer_button" then
    -- reinitialize idle timer for TTS Help view timeout
    initialize_idle_timer()
end

if kernel.getlasthitactorname() == "play_pass_disclaimer_button" then
    play_pass_help_view_timer:stop()
    idle_timer:resume()
end