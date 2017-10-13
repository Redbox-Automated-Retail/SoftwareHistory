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

    t.in_cart_do_not_show = false

	t.marketing_opt_in = "DoNotShow"
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
		if ComingSoonOptIn == true then
            kernel.promosvcs_coming_soon_kioskoptin()
		end
		kernel.sessionabandon2( self.session_id, reason )
		kernel.sessionend2( self.session_id )
		kernel.sessionsave2( self.session_id )
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

	t.send_cart_to_analytics = function( self )
        kernel.rental_shopping_cart_send_cart_to_analytics( self.session_id )
	end

    t.get_count = function( self, product_family, action )
        return kernel.rental_shopping_cart_item_count( self.session_id, product_family, action)
    end

    t.count_for_item = function( self, id )
		local item_count = kernel.current_rental_shopping_cart_product_count( id )
		local as_a_rental = kernel.current_rental_shopping_cart_has_item_with_action( id, "rental" )
		return item_count, as_a_rental
	end

	t.add_item = function( self, catalog_entry, qty, action_code, source_type )
        -- add to rental shopping cart
        kernel.rental_shopping_cart_add_item( self.session_id, catalog_entry, action_code, source_type )
	end

	t.remove_item = function( self, index, user_removed )
        -- removed item from rental shopping cart
        kernel.rental_shopping_cart_remove_item(self.session_id, index, user_removed )

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
									local cart_promo_info = kernel.rental_shopping_cart_validate_and_update_promo_code(self.session_id)
                                    if cart_promo_info ~= nil and cart_promo_info.is_cart_valid == true then
										if on_complete ~= nil then
											on_complete()
										end
                                    else 
										message_view_manager:show()
										message_view_manager:set_message_header( "Oops" )
    									message_view_manager:set_message( tostring(cart_promo_info.title) .. "\n\n" .. tostring(cart_promo_info.message) )
										message_view_manager:hide_first_button()
										message_view_manager:show_second_button( strings.continue_button, done )
										message_view_manager:start_timer( done )
										view_analytics_event( "context", "rxgxWarning" )
									end
								elseif promo_code.campaign_titles ~= nil then
									local cart_promo_info = kernel.rental_shopping_cart_validate_and_update_promo_code(self.session_id)
                                    if cart_promo_info ~= nil and cart_promo_info.is_cart_valid == true then
										if on_complete ~= nil then
											on_complete()
										end
                                    else
										message_view_manager:show()
										message_view_manager:set_message_header( strings.tsp_invalid_header )
										message_view_manager:set_message( strings.tsp_invalid_message )
										message_view_manager:hide_first_button()
										message_view_manager:show_second_button( strings.continue_button, done )
										message_view_manager:start_timer( done )
										view_analytics_event( "context", "titlelevelpromoWarning" )
									end
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

		kernel.start_idle_timer( "idle_timer" )
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
				kernel.sessionsave2( self.session_id )
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

	local function display_flag( releaseDate, days, product_id, productfamilytype, index, product )
		local calculatedSortDate = string.sub( product.sort_date, 1, 8 )
		local daysByCalculatedSortDate = kernel.getelapseddays( calculatedSortDate, get_current_date_string( "%4d%02d%02d" ) )
		kernel.showactor( "flag_" .. tostring( index ) )
		if product.genres[1002] ~= nil then
			kernel.setactorimage( "flag_" .. tostring( index ), "image_flag_gold_small" )
			kernel.setactortext( "flag_" .. tostring( index ), strings.flag_award_winner )
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
        local temp_cart_item_count = kernel.rental_shopping_cart_item_count( self.session_id )
		if temp_cart_item_count == 0 then
			x = 437
		elseif temp_cart_item_count == 1 then
			x = 342
		elseif temp_cart_item_count == 2 then
			x = 247
		elseif temp_cart_item_count == 3 then
			x = 152
		elseif temp_cart_item_count == 4 then
			x = 57
		elseif temp_cart_item_count == 5 then
			x = 57
		end

        local shopping_cart_item_count = kernel.rental_shopping_cart_item_count( self.session_id )
        for k=1,shopping_cart_item_count do
            local v = kernel.get_rental_shopping_cart_item( self.session_id, k )
            if v ~= nil then
			    local catalog_entry = MasterProductCatalog[v.product_id]
			    local product_type = ProductTypes[catalog_entry.product_type_id]

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
                            kernel.rental_shopping_cart_update_in_cart_offer_made( self.session_id, true)
						    kernel.rental_shopping_cart_update_in_cart_offer_cancel( self.session_id, true )
                            kernel.rental_shopping_cart_update_in_cart_title_marketing( self.session_id, false )
					    end

					    if kernel.rental_shopping_cart_get_in_cart_title_marketing( self.session_id ) == true then
                            kernel.log("Creating temp RentalShoppingCart to retest in cart offer conditions")
                            shopping_cart_clone = {}
                            shopping_cart_clone.session_id = kernel.create_rental_shopping_cart()
                            --shopping_cart_clone.items = {}
                            local temp_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
                            for k2, v2 in ipairs( temp_cart_items ) do
                                if v2 ~= nil and k2 ~= k then
                                    kernel.log("Adding "..tostring(v2.product_id).." to temp RentalShoppingCart")
                                    kernel.rental_shopping_cart_add_item( shopping_cart_clone.session_id, v2, v2.action, v2.source_type ) 
                                    --table.insert(shopping_cart_clone.items, v2)
                                end
                            end
                             
						    local result = kernel.rental_promotion_cart_satisfies_in_cart_offer( shopping_cart_clone.session_id ) -- CartSatisfiesInCartOffer( shopping_cart_clone )
                            kernel.remove_rental_shopping_cart( shopping_cart_clone.session_id )

						    if result == false then
                                kernel.log("Showing offer canel dialog")
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

				local type_text = v.product_type
				if v.product_type_id == 17 then
					type_text = strings.digital_code
				elseif v.product_type_id == 18 then
					type_text = strings.nintendo_switch
				end
				local action_text = strings.cart_rental
				if v.action == "purchase" then
					action_text = strings.cart_purchase
				end
				shopping_cart_scene_manager:add_actor(
					"type_action" .. k,
					x - 20,
					y + 225,
					191,
					20,
					{
						text = string.gsub( strings.cart_type_action, "%S%S%S", { ["{0}"] = type_text, ["{1}"] = action_text } ),
						foregroundColor = "white",
						font = "font_paragraph_header",
						horizontalAlignment = "center",
						renderOptions = "Text"
					}
				)

				local item_price = string.format( "$%s", string.format( "%.2f", v.price ) )
				if v.action == "rental" then
					local item_pricing = StoreManager.PricingManager:get_product_price( v.product_id )
					if item_pricing.initial_night ~= item_pricing.extra_night then
						item_price = item_price .. strings.cart_first_day .. "\n" .. string.format( "$%s", string.format( "%.2f", item_pricing.extra_night ) ) .. strings.cart_additional_day
					else
						item_price = item_price .. strings.cart_day
					end
				end
				shopping_cart_scene_manager:add_actor(
					"title_price" .. k,
					x - 20,
					y + 245,
					191,
					40,
					{
						text = item_price,
						foregroundColor = "white",
						font = "font_regular",
						horizontalAlignment = "center",
						renderOptions = "Text"
					}
				)

				x = x + 190
            end
		end

		if kernel.rental_shopping_cart_is_full( self.session_id ) then
			kernel.hideactor("add_movie_add_game")
			kernel.hideactor("all_movies_button")
			kernel.hideactor("games_button")
		else
			kernel.showactor("add_movie_add_game")
			kernel.showactor("all_movies_button")
			kernel.showactor("games_button")
			kernel.setactorx("add_movie_add_game", x)
			kernel.setactorx("all_movies_button", x)
			kernel.setactorx("games_button", x)
			if ProductCatalog:in_stock_games_count() == 0 or kernel.get_application_state_value("IsADAAccessible") == true then
				if kernel.getculture() == "en-US" then
					kernel.setactorimage("add_movie_add_game","image_plus_add_movie")
				else
					kernel.setactorimage("add_movie_add_game","image_plus_add_movie_es_MX")
				end
				kernel.hideactor("games_button")
				kernel.setactorheight("all_movies_button",216)
			end

			if kernel.get_application_state_value("IsADAAccessible") == true then
				kernel.setactory( "add_movie_add_game", 170 + ada_y_difference )
				kernel.setactory( "all_movies_button", 170 + ada_y_difference )
				kernel.setactory( "games_button", 316 + ada_y_difference )
			end
		end

		if kernel.rental_shopping_cart_item_count( self.session_id ) > 0 then
			kernel.showactor( "my_rentals_button" )
			if  kernel.rental_shopping_cart_is_full( self.session_id ) == true then
				kernel.hideactor( "add_item_button" )
			else
    			kernel.showactor( "add_item_button" )
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
		if kernel.rental_shopping_cart_item_count( self.session_id ) > 0
		and ( self:get_count( nil, "rental" ) == 0 and promo_code ~= nil ) then
			self:clear_promo_code()
		end

		if promo_code ~= nil then
			kernel.showactor( "promo_code_label" )
			kernel.showactor( "promo_code_amount" )
			kernel.disableactor( "promo_code_button" )

			if self:get_count( nil, "purchase" ) == kernel.rental_shopping_cart_item_count( self.session_id ) then
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

		if kernel.current_rental_shopping_cart_item_count() > 0 then
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
		if (kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true
		and kernel.rental_shopping_cart_get_in_cart_offer_cancel( ShoppingCart.session_id ) == false) then
			kernel.disableactor("promo_code_button")
		else
			kernel.enableactor("promo_code_button")
		end

		if kernel.ttsenabled() == true then
			kernel.ttsrepeat();  -- the titles and assests have been set and toggled
		end
	end

    -- TODO: delete this next line after all the .items references have been removed
	-- t.items = {}
    -- end of section to delete later

--	t.coming_soon_optin = {
--		email_address = nil,

--		optin_items = {},

--		indexof_optin_item = function( self, product_id )
--			for k, v in ipairs( self.optin_items ) do
--				if v.item_id == tostring( product_id ) then
--					return k
--				end
--			end
--			return 0
--		end,

--		send_kiosk_optin_message = function( self )
--			if #self.optin_items ~= 0 then
--				kernel.promosvcs_kioskoptin( StoreManager:get_number(), self.optin_items, nil )
--			end
--		end
--	}
	
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
	
	t.remove_multi_disc_discs = function( self )
        kernel.rental_shopping_cart_remove_multi_disc_discs( self.session_id )
	end
	
	t.contains_multi_disc = function( self )
        kernel.rental_shopping_cart_has_mult_disc_item( self.session_id )
	end
	
	return t
end
