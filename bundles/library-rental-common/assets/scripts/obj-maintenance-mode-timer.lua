--
-- obj-maintenance-mode-timer.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newmaintenancemodetimer()

	local t = {}

	t.start = function(
		self,
		start,
		frequency )
		kernel.timercreate(
			"minutes-in-maintenance",
			start or kernel.random( 100, 1000 ),
			frequency or 600000,
			self.on_timer )
		kernel.timerstart( "minutes-in-maintenance" )
		kiosk_minutes_in_maint_mode = 0
	end

	t.on_timer = function()
		kernel.timerstop( "minutes-in-maintenance" )
		kiosk_minutes_in_maint_mode = kiosk_minutes_in_maint_mode + ((frequency or 600000)/60000)
		kernel.healthsvcs_enqueuestatistic( "MinutesInMaintenance", kiosk_minutes_in_maint_mode )
		kernel.timerstart( "minutes-in-maintenance" )
	end
	
	t.stop = function( send_zero_value )
		kernel.timerstop( "minutes-in-maintenance" )
		if send_zero_value then
			kernel.healthsvcs_enqueuestatistic( "MinutesInMaintenance", 0 )
		end
	end

	t.clear = function()
		kernel.healthsvcs_enqueuestatistic( "MinutesInMaintenance", 0 )
	end
	
	return t
end
