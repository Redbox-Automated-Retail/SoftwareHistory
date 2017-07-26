--
-- help-document-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart ~= nil and ShoppingCart.pickup_recommendations_shown == false and kernel.getlasthitactorname() ~= "play_pass_disclaimer_button" then
    -- reinitialize idle timer for TTS Help view timeout
    initialize_idle_timer()
    idle_timer:start()
end

local go_back_action = function()
    view_goback_safe( "go_back", false )
end
local rebox_logo_action = function()
    kernel.executescript( "activate_fa" )
end
local show_as_popup = false

if kernel.getlasthitactorname() == "Terms_conditions_button" or kernel.getlasthitactorname() == "terms_conditions_button" then
    kernel.set_session_value( "HelpState.CurrentHelpDocument", "Terms" )
    kernel.set_session_value( "HelpState.CurrentHelpDocumentPage", "1" )
    rebox_logo_action = nil
end

if kernel.getlasthitactorname() == "play_pass_disclaimer_button" then
    idle_timer:suspend()
    go_back_action = function()
        play_pass_help_view_timer:stop()
        idle_timer:resume()

        local prior_view = kernel.peekpriorview()

        view_goback_safe( "go_back", false )

        if prior_view == "credit_balance_view" then
            kernel.log("showing credit_balance_view again")
            view_goback_safe( "go_back", false )
            key_pad_next_getcustomercredits_func()
        end
    end
    play_pass_help_view_timer = newidletimer( "play_pass_help_view_timer" )
    play_pass_help_view_timer:initialize( 40000, go_back_action )
    play_pass_help_view_timer:start()
    kernel.set_session_value( "HelpState.CurrentHelpDocument", "PlayPass" )
    kernel.set_session_value( "HelpState.CurrentHelpDocumentPage", "1" )
    rebox_logo_action = nil
    show_as_popup = true
end

-- For the Recomendations On Pickup - Do not allow access to FMA
if ShoppingCart ~= nil and ShoppingCart.pickup_recommendations_shown == true then
    rebox_logo_action = nil
end

kernel.configure_help_view(
    GiftCardsEnabled,
    go_back_action,
    rebox_logo_action,
    show_as_popup
)