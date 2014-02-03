--
-- shopping-cart-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()
shopping_cart_scene_manager = newscenemanager()
ShoppingCart:show_cart_table()

local current_filter_alpha_selected
credit_button_hit = false


if EnableCreditsButton 
and ShoppingCart.in_cart_promo_offer_made == true 
and	ShoppingCart.in_cart_promo_offer_cancel == false then
	kernel.hideactor( "credits_button" )
elseif EnableCreditsButton 
and HideCreditsButtonOnPromoUse 
and ShoppingCart.promo_code ~= nil then	
	kernel.hideactor( "credits_button" )
elseif EnableCreditsButton then
	kernel.showactor( "credits_button" )
	kernel.setactortext( "label_bottom", strings.credit_lable_bottom_text )
elseif ShoppingCart.hidden_digital_checkout_shown == true then
	kernel.showactor( "credits_button" )
else
	kernel.hideactor( "credits_button" )
end

if DisplayModifiedShoppingCart == true then
	local dvd_pricing_table = StoreManager.PricingManager:get_product_type_price( "movies", "dvd" )
	local bluray_pricing_table = StoreManager.PricingManager:get_product_type_price( "movies", "blu-ray" )
	local games_pricing_table = StoreManager.PricingManager:get_product_type_price( "games", "*" )
	kernel.setmacro( "ShoppingCart:DvdDays", dvd_pricing_table.non_return_days + 1 )
	kernel.setmacro( "ShoppingCart:BlurayDays", bluray_pricing_table.non_return_days + 1 )
	kernel.setmacro( "ShoppingCart:GamesDays", games_pricing_table.non_return_days + 1 )
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

if StoreManager.store ~= nil and StoreManager.store.state ~= nil and StoreManager.store.state == "MD" then
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
	if PromptForCredits ~= true then
		ShoppingCart:set_use_credits( "OptedOut" )
	else
		ShoppingCart:set_use_credits( "NotSpecified" )
	end
	if CartQualifiesForInCartOffer( ShoppingCart )
	and ShoppingCart.in_cart_promo_offer_made == false then
		view_show_safe( "in_cart_offer_message_view" )
	else
		kernel.executescript( "check_out" )
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
    	browse_filter_family_tracker.selected_family = selectedfamily
		browse_filter_family_tracker:set_family_type( "ALL" )
		browse_filter_genre_selected = genre
		browse_filter_alpha_selected = nil
		browse_selected_sort = "Newest"
		browse_filter_mode = "all"
		set_number = 1
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
		setcategory( browse_filter_family_tracker.selected_family, browse_filter_genre_selected )
	end
)

if EnableHiddenDigitalCheckout == true then
	kernel.registeractorhitnotify(
		"redbox_logo",
		function()
			if ShoppingCart.hidden_digital_checkout_shown == false then
				kernel.showactor( "credits_button" )
				if ShoppingCart:size() == 0 then
					kernel.disableactor( "credits_button" )
				end
				ShoppingCart.hidden_digital_checkout_shown = true
			end
		end
	)
end
