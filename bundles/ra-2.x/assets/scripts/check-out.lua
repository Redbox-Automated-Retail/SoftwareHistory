--
-- check-out.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
check_email_address = false
if not ShoppingCart:check_out() then
	return
end

kernel.disableactor( "my_rentals_button" )

swipe_view_cancel_func = function()
	view_unwindto_safe( "shopping_cart_view", false, "check_out:swipe_view_cancel_func" )
end

swipe_view_next_func = function()
	ShoppingCart:add_multi_disc_discs()
	ShoppingCart:prepos_vend()
	if kernel.current_rental_shopping_cart_item_count() > 0 then
		if ShoppingCart:is_gift_card() == true then
			if GiftCardsEnabled == true then
				if credit_button_hit == true then
					key_pad_next_getcustomercredits_func()
				else
					ShoppingCart:set_use_credits( "OptedOut" )
					show_credits_popup = true
					gc_authorize_func()
				end
			else
				local message_view_manager = newmessageviewmanager()
				
				local auth_retry = function()
					message_view_manager:hide( true )
					kernel.executescript( "view_shopping_cart" )
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
                        kernel.executescript( "get_postal_code" )
		            end
	            )
            else
                ShoppingCart.prompt_required = false
                ShoppingCart.auth_days = 0
                kernel.sessionsetvariableauthvalues( ShoppingCart.session_id, false, 0, 0, true )
                kernel.executescript( "get_postal_code" )
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
	if ShoppingCart.email ~= nil
	and ShoppingCart.email ~= ""
    and kernel.ttsenabled() == false
	and EmailOptinCampaign ~= nil
    and ShoppingCart.play_pass_prompt == false then
		kernel.log( "ShowOptinForNormalCheckout:" ..tostring( ShowOptinForNormalCheckout )
			.. " ShowOptinForCreditsCheckout:" ..tostring( ShowOptinForCreditsCheckout )
			.. " ShoppingCart.session_use_credits:" .. kernel.get_rental_shopping_use_credits( ShoppingCart.session_id ))

		if ( ShowOptinForNormalCheckout and ShowOptinForCreditsCheckout )
		or ( ShowOptinForNormalCheckout and ( kernel.get_rental_shopping_use_credits( ShoppingCart.session_id ) == "OptedOut" or kernel.get_rental_shopping_use_credits( ShoppingCart.session_id ) == "NotSpecified" ))
		or ( ShowOptinForCreditsCheckout and kernel.get_rental_shopping_use_credits( ShoppingCart.session_id ) == "OptedIn" ) then
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
			optin_method_data = ShoppingCart.email,
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
	if kernel.ttsenabled() == false and ShoppingCart.alternate_optin and ShoppingCart.alternate_optin_not_shown and ShoppingCart.play_pass_prompt == false then
		-- Go to the the optional email opt in view
		ShoppingCart.alternate_optin_not_shown = false
		view_show_safe( "email_optin_view" )
	elseif ShoppingCart:contains_multi_disc() == true then
		view_show_safe( "multi_disc_vend_view" )
	else
		finish_vend()
	end
end

marketing_opt_in_flow = function( optin_status ) 
    if ShoppingCart.play_pass_prompt == true then
        view_show_safe( "play_pass_prompt_view" )
    else
        kernel.log("Entering marketing_opt_in_flow with status= "..tostring(optin_status) .. " and MarketingOptInEnabled: " .. tostring(MarketingOptInEnabled) )

	    if MarketingOptInEnabled and kernel.ttsenabled() == false and
	    (optin_status == "SubscribedNotConfirmed" -- 2
	    or optin_status == "NotSubscribedConfirmed" -- 3
	    or optin_status == "NotSubscribedNotConfirmed") then -- 4
		    if optin_status == "SubscribedNotConfirmed" then -- 2
			    do_marketing_confirm = true
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
end

function process_authorize_result( result )
    local message_view_manager = newmessageviewmanager()
	if result.properties.is_online == false
	    or result.properties.stand_alone == true 
    then
		result.success = true
		ShoppingCart:set_cart_type( "Offline" )
        local pending_counts = kernel.queuependingcounts()
        if pending_counts.game_rental_count + ShoppingCart:get_count( "Games", "rental" ) > OfflineGameRentalLimit then
			result.success = false
			result.properties.response = "OfflineGameRentalLimitExceeded"
        elseif pending_counts.game_purchase_count + ShoppingCart:get_count( "Games", "purchase" ) > OfflineGamePurchaseLimit then
			result.success = false
			result.properties.response = "OfflineGamePurchaseLimitExceeded"
        elseif pending_counts.total_count + kernel.current_rental_shopping_cart_item_count() > OfflineItemsLimit then
			result.success = false
			result.properties.response = "OfflineItemsLimitExceeded"
        end
		if  EnableOfflineBadCardList and result.success and not result.properties.stand_alone then
			if BadCardList:is_current_account_bad() then
				result.success = false
				result.properties.response = "Error"
			end
		end

        local shopping_cart_item_count = kernel.rental_shopping_cart_item_count( ShoppingCart.session_id )
        for k=1,shopping_cart_item_count do
            local v = kernel.get_rental_shopping_cart_item( ShoppingCart.session_id, k )
            if v ~= nil then
			    if MasterProductCatalog[v.product_id].sell_thru_new ~= nil and MasterProductCatalog[v.product_id].sell_thru_new == true then
				    result.success = false
				    result.properties.response = "OfflineSellThruNewRefused"
				    break
			    end
            end
		end
		if ShoppingCart:is_gift_card() == true then
			result.success = false
			result.properties.response = "OfflineGCDeclined"
		end
		ShoppingCart:set_blocklisted( ShoppingCart:is_card_blocklisted() )
		if ShoppingCart.blocklisted == true then
			result.success = false
			result.properties.response = "OfflineCardBlocklisted"
		end
	end
	ShoppingCart.marketing_opt_in = result.properties.confirmation_status or "DoNotShow"
	if result.success then
		if EnableOfflineBadCardList
		and result.properties.is_online
		and not result.properties.stand_alone then
			BadCardList:remove_current_account_if_bad()
		end
		message_view_manager:hide( true )
		kernel.stop_idle_timer( "idle_timer" )
		kernel.setmacro( "ComingSoonTitleIDs", "" )
		
		local keyboard_manager = newkeyboardmanager()

		local keyboard_continue_func = function()
			keyboard_manager:hide( true )
			kernel.analyticsaddelement( "session", "hasCheckout", "True", ShoppingCart.session_id )
			keyboard_manager:stop_idle_timer()
			
            -- If user has a profile and email - continue the vend process
			if ( (result.properties.has_profile == true) and (result.properties.email ~= nil) ) then
				ShoppingCart.email = result.properties.email
				-- show the email and change data screen
				view_show_safe( "email_view" )
                local ad = kernel.marketing_ad_get_screen_ad( "EmailNotification", true )
                if ad ~= nil and ad.visible == true then
                    kernel.showactor( "ad" )
                    kernel.setactorimage( "ad", ad.image_name )
                end
				set_email_view_text( result.properties.email )
			else
				marketing_opt_in_flow( ShoppingCart.marketing_opt_in )
			end
		end

		ShoppingCart.has_customer_profile = result.properties.has_profile

        ShoppingCart.play_pass_prompt = result.properties.play_pass_prompt
        ShoppingCart.play_pass_points_earned = result.properties.play_pass_points_earned

		-- If user has a profile and email - continue the vend process
		if ( (result.properties.has_profile == true) and (result.properties.email ~= nil) ) then
			-- show the email and change data screen
			keyboard_continue_func()
		else
			-- No profile or email - allow user to enter an e-mail
			keyboard_manager:init( 64 )
			if result.properties.email ~= nil then
				keyboard_manager:set_value( result.properties.email )
				check_email_address = true
			end
			keyboard_manager:set_message( strings.email_prompt_message )
			keyboard_manager:set_marketing( strings.email_marketing_message )
			keyboard_manager:set_enter_event(
				function()
					local email_changed = result.properties.email ~= keyboard_manager:get_value()
					ShoppingCart.email = keyboard_manager:get_value()
					keyboard_manager:stop_idle_timer()
					--Based on marketing opt-in determine flow
					kernel.log("Email changed: ".. tostring(email_changed))
					if MarketingOptInEnabled and kernel.ttsenabled() == false and email_changed then
						-- Show a 'Please Wait' screen while the lookup is working
						kernel.suspend_idle_timer( "idle_timer" )
						view_show_safe( "please_wait_message" )
						
						-- Perform a 2nd email lookup
						kernel.merchsvcs_emaillookup( 
							StoreManager:get_number(), 
							ShoppingCart.email,
							function( email_lookup_result )
								kernel.resume_idle_timer( "idle_timer" )
								marketing_opt_in_flow( email_lookup_result.properties.confirmation_status )
							end )
					else
						-- Do the normal flow
						keyboard_continue_func()
					end
				end )
			keyboard_manager:set_cancel_event(
				function()
					ShoppingCart.marketing_opt_in = "DoNotShow"
					keyboard_continue_func()
				end )
			keyboard_manager:set_actor_hit_event(
				function( text )
					if keyboard_manager:get_error() ~= "" then
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
			kernel.stop_idle_timer( "idle_timer" )

			keyboard_manager:set_cancel_button_text( strings.email_cancel_message )
			keyboard_manager:start_idle_timer(
				function()
					ShoppingCart.email = result.properties.email -- Defect #4036 don't clear email on a timeout
					if keyboard_manager.cancel_event ~= nil then
						keyboard_manager:raise_cancel_event()
					else
						view_unwindtobottom_safe( "check_out::keyboard_manager:idle_timer (on_expire:cancel_event = nil)" )
					end
				end,
				15000 )
		end -- result.has_profile && result.properties.email
	elseif result.properties.response == "OfflineCardBlocklisted" then
		ShoppingCart:prepos_vend_abort()
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
		if result.properties.response == "CheckIfCustomerWantsToUseCredits"
		and PromptForCredits
		and show_credits_popup
		and ( result.properties.has_profile ~= nil and result.properties.has_profile == true )
		and ( result.properties.total_credits_available ~= nil and tonumber( result.properties.total_credits_available ) > 0 )
		and ( result.properties.is_rbi == true )
		and ( result.properties.total_credits_applied ~= nil and tonumber( result.properties.total_credits_applied ) > 0 )
		then
			local credit_message_view_manager = newmessageviewmanager( "credit_message_view" )
			local show_credits_popup_timer = function()
				ShoppingCart:set_use_credits( "OptedOut" )
				credit_message_view_manager:hide( false ) -- this will re-show the "Authorizing Transaction..." message
				if GiftCardsEnabled == true and ShoppingCart:is_gift_card() == true then
					gc_authorize_func()
				else
					key_pad_next_authorize_func()
				end
			end
			
			show_credits_popup = false
			credit_message_view_manager:show()
			if result.properties.total_credits_available == 1 then
				credit_message_view_manager:set_message_header( strings.credits_remaining_singular_header )
				credit_message_view_manager:set_message( strings.credits_remaining_singular_body )
			else
				credit_message_view_manager:set_message_header( string.gsub( strings.credits_remaining_plural_header, "{0}", result.properties.total_credits_available ) )
				credit_message_view_manager:set_message( strings.credits_remaining_plural_body )
			end

			credit_message_view_manager:show_first_button(
				strings.pay_with_credits_no,
				function()
					view_analytics_event( "buttonPress", "prompt_for_credits_no_button" )
					--Remove credit_message_view from the stack once the user wants to pay with credit card
					credit_message_view_manager:hide( false )  -- this will re-show the "Authorizing Transaction..." message
					ShoppingCart:set_use_credits( "OptedOut" )
					if GiftCardsEnabled == true and ShoppingCart:is_gift_card() == true then
						gc_authorize_func()
					elseif key_pad_next_authorize_func ~= nil then
						key_pad_next_authorize_func()
					end
				end
			)
			credit_message_view_manager:show_second_button(
				strings.pay_with_credits_yes,
				function()
					view_analytics_event( "buttonPress", "prompt_for_credits_yes_button" )
					credit_message_view_manager:hide( false )  -- this will re-show the "Authorizing Transaction..." message
					ShoppingCart:set_use_credits( "OptedIn" )
					key_pad_next_getcustomercredits_func()
				end
			)
			credit_message_view_manager:start_timer( show_credits_popup_timer )
			view_analytics_event( "context", "promptForCredits" )
        else
			-- Mingle #2625
			ShoppingCart:prepos_vend_abort()

			if  EnableOfflineBadCardList
			and result.properties.is_online
			and not result.properties.stand_alone then
				if result.properties.response == "GameLimitExceeded"
				or result.properties.response == "RentalLimitExceeded"
				or result.properties.response == "Declined"
				or result.properties.response == "CustomerDisabled"
				or result.properties.response == "CreditCardError" then
					BadCardList:update_current_account()
				end
			end
			local auth_retry = function()
				message_view_manager:hide( true )
				kernel.executescript( "view_shopping_cart" )
			end
			message_view_manager:show()
			view_analytics_event( "context", "authorizationDecline" )

			message_view_manager:hide_first_button()
			message_view_manager:show_second_button( strings.continue_button, auth_retry )
			message_view_manager:start_timer( auth_retry )
			if result.properties.response == "OfflineGameRentalLimitExceeded" then
				view_analytics_event( "context", "offlineGameRentalLimitExceeded" )
                message_view_manager:set_message_header( strings.offline_game_rental_limit_exceeded_message_header )
                local game_or_games = strings.offline_game_limit_exceeded_message_game
                if OfflineGameRentalLimit > 1 then
                    game_or_games = strings.offline_game_limit_exceeded_message_games
                end
                message_view_manager:set_message( string.gsub( strings.offline_game_rental_limit_exceeded_message, "%S%S%S", { ["{0}"] = OfflineItemsLimit, ["{1}"] = OfflineGameRentalLimit, ["{2}"] = game_or_games } ) )
			elseif result.properties.response == "OfflineGamePurchaseLimitExceeded" then
				view_analytics_event( "context", "offlineGamePurchaseLimitExceeded" )
                message_view_manager:set_message_header( strings.offline_game_purchase_limit_exceeded_message_header )
                local game_or_games = strings.offline_game_limit_exceeded_message_game
                if OfflineGamePurchaseLimit > 1 then
                    game_or_games = strings.offline_game_limit_exceeded_message_games
                end
                message_view_manager:set_message( string.gsub( strings.offline_game_purchase_limit_exceeded_message, "%S%S%S", { ["{0}"] = OfflineItemsLimit, ["{1}"] = OfflineGamePurchaseLimit, ["{2}"] = game_or_games } ) )
			elseif result.properties.response == "OfflineItemsLimitExceeded" then
				view_analytics_event( "context", "offlineItemsLimitExceeded" )
                message_view_manager:set_message_header( strings.offline_items_limit_exceeded_message_header )
                message_view_manager:set_message( string.gsub( strings.offline_items_limit_exceeded_message, "%S%S%S", { ["{0}"] = OfflineItemsLimit } ) )
            elseif result.properties.response == "GameLimitExceeded" then
				if kernel.getculture() == "es-MX" then
					message_view_manager:set_message_header( strings.game_limit_exceeded_message_header )
				else
					message_view_manager:set_title( strings.game_limit_exceeded_message_header )
				end
				view_analytics_event( "context", "gameLimitExceededOnline" )
				message_view_manager:set_message_text( strings.game_limit_exceeded_message )
			elseif result.properties.response == "RentalLimitExceeded" then
				if kernel.getculture() == "es-MX" then
					message_view_manager:set_message_header( strings.rental_limit_exceeded_message_header )
				else
					message_view_manager:set_title( strings.rental_limit_exceeded_message_header )
				end
				view_analytics_event( "context", "rentalLimitExceededOnline" )
				message_view_manager:set_message_text( strings.rental_limit_exceeded_message )
			elseif result.properties.response == "AvsFailure" then
				view_analytics_event( "context", "cardNotAuthorized" )
				message_view_manager:hide_title()
				message_view_manager:set_message( strings.postal_code_mismatch_message )
			elseif result.properties.response == "CodeNotValid" then
				ShoppingCart:clear_promo_code()
				view_analytics_event( "context", "promoNotValid" )
				message_view_manager:set_message_header( strings.promo_code_not_valid_message_title )
				message_view_manager:set_message( strings.promo_code_not_valid_message )
			elseif result.properties.response == "CodeAlreadyUsed" then
                if rtwo_in_cart_template_is_running() then
                    ShoppingCart.in_cart_do_not_show = true
                    kernel.rental_shopping_cart_update_in_cart_offer_made( ShoppingCart.session_id, false)
                    kernel.rental_shopping_cart_update_in_cart_offer_cancel( ShoppingCart.session_id, false )
                end
                ShoppingCart:clear_promo_code()
				view_analytics_event( "context", "promoAlreadyUsed" )
				message_view_manager:set_message_header( strings.promo_code_already_used_message_title )
				message_view_manager:set_message( strings.promo_code_already_used_message )
			elseif result.properties.response == "CodeNotValidAtThisLocation" then
				ShoppingCart:clear_promo_code()
				view_analytics_event( "context", "promoInvalidLocation" )
				message_view_manager:set_title( strings.promo_code_error_title )
				message_view_manager:set_message( strings.promo_code_not_valid_location_message )
			elseif result.properties.response == "NewCustomerOnly" then
				ShoppingCart:clear_promo_code()
				if kernel.getculture() == "es-MX" then
					message_view_manager:set_title( strings.promo_code_transaction_error_title )
				else
					message_view_manager:set_title( strings.promo_code_error_title )
				end
				message_view_manager:set_message( strings.promo_code_new_customer_only_message )
			elseif result.properties.response == "OfflineSellThruNewRefused" then
				message_view_manager:hide_title()
				message_view_manager:set_message( strings.sell_thru_new_offline_denied )
			elseif result.properties.response == "OfflineGCDeclined" then
				view_analytics_event( "context", "redboxCardKioskOffline" )
				message_view_manager:set_message_header( strings.gc_offline_declined_title )
				message_view_manager:set_message( strings.gc_offline_declined )
			elseif result.properties.response == "GCInsufficientBalance" then
				view_analytics_event( "context", "redboxCardInsufficientFunds" )
				message_view_manager:set_message_header( strings.gc_insufficient_balance_title )
				local gc_insufficient_balance_text = string.gsub( strings.gc_insufficient_balance, "{0}", result.properties.gift_card_balance )
				message_view_manager:set_message( gc_insufficient_balance_text )
			elseif result.properties.response == "GCRentalLimitExceeded" then
				view_analytics_event( "context", "redboxCardRentalLimitExceeded" )
				message_view_manager:set_message_header( strings.gc_rental_limit_exceeded_title )
				local gc_rental_limit_exceeded_text = string.gsub( strings.gc_rental_limit_exceeded, "{0}", result.properties.gift_card_max_discs )
				message_view_manager:set_message( gc_rental_limit_exceeded_text )
			elseif result.properties.response == "GCDeclined" or result.properties.response == "Declined" then
				message_view_manager:set_message_header( strings.gc_declined_title )
				message_view_manager:set_message( strings.gc_declined )
            elseif result.properties.response == "EnforceAccertify" then
				view_analytics_event( "context", "accertifyRejectedGames" )
				message_view_manager:set_message_header( strings.accertify_enforce_accertify_header )
				message_view_manager:set_message( strings.accertify_enforce_accertify_message )
			else
				view_analytics_event( "context", "cardNotAuthorized" )
				message_view_manager:hide_title()
				message_view_manager:set_message( strings.authorization_failure_message )
			end
		end
	end
end

key_pad_next_getcustomercredits_func = function()
	--Remove credit_message_view from the stack once the user wants to pay with credits
	view_goback_safe( "check_out::remove credit message", true )

	local calculate_totals = function()
		local subtotal_by_group = ShoppingCart:sub_total()
		local subtotal_amount = 0
		local tax_amount = 0
		for _, v in pairs( subtotal_by_group ) do
			if v.discounted_amount ~= nil then
				subtotal_amount = subtotal_amount + v.discounted_amount
			else
				subtotal_amount = subtotal_amount + v.amount
			end
			tax_amount = tax_amount + v.tax_amount
		end
		return subtotal_amount, tax_amount
	end

	ShoppingCart:get_customer_credits(
		function( result )
			if result.success == true
			and result.properties.is_online == true
			and result.properties.has_profile == true
			and result.properties.stand_alone == false then
				view_show_safe( "credit_balance_view" )
				if result.properties.first_name == nil or result.properties.first_name == "" then
					kernel.setactortext( "welcome_message_name", strings.credit_balance_hi .. "," )
                    kernel.setmacro( "TTS:CreditViewWelcomeMessageName", strings.credit_balance_hi .. "," )
				else
					kernel.setactortext( "welcome_message_name", strings.credit_balance_hi .. " " .. result.properties.first_name .. "," )
                    kernel.setmacro( "TTS:CreditViewWelcomeMessageName", strings.credit_balance_hi .. " " .. result.properties.first_name .. "," )
				end

				local credits_used = tonumber( result.properties.total_credits_applied )
				local credits_available = tonumber( result.properties.total_credits_available )
				if credits_available == nil or credits_available == 0 then
					kernel.showactor( "welcome_message_no_credits" )
					kernel.hideactor( "welcome_message_using" )
					kernel.hideactor( "welcome_message_credits_used" )
					kernel.hideactor( "welcome_message_of" )
					kernel.hideactor( "welcome_message_credits_available" )
					kernel.hideactor( "welcome_message_credits" )
				else
					kernel.setactortext( "welcome_message_credits_used", credits_used )
                    kernel.setmacro( "TTS:CreditViewCreditsUsed", credits_used )
					kernel.setactortext( "welcome_message_credits_available", credits_available )
                    kernel.setmacro( "TTS:CreditViewCreditsAvailable", credits_available )
				end

				kernel.setactortext( "red_coin_credits_used", credits_used )

				local subtotal_amount, tax_amount = calculate_totals()
				local credits_amount = 0
				if result.properties.credit_allocations ~= nil then
					for _, v in pairs( result.properties.credit_allocations ) do
						credits_amount = credits_amount + v.amount
					end
				end
				local credited_tax_amount = kernel.round( credits_amount * ( StoreManager:get_rental_tax_rate() / 100 ), 16 )

				local total_amount = subtotal_amount - credits_amount + ( tax_amount - credited_tax_amount )

                local subtotal_text = "$" .. string.format( "%.2f", kernel.round( subtotal_amount, 2 ) )
				kernel.setactortext( "subtotal_amount", subtotal_text )
                kernel.setmacro( "TTS:CreditBalanceViewSubtotal", subtotal_text )

                local credit_amount_text = "-$" .. string.format( "%.2f", kernel.round( credits_amount, 2 ) )
				kernel.setactortext( "credits_amount", credit_amount_text )
                kernel.setmacro( "TTS:CreditBalanceViewCreditAmount", credit_amount_text )

                local tax_amount_text = "$" .. string.format( "%.2f", kernel.round( tax_amount - credited_tax_amount, 2 ) )
				kernel.setactortext( "tax_amount", tax_amount_text )
                kernel.setmacro( "TTS:CreditBalanceViewTaxAmount", tax_amount_text )

				if credits_amount == 0 then
					kernel.setactortext( "total_heading", "Total" )
				end

                local total_amount_text = "$" .. string.format( "%.2f", kernel.round( total_amount, 2 ) )
				kernel.setactortext( "total_amount", total_amount_text )
                kernel.setmacro( "TTS:CreditBalanceViewTotalAmount", total_amount_text )

				if StoreManager.store ~= nil and StoreManager.store.state ~= nil and ( StoreManager.store.state == "MD" or StoreManager.store.state == "SC" ) then
					kernel.hideactor( "tax_heading" )
					kernel.hideactor( "tax_amount" )
					kernel.moveactory( "grey_line", -30 )
					kernel.moveactory( "total_heading", -30 )
					kernel.moveactory( "total_amount", -30 )
				end

                local credits_left_text = string.gsub( strings.credit_balance_credits_left, "{0}", tostring( credits_available - credits_used ) )
				kernel.setactortext( "credits_left", credits_left_text )
                kernel.setmacro("TTS:CreditBalanceViewCreditsLeft", credits_left_text )

				if total_amount > 0 then
					local has_dvds = false
					local has_blu_rays = false
					local has_games = false
					local has_purchases = false
                    local shopping_cart_item_count = kernel.rental_shopping_cart_item_count( ShoppingCart.session_id )
                    for k=1,shopping_cart_item_count do
                        local v = kernel.get_rental_shopping_cart_item( ShoppingCart.session_id, k )
                        if v ~= nil then
						    if v.action == "purchase" then
							    has_purchases = true
						    else
							    if v.product_type == "DVD" then
								    has_dvds = true
							    end
							    if v.product_type == "Blu-ray" then
								    has_blu_rays = true
							    end
							    if v.product_family == "Games" then
								    has_games = true
							    end
						    end
                        end
					end
					local do_not_apply_text = strings.credit_balance_do_not_apply_1
					if has_dvds == true then
						do_not_apply_text = do_not_apply_text .. strings.credit_balance_do_not_apply_DVDs
					end
					if has_blu_rays == true then
						do_not_apply_text = do_not_apply_text .. strings.credit_balance_do_not_apply_Blurays
					end
					if has_games == true then
						do_not_apply_text = do_not_apply_text .. strings.credit_balance_do_not_apply_Games
					end
					if has_purchases == true then
						do_not_apply_text = do_not_apply_text .. strings.credit_balance_do_not_apply_Purchases
					end
					do_not_apply_text = string.sub( do_not_apply_text, 1, string.len( do_not_apply_text ) - 2 )
					do_not_apply_text = do_not_apply_text .. strings.credit_balance_do_not_apply_2
					
                    kernel.setactortext( "credits_do_not_apply_message", do_not_apply_text )
                    kernel.setmacro("TTS:CreditBalanceViewCreditsDoNotApply", do_not_apply_text )
				else
					kernel.hideactor( "credits_do_not_apply_message" )
				end
                if kernel.ttsenabled() then -- skipped checks elsewhere...
                    kernel.ttsrepeat();  -- assests have been set and toggled
                end
			elseif result.properties.response == "RentalLimitExceeded" then
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				view_analytics_event( "context", "rentalLimitExceededOnline" )
				message_view_manager:start_timer(
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::RentalLimitExceeded" )
					end,
					20000
				)

				if kernel.getculture() == "es-MX" then
					message_view_manager:set_message_header( strings.rental_limit_exceeded_message_header )
				else
					message_view_manager:set_title( strings.rental_limit_exceeded_message_header )
				end
				message_view_manager:set_message_text( strings.rental_limit_exceeded_message )

				message_view_manager:hide_first_button()
				message_view_manager:show_second_button(
					strings.continue_button,
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::RentalLimitExceeded:second_button" )
					end
				)
			elseif result.properties.response == "GCRentalLimitExceeded" then
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				view_analytics_event( "context", "gcRentalLimitExceededOnline" )
				message_view_manager:start_timer(
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::GCRentalLimitExceeded" )
					end,
					20000
				)
				
				message_view_manager:set_message_header( strings.gc_rental_limit_exceeded_title )
				local gc_rental_limit_exceeded_text = string.gsub( strings.gc_rental_limit_exceeded, "{0}", result.properties.gift_card_max_discs )
				message_view_manager:set_message( gc_rental_limit_exceeded_text )
				
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button(
					strings.continue_button,
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::GCRentalLimitExceeded:second_button" )
					end
				)
			elseif result.properties.response == "CustomerDisabled" then
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				message_view_manager:hide_first_button()
				message_view_manager:show_second_button( strings.continue_button,
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::CustomerDisabled:second_button" )
					end
				)
				message_view_manager:start_timer(
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::CustomerDisabled" )
					end
				)
				view_analytics_event( "context", "cardNotAuthorized" )
				message_view_manager:hide_title()
				message_view_manager:set_message( strings.authorization_failure_message )
			else
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				message_view_manager:start_timer(
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::credit_popup timeout" )
					end,
					20000
				)
				message_view_manager:set_title( strings.credit_popup_header )
				view_analytics_event( "context", "creditsNotFound" )
				if result.properties.has_profile == false then
					message_view_manager:set_message( strings.credit_popup_not_recognized )
				else
					message_view_manager:set_message( strings.credit_popup_offline )
				end

				message_view_manager:show_first_button( strings.credit_popup_back,
					function()
						ShoppingCart:prepos_vend_abort()
						view_unwindto_safe( "shopping_cart_view", false, "check_out::credit_popup:first_button" )
					end
				)
				message_view_manager:show_back_button_arrow( nil )
				local subtotal_amount, tax_amount = calculate_totals()
				message_view_manager:show_third_button(
					strings.credit_popup_pay_with_card .. " ($" .. string.format( "%.2f", kernel.round( subtotal_amount + tax_amount, 2 ) ) .. ")",
					function()
						--Remove credit_message_view from the stack when the credit card is not recognized or when kiosk is offline.
						view_goback_safe( "check_out::remove credits_error message", true )
						ShoppingCart:set_use_credits( "OptedOut" )
						if GiftCardsEnabled == true and ShoppingCart:is_gift_card() == true then
							gc_authorize_func()
						elseif key_pad_next_authorize_func ~= nil then
							key_pad_next_authorize_func()
						else
							ShoppingCart:prepos_vend_abort()
							view_unwindto_safe( "shopping_cart_view", false, "check_out::credit_popup:show_third_button( key_pad_next_authorize_func = nil )" )
						end
					end
				)
			end
		end
	)
end

-- setup the WPF swipe view
local swipe_view_type = "RegularCheckOut"
if (credit_button_hit) then
    swipe_view_type = "Credits"
end
kernel.set_swipe_view_parameters(swipe_view_cancel_func, swipe_view_next_func, swipe_view_type)

view_show_safe( "swipe_view" )
view_analytics_event( "context", "checkout" )