--
-- obj-coming-soon-generator.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newcomingsoongenerator()

	local is_in_date_range2 = function( release_date_string, limit_days )
	
		local current = os.date( "*t" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )

		if VerboseLogging == true then
			kernel.log( "> Release = " .. release_date_string )
			kernel.log( "> Days = " .. tostring( limit_days ) )
			kernel.log( "> Current = " .. current_string )
		end
		
		if release_date_string > current_string then
			local days_before_release = kernel.getelapseddays( current_string, release_date_string )

			if VerboseLogging == true then
				kernel.log( "> Elapsed Days = " .. tostring( days_before_release ))
			end

			if days_before_release ~= nil and days_before_release <= limit_days then
				return true
			end
		end
		return false
	end

	local t = {}

	t.create_browse_titles = function( self )
		kernel.log( "Generate coming soon titles for Browse view from Master product catalog" )
		-- cache globals to locals for improved performance
		local _MasterProductCatalog = MasterProductCatalog
		local _ProductCatalog = ProductCatalog

		local add_count = 0
		for _, product in _MasterProductCatalog:pairs() do
			if product.coming_soon_days ~= nil
			and product.coming_soon_days > 0
			and is_in_date_range2( product.merchandise_date, product.coming_soon_days ) 
			then
				if _ProductCatalog[ product.product_id ] == nil then
					add_title_without_barcodes( product.product_id )
					add_count = add_count + 1
					kernel.log( "> coming soon title " .. tostring( product.product_id ) .. " added to ProductCatalog and ProductHistory" )
				else
					local history = ProductHistory[ product.product_id ]
					if history ~= nil then
						kernel.log( "> coming soon title " .. tostring( product.product_id ) .. " updated in ProductCatalog" )
						create_or_update_product_catalog_entry( product.product_id, history )
					end
				end
			end
		end
		kernel.log( "> created " .. tostring( add_count ) .. " new coming soon titles" )
	end

	return t
end
