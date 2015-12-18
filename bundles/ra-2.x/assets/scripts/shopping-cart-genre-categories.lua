--
-- shopping-cart-genre-categories.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newshoppingcartgenrecategories()

	local mpaa_ratings = { [5]  = "G", [6]  = "PG", [7]  = "PG-13", [8]  = "R", [10] = "NR" }
	local esrb_ratings = { [14] = "EC", [15] = "E", [16] = "E 10+", [17] = "T", [18] = "M (17+)", [19] = "RP", [20] = "AO (18+)" }
	local product_types = { [1] = "DVD", [2] = "Blu-ray", [3] = "Xbox 360", [4] = "PS2", [5] = "PS3", [6] = "PSP", [7] = "Wii", [8] = "DS", [9] = "PC",  [10] = "Wii U", [11] = "PS4", [12] = "Xbox One" }
	local video_types = { [1] = true, [2] = true }
	local game_types = { [3] = true, [4] = true, [5] = true, [6] = true, [7] = true, [8] = true, [9] = true, [10] = true, [11] = true, [12] = true }

    local hit_movies_genre_id = 1020

	local flag_error = function( message )
		kernel.log( message )
		error( message )
	end

	local set_adjusted_ratings = function( self, rating_id )
		-- leaving these split out just in case they change their minds on allowable ratings...
		local adjusted_rating_mappings = {}
		adjusted_rating_mappings[5]  = function() self.ratings[5] = true end
		adjusted_rating_mappings[6]  = function() self.ratings[5] = true self.ratings[6] = true self.ratings[7] = true self.ratings[8] = true self.ratings[10] = true end
		adjusted_rating_mappings[7]  = function() self.ratings[5] = true self.ratings[6] = true self.ratings[7] = true self.ratings[8] = true self.ratings[10] = true end
		adjusted_rating_mappings[8]  = function() self.ratings[5] = true self.ratings[6] = true self.ratings[7] = true self.ratings[8] = true self.ratings[10] = true end
		adjusted_rating_mappings[9]  = function() self.ratings[5] = true self.ratings[6] = true self.ratings[7] = true self.ratings[8] = true self.ratings[10] = true end
		adjusted_rating_mappings[10] = function() self.ratings[5] = true self.ratings[6] = true self.ratings[7] = true self.ratings[8] = true self.ratings[10] = true end
		adjusted_rating_mappings[14] = function() self.ratings[14] = true self.ratings[15] = true end
		adjusted_rating_mappings[15] = function() self.ratings[14] = true self.ratings[15] = true end
		adjusted_rating_mappings[16] = function() self.ratings[14] = true self.ratings[15] = true self.ratings[16] = true self.ratings[17] = true self.ratings[18] = true self.ratings[19] = true self.ratings[20] = true end
		adjusted_rating_mappings[17] = function() self.ratings[14] = true self.ratings[15] = true self.ratings[16] = true self.ratings[17] = true self.ratings[18] = true self.ratings[19] = true self.ratings[20] = true end
		adjusted_rating_mappings[18] = function() self.ratings[14] = true self.ratings[15] = true self.ratings[16] = true self.ratings[17] = true self.ratings[18] = true self.ratings[19] = true self.ratings[20] = true end
		adjusted_rating_mappings[19] = function() self.ratings[14] = true self.ratings[15] = true self.ratings[16] = true self.ratings[17] = true self.ratings[18] = true self.ratings[19] = true self.ratings[20] = true end
		adjusted_rating_mappings[20] = function() self.ratings[14] = true self.ratings[15] = true self.ratings[16] = true self.ratings[17] = true self.ratings[18] = true self.ratings[19] = true self.ratings[20] = true end
		adjusted_rating_mappings[rating_id]()
	end

	local check_if_is_valid = function( product )
		local check_if_is_valid_dvd = function( product )
			if mpaa_ratings[product.rating_id] == nil then flag_error( "Product id: " .. product.product_id .. " has an invalid dvd rating of " .. product.rating_id .. "!" ) end
		end
		local check_if_is_valid_bluray = function( product )
			if mpaa_ratings[product.rating_id] == nil then flag_error( "Product id: " .. product.product_id .. " has an invalid bluray rating of " .. product.rating_id .. "!" ) end
		end
		local check_if_is_valid_game = function( product )
			if esrb_ratings[product.rating_id] == nil then flag_error( "Product id: " .. product.product_id .. " has an invalid game rating of " .. product.rating_id .. "!" ) end
		end
		local check_if_is_valid_functions = {
			[1] = check_if_is_valid_dvd,
			[2] = check_if_is_valid_bluray,
			[3] = check_if_is_valid_game,
			[4] = check_if_is_valid_game,
			[5] = check_if_is_valid_game,
			[6] = check_if_is_valid_game,
			[7] = check_if_is_valid_game,
			[8] = check_if_is_valid_game,
			[9] = check_if_is_valid_game
		}
		check_if_is_valid_functions[product.product_type_id]( product )
	end

	local is_in_coming_soon_date_range = function( release_date )
		local current = os.date( "*t" )
		local four_weeks_out = kernel.dateadd( current, "28" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )
		local four_weeks_out_string = string.format( "%4d%02d%02d%02d%02d", four_weeks_out.year, four_weeks_out.month, four_weeks_out.day, four_weeks_out.hour, four_weeks_out.min )
		return release_date > current_string and release_date < four_weeks_out_string
	end

	local is_coming_soon_hit_movie = function( product )
		if not video_types[product.product_type_id] then return false end
		if not product.genres[hit_movies_genre_id] then return false end
		if product.merchandise_date == nil then flag_error( "Product for id: " .. tostring(id) .. " does not have a release date!" ) end
		return is_in_coming_soon_date_range( product.merchandise_date )
	end

	local is_coming_soon_game = function( product )
		if not game_types[product.product_type_id] then return false end
		if product.merchandise_date == nil then flag_error( "Product for id: " .. tostring(id) .. " does not have a release date!" ) end
		return is_in_coming_soon_date_range( product.merchandise_date )
	end

	local process_product = function( self, product )
		if product.product_id == nil then flag_error( "Product does not contain a product_id value!" ) end
		local master_product = MasterProductCatalog[product.product_id]
		if master_product == nil then flag_error( "Product id: " .. product.product_id .. " is not in the MasterProductCatalog!" ) end
		if master_product.product_type_id == nil then flag_error( "Product id: " .. product.product_id .. " does not contain product_type_id information!" ) end
		if product_types[master_product.product_type_id] == nil then flag_error( "Product id: " .. product.product_id .. " has an invalid product_type_id of: " .. tostring(master_product.product_type_id) .. "!" ) end
		if master_product.rating_id == nil then flag_error( "Product id: " .. product.product_id .. " does not contain ratings information!" ) end
		if master_product.genres == nil then flag_error( "Product id: " .. product.product_id .. " does not contain genre information!" ) end
		check_if_is_valid( master_product )
		self.formats[master_product.product_type_id] = true
		set_adjusted_ratings( self, master_product.rating_id )
	end

	local check_if_product_matches_criteria_and_add_to_table_if_so = function( self, product, results )
		if self.formats[product.product_type_id] and self.ratings[product.rating_id] then
			table.insert( results, product )
		end
	end

	local scan_for_dvd_bluray_game = function( self, threshold )
		local results = {}
		for _, master in MasterProductCatalog:pairs() do
			if #results == threshold then break end
			if is_coming_soon_hit_movie( master ) then
				check_if_product_matches_criteria_and_add_to_table_if_so( self, master, results )
			elseif is_coming_soon_game( master ) then
				check_if_product_matches_criteria_and_add_to_table_if_so( self, master, results )
			end
		end
		return results
	end

	local scan_for_dvd_bluray = function( self, threshold )
		local results = {}
		for _, master in MasterProductCatalog:pairs() do
			if #results == threshold then break end
			if is_coming_soon_hit_movie( master ) then
				check_if_product_matches_criteria_and_add_to_table_if_so( self, master, results )
			end
		end
		return results
	end

	local game_in_selection_criteria = function( self )
		for f, _ in pairs( self.formats ) do
			if game_types[f] then return true end
		end
		return false
	end

	local t = {
		formats = {},
		ratings = {},

		process = function( self, products, total )
			if products == nil then flag_error( "Product table cannot be nil!" ) end
			if #products == 0 then flag_error( "Product table contains no entries!" ) end
			if total == nil then flag_error( "Total cannot be nil!" ) end
			if type(total) ~= "number" then flag_error( "Total must be a number!" ) end
			for _, product in ipairs( products ) do process_product( self, product ) end
			-- split the loop into games and non games, no need to check a flag each
			-- iteration if the critera doesn't contain games, and we need all the
			-- speed we can get here...
			local results =
				game_in_selection_criteria( self ) and
				scan_for_dvd_bluray_game( self, total ) or
				scan_for_dvd_bluray( self, total )
			table.sort( results, function( a, b ) return a.image_file < b.image_file end )
			return results
		end
	}

	return t
end
