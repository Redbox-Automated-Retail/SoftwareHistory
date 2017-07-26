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

	t.get_store_zip = function ( self )
		if self.store == nil then
			return 0
		else
			return self.store.zip or 0
		end
	end

	t.get_store_market_id = function ( self )
		if self.store == nil then
			return 0
		else
			return self.store.market_id or 0
		end
	end
	
	t.get_rental_tax_rate = function( self )
		if self.store == nil then
			return 0
		end
		if self.store.state == "MD" or self.store.state == "SC" then
			return 0
		else
			return self.store.rental_tax_rate or 0
		end
	end

	t.get_purchase_tax_rate = function( self )
		if self.store == nil then
			return 0
		end
		if self.store.state == "MD" or self.store.state == "SC" then
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
        return kernel.get_can_sell_used()
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
	
	t.refresh_rapid_statuses = function( self )
		local count_POS = 0
		local count_PRD = 0
		local barcodes_to_unmerchandize = {}

		local rapid_status_all = kernel.rental_inventory_get_rapidstatus_all()
		if rapid_status_all ~= nil then
            
            BarcodeStatus:clear()
            kernel.log( "activated new rapid status file, cleared BarcodeStatus table" )
            
            for k, v in pairs( rapid_status_all ) do
				local barcodes = ProductToBarcode[v.product_id]
				if barcodes ~= nil and barcodes[k] ~= nil then
					if barcodes[k].status ~= "Reserved" and barcodes[k].status ~= v.status then
						barcodes[k].status = v.status 
						count_PRD = count_PRD + 1
					end
					ProductToBarcode[v.product_id] = barcodes
				end
			end
			
			for position_key, position_info in BarcodePosition:pairs() do
				local rapid_status = rapid_status_all[position_info.barcode]
				if rapid_status ~= nil then
					position_info.status = rapid_status.status
					BarcodePosition[position_key] = position_info
					count_POS = count_POS + 1
				elseif position_info.status == "Rebalancing" or position_info.status == "Redeployment" or position_info.status == "Thinned" then
					local row = {}
					row.barcode = position_info.barcode
					table.insert( barcodes_to_unmerchandize, row )
					position_info.status = "Known"
					BarcodePosition[position_key] = position_info
					kernel.log( "Adding barcode: " .. tostring( row.barcode ) .. " to barcodes_to_unmerchandize" )
				end

				local do_not_rent_date = nil
				if BarcodeToProduct[position_info.barcode] ~= nil and BarcodeToProduct[position_info.barcode].product_id ~= nil then
					local product_id = BarcodeToProduct[position_info.barcode].product_id
					if MasterProductCatalog[product_id] ~= nil and MasterProductCatalog[product_id].do_not_rent_date ~= nil then
						do_not_rent_date = MasterProductCatalog[product_id].do_not_rent_date
					end
				end
				if do_not_rent_date ~= nil and do_not_rent_date <= get_current_date_string( "%4d%02d%02d%02d%02d" ) then
					position_info.status = "DNR"
					BarcodePosition[position_key] = position_info
				end
			end
			
			if #barcodes_to_unmerchandize > 0 then
				local hardware_manager = newhardwaremanager()
				kernel.log( "Calling clear_merch_status on " .. tostring( #barcodes_to_unmerchandize ) .. " barcodes" )
				hardware_manager:clear_merch_status( barcodes_to_unmerchandize, nil )
			end
		end
		kernel.log( "RapidStatus refresh updated " .. tostring( count_POS ) .. " barcode position records." )
		kernel.log( "RapidStatus refresh updated " .. tostring( count_PRD ) .. " product to barcode records." )
	end
	
	t.is_single_billing = function( self )
		if self.store ~= nil and self.store.collection_method ~= nil then
			return kernel.storesvcs_issinglebilling( self.store.collection_method )
		end
		return false
	end
	
	
	
	return t
end
