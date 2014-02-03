--
-- datetime.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
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

		local q,r = math.modf(t.backing.min / 5)
		local minutes = q * 5
		if r > 0 then
			minutes = minutes + 5

			if minutes >= 60 then
				minutes = 0
				t = t + "00:01:00"
			end
		end
		t.backing.min = minutes
	else
		t.backing = date
	end
	
	return t
end
