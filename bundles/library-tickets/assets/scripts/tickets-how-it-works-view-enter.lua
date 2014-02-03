--
-- how-it-works-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()

kernel.setmacro( "point_3:text", string.gsub(tickets_strings.how_it_works_point_3, "{0}", tickets_fee_for_display ) )

local last_hit_actor = kernel.getlasthitactorname()
kernel.log( "Last hit actor: ".. tostring( last_hit_actor ) )

if last_hit_actor == "tickets_button"
or last_hit_actor == "abe_tickets_button" then
	kernel.registeractorhitnotify(
		"continue_button",
		function()
			view_analytics_event( "buttonPress", "continue_button" )
			view_show( "tickets_browse_view" )
		end
	)
else
	kernel.registeractorhitnotify(
		"continue_button",
		function()
			view_analytics_event( "buttonPress", "continue_button" )
			view_goback()
		end
	)
end
