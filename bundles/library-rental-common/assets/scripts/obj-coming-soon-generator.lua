--
-- obj-coming-soon-generator.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newcomingsoongenerator()

	local t = {}

	t.create_browse_titles = function( self )
		kernel.log( "Generate coming soon titles " )
		local _ProductCatalog = ProductCatalog

		local add_count = 0
		local coming_soon_product_ids = kernel.get_coming_soon_product_ids() or {}
		for _, product_id in pairs(coming_soon_product_ids) do
			if _ProductCatalog[ product_id ] == nil then
				add_title_without_barcodes( product_id )
				add_count = add_count + 1
				kernel.log( "> coming soon title " .. tostring( product_id ) .. " added to ProductCatalog and ProductHistory" )
			else
				local history = ProductHistory[ product_id ]
				if history ~= nil then
					kernel.log( "> coming soon title " .. tostring( product_id ) .. " updated in ProductCatalog" )
					create_or_update_product_catalog_entry( product_id, history )
				else 
					kernel.log( "> coming soon title " .. tostring( product_id ) .. " already exist in product catalog and history tables" )
				end
			end
		end
		kernel.log( "> created " .. tostring( add_count ) .. " new coming soon titles" )
	end

	return t
end
