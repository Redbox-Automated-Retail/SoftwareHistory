--
-- add-time.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local interval = kernel.getactortag( kernel.getlasthitactorname() )

if kernel.getlasthitactorname() == "meridiem_button" then
	local meridiem = kernel.getactortext( kernel.getlasthitactorname() )
	
	if meridiem == "PM" then
		interval = "-" .. interval
	end
end

working_date = working_date + interval
refresh_actors(working_date)