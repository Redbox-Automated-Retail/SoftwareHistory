--
-- shopping-cart-view-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.set_session_value("BrowseState.CollectionsPopupShown", true)

initialize_idle_timer()
idle_timer:start()

ShoppingCart:remove_multi_disc_discs()

shopping_cart_scene_manager = newscenemanager()
ShoppingCart:show_cart_table()

local current_filter_alpha_selected
credit_button_hit = false

local promo_code = kernel.rental_promotion_get_promotion()
if EnableCreditsButton 
and ShoppingCart.in_cart_promo_offer_made == true 
and	ShoppingCart.in_cart_promo_offer_cancel == false then
	kernel.hideactor( "credits_button" )
elseif EnableCreditsButton 
and HideCreditsButtonOnPromoUse 
and promo_code ~= nil then	
	kernel.hideactor( "credits_button" )
elseif EnableCreditsButton then
	kernel.showactor( "credits_button" )
	kernel.setactortext( "label_bottom", strings.credit_lable_bottom_text )
else
	kernel.hideactor( "credits_button" )
end

if DisplayModifiedShoppingCart == true then
    kernel.setactortext( "passive_sale_note", strings.passive_sale_note )
	if kernel.ttsenabled() == true then
		kernel.setmacro( "TTS:passive_sale_note", strings.passive_sale_note )
	end
	kernel.showactor( "passive_sale_note" )
	
	kernel.setactortextalignx( "promo_code_amount", "far" )
	kernel.moveactorx( "promo_code_amount", -10 )
	kernel.setactortextalignx( "sub_total_amount", "far" )
	kernel.moveactorx( "sub_total_amount", -10 )
	kernel.setactortextalignx( "tax_amount", "far" )
	kernel.moveactorx( "tax_amount", -10 )
	kernel.setactortextalignx( "total_amount", "far" )
	kernel.moveactorx( "total_amount", -10 )
end

if StoreManager.store ~= nil and StoreManager.store.state ~= nil and ( StoreManager.store.state == "MD" or StoreManager.store.state == "SC" ) then
	kernel.hideactor( "label_sub_total" )
	kernel.hideactor( "sub_total_amount" )
	kernel.hideactor( "label_tax" )
	kernel.hideactor( "tax_amount" )
	kernel.moveactory( "promo_code_label", 25 )
	kernel.moveactory( "promo_code_amount", 25 )
	kernel.moveactory( "label_total", -30 )
	kernel.moveactory( "total_amount", -30 )
end

function show_offer_or_proceed_checkout()
	local promo_code = kernel.rental_promotion_get_promotion()
	if PromptForCredits ~= true
	or promo_code ~= nil then
		kernel.log( "Setting session_use_credits to OptedOut" )
		ShoppingCart:set_use_credits( "OptedOut" )
	else
		kernel.log( "PromptForCredits is true, setting session_use_credits to NotSpecified" )
		ShoppingCart:set_use_credits( "NotSpecified" )
	end

	if promo_code ~= nil and promo_code.intent == "RxGx" then
		ShoppingCart:validate_cart_update_promo_code(
			function( cart_promo_info )
				kernel.executescript( "check_out" )
			end,
			function( cart_promo_info )
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				message_view_manager:set_message_header( "Oops" )
				if kernel.getculture() == "es-MX" then
					message_view_manager:set_message( cart_promo_info.title_spanish .. "\n\n" .. cart_promo_info.message_spanish )
				else
					message_view_manager:set_message( cart_promo_info.title_english .. "\n\n" .. cart_promo_info.message_english )
				end
				message_view_manager:hide_first_button()
				message_view_manager:show_third_button( strings.use_promo_button,
					function()
						view_analytics_event( "buttonPress", "keepPromo" )
						message_view_manager:hide()
                        kernel.set_session_value( "BrowseState.BrowseSelectedSort", "Newest" )
                        kernel.set_session_value( "BrowseState.BrowseFilterMode", "All" )
                        kernel.set_session_value("BrowseState.BrowseGenre", nil)
                        kernel.set_session_value("BrowseState.BrowseTitleType", "All")
		                kernel.set_session_value("BrowseState.BrowseTitleFamily", "Movies")
                        kernel.set_session_value("BrowseState.PageNumber", 1)
                        kernel.executescript( "browse_products" )
					end
				)
				message_view_manager:show_fourth_button( strings.do_not_use_promo_button,
					function()
						view_analytics_event( "buttonPress", "removePromo" )
						message_view_manager:hide()
						ShoppingCart:clear_promo_code()
						
                        if free_in_cart_template_is_running() == false
                        and rtwo_in_cart_template_is_running() == false
                        and CartQualifiesForInCartOffer( ShoppingCart )
						and ShoppingCart.in_cart_promo_offer_made == false then
							view_show_safe( "in_cart_offer_message_view" )
                        else
							kernel.executescript( "check_out" )
						end
					end
				)
				message_view_manager:start_timer(
					function()
						message_view_manager:hide()
					end
				)
				view_analytics_event( "context", "rxgxWarning" )
			end
		)
	elseif promo_code ~= nil and promo_code.campaign_titles ~= nil then
		ShoppingCart:validate_cart_update_promo_code(
			function()
				kernel.executescript( "check_out" )
			end,
			function()
				local message_view_manager = newmessageviewmanager()
				message_view_manager:show()
				message_view_manager:set_message_header( strings.tsp_invalid_header )
				message_view_manager:set_message( strings.tsp_invalid_message )
				message_view_manager:hide_first_button()
				message_view_manager:show_third_button( strings.use_promo_button,
					function()
						view_analytics_event( "buttonPress", "keepPromo" )
						message_view_manager:hide()
                        kernel.set_session_value( "BrowseState.BrowseSelectedSort", "Newest" )
                        kernel.set_session_value( "BrowseState.BrowseFilterMode", "All" )
                        kernel.set_session_value("BrowseState.BrowseGenre", nil)
                        kernel.set_session_value("BrowseState.BrowseTitleType", "All")
                        kernel.set_session_value("BrowseState.BrowseTitleFamily", "Movies")
                        kernel.set_session_value("BrowseState.PageNumber", 1)
                        kernel.executescript( "browse_products" )
					end
				)
				message_view_manager:show_fourth_button( strings.do_not_use_promo_button,
					function()
						view_analytics_event( "buttonPress", "removePromo" )
						message_view_manager:hide()
                        ShoppingCart:clear_promo_code()
                        
                        if free_in_cart_template_is_running() == false
                        and rtwo_in_cart_template_is_running() == false
                        and CartQualifiesForInCartOffer( ShoppingCart )
						and ShoppingCart.in_cart_promo_offer_made == false then
							view_show_safe( "in_cart_offer_message_view" )
                        else
							kernel.executescript( "check_out" )
						end
					end
				)
				message_view_manager:start_timer(
					function()
						message_view_manager:hide()
					end
				)
				view_analytics_event( "context", "titlelevelpromoWarning" )
			end
		)
    else
        if free_in_cart_template_is_running() == false
        and rtwo_in_cart_template_is_running() == false
		and CartQualifiesForInCartOffer( ShoppingCart )
		and ShoppingCart.in_cart_promo_offer_made == false then
			view_show_safe( "in_cart_offer_message_view" )
        else
			kernel.executescript( "check_out" )
		end
	end
end

function show_credits_or_proceed_checkout()
	credit_button_hit = true
	ShoppingCart:set_use_credits( "OptedIn" )
	kernel.executescript( "check_out" )
end

kernel.registeractorhitnotify(
	"pay_button",
	function()
		show_offer_or_proceed_checkout()
	end
)

kernel.registeractorhitnotify(
	"credits_button",
	function()
		view_analytics_event( "buttonPress", "credits_button" )
		show_credits_or_proceed_checkout()
	end
)

local function setcategory(selectedfamily,genre)
        kernel.set_session_value("BrowseState.BrowseTitleFamily", selectedfamily)
        kernel.set_session_value("BrowseState.BrowseTitleType", "All")
        kernel.set_session_value("BrowseState.BrowseGenre", genre)
		kernel.set_session_value("BrowseState.BrowseSelectedSort", "Newest")
        kernel.set_session_value("BrowseState.BrowseFilterMode", "All")
        kernel.set_session_value("BrowseState.PageNumber", 1)
		kernel.executescript( "browse_products" )
end

kernel.registeractorhitnotify(
	"games_button",
	function() 
		ShoppingCart.in_cart_title_marketing = false
		setcategory( "Games", nil )
	end )

kernel.registeractorhitnotify(
	"all_movies_button",
	function() 
		ShoppingCart.in_cart_title_marketing = false
		setcategory( "Movies", ALL ) 
	end )

kernel.registeractorhitnotify(
	"back_button",
	function()
		setcategory( kernel.get_session_value("BrowseState.BrowseTitleFamily"), kernel.get_session_value("BrowseState.BrowseGenre") )
	end
)