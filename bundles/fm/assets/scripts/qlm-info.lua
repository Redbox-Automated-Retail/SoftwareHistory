--
-- qlm-info.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newqlminfo()
	local t = {}
	local mt = {}
	setmetatable( t, mt )
	
	t.bay_status = nil
	t.hardware_mgr = newhardwaremanager()
	t.on_load = nil
	t.loaded = false
	
	t.load = function(self, progress_mgr)
		local result = self.hardware_mgr:executeimmediate(
					"QLM STATUS",
					true,
					function(command, success, stack, errors)
						if success then
							self.bay_status = stack[1]
						else
							self.bay_status = "ERROR"
						end
						if self.on_load ~= nil then
							self.on_load()
						end
						if progress_mgr ~= nil then
							progress_mgr:stop()
						end
						self.loaded = true
					end)
	end
	
	t.execute_command = function( 
		self, 
		command, 
		suppress_load,
		on_complete)
		local progress_manager = nil
		
		if not suppress_load then
			progress_manager = newprogressmanager()
			progress_manager:start( false )
		end
		
		kernel.callbackenqueue(
				function()
					local result = self.hardware_mgr:executeimmediate(
										command,
										true,
										function(
											command, 
											success, 
											stack, 
											errors)
											if not suppress_load then
												self:load(progress_manager)
											end
											if on_complete then
												on_complete()
											end
										end)
				end,
				{})
	end
	
	mt.__tostring = function(self)
		return "The QLM bay is currently " .. tostring( self.bay_status ) .. "."
	end
	return t
end