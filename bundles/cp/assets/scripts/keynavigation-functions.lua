--
-- keynavigation-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newkeynavigationmanager()
	local t = {}
	t.min_index = 1
	t.max_index = 30
	t.index = t.min_index
	t.last_index = 1
	t.initialized = false
	
	t.reset = function( self )
		t.initialized = false
		self:stop()
		self:initialize()
	end
	
	t.initialize = function(self)
		
		local select_button = function()
			local actor = kernel.getactorbytaborder( self.index )
			if actor ~= nil then
				kernel.setactorbackgroundcolor( actor, "DarkOrange")
				kernel.markactordirty( actor )
			end
			if self.index ~= self.last_index then
				local old_actor = kernel.getactorbytaborder( self.last_index )
				if old_actor ~= nil then
					kernel.setactorbackgroundcolor( old_actor, "DarkRed")
					kernel.markactordirty( old_actor )
				end
				self.last_index = self.index
			end
		end
		if self.initialized == true then
			select_button()
			return
		end
		t.initialized = true
		self.index = 1
		
		kernel.registerkeypressnotify(
				"keynavigationmanager",
				function( 
					key,
					keyCode,
					modifier )
					if keyCode == 9 then
						local i
						local newTabIndex = 0
						if modifier == "Shift" then
							i =  - 1
						else
							i = 1
						end
						while newTabIndex == 0 do
							local actor = kernel.getactorbytaborder( self.index + i )
							if actor ~= nil 
							and kernel.isactorvisible(actor) == true
							and kernel.isactorenabled(actor) == true then
								newTabIndex = self.index + i
							else
								if modifier == "Shift" then
									i = i - 1
								else
									i = i + 1
								end
							end
							-- Wrap Around Logic 
							if self.index + i > self.max_index then
								i = self.min_index - self.index
							end
							if self.index + i < self.min_index then
								i = self.max_index - self.index
							end
							-- Only the original button is visible, exit
							if i == 0 and newTabIndex == 0 then
								newTabIndex = self.index
							end
						end
						self.index = newTabIndex
						select_button()
					end
					if keyCode == 32 
					or keyCode == 13 then
						local actor = kernel.getactorbytaborder( self.index )
						kernel.executehitnotify(actor)				
					end 
					
				end )
					
		select_button()
	end
	
	t.select_last_hit_actor = function( self )
		local old_actor = kernel.getactorbytaborder( KeyNavigationManager.last_index )
		if kernel.getlasthitactorname() ~= nil then
			kernel.setactorbackgroundcolor( kernel.getlasthitactorname(), "DarkOrange")
			kernel.markactordirty( kernel.getlasthitactorname() )
			for i = self.min_index, self.max_index do
				local tabbedActor = kernel.getactorbytaborder( i )
				if tabbedActor == kernel.getlasthitactorname() then
					self.last_index = i
					self.index = i
					break
				end
			end
		end
		if old_actor ~= nil and kernel.getlasthitactorname() ~= old_actor then
			kernel.setactorbackgroundcolor( old_actor, "DarkRed")
			kernel.markactordirty( old_actor )
		end
	end
	
	t.stop = function(self)
		kernel.removekeypressnotify( "keynavigationmanager" )
		t.initialized = false
	end
	
	return t
end
