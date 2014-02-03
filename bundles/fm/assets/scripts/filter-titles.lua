--
-- filter-titles.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

media_actors = { "DVD_check_box_image", 
				"blu_ray_check_box_image", 
				"games_check_box_image", 
				"MT_all_check_box_image",				
				"G_check_box_image", 
				"PG_check_box_image", 
				"PG13_check_box_image", 
				"R_check_box_image",
				"NR_check_box_image",
				"MR_all_check_box_image",				
				"E_check_box_image",
				"E10_check_box_image",
				"T_check_box_image",
				"M_check_box_image",
				"GR_all_check_box_image"		
				}

function uncheck_all_media_checkbox()
	if kernel.getactortag( "MT_all_check_box_image" ) == "checked" then
		kernel.setactortag( "MT_all_check_box_image","unchecked" )
		kernel.setactorimage( "MT_all_check_box_image","check_box" )
	end	
end

function uncheck_all_movie_rating()
	if kernel.getactortag( "MR_all_check_box_image" ) == "checked" then
		kernel.setactortag( "MR_all_check_box_image","unchecked" )
		kernel.setactorimage( "MR_all_check_box_image","check_box" )
	end
end

function uncheck_all_game_rating()
	if kernel.getactortag( "GR_all_check_box_image" ) == "checked" then
		kernel.setactortag( "GR_all_check_box_image","unchecked" )
		kernel.setactorimage( "GR_all_check_box_image","check_box" )
	end
end

for i,v in pairs( media_actors ) do
	if kernel.getlasthitactorname() == v and v~="MT_all_check_box_image" and v~="MR_all_check_box_image" and v~="GR_all_check_box_image" then
		if kernel.getlasthitactortag() == "unchecked" then
			kernel.setactortag( v , "checked" )
			kernel.setactorimage( v , "check_box_checked" )
			--uncheck the check box "all" in media when any media type is checked
			if i > 0 and i < 5 then
				uncheck_all_media_checkbox()
			--uncheck the check box "all" in movie_rating when any movie rating is checked
			elseif i > 4 and i < 11 then
				uncheck_all_movie_rating()
			--uncheck the check box "all" in game_rating when any game rating is checked
			else
				uncheck_all_game_rating()
			end
		elseif kernel.getlasthitactortag() == "checked" then
			kernel.setactortag( v , "unchecked" )
			kernel.setactorimage( v , "check_box" )		
		end
	end	
	if kernel.getlasthitactorname() == v and v == "MT_all_check_box_image" then
		if kernel.getlasthitactortag() == "checked" then
			kernel.setactortag( v , "unchecked" )
			kernel.setactorimage( v , "check_box" )
		elseif kernel.getlasthitactortag() == "unchecked" then
			kernel.setactorimage("DVD_check_box_image" , "check_box")
			kernel.setactortag( "DVD_check_box_image" , "unchecked" )
			kernel.setactorimage( "blu_ray_check_box_image" , "check_box" )
			kernel.setactortag( "blu_ray_check_box_image" , "unchecked" )
			kernel.setactorimage( "games_check_box_image" , "check_box" )
			kernel.setactortag( "games_check_box_image" , "unchecked" )			
			kernel.setactortag( "MT_all_check_box_image" , "checked" )
			kernel.setactorimage( "MT_all_check_box_image" , "check_box_checked" )
		end
	end	
	if kernel.getlasthitactorname() == v and v == "MR_all_check_box_image" then
		if kernel.getlasthitactortag() == "checked" then
			kernel.setactortag( v , "unchecked" )
			kernel.setactorimage( v , "check_box" )
		elseif kernel.getlasthitactortag() == "unchecked" then
			kernel.setactorimage("G_check_box_image" , "check_box")
			kernel.setactortag( "G_check_box_image" , "unchecked" )
			kernel.setactorimage("PG_check_box_image" , "check_box")
			kernel.setactortag( "PG_check_box_image" , "unchecked" )
			kernel.setactorimage("PG13_check_box_image" , "check_box")
			kernel.setactortag( "PG13_check_box_image" , "unchecked" )
			kernel.setactorimage("R_check_box_image" , "check_box")
			kernel.setactortag( "R_check_box_image" , "unchecked" )
			kernel.setactorimage("NR_check_box_image" , "check_box")
			kernel.setactortag( "NR_check_box_image" , "unchecked" )
			kernel.setactortag( "MR_all_check_box_image" , "checked" )
			kernel.setactorimage( "MR_all_check_box_image" , "check_box_checked" )	
		end
	end
	if kernel.getlasthitactorname() == v and v == "GR_all_check_box_image" then
		if kernel.getlasthitactortag() == "checked" then
			kernel.setactortag( v , "unchecked" )
			kernel.setactorimage( v , "check_box" )
		elseif kernel.getlasthitactortag() == "unchecked" then
			kernel.setactorimage("E_check_box_image" , "check_box")
			kernel.setactortag( "E_check_box_image" , "unchecked" )
			kernel.setactorimage("E10_check_box_image" , "check_box")
			kernel.setactortag( "E10_check_box_image" , "unchecked" )
			kernel.setactorimage("T_check_box_image" , "check_box")
			kernel.setactortag( "T_check_box_image" , "unchecked" )
			kernel.setactorimage("M_check_box_image" , "check_box")
			kernel.setactortag( "M_check_box_image" , "unchecked" )
			kernel.setactortag( "GR_all_check_box_image" , "checked" )
			kernel.setactorimage( "GR_all_check_box_image" , "check_box_checked" )	
		end	
	end
end