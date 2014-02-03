--
-- obj-message-box-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local messagebox_is_active = false

function newmessageboxmanager()

    local background_actor = "messagebox_background"
    local frame_actor = "messagebox_frame"
    local message_actor = "messagebox_text_field"
    local button_bar_ok_actor = "messagebox_button_bar_ok"
    local button_bar_cancel_actor = "messagebox_button_bar_cancel"
    local button_bar_yes_actor = "messagebox_button_bar_yes"
    local button_bar_no_actor = "messagebox_button_bar_no"
    local icon_actor = "messagebox_icon_field"

	local frame_padding = 10
	local button_width = 72
	local button_height = 32

	local _button_ok = "OK"
	local _button_cancel = "CANCEL"
	local _button_yes = "YES"
	local _button_no = "NO"

	local _button_type_ok        = "OK"
	local _button_type_ok_cancel = "OKCANCEL"
	local _button_type_yes_no    = "YESNO"
	local _button_type_set       = {}
	_button_type_set[_button_type_ok]        = true
	_button_type_set[_button_type_ok_cancel] = true
	_button_type_set[_button_type_yes_no]    = true

	local create_button = function(
			actor_name,
			x,
			y,
			text,
			id)
		kernel.createactor(
			actor_name,
			false,
			x,
			y,
			button_width,
			button_height,
			text,
			"medium_font",
			nil,
			"gray",
			"black",
			nil,
			nil,
			nil,
			nil,
			"Text, Background, DrawBorder, DrawRoundedCorners",
			"Enabled")
		kernel.registeractorhitnotify( actor_name, function() do_callback( id ) end )
		kernel.setactortextalignx( actor_name, "center" )
		kernel.setactortextaligny( actor_name, "center" )
		kernel.setcornersize( actor_name, 5, 5 )
	end

	local create_button_bar_functions = {}

	create_button_bar_functions[_button_type_ok] = function()
		create_button( button_bar_ok_actor, 0, (768-button_height), "OK", _button_ok )
	end

	create_button_bar_functions[_button_type_ok_cancel] = function()
		create_button( button_bar_ok_actor, 0, (768-button_height), "OK", _button_ok )
		create_button( button_bar_cancel_actor, 100, (768-button_height), "Cancel", _button_cancel )
	end

	create_button_bar_functions[_button_type_yes_no] = function()
		create_button( button_bar_yes_actor, 200, (768-button_height), "Yes", _button_yes )
		create_button( button_bar_no_actor, 300, (768-button_height), "No", _button_no )
	end

	local layout_button_bar_functions = {}

	layout_button_bar_functions[_button_type_ok] = function( box_start_x, box_start_y, box_width, message_height )
		local button_ok_x = box_start_x + ((box_width - button_width)/2)
		local button_ok_y = box_start_y + message_height
		kernel.setactorlocation( button_bar_ok_actor, button_ok_x, button_ok_y )
	end

	layout_button_bar_functions[_button_type_ok_cancel] = function( box_start_x, box_start_y, box_width, message_height )
		local button_ok_x = box_start_x + ((box_width - button_width)/4)
		local button_ok_y = box_start_y + message_height
		kernel.setactorlocation( button_bar_ok_actor, button_ok_x, button_ok_y )
		local button_cancel_x = box_start_x + ((3*(box_width - button_width))/4)
		local button_cancel_y = box_start_y + message_height
		kernel.setactorlocation( button_bar_cancel_actor, button_cancel_x, button_cancel_y )
	end

	layout_button_bar_functions[_button_type_yes_no] = function( box_start_x, box_start_y, box_width, message_height )
		local button_yes_x = box_start_x + ((box_width - button_width)/4)
		local button_yes_y = box_start_y + message_height
		kernel.setactorlocation( button_bar_yes_actor, button_yes_x, button_yes_y )
		local button_no_x = box_start_x + ((3*(box_width - button_width))/4)
		local button_no_y = box_start_y + message_height
		kernel.setactorlocation( button_bar_no_actor, button_no_x, button_no_y )
	end

	local _icon_type_none     = "NONE"
    local _icon_type_info     = "INFO"
    local _icon_type_question = "QUESTION"
    local _icon_type_error 	  = "ERROR"
	local _icon_type_set      = {}
	_icon_type_set[_icon_type_none]     = true
	_icon_type_set[_icon_type_info]     = true
	_icon_type_set[_icon_type_question] = true
	_icon_type_set[_icon_type_error]    = true

	local _callbacks = nil

	local validate_message_text = function( text )
        if text == nil then text = "" end
        return text
    end

    local create_background = function( )
        kernel.createactor(
            background_actor,
            false,
            0,
            0,
            1024,
            768,
            nil,
            nil,
            nil,
            "black",
            nil,
            nil,
            nil,
            nil,
            nil,
            "Background",
            "Obstruction")
        kernel.setactoropacity( background_actor, 0.50 )

        kernel.createactor(
            frame_actor,
            false,
            0,
            0,
            1024,
            768,
            nil,
            nil,
            nil,
            "white",
            nil,
            nil,
            nil,
            nil,
            nil,
            "Background, DrawBorder, DrawRoundedCorners",
            "Enabled")
        kernel.setcornersize( frame_actor, 5, 5 )
    end

    local create_text_field = function( message_text )
        local calculate_width_height = function( size, increment )
            local length_to_height = 0.30
            size.width = size.width + (increment == nil and 0 or increment)
            size["height"] = size.width * length_to_height
            return size
        end

        local default_width = 300
        local increment_width = 100

        local size = {}
        size["width"] = default_width
        size = calculate_width_height( size )
        kernel.createactor(
            message_actor,
            false,
            0,
            0,
            size.width,
            size.height,
            message_text,
            "medium_font",
            nil,
            "white",
            "black",
            nil,
            nil,
            nil,
            nil,
            "Text, Background",
            "Obstruction")
        kernel.setactortextalignx( message_actor, "center" )
        kernel.setactortextaligny( message_actor, "center" )

        local fits = false
        repeat
            local results = kernel.measureactortext( message_actor )
            if results == nil then break end
            if results.characters_fitted == string.len( message_text ) then
                fits = true
            else
                size = calculate_width_height( size, increment_width )
                kernel.setactorwidth( message_actor, size.width )
                kernel.setactorheight( message_actor, size.height )
            end
        until fits == true or size.width >= 900
    end

    local create_base_icon_field = function( width, height, icon )
        kernel.createactor(
            icon_actor,
            false,
            (1024-width),
            (0),
            width,
            height,
            nil,
            nil,
            icon,
            nil,
            nil,
            nil,
            nil,
            nil,
            nil,
            "Image",
            "Enabled")
    end

	local validate_icon_format = function( icon_format )
		if icon_format == nil then icon_format = _icon_type_info end
		if _icon_type_set[icon_format] then return icon_format end
		kernel.showmessage(
			"Invalid icon format: " ..
			icon_format ..
			" passed to newmessagebox:start(); please correct.")
		return _icon_type_none
    end

	local create_icon_field = function( icon_format )
		if icon_format == _icon_type_none then
			create_base_icon_field( 0, 0, nil )
		elseif icon_format == _icon_type_info then
			create_base_icon_field( 64, 64, "messagebox_information_icon" )
		elseif icon_format == _icon_type_question then
			create_base_icon_field( 64, 64, "messagebox_question_icon" )
		elseif icon_format == _icon_type_error then
			create_base_icon_field( 64, 64, "messagebox_error_icon" )
		else
			kernel.showmessage(
				"Invalid icon format: " ..
				tostring(icon_format) ..
				" passed to create_icon_field(); please correct.")
		end
	end

	local validate_button_format = function( button_format )
		if button_format == nil then button_format = _button_type_ok end
		if _button_type_set[button_format] then return button_format end
		kernel.showmessage(
			"Invalid button format: " ..
			button_format ..
			" passed to newmessagebox:start(); please correct.")
		return _button_type_ok
	end

	local create_button_bar = function( button_type )
		create_button_bar_functions[button_type]()
	end

	local do_layout_text_icon = function()
		local message_width = kernel.getactorwidth( message_actor )
		local message_height = kernel.getactorheight( message_actor )

		local icon_width = kernel.getactorwidth( icon_actor )
		local icon_height = kernel.getactorheight( icon_actor )

		local box_width = message_width + icon_width + frame_padding
		local upper_height = message_height > icon_height and message_height or icon_height
		local box_height = upper_height + button_height + frame_padding

		local offsets = kernel.getviewframe( )
		if offsets == nil then offsets = { x = 0, y = 0 } end

		local box_start_x = ((1024-box_width-frame_padding)/2) - offsets.x
		local box_start_y = ((768-box_height-frame_padding)/2) - offsets.y

		kernel.setactorlocation( background_actor, 0 - offsets.x, 0 - offsets.y )

		kernel.setactorlocation( frame_actor, box_start_x, box_start_y )
		kernel.setactordimension( frame_actor, box_width, box_height )

		local message_x = box_start_x + (frame_padding/2)
		local message_y = box_start_y + (upper_height-message_height)/2 + (frame_padding/2)
		kernel.setactorlocation( message_actor, message_x, message_y )

		local icon_x = box_start_x + message_width
		local icon_y = box_start_y + (upper_height-icon_height)/2
		kernel.setactorlocation( icon_actor, icon_x, icon_y )

		return box_start_x, box_start_y, box_width, message_height
	end

	local do_layout = function( button_type )
		layout_button_bar_functions[button_type]( do_layout_text_icon() )
	end

	do_callback = function( id )
		kernel.removeactor( message_actor )
		kernel.removeactor( icon_actor )
		kernel.removeactor( button_bar_ok_actor )
		kernel.removeactor( button_bar_cancel_actor )
		kernel.removeactor( button_bar_yes_actor )
		kernel.removeactor( button_bar_no_actor )
		kernel.removeactor( frame_actor )
		kernel.removeactor( background_actor )
		messagebox_is_active = false
		if _callbacks == nil then return end
		local callback = _callbacks[id]
		if callback ~= nil then callback() end
	end

	local t = {
        ButtonTypeOK       = _button_type_ok,
        ButtonTypeOKCANCEL = _button_type_ok_cancel,
        ButtonTypeYESNO    = _button_type_yes_no,

		ButtonOK     = _button_ok,
		ButtonCANCEL = _button_cancel,
		ButtonYES    = _button_yes,
		ButtonNO     = _button_no,

		IconTypeNone     = _icon_type_none,
        IconTypeInfo     = _icon_type_info,
        IconTypeQuestion = _icon_type_question,
        IconTypeError    = _icon_type_error,

        show = function( self, message_text, button_type, icon_format, callbacks )
			if messagebox_is_active then return end
            messagebox_is_active = true
            local message_text  = validate_message_text( message_text )
            local button_type = validate_button_format( button_type )
            local icon_format = validate_icon_format( icon_format )
			_callbacks = callbacks

            create_background()
            create_text_field( message_text )
            create_icon_field( icon_format )
            create_button_bar( button_type )
            do_layout( button_type )
        end,

		-- just to support luaunit and faking click of button
		testing_click = function( self, id )
			do_callback( id )
		end
	}

    return t
end
