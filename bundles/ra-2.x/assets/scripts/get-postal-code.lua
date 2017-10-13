--
-- get-postal-code.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.start_idle_timer( "idle_timer" )

kernel.setmacro( "KeyPad:MessageLabel", strings.postal_code_prompt_message )
kernel.setmacro( "KeyPad:PanelTag", strings.postal_code_tag )
kernel.setmacro( "KeyPad:Text", "" )

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
    kernel.setmacro( "KeyPad:AuthText", string.gsub( strings.zip_code_auth_text, "{0}", convert_auth_days( ShoppingCart.auth_days ) ) )
else
    kernel.setmacro( "KeyPad:AuthText", "" )
end

key_pad_minimum_length = 5
key_pad_value = nil

view_show_safe( "key_pad_view" )
view_analytics_event( "context", "postalCode" )