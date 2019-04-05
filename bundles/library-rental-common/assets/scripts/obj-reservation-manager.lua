--
-- obj-reservation-manager.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newreservationmanager()
	local t = {}

    t.vend_manager = newvendmanager()

	t.init = function( self )
		kernel.register_with_broker_service()
        kernel.initialize_expire_reservation_timer()
	end

	t.start_new = function(self)
        kernel.log("ReservationManager:start_new")
        self.vend_manager:init()
	end

	t.vend = function( self )
        kernel.log("ReservationManager:vend BEGIN")
		kernel.analyticsaddnode( "session", "onlinePickup", ShoppingCart.session_id )

        local barcodes_to_vend = kernel.get_reserved_barcodes_to_vend()
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
                kernel.preposition_vend_complete(vend_results)
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

    -- doesn't have to be converted yet since it doesn't really reference the Reservations table
	t.remove_orphaned_reservations = function( self )
		kernel.log("Remove Orphan Reservations BEGIN")
		local reserved_barcodes = {}
		
		-- Build list of all currently reserved barcodes for faster lookup
        local barcode_list = kernel.get_reserved_barcodes()

        if barcode_list ~= nil then
            for _, barcode in pairs(barcode_list) do
                reserved_barcodes[barcode] = true
                kernel.log( "> found true reserved barcode " .. tostring(barcode) )
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

	return t
end
