--
-- help-document-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart.pickup_recommendations_shown == false then
    -- reinitialize idle timer for TTS Help view timeout
    initialize_idle_timer()
    idle_timer:start()
end

local reboxLogoAction = function ()
    kernel.executescript( "activate_fa" )
end

if kernel.getlasthitactorname() == "Terms_conditions_button" or kernel.getlasthitactorname() == "terms_conditions_button" then
    kernel.set_session_value("HelpState.CurrentHelpDocument", "Terms")
    kernel.set_session_value("HelpState.CurrentHelpDocumentPage", "1")
    reboxLogoAction = nil
end


-- For the Recomendations On Pickup - Do not allow access to FMA
if ShoppingCart.pickup_recommendations_shown == true then
    redboxLogoAction = nil
end

kernel.configure_help_view(
    GiftCardsEnabled,
    function()
        view_goback_safe( "go_back", false )
    end,
    reboxLogoAction
)