--
-- cust-name-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local first = kernel.getmacro("CustName:First")
local last = kernel.getmacro("CustName:Last")

if #first < 1 or #last < 1 then
	kernel.disableactor("ok_button")
	kernel.setactorimage("image_next_right_arrow_btn","image_next_right_arrow_gray")
	kernel.disableactor("image_next_right_arrow_btn")
end

if current_email_manager ~= nil then
	current_email_manager.edit_first = true
end