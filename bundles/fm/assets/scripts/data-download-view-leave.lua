--
-- data-download-view-leave.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


download_monitor:stop()
kernel.timerremove("download-monitor")
data_download_pager:reset()
if check_state_timer ~= nil then
	check_state_timer:stop()
	kernel.timerremove("check_update_state_timer")
end