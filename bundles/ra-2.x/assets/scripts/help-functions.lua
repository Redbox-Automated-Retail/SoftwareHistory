--
-- help-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local _showselectedbutton = function( name )
	kernel.setmacro( "HelpButtonText", kernel.getactortext( name .. "_button" ))
	kernel.setactorx( "selected_button", kernel.getactorx( name .. "_button" ) )
	kernel.setactorwidth( "selected_button", kernel.getactorwidth( name .. "_button" ) )
	kernel.showactor( "selected_button" )
	kernel.hideactor( name .. "_button" )
end

function help_show_faq()
	kernel.setmacro( "Help:Title", strings.faq_title )
	kernel.showactor("contact_redbox_label")
	
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 155, ["g"] = 155,["b"] = 155})
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_redbox_card_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_redbox_card_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorforegroundcolor( "faq_button", "black" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
	kernel.setactorforegroundcolor( "terms_redbox_card_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
	
	help_current_table = faq
	help_image_page = 1
end

function help_show_terms()
	kernel.setmacro( "Help:Title", strings.terms_of_use_title )
	kernel.hideactor("contact_redbox_label")
	
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 155, ["g"] = 155,["b"] = 155})	
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	
	kernel.setactorbackgroundcolor("terms_redbox_card_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_redbox_card_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "terms_button", "black" )
	kernel.setactorforegroundcolor( "terms_redbox_card_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
	
	help_current_table = terms
	help_image_page = 1
end

function help_show_terms_redbox_card()
	kernel.setmacro( "Help:Title", strings.terms_of_use_title )
	kernel.hideactor("contact_redbox_label")
	
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_redbox_card_button", {["r"] = 155, ["g"] = 155,["b"] = 155})
	kernel.setactorgradienttargetcolor("terms_redbox_card_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
	kernel.setactorforegroundcolor( "terms_redbox_card_button", "black" )
	kernel.setactorforegroundcolor( "privacy_button", "white" )
	
	help_current_table = terms_redbox_card
	help_image_page = 1
end

function help_show_privacy()
	kernel.setmacro( "Help:Title", strings.privacy_policy_title )
	kernel.hideactor("contact_redbox_label")
	
	kernel.setactorbackgroundcolor("faq_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("faq_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("terms_redbox_card_button", {["r"] = 111, ["g"] = 111,["b"] = 111})
	kernel.setactorgradienttargetcolor("terms_redbox_card_button", {["r"] = 70, ["g"] = 70,["b"] = 70})
	
	kernel.setactorbackgroundcolor("privacy_button", {["r"] = 155, ["g"] = 155,["b"] = 155})	
	kernel.setactorgradienttargetcolor("privacy_button", {["r"] = 219, ["g"] = 219,["b"] = 219})
	
	kernel.setactorforegroundcolor( "faq_button", "white" )
	kernel.setactorforegroundcolor( "terms_button", "white" )
	kernel.setactorforegroundcolor( "terms_redbox_card_button", "white" )
	kernel.setactorforegroundcolor( "privacy_button", "black" )
	
	help_current_table = privacy
	help_image_page = 1
end

function help_show_buttons()
	if help_current_table == faq then
		_showselectedbutton("faq")
		kernel.showactor( "terms_button" )
		kernel.showactor( "terms_redbox_card_button" )
		kernel.showactor( "privacy_button" )
	elseif help_current_table == terms then
		_showselectedbutton("terms")
		kernel.showactor( "faq_button" )
		kernel.showactor( "terms_redbox_card_button" )
		kernel.showactor( "privacy_button" )
	elseif help_current_table == terms_redbox_card then
		_showselectedbutton("terms_redbox_card")
		kernel.showactor( "faq_button" )
		kernel.showactor( "terms_button" )
		kernel.showactor( "privacy_button" )
	elseif help_current_table == privacy then
		_showselectedbutton("privacy")
		kernel.showactor( "faq_button" )
		kernel.showactor( "terms_button" )
		kernel.showactor( "terms_redbox_card_button" )
	end
	
	if GiftCardsEnabled == false then
		kernel.hideactor( "terms_redbox_card_button" )
	end
end

function help_show_table()
	if help_image_page == 1 then
		kernel.hideactor( "left_arrow" )
	else
		kernel.showactor( "left_arrow" )
	end
	
	if help_image_page == #help_current_table then
		kernel.hideactor( "right_arrow" )
	else
		kernel.showactor( "right_arrow" )
	end
	local counter = string.gsub( strings.help_page_counter, "{0}", tostring( help_image_page ) )
	counter = string.gsub( counter, "{1}", tostring( #help_current_table ) )
	kernel.setactortext( "page_counter", counter )
	local column = help_current_table[help_image_page]
	local path = column["path"]
	kernel.setactorimage( "image_container", path )
end
