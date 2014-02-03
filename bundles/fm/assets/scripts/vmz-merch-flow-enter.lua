--
-- vmz-merch-flow-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:stop()

if vmz_merch_flow_functions == nil then
	vmz_merch_flow_functions = newvmzmerchflowfunctions() 
end

if vmz_merch_flow_functions.loaded ~= true then
	vmz_merch_flow_functions:load()
else
	vmz_merch_flow_functions:configurestep()
end