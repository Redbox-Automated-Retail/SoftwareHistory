--
-- obj-reservation-manager.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newreservationmanager()
	local t = {}

    t.vend_manager = newvendmanager()
	t.card_hash_id = nil
	t.session_id = nil

    t.get_broker_service_prices = function( self, for_rop_flow )
        kernel.log("Get broker service prices")
		local dvdPrice = StoreManager.PricingManager:get_product_type_price( "movies", "dvd", null, for_rop_flow )
		local blurayPrice = StoreManager.PricingManager:get_product_type_price( "movies", "blu-ray", null, for_rop_flow )
		local gamePrice = StoreManager.PricingManager:get_product_type_price( "games", "*", null, for_rop_flow )
        local current_price_set = StoreManager.PricingManager:get_current_price_set()
        local current_price_set_id = 1
        if current_price_set ~= nil and current_price_set.PriceSetID ~= nil then
            current_price_set_id = current_price_set.PriceSetID
        end

		local servicePrices = {
			Movie = {
				default = {
					initial_night = dvdPrice.initial_night,
					extra_night = dvdPrice.extra_night,
					non_return = dvdPrice.non_return,
					non_return_days = dvdPrice.non_return_days,
					purchase_price = StoreManager:get_purchase_price(),
					expiration_price = dvdPrice.expiration_price,
					pricesetid = current_price_set_id },
				BluRay  = {
					initial_night = blurayPrice.initial_night,
					extra_night = blurayPrice.extra_night,
					non_return = blurayPrice.non_return,
					non_return_days = blurayPrice.non_return_days,
					expiration_price = blurayPrice.expiration_price,
					pricesetid = current_price_set_id },
					},
			Game  = {
				default = {
					initial_night = gamePrice.initial_night,
					extra_night = gamePrice.extra_night,
					non_return = gamePrice.non_return,
					non_return_days = gamePrice.non_return_days,
					expiration_price = gamePrice.expiration_price,
					pricesetid = current_price_set_id },
					}
			}
		return servicePrices
	end

	t.init = function( self )
		local servicePrices = self:get_broker_service_prices()
		
		kernel.resvsvcs_register3(
			StoreManager:get_number(),
			servicePrices,
			function(
					card_id,
					email,
					reference_number,
					reserve_date,
					products_to_reserve,
					applied_credit,
					applied_hive_online_promo,
					applied_title_marketing,
					customer_number,
					discounted_sub_total,
					grand_total,
					history_title_ids,
					hive_online_promo_discount,
					is_mnp,
					number_of_credits_remaining,
					promo_code,
					sub_total,
					tax,
					reservation_titles,
					is_gc,
					gc_ids,
					cc_ids,
					tax_rate,
					zip_code,
					is_mdv )
					
				local result = { success = false, items = {} }
				local pricesNotAdjusted = self:get_broker_service_prices( true )
				
				local reservation = {
					transaction_date = reserve_date,
					email_address = email,
					barcodes = {},
					prices = pricesNotAdjusted,
					applied_credit = applied_credit,
					applied_hive_online_promo = applied_hive_online_promo,
					applied_title_marketing = applied_title_marketing,
					customer_number = customer_number,
					discounted_sub_total = discounted_sub_total,
					grand_total = grand_total,
					history_title_ids = history_title_ids,
					hive_online_promo_discount = hive_online_promo_discount,
					is_mnp = is_mnp,
					number_of_credits_remaining = number_of_credits_remaining,
					promo_code = promo_code,
					sub_total = sub_total,
					tax = tax,
					reservation_titles = reservation_titles,
					reservation_version = 3,
					is_gc = is_gc,
					gc_ids = gc_ids,
					cc_ids = cc_ids,
					tax_rate = tax_rate,
					zip_code = zip_code,
					is_mdv = is_mdv
				}
				
				local products = {}
				for k, v in pairs( products_to_reserve ) do
					for _, id in ipairs( v ) do
						table.insert(
							products,
							{ product_id = id.product_id, action = k } )
					end
				end
				
				local shopping_cart_barcodes = ShoppingCart:get_barcodes_to_vend()
				local multi_disc_products = {}
                local shopping_cart_item_count = kernel.rental_shopping_cart_item_count( ShoppingCart.session_id )
                for k=1,shopping_cart_item_count do
                    local v = kernel.get_rental_shopping_cart_item( ShoppingCart.session_id, k )
                    if v ~= nil then
					    if is_multi_disc( v.product_id ) == true then
						    local product_id_list = ProductGroup[MultiDiscXREF[v.product_id].product_group_id].product_id_list
						    for k2, v2 in ipairs( product_id_list ) do
							    if v2 ~= v.product_id and ShoppingCart:has_item( v2 ) == false and MasterProductCatalog[v2] ~= nil then
								    table.insert( multi_disc_products, MasterProductCatalog[v2] )
							    end
						    end
					    end
                    end
				end
				local multi_disc_barcodes = get_barcodes_for_products( multi_disc_products )
				for k, v in ipairs( multi_disc_barcodes ) do
					table.insert( shopping_cart_barcodes, v )
				end
				
				local products_visible_to_user = {}
				
				local add_product = function( product )
					if is_multi_disc( product.product_id ) == true then
						local product_id_list = ProductGroup[MultiDiscXREF[product.product_id].product_group_id].product_id_list
						for k, v in ipairs( product_id_list ) do
							table.insert( products_visible_to_user, { product_id = v, action = "rental" } )
						end
					else
						table.insert( products_visible_to_user, { product_id = product.product_id, action = "rental" } )
					end
				end

				local add_product_to_visible_list = function( product )
					if product == nil then
						return
					end
					if product.kiosk_product_type == 3 then --title rollup
						for _,v in pairs(product.kiosk_products) do
							add_product( v )
						end
					else
						add_product( product )
					end
				end

				local process_visible_browse_products = function ()
					local productList = kernel.get_visible_browse_products()
					if productList ~= nil then
						for i=1, #productList do
							add_product_to_visible_list( productList[i] )
						end
					end
				end
                local center_carousel_product = kernel.get_session_value("BrowseState.CenterCarouselProductLuaTable")
				if kernel.peekview() == "start_view" and center_carousel_product ~= nil then
					add_product_to_visible_list( center_carousel_product )		
				end

				if kernel.peekview() == "browse_view" then
					process_visible_browse_products()
				end
				if kernel.peekview() == "product_detail_view" then
					add_product_to_visible_list( kernel.get_session_value("BrowseState.BrowseSelectionSelectedProductLuaTable") )
				end
				if ShoppingCart.recommendations_generator.recommended_list ~= nil and #ShoppingCart.recommendations_generator.recommended_list > 0 then
					for k, v in ipairs( ShoppingCart.recommendations_generator.recommended_list ) do
						table.insert( products_visible_to_user, { product_id = v.product_id, action = "rental" } )
					end
				end
				if kernel.peekview() == "blu_ray_upsell_view" then
                    local blurayUpsellItemCount = kernel.current_rental_shopping_cart_get_bluray_upsell_item_count( false )
                    for k=1,blurayUpsellItemCount do
                        local v = kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index( k )
                        if v ~= nil then
						    if v.offer == nil then
							    table.insert( products_visible_to_user, { product_id = v.blu_ray_product_id, action = "rental" } )
						    end
                        end
					end
				end
				
				local in_play_barcodes = get_barcodes_for_products( products_visible_to_user, shopping_cart_barcodes )
				for _, v in ipairs( in_play_barcodes ) do
					table.insert( shopping_cart_barcodes, v )
				end
				local candidate_barcodes = get_barcodes_for_products(
					products,
					shopping_cart_barcodes )
					
				local account_reservations = Reservations[card_id]
				if account_reservations == nil then
					account_reservations = {}
				end
				for k, v in ipairs( products ) do
					local barcodes = reservation[ "barcodes" ]
					if barcodes[v.action] == nil then
						barcodes[v.action] = {}
					end
					table.insert( barcodes[v.action], v.barcode )
				end
				
				local reservable_product_ids = {}
				local can_reserve_entire_cart = true
				
				for _, v in ipairs( candidate_barcodes ) do
					local product_info = BarcodeToProduct[v]
					if product_info ~= nil then
						table.insert( result.items, { ["product_id"] = product_info["product_id"], ["barcode"] = v, ["status"] = "Fullfilled" } )
						reservable_product_ids[ product_info["product_id"] ] = true
						kernel.log("> ReservationManager: found good product_id : " .. tostring(reservable_product_ids[ product_info["product_id"] ]) )
					end
				end
				--determine which products could not be reserved and store them in the result along with reason
				for k, v in pairs( products ) do
					kernel.log("> ReservationManager: found requsted product_id : " .. tostring(v.product_id) )
					if reservable_product_ids[ v.product_id ] ~= true then
						local status = "ProductSelected"
						if v.status ~= nil and v.status == "InventoryError" then
							status = "InventoryError"
						end
						table.insert( result.items, { ["product_id"] = v.product_id, ["barcode"] = "empty", ["status"] = status } )
						--if there is one item in the cart that can't be reserved then the entire cart cannot be reserved
						can_reserve_entire_cart = false
					end
				end
				if #result.items > 0 and can_reserve_entire_cart == true then
					update_barcode_status( candidate_barcodes, "Reserved" )
					account_reservations[reference_number] = reservation
					Reservations[card_id] = account_reservations -- writes the reservation locally to the database
					result.success = true					
					if need_to_reload_carousel ~= nil and need_to_reload_carousel == true and kernel.peekview() == kernel.get_start_over_view_name() then
                        kernel.reload_carousel(true)
					end
				else
					kernel.log("ReservationManager: Could not satisfy reservation request; unable to reserve requested inventory." )
					--This needs to be nil when inventory can not be allocated 
					result.error_message = nil
					result.success = false
				end
				return result
			end
		)
		
		if not kernel.timerexists( "reservation-expire-timer" ) then
			kernel.timercreate(
				"reservation-expire-timer",
				--kernel.random(1000, 600000),
				30000,
				7200000,
				function()
					ReservationManager:expire()
				end
			)
			kernel.timerstart("reservation-expire-timer")
		end
	end

	t.sync_reservations = function( self )
		--Look for magic cardid 'deddie' if this does not exist then download reservations from the server
		if Reservations[ "deddie" ] == nil then
			kernel.resvsvcs_getreservations(
				StoreManager:get_number(),
				function( r )
					local reserved_barcodes = {}
					for k, v in ipairs( r.properties.reservations ) do
						local account_reservations = Reservations[v.card_id]
						if account_reservations == nil then
							account_reservations = {}
						end
						local reservation = account_reservations[v.reference_number]
						if reservation == nil then
							reservation = {
								transaction_date = v.transaction_date,
								barcodes = v.barcodes
							}
							for _, barcode in ipairs(v.barcodes) do
								reserved_barcodes.insert(barcode)
							end
							account_reservations[v.reference_number] = reservation
							Reservations[v.card_id] = account_reservations
						end
					end
					Reservations[ "deddie" ] = {}
					update_barcode_status(reserved_barcodes, "Reserved")
				end
			)
		end
	end

	t.does_account_have_reservations = function( self )
		return self:get_accounts_reservations() ~= nil
	end

	t.get_key_card_hash_id = function( self, swiped_card_hash_id )
		if Reservations[swiped_card_hash_id] == nil then
			for k, v in Reservations:pairs() do
				for r_k, reservation in pairs( v ) do
					if reservation.reservation_version == 3 then
						for cc_k, cc_hash_id in ipairs( reservation.cc_ids or {} ) do
							if swiped_card_hash_id == cc_hash_id then
								return k
							end
						end
						for gc_k, gc_hash_id in ipairs( reservation.gc_ids or {} ) do
							if swiped_card_hash_id == gc_hash_id then
								return k
							end
						end
					end
				end
			end
		end
		return swiped_card_hash_id
	end

	t.get_account_reservations = function( self, run_expire )
		if run_expire == nil then
			run_expire = false
		end
		if not kernel.trackdataisvalid() then
			return nil
		end
		self.card_hash_id = self:get_key_card_hash_id( kernel.trackgethashid() )
		-- Clean out any expired reservations for this card
		if run_expire == true then
			self:expire(self.card_hash_id)
		end
		return Reservations[self.card_hash_id]
	end

	t.expire = function( self, card_id )
		local expire_time = newdatetime( kernel.datetimeparsefromstringtime( StoreManager:get_return_time() ) )
		kernel.log("Reservation expire running for time " .. tostring( expire_time ) )
		if card_id ~= nil then
			kernel.log("Reservation expire card_id ..." .. string.sub( card_id or "NULL ", -5 ) )
		end
		local current_time = newdatetime()
		if kernel.datecompare( current_time.backing, expire_time.backing ) < 0 then
			expire_time = expire_time - 1
			kernel.log( "Setting expire time back a day since current time is before expire time" )
		end

		local barcodes = {}

		-- We want to expire everything that was reserved before today
		expire_time.backing.min = 0
		expire_time.backing.hour = 0
		expire_time.backing.sec = 0

		local reservations = {}
		for key, value in Reservations:pairs() do
			--Magic cardid deddie
			if key ~= "deddie" then
				if card_id == nil or (card_id ~= nil and key == card_id) then
					reservations[key] = value
				end
			end
		end

		local update_backing = false
		for key, value in pairs(reservations) do
			for reference_number, reservation in pairs(value) do
				local trans_date = newdatetime(kernel.datetimeparsefromstringdate(reservation.transaction_date))
				if trans_date ~= nil
				and expire_time ~= nil
				and kernel.datecompare( trans_date.backing, expire_time.backing ) < 0 then
					kernel.resvsvcs_expire(StoreManager:get_number(), reference_number)
					kernel.log("Expiring Reference Number " .. reference_number )
					for _, codes in pairs(reservation.barcodes) do
						for _, barcode in ipairs(codes) do
							barcodes[#barcodes + 1] = barcode
						end
					end
					update_backing = true
					reservations[key][reference_number] = nil
				else
					kernel.log("Reservation has not expired. Reference Number: " .. reference_number )
				end
			end

			if update_backing == true then
				local count = 0
				for id, reservations in pairs(reservations[key]) do
					count = count + 1
				end
				if count == 0 then
					Reservations:remove(key)
				else
					Reservations[key] = reservations[key]
				end
			end
		end

		update_barcode_status(barcodes, "Known")
	end

	t.start_new = function(self)
        self.vend_manager:init()
		self.cache.barcodes = {}
	end

	t.vend = function( self )
        kernel.log("ReservationManager:vend BEGIN")

		self.session_id = ShoppingCart.session_id
		kernel.analyticsaddnode( "session", "onlinePickup", self.session_id )
		CloseAnalyticsOnStartView = false

        local barcodes_to_vend = self:get_barcodes_to_vend()
        if barcodes_to_vend == nil
        or #barcodes_to_vend == 0 then
            show_maintenance_view( 
				"obj-reservation-manager.lua", 
				"HardwareError", 
				"CheckoutReservedFalseInventory", 
				"barcodes_to_vend is empty", 
				"0032RSVB", 
				false )
            return
        end

		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
				--OnComplete
				kernel.log("ReservationManager:Vend_Manager.OnComplete() BEGIN")
                local account_reservations = self:get_account_reservations()
                local barcodes_to_reset = {}
                local barcode_indexed_results = {}
                for k, v in ipairs( vend_results ) do
                    barcode_indexed_results[v.barcode] = v.status
                end
                for ref, res in pairs( account_reservations ) do
                    local pick_up_results = {}
					local barcode_count = 0
                    for group, codes in pairs( res.barcodes ) do
                        local group_results = {}
                        for _, code in ipairs(codes) do
							
							local multi_disc = false
							if BarcodeToProduct[code] ~= nil and BarcodeToProduct[code].product_id ~= nil then
								multi_disc = is_multi_disc( BarcodeToProduct[code].product_id )
							end
							
							local result = {
                                barcode = code,
								multi_disc = multi_disc,
                                status = barcode_indexed_results[code] or "NotFullfilled"
                            }
                            if result.status ~= "Fullfilled" then
								kernel.log("> Adding " .. tostring( result.barcode ) .. " to reset barcode list" )
                                barcodes_to_reset[#barcodes_to_reset + 1] = result.barcode
                            end
                            group_results[#group_results + 1] = result
							
							local attributes = {
								barcode = result.barcode,
								multi_disc = result.multi_disc,
								status = result.status
							}
							kernel.analyticsaddnode( "onlinePickup", "cartItem", t.session_id )
							kernel.analyticsaddattributes( "cartItem", attributes, t.session_id )
							barcode_count = barcode_count + 1
                        end
                        pick_up_results[group] = group_results
                    end
                    kernel.log("ReservationManager:vend pick_up_results for reservation " .. ref )
                    for group, group_results in pairs( pick_up_results ) do
                        kernel.log("ReservationManager:vend pick_up_results for group " .. group )
                        for _, result in ipairs(group_results) do
                            kernel.log(
                                "ReservationManager:vend group_result barcode " .. tostring( result.barcode ) ..
								" multi_disc " .. tostring( result.multi_disc ) ..
                                " status " .. tostring( result.status ) )
                        end
                    end
					if barcode_count == 0 then
						kernel.log("> Reservation " .. ref .. " had 0 barcodes, skipping Pickup() call" )
					else
						kernel.log("ReservationManager:vend: Calling kernel.resvsvcs_pickup")
						kernel.resvsvcs_pickup(
							StoreManager:get_number(),
							res.email_address,
							ref,
							pick_up_results,
							ShoppingCart.blocklisted,
							nil )
					end
                end
				
				kernel.log("ReservationManger:vend: Removing reservations for ..." ..  string.sub( self.card_hash_id or "NULL ", 1, 5 ) )
				Reservations:remove( self.card_hash_id )

                if #barcodes_to_reset <= 0 then
                    kernel.log( "ReservationManger:vend: No barcodes to reset" )
                else
					kernel.log( "ReservationManger:vend: resetting status to Known for " .. tostring(#barcodes_to_reset) .. " discs" )
					update_barcode_status( barcodes_to_reset, "Known" )
                end
				
				SubmitAnalyticsPricing( t.session_id )
				kernel.analyticsclose( t.session_id, "False" )
				kernel.log("ReservationManager:Vend_Manager.OnComplete() COMPLETE")
				
			end,
			function()
				--OnReady
			end,
			function()
				--OnBusy
			end )
			
		self.vend_manager:prepos_vend_continue()
        kernel.log("ReservationManager:vend COMPLETE")
    end

	t.get_barcodes_to_vend = function( self )
		if self.cache.barcodes == nil
		or #self.cache.barcodes <= 0  then
			self.cache.barcodes = {}
			local reservations = self:get_account_reservations()
			local barcodes = {}
			for _, res in pairs( reservations or {} ) do
				for _, codes in pairs( res.barcodes or {} ) do
					for _, code in ipairs( codes or {} ) do
						barcodes[#barcodes + 1]  = code
					end
				end
			end
			self.cache.barcodes = barcodes
		end
		return self.cache.barcodes
	end

	t.is_reservations_version = function( self, version )
		local reservations = self:get_account_reservations()
		for _, res in pairs( reservations or {} ) do
			if res.reservation_version ~= nil then
				if res.reservation_version ~= version then
					return false
				end
			else 
				-- version 1 is not set in the data,
				-- and these reservervations have reservation_version == nil
				if version ~= 1 then
					return false
				end
			end
		end
	
		if reservations == nil then
			return false
		end
		
		return true
	end

	t.is_gc = function( self )
		local reservations = self:get_account_reservations()
		for _, v in pairs( reservations or {} ) do
			if v.is_gc ~= nil and v.is_gc == true then
				return true
			end
		end
		return false
	end

	t.is_mnp = function( self )
		local reservations = self:get_account_reservations()
		for _, v in pairs( reservations or {} ) do
			if v.is_mnp ~= nil and v.is_mnp == true then
				return true
			end
		end
		return false
	end

	t.is_mdv = function( self )
		local reservations = self:get_account_reservations()
		for _, v in pairs( reservations or {} ) do
			if v.is_mdv ~= nil and v.is_mdv == true then
				return true
			end
		end
		return false
	end

    t.has_purchase = function( self )
        local reservations = self:get_account_reservations()
        for _, v in pairs( reservations or {} ) do
            for action, barcodes in pairs( v.barcodes or {} ) do
                if action == "purchase" then
                    return true
                end
            end
        end
        return false
    end

	t.remove_orphaned_reservations = function( self )
		kernel.log("Remove Orphan Reservations BEGIN")
		local reserved_barcodes = {}
		
		-- Build list of all currently reserved barcodes for faster lookup
		for card_id, reservation_list in Reservations:pairs() do
			for _, res in pairs( reservation_list ) do
				for _, codes in pairs( res.barcodes ) do
					for _, code in ipairs(codes) do
						reserved_barcodes[ code ] = true
						kernel.log( "> found true reserved barcode " .. tostring(code) )
					end
				end
			end
		end

		-- Look from barcode status back into reserved discs and clear any orphans
		for k, v in BarcodePosition:pairs() do
			if v.status == "Reserved" 
			and reserved_barcodes[ v.barcode ] == nil then
				-- Get correct status from corporate inventory
				local barcode_to_product = BarcodeToProduct[ v.barcode ]
				
				if barcode_to_product ~= nil
				and barcode_to_product.product_id ~= nil
				and MasterProductCatalog[ barcode_to_product.product_id ] ~= nil then
                    v.status = get_rapid_status( v.barcode, "Known" )
					if barcode_to_product.product_id ~= nil then
						local barcodes = ProductToBarcode[ barcode_to_product.product_id ]
						if barcodes ~= nil and barcodes[ v.barcode ] ~= nil then 
							barcodes[ v.barcode ].status = v.status
							ProductToBarcode[ barcode_to_product.product_id ] = barcodes --forces commit
						end
					end
				else
					v.status = "Unknown Title"
				end
				
				BarcodePosition[ k ] = v --forces commit
				kernel.log( "> oprhan reservation cleared for: " .. tostring( v.barcode ))
				
			end
		end
		kernel.log("Remove Orphan Reservations COMPLETE")
	end
	
	t.reset = function( self )
		kernel.log( "ReservationManager:Reset()" )
		kernel.resvsvcs_reset()
	end

	t.cache = {}
	t.cache.barcodes = {}

	return t
end
