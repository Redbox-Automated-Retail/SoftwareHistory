--
-- keyboard-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newkeyboardmanager()
	local t = {
		is_showing = false,

		title_name = nil,
 
		init = function( self, maximum_length )
			self.maximum_length = 50 --DEFECT:6581 - Changed to 50 according to DB limits
			self.enter_event = nil
			self.cancel_event = nil
			self.validate_event = nil
			self.actor_hit_event = nil
			self:set_value( "" )
			self:set_error( "" )
			self:set_message( "" )
			self:set_marketing( strings.email_standard_assurance )
		end,

		show = function( self, tag_name )
			if self.is_showing then
				return
			end
			title_name = tag_name
			kernel.setmacro( "KeyBoard:PanelTag", tag_name )
			self.is_showing = true
			if kernel.peekview() ~= "key_board_view" then
				view_show_safe( "key_board_view" )
				current_keyboard_manager = self
				
				if tag_name == "EMAIL ADDRESS" or string.find( tag_name, "CORREO ELECTR" ) ~= nil then
					kernel.showactor("yahoo_button")
					kernel.showactor("hotmail_button")
					kernel.showactor("gmail_button")
					kernel.showactor("aol_button")
					kernel.showactor("under_score_button")
					kernel.showactor("at_button")
					kernel.showactor("dot_button")
					kernel.showactor( "dotcom_button" )
					kernel.showactor( "clear_button" )
					kernel.showactor( "clear_button_icon" )
					kernel.showactor( "marketing_label" )
					kernel.setactorwidth( "space_bar_button", 315 )
					kernel.setactorwidth( "text_label", 615 )
					kernel.setactorx( "back_button", 695 )	
					if kernel.getculture() == "en-US" then
						kernel.setactorx( "back_button_arrow", 715 )	
					else
						kernel.setactorx( "back_button_arrow" , 714 )	
					end				
				elseif tag_name == "PROMO CODE" or string.find( tag_name, "PROMOCI" ) ~= nil then
					kernel.setactorx("two_button",143)
					kernel.setactorx("three_button",219)
					kernel.setactorx("four_button",295)
					kernel.setactorx("five_button",371)
					kernel.setactorx("six_button",447)
					kernel.setactorx("seven_button",523)
					kernel.setactorx("eight_button",599)
					kernel.setactorx("nine_button",675)
					kernel.setactorx("zero_button",751)
					if TextToSpeechEnabled == true then
						self:set_marketing( "" )
					end
				end
			end
			if TextToSpeechEnabled == true then
				kernel.ttsrepeat();  -- assests have been updated
			end
		end,

		hide = function( self, pop_only )
			self.is_showing = false
			self:stop_timer()
			self:stop_idle_timer()
			if kernel.peekview() == "key_board_view" then
				view_goback_safe( "keyboardmanager:hide()", pop_only )
			end
			current_keyboard_manager = nil
		end,

		start_idle_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			timeout = timeout or 8000
			if TextToSpeechEnabled == true then
				timeout = TTSTimeoutKeyboardView
			end
			
			self.idle_timer = newidletimer( "keyboard-idle-timer" )
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
			if kernel.timerexists( "keyboard_timer" ) then
				return
			end
			timeout = timeout or 8000
			if TextToSpeechEnabled == true then
				timeout = TTSTimeoutKeyboardView
			end
			kernel.timercreate(
				"keyboard_timer",
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
			kernel.timerstart( "keyboard_timer" )
		end,

		stop_timer = function( self )
			kernel.timerremove( "keyboard_timer" )
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
				keyboard_value = self:get_value()
				self:raise_enter_event()
				return
			elseif last_hit_tag == "clear" then
				self:set_value( "" )
				self:set_error( "" )
				return
			elseif last_hit_tag == "cancel" then
				if show_email_confirmation and not check_email_address then
					show_email_confirmation = false
					
					local message_view_manager = newmessageviewmanager()
					message_view_manager:hide( true )
					message_view_manager:show()
					message_view_manager:show_first_button( strings.email_first_button_text )
					message_view_manager:show_third_button( strings.email_second_button_text )
					kernel.registeractorhitnotify(
						"first_button",
						function()
							self:set_value( "" )
							self:raise_cancel_event()				
							return
						end )
					kernel.registeractorhitnotify(
						"third_button",
						function()
							view_goback_safe( "keyboardmanager:handle_actor_hit (third_button)" )
							self:set_value( "" )
							self:set_error( "" )
						end	)
					kernel.hideactor( "mature_rating_back_button_arrow" )
					message_view_manager:set_title( strings.email_confirmation_header_message )
					message_view_manager:set_message( strings.email_confirmation_body_message )
				else
					self:set_value( "" )
					self:raise_cancel_event()				
					return
				end
			end						

			if text ~= nil and kernel.getlasthitactortag() ~= "back"
			and string.len( text ) >= self.maximum_length then
				return
			end

			if last_hit_tag == "back" then
				if string.len( text ) > 0 then
					text = string.sub( text, 1, string.len( text ) - 1 )
					self:set_value( text )
				end
			else
				text = text .. last_hit_tag
				if string.len( text ) <= self.maximum_length then					
					self:set_value( text )
				end
			end
			
			if title_name == "PROMO CODE" or string.find( title_name, "PROMOCI" ) ~= nil then
				if  text ~= nil and string.len( text ) > 3 then
					kernel.enableactor("ok_button")
					kernel.showactor("image_next_right_arrow")
					kernel.hideactor("image_next_right_arrow_gray")
				else
					kernel.disableactor("ok_button")
					kernel.hideactor("image_next_right_arrow")
					kernel.showactor("image_next_right_arrow_gray")
				end
			end
			
			self:raise_actor_hit_event( text )
		end,

		get_value = function( self )
			return kernel.getmacro( "KeyBoard:Text" )
		end,

		set_value = function( self, value )
			kernel.setmacro( "KeyBoard:Text", value or "" )
			kernel.markactordirty( "text_label" )
			if TextToSpeechEnabled == true then
				kernel.ttsrepeat();  -- assests have been updated
			end
		end,

		get_error = function( self )
			return kernel.getmacro( "KeyBoard:ErrorLabel" )
		end,

		set_error = function( self, value )
			kernel.setmacro( "KeyBoard:ErrorLabel", value or "" )
			kernel.markactordirty( "error_label" )
			if TextToSpeechEnabled == true then
				kernel.ttsrepeat();  -- assests have been updated
			end
		end,

		get_message = function( self )
			return kernel.getmacro( "KeyBoard:MessageLabel" )
		end,

		set_message = function( self, value )
			kernel.setmacro( "KeyBoard:MessageLabel", value or "" )
			kernel.markactordirty( "message_label" )
		end,

		get_marketing = function( self )
			return kernel.getmacro( "KeyBoard:MarketingLabel" )
		end,

		set_marketing = function( self, value )
			kernel.setmacro( "KeyBoard:MarketingLabel", value or strings.email_standard_assurance )
			kernel.markactordirty( "marketing_label" )
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
				view_goback_safe( "keyboardmanager:raise_enter_event (enter_func = nil)" )
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
				view_goback_safe( "keyboardmanager:raise_cancel_event (cancel_event = nil)" )
			end
		end,

		set_cancel_button_text = function( self, value )
			kernel.setactortext( "cancel_button", value )
		end
	}
	return t
end
