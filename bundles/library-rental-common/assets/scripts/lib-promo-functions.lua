--
-- lib-promo-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function GetInCartPromoCode()
    return kernel.get_in_cart_promo_code()
end

function GetKioskOptinCampaign()
	local kiosk_campaigns = kernel.gettable( "opt_in_campaign" )
	return kiosk_campaigns[1]
end

local function is_excluded( exclude_ids, product_id )
    for _, id in ipairs( exclude_ids ) do
        if id == tostring( product_id ) then
            return true
        end
	end
    return false
end

