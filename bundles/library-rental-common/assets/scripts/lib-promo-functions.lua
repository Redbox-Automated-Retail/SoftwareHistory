--
-- lib-promo-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


local function CartCountByCriteria(
	cart, 
	action, 
	add_sequence, 
	product_type, 
	product_family, 
	product_id_list, 
	exclude_id_list,
	product_type_list)

	kernel.log( ">  CartCountBy a:" .. tostring(action) .. ", s:" .. tostring(add_sequence) .. 
		", pt:" .. tostring(product_type) .. ", pf:" .. tostring(product_family) .. 
		", incl:" .. tostring(product_id_list) .. ", excl:" .. tostring(exclude_id_list) ..
		", ptlst:" .. tostring(product_type_list) )


	local count = 0
	local counted_items = {}
	for k, v in ipairs( cart.items ) do
		kernel.log( ">  item " .. v.product_id .. " a:" .. v.action .. ", s:" .. v.add_sequence .. 
			", pt:" .. v.product_type .. ", pf:" .. v.product_family .. ", pid:" .. v.product_id )
		if ( action == nil or action == "*" or action == v.action)
		and ( add_sequence == nil or add_sequence == "*" or add_sequence == v.add_sequence )
		and ( product_type == nil or product_type == "*" or product_type == v.product_type )
		and ( product_family == nil or product_family == "*" or product_family == v.product_family )
		and ( product_id_list == nil or inlist( product_id_list, v.product_id, "," ) )
		and ( exclude_id_list == nil or inlist( exclude_id_list, v.product_id, "," ) == false )
		and ( product_type_list == nil or inlist( product_type_list, v.product_type, "," ) )
		and ( MasterProductCatalog[v.product_id] ~= nil and ( MasterProductCatalog[v.product_id].disc_number == nil or MasterProductCatalog[v.product_id].disc_number == 1 ) )
		then
			count = count + 1
			counted_items[ count ] = v
		end
	end
	kernel.log( ">  returns " .. tostring( count ) )
	return count, counted_items
end

local function BrowseCountByCriteria(
	product_type,
	product_family,
	product_type_list)
	
	kernel.log( ">  BrowseCountBy " .. "pt:" .. tostring(product_type) .. 
		", pf:" .. tostring(product_family) .. ", ptlst:" .. tostring(product_type_list) )

	--cache globals to locals for improved performance
	local _MasterProductCatalog = MasterProductCatalog
	local count = 0
	
	local _incart_include_titles, _incart_exclude_titles = GetInCartTitleLists( true )

	for k, v in ProductCatalog:pairs() do
		if ProductCatalog:in_stock( v.product_id ) == true
		and v.is_available == true
		and v.product_id ~= 1139
		and ( product_family == nil or product_family == v.product_family_name )
		and ( product_type == nil or product_type == v.product_type_name )
		and ( product_type_list == nil or inlist( product_type_list, v.product_type_name, "," ) )
		then
			local master_product = _MasterProductCatalog[ v.product_id ]
			local cart_count, as_rental = ShoppingCart:count_for_item( v.product_id ) 
			
			if ( master_product.do_not_rent_date == nil or master_product.do_not_rent_date > get_current_date_string( "%4d%02d%02d%02d%02d" ) )
			and ( cart_count == 0 or as_rental == false )
			and ( _incart_include_titles == nil or _incart_include_titles[ v.product_id ] ~= nil )
			and ( _incart_exclude_titles == nil or _incart_exclude_titles[ v.product_id ] == nil )
			and ( master_product.disc_number == nil or master_product.disc_number == 1 )
			then
				count = count + 1
			end
		end
	end

	kernel.log( ">  returns " .. tostring( count ) )
	return count
end


function CartQualifiesForInCartOffer( shopping_cart )
	kernel.log( "CartQualifiesForInCartOffer BEGIN" )
    if 	InCartPromoRules == nil then
		kernel.log( "InCartPromoRules == nil at CartQualifiesForInCartOffer" )
		return false
	end
    if free_in_cart_template_is_running() then
        return false
    end

	-- Having an existing Promo code on the cart prevents a customer from receiving an In-Cart promo offer
	local promo_code = kernel.rental_promotion_get_promotion()
	if promo_code ~= nil and promo_code.name ~= nil then
		kernel.log( "customer already entered a promo code, CartQualifiesForInCartOffer = false" )
		return false
	end

	local result = false
	local redemption_data = nil
	
	local condition_results = {}
	local condition_index = 0
	local ask_only_once = false

	for k, v in ipairs( InCartPromoRules ) do
		if v.offer_condition ~= nil then
			-- Get accurate cart counts according to satisfy_condition details
			local min_cart = true
			local max_cart = true
			local true_count = 0
			
			if v.offer_condition == "product" then
				true_count = CartCountByCriteria( 
					shopping_cart, 
					v.item_action, 
					v.add_sequence, 
					v.product_type, 
					v.product_family, 
					v.product_id_list, 
					v.exclude_id_list,
					v.product_type_list )
			end
			if v.offer_condition == "cart" then
				true_count = CartCountByCriteria( shopping_cart, v.item_action, v.add_sequence, nil, nil, nil, nil, nil )
			end
			if v.offer_condition == "browse_min_qty" then
				true_count = BrowseCountByCriteria( v.product_type, v.product_family, v.product_type_list )
			end
				
			-- Compare totals with defined min and max values to make the offer
			if v.min_qty ~= nil and v.min_qty ~= "" and true_count < tonumber( v.min_qty ) then
				if VerboseLogging == true then kernel.log( ">> " .. v.offer_condition .. ", min_qty=" ..tostring(v.min_qty) .. " true_count=" ..tostring( true_count )) end
				min_cart = false
			end
			kernel.log( "> min_qty = " .. tostring( min_cart ))
			if v.max_qty ~= nil and v.max_qty ~= "" and true_count > tonumber( v.max_qty ) then
				if VerboseLogging == true then kernel.log( ">> " .. v.offer_condition .. ", max_qty=" ..tostring(v.max_qty) .. " true_count=" ..tostring( true_count )) end
				max_cart = false
			end
			kernel.log( "> max_qty = " .. tostring( max_cart ))
			condition_index = condition_index + 1
			condition_results[condition_index] =  min_cart == true and max_cart == true
			if VerboseLogging == true then 
				kernel.log( ">  offer condition (" .. tostring(condition_index) ..") " .. v.offer_condition .. " returns " .. 
				tostring(min_cart == true and max_cart == true )) 
			end

			if v.ask_only_once  ~= nil and v.ask_only_once == "True" then
				ask_only_once = true
			end
		end

		-- Grab the redemption data for use later
		if v.redemption_rule ~= nil then
			redemption_data = v
		end
	end
		
	-- Assumption is that ALL defined contraint rules must be true; a single false kills the deal
	local condition_outcome = true
	for k, v in pairs( condition_results ) do
		if v == false then
			kernel.log( ">  failed condition " .. tostring( k ) )
			condition_outcome = false
		end
	end

	-- If the offer should only be made once, don't show it twice
	if shopping_cart.in_cart_promo_offer_made ~= nil 
	and shopping_cart.in_cart_promo_offer_made == true 
	and ask_only_once == true 
	then
		kernel.log( ">  offer already presented, returning False" )
		condition_outcome = false
	end
	
	-- If this is title marketing (include titles) and all titles are out of stock, don't make the offer
	if redemption_data ~= nil and redemption_data.product_id_list ~= nil then
		local in_stock_count = 0
		local title_list = string.split( redemption_data.product_id_list, "," )
		for _, v in ipairs( title_list ) do
			local history = ProductHistory[ v ]
			local cache = ProductCatalog[ v ]
			if ProductCatalog:in_stock( v, history ) == true 
			and cache.is_available then
				in_stock_count = in_stock_count + 1
			end
		end
		if in_stock_count == 0 then
			kernel.log( ">  no titles in the offer are in stock, returning False" )
			condition_outcome = false
		end
	end
	
	result = condition_outcome

	return result
end

function GetInCartPromoAction()
    if InCartPromoRules == nil then
		return nil
	end
    if free_in_cart_template_is_running() then
        return nil
    end

	for k, v in ipairs( InCartPromoRules ) do
		if v.offer_action ~= nil then
			return v
		end
	end
end

function GetInCartPromoCode()
	if InCartPromoRules == nil then
		return nil
	end
	for k, v in ipairs( InCartPromoRules ) do
		if v.redemption_code ~= nil then
			return v.redemption_code
		end
	end
end

function LoadInCartPromoMessaging()
    if kernel.ttsenabled() == true then
	    kernel.setmacro( "InCartPromoHeaderMessage", "" )
	    kernel.setmacro( "InCartPromoBodyMessage", "" )
	    kernel.setmacro( "InCartPromoYesButton", "" )
	    kernel.setmacro( "InCartPromoNoButton", "" )
    end

    if InCartPromoRules == nil then
		return
	end
	for k, v in ipairs( InCartPromoRules ) do
		if v.offer_language ~= nil 
		and v.offer_language == kernel.getculture() then

			local bodyMessageWithPrice = v.InCartPromoBodyMessage
			-- Update additional night price to what is actually running on this kiosk if the macro exists in the BodyMessage
			local extra_night = StoreManager.PricingManager:get_price_setting( "movies", "dvd", "extra_night", "rental" )
			if extra_night ~= nil then
				bodyMessageWithPrice = string.gsub( v.InCartPromoBodyMessage, "{0}", string.format( "%.2f", extra_night ))
			end
			
			kernel.setmacro( "InCartPromoHeaderMessage", v.InCartPromoHeaderMessage )
			kernel.setmacro( "InCartPromoBodyMessage", bodyMessageWithPrice )
			kernel.setmacro( "InCartPromoYesButton", v.InCartPromoYesButton )
			kernel.setmacro( "InCartPromoNoButton", v.InCartPromoNoButton )
			return
		end
	end
end

function CartHasAnyPostOfferItems( shopping_cart )
    if free_in_cart_template_is_running() then
        return false
    end
    return CartCountByCriteria( shopping_cart, nil, "post_offer" ) > 0
end

function CartHasProductTypeFamily( shopping_cart, product_type, product_family )
    if free_in_cart_template_is_running() then
        return false
    end
    return CartCountByCriteria( shopping_cart, nil, nil, product_type, product_family ) > 0
end

function CartSatisfiesInCartOffer( shopping_cart )
	kernel.log( "CartSatisfiesInCartOffer BEGIN" )
	if InCartPromoRules == nil then
		if VerboseLogging == true then kernel.log( "> In cart Promo is null" ) end
		return false, nil
	end
    if free_in_cart_template_is_running() then
        return false, nil
    end

	if shopping_cart.in_cart_promo_offer_made == nil or shopping_cart.in_cart_promo_offer_made == false then
		-- The offer does not apply if it was never shown to the customer
		if VerboseLogging == true then kernel.log( "> offer was never presented to cusotmer" ) end
		return false, nil
	end

	local redemption_data = nil
	local condition_results = {}
	local condition_index = 0

		for k, v in ipairs( InCartPromoRules ) do
			if v.satisfy_condition ~= nil then
				-- Get accurate cart counts according to satisfy_condition details
				local min_cart = true
				local max_cart = true
				local true_count = 0
				if v.satisfy_condition == "product" then
					true_count = CartCountByCriteria( 
						shopping_cart, 
						v.item_action, 
						v.add_sequence, 
						v.product_type, 
						v.product_family, 
						v.product_id_list, 
						v.exclude_id_list,
						v.product_type_list )
				end
				if v.satisfy_condition == "cart" then
					true_count = CartCountByCriteria( shopping_cart, v.item_action, v.add_sequence, nil, nil, nil, nil, nil )
				end
				
				-- Compare totals with defined min and max values to allow the discount
				if v.min_qty ~= nil and v.min_qty ~= "" and true_count < tonumber( v.min_qty ) then
					if VerboseLogging == true then kernel.log( ">> c:min_qty=" ..tostring(v.min_qty) .. " true_count=" ..tostring( true_count )) end
					min_cart = false
				end
				if v.max_qty ~= nil and v.max_qty ~= "" and true_count > tonumber( v.max_qty ) then
					if VerboseLogging == true then kernel.log( ">> c:max_qty=" ..tostring(v.max_qty) .. " true_count=" ..tostring( true_count )) end
					max_cart = false
				end
				condition_index = condition_index + 1
				condition_results[condition_index] =  min_cart == true and max_cart == true
				if VerboseLogging == true then
					kernel.log( ">  offer condition (" .. tostring(condition_index) ..") " .. v.satisfy_condition .. " returns " .. 
						tostring(min_cart == true and max_cart == true ))
				end
			end
			
			-- Grab the redemption data for use later
			if v.redemption_code ~= nil then
				redemption_data = v
			end
		end
		
	-- Assumption is that ALL defined contraint rules must be true; a single false kills the deal
	local condition_outcome = true
	for k, v in pairs( condition_results ) do
		if v == false then
			kernel.log( ">  failed condition " .. tostring( k ) )
			condition_outcome = false
		end
	end

	return condition_outcome, redemption_data
end


function UpdateInCartPromoValue( shopping_cart )
    if free_in_cart_template_is_running() then
        return
    end

	-- Satisfy conditions must be checked before moving on
	local discount_valid, redemption_data = CartSatisfiesInCartOffer( shopping_cart )
	local promo_code = kernel.rental_promotion_get_promotion()
	if discount_valid == false 
	and ( promo_code == nil or redemption_data == nil )
	then
		-- The discount was never taken, okay to leave
		return
	end

	if promo_code ~= nil then
		if promo_code.name ~= nil
		and promo_code.name ~= redemption_data.redemption_code then
			-- The user added a different program after viewing the offer, they are DONE
			return
		end
		if promo_code.name ~= nil
		and promo_code.name == redemption_data.redemption_code
		and discount_valid == false then
			-- The user no longer qualifies for the offer, remove the code and the discount
			shopping_cart:clear_promo_code()
			return
		end
	end


	if discount_valid == true then
		-- The user qualifies, add the discount (and re-calc now that we have qty-based discounts)
	
		local promo_price = nil
		-- There should only be ONE redemption rule defined in the active file
	
		-- Discount is one specified value (like initial_night) from the price set of the designated product type
		if redemption_data.redemption_rule == "product_type"
		and redemption_data.product_family ~= nil
		and redemption_data.product_type ~= nil
		then
			local price_object = StoreManager.PricingManager:get_product_type_price( redemption_data.product_family, redemption_data.product_type, nil )
			if price_object ~= nil then
				promo_price = price_object[ redemption_data.redemption_amount ]
			end
		end

		-- Discount is a fixed amount no matter what is in the cart once validation has passed
		if redemption_data.redemption_rule == "fixed_amount"
		and redemption_data.redemption_amount ~= nil 
		then
			promo_price = redemption_data.redemption_amount
		end

		-- Remaining offers need this data
		local qualified_count, qualified_items = CartCountByCriteria( 
			shopping_cart, 
			redemption_data.item_action, 
			redemption_data.add_sequence, 
			redemption_data.product_type, 
			redemption_data.product_family, 
			redemption_data.product_id_list,
			redemption_data.exclude_id_list,
			redemption_data.product_type_list )

		-- Discount is a fixed amount x the allowed quantity in the cart which were added post-offer
		if redemption_data.redemption_rule == "qty_based_amount"
		and redemption_data.redemption_amount ~= nil 
		then
			kernel.log( "qty_based_amount = " .. tostring( redemption_data.redemption_amount ) .. " *  " .. tostring( qualified_count ) )
			if qualified_count > tonumber( redemption_data.max_qty ) then
				kernel.log( "> user exceeded the max qty on a qty-based-amount in-cart, resetting discount to max allowed" )
				qualified_count = tonumber( redemption_data.max_qty )
			end
			promo_price = redemption_data.redemption_amount * qualified_count
		end

		-- Discount is for the initial_night value of qualifying cart items, possibly of mixed types / prices
		if redemption_data.redemption_rule == "post_offer_first_night" then
			local first_night_charges = {}
			local count = 0
			for _, v in ipairs( qualified_items ) do
				local price_object = StoreManager.PricingManager:get_product_type_price( v.product_family, v.product_type, nil )
				if price_object ~= nil and price_object.initial_night ~= nil then
					count = count + 1
					first_night_charges[ count ] = tonumber( price_object.initial_night )
				end
			end
			-- Order the charges from lowest cost to highest cost
			table.sort( first_night_charges, function( a, b ) return a < b end )
			local max_apply_number = tonumber( redemption_data.max_qty  )
			if qualified_count < max_apply_number then
				max_apply_number = qualified_count
			end
			-- Add the initial night prices to the discount total, up to the max allowed
			promo_price = 0
			local used_count = 1
			while used_count <= max_apply_number do
				promo_price = promo_price + first_night_charges[ used_count ]
				used_count = used_count + 1
			end
		end


		-- Once the amount is calculated, assign it to the cart object
		if promo_price ~= nil then
			local promo_code = {
				name = redemption_data.redemption_code,
				original_price = promo_price,
                price = promo_price,
				quantity = 1,
				intent = "FixedValue",
				original_percent = 0,
                percent = 0,
				rent_qty = 0,
				rent_format = nil,
				get_qty = 0,
				get_format = nil,
                campaign_titles = nil
			}
			kernel.rental_promotion_set_promotion( promo_code )
			kernel.sessionsetdiscount2(
				shopping_cart.session_id,
				promo_code.name,
				promo_code.price )
			
			kernel.log( "UpdateInCartPromoValue: A Promocode is being used, setting session_use_credits to OptedOut" )
			shopping_cart:set_use_credits( "OptedOut" )
		end
		
	end

end

function LoadInCartData()
	-- Fall through logic is by design if config-based in-cart data not created or deployed correctly
	
	local function add_multi_discs( product_ids_string )
		local product_ids = string.split( product_ids_string, "," )
		for k1, v1 in ipairs( product_ids ) do
			if is_multi_disc( v1 ) == true then
				local product_id_list = ProductGroup[MultiDiscXREF[v1].product_group_id].product_id_list
				for k2, v2 in pairs( product_id_list ) do
					if inlist( product_ids_string, v2, "," ) == false then
						product_ids_string = product_ids_string .. "," .. v2
					end
				end
			end
		end
		return product_ids_string
	end
	
	local iccsTable = kernel.gettable("in_cart_config_settings")
	local iccsVars = nil
	if iccsTable ~= nil then iccsVars = iccsTable[1] end
	
	if iccsVars ~= nil and iccsVars.template_name ~= nil and iccsVars.template_name ~= "DEFAULT NO OFFER" then
		kernel.log( "InCart promotion.config setting found: " .. iccsVars.template_name )
		local working_rule_set = kernel.gettable( iccsVars.template_name )		
		if working_rule_set ~= nil then
			kernel.log( "> InCart template loaded: " .. iccsVars.template_name)
			local variables_used = kernel.getresourceproperty( iccsVars.template_name, "variables_used" )
			if variables_used ~= nil then
				-- Check if all defined variables at the template level have corresponding config values defined for this kiosk
				local macro_list = {}
				local macro_count = 0 --Required because string indexed arrays do not react correctly to the # operator for counting
				local variable_list = string.split( variables_used, "," )
				for _, v in ipairs( variable_list ) do
					local macro_value = iccsVars[v]
					if macro_value ~= nil then
						macro_list[ v ] = macro_value
						kernel.log( "> loaded " .. v .. " = " .. macro_value )
						macro_count = macro_count + 1
					end
				end
				
				kernel.log( "> Variable count:" .. tostring(#variable_list) .. ", macro count:" .. tostring( macro_count ))
				if #variable_list == macro_count then
					-- Replace all variable placeholders with config specified values
					for k, v in pairs( macro_list ) do
						kernel.log( "> making replacments for: " .. k )
						
						if k == "INCLUDED_TITLES" or k == "EXCLUDED_TITLES" then
							v = add_multi_discs( v )
						end
						
						for _, rule_group in ipairs( working_rule_set ) do
							for data_name, data_value in pairs( rule_group ) do
								if string.find( data_value, "{{" .. k .. "}}" ) then
									local old_value = data_value
									data_value = string.gsub( data_value, "{{" .. k .. "}}", v )
									--kernel.log( ">  " .. data_name .. ": " .. old_value .. " --> " .. data_value )
									rule_group[ data_name ] = data_value -- force to save?
								end
							end
						end
					end

					-- All required data has been loaded and the rule set template has been filled
					InCartPromoRules = working_rule_set
					InCartTemplateName = iccsVars.template_name
					kernel.log( "> InCart template loaded successfully" )
					return
				else
					kernel.log( "> Not all template variables were defined in promotion.config, unable to use template" ) 
				end
			else
				kernel.log( "> InCart template variable names not defined in resource header" )
			end
		else
			kernel.log( "> InCart template resource file missing or unable to load" )
		end
	end
	
	-- If the code gets here there is no config in-cart set, or it is missing files or data values, use old file-based method only
	InCartPromoRules = kernel.gettable( "in_cart_promotion_v2" )
	local program_name = kernel.getresourceproperty( "in_cart_promotion_v2", "program_name" )
	InCartTemplateName = program_name
	kernel.log( "InCart loaded: " .. tostring( program_name ) )
end

function GetKioskOptinCampaign()
	local kiosk_campaigns = kernel.gettable( "opt_in_campaign" )
	return kiosk_campaigns[1]
end

function GetInCartTitleLists( ignore_offer_status )
	local _incart_include_titles = {}
	local _incart_exclude_titles = {}
    local _in_cart = GetInCartPromoAction()

	if ( ignore_offer_status == true 
	or ShoppingCart ~= nil
	and ShoppingCart.in_cart_promo_offer_made == true
	and ShoppingCart.in_cart_promo_offer_cancel == false )
	and _in_cart ~= nil then
		if _in_cart.product_id_list ~= nil 
		and #_in_cart.product_id_list > 0 then
			-- Build in-cart include ONLY these titles list
			local title_list = string.split( _in_cart.product_id_list, "," )
			for _, v in ipairs( title_list ) do
				if tonumber( v ) ~= nil then
					_incart_include_titles[ tonumber( v ) ] = true
				end
			end
		else
			_incart_include_titles = nil
		end
		if _in_cart.exclude_id_list ~= nil
		and #_in_cart.exclude_id_list > 0 then
			-- Build in-cart exclude these titles list
			local title_list = string.split( _in_cart.exclude_id_list, "," )
			for _, v in ipairs( title_list ) do
				if tonumber( v ) ~= nil then
					_incart_exclude_titles[ tonumber( v ) ] = true
				end
			end
		else
			_incart_exclude_titles = nil
		end
	else
		_incart_include_titles = nil
		_incart_exclude_titles = nil
	end
	return _incart_include_titles, _incart_exclude_titles
end

function free_in_cart_template_is_running()
    return InCartTemplateName ~= nil and ( InCartTemplateName == "FREE DVD I" or InCartTemplateName == "FREE BLU-RAY J" or InCartTemplateName == "FREE GAME K" )
end

function free_in_cart_template_get_value( key )
    for _, rule_group in ipairs( InCartPromoRules ) do
        for k, v in pairs( rule_group ) do
            if k == key then
                return v
            end
        end
    end
    return nil
end

function free_in_cart_template_is_cart_valid()
    local function is_excluded( exclude_ids, product_id )
        for _, id in ipairs( exclude_ids ) do
            if id == tostring( product_id ) then
                return true
            end
		end
        return false
    end
    local product_family = free_in_cart_template_get_value( "product_family" )
    local product_type = free_in_cart_template_get_value( "product_type" )
	local exclude_ids = string.split( free_in_cart_template_get_value( "exclude_id_list" ), "," )
    for k, v in ipairs( ShoppingCart.items ) do
        if v.action == "rental" and is_excluded( exclude_ids, v.product_id ) == false and v.product_family == product_family and ( v.product_type == product_type or product_type == "*" ) then
            return true
        end
    end
    return false
end

function free_in_cart_template_set_promo()
    local redemption_code = GetInCartPromoCode()
    local initial_night = StoreManager.PricingManager:get_price_setting( free_in_cart_template_get_value( "product_family" ), free_in_cart_template_get_value( "product_type" ), "initial_night", "rental" )
    if redemption_code ~= nil and initial_night ~= nil then
        local promo_code = {
	        name = redemption_code,
	        original_price = initial_night,
            price = initial_night,
	        quantity = 1,
	        intent = "FixedValue",
	        original_percent = 0,
            percent = 0,
	        rent_qty = 0,
	        rent_format = nil,
	        get_qty = 0,
	        get_format = nil,
            campaign_titles = nil
        }
        kernel.rental_promotion_set_promotion( promo_code )
        kernel.sessionsetdiscount2(
	        ShoppingCart.session_id,
	        promo_code.name,
	        promo_code.price
        )
    end
end