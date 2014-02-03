--
-- email-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newemailmanager()
	local t = {
		is_showing = false,
		
		email_optin = false,
		
		display_no_thanks = false,

		title_name = nil,

		init = function( self, maximum_length )
			self.maximum_length = maximum_length or 32
			self.email_optin = false
			self.enter_event = nil
			self.cancel_event = nil
			self.validate_event = nil
			self.actor_hit_event = nil
			self:set_value( "" )
			self:set_error( "" )
			self:set_message( "" )
			self.display_no_thanks = false
		end,

		show = function( self, tag_name )
			if self.is_showing then
				return
			end
			title_name = tag_name
			kernel.setmacro( "Email:PanelTag", tag_name )
			self.is_showing = true
			if kernel.peekview() ~= "tickets_email_view" then
				current_email_manager = self
				view_show_safe( "tickets_email_view" )
			end
		end,

		hide = function( self, pop_only )
			self.is_showing = false
			self:stop_timer()
			self:stop_idle_timer()
			if kernel.peekview() == "tickets_email_view" then
				view_goback_safe( "emailmanager:hide()", pop_only )
			end
			current_email_manager = nil
		end,

		start_idle_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			timeout = timeout or 8000
			self.idle_timer = newidletimer( "email-idle-timer" )
			self.idle_timer:initialize(
				timeout,
				function()
					self.idle_timer:stop()
					if expired_func ~= nil then
						expired_func()
					else
						self:hide()
					end
				end )
			self.idle_timer:start()
		end,

		stop_idle_timer = function( self )
			if self.idle_timer == nil then
				return
			end
			self.idle_timer:stop()
			self.idle_timer = nil
		end,

		start_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			if kernel.timerexists( "email_timer" ) then
				return
			end
			timeout = timeout or 8000
			kernel.timercreate(
				"email_timer",
				timeout,
				timeout,
				function()
					self:stop_timer()
					if expired_func ~= nil then
						expired_func()
					else
						self:hide()
					end
				end )
			kernel.timerstart( "email_timer" )
		end,

		stop_timer = function( self )
			kernel.timerremove( "email_timer" )
		end,

		handle_actor_hit = function( self )

            local last_hit_tag = nil
            if kernel.getlasthitactortag ~= nil then
                last_hit_tag = kernel.getlasthitactortag()
            else
                last_hit_tag = actor_last_hit_tag
            end

            if last_hit_tag == nil then
                return
            end

			local text = self:get_value()
			if last_hit_tag == "enter" then
				if not self:raise_validate_event() then
					return
				end
				self:raise_enter_event()
				return
			elseif last_hit_tag == "clear" then
				self:set_value( "" )
				self:set_error( "" )
				return
			elseif last_hit_tag == "cancel" then
				self:set_value( "" )  --do we want to clear it? probably not
				self:raise_cancel_event()
				return
			elseif last_hit_tag == "request_sale" then
				local request = self.email_optin or false
				if request == false then
					kernel.setactorimage("image_request_sale_updates", "image_green_selected_checkbox" )
					self:set_email_optin( true )
				else
					kernel.setactorimage("image_request_sale_updates", "image_gray_unselected_checkbox" )
					self:set_email_optin( false )
				end
				return
			end						

			if  text ~= nil and kernel.getlasthitactortag() ~= "back"
			and string.len( text ) == self.maximum_length then
				return
			end

			if last_hit_tag == "back" then
				if string.len( text ) > 0 then
					text = string.sub( text, 1, string.len( text ) - 1 )
					self:set_value( text )
				end
			else
				text = text .. last_hit_tag
				self:set_value( text )
			end
			
			self:raise_actor_hit_event( text )
		end,

		get_value = function( self )
			return kernel.getmacro( "Email:Text" )
		end,

		set_value = function( self, value )
			kernel.setmacro( "Email:Text" , value or "" )
			kernel.markactordirty( "text_label" )
			if value == nil or #value < 3 then
				kernel.disableactor( "ok_button" )
				kernel.setactorimage( "ok_button_arrow", "image_next_right_arrow_gray" )
			else
				kernel.enableactor( "ok_button" )
				kernel.setactorimage( "ok_button_arrow", "image_next_right_arrow" )
			end
		end,

		get_error = function( self )
			return kernel.getmacro( "Email:ErrorLabel" )
		end,

		set_error = function( self, value )
			kernel.setmacro( "Email:ErrorLabel", value or "" )
			kernel.markactordirty( "error_label" )
		end,

		get_message = function( self )
			return kernel.getmacro( "Email:MessageLabel" )
		end,

		set_message = function( self, value )
			kernel.setmacro( "Email:MessageLabel", value or "" )
			kernel.markactordirty( "message_label" )
		end,

		get_email_optin = function( self )
			return self.email_optin
		end,

		set_email_optin = function( self, value )
			self.email_optin = value
		end,

		set_enter_event = function( self, func )
			self.enter_func = func
		end,

		set_cancel_event = function( self, func )
			if func ~= nil then
				self.display_no_thanks = true
			end
			self.cancel_event = func
		end,

		set_validate_event = function( self, func )
			self.validate_event = func
		end,

		set_actor_hit_event = function( self, func )
			self.actor_hit_event = func
		end,

		raise_enter_event = function( self )
			view_analytics_event( "buttonPress", "enter_button" )
			if self.enter_func ~= nil then
				self.enter_func()
			else
				view_goback_safe( "emailmanager:raise_enter_event (enter_func = nil)" )
			end
		end,

		raise_validate_event = function( self )
			if self.validate_event ~= nil then
				local message = self.validate_event( self:get_value() )
				if message ~= nil then
					self:set_error( message )
					return false
				end
			end
			return true
		end,

		raise_actor_hit_event = function( self )
			if self.actor_hit_event ~= nil then
				self.actor_hit_event( self:get_value() )
			end
		end,

		raise_cancel_event = function( self )
			view_analytics_event( "buttonPress", "cancel_button" )
			if self.cancel_event ~= nil then
				self.cancel_event()
			else
				view_goback_safe( "emailmanager:raise_cancel_event (cancel_event = nil)" )
			end
		end,

		set_cancel_button_text = function( self, value )
			kernel.setactortext( "cancel_button", value )
		end,
		
		set_ok_button_text = function( self, value )
			kernel.setactortext( "ok_button", value )
		end,
		
		show_ok_button_arrow = function( self )
			kernel.showactor( "ok_button_arrow" )
		end
	}
	return t
end
