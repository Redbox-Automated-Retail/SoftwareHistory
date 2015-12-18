--
-- message-view-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newmessageviewmanager(message_view_name)

	local view_actor_prefix_map = {
		["in_cart_offer_message_view"] = "in_cart_offer_",
		["credit_message_view"]   = "credit_",
		["unplayable_return_confirm"] = "unplayable_"
	}

	local actor_prefix = view_actor_prefix_map[message_view_name] or ""

	local t = {
		name = message_view_name or "message_view",

		first_button_name = actor_prefix.. "first_button",

		second_button_name = actor_prefix.. "second_button",
		
		third_button_name = actor_prefix.. "third_button",
		
		fourth_button_name = actor_prefix.. "fourth_button",
		
		prompt_timer = actor_prefix.. "prompt_timer",

		show = function( self )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewTitle", "" )
				kernel.setmacro( "TTS:MessageViewText", "" )
				kernel.setmacro( "TTS:MessageViewButton1Text", "" )
				kernel.setmacro( "TTS:MessageViewButton2Text", "" )
				kernel.setmacro( "TTS:MessageViewButton3Text", "" )
			end
			if self.is_showing then
				return
			end
			self.is_showing = true
			if kernel.peekview() ~= self.name then
				view_show_safe( self.name )
			end
		end,

		hide = function( self, pop_only )
            if not self.is_showing then
                return
            end
			self.is_showing = false
			self:stop_timer()
            if kernel.peekview() == self.name then
				view_goback_safe( "messageviewmanager:hide()", pop_only )
            end
		end,

		set_idle_timer_title = function( self, text )
			kernel.hideactor( actor_prefix.. "title_label" )
			kernel.hideactor( actor_prefix.. "unplayable_message_header" )
			kernel.hideactor( actor_prefix.. "blacklist_message_header" )
			kernel.hideactor( "message_header_label" )
			kernel.hideactor( "third_button" )
			kernel.hideactor( "fourth_button" )
			kernel.showactor( actor_prefix.. "title_timeout_label" )
			kernel.setmacro( actor_prefix.. "MessageTimeoutTitle", text )
			kernel.markactordirty( actor_prefix.. "title_timeout_label" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewTitle", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
			
		end,
		
		set_title = function( self, text )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.showactor( actor_prefix.. "title_label" )
			kernel.setmacro( actor_prefix.. "MessageTitle", text )
			kernel.markactordirty( actor_prefix.. "title_label" )
			kernel.hideactor( actor_prefix.. "unplayable_message_header" )
			kernel.hideactor( "message_header_label" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewTitle", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		hide_title = function( self )
			if not self.is_showing then
				return
			end
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.hideactor( actor_prefix.. "unplayable_message_header" )
			kernel.hideactor( "title_label" )
			kernel.setactory( "message_label", 196 )
			kernel.setactory( "credit_message_label", 196 )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewTitle", "" )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		set_unplayable_message = function( self, text )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.hideactor( actor_prefix.. "title_label" )
			kernel.hideactor( "back_button_arrow" )			
			kernel.showactor( actor_prefix.. "unplayable_message_header" )
			kernel.setmacro( actor_prefix.. "UnplayableMessageLabel", text )
			kernel.markactordirty( actor_prefix.. "unplayable_message_header" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewText", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		set_blacklist_message = function( self, text )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.hideactor( "back_button_arrow" )
			kernel.showactor( actor_prefix.. "blacklist_message_header" )
			kernel.setmacro( actor_prefix.. "BlacklistMessageLabel", text )
			kernel.markactordirty( actor_prefix.. "blacklist_message_header" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewText", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		set_message_header = function( self, text )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			--kernel.hideactor( actor_prefix.. "message_label" )
			kernel.hideactor( "message_label_text" )
			kernel.showactor( "message_header_label" )
			kernel.setmacro( "MessageHeaderText", text )
			kernel.hideactor( actor_prefix.. "unplayable_message_header" )
			kernel.markactordirty( "message_header_label" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewTitle", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,		
		
		set_message_text = function( self, text )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.hideactor( actor_prefix.. "message_label" )
			--kernel.hideactor( "message_header_label" )
			kernel.showactor( "message_label_text" )
			kernel.setmacro( "MessageLabelText", text )
			kernel.markactordirty( "message_label_text" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewText", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		set_message = function( self, text )
			kernel.hideactor( "message_label_text" )
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.showactor( actor_prefix.. "message_label" )
			kernel.setmacro( actor_prefix.. "MessageText", text )
			kernel.markactordirty( actor_prefix.. "message_label" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewText", text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		hide_message = function( self )
			if not self.is_showing then
				return
			end
			kernel.hideactor( actor_prefix.. "title_timeout_label" )
			kernel.hideactor( "back_button_arrow" )
			kernel.hideactor( actor_prefix.. "message_label" )
			kernel.setactory( actor_prefix.. "title_label", 210 )
			kernel.hideactor( "message_header_label" )
			kernel.hideactor( "message_label_text" )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewText", "" )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,

		start_timer = function( self, expired_func, timeout )
			if not self.is_showing then
				return
			end
			if kernel.timerexists( self.prompt_timer ) then
				return
			end
			timeout = timeout or 15000
			if TextToSpeechEnabled == true then
				kernel.ttsrepeat();  -- the titles and assests have been set and toggled
				timeout = TTSTimeoutMessageView
			end
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

		show_first_button = function( self, button_text, func )
			if not self.is_showing then
				return
			end
			kernel.registeractorhitnotify(
				self.first_button_name,
				function()
					self:stop_timer()
					if func ~= nil then
						func()
					end
                    self:hide()
				end )
			kernel.showactor( self.first_button_name )
			kernel.setactortext( self.first_button_name, button_text )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewButton1Text", button_text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		hide_first_button = function( self )
			if not self.is_showing then
				return
			end
			kernel.hideactor( self.first_button_name )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewButton1Text", "" )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,

		show_second_button = function( self, button_text, func )
			if not self.is_showing then
				return
			end
			kernel.registeractorhitnotify(
				self.second_button_name,
				function()
					self:stop_timer()
					if func ~= nil then
						func()
					end
                    self:hide()
				end )
			kernel.showactor( self.second_button_name )
			kernel.setactortext( self.second_button_name, button_text )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewButton2Text", button_text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		show_third_button = function( self, button_text, func )
			if not self.is_showing then
				return
			end
			kernel.registeractorhitnotify(
				self.third_button_name,
				function()
					self:stop_timer()
					if func ~= nil then
						func()
					end
                    self:hide()
				end )
			kernel.showactor( self.third_button_name )
			kernel.setactortext( self.third_button_name, button_text )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewButton3Text", button_text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		show_fourth_button = function( self, button_text, func )
			if not self.is_showing then
				return
			end
			kernel.registeractorhitnotify(
				self.fourth_button_name,
				function()
					self:stop_timer()
					if func ~= nil then
						func()
					end
                    self:hide()
				end )
			kernel.showactor( self.fourth_button_name )
			kernel.setactortext( self.fourth_button_name, button_text )
			if TextToSpeechEnabled == true then
				kernel.setmacro( "TTS:MessageViewButton4Text", button_text )
				kernel.ttsrepeat();  -- assests have been set and toggled
			end
		end,
		
		show_back_button_arrow = function( self, func )
			if not self.is_showing then
				return
			end
			kernel.registeractorhitnotify(
				"back_button_arrow",
				function()
					self:stop_timer()
					if func ~= nil then
						func()
					end
					self:hide()
				end )
			kernel.showactor( "back_button_arrow" )
		end,
		
		is_showing = false
	}
	return t
end
