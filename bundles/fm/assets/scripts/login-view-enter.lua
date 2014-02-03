--
-- login-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

--check to see if service is available
if kernel.getservicestatus( "halsvc$default" ) ~= "Running" then
	kernel.activatedefaultbundle()
	return
end

upper_case_flag = false
active_actor = "login_textbox"
key_board_minimum_length = 20
lv_password = ""

kernel.registerkeypressnotify(
	"login_key_handler",
	function( keystroke )
		local upper_case_flag_save = upper_case_flag
		if keystroke == "\t" then
			if active_actor == "login_textbox" then
				active_actor = "login_textbox"
				kernel.setlasthitactorname( "password_textbox" )
			else
				active_actor = "password_textbox"
				kernel.setlasthitactorname( "login_textbox" )
			end
			kernel.executescript("activate_textbox")
		elseif keystroke == "\r" then
			kernel.executescript("authenticate")
		else
			kernel.setlasthitactorname( active_actor )
			if keystroke == "\b" then
				keystroke = "back"
			elseif keystroke == " " then
				keystroke = "space"
			else
				if string.upper(keystroke) == keystroke then
					upper_case_flag = true
					keystroke = string.lower(keystroke)
				end
			end
			local actor = kernel.getactorsbytag("keydata=" .. keystroke)
			if actor == nil then
				return
			end
			kernel.setlasthitactortag( kernel.getactortag(actor[1]) )
			kernel.executescript("keyboard_update")
			upper_case_flag = upper_case_flag_save
		end
	end )
