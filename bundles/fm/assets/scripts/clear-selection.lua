--
-- clear-selection.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local selected_actors = kernel.getactorsbytag("selected")

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

if selected_actors == nil then
	return
end

for k,v in ipairs(selected_actors) do
	kernel.hideactor(v .. "_glow")
	kernel.setactortag(v, nil)
end

kernel.setactorimage( "DVD_check_box_image","check_box" )
kernel.setactortag( "DVD_check_box_image", "unchecked" )
kernel.setactorimage( "blu_ray_check_box_image","check_box" )
kernel.setactortag( "blu_ray_check_box_image", "unchecked")
kernel.setactorimage( "games_check_box_image","check_box" )
kernel.setactortag("games_check_box_image", "unchecked")
kernel.setactorimage( "G_check_box_image","check_box" )
kernel.setactortag("G_check_box_image", "unchecked")
kernel.setactorimage( "PG_check_box_image","check_box" )
kernel.setactortag("PG_check_box_image", "unchecked")
kernel.setactorimage( "PG13_check_box_image","check_box" )
kernel.setactortag("PG13_check_box_image", "unchecked")
kernel.setactorimage( "R_check_box_image","check_box" )
kernel.setactortag("R_check_box_image", "unchecked")
kernel.setactorimage( "NR_check_box_image","check_box" )
kernel.setactortag("NR_check_box_image", "unchecked")
kernel.setactorimage( "E_check_box_image","check_box" )
kernel.setactortag("E_check_box_image", "unchecked")
kernel.setactorimage( "E10_check_box_image","check_box" )
kernel.setactortag("E10_check_box_image", "unchecked")
kernel.setactorimage( "T_check_box_image","check_box" )
kernel.setactortag("T_check_box_image", "unchecked")
kernel.setactorimage( "M_check_box_image","check_box" )
kernel.setactortag("M_check_box_image", "unchecked")
kernel.setactorimage( "MT_all_check_box_image","check_box_checked" )
kernel.setactortag("MT_all_check_box_image", "checked")
kernel.setactorimage( "MR_all_check_box_image","check_box_checked" )
kernel.setactortag("MR_all_check_box_image", "checked")
kernel.setactorimage( "GR_all_check_box_image","check_box_checked" )
kernel.setactortag("GR_all_check_box_image", "checked")