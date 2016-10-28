--
-- play-pass-opt-in-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

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

idle_timer:suspend()

local timeout = kernel.ttsgettimeout( "long", 40000 )
local timeout_func = function()
    kernel.sessionsetplaypasspromptaccepted( ShoppingCart.session_id, false, ShoppingCart.play_pass_points_earned )
    kernel.promosvcs_kioskoptin( StoreManager:get_number(), email_optin, nil )
    continue_vend()
end
kernel.timercreate(
	"play_pass_opt_in_view_timer",
	timeout,
	timeout,
	function()
        timeout_func()
	end
)
kernel.timerstart( "play_pass_opt_in_view_timer" )

kernel.setactortext( "prompt_message", string.gsub( strings.play_pass_prompt_message, "{0}", ShoppingCart.play_pass_points_earned ) )
kernel.setactortext( "no_button", strings.no_thanks_button )
kernel.setactortext( "yes_button", strings.play_pass_join_now )

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
        timeout_func = function()
            continue_vend()
        end
        view_analytics_event( "buttonPress", "play_pass_join_button" )
        kernel.sessionsetplaypasspromptaccepted( ShoppingCart.session_id, true, ShoppingCart.play_pass_points_earned )
        email_optin[1].item_id = tostring( 1 )
        kernel.promosvcs_kioskoptin( StoreManager:get_number(), email_optin, nil )
        kernel.setactorimage( "background", "image_play_pass_confirm" )
        kernel.hideactor( "prompt_message" )
        kernel.setactortext( "confirm_message", string.gsub( strings.play_pass_confirm_message, "{0}", ShoppingCart.play_pass_points_earned ) )
        kernel.setactortext( "email_message", string.gsub( strings.play_pass_email_message, "{0}", tostring( ShoppingCart.email ) ) )
        kernel.hideactor( "no_button" )
        kernel.setactortext( "yes_button", strings.okay_button )
        kernel.registeractorhitnotify(
	        "yes_button",
	        function()
                continue_vend()
	        end
        )
	end
)