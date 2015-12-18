--
-- reco-on-pickup-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.log( "reco_on_pickup_view_enter - started" )

-- Allows newer views on the stack to know the reco_view is on the stack
ShoppingCart.pickup_recommendations_shown = true

-- Get the customer information for the kernel.resvsvcs_modresvpickup call
local reference_number = ""
local reservation = nil
local email_address = ""
local account_reservations = ReservationManager:get_account_reservations() 
for ref, res in pairs( account_reservations ) do
	reference_number = ref
	reservation = res
	email_address = res.email_address
end
ShoppingCart.reservation_card_hash_id = ReservationManager.card_hash_id

-- TODO-RECO '99999' is sent up for the postal code because the postal code from the reservation is not passed through
ShoppingCart.postal_code = '99999'

-- Get the initial reservation count
local reservation_barcodes = ReservationManager:get_barcodes_to_vend()
local reserved_count = #reservation_barcodes

-- Set the PricingManager to use the Reservation Pricing
-- This must be set before the reservation titles are added to the cart
StoreManager.PricingManager:update_default_price_set( reservation.prices )

-- TODO-RECO Tax at reservation - should this be passed down with reservation data?
--ShoppingCart:set_reservation_tax_rate( reservation.tax )

-- Ensure the reservations are in the cart
for k, v in ipairs( reservation_barcodes ) do
	local product = BarcodeToProduct[v]
	if product ~= nil
	and not ShoppingCart:has_item( product.product_id )
	and MasterProductCatalog[product.product_id] ~= nil then
		local catalog_entry = MasterProductCatalog[product.product_id]
		catalog_entry.barcode = v
		ShoppingCart:add_item( catalog_entry, 1, "rental", "Reservation" )	
	end
end

-- Generate a list of products to present for Recomendations
local reco_list = ShoppingCart.recommendations_generator.recommended_list

-- Set the title to be custom to game(s) or movies(s) if applicable
-- Do this dynamically based on games/movies and plurality
local cart_title = strings.pickup_discs  -- default to pickup_discs
local num_movies = 0
local num_games = 0
for k, v in ipairs( reservation_barcodes ) do
	local product = BarcodeToProduct[ v ]
	if product ~= nil then
		local catalog_product = MasterProductCatalog[product.product_id]
		if catalog_product then
			local product_type = ProductTypes[catalog_product.product_type_id]
			if product_type.product_family_id == 1 then
				num_movies = num_movies + 1
			end
			if product_type.product_family_id == 2 then
				num_games = num_games + 1
			end
		end
	end
end

if ( num_movies == 1 and reserved_count == 1 ) then
	cart_title = strings.pickup_movie
end
if ( num_movies == reserved_count and reserved_count > 1 ) then
	cart_title = strings.pickup_movies
end
if ( num_games == 1 and reserved_count == 1 ) then
	cart_title = strings.pickup_game
end
if ( num_games == reserved_count and reserved_count > 1 ) then
	cart_title = strings.pickup_games
end
kernel.setactortext( "label_cart_description", string.gsub( cart_title,"{0}", reserved_count ) )

-- Display the Reserved Items
local function display_cart_items()
	kernel.log( "reco_on_pickup_view_enter:display_cart_items()" )

	for k, v in ipairs( ShoppingCart.items ) do
		-- Hide the Add More Assets
		kernel.hideactor( "label_add_more_" .. k )
		kernel.hideactor( "cart_add_more_arrow_" .. k )
		
		-- Get the product object to display
		local catalog_entry = MasterProductCatalog[v.product_id]

		-- Show the Title Image
		draw_image( catalog_entry, "cart_image_" .. k )
		
		-- Show the Title Name for reserved titles and the cost/day for reco titles
		if k <= reserved_count then
			kernel.setactortext( "label_reserved_title_" .. k, strings.reserved_title )
		else
			local product_pricing = StoreManager.PricingManager:get_product_price( v.product_id )
			local cost_per_day = string.format( "%.2f", product_pricing.extra_night )
			local localized_cost_per_day = string.gsub( strings.totals_per_day,"{0}", cost_per_day )
			kernel.setactortext( "label_reserved_title_" .. k,  "("..localized_cost_per_day..")" )
			kernel.showactor( "label_reserved_title_" .. k )
		end
		
		-- Show the Blu-ray flag for blu-rays
		if catalog_entry.product_type_id == 2 then
			kernel.showactor( "cart_bluray_banner_" .. k )
		else
			kernel.hideactor( "cart_bluray_banner_" .. k )
		end
		
		-- Show the Remove Button for newly added titles
		if k > reserved_count then
			kernel.showactor( "cart_remove_button_" .. k )
		else
			kernel.hideactor( "cart_remove_button_" .. k )
		end
	end
	
	-- Display the empty slots
	for i = #ShoppingCart.items + 1, max_reco_cart_items do
		-- Setup the empty slots
		kernel.setactorimage( "cart_image_" .. i, "add_more_box" )
		kernel.setactortext( "label_add_more_" .. i, strings.add_more )
		kernel.showactor( "label_add_more_" .. i )
		kernel.showactor( "cart_add_more_arrow_" .. i )
		kernel.hideactor( "cart_bluray_banner_" .. i )
		kernel.hideactor( "cart_remove_button_" .. i )
		kernel.hideactor( "label_reserved_title_" .. i )
	end
end
-- Draw the initial cart items
display_cart_items()

-- Localize the rest of the text assets
kernel.setactortext( "label_reco_description", strings.add_one_of_these )
kernel.setactortext( "terms_conditions_button", strings.terms_privacy )
kernel.setactortext( "reserved_discs_label", strings.totals_reserved_discs )
kernel.setactortext( "added_discs_label", strings.totals_added_discs )
kernel.setactortext( "credit_label", strings.totals_credit )
kernel.setactortext( "tax_label", strings.totals_tax )
kernel.setactortext( "label_charged_once", strings.charged_once )
kernel.setactortext( "label_agree_to_terms", strings.by_pressing_pay )

-- Display the Recomended Titles
local function display_reco_items()
	kernel.log( "reco_on_pickup_view_enter:display_reco_items()" )
	for k, v in ipairs( reco_list ) do
		-- Show the Title Image
		if k <= max_reco_items then
			draw_image( v, "reco_image_" .. k )

			-- Fade the title if its in the cart
			if ShoppingCart:has_item( v.product_id ) then
				kernel.setactortext( "reco_in_cart_" .. k, strings.in_your )
				kernel.showactor( "reco_in_cart_" .. k )
				kernel.showactor( "reco_cart_image_" .. k )
				kernel.hideactor( "reco_add_button_" .. k )
				kernel.showactor( "reco_dark_overlay_" .. k )
				kernel.hideactor( "reco_bluray_banner_" .. k )
				kernel.disableactor( "reco_image_" .. k )
			else
				if #ShoppingCart.items < max_reco_cart_items then
					kernel.showactor( "reco_add_button_" .. k )
					kernel.setactorwidth( "reco_bluray_banner_" .. k, 65 )
				else
					kernel.hideactor( "reco_add_button_" .. k )
					kernel.setactorwidth( "reco_bluray_banner_" .. k, 83 )
				end
				kernel.hideactor( "reco_in_cart_" .. k )
				kernel.hideactor( "reco_cart_image_" .. k )
				kernel.hideactor( "reco_dark_overlay_" .. k )
				kernel.enableactor( "reco_image_" .. k )

				-- Show the Blu-ray flag for blu-rays
				if v.product_type_id == 2 then
					kernel.showactor( "reco_bluray_banner_" .. k )
				else
					kernel.hideactor( "reco_bluray_banner_" .. k )
				end
			end
		end
	end

	-- Center the Reco Items across the page
	local reco_spacing = 155	-- spacing between each title
	local spacing_buffer = 70	-- amount of the spacing that is whitespace buffer
	local length_needed = (#reco_list * reco_spacing) - spacing_buffer
	local x_start = (1024 - length_needed) / 2
	for k, v in ipairs( reco_list ) do
		kernel.setactorx( "reco_image_" .. k, x_start )
		kernel.setactorx( "reco_dark_overlay_" .. k, x_start )
		kernel.setactorx( "reco_in_cart_" .. k, x_start )
		kernel.setactorx( "reco_cart_image_" .. k, x_start + 26 )
		kernel.setactorx( "reco_bluray_banner_" .. k, x_start + 1 )
		kernel.setactorx( "reco_add_button_" .. k, x_start + 56 )
		x_start = x_start + reco_spacing
	end
	
	for i = #reco_list + 1, max_reco_items do
		-- Blank out any unused reco slots
		kernel.hideactor( "reco_image_" .. i )
		kernel.hideactor( "reco_dark_overlay_" .. i )
		kernel.hideactor( "reco_in_cart_" .. i )
		kernel.hideactor( "reco_cart_image_" .. i )
		kernel.hideactor( "reco_bluray_banner_" .. i )
		kernel.hideactor( "reco_add_button_" .. i )
	end
end
-- Draw the initial Recomended Titles
display_reco_items()

-- Display the Totals section
local function display_totals()
	kernel.log( "reco_on_pickup_view_enter:display_totals()" )

	if #ShoppingCart.items > reserved_count then
		local reserved_titles_subtotal = 0
		local reserved_discount_amount = 0
		local reserved_applied_credit_amount = 0
		local title_marketing_discount_amount = 0

		-- Locals to assist in moving the Total/Tax assets up as Tax/Credit lines are not needed
		local totals_y = 740
		local tax_y = 706
		local total_seperator_y = 731
		local y_offset = 0
		local y_offset_increment = -24

		-- Get the Subtotals from the Reservation
		for k, v in ipairs( reservation.reservation_titles ) do
			reserved_titles_subtotal = reserved_titles_subtotal + v.price
			reserved_discount_amount = reserved_discount_amount + (v.price - v.discounted_price)
			if reservation.applied_credit == true
			and v.number_of_credits > 0 then
				reserved_applied_credit_amount = reserved_applied_credit_amount + v.price
			elseif reservation.applied_title_marketing == true then
				title_marketing_discount_amount = title_marketing_discount_amount + -(v.discount)
			end
		end
		
		-- Apply any left over promo value to the ShoppingCart
		-- NOTE: this has to be done before ShoppingCart:generate_totals is called
		if reservation.applied_hive_online_promo == true
		or reservation.applied_title_marketing == true then
			local discount = title_marketing_discount_amount
			if reservation.applied_hive_online_promo == true then
				discount = -(reservation.hive_online_promo_discount)
			end
			kernel.log( "reco_on_pickup_view_enter:display_totals() - applied_hive_online_promo, reserved discount amount:".. discount )
			local promo_code = {
				name = reservation.promo_code,
				price = discount,
				quantity = 1,
				intent = "FixedValue",
				percent = 0,
				rent_qty = 0,
				rent_format = nil,
				get_qty = 0,
				get_format = nil
			}
			kernel.rental_promotion_set_promotion( promo_code )
			kernel.sessionsetdiscount2(
				ShoppingCart.session_id,
				reservation.promo_code,
				discount
			)
		elseif reservation.applied_credit == true then
			kernel.log( "reco_on_pickup_view_enter:display_totals() - applied_credit, reserved discount amount:".. reserved_discount_amount )
		else
			-- no Promo/Discount line so move the Tax and Totals up
			tax_y = tax_y + y_offset_increment
			totals_y = totals_y + y_offset_increment
			total_seperator_y = total_seperator_y + y_offset_increment
		end

		-- Get the Subtotals from the newly added items
		local cart_totals = ShoppingCart:generate_totals( reserved_applied_credit_amount )

		-- Calculate the totals keeping the reservation tax/totals/credits seperated from added titles
		local added_title_subtotal = cart_totals["sub_total"] + cart_totals["discount_used"] - reserved_titles_subtotal
		local total_total = cart_totals["total"]
		
		local applied_credit_amount = cart_totals["discount_used"]

		-- Set text on Pay Button
		kernel.setactortext( "pay_button", strings.pay .. " ("..string.format( "$%.2f", total_total )..")" )
		
		-- Customer has additial titles in the cart, so display the Totals
		kernel.setactortext( "reserved_discs_amount", string.format( "$%.2f", kernel.round(reserved_titles_subtotal,2) ) )
		kernel.setactortext( "added_discs_amount", string.format( "$%.2f", kernel.round(added_title_subtotal,2) ) )
		kernel.setactortext( "credit_amount", string.format( "-$%.2f", kernel.round(applied_credit_amount,2) ) )
		kernel.setactortext( "tax_amount", string.format( "$%.2f", kernel.round(cart_totals["tax_amount"],2) ) )
		kernel.setactortext( "total_amount", string.format( "$%.2f", kernel.round(total_total,2) ) )
		
		if reservation.applied_hive_online_promo == true
		or reservation.applied_title_marketing == true then
			kernel.setactortext( "credit_label", strings.totals_promo_code )
			kernel.showactor( "credit_label" )
			kernel.showactor( "credit_amount" )
		elseif reservation.applied_credit == true then
			kernel.setactortext( "credit_label", strings.totals_credit )
			kernel.showactor( "credit_label" )
			kernel.showactor( "credit_amount" )
		else 
			-- hide the Credit/Promo line
			kernel.hideactor( "credit_label" )
			kernel.hideactor( "credit_amount" )
		end
		
		kernel.showactor( "terms_conditions_border" )
		kernel.showactor( "terms_conditions_button" )
		kernel.showactor( "totals_border" )
		kernel.showactor( "reserved_discs_label" )
		kernel.showactor( "reserved_discs_amount" )
		kernel.showactor( "added_discs_label" )
		kernel.showactor( "added_discs_amount" )
		kernel.showactor( "total_line_seperator" )
		kernel.showactor( "total_label" )
		kernel.showactor( "total_amount" )
		kernel.showactor( "label_total_border" )
		kernel.showactor( "label_charged_once" )
		kernel.showactor( "label_agree_to_terms" )
		
		if StoreManager.store ~= nil and StoreManager.store.state ~= nil and ( StoreManager.store.state == "MD" or StoreManager.store.state == "SC" ) then
			totals_y = totals_y + y_offset_increment
			total_seperator_y = total_seperator_y + y_offset_increment

			-- hide the tax line
			kernel.hideactor( "tax_label" )
			kernel.hideactor( "tax_amount" )
		else
			kernel.showactor( "tax_label" )
			kernel.showactor( "tax_amount" )
		end
		
		-- Position the Totals & Tax based on if Promos/Credits/Tax (MD) should display
		kernel.setactory( "total_line_seperator", total_seperator_y )  -- originally 731
		kernel.setactory( "total_label", totals_y ) -- originally 740
		kernel.setactory( "total_amount", totals_y ) -- originally 740
		kernel.setactory( "tax_label", tax_y ) -- originally 740
		kernel.setactory( "tax_amount", tax_y ) -- originally 740
	else
		-- Set text on Pay Button
		kernel.setactortext( "pay_button", strings.pickup .. " ("..reserved_count..")" )

		-- Hide the Totals
		kernel.hideactor( "terms_conditions_border" )
		kernel.hideactor( "terms_conditions_button" )
		kernel.hideactor( "totals_border" )
		kernel.hideactor( "reserved_discs_label" )
		kernel.hideactor( "reserved_discs_amount" )
		kernel.hideactor( "added_discs_label" )
		kernel.hideactor( "added_discs_amount" )
		kernel.hideactor( "credit_label" )
		kernel.hideactor( "credit_amount" )
		kernel.hideactor( "tax_label" )
		kernel.hideactor( "tax_amount" )
		kernel.hideactor( "total_line_seperator" )
		kernel.hideactor( "total_label" )
		kernel.hideactor( "total_amount" )
		kernel.hideactor( "label_total_border" )
		kernel.hideactor( "label_charged_once" )
		kernel.hideactor( "label_agree_to_terms" )
	end
	
	-- Move the Pay button depending on if text need to be displayed below
	if #ShoppingCart.items > reserved_count then
		kernel.setactory( "pay_button", 648 )
	else
		kernel.setactory( "pay_button", 663 )
	end
	
end
-- Display the initial totals
display_totals()


local function add_reco_item( item ) 
	local cart_items_pre_click = #ShoppingCart.items

	-- Add to the cart if not already full
	if cart_items_pre_click < max_reco_cart_items then
		kernel.log( "reco_on_pickup_view_enter:add_reco_item - product_id:" .. item.product_id )
		view_analytics_event( "buttonPress", "reco-cart-add-item" )

		-- Add the Item to the ShoppingCart
		selected_product = item
		item_added_from_view = kernel.peekview()
		browse_selection_cart_action = "rental"
		check_for_warning_popups( check_alert_table, selected_product, nil )

		-- Refresh the Cart and Reco items
		if  cart_items_pre_click ~= #ShoppingCart.items then
			display_cart_items()
			display_reco_items()
			display_totals()
		end
	end
end

-- Register and handle the + button and the Title image on the Recomended titles
for i = 1, max_reco_items do
	kernel.registeractorhitnotify(
		"reco_add_button_" .. i,
		function()
			add_reco_item( reco_list[i] )
		end
	)
	kernel.registeractorhitnotify(
		"reco_image_" .. i,
		function()
			add_reco_item( reco_list[i] )
		end
	)
end

local function remove_reco_item( itemIndex )
	kernel.log( "reco_on_pickup_view_enter:remove_reco_item - index:" .. itemIndex )
	view_analytics_event( "buttonPress", "reco-cart-remove-item" )

	ShoppingCart:remove_item( itemIndex )
	
	-- Refresh the Cart and Reco items
	display_cart_items()
	display_reco_items()
	display_totals()
end

-- Register and handle the X button on the cart titles
for i = 1, max_reco_cart_items do
	if ( i > reserved_count ) then
		kernel.registeractorhitnotify(
			"cart_remove_button_" .. i,
			function()
				remove_reco_item( i )
			end
		)
		kernel.registeractorhitnotify(
			"cart_image_" .. i,
			function()
				remove_reco_item( i )
			end
		)
	end
end

local vend_only_reserved_titles = function()
	kernel.log( "reco_on_pickup_view_enter:vend_only_reserved_titles" )
	
	while reserved_count > 0 and #ShoppingCart.items > reserved_count do
		kernel.log( "reco_on_pickup_view_enter:removing " .. tostring( ShoppingCart.items[#ShoppingCart.items].product_id ) )
		ShoppingCart:remove_item( #ShoppingCart.items, false )
	end
	
	view_show_safe( "vend_view" )
	if quick_return_is_running == false then
		ShoppingCart.vend_manager:show_movie_or_coming_soon()
	end
	ShoppingCart:vend_reservation_cart( email_address, reference_number, false )
end

local vend_all_titles = function()
	kernel.log( "reco_on_pickup_view_enter:vend_all_titles" )
	view_show_safe( "vend_view" )
	if quick_return_is_running == false then
		ShoppingCart.vend_manager:show_movie_or_coming_soon()
	end
	ShoppingCart:vend_reservation_cart( email_address, reference_number, true )
end

local reco_pay_or_vend = function()
	-- Stop & remove the custom timer
	reco_idle_timer:stop()
	kernel.timerremove( "reco_idle_timer" )
	
	if ( #ShoppingCart.items > reserved_count ) then
		ShoppingCart:resv_authorize(
			reference_number,
			function( result )
				if result.success == true then
					vend_all_titles()
				else
					kernel.log( "reco_on_pickup_view_enter:reco_pay_or_vend - resv_authorize failed" )
					local message_view_manager = newtwolinemessageviewmanager()
					message_view_manager:show()
					if result.properties.response ~= "Approved" then
						view_analytics_event( "context", tostring( result.properties.response ) )
					end
					message_view_manager:set_message_header( strings.rop_auth_fail_header )
					message_view_manager:set_message_line_one( strings.rop_auth_fail_message_line_one )
					message_view_manager:set_message_line_two( strings.rop_auth_fail_message_line_two )
					message_view_manager:start_timer( vend_only_reserved_titles )
				end
			end
		)
	else
		vend_only_reserved_titles()
	end
end

-- Cature the hits to the Pay button so we can branch based on selections
kernel.registeractorhitnotify(
	"pay_button",
	function()
		if #ShoppingCart.items > reserved_count then 
			view_analytics_event( "buttonPress", "pay_button" )
		else
			view_analytics_event( "buttonPress", "pickup_button" )
		end
		reco_pay_or_vend()
	end
)

local reco_idle_expiration = function()
	-- Stop & remove the custom timer
	reco_idle_timer:stop()
	kernel.timerremove( "reco_idle_timer" )
	
	kernel.log( "reco_on_pickup_view_enter:reco_idle_timer - expired" )
	
	vend_only_reserved_titles()
end

local pickup_button = function ()
	view_analytics_event( "buttonPress", "pickup_button" )
	reco_idle_expiration()
end


-- Custom Reco-Idle-Timer
if not kernel.timerexists( "reco_idle_timer" ) then
	reco_idle_timer = newidletimer( "reco_idle_timer" )
	reco_idle_timer:initialize(
		40000,
		function()
			local message_view_manager = newmessageviewmanager( "reco_need_more_time_view" )
			message_view_manager:show()
			view_analytics_event( "context", "recoIdleTimer" )

			message_view_manager:hide_message()
			message_view_manager:set_message_text( strings.reco_need_more_time )
			kernel.setactortext( "second_button_reco_1",strings.keep_browsing_1 )
			kernel.setactortext( "second_button_reco_2",strings.keep_browsing_2 )
			message_view_manager:set_idle_timer_title( "" )
			message_view_manager:show_first_button( strings.pickup, pickup_button )
			message_view_manager:show_second_button(
				"",
				function()
					view_analytics_event( "buttonPress", "keep_browsing_button" )
					view_goback_safe( "need_more_time:second_button", false )
				end
			)
			kernel.showactor( "second_button" )
			kernel.showactor( "second_button_reco_1" )
			kernel.showactor( "second_button_reco_2" )
			message_view_manager:start_timer( reco_idle_expiration )
		end )
		
	-- Start the custom reco_idle_timer
	reco_idle_timer:start()
end

-- Stop the standard idle_timer
idle_timer:stop()
kernel.log( "reco_on_pickup_view_enter - completed" )
