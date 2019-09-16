--
-- shopping-cart-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newshoppingcart()
	local t = {}

	t.vend_manager = newvendmanager()

	t.alternate_optin = false
	t.alternate_optin_not_shown = true
	
	t.has_customer_profile = false
	
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
	
	t.analytics_add_auth_account_info = function( self, response)
		kernel.analyticsaddauthtransactionInfo(
			response.properties.customer_profile_number,
			response.properties.account_number,
			response.properties.reference_number,
			self.session_id)
	end

	t.analytics_service_add_auth_account_info = function( self, response )
		kernel.analyticsserviceaddauthtransactionInfo(
			response.properties.customer_profile_number,
			response.properties.account_number,
			response.properties.reference_number,
			self.session_id)
	end

	t.resv_authorize = function( self, reference_number, on_complete )
		kernel.log( "ShoppingCart:resv_authorize BEGIN" )
		kernel.log("Resv. Authorize for Session ID: " .. self.session_id )
		if kernel.get_session_value("UserInfo.PostalCode") ~= nil then
			kernel.sessionsetpostalcode2( self.session_id, kernel.get_session_value("UserInfo.PostalCode") )
		end
		self:get_barcodes_to_vend()
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k,v in ipairs( shopping_cart_items ) do
			kernel.log( "Session add Line Item: " .. tostring( v.barcode ) .. " for ".. tostring( v.action ) .. " Price: " .. tostring( v.price ) ..
				" ".. tostring( v.product_family ) .. "-" .. tostring( v.product_type ) .. " source type: " .. tostring( v.source_type )
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
                kernel.device_service_report_auth_result(r.properties.response == "Approved")
				local authResponse = {
					approved = tostring( r.properties.response == "Approved" ),
					online = tostring( r.properties.is_online ),
					timestamp = tostring( newdatetime() ),
					response = r.response
				}
				kernel.analyticsaddattributes( "resvAuthorize", authResponse, self.session_id )
				
				kernel.analyticsserviceaddeventauthorization( self.session_id, "resvAuthorize", r.properties.is_online, r.properties.response == "Approved", nil )
				
				self:analytics_add_auth_account_info(r)

				self:analytics_service_add_auth_account_info(r)

				if on_complete ~= nil then
					on_complete( r )
				end
			end
		)
		
		kernel.log( "ShoppingCart:resv_authorize COMPLETE" )
	end
	
	t.validatepickup = function( self, reference_numbers, on_complete )
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
                kernel.device_service_report_auth_result(r.properties.response == "Approved")
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
				
				kernel.analyticsserviceaddeventauthorization( self.session_id, "pickupValidation", r.properties.is_online, nil, r.properties.response == "Approved" )
				
				self:analytics_add_auth_account_info(r)

				self:analytics_service_add_auth_account_info(r)

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
        
        local customer_profile_number = kernel.get_session_value("PersonalizationSession.CustomerProfileNumber")
		kernel.merchsvcs_reconcile( self.session_id, customer_profile_number )
		self:end_session()
	end
	
	t.pickupauthorize = function( self, reference_number, cart_was_modified, on_complete )
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

	t.clear_promo_code = function( self )
        kernel.rental_shopping_cart_clear_promo_code( self.session_id )
	end

	t.prepos_vend = function( self )
		kernel.log("> ShoppingCart:prepos_vend starting");
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

	t.set_items_to_not_vended = function( self )
        local shopping_cart_items = kernel.get_rental_shopping_cart_items( self.session_id )
        for k, v in ipairs( shopping_cart_items ) do
            -- update status in the lua object and on the .net side 
			v.status = "NotVended"
            kernel.rental_shopping_cart_update_item_status( self.session_id, k, "NotVended" )
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
