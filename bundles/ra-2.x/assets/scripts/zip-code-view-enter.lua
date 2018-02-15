--
-- zip-code-view-enter.lua
-- Copyright (C) 2017 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.start_idle_timer( "idle_timer" )
already_sent_auth = false

local auth_days_text = ""
if ShoppingCart.prompt_required == true then
    local function convert_auth_days( auth_days )
        local auth_days_lookup = { 
            [0] = { en = "zero", es = "cero" }, 
            [1] = { en = "one", es = "uno" }, 
            [2] = { en = "two", es = "dos" }, 
            [3] = { en = "three", es = "tres" }, 
            [4] = { en = "four", es = "cuatro" }, 
            [5] = { en = "five", es = "cinco" }, 
            [6] = { en = "six", es = "seis" }, 
            [7] = { en = "seven", es = "siete" }, 
            [8] = { en = "eight", es = "ocho" }, 
            [9] = { en = "nine", es = "nueve" } 
        }
        if auth_days_lookup[auth_days] ~= nil then
            if kernel.getculture() == "es-MX" then
                return auth_days_lookup[auth_days].es
            end
            return auth_days_lookup[auth_days].en
        end
        if kernel.getculture() == "es-MX" then
            return auth_days_lookup[5].es
        end
        return auth_days_lookup[5].en
    end
    auth_days_text = string.gsub( strings.zip_code_auth_text, "{0}", convert_auth_days( ShoppingCart.auth_days ) )
end

kernel.configure_zip_code_view(
	auth_days_text,
    function( display_text )
        if already_sent_auth then
            kernel.log("Already sent auth")
            return
        end
        already_sent_auth = true
		if kernel.get_rental_shopping_use_credits( ShoppingCart.session_id ) == "OptedIn" then
			ShoppingCart.postal_code = display_text
			key_pad_next_getcustomercredits_func()
		else
			if key_pad_next_authorize_func ~= nil then
				ShoppingCart.postal_code = display_text
				show_credits_popup = true
				key_pad_next_authorize_func()
			else
				view_goback_safe( "keypad:next_button", false )
			end
		end
	end
)

view_analytics_event( "context", "postalCode" )