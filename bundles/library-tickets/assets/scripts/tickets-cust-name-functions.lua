--
-- cust-name-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newcustnamemanager()
	local t = {
		is_showing = false,
		
		edit_first = true,
		
		title_name = nil,
 
		init = function( self, maximum_length )
			self.maximum_length = maximum_length or 32
			self.enter_event = nil
			self.cancel_event = nil
			self.validate_event = nil
			self.actor_hit_event = nil
			self:set_first_value( "" )
			self:set_last_value( "" )
			self:set_error( "" )
			self:set_message( "" )
		end,

		show = function( self, tag_name )
			if self.is_showing then
				return
			end
			title_name = tag_name
			kernel.setmacro( "CustName:PanelTag", tag_name )
			self.is_showing = true
			if kernel.peekview() ~= "tickets_cust_name_view" then
				view_show_safe( "tickets_cust_name_view" )
				current_email_manager = self
			end
		end,

		hide = function( self, pop_only )
			self.is_showing = false
			self:stop_timer()
			self:stop_idle_timer()
			if kernel.peekview() == "tickets_cust_name_view" then
				view_goback_safe( "custnamemanager:hide()", pop_only )
			end
			current_email_manager = nil
		end,

		start_idle_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			timeout = timeout or 8000
			self.idle_timer = newidletimer( "cust-name-idle-timer" )
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
			if kernel.timerexists( "cust_name_timer" ) then
				return
			end
			timeout = timeout or 8000
			kernel.timercreate(
				"cust_name_timer",
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
			kernel.timerstart( "cust_name_timer" )
		end,

		stop_timer = function( self )
			kernel.timerremove( "cust_name_timer" )
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
			elseif last_hit_tag == "first" then
				if self.edit_first == false then
					self.edit_first = true
					self:update_xy( "first", "last" )
				end
				return
			elseif last_hit_tag == "last" then
				if self.edit_first == true then
					self.edit_first = false
					self:update_xy( "last", "first" )
				end
				return
			elseif last_hit_tag == "clear" then
				self:set_value( "" )
				self:set_error( "" )
				return
			elseif last_hit_tag == "request_sale" then
				local request = kernel.getmacro("CustName:request_sale") or "false"
				if request == "false" then
					kernel.setactorimage("image_request_sale_updates", "image_red_checkmark" )
					kernel.setmacro("CustName:request_sale", "true")
				else
					kernel.setactorimage("image_request_sale_updates", "image_gray_checkmark" )
					kernel.setmacro("CustName:request_sale", "false")
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
			if self.edit_first == true then
				return self:get_first_value()
			end
			return self:get_last_value()
		end,

		set_value = function( self, value )
			if self.edit_first == true then
				self:set_first_value( value )
			else
				self:set_last_value( value )
			end
		end,

		get_first_value = function( self )
			return kernel.getmacro( "CustName:First" )
		end,
		
		set_first_value = function( self, value )
			kernel.setmacro( "CustName:First" , value or "" )
			kernel.markactordirty( "text_first_name_label" )
			last = self:get_last_value() or ""
			if #value < 1 or #last < 1 then
				kernel.disableactor("ok_button")
				kernel.setactorimage("image_next_right_arrow_btn","image_next_right_arrow_gray")
			else
				kernel.enableactor("ok_button")
				kernel.setactorimage("image_next_right_arrow_btn","image_next_right_arrow")
			end
		end,
		
		get_last_value = function( self )
			return kernel.getmacro( "CustName:Last" )
		end,
		
		set_last_value = function( self, value )
			kernel.setmacro( "CustName:Last" , value or "" )
			kernel.markactordirty( "text_last_name_label" )
			first = self:get_first_value() or ""
			if #value < 1 or #first < 1 then
				kernel.disableactor("ok_button")
				kernel.setactorimage("image_next_right_arrow_btn","image_next_right_arrow_gray")
			else
				kernel.enableactor("ok_button")
				kernel.setactorimage("image_next_right_arrow_btn","image_next_right_arrow")
			end
			
		end,

		get_error = function( self )
			return kernel.getmacro( "CustName:ErrorLabel" )
		end,

		set_error = function( self, value )
			kernel.setmacro( "CustName:ErrorLabel", value or "" )
			kernel.markactordirty( "error_label" )
		end,

		get_message = function( self )
			return kernel.getmacro( "CustName:MessageLabel" )
		end,

		set_message = function( self, value )
			kernel.setmacro( "CustName:MessageLabel", value or "" )
			kernel.markactordirty( "message_label" )
		end,

		set_enter_event = function( self, func )
			self.enter_func = func
		end,

		set_cancel_event = function( self, func )
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
				view_goback_safe( "custnamemanager:raise_enter_event (enter_func = nil)" )
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
				view_goback_safe( "custnamemanager:raise_cancel_event (cancel_event = nil)" )
			end
		end,

		set_cancel_button_text = function( self, value )
			kernel.setactortext( "cancel_button", value )
		end,

		update_xy = function( self, active_field, inactive_field )
			local active_name = "image_" .. active_field .. "_name_text_box"
			local inactive_name = "image_" .. inactive_field .. "_name_text_box"
			kernel.setactorimage( active_name, "image_will_call_entry_field_active" )
			kernel.setactorimage( inactive_name, "image_will_call_entry_field" )
			local old_x = kernel.getactorx( active_name )
			local old_y = kernel.getactory( active_name )
			kernel.setactorx( active_name, old_x - 5 )
			kernel.setactory( active_name, old_y - 5 )
			kernel.setactorheight( active_name, 81 )
			kernel.setactorwidth( active_name, 319 )
			old_x = kernel.getactorx( inactive_name )
			old_y = kernel.getactory( inactive_name )
			kernel.setactorx( inactive_name, old_x + 5 )
			kernel.setactory( inactive_name, old_y + 5 )
			kernel.setactorheight( inactive_name, 71 )
			kernel.setactorwidth( inactive_name, 309 )
		end
	}
	return t
end
