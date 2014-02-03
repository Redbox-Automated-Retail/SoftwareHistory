--
-- obj-store-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newstoremanager()
	local t = {}

	t.init = function( self )
		self.store_number = kernel.getmachinevalueordefault(
			"Store",
			"ID",
			"N/A" )
		self:update_store()
		self.PricingManager = newpricingmanager()
	end

	t.get_number = function( self )
		return self.store_number
	end

	t.get_return_time = function( self )
		if self.store == nil then
			return "9:00PM"
		end
		return self.store.return_time
	end

	t.get_rental_tax_rate = function( self )
		if self.store == nil then
			return 0
		end
		if self.store.state == "MD" then
			return 0
		else
			return self.store.rental_tax_rate or 0
		end
	end

	t.get_purchase_tax_rate = function( self )
		if self.store == nil then
			return 0
		end
		if self.store.state == "MD" then
			return 0
		else
			return self.store.purchase_tax_rate or 0
		end
	end

	t.get_purchase_price = function( self )
		if self.store == nil then
			return 0
		end
		return self.PricingManager:get_adjusted_price( self.store.purchase_price, "purchase" )
	end

	t.get_purchase_new_price = function( self )
		if self.PricingManager ~= nil then
			local new_dvd_price = self.PricingManager:get_price_setting( "movies", "newdvd", "initial_night", "rental" )
			if new_dvd_price ~= nil then
				new_dvd_price = self.PricingManager:get_adjusted_price( new_dvd_price, "purchase" )
			end
			return new_dvd_price
		end
		return nil
	end

	t.is_sell_thru_enabled = function( self )
		local enabled = self.store ~= nil
			and self.store.sell_thru
			and EnableSellThruOfUsed
		if  self.is_old_enough == nil
		and self.store ~= nil
		and self.store.open_date ~= nil then
			local now = os.date( "*t" )
			local end_date = string.format(
				"%4d%02d%02d",
				now.year,
				now.month,
				now.day )
			local days_elapsed = kernel.getelapseddays(
				self.store.open_date,
				end_date )
			self.is_old_enough = days_elapsed ~= nil and days_elapsed >= 60
		end
		return enabled and self.is_old_enough
	end

	t.has_sell_thru_new_in_stock = function()
		for _, v in ProductCatalog:pairs() do
			local check_product = MasterProductCatalog[v.product_id]
			if check_product ~= nil
			and check_product.sell_thru_new == true
			and check_product.product_type_id == 1
			and v.in_stock == true then
				return true
			end
		end
		return false
	end

	t.set_number = function( self, value )
		self.store_number = value
		self:update_store()
		kernel.setstorenumber( self.store_number )
		kernel.reenterview()
	end

	t.update_store = function( self )
		self.store = Stores[self.store_number]
		if self.store ~= nil then
			kernel.setmacro( "Store:ReturnTime", self.store.return_time )
			kernel.setfilter( "state", self.store.state )
		else
			-- Mingle #1500 do not show this popup - requested by the Field Operations
			-- Cannot conver to Lua based MessageBox because there is no scene yet, thus can't add actors...
			--kernel.showmessage( "No Store is defined for Store Number: " ..
			--	self.store_number .. ", use the Control Panel to configure the store before proceeding.")
		end
		kernel.setstorenumber( self.store_number )
		kernel.setmacro( "StoreNumber",	self.store_number )
	end
	
	t.reload_pricing_data = function( self, ui_version )
		-- Product Pricing
		self.PricingManager:reload()
		-- In cart promotion
		LoadInCartData()
		-- Email Opt In Campaign
		EmailOptinCampaign = GetKioskOptinCampaign()
		kernel.log( "EmailOptin loaded: " .. tostring( kernel.getresourceproperty( "opt_in_campaign", "program_name" )))
	end
	
	t.check_product_catalog_for_games_platforms = function( self )
		local _ProductCatalog = ProductCatalog
		for _, v in _ProductCatalog:pairs() do
			local product = MasterProductCatalog[ v.product_id ]
			local product_type = product ~= nil and ProductTypes[product.product_type_id] or nil
			if product ~= nil and product_type ~= nil and product_type.product_family_id == 2 and v.in_stock == true then
				update_game_has_content_registry_keys( product_type )
			end
		end
	end
	
	t.refresh_rapid_thins = function( self )
		if use_rapid_thin_file() == false then
			kernel.log( "RapidThin file is old, does not exist, or contians no data. Rapid thin refresh skipped." )
			return
		end
			
		local count_POS = 0
		local count_PRD = 0
		for k, v in RapidThinBarcodes:pairs() do
			local barcodes = ProductToBarcode[ v.product_id ]
			if barcodes ~= nil and barcodes[k] ~= nil then
				if barcodes[k].status ~= "Reserved" and barcodes[k].status ~= v.status then
					if v.status == "Thinned"
					and RebalancesUnThinInuse ~= nil 
					and RebalancesUnThinInuse[ k ] ~= nil then
						kernel.log(" > Unthin detected in refresh_rapid_thins: " .. tostring( k ) )
						barcodes[k].status = "Known"
					else
						barcodes[k].status = v.status 
						count_PRD = count_PRD + 1
					end
				end
				ProductToBarcode[ v.product_id ] = barcodes
			end
		end

		for position_key, position_info in BarcodePosition:pairs() do
			local rapid_thin = RapidThinBarcodes[ position_info.barcode ]
			if rapid_thin ~= nil then
				if rapid_thin.status == "Thinned"
				and RebalancesUnThinInuse ~= nil 
				and RebalancesUnThinInuse[ position_info.barcode ] ~= nil then
					kernel.log(" > Unthin detected in refresh_rapid_thins: " .. tostring( position_info.barcode ) )
					position_info.status = "Known"
				else
					position_info.status = rapid_thin.status
				end
				BarcodePosition[position_key] = position_info
				count_POS = count_POS + 1
			end
		end
		kernel.log( "RapidThin refresh updated " .. tostring( count_POS ) .. " barcode position records." )
		kernel.log( "RapidThin refresh updated " .. tostring( count_PRD ) .. " product to barcode records." )
	end		
	
	t.is_single_billing = function( self )
		if self.store ~= nil and self.store.collection_method ~= nil then
			return kernel.storesvcs_issinglebilling( self.store.collection_method )
		end
		return false
	end
	
	
	
	return t
end
