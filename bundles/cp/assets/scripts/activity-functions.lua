--
-- activity-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newactivitymanager()
	local t = {}

	t.rundlls = {}

	t.open_processes = {}

	t.maintenance_mode_enabled = false

    t.messagebox = newmessageboxmanager()

	t.execute = function( self, name, parms )
		local func = self.activities[name]
		if func == nil then
			self.messagebox:show( "The specified activity " .. name .. " does not exist." )
			return
		end
		
		-- Audit for 'configure_kiosk' or 'open_kiosk' will be sent only after user has finished kiosk configuration.
		if name ~= "configure_kiosk" then
			kernel.healthsvcs_sendauditevent(
				StoreNumber,
				name,
				CurrentUser.id,
				"CP Bundle",
				"Activate Control Panel function." )
		end
		func( self, parms )
	end

	t.launch_external_process = function( self, processname, arguments )
		local pid = kernel.startprocess( processname , arguments, false )
		if pid >= 0 then
			self.open_processes[pid] = processname
		end
	end
	
	t.get_system32_path = function( self, fileName )
		return os.getenv("windir") .. "\\system32\\" .. fileName
	end
	
	t.close_control_panel = function( self )
		kernel.closesecurebrowser()
		t.kill_open_processes(self)
		idle_timer:stop()
		kernel.timerstop( "date_time_refresh" )
		kernel.activatedefaultbundle()
	end

	t.update_watch_dog_status = function( self )
		local is_enabled = kernel.getmachinevalueordefault( "Store", "WatchDogEnabled", "true" ) == "true"
		if is_enabled then
			kernel.setactortext( "watch_dog_toggle", "Turn Off\nWatch Dog" )
		else
			kernel.setactortext( "watch_dog_toggle", "Turn On\nWatch Dog" )
		end
	end

	t.update_touch_screen_status = function( self )
		local status = kernel.touchscreenstatus()
		if status == "Enabled" then
			kernel.setactortext( "toggle_touch_screen", "Turn Off\nTouchscreen" )
		elseif status == "Disabled" then
			kernel.setactortext( "toggle_touch_screen", "Turn On\nTouchscreen" )
		elseif status == "Unavailable" then
			kernel.setactortext( "toggle_touch_screen", "Touchscreen\nUnavailable" )
		end
	end

	t.update_screen_saver_status = function( self )
		if kernel.screensaverisactive() then
			kernel.setactortext( "toggle_screen_saver", "Turn Off\nScreen Saver" )
		else
			kernel.setactortext( "toggle_screen_saver", "Turn On\nScreen Saver" )
		end
	end

	t.update_elo_pcap_status = function( self )
		local driver_control = kernel.touchscreendriver()
		if driver_control == "" or not kernel.doesfileexist( driver_control ) then
			kernel.hideactor( "elo_pcap_control" )
		end
	end

	t.update_out_of_order_status = function( self )
		if kernel.getmachinevalueordefault( "Store", "OutOfOrder", "false" ) == "true" then
			kernel.setactortext( "toggle_out_of_order", "Turn Off\nOut of Order" )
			kernel.showactor( "out_of_order_message" )
		else
			kernel.setactortext( "toggle_out_of_order", "Turn On\nOut of Order" )
			kernel.hideactor( "out_of_order_message" )
		end
	end

	t.update_hal_service_status = function( self )
		if kernel.getservicestatus( "halsvc$default" ) == "Running" then
			kernel.timerstop( "hal_status_timer" )
			kernel.timerremove( "hal_status_timer" )
			kernel.setactortext( "toggle_hal_service", "Stop HAL\nService" )
			kernel.enableactor( "toggle_hal_service" )
		elseif kernel.getservicestatus( "halsvc$default" ) == "Stopped" then
			kernel.timerstop( "hal_status_timer" )
			kernel.timerremove( "hal_status_timer" )
			kernel.setactortext( "toggle_hal_service", "Start HAL\nService" )
			kernel.enableactor( "toggle_hal_service" )
		elseif kernel.getservicestatus( "halsvc$default" ) == "StartPending" then
			kernel.setactortext( "toggle_hal_service", "Starting HAL\nService" )
			kernel.disableactor( "toggle_hal_service" )
		elseif kernel.getservicestatus( "halsvc$default" ) == "StopPending" then
			kernel.setactortext( "toggle_hal_service", "Stopping HAL\nService" )
			kernel.disableactor( "toggle_hal_service" )
		else
			kernel.disableactor( "toggle_hal_service" )
		end
	end

	t.update_close_kiosk_status = function( self )
		local running_processes = kernel.listrunningprocesses()
		if running_processes ~= nil then
			for _, v in pairs( running_processes ) do
				if v.name == "halsvc-win32" then
					kernel.setactorforegroundcolor( "close_kiosk", "White" )
					kernel.enableactor( "close_kiosk" )
					return
				end
			end
		end
		kernel.setactorforegroundcolor( "close_kiosk", "Gray" )
		kernel.disableactor( "close_kiosk" )
	end

	t.kill_open_processes = function(self)
		for k, v in pairs( self.open_processes ) do
			if string.find( string.upper( v ), "HALMGMTC.EXE" ) == nil 
			and string.find( string.upper( v ), "MSHALTESTER.EXE" ) == nil then
				kernel.log( "kill_open_process " .. v )
				kernel.starthiddenprocess( "taskkill.exe" , "/F /T /PID " .. k )
			end
		end
		kernel.log("CHECK FOR RUNDLL32 PROCESSES")
		rundll32current = kernel.listpidrundll32s()
		--kill running programs using rundll32
		for _,v1 in pairs( rundll32current ) do
			exists = false
			--search ignore list, created when entering CP
			for _,v2 in pairs( self.rundlls ) do
				if v1 == v2 then
					exists = true
					break
				end
			end
			if exists == false then
				kernel.log( "kill_open_process rundll32.exe")
				kernel.starthiddenprocess( "taskkill.exe", "/F /T /PID " .. v1 )
			end
		end
	end

	t.activities = {}

	t.activities.logout = function( self, parms )
		self:close_control_panel()
	end

	t.activities.start_kiosk = function( self, parms )
		self:close_control_panel()
	end

	t.activities.computer_management = function( self, parms )
		self:launch_external_process( "mmc.exe", self:get_system32_path("compmgmt.msc") )
	end

	t.activities.display = function( self, parms )
		self:launch_external_process( "control.exe", "desk.cpl" )
	end

	t.activities.date_time = function( self, parms )
		self:launch_external_process( "control.exe", "timedate.cpl" )
	end

	t.activities.device_manager = function( self, parms )
		self:launch_external_process( "mmc.exe", self:get_system32_path("devmgmt.msc") )
	end

	t.activities.event_viewer = function( self, parms )
		self:launch_external_process( "mmc.exe", self:get_system32_path("eventvwr.msc") )
	end

	t.activities.network = function( self, parms )
		self:launch_external_process( "explorer.exe", "::{7007ACC7-3202-11D1-AAD2-00805FC1270E}" )
	end

	t.activities.add_remove_programs = function( self, parms )
		self:launch_external_process( "control.exe", "appwiz.cpl" )
	end

	t.activities.kiosk_engine_logs_folder = function( self, parms )
		self:launch_external_process( "explorer.exe",  "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine\\logs" )
	end

	t.activities.open_c_drive = function( self, parms )
		self:launch_external_process( "explorer.exe",  "C:\\" )
	end

	t.activities.redbox_directory = function( self, parms )
		self:launch_external_process( "explorer.exe",  "C:\\Program Files\\Redbox" )
	end

	t.activities.command_prompt = function( self, parms )
		self:launch_external_process( "cmd.exe", "/K cd c:\\program files" )
	end

	t.activities.display_config = function( self, parms )
		self:launch_external_process( "control.exe", "desk.cpl" )
	end

	t.activities.notepad= function( self, parms )
		self:launch_external_process( "notepad.exe", nil )
	end

	t.activities.open_hal_service_logs = function( self, parms )
		self:launch_external_process( "explorer.exe", "C:\\Program Files\\Redbox\\KioskLogs" )
	end

	t.activities.open_kaseya_config = function( self, parms )
		local kaseya_path = kernel.storesvcs_getkaseyainipath()
		if kaseya_path ~= nil then
			self:launch_external_process( "notepad.exe", kaseya_path )
		end
	end

	t.activities.open_camera_directory = function( self, parms )
		local hal_path = kernel.getservicepath( "HalSvc$Default" )
		if hal_path == nil then
			messagebox:show("Failed to locate HAL install path")
			return
		end
		local path = kernel.getfullpath( hal_path .. "\\..\\video")
		if path == nil then
			messagebox:show( "Could not build path to video directory")
			return
		end
		self:launch_external_process( "explorer.exe", path )
	end

	t.activities.elo_pcap = function( self, parms )
		local driver = kernel.touchscreendriver()
		self:launch_external_process(
		   kernel.getfilenamefrompath(driver),
		   driver )
	end

	t.activities.hyper_terminal = function( self, parms )
		self:launch_external_process( "c:\\program files\\Windows NT\\hypertrm.exe", "" )
	end

	t.activities.secure_browser = function( self, parms )
		kernel.launchsecurebrowser(
			kernel.gettable( "approved_urls" ),
			function( browse_type, url )
				kernel.healthsvcs_sendauditevent(
					StoreNumber,
					browse_type,
					CurrentUser.id,
					"CP_SecureBrowser",
					"URL: " .. url )
			end )
	end

	t.activities.reboot = function( self, parms )
		local messagebox = newmessageboxmanager()
		callbacks = {}
		callbacks[messagebox.ButtonYES] = function()
			kernel.startprocess( "shutdown.exe", "-r -t 0", true )
		end
		messagebox:show(
			"Are you sure you want to reboot the kiosk?",
			messagebox.ButtonTypeYESNO,
			messagebox.IconTypeQuestion,
			callbacks )
	end

	t.activities.control_panel = function( self, parms )
		self:launch_external_process( "control.exe", "" )
	end

	t.activities.ms_hal_tester = function( self, parms )
		local ms_hal_tester_path = "C:\\Program Files\\Redbox\\MS HAL Tester\\bin"
		local directory_exists = kernel.doesdirectoryexist(ms_hal_tester_path)
		if(directory_exists == false) then
			local messagebox = newmessageboxmanager()
			messagebox:show( "Could not find MSHALTester directory.", messagebox.ButtonTypeOK, messagebox.IconTypeError )
			return
		end
		local path = kernel.combinepaths( ms_hal_tester_path, "mshaltester.exe")
		self:launch_external_process( path, "--secure --username:" .. CurrentUser.id )
	end

	t.activities.kill_process = function( self, parms )
		kernel.showtaskmanager(
			{ "halsvc-win32" },
			function( processes )
				local process_names = {}
				for k, v in ipairs( processes ) do
					if v.process_name ~= "test" then
						kernel.log("activities.kill_process " .. v.process_name )
						kernel.killprocessbyid( v.pid )
						process_names[#process_names + 1] = v.process_name
					end
				end
				if #process_names > 0 then
					kernel.healthsvcs_sendauditevent(
						StoreNumber,
						"CP_Process_Killed",
						CurrentUser.id,
						"CP Bundle",
						"The following processes were killed: " .. table.concat( process_names, ", ") )
				end
				return #processes > 0
			end )
	end

	t.activities.toggle_screen_saver = function( self, parms )
		if kernel.screensaverisactive() then
			kernel.screensaverdisable()
		else
			kernel.screensaverenable()
		end
		self:update_screen_saver_status()
	end

	t.activities.toggle_touch_screen = function( self, parms )
		kernel.toggletouchscreen()
		self:update_touch_screen_status()
	end

	t.activities.toggle_watch_dog = function( self, parms )
		local is_enabled = kernel.getmachinevalueordefault( "Store", "WatchDogEnabled", "true" ) == "true"
		is_enabled = not is_enabled
		kernel.setmachinevalue(
			"Store",
			"WatchDogEnabled",
			tostring( is_enabled) )
		self:update_watch_dog_status()
	end

	t.activities.configure_kiosk = function( self, parms )
		kernel.executescript( "configure_kiosk" )
	end

	t.activities.close_kiosk = function( self, parms )
		kernel.executescript( "close_kiosk" )
	end

	t.activities.toggle_maintenance_mode = function( self, parms )
		kernel.executescript( "toggle_maintenance_mode" )
	end

	t.activities.toggle_out_of_order = function( self, parms )
		if kernel.getmachinevalueordefault( "Store", "OutOfOrder", "false" ) == "true" then
			kernel.setmachinevalue( "Store", "OutOfOrder", "false" )
			kernel.hardwarenotifymaintmodestate( false )
		else
			kernel.healthsvcs_enqueuestatistic( "MinutesInMaintenance", 0 )
			kernel.setmachinevalue( "Store", "OutOfOrder", "true" )
			kernel.hardwarenotifymaintmodestate( true )
		end
		self:update_out_of_order_status()
	end
	
	t.activities.toggle_hal_service = function( self, parms )
		if kernel.getservicestatus( "halsvc$default" ) == "Running" then
			kernel.stopservice( "halsvc$default" )
		elseif kernel.getservicestatus( "halsvc$default" ) == "Stopped" then
			kernel.startservice( "halsvc$default" )
		end
		kernel.timercreate( "hal_status_timer", 100, 100,
			function()
				self:update_hal_service_status()
			end
		)
		kernel.timerstart( "hal_status_timer" )
	end

	t.activities.start_field_maintanence_application = function( self )
		t.kill_open_processes(self)
		idle_timer:stop()
		kernel.timerstop( "date_time_refresh" )
		kernel.activatebundle( "Field Maintenance Application", "*" )
	end

	return t
end
