--
-- shopping-cart-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

BadCardList.is_current_account_bad = function( self )
	return self[kernel.trackgethashid()] ~= nil
end

BadCardList.update_current_account = function( self )
	local hash_id = kernel.trackgethashid()
	local bad_card_info = self[hash_id]
	if bad_card_info == nil then
		bad_card_info = { number_of_attempts = 0 }
	end
	local now = os.date( "*t" )
	bad_card_info.last_attempt = string.format(
		"%4d%02d%02d%02d%02d",
		now.year,
		now.month,
		now.day,
		now.hour,
		now.min )
	bad_card_info.number_of_attempts = bad_card_info.number_of_attempts + 1
	self[hash_id] = bad_card_info
end

BadCardList.remove_current_account_if_bad = function( self )
	if self:is_current_account_bad() then
		self:remove( kernel.trackgethashid() )
	end
end

function newshoppingcart()
	local t = {}

	t.vend_manager = newvendmanager()

	t.messagebox = newmessageboxmanager()

	t.alternate_optin = false
	t.alternate_optin_not_shown = true
	
	t.reservation_card_hash_id = nil
	
	t.has_customer_profile = false
	
	t.session_committed = false

	t.blocklisted = false

    t.prompt_required = false

    t.auth_days = 0

    t.play_pass_prompt = false

    t.play_pass_points_earned = 0

	t.abandon = function( self, reason )
        kernel.abandon_rental_shopping_cart( self.session_id, reason )
	end

	t.commit = function( self, reason, should_end_and_save )
		if ComingSoonOptIn == true then
			kernel.promosvcs_coming_soon_kioskoptin()
		end
		kernel.sessioncommit2( self.session_id, reason )
		self.session_committed = true
		if should_end_and_save then
			self:save_session()
		end
	end

	t.close_analytics = function( self, abandoned )
        kernel.close_shopping_cart_analytics(self.session_id, abandoned)
	end

	t.end_session = function( self )
		if kernel.sessiongetstatus( self.session_id ) == "Active" then
			kernel.sessionabandon2( self.session_id, "Session was not properly ended; marking as abandoned." )
		end
		self:save_session()
	end

	t.save_session = function( self )
		if self.session_id == nil then
			return
		end
		kernel.sessionend2( self.session_id )
	end

	t.check_out = function( self )
		local barcodes = self:get_barcodes_to_vend()
		if #barcodes == 0 then
			self.messagebox:show( strings.inventory_error )
			-- Remove all the items from the cart since none of them have inventory
			kernel.rental_shopping_cart_clear( self.session_id )
			return false
		end

		-- Server does not handle promo codes correctly if there are no rentals in the cart.
		-- This is a kluge which should be fixed server side.
        if self:get_count( nil, "rental" ) == 0 then
			self:clear_promo_code()
		end

		kernel.sessionaddevent2(
			self.session_id,
			"Checkout",
			"User activated check out." )
		return true
	end

	t.start_new = function( self )
		kernel.log("Shopping cart start_new() " )
		self.email = nil
		kernel.rental_promotion_clear_promotion()
		self.postal_code = nil
		self.blocklisted = false
		self.vend_manager:init()
		self.session_id = kernel.sessionstartnew2( StoreManager:get_number() )
		kernel.log(">  session_id = " .. self.session_id )
		kernel.sessionsetcurrent( self.session_id )
        kernel.create_rental_shopping_cart( self.session_id )
	end

	t.set_cart_type = function( self, cart_type )
		kernel.sessionsetcarttype2( self.session_id, cart_type )
	end

	t.add_session_event = function( self, event_type, event_description )
		kernel.sessionaddevent2( self.session_id, event_type, event_description )
	end

	t.set_blocklisted = function( self, value )
		self.blocklisted = value
		kernel.sessionsetblocklisted( self.session_id, value )
	end

	t.set_use_credits = function( self, value )
        kernel.set_rental_shopping_use_credits( self.session_id,  value )
	end

	t.preauth = function( self, on_complete )
        kernel.log("ShoppingCart.Preauth")
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:preauth was called with invalid account data." )
			return
		end
		kernel.log("PreAuth for Session ID: " .. self.session_id )
		if self.postal_code ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, self.postal_code )
		end
		self:get_barcodes_to_vend()
		self:set_session_cart_groups()
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " " .. tostring( v.source_type ) )
			kernel.sessionaddlineitem6(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.source_type,
				v.blu_ray_upsell,
				v.multi_disc )
		end
		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )
		kernel.analyticsaddnode( "authorizations", "preauth", self.session_id )

        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_preauth(
			self.session_id,
            customer_profile_number,
			function( r )
				local preAuthResponse = {
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() )
				}
				kernel.analyticsaddattributes( "preauth", preAuthResponse, self.session_id )

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end

	t.authorize = function( self, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:authorize was called with invalid account data." )
			return
		end
		kernel.log("Authorize for Session ID: " .. self.session_id )
		if self.postal_code ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, self.postal_code )
		end
		self:get_barcodes_to_vend()
		self:set_session_cart_groups()
		--self.blu_ray_upsell:set_shopping_cart_items()
		local clear_in_cart_promo = false
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " " .. tostring( v.source_type ) )
			kernel.sessionaddlineitem6(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.source_type,
				v.blu_ray_upsell,
				v.multi_disc )
			if v.status == "InventoryError" then
				clear_in_cart_promo = true
			end
		end
		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )
		kernel.analyticsaddnode( "authorizations", "authorize", self.session_id )

		local promo_code = kernel.rental_promotion_get_promotion()
		if clear_in_cart_promo == true and promo_code ~= nil and promo_code.name ~= nil and promo_code.name == GetInCartPromoCode() then
			kernel.log( "> Inventory Error: In-Cart promo code " .. tostring( promo_code ) .. " cleared." )
			self:clear_promo_code()
		end
        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_authorize(
			self.session_id,
            customer_profile_number,
			function( r )
				if r.properties.response == "Approved" then
					self:commit( "User checked out successfully." )
				else
					-- Remove Invalid Promo Codes if authorization fails, otherwise they persist even if the user doesn't try it again!
					if r.errors ~= nil and table_count( r.errors ) > 0 then
						local clear_promo = false
						for _, v in ipairs( r.errors ) do
							if v.code == "(D003)" then
								clear_promo = true
								break
							end
						end
						if clear_promo == true then
							kernel.log( "> Invalid Promo code " .. tostring( promo_code ) .. " cleared." )
							self:clear_promo_code()
						end
					end
				end

				local authResponse = {
					approved = tostring( r.properties.response == "Approved"),
					online = tostring( r.properties.is_online),
					timestamp = tostring( newdatetime() ),
					response = r.response
				}
				kernel.analyticsaddattributes( "authorize", authResponse, self.session_id )

				self:analytics_add_auth_account_info(r)

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end
	
	t.analytics_add_auth_account_info = function( self, response)
		kernel.analyticsaddauthtransactionInfo(
			response.properties.customer_profile_number,
			response.properties.account_number,
			response.properties.reference_number,
			self.session_id)
	end

	t.resv_authorize = function( self, reference_number, on_complete )
		kernel.log( "ShoppingCart:resv_authorize BEGIN" )
		
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:resv_authorize was called with invalid account data." )
			return
		end
		kernel.log("Resv. Authorize for Session ID: " .. self.session_id )
		if self.postal_code ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, self.postal_code )
		end
		self:get_barcodes_to_vend()
		self:set_session_cart_groups()
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " source type: " .. tostring( v.source_type )
			)
			kernel.sessionaddlineitem4(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.source_type
			)
		end
		
		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )
		kernel.analyticsaddnode( "authorizations", "resvAuthorize", self.session_id )

        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_resvauthorize(
			self.session_id,
            customer_profile_number,
			reference_number,
			function( r )
				local authResponse = {
					approved = tostring( r.properties.response == "Approved" ),
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() ),
					response = r.response
				}
				kernel.analyticsaddattributes( "resvAuthorize", authResponse, self.session_id )

				self:analytics_add_auth_account_info(r)

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
		
		kernel.log( "ShoppingCart:resv_authorize COMPLETE" )
	end
	
	t.validatepickup = function( self, reference_numbers, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:authorize was called with invalid account data." )
			return
		end

		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )

		kernel.log("Pickup Validation for Session ID: " .. self.session_id )
		--[[Vaibhav - Uncomment below line if there is no change at the server side to exclude
			PostalCode from madatory fields for this message. We will send a dummy postal code in that case.]]
		--kernel.sessionsetpostalcode2( self.session_id, "99999" )
		kernel.analyticsaddnode( "authorizations", "pickupValidation", self.session_id )

        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_validatepickup(
			self.session_id,
            customer_profile_number,
			reference_numbers,
			function( r )
				if r.properties.response ~= nil then
					kernel.log( "Pickup Validation Response : ".. r.properties.response )
				end

				local pickupValidationResponse = {
					pickupValid = tostring( r.properties.response == "Approved" ),
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() ),
					response = r.properties.response
				}
				kernel.log( pickupValidationResponse )
				kernel.analyticsaddattributes( "pickupValidation", pickupValidationResponse, self.session_id )

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end

	t.reconcile = function( self )
		kernel.log("Shopping cart reconcile() " )
		if self.email ~= nil then
			kernel.sessionsetemail2( self.session_id, self.email )
		end
		self:set_session_cart_groups()
        
        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_reconcile( self.session_id, customer_profile_number )
		self:end_session()
	end
	
	t.mod_resv_pickup = function( self, email, reference_number, modified_cart )
		kernel.log( "ShoppingCart:mod_resv_pickup BEGIN" )
		
		if self.email ~= nil then
			kernel.sessionsetemail2( self.session_id, email )
		end
		kernel.resvsvcs_modresvpickup(
			StoreManager:get_number(),
			email,
			reference_number,
			self.session_id,
			modified_cart,
			nil
		)
		self:end_session()
		
		kernel.log( "ShoppingCart:mod_resv_pickup COMPLETE" )
	end
	
	t.get_customer_credits = function( self, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:get_customer_credits was called with invalid account data." )
			return
		end

		if self.postal_code ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, self.postal_code )
		end

		self:set_session_cart_groups()
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) )
			kernel.sessionaddlineitem3(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.multi_disc )
		end

		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )

		kernel.digitalsvcs_getcustomercredits(
			self.session_id,
			function( r )
				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end

	t.set_session_cart_groups = function( self )
        kernel.rental_shopping_cart_set_session_cart_groups( self.session_id )
	end

    t.get_count = function( self, product_family, action )
        return kernel.rental_shopping_cart_item_count( self.session_id, product_family, action)
    end

    t.count_for_item = function( self, id )
		local item_count = kernel.current_rental_shopping_cart_product_count( id )
		local as_a_rental = kernel.current_rental_shopping_cart_has_item_with_action( id, "rental" )
		return item_count, as_a_rental
	end

	t.total = function( self )
		kernel.log( "Total shopping cart" )
        local rental_totals = kernel.rental_shopping_cart_get_total( self.session_id )
		if rental_totals ~= nil then
            kernel.log( ">  st:" .. tostring( rental_totals.sub_total ) .. ", t:" .. tostring( rental_totals.tax_amount ) .. ", dau:" .. tostring( rental_totals.discount_amount_used ) .. ", sf:" .. tostring( rental_totals.service_fee ) .. ", gt:" .. tostring( rental_totals.grand_total ) )
        end
        return rental_totals
	end

	t.sub_total = function( self )
		kernel.log( "SubTotal shopping cart by group" )
        local rental_sub_totals = kernel.rental_shopping_cart_get_sub_total( self.session_id )
		for k, v in pairs( rental_sub_totals ) do
            kernel.log( ">  a:" .. tostring( k ) .. " d:" .. tostring( v.discounted_amount ) .. ", t:" .. tostring( v.tax_amount ) .. ", $:" .. tostring( v.amount ) )
        end
        return rental_sub_totals
	end

	t.has_item = function( self, item )
        return kernel.rental_shopping_cart_has_item( self.session_id, item )
	end

	t.clear_promo_code = function( self )
        kernel.rental_shopping_cart_clear_promo_code( self.session_id )
	end

	t.prepos_vend = function( self )
		local barcodes_to_vend = self:get_barcodes_to_vend()
		if barcodes_to_vend == nil
		or #barcodes_to_vend == 0 then
			show_maintenance_view(
				"shopping-cart-functions.lua",
				"HardwareError",
				"VendFalseInventory",
				"barcodes_to_vend is empty",
				"0064SCBE",
				false )
			-- Remove all the items from the cart since none of them have inventory
			kernel.rental_shopping_cart_clear( self.session_id )
			return
		end

		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
				for _, v in ipairs( vend_results ) do
					kernel.log( "> vend_results b:" .. tostring( v.barcode ) .. ", s:" .. tostring( v.status ) )
                    local item_index = kernel.get_rental_shopping_cart_item_index_by_barcode( self.session_id, v.barcode )
                    if item_index > 0 then
                        kernel.rental_shopping_cart_update_item_status( self.session_id, item_index, v.status )
                    end
				end
				local all_vended = true
				local at_least_one_game = false
                local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
                for _,v in ipairs( shopping_cart_items ) do
					kernel.log( "> local cart item : " .. tostring( v.barcode ) )
					if v.status == "Pending"
					or v.status == "ProductSelected" then
                        -- update status in the lua object and on the .net side 
                        kernel.log( "item not fullfilled" )
						v.status = "NotFullfilled"
                        kernel.rental_shopping_cart_update_item_status( self.session_id, _, "NotFullfilled" )
					end

					-- this is where I need to add any barcodes which did not get added earlier due to prior pre-picker inventory issues!!
					kernel.log("Session update Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
						" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) )
					kernel.sessionaddlineitem3(
						self.session_id,
						v.action,
						v.barcode,
						v.status,
						v.price,
						v.quantity,
						v.pricing_policy,
						v.product_family,
						v.product_type,
						v.multi_disc )

					if v.status ~= "Fullfilled" then
						all_vended = false
					end
					if v.status == "Fullfilled" and v.product_family == "Games" then
						at_least_one_game = true
					end
				end

				-- Clear in-cart promo if all items did not vend successfully
				local promo_code = kernel.rental_promotion_get_promotion()
				if all_vended == false
				and promo_code ~= nil
				and promo_code.name ~= nil
				and promo_code.name == GetInCartPromoCode() then
					kernel.log( "Not all items vended, clearing InCart promotion" )
					self:clear_promo_code()
				end

				-- Clear Game required promo if no game was vended
				if at_least_one_game == false
				and promo_code ~= nil
				and promo_code.intent == "FirstNightGameGameRequired" then
					kernel.log( "No game was vended, clearing GamesRequired promotion" )
					self:clear_promo_code()
				end

				if self.session_committed == true then
					self:reconcile()
				end
				self:close_analytics( "False" )
				kernel.sessionend2( self.session_id )
			end )
			
		if self.vend_manager.vending == false then
			show_maintenance_view(
				"shopping-cart-functions.lua",
				"HardwareError",
				"HALJobNotCreated",
				"failed to create prepos_vend job in HAL",
				"0064SCBE",
				true )
			-- Remove all the items from the cart since none of them have inventory
			kernel.rental_shopping_cart_clear( self.session_id )
			return
		end
	end

	t.prepos_vend_continue = function( self )
		self.vend_manager:prepos_vend_continue()
	end

	t.prepos_vend_abort = function( self )
		self.vend_manager:prepos_vend_abort()
	end
	
	t.vend_reservation_cart = function( self, email, reference_number, modified_cart )
		kernel.log( "ShoppingCart:vend_reservation_cart BEGIN" )
		
		kernel.analyticsaddnode( "session", "onlinePickup", self.session_id )
		
		local barcodes_to_vend = self:get_barcodes_to_vend()
		if barcodes_to_vend == nil
		or #barcodes_to_vend == 0 then
			show_maintenance_view(
				"shopping-cart-functions.lua",
				"HardwareError",
				"VendFalseInventory",
				"barcodes_to_vend is empty",
				"0064SCBE",
				false )
			-- Remove all the items from the cart since none of them have inventory
			kernel.rental_shopping_cart_clear( self.session_id )
			return
		end
		
		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
				--OnComplete
				for _, v in ipairs( vend_results ) do
					kernel.log( "> vend_results b:" .. tostring( v.barcode ) .. ", s:" .. tostring( v.status ) )
                    local item_index = kernel.get_rental_shopping_cart_item_index_by_barcode( self.session_id, v.barcode )
                    if item_index > 0 then
                        kernel.rental_shopping_cart_update_item_status( self.session_id, item_index, v.status )
                    end
				end
				self:set_session_cart_groups()

				local barcodes_to_reset = {}
				
                local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
                for _,v in ipairs( shopping_cart_items ) do
					kernel.log( "> local cart item: " .. tostring( v.barcode ) )
					if v.status == "Pending"
					or v.status == "ProductSelected" then
                        -- update status in the lua object and on the .net side 
                        kernel.log( "item not fullfilled" )
						v.status = "NotFullfilled"
                        kernel.rental_shopping_cart_update_item_status( self.session_id, _, "NotFullfilled" )
					end
					
					if v.status ~= "Fullfilled" then
						kernel.log("> Adding " .. tostring( v.barcode ) .. " to reset barcode list" )
						barcodes_to_reset[#barcodes_to_reset + 1] = v.barcode
					end
					
					-- this is where I need to add any barcodes which did not get added earlier due to prior pre-picker inventory issues!!
					kernel.log( "Session update Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
						" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " source type: " .. tostring( v.source_type ) )
					kernel.sessionaddlineitem4(
						self.session_id,
						v.action,
						v.barcode,
						v.status,
						v.price,
						v.quantity,
						v.pricing_policy,
						v.product_family,
						v.product_type,
						v.source_type
					)
					
					local attributes = {
						barcode = tostring( v.barcode ),
						status = tostring( v.status )
					}
					kernel.analyticsaddnode( "onlinePickup", "cartItem", self.session_id )
					kernel.analyticsaddattributes( "cartItem", attributes, self.session_id )
				end
				
				if self.session_committed == true then
					self:mod_resv_pickup( email, reference_number, modified_cart )
				end
				
				kernel.log( "ShoppingCart:vend_reservation_cart: Removing reservations for ..." ..  string.sub( self.reservation_card_hash_id or "NULL ", 1, 5 ) )
				Reservations:remove( self.reservation_card_hash_id )
				
				if #barcodes_to_reset <= 0 then
					kernel.log( "ShoppingCart:vend_reservation_cart: No barcodes to reset" )
				else
					kernel.log( "ShoppingCart:vend_reservation_cart: resetting status to Known for " .. tostring(#barcodes_to_reset) .. " discs" )
					update_barcode_status( barcodes_to_reset, "Known" )
				end
				
				self:close_analytics( "False" )
				kernel.sessionend2( self.session_id )
			end
		)
		self.vend_manager:prepos_vend_continue()
		kernel.log( "ShoppingCart:vend_reservation_cart COMPLETE" )
	end
	
	t.get_barcodes_to_vend = function( self )
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
		local barcodes_for_products = get_barcodes_for_products( shopping_cart_items )
        -- assign barcode and status to the rental shopping cart items
        for k,v in ipairs( shopping_cart_items ) do
            if v ~= nil then
                kernel.rental_shopping_cart_update_item_barcode( self.session_id, k, v.barcode )
                kernel.rental_shopping_cart_update_item_status( self.session_id, k, v.status )
            end
        end
        return barcodes_for_products
	end

	local function trim_spaces(s)
	  local i1,i2 = string.find(s,'^%s*')
	  if i2 >= i1 then s = string.sub(s,i2+1) end
	  local i1,i2 = string.find(s,'%s*$')
	  if i2 >= i1 then s = string.sub(s,1,i1-1) end
	  return s
	end

	t.generate_totals = function( self, credits_amount )
		local subtotal_by_group = self:sub_total()
		local totals = {}
		totals["sub_total"] = 0
		totals["tax_amount"] = 0
		totals["discount_used"] = 0
		for k,v in pairs( subtotal_by_group ) do
			if v.discounted_amount ~= nil then
				totals["sub_total"] = totals["sub_total"] + v.discounted_amount
				totals["discount_used"] = totals["discount_used"] + v.amount - v.discounted_amount
			else
				totals["sub_total"] = totals["sub_total"] + v.amount
			end
			totals["tax_amount"] = totals["tax_amount"] + v.tax_amount
		end

		totals["total"] = totals["sub_total"] + totals["tax_amount"]

		-- account for credits separately
		if credits_amount ~= nil and credits_amount > 0 then
				local credited_tax_amount = kernel.round( credits_amount * ( StoreManager.store.rental_tax_rate / 100 ), 16 )
				totals["discount_used"] = totals["discount_used"] + credits_amount
				totals["tax_amount"] = totals["tax_amount"] - credited_tax_amount
				totals["sub_total"] = totals["sub_total"] - credits_amount
				if StoreManager.store ~= nil and StoreManager.store.state ~= nil and StoreManager.store.state ~= "MD" and StoreManager.store.state ~= "SC" then
					totals["total"] = totals["total"] - credits_amount - credited_tax_amount
				else
					totals["total"] = totals["total"] - credits_amount
				end
		end
		
		return totals
	end

	t.get_coming_soon_vend_screen_titles_automation = function( self )
		local coming_soon_title_generator = newcomingsoongenerator()
        local items_in_cart = kernel.get_rental_shopping_cart_items( self.session_id )
		local coming_soon_products = coming_soon_title_generator:get_products( items_in_cart, 3 )
		local coming_soon_product_ids = {}
		for k, v in ipairs( coming_soon_products ) do
			table.insert(coming_soon_product_ids, v.product_id )
		end
		return coming_soon_product_ids
	end
	
	t.is_gift_card = function( self )
		return kernel.trackgetcardtype() == "RedboxGiftCard"
	end
	
	t.gc_authorize = function( self, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:gc_authorize was called with invalid account data." )
			return
		end
		kernel.log( "GC Authorize for Session ID: " .. self.session_id )
		--if self.postal_code ~= nil then
			--kernel.sessionsetpostalcode2( self.session_id, self.postal_code )
		--end
		self:get_barcodes_to_vend()
		self:set_session_cart_groups()
		--self.blu_ray_upsell:set_shopping_cart_items()
		local clear_in_cart_promo = false
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " " .. tostring( v.source_type ) )
			kernel.sessionaddlineitem5(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.source_type,
				v.blu_ray_upsell )
			if v.status == "InventoryError" then
				clear_in_cart_promo = true
			end
		end
		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )
		kernel.analyticsaddnode( "authorizations", "gc_authorize", self.session_id )
		
		local promo_code = kernel.rental_promotion_get_promotion()
		if clear_in_cart_promo == true and promo_code ~= nil and promo_code.name ~= nil and promo_code.name == GetInCartPromoCode() then
			kernel.log( "> Inventory Error: In-Cart promo code " .. tostring( promo_code ) .. " cleared." )
			self:clear_promo_code()
		end

        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_gcauthorize(
			self.session_id,
            customer_profile_number,
			function( r )
				if r.properties.response == "Approved" then
					self:commit( "User checked out successfully." )
				else
					-- Remove Invalid Promo Codes if authorization fails, otherwise they persist even if the user doesn't try it again!
					if r.errors ~= nil and table_count( r.errors ) > 0 then
						local clear_promo = false
						for _, v in ipairs( r.errors ) do
							if v.code == "(D003)" then
								clear_promo = true
								break
							end
						end
						if clear_promo == true then
							kernel.log( "> Invalid Promo code " .. tostring( promo_code ) .. " cleared." )
							self:clear_promo_code()
						end
					end
				end
				
				local authResponse = {
					approved = tostring( r.properties.response == "Approved" ),
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() ),
					response = r.response
				}
				kernel.analyticsaddattributes( "gc_authorize", authResponse, self.session_id )
				
				self:analytics_add_auth_account_info(r)

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end
	
	t.is_card_blocklisted = function( self )
		local blocklisted = false
		if OfflineBlockListEnabled == true 
		and kernel.healthsvcs_isonline() == false
		and self:is_gift_card() == false then
			local first_name = kernel.trackgetfirstname() or ""
			local last_name = kernel.trackgetlastname() or ""
			local zip_code = StoreManager:get_store_zip() or ""
			local market_id = StoreManager:get_store_market_id()
			blocklisted = kernel.rental_is_name_in_blocklist( first_name, last_name, market_id, zip_code )
		end
		return blocklisted
	end

	t.set_items_to_not_vended = function( self )
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k, v in ipairs( shopping_cart_items ) do
            -- update status in the lua object and on the .net side 
			v.status = "NotVended"
            kernel.rental_shopping_cart_update_item_status( self.session_id, k, "NotVended" )

			kernel.sessionaddlineitem3(
				self.session_id,
				v.action,
				v.barcode,
				v.status,
				v.price,
				v.quantity,
				v.pricing_policy,
				v.product_family,
				v.product_type,
				v.multi_disc )
		end
	end
	
	t.add_multi_disc_discs = function( self )
        kernel.rental_shopping_cart_add_multi_disc_discs( self.session_id )
	end
	
	t.contains_multi_disc = function( self )
        kernel.rental_shopping_cart_has_mult_disc_item( self.session_id )
	end

	t.service_fee_enabled = function( self )
		return kernel.rental_shopping_cart_service_fee_enabled( self.session_id )
	end

	t.default_service_fee_cents = function( self )
		return kernel.rental_shopping_cart_default_service_fee_cents( self.session_id )
	end

	return t
end
