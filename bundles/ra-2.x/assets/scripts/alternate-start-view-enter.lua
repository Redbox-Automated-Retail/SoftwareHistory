--
-- alternate-start-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.timerstart( "reservation-expire-timer" )
idle_timer:stop()

selected_movies_list = {}

-- This forces the data to actually get loaded into memory to increase speed of the browse functionality
local garbage1 = ProductFamilies:pairs()
local garbage2 = ProductCatalog:pairs()
local garbage3 = ProductTypes:pairs()

-- Pre-setting these values back to defaults here saves time
browse_filter_mode = "all"
browse_sort_mode = "release-date"
browse_filter_description = nil
browse_filter_genre_selected = nil
browse_filter_alpha_selected = nil
browse_filter_coming_soon_selected = nil

ShoppingCart = newshoppingcart()
ShoppingCart:start_new()
ShoppingCartList[ ShoppingCart.session_id ] = ShoppingCart


browse_filter_family_tracker = ProductFamilies:new_selection_tracker( "Movies" )
ProductCatalog:get_default_family()


