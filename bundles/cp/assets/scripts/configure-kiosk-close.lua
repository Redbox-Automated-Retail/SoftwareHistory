--
-- configure-kiosk-close.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.removeactor( "popup_background" )

idle_timer:start()
view_goback()	