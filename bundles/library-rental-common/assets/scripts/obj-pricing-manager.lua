--
-- obj-pricing-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newpricingmanager()
	local t = {}

	t.reload = function( self )
		kernel.log( ">  Looking for pricing configuration..." )
		local pricing_config_name = kernel.config_getvalue( "system", "General", "PricingTableName" )
		
		if pricing_config_name == nil then
			pricing_config_name = "pricing"
			kernel.log( ">  Default to 'pricing' and standard pricing process..." )
		else
			kernel.log( ">  Found pricing configuration '" .. pricing_config_name .. "'." )
			local try_pricing_file = kernel.gettable( pricing_config_name )
			
			if try_pricing_file == nil then
				kernel.log( ">  Pricing configuration file not found..." )
				pricing_config_name = "pricing"
				kernel.log( ">  Default to 'pricing' and standard pricing process..." )
			end
		end

		kernel.log( "PriceManager.Reload()" )
		-- Check for price changes from currently loaded values
		local price_has_changed = false
		if self.StandardPricing ~= nil then
			kernel.log( ">  Checking for price changes..." )

			-- We only compare the standard set because the web site does not get the title_price or pricing_other data
			local compare_set = kernel.gettable( pricing_config_name )
			for k, v in ipairs( compare_set ) do
				v.non_return = self:calculate_non_return_amount( v.initial_night, v.non_return_days )
			end
			
			price_has_changed = deepcompare( self.StandardPricing.Rows, compare_set ) ~= true
		end

		-- Clear existing data
		self.StandardPricing = {}
		self.AlternatePriceSets = {}
		self.AlternatePriceTitles = {}

		-- Standard Pricing and Release Date are used if title does not have an override
		self.StandardPricing.Rows = kernel.gettable( pricing_config_name )
		for k, v in ipairs( self.StandardPricing.Rows ) do
			v.non_return = self:calculate_non_return_amount( v.initial_night, v.non_return_days )
		end
		self.StandardPricing.PriceSetID = kernel.getresourceproperty( pricing_config_name, "price_set_id" )

		-- Load any titles with alternate prices
		kernel.log( ">  Check for Titles assigned alternate pricing" )
		local title_price = kernel.gettable( "title_price" )
		for k, v in ipairs( title_price ) do
			kernel.log( ">  id=" .. tostring( v.title_id ) .. ", p=" .. tostring( v.price_resource ) )
			local product = MasterProductCatalog[ v.title_id ]
			if product ~= nil then
				local now = get_current_date_string( "%4d%02d%02d%02d%02d" )
				kernel.log( ">  rd=" .. tostring( product.merchandise_date ) .. ", xar=" .. tostring( v.expire_at_release ) )
				-- If hive release date already passed, price exception no longer applies
				if v.expire_at_release == false or ( v.expire_at_release == true and now < product.merchandise_date ) then
					-- Add this title to the list
					self.AlternatePriceTitles[ v.title_id ] = { 
						price_resource = v.price_resource,
						release_early_days = v.release_early_days,
						expire_at_release = v.expire_at_release
					}
					
					-- Load only the additional price sets used by the alternate priced titles
					if self.AlternatePriceSets[ v.price_resource ] == nil then
						local alt_data = kernel.gettable( v.price_resource )
						if alt_data ~= nil then
							self.AlternatePriceSets[ v.price_resource ] = {
								PriceSetID = kernel.getresourceproperty( v.price_resource, "price_set_id" ),
								Rows = alt_data
							}
							kernel.log( "> Alternate price set: " .. v.price_resource .. " added to cache" )
						end
					end
				end
			end
		end
		kernel.log( ">  Title alternate price check COMPLETE" )
		
		-- If the price has changed the web broker must re-register after the new prices have been loaded
		if price_has_changed then
			kernel.log( ">  Price change detected; force web broker to re-register" )
			ReservationManager:init()
		end
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
		
		price_set = self.StandardPricing
		for k, v in ipairs( price_set.Rows ) do
			if v.family == string.lower( "movies" ) then
				if v.type == string.lower( "dvd" ) then
					v.initial_night = price_data.Movie.default.initial_night
					v.extra_night = price_data.Movie.default.extra_night
					v.non_return = price_data.Movie.default.non_return
					v.non_return_days = price_data.Movie.default.non_return_days
					v.expiration_price = price_data.Movie.default.expiration_price
				elseif v.type == string.lower( "blu-ray" ) then
					v.initial_night = price_data.Movie.BluRay.initial_night
					v.extra_night = price_data.Movie.BluRay.extra_night
					v.non_return = price_data.Movie.BluRay.non_return
					v.non_return_days = price_data.Movie.BluRay.non_return_days
					v.expiration_price = price_data.Movie.BluRay.expiration_price
				end
			elseif v.family == string.lower ( "games" ) then
				v.initial_night = price_data.Game.default.initial_night
				v.extra_night = price_data.Game.default.extra_night
				v.non_return = price_data.Game.default.non_return
				v.non_return_days = price_data.Game.default.non_return_days
				v.expiration_price = price_data.Game.default.expiration_price
			end
		end
	end
	
	t.is_early_released = function( self, product )
		if product == nil then
			return false
		end
		local alt_title = self.AlternatePriceTitles[ product.product_id ]
		if alt_title ~= nil then
			local now = get_current_date_string( "%4d%02d%02d%02d%02d" )
			local date_diff = kernel.getelapseddays( now, product.merchandise_date )
			if date_diff ~= nil 
			and alt_title.release_early_days ~= nil
			and alt_title.release_early_days >= date_diff
			then
				return true
			end
		end
		return false
	end
	
	t.get_early_release_date = function( self, product )
		if (self:is_alternate_priced(product.product_id) == true) then
			local release_date = kernel.datetimeparsefromstringdate(product.merchandise_date)
			local early_release_date = kernel.dateadd(release_date, "-"..tostring(self.AlternatePriceTitles[product.product_id].release_early_days))
			return string.format( "%4d%02d%02d%02d%02d", early_release_date.year, early_release_date.month, early_release_date.day, early_release_date.hour, early_release_date.min )
		else
			return product.merchandise_date
		end
	end
	
	t.is_alternate_priced = function( self, product_id )
		return self.AlternatePriceTitles[ product_id ] ~= nil
	end
	
	t.get_empty_case_price = function( self )
		return self:get_price_setting( "movies", "dvd", "initial_night", "purchase" )
	end
	
	t.get_product_price = function( self, product_id )
		local product = MasterProductCatalog[product_id]
		if product ~= nil then
			local product_type = ProductTypes[product.product_type_id]
			local product_family = ProductFamilies[product_type.product_family_id]

			-- Check for Title exclusions first
			local alt_price = self.AlternatePriceTitles[ product_id ]
			if alt_price ~= nil then
				kernel.log( tostring( product_id ) .. " assigned to Alternate Price set" )
				local result = self:get_product_type_price( 
					string.lower(product_family.product_family_name), 
					string.lower(product_type.product_type_name),
					alt_price.price_resource )
				result[ "pricesetid" ] = self.AlternatePriceSets[ alt_price.price_resource ].PriceSetID
				return result
			end

			-- Standard Pricing Applies
			local result = self:get_product_type_price( 
				string.lower(product_family.product_family_name), 
				string.lower(product_type.product_type_name),
				nil )
			result[ "pricesetid" ] = self.StandardPricing.PriceSetID
			return result
		end
	end
	
	t.get_product_type_price = function( self, product_family, product_type, alternate_pricing, for_rop_flow )
		if( product_family ~= nil and product_type ~= nil ) then
			local price_set = nil
			if alternate_pricing ~= nil then
				price_set = self.AlternatePriceSets[ alternate_pricing ]
			end
			if price_set == nil then
				price_set = self.StandardPricing
			end

			for k, v in ipairs( price_set.Rows ) do
				if v.family == string.lower( product_family ) 
				and (v.type == string.lower( product_type ) or v.type == "*" ) then
					if for_rop_flow == true then
						return {
							initial_night = v.initial_night,
							extra_night = v.extra_night,
							non_return = v.non_return,
							non_return_days = v.non_return_days,
							expiration_price = v.expiration_price
						}
					else
						return {
							initial_night = self:get_adjusted_price( v.initial_night, "rental"),
							extra_night = self:get_adjusted_price( v.extra_night, "rental"),
							non_return = v.non_return,
							non_return_days = v.non_return_days,
							expiration_price = self:get_adjusted_price( v.expiration_price, "rental")
						}
					end
				end
			end
		end
	end
	
	t.get_adjusted_price = function( self, price, action )
		if StoreManager.store ~= nil and StoreManager.store.state ~= nil and StoreManager.store.state == "MD" then
			local tax_rate = 0
			if action == "rental" then
				-- Need the real tax rate, not the override so go direct to the database row
				tax_rate =  StoreManager.store.rental_tax_rate
			elseif action == "purchase" then
				-- Need the real tax rate, not the override so go direct to the database row
				tax_rate = StoreManager.store.purchase_tax_rate
			end

			if tax_rate ~= 0 then
				return round( price * ( ( tax_rate / 100 ) + 1 ), 2 )
			end
		end
		return price
	end

	t.get_price_setting = function( self, product_family, product_type, setting_name, action )
		if( product_family ~= nil and product_type ~= nil ) then
			for k, v in ipairs( self.StandardPricing.Rows ) do
				if v.family == string.lower(product_family)
				and (v.type == string.lower(product_type) or v.type == "*") then
					local setting_value = v[ setting_name ]
					if setting_value ~= nil and setting_name ~= "non_return" and setting_name ~= "non_return_days" then
						setting_value = self:get_adjusted_price( setting_value, action )
					end
					return setting_value
				end
			end
		end
	end

	t.get_broker_service_prices = function( self, for_rop_flow )
		local dvdPrice = self:get_product_type_price( "movies", "dvd", null, for_rop_flow )
		local blurayPrice = self:get_product_type_price( "movies", "blu-ray", null, for_rop_flow )
		local gamePrice = self:get_product_type_price( "games", "*", null, for_rop_flow )
		local servicePrices = {
			Movie = {
				default = {
					initial_night = dvdPrice.initial_night,
					extra_night = dvdPrice.extra_night,
					non_return = dvdPrice.non_return,
					non_return_days = dvdPrice.non_return_days,
					purchase_price = StoreManager:get_purchase_price(),
					expiration_price = dvdPrice.expiration_price,
					pricesetid = StoreManager.PricingManager.StandardPricing.PriceSetID or "1" },
				BluRay  = {
					initial_night = blurayPrice.initial_night,
					extra_night = blurayPrice.extra_night,
					non_return = blurayPrice.non_return,
					non_return_days = blurayPrice.non_return_days,
					expiration_price = blurayPrice.expiration_price,
					pricesetid = StoreManager.PricingManager.StandardPricing.PriceSetID or "1" },
					},
			Game  = {
				default = {
					initial_night = gamePrice.initial_night,
					extra_night = gamePrice.extra_night,
					non_return = gamePrice.non_return,
					non_return_days = gamePrice.non_return_days,
					expiration_price = gamePrice.expiration_price,
					pricesetid = StoreManager.PricingManager.StandardPricing.PriceSetID or "1" },
					}
			}
		return servicePrices
	end
	
	t.calculate_non_return_amount = function( self, initial_night_amount, non_return_days )
		if StoreManager.store ~= nil and StoreManager.store.state ~= nil and StoreManager.store.state == "MD" then
			return kernel.round( initial_night_amount * ( ( StoreManager.store.rental_tax_rate / 100 ) + 1 ), 2 ) * non_return_days
		end
		return initial_night_amount * non_return_days
	end
	
	return t
end