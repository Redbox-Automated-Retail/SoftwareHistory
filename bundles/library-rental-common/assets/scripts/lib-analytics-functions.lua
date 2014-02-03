--
-- lib-analytics-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


SubmitAnalyticsPricing = function( session_id)
	if StoreManager ~= nil and StoreManager.PricingManager ~= nil then
		kernel.analyticsaddnode( "session", "pricing", session_id )
		for k, v in ipairs( StoreManager.PricingManager.StandardPricing.Rows ) do
			kernel.analyticsaddnode( "pricing", "productPrice", session_id )
			kernel.analyticsaddattribute( "productPrice", "productFamily", v.family, session_id )
			kernel.analyticsaddattribute( "productPrice", "productType", v.type, session_id )
			kernel.analyticsaddattribute( "productPrice", "currency", v.currency or "USD", session_id )
			local priceTable = 
				{
					firstNightAmt = tostring(v.initial_night),
					extraNightAmt = tostring(v.extra_night),
					nonReturnAmt = tostring(v.non_return),
					nonReturnDays = tostring(v.non_return_days),
					expirationPrice = tostring(v.expiration_price),
					pricesetid = StoreManager.PricingManager.StandardPricing.PriceSetID or "1"
				}
			kernel.analyticsaddelements( "productPrice", priceTable, session_id )
		end
	end
end

