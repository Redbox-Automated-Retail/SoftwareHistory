--
-- free-trial-clear-user-data.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Erase values related to a user, so they don't bleed over into the next user's workflow

kernel.setmacro( "JVHardTake:KeyPad:MessageLabel", "" )
kernel.setmacro( "JVHardTake:KeyPad:PanelTag", "" )
kernel.setmacro( "JVHardTake:KeyPad:Text", "" )
kernel.setmacro( "JVHardTake:KeyBoard:PanelTag", "" )
kernel.setmacro( "JVHardTake:KeyBoard:Text", "" )
kernel.setmacro( "JVHardTake:Terms:Agree", "unchecked" )
kernel.setmacro( "JVHardTake:Terms:Consent", "unchecked" )
kernel.setmacro( "JVHardTake:Email:EditPath", "" )
kernel.setmacro( "JVHardTake:Zip:EditPath", "" )
kernel.setmacro( "JVHardTake:User:HasProfile", nil )