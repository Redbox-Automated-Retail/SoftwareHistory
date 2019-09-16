--
-- error-message-view-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newerrormessageviewmanager()
	local t = {
		show = function( self )
			if self.is_showing then
				return
			end
			self.is_showing = true
			if kernel.peekview() ~= "error_message_view" then
				view_show_safe( "error_message_view" )
			end
		end,

		hide = function( self, pop_only )
            if not self.is_showing then
                return
            end
			self.is_showing = false
			self:stop_timer()
            if kernel.peekview() == "error_message_view" then
				view_goback_safe( "errormessageviewmanager:hide()", pop_only )
            end
		end,

		set_message = function( self, text )
			kernel.setanalyticsviewmessage(text)
			kernel.setmacro( "MessageText", text )
			kernel.markactordirty( "message_label" )
			if kernel.ttsenabled() == true then
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,

		start_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			if kernel.timerexists( "prompt_timer" ) then
				return
			end
			timeout = timeout or 8000
            timeout = kernel.ttsgettimeout( "short", timeout )
			kernel.timercreate(
				"prompt_timer",
				timeout,
				timeout,
				function()
					self:stop_timer()
					if expired_func ~= nil then
						expired_func()
					end
                    --self:hide()
				end )
			kernel.timerstart( "prompt_timer" )
		end,

		stop_timer = function( self )
			kernel.timerremove( "prompt_timer" )
		end,
		
		is_showing = false
	}
	return t
end
