--
-- lib-promo-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local function is_excluded( exclude_ids, product_id )
    for _, id in ipairs( exclude_ids ) do
        if id == tostring( product_id ) then
            return true
        end
	end
    return false
end

