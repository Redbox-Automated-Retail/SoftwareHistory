--
-- lib-extension-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function table_count( t )
	local count = 0
	for _, _ in pairs( t ) do count = count + 1 end
	return count
end

function get_current_date_string( mask )
	local now = os.date( "*t" )
	return string.format( mask, now.year, now.month, now.day, now.hour, now.min, now.sec )
end

function get_current_date_utc_string( mask )
	local now = os.date( "!*t" )
	return string.format( mask, now.year, now.month, now.day, now.hour, now.min, now.sec )
end
	
function parseDateTime( dateString )
	local Y = string.sub( dateString, 1, 4 )
	local M = string.sub( dateString, 5, 6 )
	local D = string.sub( dateString, 7, 8 )
	local h = string.sub( dateString, 9, 10 )
	local m = string.sub( dateString, 11, 12 )
	local s = string.sub( dateString, 13, 14 )
	return os.time( { year = Y, month = M, day = D, hour = h, min = m, sec = s } )
end

function round(num, idp)
  local mult = 10^(idp or 0)
  return math.floor(num * mult + 0.5) / mult
end

function trim (s)
	return (string.gsub(s, "^%s*(.-)%s*$", "%1"))
end

string.padl = function(str, len, char)
	if char == nil then char = ' ' end
	return str .. string.rep(char, len - #str)
end

string.padr = function(str, len, char)
	if char == nil then char = ' ' end
	return string.rep(char, len - #str) .. str
end

function indexof( t, value )
	for k, v in ipairs( t ) do
		if v == value then
			return k
		end
	end
	return -1	
end

function flatten_table( t )
	local buffer = ""
	for k, v in pairs(t) do
		if buffer:len() > 0 then buffer = buffer .. ", " end
		buffer = buffer .. tostring(k) .. ": "
		if type(v) == "table" then
			buffer = buffer .. flatten_table(v)
		else
			buffer = buffer .. tostring(v)
		end
	end
	return buffer
end

function converterrorstostring( error_table )
	local s = ""
	for k, v in pairs( error_table ) do
		s = s .. k .. ": " .. v .. "\n"
	end
	return s
end

function inlist( str, val, delim )
	local list = string.split( str, delim )
	for _, v in ipairs(list) do
		if tostring( val ) == tostring( v ) then 
			return true 
		end
	end
	return false
end

string.split = function( str, delim, maxNb )
    -- Eliminate bad cases...
    if string.find(str, delim) == nil then
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
    -- Handle the last field
    if nb ~= maxNb then
        result[nb + 1] = string.sub(str, lastPos)
    end
    return result
end

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

function split(s, c)
	local parts = {}
	local start = 1
	while true do
		local spos,epos = s:find(c, start, true)
		if spos ~= nil then
			table.insert(parts, s:sub(start, spos-1))
			start = epos+1
		else
			if start <= s:len() then
				table.insert(parts, s:sub(start))
			end
			break
		end
	end
	
	return parts
end

function iif(condition, success, failure)
	if condition then
		return success
	else
		return failure
	end
end

function filtertable(table, filter)
	local result = {}
	for k, v in pairs(table) do
		if filter(v) then
			result[#result+1] = v
		end
	end
	return result
end

function newstack()
  t = {
    backing = {},
    index = 0,

    init = function(
      self,
      t )
      if t ~= nil then
          for _, v in next, t do
            self.backing[#self.backing + 1] = v
            index = index + 1
          end
        end
    end,

    count = function(
      self)
      return #self.backing
    end,

    peek = function(
      self)
      if self.index <= 0
        or self:count() <= 0 then return nil end
      return self.backing[self.index]
    end,

    push = function(
      self,
      item )
        if item == nil then return end
        self.index = self.index + 1
        self.backing[self.index] = item
    end,

    pop = function(
      self )
      if self.index <= 0
        or self:count() <= 0 then return nil end
      local item = self.backing[self.index]
      self.index = self.index - 1
      return item
    end
  }
  return t
end

function deepcopy( object )
    local lookup_table = {}
    local function _copy( object )
        if type( object ) ~= "table" then
            return object
        elseif lookup_table[object] then
            return lookup_table[object]
        end
        local new_table = {}
        lookup_table[object] = new_table
        for index, value in pairs( object ) do
            new_table[_copy( index )] = _copy( value )
        end
        return setmetatable( new_table, getmetatable( object ) )
    end
    return _copy( object )
end

function deepcompare( t1, t2, ignore_mt )
	local ty1 = type( t1 )
	local ty2 = type( t2 )
	-- Objects of different data types are not equal by definition
	if ty1 ~= ty2 then
		return false 
	end
	-- non-table types can be directly compared
	if ty1 ~= "table" and ty2 ~= "table" then
		return t1 == t2 
	end
	-- Tables which have the metamethod __eq can be directly compared using the metamethod
	local mt = getmetatable( t1 )
	if not ignore_mt and mt and mt.__eq then
		return t1 == t2 
	end
	-- Otherwise perform the deep compare iteratively
	for k1, v1 in pairs( t1 ) do
		local v2 = t2[ k1 ]
		if v2 == nil or not deepcompare( v1, v2 ) then
			return false 
		end
	end
	for k2, v2 in pairs( t2 ) do
		local v1 = t1[ k2 ]
		if v1 == nil or not deepcompare( v1, v2 ) then
			return false 
		end
	end
	return true
end
