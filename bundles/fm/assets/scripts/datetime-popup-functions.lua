--
-- datetime-popup-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function refresh_actors(date)
	kernel.setactortext("minute_button", string.format("%02d", working_date.backing.min))
	kernel.setactortext("month_button", string.format("%02d",  working_date.backing.month))
	kernel.setactortext("day_button", string.format("%02d", working_date.backing.day))
	kernel.setactortext("year_button", string.format("%02d", string.sub(working_date.backing.year, 3)))

	if working_date.backing.hour >= 12 then
		kernel.setactortext("meridiem_button", "PM")
	else
		kernel.setactortext("meridiem_button", "AM")
	end
	
	if working_date.backing.hour > 12 then
		kernel.setactortext("hour_button", string.format("%02d", working_date.backing.hour-12))
	else
		if working_date.backing.hour == 0 then
			kernel.setactortext("hour_button", "12")
		else
			kernel.setactortext("hour_button", string.format("%02d", working_date.backing.hour))
		end
	end
	
	local day_name = ""
	if working_date.backing.wday == 1 then
		day_name = "Sunday"
	elseif working_date.backing.wday == 2 then
		day_name = "Monday"
	elseif working_date.backing.wday == 3 then
		day_name = "Tuesday"
	elseif working_date.backing.wday == 4 then
		day_name = "Wednesday"
	elseif working_date.backing.wday == 5 then
		day_name = "Thursday"
	elseif working_date.backing.wday == 6 then
		day_name = "Friday"
	elseif working_date.backing.wday == 7 then
		day_name = "Saturday"
	end
	kernel.setactortext("datetime_display", "Job will run on " .. day_name .. ", " .. tostring(working_date))
end