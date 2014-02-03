--
-- unplayable-return.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.log( "Unplayable-Return BEGIN" )

-- Email is required
local keyboard_manager = newkeyboardmanager()
keyboard_manager:init( 64 )
keyboard_manager:set_message( strings.unplayable_email_prompt )
keyboard_manager:set_marketing( strings.unplayable_email_message )
show_email_confirmation = false

local keyboard_continue_func = function()
	keyboard_manager:hide( true )
	local email_address = ShoppingCart.email
	kernel.log( "> Unplayable-Return email accepted " .. email_address )

	-- Okay to proceed with the return
	local return_manager = newreturnmanager( ShoppingCart )
	return_manager:init()
	return_manager.show_success_message = function( self, item_id, return_type )
		kernel.log( "> Unplayable-Return received barcode: " .. item_id )
		kernel.setmacro( "ProductType", "Item" )
		CloseAnalyticsOnStartView = false

		local mark_locally = true
		if BarcodeToProduct[item_id] ~= nil then
			local product_id = BarcodeToProduct[item_id].product_id
			if  MasterProductCatalog[product_id] ~= nil
			and MasterProductCatalog[product_id].product_type_id ~= nil
			and MasterProductCatalog[product_id].product_type_id == 2 then
				kernel.log( tostring( product_id ) .. " is a Blu-ray and will NOT be marked unplayable." )
				mark_locally = false
			end
		end

		local current_date_string = get_current_date_string( "%4d%02d%02d%02d%02d" )
		--Make server call to report the barcode
		kernel.markasunplayable(
			StoreManager:get_number(),
			item_id,
			email_address,
			current_date_string,
			function( result )
				if mark_locally == true
				and result.success == true
				and result.properties.is_unplayable == true
				then
					--Insert new row into the UnplayableProduct table
					local row = {}
					row.returned_time = current_date_string
					row.email = email_address
					UnplayableProduct[ item_id ] = row
					kernel.log( tostring( item_id ) .. " has been marked as unplayable." )
				else
					kernel.log( tostring( item_id ) .. " has not been marked as unplayable." )
				end
			end )

		local message_view_manager = newmessageviewmanager()
		message_view_manager:show()
		view_analytics_event( "context", "unplayableSuccess" )
		message_view_manager:set_message_header( strings.unplayable_return_acceptedA )
		message_view_manager:set_message( strings.unplayable_return_acceptedB )
		message_view_manager:show_second_button(
			strings.done_button,
			function()
				message_view_manager:hide()
				ABE_ON = false
				view_unwindtobottom_safe( "unplayableSuccess second_button clicked" )
			end
		)
		message_view_manager:start_timer(
			function()
				ABE_ON = false
				view_unwindtobottom_safe( "unplayableSuccess timer expired" )
			end
		)

		return_manager.did_show_success_message = true
	end

	return_manager:begin_return( "unplayable_return" )
end

keyboard_manager:set_enter_event(
	function()
		ShoppingCart.email = keyboard_manager:get_value()
		keyboard_continue_func()
	end )

keyboard_manager:set_actor_hit_event(
	function( text )
		if keyboard_manager:get_error( ) ~= "" then
			keyboard_manager:set_error( "" )
		end
	end )

keyboard_manager:set_validate_event(
	function( text )
		EmailValidator = newemailvalidator()
		if EmailValidator.validate_email( text ) == false then
			return strings.email_error_message
		end
		return nil
	end )

keyboard_manager:show( strings.email_tag )
view_analytics_event( "context", "emailAddress" )

keyboard_manager:set_cancel_button_text( strings.cancel_button )
keyboard_manager:start_idle_timer(
	function()
		ABE_ON = false
		view_unwindtobottom_safe( "Unplayable Return: email Address entry timeout" )
	end,
	15000 )


