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
	t.recommendations_generator = newrecommendationsgenerator()
	t.messagebox = newmessageboxmanager()

	t.in_cart_promo_offer_made = false
	t.in_cart_promo_offer_cancel = false
	t.in_cart_title_marketing = false
    t.in_cart_do_not_show = false

	t.marketing_opt_in = "DoNotShow"
	t.alternate_optin = false
	t.alternate_optin_not_shown = true
	
	t.pickup_recommendations_shown = false
	t.reservation_tax_rate = nil
	t.reservation_card_hash_id = nil
	
	t.has_customer_profile = false
	
	t.session_committed = false

	t.session_use_credits = "NotSpecified"

	t.blocklisted = false

    t.prompt_required = false

    t.auth_days = 0

    t.play_pass_prompt = false

    t.play_pass_points_earned = 0

	t.size = function( self )
		return #self.items
	end

	t.set_reservation_tax_rate = function ( self, reservation_tax )
		self.reservation_tax_rate = reservation_tax
	end
	
	t.abandon = function( self, reason )
		if ComingSoonOptIn == true then
			self.coming_soon_optin:send_kiosk_optin_message()
		end
		kernel.sessionabandon2( self.session_id, reason )
		kernel.sessionend2( self.session_id )
		kernel.sessionsave2( self.session_id )
	end

	t.commit = function( self, reason, should_end_and_save )
		if ComingSoonOptIn == true then
			self.coming_soon_optin:send_kiosk_optin_message()
		end
		kernel.sessioncommit2( self.session_id, reason )
		self.session_committed = true
		if should_end_and_save then
			self:save_session()
		end
	end

	t.close_analytics = function( self, abandoned )
		self:send_cart_to_analytics()
		SubmitAnalyticsPricing( self.session_id )
		kernel.analyticsclose( self.session_id, abandoned )
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
		kernel.sessionsave2( self.session_id )
	end

	t.update_cart_macros = function( self )
		for k, v in ipairs( self.items ) do
            local product = MasterProductCatalog[ v.product_id ]
            kernel.log("TTS:CartTitle_"..tostring(k).."_Name = "..tostring(product.long_name) )
			kernel.setmacro( "TTS:CartTitle_"..tostring(k).."_Name", tostring(product.long_name) )
            kernel.log("TTS:CartTitle_"..tostring(k).."_Action = "..tostring(v.action) )
			kernel.setmacro( "TTS:CartTitle_"..tostring(k).."_Action", tostring(v.action) )
		end
		for i=(#self.items+1),5 do
			kernel.log("TTS:CartTitle_"..tostring(i).."_Name = empty")
			kernel.setmacro( "TTS:CartTitle_"..tostring(i).."_Name", "" )
			kernel.log("TTS:CartTitle_"..tostring(i).."_Action = empty")
			kernel.setmacro( "TTS:CartTitle_"..tostring(i).."_Action", "" )
		end

		if #self.items > 0 then
			kernel.setmacro( "TTS:NumberOfTitlesInCart", #self.items )
		else
			kernel.setmacro( "TTS:NumberOfTitlesInCart", "" )
		end
		
		if #self.items == 1 then
			kernel.setmacro( "TTS:CartTitlePlurality", "Title" )
		elseif #self.items > 1 then
			kernel.setmacro( "TTS:CartTitlePlurality", "Titles" )
		end
	end
	
	t.check_out = function( self )
		local barcodes = self:get_barcodes_to_vend()
		if #barcodes == 0 then
			self.messagebox:show( strings.inventory_error )
			-- Remove all the items from the cart since none of them have inventory
			self.items = {}
			if kernel.ttsenabled() == true then
				self.update_cart_macros()
			end
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
		self.items = {}
		if kernel.ttsenabled() == true then
			self:update_cart_macros()
		end
		self.session_use_credits = "NotSpecified"
		self.email = nil
		kernel.rental_promotion_clear_promotion()
		self.postal_code = nil
		self.blocklisted = false
		self.vend_manager:init()
		self.session_id = kernel.sessionstartnew2( StoreManager:get_number() )
		kernel.log(">  session_id = " .. self.session_id )
		kernel.sessionsetcurrent( self.session_id )
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
		self.session_use_credits = value
		kernel.sessionsetusecredits( self.session_id, value )
	end

	t.is_full = function( self )
		local rent_count = 0
		for k, v in ipairs( self.items ) do
			if v.action == "rental" then
				rent_count = rent_count + 1
			end
		end
		return (StoreManager:is_sell_thru_enabled() == false and rent_count == 5)
			or ((StoreManager:has_sell_thru_new_in_stock() == false or StoreManager:get_purchase_new_price() == nil) and rent_count == 5)
			or #self.items == 7
	end

	t.preauth = function( self, on_complete )
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
		self.blu_ray_upsell:set_shopping_cart_items()
		for k, v in ipairs( self.items ) do
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

		kernel.merchsvcs_preauth(
			self.session_id,
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
		self.blu_ray_upsell:set_shopping_cart_items()
		local clear_in_cart_promo = false
		for k, v in ipairs( self.items ) do
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
		kernel.merchsvcs_authorize(
			self.session_id,
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

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
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
		for k, v in ipairs( self.items ) do
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

		kernel.merchsvcs_resvauthorize(
			self.session_id,
			reference_number,
			function( r )
				local authResponse = {
					approved = tostring( r.properties.response == "Approved" ),
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() ),
					response = r.response
				}
				kernel.analyticsaddattributes( "resvAuthorize", authResponse, self.session_id )

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

		kernel.merchsvcs_validatepickup(
			self.session_id,
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
		kernel.merchsvcs_reconcile( self.session_id )
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
		for k, v in ipairs( self.items ) do
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
		local subtotal_by_group = self:sub_total()
		for k, v in pairs( subtotal_by_group ) do
			local grand_total = 0
			if v.discounted_amount ~= nil then
				grand_total = v.discounted_amount + v.tax_amount
			else
				grand_total = v.amount + v.tax_amount
			end
			kernel.sessionaddlineitemgroup2(
				self.session_id,
				k,
				v.tax_rate,
				v.amount,
				v.discounted_amount or v.amount,
				v.tax_amount,
				grand_total )
		end
	end

	t.send_cart_to_analytics = function( self )
		kernel.log( "Gather Up Cart Data for Analytics" )
		-- Cart Totals
		local subtotal_by_group = self:sub_total()
		local sub_total = 0
		local tax_amount = 0
		if ( self.pickup_recommendations_shown == false ) then
			for k, v in pairs( subtotal_by_group ) do
				if v.discounted_amount ~= nil then
					sub_total = sub_total + v.discounted_amount
				else
					sub_total = sub_total + v.amount
				end
				tax_amount = tax_amount + v.tax_amount
			end
		end
		local grand_total = sub_total + tax_amount
		kernel.analyticsaddelement( "shoppingCart", "subTotal", tostring(sub_total), self.session_id)
		kernel.analyticsaddelement( "shoppingCart", "taxAmount", tostring(tax_amount), self.session_id)
		kernel.analyticsaddelement( "shoppingCart", "grandTotal", tostring(grand_total), self.session_id)
		-- Cart Items
		for k, v in ipairs( self.items ) do
			local attributes = {
				productID = tostring( v.product_id ),
				action = v.action,
				price = tostring( v.price ),
				quantity = tostring( v.quantity or 1 ),
				taxRate = tostring( v.tax_rate ),
				removed = "False",
				status = tostring( v.status ),
				productFamily = v.product_family,
				productType = v.product_type,
				barcode = v.barcode or "",
				sourceType = v.source_type or "Browse"
			}
			if self.in_cart_promo_offer_cancel == false and self.in_cart_promo_offer_made == true then
				attributes["add_sequence"] = v.add_sequence or ""
			end
			
			kernel.analyticsaddnodewithattributes( "cartItems", "cartItem", attributes, self.session_id )
		end
	end

    t.get_count = function( self, product_family, action )
		local count = 0
        for k, v in ipairs( self.items ) do
            if ( product_family == nil or v.product_family == product_family ) and ( action == nil or v.action == action ) and ( MasterProductCatalog[v.product_id].disc_number == nil or MasterProductCatalog[v.product_id].disc_number == 1 ) then
				count = count + 1
			end
		end
		return count
    end

    t.count_for_item = function( self, id )
		local item_count = 0
		local as_a_rental = false
		for k, v in ipairs( self.items ) do
			if v.product_id == id then
				item_count = item_count + 1
				if v.action == "rental" then
					as_a_rental = true
				end
			end
		end
		return item_count, as_a_rental
	end

	t.add_item = function( self, catalog_entry, qty, action_code, source_type )
		local product_pricing = StoreManager.PricingManager:get_product_price( catalog_entry.product_id )
		local item_price = product_pricing.initial_night
		
		if is_multi_disc( catalog_entry.product_id ) == true and MasterProductCatalog[catalog_entry.product_id] ~= nil and MasterProductCatalog[catalog_entry.product_id].disc_number ~= nil and MasterProductCatalog[catalog_entry.product_id].disc_number > 1 then
			product_pricing.initial_night = 0
			product_pricing.extra_night = 0
			product_pricing.non_return = 0
			product_pricing.expiration_price = 0
			
			item_price = 0
		end
		
		local item_tax_rate = ( StoreManager:get_rental_tax_rate() / 100 )
		if self.reservation_tax_rate ~= nil then
			item_tax_rate = self.reservation_tax_rate
		end
		
		local product_type = ProductTypes[ catalog_entry.product_type_id ]
		local product_family = ProductFamilies[ product_type.product_family_id ]
		local product = MasterProductCatalog[ catalog_entry.product_id ]

		if action_code == "purchase" then
			local expire_price = 0
			if catalog_entry.product_id == 1139 then
				item_price = StoreManager.PricingManager:get_empty_case_price()
				expire_price = item_price
			else
				if product_family.product_family_name == "Games" then
					item_price = catalog_entry.purchase_price
				else
					if catalog_entry.sell_thru_new == true then
						item_price = StoreManager:get_purchase_new_price()
					else
    					item_price = catalog_entry.purchase_price
					end
				end
				expire_price = product_pricing.expiration_price
			end
			item_tax_rate = ( StoreManager:get_purchase_tax_rate() / 100 )
			product_pricing = {
				purchase = item_price,
				expiration_price = expire_price
			}
		end

		local session_message = string.format( "User added item '%s' (%s)",
			catalog_entry.long_name,
			catalog_entry.product_id )
		kernel.log( "ShoppingCart:add_item: " .. tostring( session_message ) )

		local added_sequence = "pre_offer"
		if self.in_cart_promo_offer_made == true then
			added_sequence = "post_offer"
		end
		
		-- Emergency fix to Empty or Stuck inventory problems with Reco on pickup logic
		if source_type ~= "Reservation" then
			catalog_entry.barcode = nil
		end
		
		table.insert(
			self.items,
			{	product_id = catalog_entry.product_id,
				product_type_id = catalog_entry.product_type_id,
				rating_id = catalog_entry.rating_id,
				merchandise_date = catalog_entry.merchandise_date,
				genres = catalog_entry.genres,
				quantity = qty or 1,
				product_family = product_family.product_family_name,
				product_type = product_type.product_type_name,
				add_sequence = added_sequence,
				price = item_price,
				pricing_policy = product_pricing,
				action = action_code,
				tax_rate = item_tax_rate,
				status = "Pending",
				barcode = catalog_entry.barcode,
				source_type = source_type or "Browse" } )
		if ShoppingCart.in_cart_promo_offer_cancel == false then
			UpdateInCartPromoValue( ShoppingCart )
		end
		
		self:validate_cart_update_promo_code( nil, nil )
		
		if kernel.ttsenabled() == true then
			self:update_cart_macros()
			kernel.ttsrepeat()
		end
	end

	t.remove_item = function( self, index, user_removed )
		local removed_item = self.items[index]
		if removed_item == nil then return end

		local remove_status = "True"
		
		if user_removed ~= nil and user_removed == false then
			remove_status = "False"
		end

		local attributes = {
			productID = tostring( removed_item.product_id ),
			action = removed_item.action,
			price = tostring( removed_item.price ),
			quantity = tostring( removed_item.quantity or 1 ),
			taxRate = tostring( removed_item.tax_rate ),
			removed = remove_status,
			status = tostring( removed_item.status ),
			productFamily = removed_item.product_family,
			productType = removed_item.product_type,
			sourceType = tostring( removed_item.source_type )
		}
		kernel.analyticsaddnode( "cartItems", "cartItem", self.session_id )
		kernel.analyticsaddattributes( "cartItem", attributes, self.session_id )

		--remove_movie_from_selected_product_map("selectedmovie_"..index)
		if removed_item.barcode ~= nil then
			kernel.sessionremovelineitem2( self.session_id, removed_item.barcode )
		end
		local session_message = string.format( "User removed item %s (%s)",
			MasterProductCatalog[removed_item.product_id].long_name,
			removed_item.product_id )
		kernel.log( "ShoppingCart:remove_item: " .. tostring( session_message ) )
		table.remove( self.items, index )
		if kernel.ttsenabled() == true then
			self:update_cart_macros()
			kernel.ttsrepeat();
		end

        kernel.set_session_value("BrowseState.ShoppingCartFullMessagesShown", false)

		-- Clear Game Required promo code if user removed last game from cart
		local promo_code = kernel.rental_promotion_get_promotion()
		if promo_code ~= nil
		and promo_code.intent == "FirstNightGameGameRequired"
		and CartHasProductTypeFamily( self, nil, "Games" ) == false
		then
			self:clear_promo_code()
		end

		if free_in_cart_template_is_running() then
            if free_in_cart_template_is_cart_valid() == false and ShoppingCart.in_cart_promo_offer_made == true then
                self:clear_promo_code()
                ShoppingCart.in_cart_promo_offer_made = false
            end
        elseif rtwo_in_cart_template_is_running() then
            if rtwo_in_cart_template_get_valid_item_count() < 2 and ShoppingCart.in_cart_promo_offer_made == true then
                self:clear_promo_code()
                ShoppingCart.in_cart_promo_offer_made = false
                ShoppingCart.in_cart_promo_offer_cancel = false
            end
        elseif ShoppingCart.in_cart_promo_offer_cancel == false then
			UpdateInCartPromoValue( ShoppingCart )
		end

		self:validate_cart_update_promo_code( nil, nil )
	end

	t.sub_total = function( self )
		kernel.log( "SubTotal shopping cart by group" )
		local subtotal_by_group = {}
		for k, v in ipairs( self.items ) do
			kernel.log( ">  p:" .. tostring(v.product_id) .. ", a:" .. tostring(v.action) .. " $:" .. tostring(v.price) .. ", s:" .. tostring(v.status) )
			if v.action ~= nil then
				if subtotal_by_group[v.action] == nil then
					subtotal_by_group[v.action] = {}
					subtotal_by_group[v.action].amount = 0
					subtotal_by_group[v.action].tax_rate = v.tax_rate
				end
				if v.status == "Pending"
				or v.status == "ProductSelected"
				or v.status == "Fullfilled" then
					local quantity = v.quantity or 1
					local extension = v.price * quantity
					subtotal_by_group[v.action].amount = subtotal_by_group[v.action].amount + extension
				end
			end
		end

		-- Calculate and apply promotion value as needed
		local promo_code = kernel.rental_promotion_get_promotion()
		if  promo_code ~= nil
		and subtotal_by_group.rental ~= nil then
			kernel.log( ">  promo:" .. tostring( promo_code.name ) .. ", intent:" .. tostring( promo_code.intent ) )
			if promo_code.intent ~= nil and promo_code.intent == "PercentOffCartTotal" then
				local promo_value = round( promo_code.percent * subtotal_by_group.rental.amount, 2 )
				subtotal_by_group.rental.discounted_amount = subtotal_by_group.rental.amount - promo_value
				-- Percentage off must be constantly updated whenever the cart changes
				promo_code.price = promo_value
				kernel.rental_promotion_set_promotion( promo_code )
				kernel.sessionsetdiscount2(
					self.session_id,
					promo_code.name,
					promo_value
				)
			else
				subtotal_by_group.rental.discounted_amount = subtotal_by_group.rental.amount - ( promo_code.quantity * promo_code.price )
			end
		end

		for k, v in pairs( subtotal_by_group ) do
			if v.discounted_amount ~= nil then
				v.discounted_amount = math.max( kernel.round( v.discounted_amount, 16 ), 0 )
				v.tax_amount = kernel.round( v.discounted_amount * v.tax_rate, 16 )
			else
				v.amount = math.max( v.amount, 0 )
				v.tax_amount = kernel.round( v.amount * v.tax_rate, 16 )
			end
			kernel.log( ">  a:" .. tostring( k ) .. " d:" .. tostring( v.discounted_amount ) .. ", t:" .. tostring( v.tax_amount ) .. ", $:" .. tostring( v.amount ) )
		end
		return subtotal_by_group
	end

	t.get_item_by_barcode = function( self, barcode )
		for k, v in ipairs( self.items ) do
			if v.barcode == barcode then
				return v
			end
		end
		return nil
	end

	t.has_item = function( self, item )
		for k, v in ipairs( self.items ) do
			if v.product_id == item then
				return true
			end
		end
		return false
	end

	t.get_promo_code = function( self, on_complete, on_error )
		local keyboard_manager = newkeyboardmanager()
		keyboard_manager:init( 31 )
		keyboard_manager:set_message( strings.promo_code_prompt_message )
		kernel.setmacro( "TTSKeyBoard:PromoEntry", "true" )
		kernel.disableactor("ok_button")
		keyboard_manager:set_validate_event(
			function( text )
				if string.len( text ) < 3 then
					return strings.promo_code_error_message
				end
				return nil
			end )
		keyboard_manager:set_enter_event(
			function()
				local message_view_manager = newmessageviewmanager()
				local error_message_view_manager = newerrormessageviewmanager()
				local code = keyboard_manager:get_value()
				kernel.setmacro( "TTSKeyBoard:PromoEntry", nil )
				keyboard_manager:hide( true )
				error_message_view_manager:show()
				error_message_view_manager:set_message( strings.promo_code_validating_message )

				local online_flag = "True"
				kernel.analyticsaddnode( "promoCodes", "promoCode", self.session_id )
				kernel.analyticsaddattribute( "promoCode", "code", code, self.session_id )

				-- do not allow users to manually enter the in-cart promotional code
				if code == GetInCartPromoCode() then
					kernel.log( "User manualy entered the InCartPromoCode - clearing!" )
					code = ""
				end

				kernel.promosvcs_validatecode(
					StoreManager:get_number(),
					code,
					function( r )
						if r.success then
							kernel.analyticsaddattribute( "promoCode", "valid", "True", self.session_id )
							
							local promo_price = nil
							local promo_percent = 0
							
							if r.properties.promotion_intent == "FirstNightDVD" then
								promo_price = StoreManager.PricingManager:get_price_setting( "movies", "dvd", "initial_night", "rental" )
							end
							if r.properties.promotion_intent == "FirstNightBluray" then
								promo_price = StoreManager.PricingManager:get_price_setting( "movies", "blu-ray", "initial_night", "rental" )
							end
							if r.properties.promotion_intent == "FirstNightGame"
							or r.properties.promotion_intent == "FirstNightGameGameRequired" then
								promo_price = StoreManager.PricingManager:get_price_setting( "games", "*", "initial_night", "rental" )
							end
							if r.properties.promotion_intent == "PercentOffCartTotal" then
								local subtotal_by_group = self:sub_total()
								promo_percent = r.properties.amount
								if subtotal_by_group.rental ~= nil and subtotal_by_group.rental.amount > 0 then
									promo_price = round( promo_percent * subtotal_by_group.rental.amount, 2 )
								else
									promo_price = 0
								end
							end

							if promo_price == nil then
								-- FixedValue is the default
								promo_price = r.properties.amount
							end

							if r.properties.promotion_intent == "FirstNightGameGameRequired"
							and CartHasProductTypeFamily( self, nil, "Games" ) == false
							then
								-- The promo is not valid if the required product family/type is not in the cart
								r.success = false
								r.properties.response = "GameRequired"
							else
                                -- This promo is okay to proceed
								local promo_code = {
									name = code,
									original_price = promo_price,
                                    price = promo_price,
									quantity = 1,
									intent = r.properties.promotion_intent,
                                    original_percent = promo_percent,
									percent = promo_percent,
									rent_qty = r.properties.rent_qty,
									rent_format = r.properties.rent_format,
									get_qty = r.properties.get_qty,
									get_format = r.properties.get_format,
                                    campaign_titles = r.properties.campaign_titles
								}
								kernel.rental_promotion_set_promotion( promo_code )
								kernel.sessionsetdiscount2(
									self.session_id,
									promo_code.name,
									promo_code.price
								)

								local done = function()
									message_view_manager:hide()
									if on_error ~= nil then
										on_error()
									end
								end
								if promo_code.intent == "RxGx" then
									self:validate_cart_update_promo_code(
										function( cart_promo_info )
											if on_complete ~= nil then
												on_complete()
											end
										end,
										function( cart_promo_info )
											message_view_manager:show()
											message_view_manager:set_message_header( "Oops" )
											if kernel.getculture() == "es-MX" then
												message_view_manager:set_message( cart_promo_info.title_spanish .. "\n\n" .. cart_promo_info.message_spanish )
											else
												message_view_manager:set_message( cart_promo_info.title_english .. "\n\n" .. cart_promo_info.message_english )
											end
											message_view_manager:hide_first_button()
											message_view_manager:show_second_button( strings.continue_button, done )
											message_view_manager:start_timer( done )
											view_analytics_event( "context", "rxgxWarning" )
										end
									)
								elseif promo_code.campaign_titles ~= nil then
                                    self:validate_cart_update_promo_code(
										function()
											if on_complete ~= nil then
												on_complete()
											end
										end,
										function()
											message_view_manager:show()
											message_view_manager:set_message_header( strings.tsp_invalid_header )
											message_view_manager:set_message( strings.tsp_invalid_message )
											message_view_manager:hide_first_button()
											message_view_manager:show_second_button( strings.continue_button, done )
											message_view_manager:start_timer( done )
											view_analytics_event( "context", "titlelevelpromoWarning" )
										end
									)
                                else
									kernel.analyticsaddattribute( "promoCode", "value", tostring( promo_code.price ), self.session_id )
									if on_complete ~= nil then
										on_complete()
									end
								end
							end
						end

						if r.success == false then
							message_view_manager:show()
							--message_view_manager:hide_title()
							message_view_manager:hide_first_button()
							kernel.analyticsaddattribute( "promoCode", "valid", "False", self.session_id )
							kernel.analyticsaddattribute( "promoCode", "value", "0.00", self.session_id )
							view_analytics_event( "context", "promoCodeDecline" )

							if r.properties.response == "CodeAlreadyUsed" then
								view_analytics_event( "context", "promoAlreadyUsed" )
								message_view_manager:set_message_header( strings.promo_code_already_used_message_title )
								message_view_manager:set_message( strings.promo_code_already_used_message )
							elseif r.properties.response == "CodeNotValidAtThisLocation" then
								view_analytics_event( "context", "promoInvalidLocation" )
								message_view_manager:set_title( strings.promo_code_error_title )
								message_view_manager:set_message( strings.promo_code_not_valid_location_message )
							elseif r.properties.response == "Error"
								or r.properties.response == "Server Busy"
								or r.properties.response == "Communications Failure" then
								view_analytics_event( "context", "promoCantBeUsed" )
								online_flag = "False"
								message_view_manager:set_message_header( strings.promo_code_validation_failure_message_title )
								message_view_manager:set_message( strings.promo_code_validation_failure_message )
							elseif r.properties.response == "GameRequired" then
								view_analytics_event( "context", "promoCantBeUsed" )
								message_view_manager:set_title( strings.promo_code_game_required_title )
								message_view_manager:set_message( strings.promo_code_game_required_message )
							else
								view_analytics_event( "context", "promoNotValid" )
								message_view_manager:set_message_header( strings.promo_code_not_valid_message_title )
								message_view_manager:set_message( strings.promo_code_not_valid_message )
							end
							kernel.analyticsaddattribute( "promoCode", "serverFailureReason", tostring( r.properties.response ), self.session_id )
							message_view_manager:show_second_button( strings.continue_button,
								function()
									if on_error ~= nil then
										on_error()
									else
										message_view_manager:hide()
									end
								end
							)
							message_view_manager:start_timer(
								function()
									if on_error ~= nil then
										on_error()
									else
										message_view_manager:hide()
									end
								end )
						end
						kernel.analyticsaddattribute( "promoCode", "online", online_flag, self.session_id )
					end )

			end )
		keyboard_manager:show( strings.promo_code_tag )
		view_analytics_event( "context", "promoCode" )

		idle_timer:start()
	end

	t.validate_cart_update_promo_code = function( self, on_valid, on_invalid )
		local promo_code = kernel.rental_promotion_get_promotion()
		if promo_code ~= nil and promo_code.intent == "RxGx" then
			local dvd_price = StoreManager.PricingManager:get_price_setting( "movies", "dvd", "initial_night", "rental" )
			local blu_ray_price = StoreManager.PricingManager:get_price_setting( "movies", "blu-ray", "initial_night", "rental" )
			local game_price = StoreManager.PricingManager:get_price_setting( "games", "*", "initial_night", "rental" )
			local cart_promo_info = kernel.rental_promotion_rxgx_validate_cart( self.items, dvd_price, blu_ray_price, game_price )
			if cart_promo_info ~= nil then
				promo_code.price = cart_promo_info.promo_code_value
				kernel.rental_promotion_set_promotion( promo_code )
				kernel.sessionsetdiscount2(
					self.session_id,
					promo_code.name,
					promo_code.price
				)
				if cart_promo_info.is_cart_valid == true then
					if on_valid ~= nil then
						on_valid( cart_promo_info )
					end
				else
					if on_invalid ~= nil then
						on_invalid( cart_promo_info )
					end
				end
			else
				self:clear_promo_code()
			end
		elseif promo_code ~= nil and promo_code.campaign_titles ~= nil then
            local is_valid = kernel.rental_promotion_tsp_validate_cart( self.items )
            if is_valid == true then
				promo_code.price = promo_code.original_price
                promo_code.percent = promo_code.original_percent
				kernel.rental_promotion_set_promotion( promo_code )
				kernel.sessionsetdiscount2(
					self.session_id,
					promo_code.name,
					promo_code.price
				)
				if on_valid ~= nil then
					on_valid()
				end
            else
				promo_code.price = 0
                promo_code.percent = 0
				kernel.rental_promotion_set_promotion( promo_code )
				kernel.sessionsetdiscount2(
					self.session_id,
					promo_code.name,
					promo_code.price
				)
				if on_invalid ~= nil then
					on_invalid()
				end
            end
        end
	end
	
	t.clear_promo_code = function( self )
		kernel.log( "> Clear Promo Code was called" )
		kernel.rental_promotion_clear_promotion()
		kernel.sessioncleardiscount2( self.session_id )
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
			self.items = {}
			if kernel.ttsenabled() == true then
				self:update_cart_macros()
			end
			return
		end

		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
				for _, v in ipairs( vend_results ) do
					kernel.log( "> vend_results b:" .. tostring( v.barcode ) .. ", s:" .. tostring( v.status ) )
					local item = self:get_item_by_barcode( tostring( v.barcode ) )
					if item ~= nil then
						item.status = v.status
					end
				end
				local all_vended = true
				local at_least_one_game = false
				for _, v in ipairs( self.items ) do
					kernel.log( "> local cart item: " .. tostring( v.barcode ) )
					if v.status == "Pending"
					or v.status == "ProductSelected" then
						v.status = "NotFullfilled"
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
				kernel.sessionsave2( self.session_id )
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
			self.items = {}
			if kernel.ttsenabled() == true then
				self:update_cart_macros()
			end
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
			self.items = {}
			if kernel.ttsenabled() == true then
				self:update_cart_macros()
			end
			return
		end
		
		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
				--OnComplete
				for _, v in ipairs( vend_results ) do
					kernel.log( "> vend_results b:" .. tostring( v.barcode ) .. ", s:" .. tostring( v.status ) )
					local item = self:get_item_by_barcode( tostring( v.barcode ) )
					if item ~= nil then
						item.status = v.status
					end
				end
				self:set_session_cart_groups()

				local barcodes_to_reset = {}
				
				for _, v in ipairs( self.items ) do
					kernel.log( "> local cart item: " .. tostring( v.barcode ) )
					if v.status == "Pending"
					or v.status == "ProductSelected" then
						v.status = "NotFullfilled"
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
				kernel.sessionsave2( self.session_id )
			end
		)
		self.vend_manager:prepos_vend_continue()
		kernel.log( "ShoppingCart:vend_reservation_cart COMPLETE" )
	end
	
	t.get_barcodes_to_vend = function( self )
		return get_barcodes_for_products( self.items )
	end

	local function trim_spaces(s)
	  local i1,i2 = string.find(s,'^%s*')
	  if i2 >= i1 then s = string.sub(s,i2+1) end
	  local i1,i2 = string.find(s,'%s*$')
	  if i2 >= i1 then s = string.sub(s,1,i1-1) end
	  return s
	end

	local function display_flag( releaseDate, days, product_id, productfamilytype, index, product )
		local calculatedSortDate = string.sub( product.sort_date, 1, 8 )
		local daysByCalculatedSortDate = kernel.getelapseddays( calculatedSortDate, get_current_date_string( "%4d%02d%02d" ) )
		kernel.showactor( "flag_" .. tostring( index ) )
		if product.genres[1002] ~= nil then
			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_gold_small" )
			kernel.setactortext( "flag_" .. tostring( index ), strings.flag_award_winner )
		elseif StoreManager.PricingManager:is_early_released( product )
		and StoreManager.PricingManager:is_alternate_priced( product_id ) then

			local product_pricing = StoreManager.PricingManager:get_product_price( product_id )
			local first_night_string = "$3"
			if product_pricing.initial_night ~= nil then
				first_night_string = string.format( "%s%.2f", strings.currency_symbol, product_pricing.initial_night )
			end

			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_red_small" )
			kernel.setactortext( "flag_" .. tostring( index ), string.gsub( strings.flag_price_test, "{0}", first_night_string ) )

		elseif daysByCalculatedSortDate ~= nil
		and daysByCalculatedSortDate <= 14
		and product.genres[1025] ~= nil
		and productfamilytype == 1 then
			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_red_small" )
			kernel.setactortext( "flag_" .. tostring( index ), strings.flag_back_again )
		elseif daysByCalculatedSortDate ~= nil
		and daysByCalculatedSortDate <= 14
		and product.genres[1095] ~= nil
		and productfamilytype == 2 then
			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_red_small" )
			kernel.setactortext( "flag_" .. tostring( index ), strings.flag_classics )
		elseif days <= 28 then
			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_red_small" )
			kernel.setactortext( "flag_" .. tostring( index ), strings.flag_new )
		elseif days >= 105
		and product_id ~= 1139
		and productfamilytype == 1
		and kernel.is_product_thinned(product_id) == true then
			kernel.setactorimage("flag_"..tostring(index),"image_flag_orange_small")
			kernel.setactortext("flag_"..tostring(index),strings.flag_last_chance)
		else
			kernel.hideactor("flag_"..tostring(index))
		end
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

	t.show_cart_table = function( self )
		shopping_cart_scene_manager:clear()

		local today = os.date( "*t" )

		local x = 0
		local y = 170
		local banner_y = 346
        local ada_y_difference = 100
		
        if kernel.get_application_state_value("IsADAAccessible") == true then
			y = y + ada_y_difference
			banner_y = banner_y + ada_y_difference
		end

		if #self.items == 0 then
			x = 437
		elseif #self.items == 1 then
			x = 342
		elseif #self.items == 2 then
			x = 247
		elseif #self.items == 3 then
			x = 152
		elseif #self.items == 4 then
			x = 57
		elseif #self.items == 5 then
			x = 57
		end


		for k, v in ipairs( self.items ) do
			local catalog_entry = MasterProductCatalog[v.product_id]
			local product_type = ProductTypes[catalog_entry.product_type_id]
			local product_type_md = ProductTypesMetaData[product_type.product_type_id]

			local rowName = "row_name_" .. k
			local numberColumnName = "number_column_" .. k
			local typeColumnName = "type_column_" .. k
			local nameColumnName = "name_column_" .. k
			local priceColumnName = "price_column_" .. k
			local removeColumnName = "remove_column_" .. k
			local removeButtonName = "remove_button_" .. k

			local imageName = "image_name_" .. k
			shopping_cart_scene_manager:add_actor(
				imageName,
				x,
				y,
				151,
				216,
				{ renderOptions = "Image, DrawBorder" } )
			kernel.setbordercolor( imageName, {["r"] = 0, ["g"] = 0, ["b"] = 0} )
			kernel.setborderstrokeweight( imageName, 1 )

			draw_image(catalog_entry, imageName)

			local flagName = "flag_" .. k
			shopping_cart_scene_manager:add_actor(
				flagName,
				x-17,
				y-26,
				140,
				40,
				{	renderOptions = "Image, Text",
					foregroundColor="white",
					visible="false",
					font="font_akzidenz_bold_tiny",
					horizontalAlignment="center",
					textRegion = { x = 2, y = 5, width = 120, height = 20 }
					 } )


			local bannerName = "banner_" .. k
			shopping_cart_scene_manager:add_actor(
				bannerName,
				x,
				banner_y,
				151,
				40,
				{	renderOptions = "Image, Text",
					foregroundColor="white",
					verticalAlignment="center",
					visible="false",
					horizontalAlignment="center" }
			)

			--display banner

			display_banner( product_type.product_type_name, product_type.product_family_id, k )

			local releaseDate = string.sub(catalog_entry.merchandise_date, 1, 8)
			local days = kernel.getelapseddays( releaseDate, get_current_date_string( "%4d%02d%02d" ) )

			--display flag

			display_flag( releaseDate, days, v.product_id, product_type.product_family_id, k, catalog_entry )

			local cancel_imageName = "cancel_image_name_" .. k
            local cancel_y = y - 17
            if kernel.get_application_state_value("IsADAAccessible") == true then
                cancel_y = cancel_y + 205
            end
            shopping_cart_scene_manager:add_actor(
				cancel_imageName,
				x + 125,
				cancel_y,
				41,
				41,
				{ 	renderOptions = "BackgroundGradient, Text, DrawBorder",
					gradientAngle="90",
					font="font_akzidenz_bold_small",
					text="X",
					foregroundColor="White",
					verticalAlignment="center",
					horizontalAlignment="center",
					hitOptions="Enabled"}
			)

			kernel.setcornersize( cancel_imageName, 20, 20 )
			kernel.setactorbackgroundcolor( cancel_imageName, {["r"] = 54, ["g"] = 50,["b"] = 50} )
			kernel.setactorgradienttargetcolor( cancel_imageName, {["r"] = 24, ["g"] = 20,["b"] = 20} )
			kernel.setbordercolor( cancel_imageName, {["r"] = 255, ["g"] = 255, ["b"] = 255} )
			kernel.setborderstrokeweight( cancel_imageName, 2 )

			kernel.registeractorhitnotify(
				cancel_imageName,
				function()
					local remove_item = function()
						view_analytics_event2( "buttonPress", "shopping-cart-remove-item," .. tostring( catalog_entry.product_id ), self.session_id )
						self:remove_item( k )
						shopping_cart_scene_manager:clear()
						self:show_cart_table()
					end

					local clear_in_cart = function()
						self:clear_promo_code()
						self.in_cart_promo_offer_made = true
						self.in_cart_promo_offer_cancel = true
						self.in_cart_title_marketing = false
					end

					if self.in_cart_title_marketing == true then
						local shopping_cart_clone = deepcopy( self )
						table.remove( shopping_cart_clone.items, k )

						local result, data = CartSatisfiesInCartOffer( shopping_cart_clone )
						if result == false then
							local message_view_manager = newmessageviewmanager()
							message_view_manager:show()
							view_analytics_event2( "context", "In-cart item remove warning", self.session_id )
							message_view_manager:start_timer(
								function()
									kernel.log( "Title marketing remove item warning popup timed out" )
									remove_item()
									clear_in_cart()
								end,
								20000
							)
							message_view_manager:set_title( strings.in_cart_remove_item_warning_header )
							message_view_manager:set_message( strings.in_cart_remove_item_warning_message )
							message_view_manager:show_first_button(
								strings.back_button,
								function()
									kernel.log( "User cancelled remove item with back" )
									view_analytics_event2( "buttonPress", "in-cart-removeitem-cancel", self.session_id )
								end
							)
							message_view_manager:show_second_button(
								strings.continue_button,
								function()
									view_analytics_event2( "buttonPress", "in-cart-removeitem-confirm", self.session_id )
									kernel.log( "User confirmed remove item warning popup with continue" )
									remove_item()
									clear_in_cart()
								end
							)
						else
							remove_item()
						end
					else
						remove_item()
					end
				end
			)

			if DisplayModifiedShoppingCart ~= true then
				local reflection_imageName = "reflection_image_name_" .. k
				shopping_cart_scene_manager:add_actor(
					reflection_imageName,
					x,
					y + 356,
					151,
					-140,
					{
						renderOptions = "Image"
						} )
				draw_image(catalog_entry, reflection_imageName)

				kernel.setactoropacity(reflection_imageName, 0.32)

				---fading the reflection image

				local reflection_image_opacity1 = "reflection_image_opacity_hiding_" ..k
				shopping_cart_scene_manager:add_actor(
					reflection_image_opacity1,
					x ,
					y + 216,
					151,
					140,
					{
					image="image_transparent",
					renderOptions="Image"
					}
				)
			end

			local product_title = trim_spaces( catalog_entry.long_name )

			local title_imageName = "title_" .. k
			shopping_cart_scene_manager:add_actor(
				title_imageName,
				x + 10,
				y + 231,
				130,
				70,
				{
					foregroundColor="White",
					font="font_akzidenz_bold_tiny",
					horizontalAlignment="center",
					verticalAlignment="near",
					renderOptions = "Text"
				}
			)

			if DisplayModifiedShoppingCart ~= true then
				kernel.setactortext(title_imageName,product_title )
			end

			local size = kernel.measureactortext( title_imageName )
			local height = 0
			if size ~= nil then
				height = math.ceil( size.height )
			end

			local title_price = "title_price" .. k
			shopping_cart_scene_manager:add_actor(
				title_price,
				x + 5,
				y + 231 + height,
				130,
				20,
				{
					foregroundColor="black",
					font="font_smaller_italic",
					horizontalAlignment="center",
					verticalAlignment="near",
					renderOptions = "Text"
				}
			)

			local item_price = string.format("$%s", string.format("%.2f", v.price ) )
			if v.action == "rental" then
				local item_pricing = StoreManager.PricingManager:get_product_price( v.product_id )
				if item_pricing.initial_night ~= item_pricing.extra_night then
					item_price = item_price .. " " .. strings.first_day
					local item_price2 = string.format("$%s", string.format("%.2f", item_pricing.extra_night ) )
					local title_price2 = "title_price2" .. k
					shopping_cart_scene_manager:add_actor(
						title_price2,
						x + 5,
						y + 246 + height,
						130,
						20,
						{
							foregroundColor="black",
							font="font_smaller_italic",
							horizontalAlignment="center",
							verticalAlignment="near",
							renderOptions = "Text"
						}
					)
					local item_price2 = item_price2 .. " " .. strings.extra_day
					kernel.setactorwidth( title_price2, kernel.getactorwidth( title_price2 ) + 10 )
					kernel.setactortext( title_price2, item_price2 )
				else
					item_price = item_price .. " " .. strings.a_day
				end
			elseif v.action == "purchase" then
				item_price = item_price .. " " .. strings.purchase
			end
			--kernel.moveactorx( title_price, -5 )
			kernel.setactorwidth( title_price, kernel.getactorwidth( title_price ) + 10 )
			kernel.setactortext( title_price, item_price )

			x= x + 190

		end

		if #self.items == 5 then
			kernel.hideactor("add_movie_add_game")
			kernel.hideactor("add_movie_add_game_reflection")
			kernel.hideactor("add_movie_add_game_reflection_opacity_hiding")
			kernel.hideactor("all_movies_button")
			kernel.hideactor("games_button")
		else
			kernel.showactor("add_movie_add_game")
			if DisplayModifiedShoppingCart ~= true then
				kernel.showactor("add_movie_add_game_reflection")
				kernel.showactor("add_movie_add_game_reflection_opacity_hiding")
			else
				kernel.hideactor("add_movie_add_game_reflection")
				kernel.hideactor("add_movie_add_game_reflection_opacity_hiding")
			end
			kernel.showactor("all_movies_button")
			kernel.showactor("games_button")
			kernel.setactorx("add_movie_add_game", x)
			kernel.setactorx("add_movie_add_game_reflection", x)
			kernel.setactorx("add_movie_add_game_reflection_opacity_hiding", x)
			kernel.setactorx("all_movies_button", x)
			kernel.setactorx("games_button", x)
			if ProductCatalog:in_stock_games_count() == 0 or kernel.get_application_state_value("IsADAAccessible") == true then
				if kernel.getculture() == "en-US" then
					kernel.setactorimage("add_movie_add_game","image_plus_add_movie")
					kernel.setactorimage("add_movie_add_game_reflection","image_plus_add_movie")
				else
					kernel.setactorimage("add_movie_add_game","image_plus_add_movie_es_MX")
					kernel.setactorimage("add_movie_add_game_reflection","image_plus_add_movie_es_MX")
				end
				kernel.hideactor("games_button")
				kernel.setactorheight("all_movies_button",216)
			end

			if kernel.get_application_state_value("IsADAAccessible") == true then
				kernel.setactory( "add_movie_add_game", 170 + ada_y_difference )
				kernel.setactory( "add_movie_add_game_reflection", 526 + ada_y_difference )
				kernel.setactory( "add_movie_add_game_reflection_opacity_hiding", 386 + ada_y_difference )
				kernel.setactory( "all_movies_button", 170 + ada_y_difference )
				kernel.setactory( "games_button", 316 + ada_y_difference )
			end
		end

		if StoreManager:get_purchase_new_price() ~= nil and StoreManager:has_sell_thru_new_in_stock() then
			kernel.hideactor( "add_item_button" )
			kernel.showactor( "back_to_new_for_sale_button" )
			kernel.showactor( "back_to_rentals_button" )
			kernel.setactorlocation( "add_promo_button", 847, 682 )
		end
		if #self.items > 0 then
			kernel.showactor( "my_rentals_button" )
			if self:is_full() == true then
				if StoreManager:get_purchase_new_price() ~= nil and StoreManager:has_sell_thru_new_in_stock() then
					kernel.hideactor( "back_to_new_for_sale_button" )
					kernel.hideactor( "back_to_rentals_button" )
				else
					kernel.hideactor( "add_item_button" )
				end
			else
				if StoreManager:get_purchase_new_price() ~= nil and StoreManager:has_sell_thru_new_in_stock() then
					kernel.showactor( "back_to_new_for_sale_button" )
					kernel.showactor( "back_to_rentals_button" )
				else
					kernel.showactor( "add_item_button" )
				end
			end
		else
			kernel.hideactor( "my_rentals_button" )
		end

		local subtotal_by_group = self:sub_total()
		local sub_total = 0
		local tax_amount = 0
		for k,v in pairs( subtotal_by_group ) do
			if v.discounted_amount ~= nil then
				sub_total = sub_total + v.discounted_amount
			else
				sub_total = sub_total + v.amount
			end
			tax_amount = tax_amount + v.tax_amount

		end

		local total = sub_total + tax_amount

		kernel.setmacro(
			"Subtotal",
			string.format( "%s%.2f", strings.currency_symbol, kernel.round( sub_total, 2 ) ) )
		kernel.setmacro(
			"Tax",
			string.format( "%s%.2f", strings.currency_symbol, kernel.round( tax_amount, 2 ) ) )
		kernel.setmacro(
			"Total",
			string.format( "%s%.2f", strings.currency_symbol, kernel.round( total, 2 ) ) )

		local promo_code = kernel.rental_promotion_get_promotion()
		if #self.items > 0
		and ( self:get_count( nil, "rental" ) == 0 and promo_code ~= nil ) then
			self:clear_promo_code()
		end

		if promo_code ~= nil then
			kernel.showactor( "promo_code_label" )
			kernel.showactor( "promo_code_amount" )
			kernel.disableactor( "promo_code_button" )

			if self:get_count( nil, "purchase" ) == #self.items then
				kernel.setmacro(
					"Discount",
					string.format( "-%s%.2f",
						strings.currency_symbol,
						0 ) )
			else
				local promo_code_price = promo_code.price
				kernel.setmacro(
					"Discount",
					string.format( "-%s%.2f",
						strings.currency_symbol,
						promo_code_price ) )
			end
		else
			kernel.hideactor( "promo_code_label" )
			kernel.hideactor( "promo_code_amount" )
			kernel.enableactor( "promo_code_button" )
		end

		local target_product = nil

		if  self:is_full() == false
		and #self.items > 0 then
			math.randomseed( os.time() )
			local random_cart_item = math.random( 1, #self.items )
			target_product = MasterProductCatalog[self.items[random_cart_item].product_id]
		end
		if ShoppingCart:size() > 0 then
			kernel.enableactor("pay_button")
			if EnableCreditsButton then
				kernel.enableactor("credits_button")
			end
		else
			kernel.disableactor("pay_button")
			if EnableCreditsButton then
				kernel.disableactor("credits_button")
			end
		end
		if (ShoppingCart.in_cart_promo_offer_made == true
		and ShoppingCart.in_cart_promo_offer_cancel == false) then
			kernel.disableactor("promo_code_button")
		else
			kernel.enableactor("promo_code_button")
		end

		if kernel.ttsenabled() == true then
			kernel.ttsrepeat();  -- the titles and assests have been set and toggled
		end
	end

	t.items = {}

	t.coming_soon_optin = {
		email_address = nil,

		optin_items = {},

		indexof_optin_item = function( self, product_id )
			for k, v in ipairs( self.optin_items ) do
				if v.item_id == tostring( product_id ) then
					return k
				end
			end
			return 0
		end,

		send_kiosk_optin_message = function( self )
			if #self.optin_items ~= 0 then
				kernel.promosvcs_kioskoptin( StoreManager:get_number(), self.optin_items, nil )
			end
		end
	}
	
	t.get_coming_soon_vend_screen_titles_automation = function( self )
		local coming_soon_title_generator = newcomingsoongenerator()
		local coming_soon_products = coming_soon_title_generator:get_products( self.items, 3 )
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
		self.blu_ray_upsell:set_shopping_cart_items()
		local clear_in_cart_promo = false
		for k, v in ipairs( self.items ) do
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
		kernel.merchsvcs_gcauthorize(
			self.session_id,
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
				
				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end
	
	t.blu_ray_upsell = {
		do_not_show_again = false,
		
		items = {},
		
		has_in_stock_and_available_blu_rays = function( self, insert )
			local has_items = false
			
			local function contains( items, dvd_product_id )
				for k, v in ipairs( items ) do
					if v.dvd_product_id == dvd_product_id then
						return true
					end
				end
				return false
			end
			
			local function get_blu_ray_product_id_from_product_id_list( product_id_list )
				for k, v in pairs( product_id_list ) do
					if MasterProductCatalog[v] ~= nil and MasterProductCatalog[v].product_type_id == 2 then
						return v
					end
				end
				return nil
			end
			
			local function shopping_cart_contains_blu_ray_product_id( blu_ray_product_id )
				for k, v in ipairs( ShoppingCart.items ) do
					if v.product_id == blu_ray_product_id then
						return true
					end
				end
				return false
			end
			
			for k, v in ipairs( ShoppingCart.items ) do
				if MasterProductCatalog[v.product_id] ~= nil
				and MasterProductCatalog[v.product_id].product_type_id == 1
				and v.action == "rental"
				and contains( self.items, v.product_id ) == false then
					if TitleRollupXREF[v.product_id] ~= nil then
						local product_group_id = TitleRollupXREF[v.product_id].product_group_id
						if ProductGroup[product_group_id] ~= nil then
							local product_id_list = ProductGroup[product_group_id].product_id_list
							if product_id_list ~= nil and #product_id_list > 0 then
								local blu_ray_product_id = get_blu_ray_product_id_from_product_id_list( product_id_list )
								if blu_ray_product_id ~= nil
								and ProductCatalog[blu_ray_product_id] ~= nil
								and ProductCatalog[blu_ray_product_id].in_stock == true
								and ProductCatalog:in_stock( blu_ray_product_id ) == true
								and ProductCatalog[blu_ray_product_id].is_available == true
								and shopping_cart_contains_blu_ray_product_id( blu_ray_product_id ) == false then
									if insert == true then
										table.insert( self.items, { dvd_product_id = v.product_id, blu_ray_product_id = blu_ray_product_id, product_group_id = product_group_id } )
									end
									has_items = true
								end
							end
						end
					end
				end
			end
			return has_items
		end,
		
		in_cart_check = function( self )
			local in_cart_running = false
			local games_template_running = false
			
			if InCartTemplateName ~= nil and InCartTemplateName ~= "Default-No-Offer" then
				in_cart_running = true
			end
			
			if InCartTemplateName ~= nil
			and ( InCartTemplateName == "ROGO GAME B" or InCartTemplateName == "ROGO GAME FREE G" or InCartTemplateName == "ROGO GAME FREE INCLUDE H" ) then
				games_template_running = true
			end
			
			if in_cart_running == false or games_template_running == true then
				return true
			end
			return false
		end,
		
		show = function( self )
			if BluRayUpsellEnabled == true
            and kernel.ttsenabled() == false
			and self:in_cart_check() == true
			and self:has_in_stock_and_available_blu_rays( false ) == true
			and self.do_not_show_again == false then
				return true
			end
			return false
		end,
		
		set_shopping_cart_items = function( self )
			local function get_blu_ray_upsell_table( shopping_cart_item )
				for k, v in ipairs( self.items ) do
					if v.offer == "accepted" and v.blu_ray_product_id == shopping_cart_item.product_id then
						return { product_group_id = v.product_group_id, offer = v.offer, title_id = v.blu_ray_product_id }
					elseif v.offer == "rejected" and v.dvd_product_id == shopping_cart_item.product_id then
						return { product_group_id = v.product_group_id, offer = v.offer, title_id = v.dvd_product_id }
					end
				end
				return {}
			end
			
			for k, v in ipairs( ShoppingCart.items ) do
				v.blu_ray_upsell = get_blu_ray_upsell_table( v )
			end
		end
	}

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
		for k, v in ipairs( self.items ) do
			v.status = "NotVended"
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
		local multi_disc_product_ids_to_add = {}
		for k, v in ipairs( self.items ) do
			if is_multi_disc( v.product_id ) == true then
				local product_id_list = ProductGroup[MultiDiscXREF[v.product_id].product_group_id].product_id_list
				for k2, v2 in ipairs( product_id_list ) do
					if v2 ~= v.product_id and self:has_item( v2 ) == false then
						table.insert( multi_disc_product_ids_to_add, v2)
					end
				end
			end
		end
		
		for k, v in ipairs( multi_disc_product_ids_to_add ) do
			if MasterProductCatalog[v] ~= nil then
				kernel.log( "MDV - adding product id " .. tostring( v ) )
				self:add_item( MasterProductCatalog[v], 1, "rental", "Browse" )
			end
		end
	end
	
	t.remove_multi_disc_discs = function( self )
		local function get_shopping_cart_index_by_product_id( product_id )
			for k, v in ipairs( self.items ) do
				if v.product_id == product_id then
					return k
				end
			end
			return 0
		end
		
		local multi_disc_product_ids_to_remove = {}
		for k, v in ipairs( self.items ) do
			if is_multi_disc( v.product_id ) == true and MasterProductCatalog[v.product_id] ~= nil and MasterProductCatalog[v.product_id].disc_number ~= nil and MasterProductCatalog[v.product_id].disc_number > 1 then
				table.insert( multi_disc_product_ids_to_remove, v.product_id )
			end
		end
		
		for k, v in ipairs( multi_disc_product_ids_to_remove ) do
			kernel.log( "MDV - removing product id " .. tostring( v ) )
			self:remove_item( get_shopping_cart_index_by_product_id( v ), false )
		end
	end
	
	t.set_multi_disc_flags = function( self )
		for k, v in ipairs( self.items ) do
			v.multi_disc = is_multi_disc( v.product_id )
		end
	end
	
	t.contains_multi_disc = function( self )
		for k, v in ipairs( self.items ) do
			if is_multi_disc( v.product_id ) == true then
				return true
			end
		end
		return false
	end
	
	t.contains_all_title_rollup_movie_group = function( self, product )
		if product ~= nil and product.kiosk_product_type == 3 and product.is_movie == true then
			local dvd_product = nil
			local blu_ray_product = nil
			
			if product.dvd_index > 0 then
				dvd_product = product.kiosk_products[product.dvd_index]
			end
			if  product.blu_ray_index > 0 then
				blu_ray_product = product.kiosk_products[product.blu_ray_index]
			end
			
			if (dvd_product == nil and blu_ray_product ~= nil) then
			  return self:has_item( blu_ray_product.product_id ) == true
			end
			if (blu_ray_product == nil and dvd_product ~= nil) then
			  return self:has_item( dvd_product.product_id ) == true
			end
			if (blu_ray_product ~= nil and dvd_product ~= nil) then
				-- make sure product has stock or this function doesn't work correctly
				return (  (blu_ray_product.in_stock == false) or (self:has_item( blu_ray_product.product_id ) == true) ) and ( (dvd_product.in_stock == false) or  (self:has_item( dvd_product.product_id ) == true) )
			end

			return false
		else
			return product ~= nil and self:has_item( product.product_id ) == true
		end
	end
	
	return t
end
