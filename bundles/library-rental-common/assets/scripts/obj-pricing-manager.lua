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
	
	t.update_default_price_set = function ( self, price_data )
		--price_data={ 
		--	Movie={ 
		--		default={ purchase_price=7, extra_night=1.2, expiration_price=1.2, initial_night=1.2, pricesetid=1, non_return=24, non_return_days=20 },
		--		BluRay={ extra_night=1.5, expiration_price=1.5, initial_night=1.5, pricesetid=1, non_return=33, non_return_days=22 }
		--	},
		--	Game={
		--		default={ extra_night=2, expiration_price=2, initial_night=2, pricesetid=1, non_return=58, non_return_days=29 }
		--	}
		--}
		kernel.pricing_service_update_current_price_set( price_data )
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