--
-- lib-product-functions.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


EmployeeStores = {
	[14129] = true,
	[22690] = true,
	[24625] = true
}

TVRatings = {
	[21] = { rating_id = 21, name = "TVMA" },
	[22] = { rating_id = 22, name = "TVPG" },
	[23] = { rating_id = 23, name = "TV14" },
	[24] = { rating_id = 24, name = "TVG" },
	[25] = { rating_id = 25, name = "TVY" },
	[26] = { rating_id = 26, name = "TVY7" }
}

Ratings = {
	[5] =  { rating_id = 5,  name = "G" },
	[6] =  { rating_id = 6,  name = "PG" },
	[7] =  { rating_id = 7,  name = "PG-13" },
	[8] =  { rating_id = 8,  name = "R" },
	[10] = { rating_id = 10, name = "NR" }
}

local function in_stock( product_id, history )
	local cart_count, as_a_rental = ShoppingCart:count_for_item( product_id )
	if cart_count > 0 and as_a_rental and StoreManager:is_sell_thru_enabled() == false then
		return false
	end
	
	if history == nil then
		history = ProductHistory[product_id]
	end
	
	if history == nil then
		return false
    elseif history.on_hand_count == nil then
        return false
	else
		return (history.on_hand_count - cart_count) > 0
	end
end

local function multi_disc_in_stock( product_id_list )
	for k, v in pairs( product_id_list ) do
		if in_stock( v ) == false then
			return false
		end
	end
	return true
end

local function out_of_stock_within_expiration_date( product_id, history )
	if nil == product_id then error( "product_id cannot be nil!" ) end
	if nil == history then return false end
	if nil == history.on_hand_count then return false end
	if history.on_hand_count > 0 then return false end
	if nil == history.last_date_out_of_stock then return false end
	if history.thinned_out == true then return false end
	local days = kernel.getelapseddays( history.last_date_out_of_stock, get_current_date_string( "%4d%02d%02d" ) )
	return not (days > 25)
end

local function multi_disc_out_of_stock_within_expiration_date( product_id_list )
	local function any_disc_out_of_stock_within_expiration_date()
		for k, v in pairs( product_id_list ) do
			if out_of_stock_within_expiration_date( v, ProductHistory[v] ) == true then
				return true
			end
		end
		return false
	end
	
	local function any_disc_never_in_stock()
		for k, v in pairs( product_id_list ) do
			if in_stock( v ) == false and out_of_stock_within_expiration_date( v, ProductHistory[v] ) == false then
				return true
			end
		end
		return false
	end
	
	if any_disc_out_of_stock_within_expiration_date() == true and any_disc_never_in_stock() == false then
		return true
	end
	return false
end

ProductFamilies.get_family = function( self, id )
	for k, v in self:ipairs() do
		if v.product_family_id == id
		or v.product_family_name == id then
			return v
		end
	end
	return nil
end

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

ProductCatalog.has_mature_rating = function( self, catalog_entry )
	return catalog_entry ~= nil
		and (catalog_entry.rating_id == 8
			or catalog_entry.rating_id == 18
			or catalog_entry.rating_id == 20
			or catalog_entry.rating_id == 21)
end

ProductCatalog.out_of_stock_within_expiration_date = function( self, product_id, history )
	if is_multi_disc( product_id ) == true then
		return multi_disc_out_of_stock_within_expiration_date( ProductGroup[MultiDiscXREF[product_id].product_group_id].product_id_list )
	else
		return out_of_stock_within_expiration_date( product_id, history )
	end
end

ProductCatalog.out_of_stock_within_expiration_date_product = function( self, product )
	if product ~= nil and product.kiosk_product_type ==  3 then
		local dvd_product = nil
		local blu_ray_product = nil
		if product.dvd_index ~= nil and product.dvd_index > 0 then
			dvd_product = product.kiosk_products[product.dvd_index]
		end
		if product.blu_ray_index ~= nil and product.blu_ray_index > 0 then
			blu_ray_product = product.kiosk_products[product.blu_ray_index]
		end
		
		local dvd_out_of_stock_within_expiration_date = false
		local blu_ray_out_of_stock_within_expiration_date = false
		if dvd_product ~= nil and dvd_product.product_id ~= nil then
			dvd_out_of_stock_within_expiration_date = self:out_of_stock_within_expiration_date( dvd_product.product_id )
		end
		if blu_ray_product ~= nil and blu_ray_product.product_id ~= nil then
			blu_ray_out_of_stock_within_expiration_date = self:out_of_stock_within_expiration_date( blu_ray_product.product_id )
		end
		
		return dvd_out_of_stock_within_expiration_date and blu_ray_out_of_stock_within_expiration_date
	elseif product ~= nil and product.product_id ~= nil then
		return self:out_of_stock_within_expiration_date( product.product_id )
	end
	return false
end

ProductCatalog.product_type_count = function( self, filtered_products, type_id, in_stock_only )
	local count = 0
	if filtered_products ~= nil and type_id ~= nil and in_stock_only ~= nil then
		for k, v in pairs( filtered_products ) do
			if  v ~= nil
			and ( self:in_stock(v.product_id) == true or in_stock_only == false )
			and (v.product_type_id == type_id) then
				count = count + 1
			end
		end
	end
	return count
end

ProductCatalog.get_copy_with_release_date = function( self )
	local temp_product_catalog = {}
	for k, v in ProductCatalog:pairs() do
		local temp_product = {}
		temp_product["product_id"] = v.product_id
		temp_product["in_stock"] = self:in_stock( v.product_id )
		temp_product["is_available"] = v.is_available
		temp_product["can_be_sold"] = v.can_be_sold
		local master_product = MasterProductCatalog[v.product_id]
		if master_product ~= nil
		and master_product.merchandise_date ~= nil
		then
			temp_product["merchandise_date"] = MasterProductCatalog[v.product_id].merchandise_date
		end
		table.insert(temp_product_catalog, temp_product)
	end
	return temp_product_catalog
end

ProductCatalog.in_stock_games_count = function( self )
	local count = 0
	for _, v in ProductCatalog:pairs() do
		if v ~= nil then
			local product = MasterProductCatalog[v.product_id]
			if product ~= nil then
				local dnr = false
				if product.do_not_rent_date ~= nil then
					if product.do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" ) then
						dnr = true
					end
				end
				
				local product_type = ProductTypes[product.product_type_id]
				if product_type ~= nil
				and product_type.product_family_id == 2
				and self:in_stock( v.product_id ) == true
				and dnr == false then
					count = count + 1
				end
			end
		end
	end
	return count
end

ProductCatalog.in_stock = function( self, product_id, history )
	if is_multi_disc( product_id ) == true then
		return multi_disc_in_stock( ProductGroup[MultiDiscXREF[product_id].product_group_id].product_id_list )
	else
		return in_stock( product_id, history )
	end
end

ProductCatalog.in_stock_product = function( self, product )
	if product ~= nil and product.kiosk_product_type ==  3 then
		local dvd_product = nil
		local blu_ray_product = nil
		if product.dvd_index ~= nil and product.dvd_index > 0 then
			dvd_product = product.kiosk_products[product.dvd_index]
		end
		if product.blu_ray_index ~= nil and product.blu_ray_index > 0 then
			blu_ray_product = product.kiosk_products[product.blu_ray_index]
		end
		
		local dvd_in_stock = false
		local blu_ray_in_stock = false
		if dvd_product ~= nil and dvd_product.product_id ~= nil then
			dvd_in_stock = self:in_stock( dvd_product.product_id )
		end
		if blu_ray_product ~= nil and blu_ray_product.product_id ~= nil then
			blu_ray_in_stock = self:in_stock( blu_ray_product.product_id )
		end
		
		return dvd_in_stock or blu_ray_in_stock
	elseif product ~= nil and product.product_id ~= nil then
		return self:in_stock( product.product_id )
	end
	return false
end