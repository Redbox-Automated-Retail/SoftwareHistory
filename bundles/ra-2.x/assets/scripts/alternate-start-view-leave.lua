--
-- alternate-start-view-leave.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == "find_games_button" or kernel.getlasthitactorname() == "games_label_1" or kernel.getlasthitactorname() == "games_label_2" then
 browse_filter_family_tracker.selected_family = "Games"
 browse_filter_family_tracker:set_family_type("ALL")
 browse_filter_genre_selected = nil
end
if kernel.getlasthitactorname() == "find_movies_button" or kernel.getlasthitactorname() == "movies_label_1" or kernel.getlasthitactorname() == "movies_label_2" then
 browse_filter_family_tracker.selected_family = "Movies"
 browse_filter_family_tracker:set_family_type("ALL")
 browse_filter_genre_selected = ALL
end

set_number = 1
browse_selected_sort = "Newest"
