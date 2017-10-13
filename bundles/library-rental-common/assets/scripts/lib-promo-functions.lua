--
-- lib-promo-functions.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- note: CartCountByCriteria this was ported to .net but still exists for the Lua side for the short term....
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
    local shopping_cart_items = kernel.get_rental_shopping_cart_items( cart.session_id )
	for k, v in ipairs( shopping_cart_items ) do
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
    if free_in_cart_template_is_running() or rtwo_in_cart_template_is_running() then
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
	if kernel.rental_shopping_cart_get_in_cart_offer_made( shopping_cart.session_id ) ~= nil 
	and kernel.rental_shopping_cart_get_in_cart_offer_made( shopping_cart.session_id ) == true 
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
        if v.offer_language ~= nil and v.offer_language == kernel.getculture()
        and ( rtwo_in_cart_template_is_running() == false or ( rtwo_in_cart_template_is_running() and ( ( rtwo_in_cart_template_get_valid_item_count() == 1 and v.offer_message_version == "1" ) or ( rtwo_in_cart_template_get_valid_item_count() > 1 and v.offer_message_version == "2" ) ) ) ) then
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

function CartHasProductTypeFamily( shopping_cart, product_type, product_family )
    if free_in_cart_template_is_running() or rtwo_in_cart_template_is_running() then
        return false
    end
    return CartCountByCriteria( shopping_cart, nil, nil, product_type, product_family ) > 0
end

function LoadInCartData()
    kernel.rental_promotion_load_incart_data()

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
                    kernel.set_application_state_value("InCartTemplateName", iccsVars.template_name)
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
    kernel.set_application_state_value("InCartTemplateName", program_name)
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
	and kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == true
	and kernel.rental_shopping_cart_get_in_cart_offer_cancel( ShoppingCart.session_id ) == false )
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

local function get_value( key )
    for _, rule_group in ipairs( InCartPromoRules ) do
        for k, v in pairs( rule_group ) do
            if k == key then
                return v
            end
        end
    end
    return nil
end

local function is_excluded( exclude_ids, product_id )
    for _, id in ipairs( exclude_ids ) do
        if id == tostring( product_id ) then
            return true
        end
	end
    return false
end

function free_in_cart_template_is_running()
    return kernel.rental_promotion_free_in_cart_template_is_running()
end

function free_in_cart_template_is_cart_valid()
    return kernel.rental_promotion_is_free_in_cart_valid()
end

function free_in_cart_template_set_promo()
    local redemption_code = GetInCartPromoCode()
    local initial_night = StoreManager.PricingManager:get_price_setting( get_value( "product_family" ), get_value( "product_type" ), "initial_night", "rental" )
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

function free_in_cart_template_show_message()
    local promo_code = kernel.rental_promotion_get_promotion()
    return promo_code == nil and free_in_cart_template_is_running() and free_in_cart_template_is_cart_valid() and kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == false
end

function rtwo_in_cart_template_is_running()
    return kernel.rental_promotion_rent_two_in_cart_template_is_running()
end

function rtwo_in_cart_template_get_valid_item_count()
    return kernel.rental_promotion_rent_two_in_cart_item_count()
end

function rtwo_in_cart_template_set_promo()
    local redemption_code = GetInCartPromoCode()
    local redemption_amount = get_value( "redemption_amount" )
    if redemption_code ~= nil and redemption_amount ~= nil then
        local promo_code = {
	        name = redemption_code,
	        original_price = redemption_amount,
            price = redemption_amount,
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

function rtwo_in_cart_template_show_message()
    if rtwo_in_cart_template_is_running() == false then
        return false
    end
    local show_first_message = rtwo_in_cart_template_get_valid_item_count() == 1 and kernel.rental_shopping_cart_get_in_cart_offer_made( ShoppingCart.session_id ) == false
    local show_second_message = rtwo_in_cart_template_get_valid_item_count() > 1
    local promo_code = kernel.rental_promotion_get_promotion()
    return promo_code == nil and ShoppingCart.in_cart_do_not_show == false and ( show_first_message or show_second_message )
end