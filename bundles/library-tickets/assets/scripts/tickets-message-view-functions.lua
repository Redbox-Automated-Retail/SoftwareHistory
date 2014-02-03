--
-- tickets-message-view-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newticketsmessageviewmanager()

	local t = {
		name = "tickets_message_view",
		
		first_button_name = "gray_button",
		
		second_button_name = "red_button",
		
		prompt_timer = "tickets_prompt_timer",
		
		is_showing = false,
		
		on_hide = nil, -- on can override the default view_goback by assigning this method
		
		show = function( self )
			if self.is_showing then
				return
			end
			kernel.popmovie()
			self.is_showing = true
			if kernel.peekview() ~= self.name then
				view_show_safe( self.name )
			end
		end,
		
		hide = function( self )
			if not self.is_showing then
				return
			end
			self.is_showing = false
			self:stop_timer()
			if kernel.peekview() == self.name then
				if on_hide ~= nil then
					on_hide()
					on_hide = nil
				else
					view_goback_safe( "ticketsmessageviewmanager:hide()", pop_only )
				end
			end
		end,
		
		show_message_header = function( self )
			kernel.showactor( "tickets_message_header" )
		end,
		
		show_message = function( self )
			kernel.showactor( "tickets_message" )
		end,
		
		set_message_header = function( self, text )
			kernel.setmacro( "TicketsMessageView:MessageHeaderText", text )
			self:show_message_header()
		end,
		
		set_message = function( self, text )
			kernel.setmacro( "TicketsMessageView:MessageText", text )
			self:show_message()
		end,
		
		set_first_button_text = function( self, text )
			kernel.setmacro( "TicketsMessageView:FirstButtonText", text )
		end,
		
		set_second_button_text = function( self, text )
			kernel.setmacro( "TicketsMessageView:SecondButtonText", text )
		end,
		
		set_first_button_action = function( self, action )
			if action ~= nil  then
				kernel.registeractorhitnotify(
					"first_button",
					function()
						action()
						self:hide()
					end
				)
			end
		end,
		
		set_second_button_action = function( self, action )
			if action ~= nil  then
				kernel.registeractorhitnotify(
					"second_button",
					function()
						action()
						self:hide()
					end
				)
			end
		end,
		
		show_first_button = function( self, text, action )
			if not self.is_showing then
				return
			end
			if text ~= nil then
				self:set_first_button_text( text )
			end
			if action ~= nil then
				self:set_first_button_action( action )
			end
			kernel.showactor( "first_button" )
		end,
		
		show_second_button = function( self, text, action )
			if not self.is_showing then
				return
			end
			if text ~= nil then
				self:set_second_button_text( text )
			end
			if action ~= nil then
				self:set_second_button_action( action )
			end
			kernel.showactor( "second_button" )
		end,
		
		show_first_button_back_arrow = function( self )
			kernel.showactor( "message_view_back_arrow" )
		end,
		
		start_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			if kernel.timerexists( self.prompt_timer ) then
				return
			end
			timeout = timeout or 15000
			kernel.timercreate(
				self.prompt_timer,
				timeout,
				timeout,
				function()
					self:stop_timer()
					if expired_func ~= nil then
						expired_func()
					end
                    self:hide()
				end )
			kernel.timerstart( self.prompt_timer )
		end,
		
		stop_timer = function( self )
			if kernel.timerexists( self.prompt_timer ) then
				kernel.timerremove( self.prompt_timer )
			end
		end,
		
	}
	
	return t

end