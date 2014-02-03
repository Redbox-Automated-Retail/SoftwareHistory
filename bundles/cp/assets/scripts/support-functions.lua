--
-- support-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function shellsort( table, compare )
	local incs = { 1391376, 463792, 198768, 86961, 33936,13776, 4592, 1968, 861, 336, 112, 48, 21, 7, 3, 1 }
	local n = #table
	for _, h in ipairs(incs) do
      for i = h + 1, n do
        local v = table[i]
        for j = i - h, 1, -h do
          local testval = table[j]
          if not compare(v, testval) then break end
          table[i] = testval
          i = j
        end
        table[i] = v
      end 
    end
    return table
end

string.split = function( str, delim, maxNb )
	if str == nil or delim == nil then return nil end
    if string.find( str, delim ) == nil then
        return { str }
    end
    if maxNb == nil or maxNb < 1 then
        maxNb = 0    -- No limit
    end
    local result = {}
    local pat = "(.-)" .. delim .. "()"
    local nb = 0
    local lastPos
    for part, pos in string.gfind(str, pat) do
        nb = nb + 1
        result[nb] = part
        lastPos = pos
        if nb == maxNb then break end
    end
    if nb ~= maxNb then
        result[nb + 1] = string.sub(str, lastPos)
    end
    return result
end

function converterrorstostring( error_table )
	local s = ""
	for k, v in pairs( error_table ) do
		s = s .. k .. ": " .. v .. "\n"
	end
	return s
end

function newscenemanager()
	local t = {
		add_actor = function(
				self,
				name,
				x,
				y,
				width,
				height,
				parms )
			if parms == nil then
				parms = {}
			end
			kernel.createactor(
				name,
				parms.addToFront or false,
				x,
				y,
				width,
				height,
				parms.text,
				parms.font,
				parms.image,
				parms.backgroundColor,
				parms.foregroundColor,
				parms.textRegion,
				parms.textTranslationPoint,
				parms.textRotationAngle,
				parms.hotSpot,
				parms.renderOptions,
				parms.hitOptions )
			if parms.verticalAlignment ~= nil then
				kernel.setactortextaligny( name, parms.verticalAlignment )
			end
			if parms.horizontalAlignment ~= nil then
				kernel.setactortextalignx( name, parms.horizontalAlignment )
			end
			if parms.opacity ~= nil then
				kernel.setactoropacity( name, parms.opacity )
			end
			if parms.tabs ~= nil then
				kernel.setactortabstops( name, parms.tabs )
			end
			if parms.hit ~= nil then
				kernel.registeractorhitnotify( name, parms.hit )
			end
			if parms.tag ~= nil then
				kernel.setactortag( name, parms.tag )
			end
			table.insert( self.actors, name )
		end,

		remove_actor = function( self, name )
			kernel.removeactor( name )
			table.remove( self.actors, name )
		end,

		clear = function( self )
			kernel.removeactors( self.actors )
			self.actors = {}
		end,

		actors = {}
	}
	return t
end

function newpager()
	local t = {
		next_changed = nil,

		prev_changed = nil,

		page_number = function( self )
			return #self.previous_pages.values + 1
		end,

		mark_current_page = function( self )
			self.last_start_index = self.current_index
			self.in_page_index = 1
		end,

		reset = function( self )
			if self.next_changed ~= nil then
				self.next_changed( false )
			end
			if self.prev_changed ~= nil then
				self.prev_changed( false )
			end
			self.previous_pages:clear()
			self.in_page_index = 0
			self.current_index = 0
			self.last_start_index = 0
			self.is_valid = true
		end,

		update_paging = function( self, size )
			if self.next_changed ~= nil then
				self.next_changed( self.current_index + self.in_page_index < size )
			end
			if self.prev_changed ~= nil then
				self.prev_changed( self:has_previous_pages() )
			end
		end,

		invalidate = function( self )
			self.is_valid = false
		end,

		get_index = function( self )
			return self.current_index + self.in_page_index
		end,

		increment_index = function( self )
			self.in_page_index = self.in_page_index + 1
		end,

		set_index = function( self, index )
			self.in_page_index = index
		end,
		
		page_next = function( self )
			self.current_index = self.current_index + self.in_page_index
			self.previous_pages:push( self.last_start_index )
			self.in_page_index = 1
		end,

		page_back = function( self )
			self.current_index = self.previous_pages:pop()
			self.in_page_index = 1
		end,

		has_previous_pages = function( self )
			return #self.previous_pages.values > 0
		end,

		previous_pages = {
			clear = function( self )
				self.values = {}
			end,
			push = function( self, value )
				table.insert( self.values, value )
			end,
			pop = function( self )
				local size = #self.values
				local value = self.values[size]
				table.remove( self.values, size )
				return value
			end,
			values = {}
		},

		in_page_index = 0,

		current_index = 0,

		last_start_index = 0,

		is_valid = false
	}
	return t
end
