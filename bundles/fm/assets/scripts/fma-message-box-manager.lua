local vmz_messagebox_is_active = false

function newfmamessageboxmanager()

    local background_actor = "messagebox_background"
    local frame_actor = "messagebox_frame"
    local header_actor = "messagebox_header_field"
    local message_actor = "messagebox_text_field"
    local button_bar_ok_actor = "messagebox_button_bar_ok"
	local cancel_button = "messagebox_cancel"

	local header_height = 60
	local frame_margin = 120
	local frame_padding = 35
	local button_width = 180
	local button_height = 80
	
	local _callbacks = nil
	local _button_ok = "OK"
	local _button_cancel = "CANCEL"

	local count = 0

	local validate_text = function( text )
        if text == nil then text = "" end
        return text
    end

	local create_button = function( text, id)
		x = (1024 - button_width) / 2
		y = 768 - frame_padding - frame_margin - button_height
		kernel.createactor(
			button_bar_ok_actor, false,
			x, y, button_width,	button_height,
			text, "medium_large_font",
			nil,
			"DarkRed", "White",
			nil, nil, nil, nil,
			"Text, Background, DrawBorder, DrawRoundedCorners, TextShadow",
			"Enabled")
		kernel.registeractorhitnotify( button_bar_ok_actor, function() do_callback( _button_ok ) end )
		kernel.setactortextalignx( button_bar_ok_actor, "center" )
		kernel.setactortextaligny( button_bar_ok_actor, "center" )
		kernel.setcornersize( button_bar_ok_actor, 15, 15 )
		kernel.setborderstrokeweight( button_bar_ok_actor, 3 )
		kernel.setbordercolor( button_bar_ok_actor, "LightGray" )
	end

    local create_background = function( )
        kernel.createactor(
            background_actor, false,
            0, 0, 1024, 768,
            nil, nil, nil,
            "black",
            nil, nil, nil, nil, nil,
            "Background",
			"Obstruction")
        kernel.setactoropacity( background_actor, 0.50 )

		local x = frame_margin
		local y = frame_margin
		local width = 1024 - (2 * frame_margin)
		local height = 768 - (2 * frame_margin)
        kernel.createactor(
            frame_actor, false,
            x, y, width, height,
            nil, nil, nil,
            "White",
            nil, nil, nil, nil, nil,
            "Background, DrawBorder, DrawRoundedCorners",
            "Enabled")
        kernel.setcornersize( frame_actor, 15, 15 )
		kernel.setborderstrokeweight( frame_actor, 3 )
		kernel.setbordercolor( frame_actor, "DarkRed" )
    end

	local create_cancel_button = function ()
		-- Display the cancel button
		local x = 1024 - frame_margin - 20 - 5
		local y = frame_margin - 21 + 5
		local width = 40
		local height = 42
		kernel.createactor(
			cancel_button, false,
			x, y, width, height,
            nil, nil,
			"image_x_out_button",
			nil, nil, nil, nil, nil, nil,
            "Image",
            "Enabled")
		kernel.registeractorhitnotify( cancel_button, function() do_callback( _button_cancel ) end )
	end
	
    local create_header_field = function( header_text )
		local x = frame_margin + frame_padding
		local y = frame_margin + (frame_padding / 2)
		local width = 1024 - (2 * (frame_margin + frame_padding))
		local height = header_height
        kernel.createactor(
            header_actor, false,
            x, y, width, height,
            header_text,
            "medium_larger_font",
            nil,
            "White",
            "DarkRed",
            nil, nil, nil, nil,
            "Text, Background",
            "Obstruction")
        kernel.setactortextalignx( message_actor, "left" )
        kernel.setactortextaligny( message_actor, "top" )
    end

    local create_text_field = function( message_text )
		local x = frame_margin + frame_padding
		local y = frame_margin + frame_padding + header_height + frame_padding
		local width = 1024 - (2 * (frame_margin + frame_padding))
		local height = 768 - (2 * (frame_margin + frame_padding)) - header_height - frame_padding
        kernel.createactor(
            message_actor, false,
            x, y, width, height,
            message_text,
            "font_target_small",
            nil,
            "White",
            "black",
            nil, nil, nil, nil,
            "Text, Background",
            "Obstruction")
        kernel.setactortextalignx( message_actor, "left" )
        kernel.setactortextaligny( message_actor, "top" )
    end

	local hide_message_box = function()
		kernel.removeactor( header_actor )
		kernel.removeactor( message_actor )
		kernel.removeactor( button_bar_ok_actor )
		kernel.removeactor( frame_actor )
		kernel.removeactor( background_actor )
		kernel.removeactor( cancel_button )
		
		vmz_messagebox_is_active = false
	end
	
	do_callback = function( id )
		hide_message_box()
		if _callbacks == nil then return end
		local callback = _callbacks[id]
		if callback ~= nil then callback() end
	end
	
	local t = {
		max_count = 10,
		
        show = function( self, header_text, message_text, button_text, cancelable, callbacks )
			self.count = 0
			
			if vmz_messagebox_is_active then
				-- end the current message box
				do_callback( _button_ok )
			end
			
            vmz_messagebox_is_active = true
            local message_text  = validate_text( message_text )
			_callbacks = callbacks

            create_background()
            create_header_field( header_text )
            create_text_field( message_text )
			
			if cancelable then
				create_cancel_button()
			end
			
			if button_text ~= nil then
				create_button( button_text, _callback )
			end
			
        end,

		hide = function( self )
			hide_message_box()
		end,

		update_message = function( self, message_text ) 
			kernel.setactortext( message_actor, validate_text(message_text) )
		end,
		
		increment_count = function( self )
			if self.count < tonumber( self.max_count ) then
				self.count = self.count + 1
				self:update_message( "Disc " .. self.count .. " of " .. self.max_count )
			end
		end,
		
		perform_callback = function( self, id )
			do_callback( id )
		end,
		
		-- just to support luaunit and faking click of button
		testing_click = function( self, id )
			do_callback( id )
		end

	}

    return t
end
