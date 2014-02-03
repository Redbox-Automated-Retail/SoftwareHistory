--
-- tickets-swipe-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

kernel.pushmovie( "swipe_movie", { x = 0, y = 112, width = 1024, height = 543 } )

kernel.trackdatastart()
kernel.trackdatanotify(
	function()
		kernel.timerstop( "tickets_swipe_timer" )
		--kernel.popmovie()
		if kernel.trackdataisvalid() == false then
			kernel.popmovie()
			view_goback_safe( "bad_swipe_action", false )
		else
			if tickets_swipe_view_next_func ~= nil then
				tickets_swipe_view_next_func()
			else
				kernel.popmovie()
				view_goback_safe( "trackdatanotify", false )
			end
		end
	end
)

kernel.timercreate(
	"tickets_swipe_timer",
	50000,
	nil,
	function()
		kernel.timerstop( "tickets_swipe_timer" )
		view_goback_safe( "tickets_swipe_timer:timeout", false )
	end
)
kernel.timerstart( "tickets_swipe_timer" )

kernel.registeractorhitnotify(
	"back_button",
	function()
		kernel.timerstop( "tickets_swipe_timer" )
		kernel.popmovie()
		if tickets_swipe_view_cancel_func ~= nil then
			tickets_swipe_view_cancel_func()
		else
			view_goback_safe( "tickets_swipe_view:back_button", false )
		end
	end
)