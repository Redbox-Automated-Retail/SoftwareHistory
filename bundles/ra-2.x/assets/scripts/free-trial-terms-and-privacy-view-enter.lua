--
-- free-trial-terms-and-privacy-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_terms_and_privacy_legal_text_back_button" )
		view_goback_safe("free_trial_terms_and_privacy", false)
	end 
)

if kernel.getlasthitactorname() == "Terms_conditions_button" or kernel.getlasthitactorname() == "terms_conditions_button" then
	free_trial_help_show_terms()
end

if help_current_table == free_trial_terms then
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 155, ["g"] = 155,["b"] = 155})
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("billing_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("billing_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorforegroundcolor( "terms_button", "black" )
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "billing_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
elseif help_current_table == free_trial_privacy then
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 155, ["g"] = 155,["b"] = 155})	
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("billing_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("billing_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorforegroundcolor( "privacy_button", "black" )
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "billing_button", "white" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
elseif help_current_table == free_trial_billing then
	kernel.setactorbackgroundcolor("billing_button", {["r"] = 155, ["g"] = 155,["b"] = 155})	
	kernel.setactorgradienttargetcolor("billing_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorforegroundcolor( "billing_button", "black" )
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
elseif help_current_table == free_trial_faq then
	help_current_table = free_trial_faq
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 155, ["g"] = 155,["b"] = 155})	
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})	
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorbackgroundcolor("billing_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("billing_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	kernel.setactorforegroundcolor( "faq_button", "black" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
	kernel.setactorforegroundcolor( "billing_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
end

local set_actors_x_position = function( increment )
	local x = kernel.getactorx( "faq_button" ) + kernel.getactorwidth( "faq_button" ) + 20
	kernel.setactorx( "billing_button", x )

	x = x + kernel.getactorwidth( "billing_button" ) + 20
	kernel.setactorx( "terms_button", x )	

	x = x + kernel.getactorwidth( "terms_button" ) + 20
	kernel.setactorx( "privacy_button", x )	
end

local title = kernel.getmacro("Help:Title")

if title == strings.faq_title then
	kernel.showactor("contact_redbox_label")
elseif title == strings.terms_of_use_title or title == strings.privacy_policy_title then 
	kernel.hideactor("contact_redbox_label")
end

free_trial_help_show_buttons()
free_trial_help_show_table()

-- For the Recomendations On Pickup - Do not allow access to FMA
if ShoppingCart.pickup_recommendations_shown == true then
	kernel.disableactor("redbox_logo")
end


