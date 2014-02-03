--
-- lib-attract-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function attract_startpulsebutton( actor_name )
	kernel.timercreate(
		"attract-timer",
		3000,
		3000,
		function()
			local alpha_function = function( n, t, v )
				kernel.setactoropacity( actor_name, v )
			end
			kernel.tweencreate(
				"pulse-down",
				"Linear",
				1.00,
				.50,
				15,
				alpha_function,
				function( n )
					kernel.tweencreate(
						"pulse-up",
						"Linear",
						.50,
						1.00,
						15,
						alpha_function,
						nil )
					kernel.tweenstart( "pulse-up" )
				end )
			kernel.tweenstart( "pulse-down" )
		end )
	kernel.timerstart( "attract-timer" )
end

function attract_stoppulsebutton()
	kernel.tweenremove( "pulse-up" )
	kernel.tweenremove( "pulse-down" )
	kernel.timerremove( "attract-timer" )
end
