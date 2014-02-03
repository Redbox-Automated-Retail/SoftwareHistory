--
-- configure-kiosk.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.createactor(
	"popup_background",
	false,
	0,
	0,
	1024,
	768,
	nil,
	nil,
	nil,
	"black",
	nil,
	nil,
	nil,
	nil,
	nil,
	"Background",
	"Obstruction")
kernel.setactoropacity( "popup_background", 0.50 )

prior_kiosk_id = StoreNumber
KeyNavigationManager.stop()
view_show( "configure_kiosk_view" )	