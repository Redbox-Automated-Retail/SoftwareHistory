--
-- play-pass-confirm-view-enter.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

kernel.timercreate(
	"play_pass_confirm_view_timer",
	40000,
	40000,
	function()
        continue_vend()
	end
)
kernel.timerstart( "play_pass_confirm_view_timer" )

kernel.setactortext( "message", string.gsub( strings.play_pass_confirm_message, "{0}", ShoppingCart.play_pass_points_earned ) )
kernel.setactortext( "email_message", string.gsub( strings.play_pass_email_message, "{0}", tostring( ShoppingCart.email ) ) )
kernel.setactortext( "disclaimer_message", strings.play_pass_confirm_disclaimer_message )
kernel.setactortext( "disclaimer_button", strings.play_pass_terms )
kernel.setactortext( "yes_button", strings.okay_button )

if kernel.getculture() == "es-MX" then
    kernel.setactorx( "disclaimer_button", 858 )
    kernel.setactory( "disclaimer_button", 581 )
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
	"yes_button",
	function()
        continue_vend()
	end
)