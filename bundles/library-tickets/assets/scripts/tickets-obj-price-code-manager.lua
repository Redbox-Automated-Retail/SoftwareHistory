--
-- tickets-obj-price-code-manager.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

DefaultNoPriceCode = { ["PriceCodeId"] = 0, ["Quantity"] = 0, ["PriceCodeName"] = "NO PRICE CODES", ["MinSupplierPricePerTicket"] = 0, ["MaxSupplierPricePerTicket"] = 0, ["PriceCodeShortNote"] = "All Ages" }

PriceCodeDisplayOrder = {
	["ADULT"] = 1,
	["CHILD"] = 2,
	["SENIOR"] = 3,
	["MILITARY"] = 4,
	["STUDENT"] = 5
}

DefaultInitialQuantity = 2

function newpricecodemanager()
	t = {}
	
	t.min_ticket_quantity = 0
	
	t.max_ticket_quantity = 1
	
	t.working_price_codes = {}
	
	t.select_price_codes = {}
	
	t.old_working_price_codes = {}
	
	t.set_working_price_codes = function( self, new_price_codes )
		self.working_price_codes = {}
		if new_price_codes == nil or #new_price_codes == 0 then
			self:add_working_price_code( DefaultNoPriceCode )
		else
			for k, v in pairs( new_price_codes ) do
				self:add_working_price_code( { ["PriceCodeId"] = v.PriceCodeId, ["Quantity"] = v.Quantity or 0, ["PriceCodeName"] = v.PriceCodeName, ["MinSupplierPricePerTicket"] = v.MinSupplierPricePerTicket, ["MaxSupplierPricePerTicket"] = v.MaxSupplierPricePerTicket, ["PriceCodeShortNote"] = v.PriceCodeShortNote } )
			end
			table.sort(
				self.working_price_codes,
				function( pc1, pc2 )
					local do_1 = PriceCodeDisplayOrder[string.upper( pc1.PriceCodeName )] or 999
					local do_2 = PriceCodeDisplayOrder[string.upper( pc2.PriceCodeName )] or 999
					return do_1 <= do_2
				end
			)
		end
		for i = 1, DefaultInitialQuantity do
			self:add_to_price_code_quantity( self.working_price_codes[1].PriceCodeId, 1 )
		end
	end
	
	t.add_working_price_code = function( self, new_price_code )
		table.insert( self.working_price_codes, { ["PriceCodeId"] = new_price_code.PriceCodeId, ["Quantity"] = new_price_code.Quantity or 0, ["PriceCodeName"] = new_price_code.PriceCodeName, ["MinSupplierPricePerTicket"] = new_price_code.MinSupplierPricePerTicket, ["MaxSupplierPricePerTicket"] = new_price_code.MaxSupplierPricePerTicket, ["PriceCodeShortNote"] = new_price_code.PriceCodeShortNote } )
	end
	
	t.clear_working_price_codes = function( self )
		self.working_price_codes = {}
	end
	
	t.load_working_price_codes_for_selected_production = function( self, selected_production )
		self.max_ticket_quantity = selected_production.ticket_limit
		local selected_production_price_codes = {}
		selected_production_price_codes = ProductionPriceCodes[selected_production.production_id].price_codes
		self:set_working_price_codes( selected_production_price_codes )
		if self:count() > 2 then
			choose_quantity_view_name = "tickets_choose_quantity_view_2"
		else
			choose_quantity_view_name = "tickets_choose_quantity_view"
		end
	end
	
	t.reset_working_price_codes = function( self )
		self:load_working_price_codes_for_selected_production( ShoppingCart.TicketsCart.production )
		self.old_working_price_codes = {}
	end
	
	t.get_total_tickets_quantity = function( self )
		local total_quantity = 0
		for _, pc in ipairs( self.working_price_codes ) do
			total_quantity = total_quantity + ( pc.Quantity or 0 )
		end
		return total_quantity
	end
	
	t.get_working_price_code_quantity = function(self, price_code_id )
		if self.working_price_codes == nil then
			return 0
		end
		for _, pc in ipairs( self.working_price_codes ) do
			if pc.PriceCodeId == price_code_id then
				return pc.Quantity
			end
		end
		kernel.log( "No Price Code Quantity found." )
		return 0
	end
	
	t.add_to_price_code_quantity = function( self, price_code_id, quantity_to_add, show_max_quantity_popup )
		for _, pc in ipairs( self.working_price_codes ) do
			if pc.PriceCodeId == price_code_id then
				local total_quantity = self:get_total_tickets_quantity()
				if quantity_to_add < 0 then
					if total_quantity > self.min_ticket_quantity and pc.Quantity > 0 then
						pc.Quantity = pc.Quantity + quantity_to_add
					end
				else
					if self.max_ticket_quantity == nil
					or total_quantity < self.max_ticket_quantity then
						pc.Quantity = pc.Quantity + quantity_to_add
					else
						if quantity_to_add > 0 and show_max_quantity_popup then
							local MessageView = newmessageviewmanager()
							MessageView:show()
							MessageView:set_title( tickets_strings.max_quantity_message_title )
							MessageView:set_message( tickets_strings.max_quantity_message )
							MessageView:show_second_button(
								"OK",
								function()
									MessageView:hide()
								end
							)
							MessageView:start_timer( function() MessageView:hide() end )
						end
					end
				end
			end
		end
	end
	
	t.save_working_price_codes = function( self )
		self.old_working_price_codes = {}
		for k, v in ipairs( self.working_price_codes ) do
			table.insert( self.old_working_price_codes, { ["PriceCodeId"] = v.PriceCodeId, ["Quantity"] = v.Quantity, ["PriceCodeName"] = v.PriceCodeName, ["MinSupplierPricePerTicket"] = v.MinSupplierPricePerTicket, ["MaxSupplierPricePerTicket"] = v.MaxSupplierPricePerTicket, ["PriceCodeShortNote"] = v.PriceCodeShortNote } )
		end
	end
	
	t.has_working_price_codes_quantity_changed = function( self )
		if self.old_working_price_codes == nil or #self.old_working_price_codes == 0 then return false end
		for k, v in ipairs( self.old_working_price_codes ) do
			if self.working_price_codes[k] == nil then
				return true
			end
			if VerboseLogging then kernel.log( string.format( "old_quantity(%s) ~= new_quantity(%s) == %s", v.Quantity, self.working_price_codes[k].Quantity, tostring( v.Quantity ~= self.working_price_codes[k].Quantity ) ) ) end
			if v.Quantity ~= self.working_price_codes[k].Quantity then
				return true
			end
		end
		return false
	end
	
	t.count = function( self )
		return table_count( self.working_price_codes )
	end
	
	t.select_valid_price_codes = function( self )
		self.select_price_codes = {}
		for k, v in pairs( self.working_price_codes ) do
			if v.Quantity ~= nil
			and v.Quantity > 0 then
				table.insert( self.select_price_codes, v )
			end
		end
		return self.select_price_codes
	end
	
	t.get_min_max_price_range_text = function( self, min, max )
		local min_max_price_range_text = ""
		if min ~= nil and min > 0 and max ~= nil and max > 0 and min ~= max then
			min_max_price_range_text = "$" .. tostring( math.floor( min ) ) .. " - $" .. tostring( math.ceil( max ) )
		elseif min ~= nil and min > 0 then
			min_max_price_range_text = "$" .. tostring( math.floor( min ) )
		end
		return min_max_price_range_text
	end
	
	return t
end