--
-- help-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if ShoppingCart.pickup_recommendations_shown == false then
	idle_timer:start()
end

if ProfileVersion ~= nil
and ProfileVersion.origin_machine ~= nil
and ProfileVersion.generated_on ~= nil then
    kernel.setmacro(
        "ProfileVersion",
        string.format( "%s %s", tostring( ProfileVersion.origin_machine.name ), tostring( ProfileVersion.generated_on.date ) ) )
else
    kernel.setmacro( "ProfileVersion", "N/A" )
end

if InventoryVersion ~= nil
and InventoryVersion.origin_machine ~= nil
and InventoryVersion.generated_on ~= nil then
    kernel.setmacro(
        "InventoryVersion",
        string.format( "%s %s", tostring( InventoryVersion.origin_machine.name ), tostring( InventoryVersion.generated_on.date ) ) )
else
    kernel.setmacro( "InventoryVersion", "N/A" )
end

if kernel.getlasthitactorname() == "Terms_conditions_button" or kernel.getlasthitactorname() == "terms_conditions_button" then
	help_show_terms()
end

local set_actors_x_position = function( increment )
	local x_faq = kernel.getactorx( "faq_button" )
	local x_terms = kernel.getactorx( "terms_button" )
	local x_terms_redbox_card = kernel.getactorx( "terms_redbox_card_button" )
	local x_privacy = kernel.getactorx( "privacy_button" )
	local x_unplayable = kernel.getactorx( "unplayable_return_button" )
	kernel.setactorx( "faq_button", x_faq - 100 + increment )
	kernel.setactorx( "terms_button", x_terms - 100 + increment )
	kernel.setactorx( "terms_redbox_card_button", x_terms_redbox_card - 100 + increment )
	if GiftCardsEnabled == false then
		if kernel.getculture() == "en-US" then
			x_privacy = x_privacy - 180
		else
			x_privacy = x_privacy - 153
		end
	end
	kernel.setactorx( "privacy_button", x_privacy - 100 + increment )
	kernel.setactorx( "unplayable_return_button", x_unplayable - 100 + increment )
end

if EnableUnplayableReturn == false then
	set_actors_x_position(50)
	kernel.hideactor( "unplayable_return_button" )
else
	set_actors_x_position(0)
	kernel.registeractorhitnotify( 
		"unplayable_return_button", 
		function()
			kernel.executescript("unplayable_confirm")
		end	)
end

local title = kernel.getmacro("Help:Title")
if title == strings.faq_title then
	kernel.showactor("contact_redbox_label")
elseif title == strings.terms_of_use_title or title == strings.privacy_policy_title then 
	kernel.hideactor("contact_redbox_label")
end

help_show_buttons()
help_show_table()

-- For the Recomendations On Pickup - Do not allow access to FMA
if ShoppingCart.pickup_recommendations_shown == true then
	kernel.disableactor("redbox_logo")
end
