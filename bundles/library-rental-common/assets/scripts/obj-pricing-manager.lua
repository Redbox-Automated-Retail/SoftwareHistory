--
-- obj-pricing-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newpricingmanager()
	local t = {}

    t.get_current_price_set = function(self)
        return kernel.pricing_service_get_current_price_set()
    end

	t.reload = function( self )
        kernel.reload_pricing_service()
	end
	
	t.is_early_released = function( self, product )
        -- early release code was never used and has been removed
        return false
	end
	
	t.get_early_release_date = function( self, product )
        -- early release code was never used and has been removed
        return product.merchandise_date
	end
	
	t.is_alternate_priced = function( self, product_id )
        -- early release code was never used and has been removed
        return false
	end
	
	t.get_empty_case_price = function( self )
        return kernel.pricing_service_get_price_setting( "movies", "dvd", "initial_night", "purchase" )
	end
	
	t.get_product_price = function( self, product_id )
        return kernel.pricing_service_get_pricing_record(product_id)
	end
	
	t.get_product_type_price = function( self, product_family, product_type, alternate_pricing, for_rop_flow )
        return kernel.pricing_service_get_product_type_price(product_family, product_type, for_rop_flow )
	end
	
	t.get_adjusted_price = function( self, price, action )
        return kernel.pricing_service_get_adjusted_price( price, action )
	end

	t.get_price_setting = function( self, product_family, product_type, setting_name, action )
        return kernel.pricing_service_get_price_setting( product_family, product_type, setting_name, action )
	end

	return t
end