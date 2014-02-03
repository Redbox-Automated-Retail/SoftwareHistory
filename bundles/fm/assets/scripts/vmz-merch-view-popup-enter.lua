--
-- vmz-merch-view-popup-enter.lua 
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local called_from = kernel.getlasthitactortag()

kernel.log( "VMZ merch popup enter: last actor = " .. called_from )

if called_from == "unload" then
	kernel.setactorimage( "merch_image_box", "vmz_unload_view" )
end

idle_timer:stop()
