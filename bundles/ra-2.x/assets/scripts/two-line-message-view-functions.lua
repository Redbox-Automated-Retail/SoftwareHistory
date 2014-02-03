--
-- two-line-message-view-functions.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newtwolinemessageviewmanager()
	local t = {
		show = function( self )
			if self.is_showing then
				return
			end
			self.is_showing = true
			if kernel.peekview() ~= "two_line_message_view" then
				view_show_safe( "two_line_message_view" )
			end
		end,

		hide = function( self, pop_only )
            if not self.is_showing then
                return
            end
			self.is_showing = false
			self:stop_timer()
            if kernel.peekview() == "two_line_message_view" then
				view_goback_safe( "twolinemessageviewmanager:hide()", pop_only )
            end
		end,

		set_message_header = function( self, text )
			kernel.setmacro( "HeaderMessageText", text )
			kernel.markactordirty( "message_label_header" )
		end,

		set_message_line_one = function( self, text )
			kernel.setmacro( "LineOneMessageText", text )
			kernel.markactordirty( "message_label_one" )
		end,
		
		set_message_line_two = function( self, text )
			kernel.setmacro( "LineTwoMessageText", text )
			kernel.markactordirty( "message_label_two" )
		end,

		start_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			if kernel.timerexists( "prompt_timer" ) then
				return
			end
			timeout = timeout or 8000
			kernel.timercreate(
				"prompt_timer",
				timeout,
				timeout,
				function()
					self:stop_timer()
					if expired_func ~= nil then
						expired_func()
					end
                    self:hide()
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
