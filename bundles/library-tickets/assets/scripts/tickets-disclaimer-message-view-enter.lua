--
-- tickets-disclaimer-message-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

disclaimer_idle_timer = newidletimer( "disclaimer_idle_timer" )
disclaimer_idle_timer:initialize(
	40000,
	function()
		local expire_session = function()
			-- the standard idle timer was stoped so a custom disclaimer timer could be used
			-- go back to using the standard idle_timer
			idle_timer:start()
			disclaimer_idle_timer:stop()
			kernel.log( "idle_timer:expire_session()" )
			ShoppingCart.TicketsCart:reset_cart()
			view_analytics_event( "context", "TicketsMessageView: Timeout." )
			view_unwindtobottom_safe( "application_startup::disclaimer_idle_timer.expire_session" )
		end
		local message_view_manager = newmessageviewmanager()
		message_view_manager:show()
		view_analytics_event( "context", "idleTimer" )
		message_view_manager:hide_message()
		message_view_manager:set_idle_timer_title( strings.more_time_prompt_message )
		message_view_manager:show_first_button( strings.no_thanks_button, expire_session )
		message_view_manager:show_second_button(
			strings.yes_button,
			function()
			end
		)
		message_view_manager:start_timer( expire_session )
	end )

-- Stop the standard idle timer
-- Start a custom disclaimer idle timer
idle_timer:stop()
disclaimer_idle_timer:start()

kernel.registeractorhitnotify(
	"reject_tickets_disclaimer",
	function()
		view_analytics_event( "buttonPress", "NO_button" )
		view_unwindto( "tickets_browse_view", true, "Disclaimer: User hit NO button" )
	end
)

kernel.registeractorhitnotify(
	"accept_tickets_disclaimer",
	function()
		view_analytics_event( "buttonPress", "YES_button" )
		view_show( "tickets_shopping_cart_view" )
	end
)

-- Set the text areas from the strings file
kernel.setactortext( "tickets_disclaimer_header", tickets_strings.disclaimer_bytheway )
kernel.setactortext( "tickets_disclaimer_area", ShoppingCart.TicketsCart.selected_supplier_disclaimer_note )

