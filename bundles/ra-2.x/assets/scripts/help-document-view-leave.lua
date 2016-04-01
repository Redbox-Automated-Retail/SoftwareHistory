--
-- help-document-view-leave.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart.pickup_recommendations_shown == false then
    -- reinitialize idle timer for TTS Help view timeout
    initialize_idle_timer()
end
