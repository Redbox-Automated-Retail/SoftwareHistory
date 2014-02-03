--
-- get-postal-code.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:start()

kernel.setmacro( "KeyPad:MessageLabel", strings.postal_code_prompt_message )
kernel.setmacro( "KeyPad:PanelTag", strings.postal_code_tag )
kernel.setmacro( "KeyPad:Text", "" )
key_pad_minimum_length = 5
key_pad_value = nil

view_show_safe( "key_pad_view" )
view_analytics_event( "context", "postalCode" )
