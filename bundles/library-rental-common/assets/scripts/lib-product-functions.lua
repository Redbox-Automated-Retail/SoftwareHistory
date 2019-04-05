--
-- lib-product-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

ProductCatalog.has_many_families = function( self )
	local distinct_families = {}
	local default_family = nil
	for k, v in self:pairs() do
		local product = MasterProductCatalog[v.product_id]
		if product ~= nil then
			local product_type = ProductTypes[product.product_type_id]
			if  product_type ~= nil
			and product_type.product_type_name ~= "Promo Code"
			and product_type.product_type_name ~= "Unknown" then
				distinct_families[product_type.product_family_id] = "family"
				if default_family == nil then
					default_family = ProductFamilies[product_type.product_family_id].product_family_name
				end
			end
		end
	end
	return #distinct_families > 1, default_family
end
