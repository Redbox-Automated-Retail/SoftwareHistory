--
-- datetime.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newdatetime(date)
	local t = {}
	local mt = {}
	
	setmetatable( t, mt )
	
	mt.__add = 	function(op1, op2)
		return newdatetime(kernel.dateadd(op1.backing, op2))
	end

	mt.__sub = 	function(op1, op2)
		return newdatetime(kernel.dateadd(op1.backing, "-" .. op2))
	end

	mt.__tostring =	function(self)
		if self == nil then
			return ""
		end
		
		local hour = self.backing.hour
		local meridiem = "PM"
		if hour < 12 then
			meridiem = "AM"
			if hour == 0 then
				hour = 12
			end
		else
			if hour > 12 then
				hour = hour - 12
			end
		end

		return string.format("%02d", self.backing.month) .. "/" .. string.format("%02d", self.backing.day) .. "/" .. self.backing.year .. " " .. string.format("%02d", hour) .. ":" .. string.format("%02d", self.backing.min) .. " " .. meridiem
	end
	
	mt.__lt = function(left, right)
		local result = kernel.datecompare(left.backing, right.backing)
		return result == -1
	end
	
	mt.__le = function(left, right)
		local result = kernel.datecompare(left.backing, right.backing)
		return result == -1 or result == 0
	end
	
	if date == nil then
		t.backing = os.date("*t")

		local hour = t.backing.hour
		local q,r = math.modf(t.backing.min / 5)
		local minutes = q * 5
		if r > 0 then
			minutes = minutes + 5
			if minutes >= 60 then
				minutes = 0
				hour = hour + 1
				if hour == 25 then
					hour = 0
				end
			end
		end
		t.backing.hour = hour
		t.backing.min = minutes
	else
		t.backing = date
	end
	
	t.is_within_minutes = function( self, comp, minutes )
		-- This function will deal with hour rollover but not days, months, or years.
		-- It also assumes the comp time is ONLY in the future, not the past because UI does not allow it
		if self.backing.month == comp.backing.month
		and self.backing.day == comp.backing.day
		and self.backing.year == comp.backing.year
		and (
				(self.backing.hour == comp.backing.hour and math.abs( self.backing.min - comp.backing.min ) <= minutes )
				or ( self.backing.hour + 1 == comp.backing.hour	and ( 60 - self.backing.min ) + comp.backing.min <= minutes )
			)
		then
			return true
		else
			return false
		end
	end
	
	t.to_cron_expression = function( self )
		return "0 " .. 
			tostring( self.backing.min ) .. " "  ..
			tostring( self.backing.hour ) .. " " ..
			tostring( self.backing.day ) .. " " ..
			tostring( self.backing.month ) .. " ? " ..
			tostring( self.backing.year )
	end
	
	t.set_from_cron_expression = function( self, cron )
		local fields = {}
		string.gsub(cron, "%s*(%d+)%s*", function(values) table.insert(fields,values) end )
		self.backing.min = tonumber( fields[2] )
		self.backing.hour = tonumber( fields[3] )
		self.backing.day = tonumber( fields[4] )
		self.backing.month = tonumber( fields[5] )
		self.backing.year = tonumber( fields[6] )
	end
	
	return t
end
