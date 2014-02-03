--
-- free-trial-terms-and-privacy-agree-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local agree = kernel.getmacro("JVHardTake:Terms:Agree") or "unchecked"
local consent = kernel.getmacro("JVHardTake:Terms:Consent") or "unchecked"
local timeoutOccurred = false
		
local function onTimeout()
	timeoutOccurred = true
end

kernel.disableactor( "next_button" )
kernel.showactor("next_button_right_arrow_icon")
kernel.hideactor("image_next_white_arrow_big")

local function update_next_button_state()
	if kernel.getactortag( "agree_checkbox" ) == "checked" and kernel.getactortag( "consent_checkbox" ) == "checked" then
		kernel.enableactor( "next_button" )
		kernel.hideactor("next_button_right_arrow_icon")
		kernel.showactor("image_next_white_arrow_big")
	else
		kernel.disableactor( "next_button" )
		kernel.showactor("next_button_right_arrow_icon")
		kernel.hideactor("image_next_white_arrow_big")
	end
end

function check_uncheck_actor_from_prior_state(name, variableName)
	local tag = kernel.getmacro(variableName) or "unchecked"
	
	if tag == "unchecked" then
		kernel.setactortag(name,"unchecked" )
		kernel.setactorimage(name,"image_gray_unselected_checkbox")
	else
		kernel.setactortag(name,"checked" )
		kernel.setactorimage(name,"image_green_selected_checkbox")
	end

	update_next_button_state()
end

function check_uncheck_actor(name, variableName, kiosk_analytics_checked, kiosk_analytics_unchecked)	
	if kernel.getactortag( name ) == "checked" then
		kernel.setactortag( name,"unchecked" )
		kernel.setactorimage( name,"image_gray_unselected_checkbox" )
		view_analytics_event("buttonPress", kiosk_analytics_unchecked)
	else
		kernel.setactortag( name,"checked" )
		kernel.setactorimage( name,"image_green_selected_checkbox" )
		view_analytics_event("buttonPress", kiosk_analytics_checked)
	end

	kernel.setmacro(variableName, kernel.getactortag(name)) 
end

check_uncheck_actor_from_prior_state("agree_checkbox", "JVHardTake:Terms:Agree")
check_uncheck_actor_from_prior_state("consent_checkbox", "JVHardTake:Terms:Consent")

kernel.registeractorhitnotify(
	"terms_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_terms_and_privacy_ts_and_cs_button" )
		help_current_table = free_trial_terms
		free_trial_help_show_terms()
		view_show("free_trial_terms_and_privacy_legal_text_view")		
	end 
)

kernel.registeractorhitnotify(
	"cancel_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_terms_and_privacy_cancel_signup_button" )
		view_show("free_trial_cancel_popup")		
	end
)

function complete_signup_process()
	local email = kernel.getmacro("JVHardTake:KeyBoard:Text")
	local zip   = kernel.getmacro("JVHardTake:KeyPad:Text")	
	
	ShoppingCart:create_rbi_free_trial(email, zip, "signup", 
		function(r)
			if timeoutOccurred == true then
				remove_free_trial_please_wait_view_on_error()
				return 
			end
			
			hide_free_trial_please_wait_view()	
			
			if (not r.success) or (r.errors ~= nil and table_count( r.errors ) > 0) then
				view_show("free_trial_whoops_popup")		
			else
				if r.properties.is_eligible_free_trial then
					if r.properties.can_create_profile then
						view_show("free_trial_confirmation_view")
					else
						view_show("free_trial_soft_take_popup")
					end
				else
					view_show("free_trial_not_eligible_popup")		
				end
			end
		end
	)
end

kernel.registeractorhitnotify(
	"next_button",
	function()
		view_analytics_event( "buttonPress", "free_trial_terms_and_privacy_next_button" )
		show_free_trial_please_wait_view(onTimeout)		
		complete_signup_process()
	end
)

kernel.registeractorhitnotify(
	"agree_checkbox",
	function()
		check_uncheck_actor("agree_checkbox", "JVHardTake:Terms:Agree", "free_trial_terms_and_privacy_terms_check_enabled", "free_trial_terms_and_privacy_terms_checked_disabled")
		update_next_button_state()
	end
)

kernel.registeractorhitnotify(
	"consent_checkbox",
	function()
		check_uncheck_actor("consent_checkbox", "JVHardTake:Terms:Consent", "free_trial_terms_and_privacy_privacy_check_enabled", "free_trial_terms_and_privacy_privacy_checked_disabled")
		update_next_button_state()
	end
)