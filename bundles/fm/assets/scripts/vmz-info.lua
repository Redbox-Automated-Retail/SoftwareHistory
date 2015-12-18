--
-- vmz-info.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newvmzinfo()
	local t = {}
	local mt = {}
	setmetatable( t, mt )
	
	t.hardware_mgr = newhardwaremanager()
	t.on_load = nil
	t.loaded = false
	t.bin_count = 0
	t.thin_deck = 0
	t.thin_slot = 0
	t.last_thin_unavailable = false
	t.thins_outside_vmz_count = 0
	t.thins_outside_vmz_count_completed = false;
	t.redeploys_outside_vmz_count = 0
	t.redeploys_outside_vmz_count_completed = false;

	--note the immediate call in the immediate call is to prevent the 2 immediates fighting over which gets to run
	t.load = function(self, progress_mgr)
		local bin_load_success = false
		local thin_slot_success = false
		self.last_thin_unavailable = false
		self.thins_outside_vmz_count = 0
		self.thins_outside_vmz_count_completed = false
		self.redeploys_outside_vmz_count = 0
		self.redeploys_outside_vmz_count_completed = false;

		-- Determine the # of thins outside the VMZ
		local total_thins_list = get_thin_barcodes2()
		barcodes_vmz_status ( false, true, total_thins_list, function( result_list )
			self.thins_outside_vmz_count_completed = true
			self.thins_outside_vmz_count = #result_list
			kernel.setactortext("thins_outside_vmz_value", tostring(self.thins_outside_vmz_count))
			kernel.markactordirty("thins_outside_vmz_value")
		end)
		
		local total_redeploy_list = kernel.rental_inventory_get_rapidstatus_by_status("Redeployment");
		local redeploy_table = {}
		barcodes_vmz_status ( false, true, total_redeploy_list, function( result_list )
			self.redeploys_outside_vmz_count_completed = true
			self.redeploys_outside_vmz_count = #result_list
			kernel.setactortext("thin_redeploys_outside_vmz_value", tostring(self.redeploys_outside_vmz_count))
			kernel.markactordirty("thin_redeploys_outside_vmz_value")
		end)

		local result = self.hardware_mgr:executeimmediate(
					"COUNTER NAME=\"DUMPBIN\" GET",
					true,
					function(command, success, stack, errors)
						if success then
							if stack[1] == "SUCCESS" then
								self.bin_count = stack[2]
								bin_load_success = true
							else
								kernel.log("Retrieve Dumpbin Counter Result: Failed")
								self:failed_update_text()
								if progress_mgr ~= nil then
									progress_mgr:stop()
								end
								self.last_thin_unavailable = true
								return
							end
						else
							kernel.log("Retrieve Dumpbin Counter Failed to return successfully")
							self.bin_count = 0
							self:failed_update_text()
							if progress_mgr ~= nil then
								progress_mgr:stop()
							end
							self.last_thin_unavailable = true
							return
						end
						
						result = self.hardware_mgr:executeimmediate(
							"VMZ GET-LAST-THIN",
							true,
							function(command, success, stack, errors)
								if success then
									-- if stack is not "Found" then it is "None"
									if stack[1] == "FOUND" then
										self.thin_deck = stack[2]
										self.thin_slot = stack[3]
									else
										self.thin_deck = 0
										self.thin_slot = 0
									end
									thin_slot_success = true
								else
									kernel.log("Retrieve Last Thin Failed to return successfully")
									self.last_thin_unavailable = true
								end
								
								if bin_load_success and thin_slot_success then
									self:success_update_text()
								else
									self:failed_update_text()
								end

								if self.on_load ~= nil then
									self.on_load()
								end
								if progress_mgr ~= nil then
									progress_mgr:stop()
								end
								self.loaded = true
						end)
					end)		
	end
	--note the immediate call in the immediate call is to prevent the 2 immediates fighting over which gets to run
	t.refresh = function( self )
		local bin_load_success = false
		local thin_slot_success = false

		local progress_mgr = newprogressmanager()
		progress_mgr:start( false )
		local result = self.hardware_mgr:executeimmediate(
					"COUNTER NAME=\"DUMPBIN\" GET",
					true,
					function(command, success, stack, errors)
						if success then
							if stack[1] == "SUCCESS" then
								self.bin_count = stack[2]
								bin_load_success = true
							else
								kernel.log("Retrieve Dumpbin Counter Result: Failed")
								self:failed_update_text()
								if progress_mgr ~= nil then
									progress_mgr:stop()
								end
								return
							end
						else
							self.bin_count = 0
							kernel.log("Retrieve Dumpbin Counter Failed to return successfully")
							self:failed_update_text()
							if progress_mgr ~= nil then
								progress_mgr:stop()
							end
							return
						end
						
						result = self.hardware_mgr:executeimmediate(
							"VMZ GET-LAST-THIN",
							true,
							function(command, success, stack, errors)
								if success then
									thin_slot_success = true
									if stack[1] == "FOUND" then
										self.thin_deck = stack[2]
										self.thin_slot = stack[3]
									else
										-- if stack is not "Found" then it is "None"
										self.thin_deck = 0
										self.thin_slot = 0
									end
								else
									kernel.log("Retrieve Last Thin Failed to return successfully")
								end

								if bin_load_success and thin_slot_success then
									self:success_update_text()
								else
									self:failed_update_text()
								end

								if progress_mgr ~= nil then
									progress_mgr:stop()
								end
							end)
				end)
	end
	
	t.failed_update_text = function( self )
		kernel.setactortext("dump_bin_label","Unable to read VMZ data")
		kernel.setactorforegroundcolor("dump_bin_label","Red")
	end

	t.success_update_text = function( self )
		kernel.setactorforegroundcolor("dump_bin_label","Black")
		kernel.setactortext("dump_bin_label","Discs in Dump Bin: " .. self.bin_count)
		kernel.showactor("thins_outside_vmz_value")
		if self.thins_outside_vmz_count_completed then
			kernel.setactortext("thins_outside_vmz_value", tostring(self.thins_outside_vmz_count))
		else
			kernel.setactortext("thins_outside_vmz_value", "NA")
		end
		kernel.showactor("thin_redeploys_outside_vmz_value")
		if self.redeploys_outside_vmz_count_completed then
			kernel.setactortext("thin_redeploys_outside_vmz_value", tostring(self.redeploys_outside_vmz_count))
		else
			kernel.setactortext("thin_redeploys_outside_vmz_value", "NA")
		end
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
		--local thin_msg = "last thin at deck = " .. t.thin_deck .. " slot = " .. t.thin_slot
		--if t.thin_deck == 0 then
		--	thin_msg = "No thins in the drum."
		--end
		--return "Disks in bin:" .. t.bin_count .. "; " .. thin_msg
		return ""
	end

	return t
end