--
-- shopping-cart-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newshoppingcart()
	local t = {}

	t.vend_manager = newvendmanager()

	t.messagebox = newmessageboxmanager()

	t.alternate_optin = false
	t.alternate_optin_not_shown = true
	
	t.has_customer_profile = false
	
    t.prompt_required = false

    t.auth_days = 0

	t.abandon = function( self, reason )
        kernel.abandon_rental_shopping_cart( self.session_id, reason )
	end

	t.close_analytics = function( self, abandoned )
        kernel.close_shopping_cart_analytics(self.session_id, abandoned)
	end

	t.end_session = function( self )
        kernel.shopping_cart_end_session(self.session_id)
	end

	t.save_session = function( self )
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
		kernel.rental_promotion_clear_promotion()
		self.vend_manager:init()
		self.session_id = kernel.sessionstartnew2( StoreManager:get_number() )
		kernel.log(">  session_id = " .. self.session_id )
		kernel.sessionsetcurrent( self.session_id )
        kernel.create_rental_shopping_cart( self.session_id )
	end

	t.add_session_event = function( self, event_type, event_description )
		kernel.sessionaddevent2( self.session_id, event_type, event_description )
	end

	t.preauth = function( self, on_complete )
        kernel.log("ShoppingCart.Preauth")
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:preauth was called with invalid account data." )
			return
		end
		kernel.log("PreAuth for Session ID: " .. self.session_id )
		if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
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
		if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
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
					kernel.shopping_cart_commit(self.session_id, "User checked out successfully." )
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
		if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
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
				self:analytics_add_auth_account_info(r)

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
	end

	t.reconcile = function( self )
		kernel.log("Shopping cart reconcile() " )
		if kernel.get_session_value("UserInfo.Email") ~= nil then
			kernel.sessionsetemail2( self.session_id, kernel.get_session_value("UserInfo.Email") )
		end
		self:set_session_cart_groups()
        
        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_reconcile( self.session_id, customer_profile_number )
		self:end_session()
	end
	
	t.pickupauthorize = function( self, reference_number, cart_was_modified, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:pickupauthorize was called with invalid account data." )
			return
		end

		if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
		end

		local error_message_view_manager = newerrormessageviewmanager()
		error_message_view_manager:show()
		error_message_view_manager:set_message( strings.authorization_message )

		kernel.log( "Pickup Authorize for Session ID: " .. self.session_id )
		-- updated cart with barcodes and status
		self:get_barcodes_to_vend()
        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_pickupauthorize(
			self.session_id,
            customer_profile_number,
			reference_number,
			cart_was_modified,
			function( reference_number, result )
				if result.properties.response ~= nil then
					kernel.log( "Pickup Authorize Response : ".. result.properties.response )
				end

				if on_complete ~= nil then
					on_complete( reference_number, result )
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

				if kernel.is_session_committed(self.session_id) then
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
		if barcodes_to_vend == nil or kernel.does_shopping_cart_contain_all_reserved_barcodes() == false then
            -- send a kiosk alert
			show_maintenance_view(
				"shopping-cart-functions.lua",
				"HardwareError",
				"VendFalseInventory",
				"barcodes_to_vend is empty",
				"0064SCBE",
				false )
			-- since something has gone terribly wrong abort the session to prevent stuck vend or stuck queue
            kernel.log("Aborting reservation pickup session because reserved barcode(s) were not found in the shopping cart.")
			kernel.close_open_session_and_start_new_rental_session()
			return
		end
		
		self.vend_manager:prepos_vend(
			barcodes_to_vend,
			function( vend_results )
                kernel.recommendation_on_pickup_preposition_vend_complete(vend_results, reference_number, modified_cart)
			end
		)
		self.vend_manager:prepos_vend_continue()
		kernel.log( "ShoppingCart:vend_reservation_cart COMPLETE" )
	end
	
	t.get_barcodes_to_vend = function( self )
        return kernel.rental_shopping_cart_assign_barcodes_to_vend(self.session_id)
	end

	local function trim_spaces(s)
	  local i1,i2 = string.find(s,'^%s*')
	  if i2 >= i1 then s = string.sub(s,i2+1) end
	  local i1,i2 = string.find(s,'%s*$')
	  if i2 >= i1 then s = string.sub(s,1,i1-1) end
	  return s
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
	
	t.gc_authorize = function( self, on_complete )
		if not kernel.sessioncaptureaccountdata2( self.session_id ) then
			self.messagebox:show( "WARNING: ShoppingCart:gc_authorize was called with invalid account data." )
			return
		end
		kernel.log( "GC Authorize for Session ID: " .. self.session_id )
		--if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			--kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
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
					kernel.shopping_cart_commit(self.session_id, "User checked out successfully." )
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
	
	t.service_fee_enabled = function( self )
		return kernel.rental_shopping_cart_service_fee_enabled( self.session_id )
	end

	t.default_service_fee_cents = function( self )
		return kernel.rental_shopping_cart_default_service_fee_cents( self.session_id )
	end

	t.get_pickup_recommendations_shown = function( self )
		return kernel.rental_shopping_cart_get_pickup_recommendations_shown( self.session_id )
	end

	return t
end
