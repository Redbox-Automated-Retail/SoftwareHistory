--
-- configure-kiosk-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

SceneManager = newscenemanager()

local next_changed = function( enabled )
	if enabled == true then
		kernel.setactorimage( "scroll_down_button", "scroll_down" )
		kernel.setactorforegroundcolor( "scroll_down_button", "White" )
		kernel.enableactor( "scroll_down_button" )
	else
		kernel.setactorimage( "scroll_down_button", "scroll_down_disabled" )
		kernel.setactorforegroundcolor( "scroll_down_button", "Gray" )
		kernel.disableactor( "scroll_down_button" )
	end
end

local prev_changed = function( enabled )
	if enabled == true then
		kernel.setactorimage( "scroll_up_button", "scroll_up" )
		kernel.setactorforegroundcolor( "scroll_up_button", "White" )
		kernel.enableactor( "scroll_up_button" )
	else
		kernel.setactorimage( "scroll_up_button", "scroll_up_disabled" )
		kernel.setactorforegroundcolor( "scroll_up_button", "Gray" )
		kernel.disableactor( "scroll_up_button" )
	end

end

local show_table = function(
   table,
   row_formatter,
   empty_string,
   header )
	if header ~= nil then
		kernel.setactortext(
			"header_image",
			header.text)
		kernel.setactortabstops(
			"header_image",
			header.tabs )
	end
	if (table == nil
	or table.data == nil
	or table.pager == nil )
	or row_formatter == nil then
		return
	end
	local data = table.data
	local pager = table.pager
	SceneManager:clear()
	kernel.hideactor("selected_row")
	x = 20
	y = 120
	pager:mark_current_page()
    if #data > 0 then
        while true do
            row_formatter(
                data[ pager:get_index() ],
                x,
                y )
            y = y + 60
            if y >= 525 then
                break
            end
            pager:increment_index()
            if pager:get_index() > #data then
                break
            end
        end
    else
        local params = {
            text = empty_string,
            font = "medium_font",
            foregroundColor = "black",
            renderOptions = "Text",
            hitOptions = "Enabled",
            hit = function()
                      return
                  end
        }
        SceneManager:add_actor(
            'empty_data_set',
            x,
            y,
            720,
            20,
            params,
            nil )
    end
	pager:update_paging( #data )
end

local register_page_navigation = function(
	table_name,
	row_formatter )
	kernel.registeractorhitnotify(
	"scroll_down_button",
	function()
		local cache = KioskConfigManager.transitions.cache[ table_name ]
		cache.pager:page_next()
		show_table(
			cache,
			row_formatter )
	end )
	kernel.registeractorhitnotify(
		"scroll_up_button",
		function()
			local cache = KioskConfigManager.transitions.cache[ table_name ]
			cache.pager:page_back()
			show_table(
				cache,
				row_formatter )
		end )
end


local get_pending_states = function( parms, callback )
	 return kernel.storesvcs_getpendingstates(
         function(r)
             callback( r,
                       shellsort(
                           r.properties.states,
                           function( first, second )
                               return first.name < second.name
                           end ) )
         end )
end

local get_pending_banners = function ( parms, callback )
	return kernel.storesvcs_getpendingbanners(
		parms[1],
		function(r)
            callback( r,
                      shellsort(
                          r.properties.banners,
                          function( first, second )
                              return first.name < second.name
                          end ) )
		end )
end

local get_pending_stores = function ( parms, callback )
	return kernel.storesvcs_getpendingstores(
		parms[1],
		parms[2],
		function(r)
            callback( r,
                      shellsort(
                          r.properties.stores,
                          function( first, second )
                              return first.city < second.city
                          end ) )
		end )
end

local reboot = function( self, parms )
		local messagebox = newmessageboxmanager()
		callbacks = {}
		callbacks[messagebox.ButtonYES] = function()
			kernel.startprocess( "shutdown.exe", "-r -t 0", false )
		end
		messagebox:show(
			"Are you sure you want to reboot the kiosk?",
			messagebox.ButtonTypeYESNO,
			messagebox.IconTypeQuestion,
			callbacks )
end


function newkioskconfigurationmanager()


	local t = {}

	t.next = "start"

	t.previous = nil

	t.current = nil

	t.selected_state = nil

	t.selected_banner = nil

	t.selected_store = nil

	t.transfer = function( self )
		kernel.hideactor( "selected_row" )
		kernel.hideactor( "button_1")
		kernel.hideactor( "button_2")
		SceneManager:clear()
		local func = self.transitions[ self.next ]
		if func == nil then
			return
		end
		func( self )
	end

	t.goback = function( self )
		self.next = self.previous
		self:transfer()
	end
	

	t.show_data = function(
                          self,
                          header,
                          row_formatter,
                          empty_string,
                          kernel_function,
                          parms)
                      local current = self.current
                      if self.transitions.cache[ current ] ~= nil
                      and self.transitions.cache[ current ].data ~= nil then
                          show_table(
                              self.transitions.cache[ current ],
                              row_formatter,
                              empty_string,
                              header )
                      else
                          local progress_manager = newprogressmanager()
                          progress_manager:start(
                              768,
                              642 )
                          kernel_function(
                              parms,
                              function( result, data )
                                  progress_manager:stop()
                                  if result.success then
                                      self.transitions.cache[ current ] = {}
                                      self.transitions.cache[ current ].data = data
                                      local pager = newpager()
                                      pager.next_changed = next_changed
                                      pager.prev_changed = prev_changed
                                      self.transitions.cache[ current ].pager = pager
                                      show_table(
                                          self.transitions.cache[ current ],
                                          row_formatter,
                                          empty_string,
                                          header )
                                  else
                                      kernel.setactortext(
                                          "title_label",
                                          "Error downloading data. Please Check Network Connection." )
                                  end
                              end )
                      end
                      register_page_navigation(
                          current,
                          row_formatter )
                  end

	t.transitions = {}

	t.transitions.start = function( self )
		self.current = "start"
		self.next = "pending_states"
		self.previous = nil
		self:transfer()
	end

	t.transitions.pending_states = function( self )
		self.next = "pending_banners"
		self.current = "pending_states"
		self.previous = "start"
		self.transitions.cache[ "pending_banners" ] = nil
		self.transitions.cache[ "pending_stores" ] = nil
		x=628
		y=555
		kernel.setactortext(
				"title_label",
				"Select State" )
		kernel.setactortext(
				"button_1",
				"Next" )
		kernel.setactorlocation("button_1", x, y )
		
		kernel.showactor( "button_1" )

		kernel.registeractorhitnotify(
			"button_1",
			function()
				self:transfer()
			end )
		if self.selected_state == nil then
			kernel.disableactor( "button_1" )
			kernel.setactorforegroundcolor(
				"button_1",
				"Gray" )
		else
			kernel.enableactor( "button_1" )
			kernel.setactorforegroundcolor(
				"button_1",
				"white" )
		end

		local header = {
			text = "\tState Id\tName",
			tabs = { 10, 400 }
		}
		local row_formatter = function( row, x, y )
                                  local formatted_text = row.id .. "\t" .. row.name
                                  local params = {
                                      text = formatted_text,
                                      font = "medium_font",
                                      foregroundColor = "black",
                                      renderOptions = "Text",
                                      tabs = { 400 },
                                      hitOptions = "Enabled",
                                      hit = function()
                                                kernel.setactorlocation(
                                                    "selected_row",
                                                    5,
                                                    y - 20 )
                                                kernel.showactor( "selected_row" )
                                                self.selected_state = row
                                                kernel.enableactor( "button_1" )
                                                kernel.setactorforegroundcolor(
                                                    "button_1",
                                                    "White" )
                                            end
                                  }
                                  SceneManager:add_actor(
                                      row.id .. "_" .. row.name,
                                      x,
                                      y,
                                      700,
                                      20,
                                      params,
                                      nil )
                                  if self.selected_state ~= nil
                                      and self.selected_state.id == row.id then
                                      kernel.setactorlocation(
                                          "selected_row",
                                          5 ,
                                          y - 20 )
                                      kernel.showactor( "selected_row" )
                                      kernel.enableactor( "button_1" )
                                      kernel.setactorforegroundcolor(
                                          "button_1",
                                          "White" )
                                  end
                              end

		self:show_data(
			header,
			row_formatter,
            "No Pending States were found.",
			get_pending_states,
			nil )
	end

	t.transitions.pending_banners = function( self )
		self.next = "pending_stores"
		self.current = "pending_banners"
		self.previous = "pending_states"
		self.transitions.cache[ "pending_stores" ] = nil
		x=464
		y=555
		kernel.setactortext(
				"title_label",
				"Select State > Select Banner" )
		kernel.showactor( "button_1" )
		kernel.setactorlocation( "button_1", x, y )
		kernel.setactortext(
			"button_1",
			"Previous" )

		kernel.registeractorhitnotify(
			"button_1",
			function()
				self.selected_banner = nil
				self:goback()
			end )
		kernel.showactor( "button_2" )
		kernel.setactortext(
			"button_2",
			"Next" )
		kernel.setactorlocation( "button_2", x+145, y )
		kernel.registeractorhitnotify(
			"button_2",
			function()
				self:transfer()
			end )
		if self.selected_banner == nil then
			kernel.setactorforegroundcolor(
				"button_2",
				"Gray" )
			kernel.disableactor( "button_2" )
		else
			kernel.setactorforegroundcolor(
				"button_2",
				"White" )
			kernel.enableactor( "button_2" )
		end

		local header = {
			text = "\tBanner Id\tName",
			tabs = { 10, 400 }
		}

		local row_formatter = function( row, x, y )
                                  local formatted_text = row.id .. "\t" .. row.name
                                  local params = {
                                      text = formatted_text,
                                      font = "medium_font",
                                      foregroundColor = "black",
                                      renderOptions = "Text",
                                      tabs = {400},
                                      hitOptions = "Enabled",
                                      hit = function()
                                                kernel.setactorlocation(
                                                    "selected_row",
                                                    5,
                                                    y - 20 )
                                                kernel.showactor( "selected_row" )
                                                self.selected_banner = row
                                                kernel.enableactor( "button_2" )
                                                kernel.setactorforegroundcolor(
                                                    "button_2",
                                                    "White" )
                                            end
                                  }
                                  SceneManager:add_actor(
                                      row.id .. "_" .. row.name,
                                      x,
                                      y,
                                      700,
                                      20,
                                      params,
                                      nil )
                                  if self.selected_banner ~= nil
                                      and self.selected_banner.id == row.id then
                                      kernel.setactorlocation(
                                          "selected_row",
                                          5 ,
                                          y - 20 )
                                      kernel.showactor( "selected_row" )
									  --kernel.setactorwidth("selected_row", "400" )
                                      kernel.enableactor( "button_2" )
                                      kernel.setactorforegroundcolor(
                                          "button_2",
                                          "White" )
                                  end
                              end
		self:show_data(
			header,
			row_formatter,
            "No Pending Banners were found.",
			get_pending_banners,
			{self.selected_state.id} )
	end

	t.transitions.pending_stores = function( self )
		self.next = "confirm"
		self.current = "pending_stores"
		self.previous = "pending_banners"
		x=484
		y=555
		kernel.setactortext(
				"title_label",
				"Select State > Select Banner > Select Store" )
		kernel.showactor( "button_1" )
		kernel.setactortext(
			"button_1",
			"Previous" )
		kernel.setactorlocation( "button_1", x, y )
		kernel.registeractorhitnotify(
			"button_1",
			function()
				self.selected_store = nil
				self:goback()
			end )
		kernel.showactor( "button_2" )
		kernel.setactortext(
			"button_2",
			"Next" )
	    kernel.setactorlocation( "button_2", x+145, y )
		kernel.registeractorhitnotify(
			"button_2",
			function()
				self:transfer()
			end )
		if self.selected_store == nil then
			kernel.setactorforegroundcolor(
				"button_2",
				"Gray" )
			kernel.disableactor( "button_2" )
		else
			kernel.setactorforegroundcolor(
				"button_2",
				"White" )
			kernel.enableactor( "button_2" )
		end
		
		--show scroll up and scroll down actors 
		kernel.showactor("scroll_up_button")
		kernel.showactor("scroll_down_button")
		kernel.showactor("scroll_bar_background")
		kernel.showactor("scroll_up_background")
		kernel.showactor("scroll_down_background")


		local header = {
			text = "\tStore Id\tAddress\tCity\tState\tZip",
			tabs = { 10, 110, 230, 150, 135 }
		}

		local row_formatter = function( row, x, y )
			local formatted_text = row.id .. "\t" .. string.sub(row.address,1, 28) .. "\t" .. row.city .. "\t" .. row.state .. "\t" .. row.postal_code
			local params = {
				text = formatted_text,
				font = "medium_font",
				foregroundColor = "black",
				renderOptions = "Text",
				tabs = { 90, 240, 160, 75},
				hitOptions = "Enabled",
				hit = function()
					kernel.setactorlocation(
						"selected_row",
						5,
						y - 20 )
					kernel.showactor( "selected_row" )
					self.selected_store = row
					kernel.enableactor( "button_2" )
					kernel.setactorforegroundcolor(
						"button_2",
						"White" )
				end
			}
			SceneManager:add_actor(
				row.id .. "_Store",
				x,
				y,
				700,
				20,
				params,
				nil )
			if self.selected_store ~= nil
			and self.selected_store.id == row.id then
				kernel.setactorlocation(
					"selected_row",
					5 ,
					y - 20 )
				kernel.showactor( "selected_row" )
				kernel.enableactor( "button_2" )
				kernel.setactorforegroundcolor(
					"button_2",
					"White" )
			end
		end

		self:show_data(
			header,
			row_formatter,
            "No Pending Kiosks were found.",
			get_pending_stores,
			{self.selected_banner.id, self.selected_state.id} )
	end
	
	t.transitions.confirm = function( self )
		self.next = "finish"
		self.current = "confirm"
		self.previous = "pending_stores"
		x=484
		y=555
		kernel.setactortext(
				"title_label",
				"Select State > Select Banner > Select Store > Confirm" )

		kernel.showactor( "button_1" )
		kernel.setactortext(
			"button_1",
			"Previous" )
	    kernel.setactorlocation( "button_1", x, y )
		kernel.registeractorhitnotify(
			"button_1",
			function()
				self.selected_store = nil
				kernel.hideactor( "button_3" )
				kernel.showactor( "header_image" )
				self:goback()
			end )

		kernel.showactor( "button_2" )
		kernel.setactortext(
			"button_2",
			"Finish" )
		kernel.setactorlocation( "button_2", x+145, y )
		kernel.registeractorhitnotify(
			"button_2",
			function()
				kernel.hideactor( "button_3" )
				self:transfer()
			end )
		kernel.setactorforegroundcolor(
			"button_2",
			"White" )
		kernel.enableactor( "button_2" )
		
		
		kernel.showactor( "button_3" )
		kernel.setactortext(
			"button_3",
			"Cancel" )
		kernel.setactorlocation( "button_3", 30, y )
		kernel.registeractorhitnotify(
			"button_3",
			function()
				view_goback()
			end )
		kernel.setactorforegroundcolor(
			"button_3",
			"White" )
		
		kernel.enableactor( "button_3" )
        
		--hide scroll up and scroll down actors 
		kernel.hideactor("scroll_up_button")
		kernel.hideactor("scroll_down_button")
		kernel.hideactor("scroll_bar_background")
		kernel.hideactor("scroll_up_background")
		kernel.hideactor("scroll_down_background")

		kernel.hideactor( "header_image" )

		-- Create actors "state_value", "banner_value", "store_value"
		SceneManager:add_actor( "state_label",
			20,
			100,
			400,
			30,
			{	text = "State: " .. self.selected_state.name,
				font = "medium_large_font",
				horizontalAlignment = "near",
				renderOptions = "Text,TextShadow",
			} )

		SceneManager:add_actor( "banner_label",
			20,
			140,
			400,
			30,
			{	text = "Banner: " .. self.selected_banner.name,
				font = "medium_large_font",
				horizontalAlignment = "near",
				renderOptions = "Text,TextShadow",
			} )

		SceneManager:add_actor( "store_label",
			20,
			180,
			400,
			30,
			{	text = "Store: " .. self.selected_store.id,
				font = "medium_large_font",
				horizontalAlignment = "near",
				renderOptions = "Text,TextShadow",
			} )

	end
	
	t.transitions.finish = function( self )
		self.next = nil
		self.current = "finish"
		self.previous = "confirm"
		
		local path = kernel.storesvcs_getkaseyainipath()
		kernel.replaceinfile(
			path,
			"User_Name\\s+(?<KaseyaId>[\\w|\\-|\\.]+)",
			"KaseyaId",
			"Redbox-".. self.selected_store.id .. "." .. self.selected_store.kaseya_market_name )

		kernel.setmachinevalue(
			"Store",
			"ID",
			self.selected_store.id )
		
		kernel.log( "prior_kiosk_id: ".. prior_kiosk_id )
		kernel.log( "new_kiosk_id: ".. self.selected_store.id )
		local audit_type = "configure_kiosk"
		if tostring( prior_kiosk_id ) == "1"
		or tostring( prior_kiosk_id ) ~= tostring( self.selected_store.id ) then
			audit_type = "open_kiosk"
		elseif tostring( prior_kiosk_id ) == tostring( self.selected_store.id ) then
			audit_type = "configure_kiosk"
		end
		
		kernel.healthsvcs_sendauditevent(
			self.selected_store.id,
			audit_type,
			CurrentUser.id,
			"CP Bundle",
			"Activate Control Panel function."
		)

        kernel.systemrenamecomputer( "redbox-" .. self.selected_store.id )

		kernel.setmachinevalue(
			"Store",
			"Banner",
			self.selected_banner.name )

		kernel.setmachinevalue(
			"Store",
			"Market",
			self.selected_store.kaseya_market_name )
		kernel.setmachinevalue(
			"Store",
			"Duetime",
			self.selected_store.due_time )
		
		local progress_manager = newprogressmanager()
		progress_manager:start( 768, 642 )
		kernel.storesvcs_getopsmarket(
			self.selected_store.id,
			function( result )
				local path = "C:\\Program Files\\Redbox\\Reds\\Installers\\" .. result.properties.kaseya_server .. "Install.exe"
				local args = "/s /w /r /v /g=redbox." .. result.properties.ops_market .. " /m=redbox-" .. self.selected_store.id
				
				kernel.startprocesswaitforexit( path, args, 20000 )
				progress_manager:stop()
				
				local messagebox = newmessageboxmanager()
				callbacks = {}
				callbacks[messagebox.ButtonYES] = function()
					kernel.startprocess( "shutdown.exe", "-r -t 0", false )
				end
				callbacks[messagebox.ButtonNO] = function()
					view_goback()
				end
				messagebox:show(
					"Do you want to reboot the kiosk now?",
					messagebox.ButtonTypeYESNO,
					messagebox.IconTypeQuestion,
					callbacks
				)
			end
		)
	end

	t.transitions.cache = {}

	return t
end


