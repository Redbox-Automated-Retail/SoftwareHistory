--
-- credit-balance-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--




kernel.suspend_idle_timer( "idle_timer" )

local timeout = kernel.ttsgettimeout( "long", 40000 )
kernel.timercreate(
	"credit_balance_view_timer",
	timeout,
	timeout,
	function()
		ShoppingCart:prepos_vend_abort()
        ShoppingCart:set_use_credits( "OptedOut" )
        kernel.set_rental_shopping_applied_credits(ShoppingCart.session_id, "0")
		view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view_timer:timeout" )
	end
)
kernel.timerstart( "credit_balance_view_timer" )

kernel.registeractorhitnotify(
	"back_button",
	function()
		ShoppingCart:prepos_vend_abort()
        ShoppingCart:set_use_credits( "OptedOut" )
        kernel.set_rental_shopping_applied_credits(ShoppingCart.session_id, "0")
		view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view:back_button" )
	end
)

kernel.registeractorhitnotify(
	"confirm_button",
	function()
		--Remove credit_balance_view from the stack 
		view_goback_safe( "remove credit balance view", true )
		if GiftCardsEnabled == true and ShoppingCart:is_gift_card() == true then
			gc_authorize_func()			
		elseif key_pad_next_authorize_func ~= nil then
			key_pad_next_authorize_func()
		else
			ShoppingCart:prepos_vend_abort()
			view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view:confirm_button(key_pad_next_authorize_func = nil)" )
		end
	end
)

kernel.registeractorhitnotify(
	"disclaimer_button",
	function()
        view_analytics_event( "buttonPress", "play_pass_disclaimer_button" )
        kernel.setlasthitactorname( "play_pass_disclaimer_button" )
        ---view_show_safe( "help_document_view" )
		kernel.show_play_pass_disclaimer_view(true)
	end
)

kernel.registeractorhitnotify(
	"disclaimer_button_2",
	function()
        view_analytics_event( "buttonPress", "play_pass_disclaimer_button" )
        kernel.setlasthitactorname( "play_pass_disclaimer_button" )
        --view_show_safe( "help_document_view" )
		kernel.show_play_pass_disclaimer_view(true)
	end
)

local default_service_fee_cents = ShoppingCart:default_service_fee_cents()
if default_service_fee_cents > 0 then
	local service_fee_disclaimer_text = string.gsub( strings.service_fee_disclaimer, "{0}", default_service_fee_cents )
	kernel.setactortext( "service_fee_disclaimer", service_fee_disclaimer_text )
	kernel.setmacro( "TTS:CreditViewServiceFeeDisclaimerText", service_fee_disclaimer_text )
	kernel.showactor( "service_fee_disclaimer" )
end

if ShoppingCart:service_fee_enabled() == true then
	kernel.showactor( "service_fee_heading" )
    kernel.showactor( "service_fee_amount" )
	kernel.moveactory( "tax_heading", 37 )
    kernel.moveactory( "tax_amount", 37 )
    kernel.moveactory( "grey_line", 37 )
    kernel.moveactory( "total_heading", 37 )
    kernel.moveactory( "total_amount", 37 )
    kernel.moveactory( "credits_left", 37 )
end

local ad = kernel.marketing_ad_get_screen_ad( "Credits", true )
if ad ~= nil and ad.visible == true then
    kernel.showactor( "ad" )
    kernel.setactorimage( "ad", ad.image_name )
    kernel.moveactory( "welcome_message_name", 65 )
    kernel.moveactory( "welcome_message_using", 65 )
    kernel.moveactory( "welcome_message_credits_used", 65 )
    kernel.moveactory( "welcome_message_of", 65 )
    kernel.moveactory( "welcome_message_credits_available", 65 )
    kernel.moveactory( "welcome_message_credits", 65 )
    kernel.moveactory( "welcome_message_no_credits", 65 )
    kernel.moveactory( "red_coin_credits", 40 )
    kernel.moveactory( "red_coin_credits_used", 40 )
    kernel.moveactory( "credits_disclaimer", 40 )
    kernel.moveactory( "disclaimer_button", 40 )
    kernel.moveactory( "credits_disclaimer_2", 40 )
    kernel.moveactory( "disclaimer_button_2", 40 )
	kernel.moveactory( "service_fee_disclaimer", 40 )
    kernel.moveactory( "credits_do_not_apply_message", 40 )
    kernel.moveactory( "subtotal_heading", 40 )
    kernel.moveactory( "subtotal_amount", 40 )
    kernel.moveactory( "credits_background", 40 )
    kernel.moveactory( "credits_heading", 40 )
    kernel.moveactory( "credits_amount", 40 )
	kernel.moveactory( "service_fee_heading", 40 )
    kernel.moveactory( "service_fee_amount", 40 )
	kernel.moveactory( "tax_heading", 40 )
    kernel.moveactory( "tax_amount", 40 )
    kernel.moveactory( "grey_line", 40 )
    kernel.moveactory( "total_heading", 40 )
    kernel.moveactory( "total_amount", 40 )
    kernel.moveactory( "credits_left", 40 )
    kernel.moveactory( "back_button", 40 )
    kernel.moveactory( "back_button_arrow", 40 )
    kernel.moveactory( "confirm_button", 40 )
end