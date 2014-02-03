--
-- tickets-lock-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

create_lock_update_timer = function( lock_guids )
	local timer_name = "lock_update_timer"
	local timer_interval = lock_guids[1].LockDuration - 90000
	if timer_interval < 90000 then
		timer_interval = 90000
	end
	kernel.log( string.format( "timer_interval - %s", timer_interval ) )

	if kernel.timerexists( timer_name ) then
		kernel.timerremove( timer_name )
	end

	kernel.timercreate(
		timer_name,
		timer_interval,
		timer_interval,
		function()
			for k, v in ipairs( lock_guids ) do
				kernel.log( string.format( "|Tickets| Updating lock for TicketGroupId:%s - TicketGroupdGuid:%s - Quantity:%s", v.TicketGroupId, v.TicketGroupGuid, v.Quantity ) )
				view_analytics_event( "context", "UpdateLock Timer Sent" )
				kernel.ticketsvcs_updateticketlock(
					StoreManager:get_number(),
					v.TicketGroupId,
					v.TicketGroupGuid,
					v.Quantity,
					function( result )
						kernel.log( result )
					end
				)
			end
		end
	)
	kernel.timerstart( timer_name )
end

remove_lock_update_timer = function()
	kernel.log( "|Tickets| Removing lock update timer." )
	if kernel.timerexists( "lock_update_timer" ) then
		kernel.timerremove( "lock_update_timer" )
	end
end
