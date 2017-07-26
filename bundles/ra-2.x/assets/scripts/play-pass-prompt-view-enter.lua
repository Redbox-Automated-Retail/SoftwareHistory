--
-- play-pass-prompt-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

local email_optin = {}
table.insert( email_optin,
	{
		item_id = tostring( 2 ),
		item_type = "PlayPassOptInAtKiosk",
		optin_method_type = "EmailAddress",
		optin_method_data = ShoppingCart.email,
		template_name = nil,
		campaign_name = "PlayPassOptInAtKiosk"
	}
)

kernel.timercreate(
	"play_pass_prompt_view_timer",
	40000,
	40000,
	function()
        kernel.sessionsetplaypasspromptaccepted( ShoppingCart.session_id, false, ShoppingCart.play_pass_points_earned )
        kernel.promosvcs_kioskoptin( StoreManager:get_number(), email_optin, nil )
        continue_vend()
	end
)
kernel.timerstart( "play_pass_prompt_view_timer" )

kernel.setactortext( "message", string.gsub( strings.play_pass_prompt_message, "{0}", ShoppingCart.play_pass_points_earned ) )
kernel.setactortext( "disclaimer_message", strings.play_pass_prompt_disclaimer_message )
kernel.setactortext( "disclaimer_button", strings.play_pass_terms )
kernel.setactortext( "no_button", strings.no_thanks_button )
kernel.setactortext( "yes_button", strings.play_pass_join_now )

if kernel.getculture() == "es-MX" then
    kernel.setactorx( "disclaimer_button", 766 )
    kernel.setactory( "disclaimer_button", 572 )
end

kernel.registeractorhitnotify(
	"disclaimer_button",
	function()
        view_analytics_event( "buttonPress", "play_pass_disclaimer_button" )
        kernel.setlasthitactorname( "play_pass_disclaimer_button" )
        view_show_safe( "help_document_view" )
	end
)
kernel.registeractorhitnotify(
	"no_button",
	function()
        view_analytics_event( "buttonPress", "play_pass_no_thanks_button" )
        kernel.sessionsetplaypasspromptaccepted( ShoppingCart.session_id, false, ShoppingCart.play_pass_points_earned )
        email_optin[1].item_id = tostring( 0 )
        kernel.promosvcs_kioskoptin( StoreManager:get_number(), email_optin, nil )
        continue_vend()
	end
)
kernel.registeractorhitnotify(
	"yes_button",
	function()
        view_analytics_event( "buttonPress", "play_pass_join_button" )
        kernel.sessionsetplaypasspromptaccepted( ShoppingCart.session_id, true, ShoppingCart.play_pass_points_earned )
        email_optin[1].item_id = tostring( 1 )
        kernel.promosvcs_kioskoptin( StoreManager:get_number(), email_optin, nil )
        view_show_safe( "play_pass_confirm_view" )
	end
)