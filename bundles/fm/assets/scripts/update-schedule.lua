--
-- update-schedule.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
--local now = newdatetime()
--if working_date < now then
	--kernel.showmessage("Job will start immediately since time selected is in the past")
--end

schedule_time = newdatetime(working_date.backing)
view_goback()