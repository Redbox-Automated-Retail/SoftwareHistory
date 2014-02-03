--
-- keypad-controller.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newkeypadcontroller()

	local t = {}
	t.display_target = nil
	t.max_length = 0
	t.on_validate = nil
	t.on_keypress = nil
	t.on_show = nil
	t.last_selected_slot = nil
	t.last_slot_count = nil
	
	t.set_display_target = function(target, max_length)
		t.display_target = target
		t.max_length = max_length
		kernel.setmacro( "keyboard:" .. target, "" )
	end
	
	t.show = function(is_inventory_alert)
		if is_inventory_alert ~= nil then
			if is_inventory_alert == true then
				view_show("keypad_popup_alert_view")
			else
				view_show("keypad_popup_view")	
			end
		else
			view_show("keypad_popup_view")	
		end
		kernel.registerkeypressnotify(
			"slot_entry_handler",
			t.on_keypress or function(keystroke)
				if keystroke == "\r" then
					t.validate()
				else
					if keystroke == "\b" then
						keystroke = "back"
					elseif keystroke == " " then
						keystroke = "space"
					end
					local actor = kernel.getactorsbytag("keydata=" .. keystroke)
					if actor == nil then
						return
					end
					kernel.setlasthitactortag( kernel.getactortag(actor[1]) )
					t.update()
				end
			end)
		if t.on_show ~= nil then
			t.on_show()
		end	
	end
	
	t.update = function()
		if kernel.getlasthitactorname() == nil 
		or kernel.getlasthitactortag() == nil then
			return
		end
		local macro_name = "keyboard:" .. t.display_target
		local text = kernel.getmacro( macro_name )
		text = text or ""
		local key_data_begin, key_data_end = string.find( kernel.getlasthitactortag(), "keydata=")
		if key_data_begin == nil then
			return
		end
		local key_data = string.sub( 
								kernel.getlasthitactortag(), 
								key_data_begin+8, 
								string.len( kernel.getlasthitactortag() ))
		if string.len(text) + 1 > t.max_length 
		and (key_data ~= "clear" and key_data ~= "back") then
			return
		end
		if key_data == "clear" then
			kernel.setmacro( macro_name, "" )
			kernel.markactordirty( t.display_target )
		return
		elseif key_data == "space" then
			text = text .. " "
			kernel.setmacro( macro_name, text )
			kernel.markactordirty( t.display_target )
			return
		elseif key_data == "back" then
			if string.len( text ) > 0 then
				text = string.sub( text, 1, string.len( text ) - 1 )
				kernel.setmacro( macro_name, text )
				kernel.markactordirty( t.display_target )
			end
		else
			text = text .. key_data
			kernel.setmacro( macro_name, text )
			kernel.markactordirty( t.display_target )
		end
	end
	
	t.validate = function()
		local selected_slot = kernel.getmacro("keyboard:" .. t.display_target)
		if selected_slot == "" then
			newmessageboxmanager():show("Please enter a number")
			return
		end
		if t.on_validate ~= nil 
			and t.on_validate(selected_slot) == false then
			return			
		end
		t.last_selected_slot = selected_slot
		view_goback()	
	end
	
	t.keypad_validate_alert = function()
		local slot_count = kernel.getmacro("keyboard:" .. t.display_target)
		if t.on_validate ~= nil 
			and t.on_validate(slot_count) == false then
			return			
		end
		t.last_slot_count = slot_count
		view_goback()	
	end
	
	return t
end
	
	
