--
-- idle-timer-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newidletimer( timer_name )
	timer_name = timer_name or "idle_timer"
	local t = {
		initialize = function( self, timeout, func )
			kernel.timercreate(
				timer_name,
				timeout or 15000,
				timeout or 15000,
				function()
					kernel.timerstop( timer_name )
					if self.is_enabled == false then
						return
					end
					kernel.log( "idle timer '" .. timer_name .. "' expired." )
					func()
				end )
		end,

		start = function( self )
			if self.is_enabled == true then
				return
			end
			kernel.log( "idle timer '" .. timer_name .. "' started." )
			self.is_enabled = true
			kernel.registerclicknotify(
				timer_name,
				function( button, x, y )
					self:reset()
				end )

			kernel.registerkeypressnotify(
				timer_name,
				function( key )
					self:reset()
				end )
			self:reset()
		end,

		stop = function( self )
			if self.is_enabled == false then
				return
			end
			kernel.log( "idle timer '" .. timer_name .. "' stopped." )
			self:stop_idle_timer()
			kernel.removekeypressnotify( timer_name )
			kernel.removeclicknotify( timer_name )
			kernel.removeidlenotify( timer_name )
			self.is_registered = false
			self.is_enabled = false
		end,

		suspend = function( self )
			if not self.is_enabled then
				return
			end
			self.is_suspended = true
			self:stop()
		end,

		resume = function( self )
			if not self.is_suspended then
				return
			end
			self.is_suspended = false
			self:start()
		end,

		reset = function( self )
			self:stop_idle_timer()
			if self.is_registered == false then
				self.is_registered = true
				kernel.registeridlenotify(
					timer_name,
					function()
						self.is_registered = false
						kernel.removeidlenotify( timer_name )
						self:start_idle_timer()
						kernel.log( "system is idle, timer: " .. timer_name )
					end )
			end
		end,

		start_idle_timer = function( self )
			if self.is_timer_started == false then
				self.is_timer_started = true
				kernel.timerstart( timer_name )
			end
		end,

		stop_idle_timer = function( self )
			if self.is_timer_started == true then
				self.is_timer_started = false
				kernel.timerstop( timer_name )
			end
		end,

		is_enabled = false,

		is_suspended = false,

		is_registered = false,

		is_timer_started = false
	}
	return t
end
