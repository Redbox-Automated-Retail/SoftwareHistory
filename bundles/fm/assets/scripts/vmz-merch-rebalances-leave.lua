--
-- vmz-merch-rebalances-leave.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

vmz_merch_rebalances_timer:stop()
if kernel.timerexists("vmz_merch_rebalances_timer") then
	kernel.timerremove("vmz_merch_rebalances_timer")
end

idle_timer:start()