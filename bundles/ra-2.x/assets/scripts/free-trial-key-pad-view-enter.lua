--
-- free-trial-key-pad-view-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()
kernel.executescript("free_trial_create_cancel_idle_timer")

kernel.setmacro( "JVHardTake:KeyPad:MessageLabel", strings.postal_code_prompt_message )
kernel.setmacro( "JVHardTake:KeyPad:PanelTag", strings.postal_code_tag )
JVHardTake_key_pad_minimum_length = 5

local editpath = kernel.getmacro( "JVHardTake:Zip:EditPath" ) or ""

if (editpath == "swipe") then	
	local function get_swiped_customer()
		local timeoutOccurred = false
		
		local function onTimeout()
			timeoutOccurred = true
		end
		
		show_free_trial_please_wait_view(onTimeout)
				
		kernel.digitalsvcs_getcustomer(
			ShoppingCart.session_id,
			kernel.trackgethashid(),
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
						kernel.setmacro("JVHardTake:KeyBoard:Text", r.properties.email)
					
						if r.properties.has_customer_profile then
							kernel.setmacro( "JVHardTake:User:HasProfile", "true" )
							view_show("free_trial_email_presentation_view")
						else
							kernel.setmacro( "JVHardTake:User:HasProfile", "false" )
							kernel.setmacro( "JVHardTake:Email:EditPath", "" )
							view_show("free_trial_email_entry_view")
						end
					else
						view_show("free_trial_not_eligible_popup")		
					end
				end
			end
		)
	end

	kernel.registeractorhitnotify(
		"cancel_button",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_cancel_signup_button" )
			view_show("free_trial_cancel_popup")
		end
	)

	kernel.registeractorhitnotify(
		"next_button",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			get_swiped_customer()
		end
	)

	kernel.registeractorhitnotify(
		"next_button_right_arrow_icon",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			get_swiped_customer()
		end
	)
	
	kernel.registeractorhitnotify(
		"image_next_white_arrow_big",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			get_swiped_customer()
		end
	)
else	
	kernel.registeractorhitnotify(
		"cancel_button",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_cancel_signup_button" )
			view_show("free_trial_cancel_popup")
		end
	)

	kernel.registeractorhitnotify(
		"next_button",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			view_goback_safe("free_trial_zip_code_view", false)
		end
	)	

	kernel.registeractorhitnotify(
		"next_button_right_arrow_icon",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			view_goback_safe("free_trial_zip_code_view", false)
		end
	)
	
	kernel.registeractorhitnotify(
		"image_next_white_arrow_big",
		function()
			view_analytics_event( "buttonPress", "free_trial_zip_code_next_button" )
			view_goback_safe("free_trial_zip_code_view", false)
		end
	)
end

local text = kernel.getmacro( "JVHardTake:KeyPad:Text" )
kernel.setactortext( "text_label", text )

if  text ~= nil and string.len( text ) >= JVHardTake_key_pad_minimum_length then	
	kernel.enableactor( "next_button" )
	kernel.enableactor( "back_button" )
	kernel.setactorforegroundcolor( "next_button", "white" )
	kernel.hideactor("next_button_right_arrow_icon")
	kernel.showactor("image_next_white_arrow_big")
else
	kernel.disableactor( "next_button" )
	kernel.setactorforegroundcolor( "next_button", "gray" )
	kernel.showactor("next_button_right_arrow_icon")
	kernel.hideactor("image_next_white_arrow_big")
end