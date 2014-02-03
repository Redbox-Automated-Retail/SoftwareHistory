--
-- machine-full-view-leave.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

machine_full_idle_timer:stop()
if kernel.timerexists("machine_full_idle_timer") then
	kernel.timerremove("machine_full_idle_timer")
end

idle_timer:start()
