--
-- obj-recommendations-generator.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newrecommendationsgenerator()
	-- Local references
	local reservation = nil
	local reservationBarcodes = nil
	local customerHistory = nil

	local function reservationContainsBluRay()
		local _MasterProductCatalog = MasterProductCatalog
		if reservationBarcodes ~= nil then
			for k, v in ipairs(reservationBarcodes) do
				local product = BarcodeToProduct[ v ]
				if product ~= nil
				and _MasterProductCatalog[product.product_id] ~= nil
				and _MasterProductCatalog[product.product_id].product_type_id == 2 then
					return true
				end
			end
		end
		return false
	end
	
	local function inReservationList( productId )
		for k, v in ipairs(reservationBarcodes) do
			local product = BarcodeToProduct[ v ]
			if product ~= nil and product.product_id == productId then
				return true	
			end
		end
		return false
	end

	local function inRecentCustomerHistory ( productId )
		if ( customerHistory ~= nil ) then
			for k, v in ipairs( customerHistory ) do
				if v == productId then
					return true
				end
			end
		end
		
		return false
	end

	local function productInTable(list, product_id)
		for k, v in ipairs( list ) do
			if v.product_id == product_id then
				return true
			end
		end
		return false
	end

	local t = {}

	-- public attributes
	t.recommended_list = {}

	-- Generate Recomendations for 'Recomendations On Pickup - Top 20 version'
	t.GenerateRecomendationsTop20 = function()
		kernel.log( "obj_recommendations_generator:GenerateRecomendationsTop20: generating recommendation list based on top 20" )
		
		local _ProductCatalog = ProductCatalog
		local _MasterProductCatalog = MasterProductCatalog

		-- Get the one and only reservation object
		for ref, res in pairs( ReservationManager:get_account_reservations( true ) ) do
			reservation = res
		end

		reservationBarcodes = ReservationManager:get_barcodes_to_vend()
		customerHistory = reservation.history_title_ids
		local containsBluray = reservationContainsBluRay()
		
		-- Generate a list based on filtered ProductCatalog
		local recoTempList = {}
		for k,v in _ProductCatalog:pairs() do
			local product = _MasterProductCatalog[v.product_id]
			if #recoTempList >= 20 then -- restrict to top 20...
				break
			else
				-- If the item is in the ShoppingCart and not available - display it - its reserved for this customer!
				if product ~= nil
				and k ~= "1139"
				and v.is_available == true
				and v.in_stock == true
				and ( product.genres ~= nil and product.genres[1093] ~= nil ) -- top 20! 
				and not inReservationList ( v.product_id )
				and not inRecentCustomerHistory( v.product_id )
				and not productInTable(recoTempList, v.product_id)
				and ( product.product_type_id == 1 or product.product_type_id == 2 ) -- only Blu-Rays and DVDs
				and product.sell_thru_new == false
				and is_dnr_disc( product ) == false
				then
					if containsBluray == false and product.product_type_id == 2 then
						-- skip blu-ray as there is none in the reservation list
					else
						table.insert( recoTempList, product )

					end
				end
			end
		end

		-- Sort selected Blu-rays first
		if ( #recoTempList > 1 ) then
			table.sort( recoTempList, function( t1, t2 )
				if t1.product_type_id == 2 and t2.product_type_id ~= 2 then 
					return true
				else
					return false
				end
			end )
		end
		
		-- Restrict list to specified max
		t.recommended_list = {}
		for k, v in ipairs( recoTempList ) do
			if #t.recommended_list <  max_reco_items then
				table.insert(t.recommended_list, v)
				kernel.log( "obj_recommendations_generator:GenerateRecomendationsTop20: added recommended title id:" .. v.product_id )
			end
		end

		if #recoTempList > #t.recommended_list then
			kernel.log( "obj_recommendations_generator:GenerateRecomendationsTop20: narrowed list from:" .. #recoTempList .. "   to:" .. #t.recommended_list )
		end
		
		return #t.recommended_list
	end	

	return t
	
end