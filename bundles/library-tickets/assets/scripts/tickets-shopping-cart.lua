--
-- tickets-shopping-cart.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newticketscart()

	local t = {}

	t.event = nil
	t.production = nil
	t.selected_supplier_disclaimer_note = nil
	t.selected_delivery_method_id = nil
	t.selected_ticket_group = nil
	t.zip_code = nil
	t.get_tickets_call_made = false

	t.email_optin = false

	t.email_info_address = ""
	t.email_info_sent_product_list = { }


	t.lock_guids = {}
	t.ticket_groups = {}
	t.supplier_disclaimers = {}
	t.working_price_codes = { [1] = { ["PriceCodeId"] = 0, ["Quantity"] = 2 } }

	-- The following objects are used in the PlaceOrder server call; property names must be exact
	t.customer = { ["FirstName"] = "", ["LastName"] = "", ["Email"] = "" }
	t.order_header = { ["Subtotal"] = "0.00", ["TotalFee"] = "0.00", ["TotalTax"] = "0.00", ["Total"] = "0.00", ["OrderId"] = "0" }
	t.order_items = {}
	t.unlock_tickets = {}
	t.PriceCodeManager = newpricecodemanager()
	
	t.promotions = {}

	t.reset_working_price_codes = function( self )
		self.working_price_codes = { [1] = { ["PriceCodeId"] = 0, ["Quantity"] = 2 } }
		old_working_price_codes = {}
	end

	t.set_ticket_groups_from_response = function( self, response )
		self.ticket_groups = {}
		self.supplier_disclaimers = response.properties.SupplierDisclaimers
		for k, v in pairs( response.properties.TicketGroups ) do
			table.insert( self.ticket_groups, v )
		end
	end

	t.clear_lock_guids = function( self )
		for k, v in ipairs( self.lock_guids ) do
			kernel.log( string.format( "|Tickets| Releasing Ticket Lock for TicketGroupId:%s - TicketGroupdGuid:%s - Quantity:%s", v.TicketGroupId, v.TicketGroupGuid, v.Quantity ) )
			kernel.ticketsvcs_releaseticketlock(
				StoreManager:get_number(),
				v.TicketGroupId,
				v.TicketGroupGuid,
				v.Quantity,
				function( result )
					kernel.log( result )
				end
			)
		end
		self.lock_guids = {}
	end

	t.set_lock_guids = function( self )
		self.lock_guids = {}
		for k, v in pairs( self.ticket_groups ) do
			local lock_guid = {
				["Quantity"] = self:quantity_total( v.PriceCodes ),
				["TicketGroupId"] = v.TicketGroupId,
				["TicketGroupGuid"] = v.LockGuid,
				["LockDuration"] = v.LockDuration * 60000
			}
			table.insert( self.lock_guids, lock_guid )
		end
		create_lock_update_timer( self.lock_guids )
	end

	t.reset_cart = function( self )
		kernel.log( "|Tickets| Resetting TicketsCart." )

		self.event = nil
		self.production = nil
		self.selected_delivery_method_id = nil
		self.selected_ticket_group = nil
		self.zip_code = nil

		remove_lock_update_timer()
		self:reset_working_price_codes()
		self.ticket_groups = {}
		self.supplier_disclaimers = {}
		self.get_tickets_call_made = false
		self:clear_lock_guids()
		self:clear_order_header()
		self.order_items = {}
		self.unlock_tickets = {}
		
		self.promotions = {}
	end

	t.send_cart_to_analytics = function( self )
		if self.event ~= nil
		and self.production ~= nil
		and self.selected_ticket_group ~= nil then
			kernel.analyticsaddelement( "shoppingCart", "subTotal", ( self.order_header["Subtotal"] + self.order_header["TotalFee"] ), ShoppingCart.session_id )
			kernel.analyticsaddelement( "shoppingCart", "taxAmount", self.order_header["TotalTax"], ShoppingCart.session_id )
			kernel.analyticsaddelement( "shoppingCart", "grandTotal", self.order_header["Total"], ShoppingCart.session_id )
			kernel.analyticsaddelement( "shoppingCart", "email", self.customer["Email"], ShoppingCart.session_id )
			local attributes = {
				removed = "False",
				eventID = tostring( self.event.event_id ),
				productionID = tostring( self.production.production_id ),
				ticketGroupID = tostring( self.selected_ticket_group.TicketGroupId ),
				ticketGroupType = "",
				SuggestionType = self.selected_ticket_group.SuggestionTypeDisplayName,
				genreID = tostring( self.event.primary_category_id ),
				row = self.selected_ticket_group.Row,
				section = self.selected_ticket_group.Section
			}
			kernel.analyticsaddnode( "ticketItems", "ticketItem", ShoppingCart.session_id )
			kernel.analyticsaddattributes( "ticketItem", attributes, ShoppingCart.session_id  )
			kernel.analyticsaddnode( "ticketItem", "priceCodes", ShoppingCart.session_id )
			for k, v in pairs( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes ) do
				local pc_attributes = {
					priceCodeId = tostring( v.PriceCodeId ),
					quantity = tostring( v.Quantity ),
					pricePerTicket = tostring( v.PricePerTicket ),
					feePerTicket = tostring( v.FeePerTicket ),
					taxPerTicket = tostring( v.PricePerTicketTax )
				}
				kernel.analyticsaddnode( "priceCodes", "priceCode", ShoppingCart.session_id )
				kernel.analyticsaddattributes( "priceCode", pc_attributes, ShoppingCart.session_id )
			end
		end
	end

	t.remove_item_from_cart = function( self )
		local attributes = {
			removed = "True",
			eventID = tostring( self.event.event_id ),
			productionID = tostring( self.production.production_id ),
			ticketGroupID = tostring( self.selected_ticket_group.TicketGroupId ),
			ticketGroupType = "",
			SuggestionType = self.selected_ticket_group.SuggestionTypeDisplayName,
			genreID = tostring( self.event.primary_category_id ),
			row = self.selected_ticket_group.Row,
			section = self.selected_ticket_group.Section
		}
		kernel.analyticsaddnode( "ticketItems", "ticketItem", ShoppingCart.session_id )
		kernel.analyticsaddattributes( "ticketItem", attributes, ShoppingCart.session_id  )
		kernel.analyticsaddnode( "ticketItem", "priceCodes", ShoppingCart.session_id )
		for k, v in pairs( ShoppingCart.TicketsCart.selected_ticket_group.PriceCodes ) do
			local pc_attributes = {
				priceCodeId = tostring( v.PriceCodeId ),
				quantity = tostring( v.Quantity ),
				pricePerTicket = tostring( v.PricePerTicket ),
				feePerTicket = tostring( v.FeePerTicket ),
				taxPerTicket = tostring( v.PricePerTicketTax )
			}
			kernel.analyticsaddnode( "priceCodes", "priceCode", ShoppingCart.session_id )
			kernel.analyticsaddattributes( "priceCode", pc_attributes, ShoppingCart.session_id )
		end
		self.selected_ticket_group = nil
	end

	t.end_session = function( self )
		ShoppingCart:close_analytics( "False" )
		kernel.sessionend2( ShoppingCart.session_id )
		kernel.sessionsave2( ShoppingCart.session_id )
		view_unwindtobottom_safe( "TicketsCart:end_session" )
	end

	t.is_event_in_sent_list = function( self )
		if self.event == nil then
			kernel.log("Cannot call 'is_event_in_sent_list' without an event set in cart.")
			return false
		end
		if self.email_info_sent_product_list == nil or #self.email_info_sent_product_list < 1 then
			return false
		end

		for _,v in ipairs(self.email_info_sent_product_list) do
			if v == self.event.event_id then
				return true
			end
		end

		return false
	end

	t.sub_total = function( self, price_codes )
		local sub_total = 0
		for k, v in pairs( price_codes ) do
			sub_total = sub_total + ( ( v.PricePerTicket ) * v.Quantity )
		end
		return sub_total
	end

	t.fee_total = function( self, price_codes )
		local fee_total = 0
		for k, v in pairs( price_codes ) do
			fee_total = fee_total + ( v.FeePerTicket * v.Quantity )
		end
		return fee_total
	end

	t.quantity_total = function( self, price_codes )
		local quantity_total = 0
		for k, v in pairs( price_codes ) do
			quantity_total = quantity_total + v.Quantity
		end
		return quantity_total
	end

	t.tax_total = function( self, price_codes )
		local tax_total = 0
		for k, v in pairs( price_codes ) do
			tax_total = tax_total + ( v.PricePerTicketTax * v.Quantity )
		end
		return tax_total
	end

	t.clear_order_header = function( self )
		self.order_header = { ["Subtotal"] = "0.00", ["TotalFee"] = "0.00", ["TotalTax"] = "0.00", ["Total"] = "0.00", ["OrderId"] = "0" }
	end

	t.set_order_items = function( self )
		self.order_items = {}
		local order_item = {}
		order_item["TicketGroupId"] = self.selected_ticket_group.TicketGroupId
		order_item["LockGuid"] = self.selected_ticket_group.LockGuid
		order_item["SuggestionTypeId"] = self.selected_ticket_group.SuggestionTypeId
		order_item["DeliveryMethodId"] = self.selected_delivery_method_id
		if ( self.selected_ticket_group.SupplierNoteLogId ~= nil and self.selected_ticket_group.SupplierNoteLogId ~= "" ) then
			order_item["SupplierNoteLogId"] = tostring(self.selected_ticket_group.SupplierNoteLogId)
		end
		local deliver_to_name = ""
		if self.selected_delivery_method_id == "1" then
			deliver_to_name = self.customer.FirstName .. " " .. self.customer.LastName
		end
		order_item["DeliverToName"] = deliver_to_name
		order_item["PriceCodes"] = self.selected_ticket_group.PriceCodes
		table.insert( self.order_items, order_item )
	end

	t.set_unlock_tickets = function( self )
		self.unlock_tickets = {}
		for k, v in ipairs( self.lock_guids ) do
			if v.TicketGroupId ~= self.selected_ticket_group.TicketGroupId then
				local unlock_ticket = {}
				unlock_ticket["TicketGroupId"] = v.TicketGroupId
				unlock_ticket["TicketGroupGuid"] = v.TicketGroupGuid
				unlock_ticket["Quantity"] = v.Quantity
				table.insert( self.unlock_tickets, unlock_ticket )
			end
		end
	end
	
	t.get_production_type_attributes = function( self, production_type_id )
		local PRODUCTION_TYPE = 7
		local attributes = {}
		for k, v in AttributeIdentifierType:pairs() do
			if v.identifier_type_id == PRODUCTION_TYPE and v.identifier_id == production_type_id then
				attributes[v.attribute_id] = { name = v.attribute_name, value = v.attribute_value }
			end
		end
		return attributes
	end
	
	t.get_production_attributes = function( self, production_id )
		local PRODUCTION = 4
		local attributes = {}
		for k, v in AttributeIdentifierType:pairs() do
			if v.identifier_type_id == PRODUCTION and v.identifier_id == production_id then
				attributes[v.attribute_id] = { name = v.attribute_name, value = v.attribute_value }
			end
		end
		return attributes
	end
	
	t.get_attributes = function( self, production )
		local production_attributes = self:get_production_attributes( production.production_id )
		local production_type_attributes = self:get_production_type_attributes( production.production_type_id )
		for k, v in pairs( production_type_attributes ) do
			if production_attributes[k] == nil then
				production_attributes[k] = { name = v.name, value = v.value }
			end
		end
		return production_attributes
	end
	
	t.get_attributes_and_identifer = function(self, production_id )
		local attributes = {}
		for k, v in AttributeIdentifierType:pairs() do
			if v.identifier_id == production_id then
				attributes[k] = { name = v.attribute_name, value = v.attribute_value, identifier_type_id = v.identifier_type_id }
			end
		end
		return attributes
	end

	t.get_attribute_by_name = function( self, attributes, name )
		for k, v in pairs( attributes ) do
			if v.name == name then
				return v
			end
		end
		return nil
	end
	
	t.get_production_property_data = function( self, production_id )
		local production_property_data = {}
		for k, v in ProductionPropertyData:pairs() do
			if v.production_id == production_id then
				production_property_data[v.production_property_id] = { name = v.production_property_name, value = v.production_property_value }
			end
		end
		return production_property_data
	end
	
	t.get_production_property_data_by_name = function( self, production_property_data, name )
		for k, v in pairs( production_property_data ) do
			if v.name == name then
				return v
			end
		end
		return nil
	end
	
	t.get_production_venue_bundle = function( self, production_id )
		local production_venue_bundle = {}
		for k, v in ProductionVenues:pairs() do
			if v.production_id == production_id then
				table.insert( production_venue_bundle, v )
			end
		end
		table.sort(
			production_venue_bundle,
			function( a, b )
				return a.display_order < b.display_order
			end
		)
		return production_venue_bundle
	end
	
	t.get_production_venue_bundle_as_scene_manager = function( self, production_id, x, y, width, height )
		local production_venue_bundle_scene_manager = newscenemanager()
		local production_venue_bundle = self:get_production_venue_bundle( production_id )
		local y_pos = y
		local current_height = 0
		for k, v in ipairs( production_venue_bundle ) do
			local line_height = 19
			local line_text = "- " .. Venues[v.venue_id].venue_name
			if v.alternate_venue_id ~= nil then
				line_height = 38
				line_text = line_text .. " OR " .. Venues[v.alternate_venue_id].venue_name
			end
			
			current_height = y_pos - y
			if current_height + line_height >= height then
				break
			end
			
			local name = "name_" .. tostring( y_pos )
			production_venue_bundle_scene_manager:add_actor(
				name,
				x,
				y_pos,
				width,
				line_height,
				{ text = line_text, font = "font_akzidenz_12_regular", foregroundColor = "black", renderOptions = "Text" },
				nil
			)
			
			if line_height == 38 and kernel.measureactortext( name ).lines_filled == 1 then
				line_height = 19
				kernel.setactorheight( name, line_height )
			end
			
			y_pos = y_pos + line_height
		end
		local blank_line_left = current_height <= height - 38
		return production_venue_bundle_scene_manager, blank_line_left
	end
	
	t.get_production_datetime_display = function( self, production )
		local function format_datetime_string( datetime_string )
			local datetime = {
				year = string.sub( datetime_string, 1, 4 ),
				month = string.sub( datetime_string, 5, 6 ),
				day = string.sub( datetime_string, 7, 8 ),
				hour = string.sub( datetime_string, 9, 10 ),
				min = string.sub( datetime_string, 11, 12 ),
				sec = string.sub( datetime_string, 13, 14 )
			}
			return os.date( "%m/%d", os.time( datetime ) )
		end
		
		local production_datetime_display = ""
		local attributes = self:get_attributes( production )
		if self:get_attribute_by_name( attributes, "ProductionDateOverride" ) ~= nil and self:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= nil and self:get_attribute_by_name( attributes, "ProductionDateOverride" ).value ~= "" then
			production_datetime_display = self:get_attribute_by_name( attributes, "ProductionDateOverride" ).value
		elseif production.production_start_date_time_local ~= nil and production.production_end_date_time_local ~= nil then
			production_datetime_display = format_datetime_string( production.production_start_date_time_local ) .. " - " .. format_datetime_string( production.production_end_date_time_local )
			local current_datetime = os.date("*t")
			local current_datetime_string = string.format( "%4d%02d%02d%02d%02d%02d", current_datetime.year, current_datetime.month, current_datetime.day, current_datetime.hour, current_datetime.min, current_datetime.sec )
			if self:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ) ~= nil and self:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ).value ~= nil and self:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ).value ~= "" and production.production_start_date_time_local <= current_datetime_string then
				production_datetime_display = self:get_attribute_by_name( attributes, "ProductionStartDatePastOverride" ).value .. " " .. format_datetime_string( production.production_end_date_time_local )
			end
		end
		return production_datetime_display
	end
	
	t.should_display_disclaimer = function( self )
		if self.selected_ticket_group.SupplierNoteLogId ~= nil
		and self.selected_ticket_group.SupplierNoteLogId ~= ""
		and table_count( self.supplier_disclaimers ) ~= 0
		and self.supplier_disclaimers[self.selected_ticket_group.SupplierNoteLogId] ~= nil then
			self.selected_supplier_disclaimer_note = self.supplier_disclaimers[self.selected_ticket_group.SupplierNoteLogId].SupplierNote
			return true
		end
		
		return false
	end
	
	t.set_promotions = function( self, production_ids )
		local PRODUCTION = 4
		local current_datetime_utc = os.date("!*t")
		local current_datetime_utc_string = string.format( "%4d%02d%02d%02d%02d%02d", current_datetime_utc.year, current_datetime_utc.month, current_datetime_utc.day, current_datetime_utc.hour, current_datetime_utc.min, current_datetime_utc.sec )
		
		local promotion_identifiers = {}
		for k1, v1 in ipairs( production_ids ) do
			for k2, v2 in PromotionIdentifier:pairs() do
				if v2.identifier_type_id == PRODUCTION and v2.identifier_id == v1 and current_datetime_utc_string >= v2.promotion_start_date_time_utc and current_datetime_utc_string <= v2.promotion_end_date_time_utc and promotion_identifiers[v2.promotion_id] == nil then
					promotion_identifiers[v2.promotion_id] = v2
				end
			end
		end
		
		self.promotions = {}
		for k, v in pairs( promotion_identifiers ) do
			if Promotion[v.promotion_id] ~= nil then
				table.insert( self.promotions, Promotion[v.promotion_id] )
			end
		end
	end
	
	t.get_promotion_ids = function( self )
		local promotion_ids = {}
		for k, v in ipairs( self.promotions ) do
			table.insert( promotion_ids, v.promotion_id )
		end
		return promotion_ids
	end
	
	return t
end
