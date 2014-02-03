--
-- free-trial-swipe-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

free_trial_need_more_time_timer:suspend()
swipe_movie_resource = "swipe_free_trial_movie"
kernel.pushmovie( swipe_movie_resource, { x = 0, y = 112, width = 1024, height = 543 } )

-- Tell the kernel to begin looking for
-- valid credit card track data
kernel.trackdatastart()

-- We want to know when we have
-- valid track data so we can proceed
kernel.trackdatanotify(
	function()
		kernel.timerstop( "free_trial_swipe_timer" )
		kernel.popmovie()

		if kernel.trackgetcardtype() == "RedboxGiftCard" then
			view_show("free_trial_gift_card_swiped_popup")
		elseif kernel.trackdataisvalid() == false then
			local function bad_swipe_action()
				kernel.timerstop( "free_trial_swipe_timer" )
				kernel.log(": bad swipe action")
			end
			local message_view_manager = newmessageviewmanager()
			message_view_manager:show()
			message_view_manager:hide_title()			
			view_analytics_event( "context", "cardNotRecognized" )			
			message_view_manager:set_message("Sorry, your card could not be recognized. Please try again.")
			message_view_manager:hide_first_button()
			message_view_manager:show_second_button("CONTINUE", bad_swipe_action )
			message_view_manager:start_timer( bad_swipe_action )			
		else						
			kernel.timerstop( "free_trial_swipe_timer" )
			kernel.setmacro( "JVHardTake:Zip:EditPath", "swipe" )
			view_show("free_trial_zip_code_view")
		end
	end )

-- If the user walks away, we need to
-- go back to from where he came.
kernel.timercreate(
	"free_trial_swipe_timer",
	50000,
	nil,
	function()
		view_analytics_event( "context", "Timeout" )
		kernel.timerstop( "free_trial_swipe_timer" )
		idle_timer_show_prompt_english(idle_timer_expire_session)
	end )
kernel.timerstart( "free_trial_swipe_timer" )

kernel.registeractorhitnotify(
	"free_trial_swipe_back_button",
	function()
		kernel.timerstop( "free_trial_swipe_timer" )
		kernel.popmovie()

		view_analytics_event( "buttonPress", "free_trial_swipe_screen_back_button" )		
		view_goback_safe( "free_trial_swipe_view:back_button", false )
	end
)

kernel.registeractorhitnotify(
	"free_trial_swipe_back_button_arrow",
	function()
		kernel.timerstop( "free_trial_swipe_timer" )
		kernel.popmovie()
		
		view_analytics_event( "buttonPress", "free_trial_swipe_screen_back_button" )		
		view_goback_safe( "free_trial_swipe_view:back_button_arrow", false )
	end
)
