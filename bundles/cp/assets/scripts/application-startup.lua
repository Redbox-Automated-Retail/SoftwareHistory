--
-- application-startup.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

config = kernel.getdictionary( 'config' )
strings = kernel.getdictionary( 'strings' )

StoreNumber = kernel.getmachinevalueordefault( "Store", "ID", "N/A" )
VerboseLogging = kernel.getmachinevalueordefault( "Store", "VerboseLogging", "False" ) == "True"

kernel.registerconfigurationcallback(
	"Watch-StoreID",
	function( store, action, path, key, value )
		if store == "machine"
		and action == "set"
		and path == "Store"
		and key == "ID" then
			kernel.log( "Configuration value changed for StoreNumber: " .. value )
			StoreNumber = value
			kernel.setmacro( "StoreNumber", StoreNumber )
		end
	end )
kernel.setmacro( "StoreNumber", StoreNumber )

kernel.registercanshutdown(
	function()
		kernel.log( "Control Panel Application" )
		kernel.log( ">  Kiosk Engine is requesting a shut-down" )
		return false
	end )

function initialize()
	ActivityManager = newactivitymanager()
	ActivityManager.rundlls = kernel.listpidrundll32s()
	KeyNavigationManager = newkeynavigationmanager()
	kernel.timercreate(
		'date_time_refresh',
		1000,
		1000,
		function()
			local date_time = os.date( "*t" )
			kernel.setmacro(
				"Time",
				string.format( "%02d:%02d:%02d", date_time.hour, date_time.min, date_time.sec ) )
			kernel.markactordirty( "time" )
		end )
	kernel.timerstart( "date_time_refresh" )

	local now = os.date( "*t" )
	kernel.setmacro(
		"Date",
		string.format( "%02d/%02d/%4d", now.month, now.day, now.year ) )
	kernel.setmacro( "TimeZone", kernel.gettimezone() )

	idle_timer = newidletimer()
	idle_timer:initialize(
		600000,
		function()
			kernel.log("Idle timer on_timer() executing.")
			local okay_to_switch = true
			
			local running_processes = kernel.listrunningprocesses()
			if running_processes ~= nil then
				for k, v in pairs( running_processes ) do
					--kernel.log( "> " .. v.name )
					if string.find( string.upper( v.name ), "HALMGMTC" ) ~= nil 
					or string.find( string.upper( v.name ), "MSHALTESTER" ) ~= nil then
						kernel.log( v.name .. " is running, app close on idle timer aborted.")
						okay_to_switch = false
					end
				end
			end

			if okay_to_switch == true then
				--kernel.log( "Idle timer expired, switching to Rental Application." )
				kernel.logto( "UserLoginLog", "Control Panel | Session timed out" )
				ActivityManager:close_control_panel()
			else
				kernel.timerstart("idle_timer")
			end
		end )

	view_show( 'control_panel_view' )
end

kernel.showlogin(
	"Redbox Control Panel Login",
	300000,
	function( user_data )
		local function successful_login( user_id, password, <redacted>, is_online )
			kernel.healthsvcs_sendauditevent(
				StoreNumber,
				"CP_Login",
				user_id,
				"CP Bundle",
				audit_message )
			kernel.logto( "UserLoginLog", "Control Panel | LOGIN SUCCESS for user : ".. user_id  .. ", online = " .. tostring(is_online) )
			kernel.logto( "ControlPanelActionLog", "Control Panel | LOGIN SUCCESS for user : ".. user_id  .. ", online = " .. tostring(is_online) )
			kernel.setloginresult( "OK" )
			if is_online then
				if not kernel.securityuserexists( user_id ) then
					kernel.securitycreateuser( user_id, password, 14 )
				else
					kernel.securityuserslideexpirydate( user_id, 14 )
				end
			end
			initialize()
		end
		local function failed_login( user_id, response )
			kernel.logto( "UserLoginLog", "Control Panel | LOGIN FAILED, reason: " .. response )
			kernel.logto( "ControlPanelActionLog", "Control Panel | LOGIN FAILED, reason: " .. response )
			--kernel.log(
			--	"Control Panel Authentication failed for user: "..
			--	user_id ..
			--	" with reason code: " .. response )
			if response == "MissingUser" then
				kernel.setloginresult( "The specified user account does not exist." )
			elseif response == "AuthenticationFailed" then
				kernel.setloginresult( "The user account could not be authenticated." )
			elseif response == "Error" then
				kernel.setloginresult( "There was an error while authenticating, please try again." )
			end
		end
		kernel.authsvcs_authenticateuser(
			StoreNumber,
			user_data.user_id,
			user_data.password,
			true,
			function( r )
				CurrentUser = {
					id = user_data.user_id,
					login_time = os.date( "*t" )
				}
				if r.success then
					successful_login(
						user_data.user_id,
						user_data.password,
						"Successful login into the Control Panel Application bundle.",
						true )
				else
					if not r.properties.is_online
					or r.properties.stand_alone then
						if not kernel.securityuserexists( user_data.user_id ) then
							successful_login(
								user_data.user_id,
								user_data.password,
								"Successful login into the Control Panel Application bundle via backdoor.",
								false )
						else
							if kernel.securityauthenticate( user_data.user_id, user_data.password ) <redacted>
								successful_login(
									user_data.user_id,
									user_data.password,
									"Successful login into the Control Panel Application bundle using local account.",
									true )
							else
								failed_login( user_data.user_id, "AuthenticationFailed" )
							end
						end
					else
						failed_login( user_data.user_id, r.properties.response )
					end
				end
			end )
	end,
	function()
		kernel.logto( "UserLoginLog", "Control Panel | Login cancel button pressed" )
		kernel.activatedefaultbundle()
	end )

