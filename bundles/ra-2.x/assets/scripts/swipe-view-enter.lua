--
-- swipe-view-enter.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
swipe_movie_resource = "swipe_movie"
kernel.analyticsaddnode( "authorizations", "cardSwipe", ShoppingCart.session_id )

--We Use Same screen for online pick up and swipe.
if kernel.getlasthitactorname() == "online_pickup_button"
or kernel.getlasthitactorname() == "online_red_box_label"
or kernel.getlasthitactorname() == "online_pickup_label" then
	swipe_movie_resource = "swipe_onlinepickup_movie"
	if kernel.ttsenabled() == true then
		kernel.setmacro( "TTS:SwipeTitle", "Swipe your card to pick up your reservation." )
		kernel.setmacro( "TTS:SwipeMessage", "You can swipe your card up or down.  It doesn't matter." )
	end
elseif credit_button_hit then
	swipe_movie_resource = "swipe_subscribers_movie"
	if kernel.ttsenabled() == true then
		kernel.setmacro( "TTS:SwipeTitle", "Let's see those credits." )
		kernel.setmacro( "TTS:SwipeMessage", "To pay with credits, swipe a card up or down, that's associated with your account." )
	end
else
	swipe_movie_resource = "swipe_movie"
	if kernel.ttsenabled() == true then
		kernel.setmacro( "TTS:SwipeTitle", "Ready. Set. Swipe!" )
		kernel.setmacro( "TTS:SwipeMessage", "You can swipe your card up or down - it doesn't matter." )
	end
end

if kernel.ttsenabled() == true then
	kernel.ttsrepeat();  -- the titles and assests have been set and toggled
end

kernel.pushmovie( swipe_movie_resource, { x = 0, y = 112, width = 1024, height = 543 } )

-- Tell the kernel to begin looking for
-- valid credit card track data
kernel.trackdatastart()

-- We want to know when we have
-- valid track data so we can proceed
kernel.trackdatanotify(
	function()
		kernel.timerstop( "swipe_timer" )
		kernel.popmovie()
		if kernel.trackdataisvalid() == false then
			kernel.analyticsaddattribute( "cardSwipe", "result", "invalid", ShoppingCart.session_id )
			local function bad_swipe_action()
				view_goback_safe( "bad_swipe_action", false )	
			end
			local message_view_manager = newmessageviewmanager()
			message_view_manager:show()
			message_view_manager:hide_title()
			view_analytics_event( "context", "cardNotRecognized" )
			message_view_manager:set_message( strings.invalid_card_message )
			message_view_manager:hide_first_button()
			message_view_manager:show_second_button( strings.continue_button, bad_swipe_action )
			message_view_manager:start_timer( bad_swipe_action )
		else
			kernel.analyticsaddattribute( "cardSwipe", "result", "valid", ShoppingCart.session_id )
			if swipe_view_next_func ~= nil then
				swipe_view_next_func()
			else
				view_goback_safe( "trackdatanotify", false )
			end
		end
	end )
-- If the user walks away, we need to
-- go back to from where he came.
kernel.timercreate(
	"swipe_timer",
	50000,
	nil,
	function()
		kernel.timerstop( "swipe_timer" )
		kernel.analyticsaddattribute( "cardSwipe", "result", "timeout", ShoppingCart.session_id )
		view_goback_safe( "swipe_timer:timeout", false )
	end )
kernel.timerstart( "swipe_timer" )

kernel.registeractorhitnotify(
	"back_button",
	function()
		kernel.timerstop( "swipe_timer" )
		kernel.popmovie()
		view_analytics_event( "buttonPress", "card-swipe-back" )
		kernel.analyticsaddattribute( "cardSwipe", "result", "cancelled", ShoppingCart.session_id )
		kernel.setmacro("MatureWarning:Header","")
		if swipe_view_cancel_func ~= nil then
			swipe_view_cancel_func()
		else
			view_goback_safe( "swipe_view:back_button", false )
		end
	end
)

kernel.registeractorhitnotify(
	"back_button_arrow",
	function()
		kernel.timerstop( "swipe_timer" )
		kernel.popmovie()
		view_analytics_event( "buttonPress", "card-swipe-back" )
		kernel.analyticsaddattribute( "cardSwipe", "result", "cancelled", ShoppingCart.session_id )
		if swipe_view_cancel_func ~= nil then
			swipe_view_cancel_func()
		else
			view_goback_safe( "swipe_view:back_button_arrow", false )
		end
	end
)