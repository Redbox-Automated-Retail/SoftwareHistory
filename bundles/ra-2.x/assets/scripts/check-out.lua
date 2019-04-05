--
-- check-out.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

result_holder_item = nil
check_email_address = false
if not ShoppingCart:check_out() then
	return
end

kernel.disableactor( "my_rentals_button" )

swipe_view_cancel_func = function()
	view_unwindto_safe( "shopping_cart_view", false, "check_out:swipe_view_cancel_func" )
end

swipe_view_next_func = function()
    kernel.rental_shopping_cart_add_multi_disc_discs( ShoppingCart.session_id )
	ShoppingCart:prepos_vend()
	if kernel.current_rental_shopping_cart_item_count() > 0 then
		if kernel.is_gift_card() == true then
			if GiftCardsEnabled == true then
				gc_authorize_func()
			else
				local message_view_manager = newmessageviewmanager()
				
				local auth_retry = function()
					message_view_manager:hide( true )
					kernel.view_shopping_cart()
				end
				
				message_view_manager:show()
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button( strings.continue_button, auth_retry )
				message_view_manager:start_timer( auth_retry )
				message_view_manager:set_message_header( strings.gc_not_available_title )
				message_view_manager:set_message( strings.gc_not_available )
			end
		else
            if ShowAuthChangeMessage == true then
	            ShoppingCart:preauth(
		            function( result )
                        ShoppingCart.prompt_required = result.properties.prompt_required
                        ShoppingCart.auth_days = result.properties.auth_days
                        kernel.sessionsetvariableauthvalues( ShoppingCart.session_id, result.properties.prompt_required, result.properties.auth_rule_id, result.properties.auth_amount, result.properties.skip_auth_rule )
                        view_show_safe("zip_code_view")
		            end
	            )
            else
                ShoppingCart.prompt_required = false
                ShoppingCart.auth_days = 0
                kernel.sessionsetvariableauthvalues( ShoppingCart.session_id, false, 0, 0, true )
                view_show_safe("zip_code_view")
            end
		end
	else
		view_unwindto_safe( "shopping_cart_view", false, "check_out:INVENTORY ERROR no barcodes, cart cleared" )
	end
end

key_pad_next_authorize_func = function()
	view_goback_safe( "called from key_pad_next_authorize_func", true )
	ShoppingCart:authorize(
		function( result )
			process_authorize_result( result )
		end
	)
end

gc_authorize_func = function()
	view_goback_safe( "called from gc_authorize_func", true )
	ShoppingCart:gc_authorize(
		function( result )
			process_authorize_result( result )
		end
	)
end

check_for_campaign_opt_in = function()
	if kernel.get_session_value("UserInfo.Email") ~= nil
	and kernel.get_session_value("UserInfo.Email") ~= ""
    and kernel.ttsenabled() == false
	and EmailOptinCampaign ~= nil then
		kernel.log( "ShowOptinForNormalCheckout:" ..tostring( ShowOptinForNormalCheckout ))

		if ( ShowOptinForNormalCheckout ) then
			kernel.log("check_for_campaign_opt_in - setting alternate_optin to true")
			ShoppingCart.alternate_optin = true
		end
	end
end

function set_kiosk_optin_values( ItemType, ItemId )
	--view_analytics_event( "buttonPress", "accept-optin-button" )
	--view_analytics_event( "context", "campaign:".. email_optin_campaign.campaign_id )
	
	local email_optin = {}
	table.insert( email_optin,
		{
			item_id = ItemId,
			item_type = ItemType,
			optin_method_data = kernel.get_session_value("UserInfo.Email"),
			optin_method_type = "EmailAddress",
			template_name = "MarketingOptInTemplate",
			campaign_name = "MarketingOptInCampaign"
		}
	)
	kernel.promosvcs_kioskoptin(
		StoreManager:get_number(),
		email_optin,
		nil
	)
end

function finish_vend()
	view_show_safe( "vend_view" )
	if quick_return_is_running == false then
		ShoppingCart.vend_manager:show_movie_or_coming_soon()
	end
	if in_maintenance_mode() then
		ShoppingCart:prepos_vend_abort()
	else
		ShoppingCart:prepos_vend_continue()
	end
end

continue_vend = function()
    check_for_campaign_opt_in()
	if kernel.ttsenabled() == false and ShoppingCart.alternate_optin and ShoppingCart.alternate_optin_not_shown then
		-- Go to the the optional email opt in view
		ShoppingCart.alternate_optin_not_shown = false
		view_show_safe( "email_optin_view" )
	else
		kernel.begin_after_swipe_loyalty_signup_flow(finish_vend)
    end
end

marketing_opt_in_flow = function( optin_status ) 
    kernel.log("Entering marketing_opt_in_flow with status= "..tostring(optin_status) .. " and MarketingOptInEnabled: " .. tostring(MarketingOptInEnabled) )

	if MarketingOptInEnabled and kernel.ttsenabled() == false and
	(optin_status == "SubscribedNotConfirmed" -- 2
	or optin_status == "NotSubscribedConfirmed" -- 3
	or optin_status == "NotSubscribedNotConfirmed") then -- 4
		if optin_status == "SubscribedNotConfirmed" then -- 2
            kernel.set_session_value("BrowseState.NeedMarketingConfirm", true)
		end
		view_show_safe( "confirm_email_view" )
	elseif MarketingOptInEnabled and kernel.ttsenabled() == false
	and ShoppingCart.has_customer_profile ~= true
	and optin_status == "NewCustomer" then -- 5
		set_kiosk_optin_values("MarketingSubscribe", "1")
		view_show_safe( "optin_thank_you_view" )
	else
	--	optin_status == "DoNotShow" -- 0
	--	optin_status == "SubscribedConfirmed" -- 1
		continue_vend()
	end
end

function ShowEmailReceiptIfNeeded()
	local result = result_holder_item
	kernel.analyticsaddelement( "session", "hasCheckout", "True", ShoppingCart.session_id )
    -- If user has a profile and email - continue the vend process
	if ( (result.properties.has_profile == true) and (result.properties.email ~= nil) ) then
		kernel.set_session_value("UserInfo.Email", result.properties.email)
		-- show the email and change data screen
		kernel.show_email_receipt_view(kernel.get_session_value("UserInfo.Email"))
	else
		marketing_opt_in_flow( kernel.get_rental_shopping_marketing_opt_in_confirmation_status(ShoppingCart.session_id) )
	end
end

function process_authorize_result( result )
	if result.properties.is_online == false
	    or result.properties.stand_alone == true 
    then
		result.success = true
        kernel.sessionsetcarttype2( ShoppingCart.session_id, "Offline" )

        offline_validation_result = kernel.check_offline_validations()
        if offline_validation_result ~= nil and offline_validation_result.success ~= nil and offline_validation_result.success == false then
			result.success = false
			result.properties.response = offline_validation_result.response
            kernel.log("processing offline validation results. Setting sucees = false; response = "..tostring(offline_validation_result.response))
        else
          kernel.log("passed offline validations")
        end
	end
	kernel.set_rental_shopping_marketing_opt_in_confirmation_status( ShoppingCart.session_id, result.properties.confirmation_status or "DoNotShow" )
    kernel.set_session_value("UserInfo.CustomerProfileNumber", result.properties.customer_profile_number)
    kernel.log("setting UserInfo.CustomerProfileNumber to " .. tostring(result.properties.customer_profile_number))
    kernel.set_session_value("UserInfo.AccountNumber", result.properties.account_number)
    kernel.log("setting UserInfo.AccountNumber to " .. tostring(result.properties.account_number))
	if result.success then
        process_authorize_success_result( result )
	elseif result.properties.response == "OfflineCardBlocklisted" then
		ShoppingCart:prepos_vend_abort()
	    local message_view_manager = newmessageviewmanager()
		view_analytics_event( "context", "blocklisted" )

		-- Partial Reconcile
		ShoppingCart:set_items_to_not_vended()
		ShoppingCart:set_session_cart_groups()
		kernel.merchsvcs_reconcile( ShoppingCart.session_id )
		
		local blocklisted_complete = function()
			message_view_manager:hide( true )
			view_unwindto_safe( "shopping_cart_view", false, "blocklisted complete" )
		end
		message_view_manager:hide_title()
		message_view_manager:show()
		message_view_manager:set_message( strings.authorization_failure_message )
		message_view_manager:hide_first_button()
		message_view_manager:show_second_button( strings.continue_button, blocklisted_complete )
		message_view_manager:start_timer( blocklisted_complete )
	else
		-- Mingle #2625
		ShoppingCart:prepos_vend_abort()
		kernel.handle_auth_error( result.properties, ShoppingCart.session_id)
	end
end

function process_authorize_success_result( result )
	if kernel.get_application_state_value("EnableOfflineBadCardList")
	and result.properties.is_online
	and not result.properties.stand_alone then
		kernel.remove_account_from_offline_bad_card_list()
	end

	kernel.stop_idle_timer( "idle_timer" )
	ShoppingCart.has_customer_profile = result.properties.has_profile

    kernel.show_multi_disc_vend_view_if_needed(
        function()
        	check_if_email_entry_is_needed(result)
        end
    )
end

function check_if_email_entry_is_needed( result )
	result_holder_item = result

	-- If user has a profile and email - continue the vend process
	if ( (result.properties.has_profile == true) and (result.properties.email ~= nil) ) then
		-- show the email and change data screen
		ShowEmailReceiptIfNeeded()
	else
		local continue_callback = function( emailAddress )
			local result = result_holder_item
			local email_changed = result.properties.email ~= emailAddress
            kernel.set_session_value("UserInfo.Email", emailAddress)
			--Based on marketing opt-in determine flow
			kernel.log("Email changed: ".. tostring(email_changed))
			if MarketingOptInEnabled and kernel.ttsenabled() == false and email_changed then
				-- Show a 'Please Wait' screen while the lookup is working
				kernel.suspend_idle_timer( "idle_timer" )
				view_show_safe( "please_wait_message" )
				-- Perform a 2nd email lookup
				local lookup_routine =  function( email_lookup_result )
					kernel.resume_idle_timer( "idle_timer" )
					marketing_opt_in_flow( email_lookup_result.properties.confirmation_status )
				end
				kernel.merchsvcs_emaillookup(StoreManager:get_number(), kernel.get_session_value("UserInfo.Email"), kernel.get_session_value("UserInfo.AccountNumber"), lookup_routine)
			else
				-- Do the normal flow
				ShowEmailReceiptIfNeeded()
			end
		end
		local cancel_callback =  function()
			kernel.set_rental_shopping_marketing_opt_in_confirmation_status( ShoppingCart.session_id , "DoNotShow" )
			ShowEmailReceiptIfNeeded()
		end
		kernel.show_check_out_keyboard_view(result.properties.email, continue_callback, cancel_callback)
	end -- result.has_profile && result.properties.emailend
end

kernel.register_marketing_opt_in_flow_callback(marketing_opt_in_flow)
kernel.register_continue_vend_callback(continue_vend)

-- setup the WPF swipe view
kernel.set_swipe_view_parameters(swipe_view_cancel_func, swipe_view_next_func, "RegularCheckOut")

view_show_safe( "swipe_view" )
view_analytics_event( "context", "checkout" )