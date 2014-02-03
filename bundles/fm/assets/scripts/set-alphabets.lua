--
-- set-alphabets.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local actors = kernel.getactorsbytag("selected")
local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )
if actors == nil then
	log("Error getting actors by tag")
	return
end

alpha_table = {}
enable_filters = true
select_all_tab = false

for k, v in ipairs( actors ) do
	local alpha = kernel.getactortext(v)
	table.insert(alpha_table, alpha)
end

if kernel.getactortag( "DVD_check_box_image" ) == "checked" then
	filter_titles_tables.media[ "DVD" ] = true
end

if kernel.getactortag( "blu_ray_check_box_image" ) == "checked" then
	filter_titles_tables.media[ "Blu-ray" ] = true
end

if kernel.getactortag( "games_check_box_image" ) == "checked" then
	filter_titles_tables.media[ "Games" ] = true
end

if kernel.getactortag( "MT_all_check_box_image" ) == "checked"
or (kernel.getactortag( "DVD_check_box_image" ) == "unchecked"
	and kernel.getactortag( "blu_ray_check_box_image" ) == "unchecked"
	and kernel.getactortag( "games_check_box_image" ) == "unchecked")
then
	filter_titles_tables.media[ "DVD" ] = true
	filter_titles_tables.media[ "Blu-ray" ] = true
	filter_titles_tables.media[ "Games" ] = true
end

if kernel.getactortag( "G_check_box_image" ) == "checked" then
	filter_titles_tables.movie_rating[ "G" ] = true
end

if kernel.getactortag( "PG_check_box_image" ) == "checked" then
	filter_titles_tables.movie_rating[ "PG" ] = true
end

if kernel.getactortag( "PG13_check_box_image" ) == "checked" then
	filter_titles_tables.movie_rating[ "PG-13" ] = true
end

if kernel.getactortag( "R_check_box_image" ) == "checked" then
	filter_titles_tables.movie_rating[ "R" ] = true
end

if kernel.getactortag( "NR_check_box_image" ) == "checked" then
	filter_titles_tables.movie_rating[ "NR" ] = true
end

if kernel.getactortag( "MR_all_check_box_image" ) == "checked"
or (kernel.getactortag( "G_check_box_image" ) == "unchecked"
	and kernel.getactortag( "PG_check_box_image" ) == "unchecked"
	and kernel.getactortag( "PG13_check_box_image" ) == "unchecked"
	and	kernel.getactortag( "R_check_box_image" ) == "unchecked"
	and	kernel.getactortag( "NR_check_box_image" ) == "unchecked")
then
	filter_titles_tables.movie_rating[ "G" ] = true
	filter_titles_tables.movie_rating[ "PG" ] = true
	filter_titles_tables.movie_rating[ "PG-13" ] = true
	filter_titles_tables.movie_rating[ "R" ] = true
	filter_titles_tables.movie_rating[ "NR" ] = true
	filter_titles_tables.movie_rating[ "TVMA" ] = true
	filter_titles_tables.movie_rating[ "TVPG" ] = true
	filter_titles_tables.movie_rating[ "TV14" ] = true
	filter_titles_tables.movie_rating[ "TVG" ] = true
	filter_titles_tables.movie_rating[ "TVY" ] = true
	filter_titles_tables.movie_rating[ "TVY7" ] = true
end

if kernel.getactortag( "E_check_box_image" ) == "checked" then
	filter_titles_tables.game_rating[ "E" ] = true
end

if kernel.getactortag( "E10_check_box_image" ) == "checked" then
	filter_titles_tables.game_rating[ "E 10+" ] = true
end

if kernel.getactortag( "T_check_box_image" ) == "checked" then
	filter_titles_tables.game_rating[ "T" ] = true
end

if kernel.getactortag( "M_check_box_image" ) == "checked" then
	filter_titles_tables.game_rating[ "M (17+)" ] = true
end

if kernel.getactortag( "GR_all_check_box_image" ) == "checked"
or (kernel.getactortag( "E_check_box_image" ) == "unchecked"
	and kernel.getactortag( "E10_check_box_image" ) == "unchecked"
	and kernel.getactortag( "T_check_box_image" ) == "unchecked"
	and kernel.getactortag( "M_check_box_image" ) == "unchecked")
then
	filter_titles_tables.game_rating[ "E" ] = true
	filter_titles_tables.game_rating[ "E 10+" ] = true
	filter_titles_tables.game_rating[ "T" ] = true
	filter_titles_tables.game_rating[ "M (17+)" ] = true
	filter_titles_tables.game_rating[ "RP" ] = true
	filter_titles_tables.game_rating[ "AO (18+)" ] = true
end

if (kernel.getactortag( "GR_all_check_box_image" ) == "checked"
	and	kernel.getactortag( "MR_all_check_box_image" ) == "checked"
	and kernel.getactortag( "MT_all_check_box_image" ) == "checked")
or (kernel.getactortag( "DVD_check_box_image" ) == "unchecked"
	and kernel.getactortag( "blu_ray_check_box_image" ) == "unchecked"
	and kernel.getactortag( "games_check_box_image" ) == "unchecked"
	and kernel.getactortag( "G_check_box_image" ) == "unchecked"
	and kernel.getactortag( "PG_check_box_image" ) == "unchecked"
	and kernel.getactortag( "PG13_check_box_image" ) == "unchecked"
	and	kernel.getactortag( "R_check_box_image" ) == "unchecked"
	and	kernel.getactortag( "NR_check_box_image" ) == "unchecked"
	and kernel.getactortag( "E_check_box_image" ) == "unchecked"
	and kernel.getactortag( "E10_check_box_image" ) == "unchecked"
	and kernel.getactortag( "T_check_box_image" ) == "unchecked"
	and kernel.getactortag( "M_check_box_image" ) == "unchecked")
then
	select_all_tab = true
end

view_goback()