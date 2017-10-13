--
-- obj-coming-soon-generator.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newcomingsoongenerator()

	local TVRatingsConversion = {
		[21] = { rating_id = 21, name = "TVMA", mpaa_equiv = 8 },
		[22] = { rating_id = 22, name = "TVPG", mpaa_equiv = 6 },
		[23] = { rating_id = 23, name = "TV14", mpaa_equiv = 7 },
		[24] = { rating_id = 24, name = "TVG", mpaa_equiv = 5 },
		[25] = { rating_id = 25, name = "TVY", mpaa_equiv = 5 },
		[26] = { rating_id = 26, name = "TVY7", mpaa_equiv = 5 }
	}

	-- Currently, only EC, E, and E10 games will limit coming soon to other EC, E and E10 games.  All else are free-for-all!
	local ESRBRatingsConversion = {
		[14] = { rating_id = 14, name = "EC", equiv = 16 },
		[15] = { rating_id = 15, name = "E", equiv = 16 },
		[16] = { rating_id = 16, name = "E 10+", equiv = 16 },
		[17] = { rating_id = 17, name = "T", equiv = 20 },
		[18] = { rating_id = 18, name = "M (17+)", equiv = 20 },
		[19] = { rating_id = 19, name = "RP", equiv = 20 },
		[20] = { rating_id = 20, name = "AO (18+)", equiv = 20 }
	}

	local is_in_date_range = function( release_date_string )
		local current = os.date( "*t" )
		local three_weeks_out = kernel.dateadd( current, "21" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )
		local three_weeks_out_string = string.format( "%4d%02d%02d%02d%02d", three_weeks_out.year, three_weeks_out.month, three_weeks_out.day, three_weeks_out.hour, three_weeks_out.min )
		return release_date_string > current_string and release_date_string < three_weeks_out_string
	end

	local is_in_date_range2 = function( release_date_string, limit_days )
	
		local current = os.date( "*t" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )

		if VerboseLogging == true then
			kernel.log( "> Release = " .. release_date_string )
			kernel.log( "> Days = " .. tostring( limit_days ) )
			kernel.log( "> Current = " .. current_string )
		end
		
		if release_date_string > current_string then
			local days_before_release = kernel.getelapseddays( current_string, release_date_string )

			if VerboseLogging == true then
				kernel.log( "> Elapsed Days = " .. tostring( days_before_release ))
			end

			if days_before_release ~= nil and days_before_release <= limit_days then
				return true
			end
		end
		return false
	end

	local is_in_date_range3 = function( product )
		-- This function will account for early release titles [$3 Firsts]
		local release_date_string = product.merchandise_date
		return is_in_date_range( release_date_string )
	end

	local get_by_type = function( items_in_cart, product_type_name, max_result_count )

		-- What is in the cart is relevant
		local game_types = {}
		local highest_rating = 0
		for _, product in ipairs( items_in_cart ) do
			if VerboseLogging == true then
				kernel.log( ">  cart item p:" .. tostring( product.product_id ) ..
					" r:" .. tostring( product.rating_id ) ..
					" pt:" .. tostring( product.product_type_id ) ..
					" rd:" .. tostring( product.merchandise_date ) ..
					" " ..tostring( product.sort_name ))
			end

			-- Determine highest rating of relevant cart items
			if ( product_type_name == "game" and product.product_type_id > 2 )
			or ( product_type_name ~= "game" and product.product_type_id <= 2 ) then
				local temp_rating = product.rating_id
				if product.rating_id > 20 then
					-- TV ratings must be converted to comparable mpaa ratings
					local converted_rating = TVRatingsConversion[product.rating_id]
					if converted_rating ~= nil then
						temp_rating = converted_rating.mpaa_equiv
					end
				elseif product.rating_id >= 14 then
					-- Games ratings may also be converted to treat E, EC, and E10 as the same thing
					local converted_rating = ESRBRatingsConversion[product.rating_id]
					if converted_rating ~= nil and converted_rating.equiv ~= nil then
						temp_rating = converted_rating.equiv
					end
				elseif product.rating_id == 10 and product.product_type_id > 2 then
					-- Games with NO rating set in hive show up this way
					temp_rating = 19 --arbitrary there is acceptance criteria for this yet
				end

				if VerboseLogging == true then
					kernel.log( ">  cart_item converted rating_id " .. tostring( temp_rating ))
				end
				if temp_rating > highest_rating then
					highest_rating = temp_rating
				end
			end

			-- compile complete list of game platforms in cart
			if ( product_type_name == "game" and product.product_type_id > 2 ) then
				game_types[ product.product_type_id ] = true
			end
		end
		kernel.log( ">  highest rating_id = " .. tostring( highest_rating ))
		if highest_rating == 0 then
			-- The only item in the cart is a different product type
			highest_rating = 1000
			kernel.log( ">  highest rating_id reset to 1000" )
		end

		local results = {}
		local lookup_hash = "coming_soon_" .. product_type_name .. "_rating_" .. tostring( highest_rating )
		local cached_results = FilteredProductList[lookup_hash]
		if cached_results ~= nil then
			for _, v in ipairs( cached_results ) do
				local master_product = MasterProductCatalog[ v ]
				if master_product ~= nil
				and ( product_type_name ~= "game" or game_types[ master_product.product_type_id ] ~= nil ) then
					table.insert( results, master_product )
					if #results == max_result_count then break end
				end
			end
		end

		table.sort( results, function( a, b ) return a.merchandise_date < b.merchandise_date end )
		return results
	end

	local is_in_coming_soon_date_range = function( release_date )
		local current = os.date( "*t" )
		local four_weeks_out = kernel.dateadd( current, "28" )
		local current_string = string.format( "%4d%02d%02d%02d%02d", current.year, current.month, current.day, current.hour, current.min )
		local four_weeks_out_string = string.format( "%4d%02d%02d%02d%02d", four_weeks_out.year, four_weeks_out.month, four_weeks_out.day, four_weeks_out.hour, four_weeks_out.min )
		return release_date > current_string and release_date < four_weeks_out_string
	end

	local t = {}

	t.build_cache = function( self, product_type_name, highest_rating, max_result_count )
		local lookup_hash = "coming_soon_" .. product_type_name .. "_rating_" .. tostring( highest_rating )
		kernel.log( "> Build coming soon cache for " .. lookup_hash )
		local results = {}
		local count = 1
		-- cache globals to locals for improved performance
		local _MasterProductCatalog = MasterProductCatalog
		
		local process_product = function( product )
			if get_current_date_string( "%4d%02d%02d%02d%02d" ) < product.merchandise_date
			and product.product_type_id ~= nil
			and product.rating_id ~= nil
			then

				if VerboseLogging == true then
					kernel.log( ">  candidate p:" .. tostring( product.product_id ) ..
						" r:" .. tostring( product.rating_id ) ..
						" pt:" .. tostring( product.product_type_id ) ..
						" rd:" .. tostring( product.merchandise_date ) ..
						" " .. tostring( product.sort_name ) ..
						" g[1020]:" .. tostring( product.genres[1020] ) )
				end

				-- Game platform is checked when the cache is used, not when it is built (we may change this later)
				if (
					product_type_name == "game"
					and product.product_type_id ~= nil
					and product.product_type_id > 2
					and product.rating_id ~= nil
					and product.rating_id <= highest_rating
					and is_in_coming_soon_date_range( product.merchandise_date )
					and ( product.disc_number == nil or product.disc_number == 1 )
					)
				or (
					((product_type_name == "bluray"	and product.product_type_id == 2 ) 	or
					(product_type_name == "dvd" and product.product_type_id == 1 ) 	)
					and product.rating_id <= highest_rating
					and product.genres ~= nil
					and product.genres[1020] ~= nil
					and product.merchandise_date ~= nil
					and is_in_date_range3( product ) == true
					and ( product.disc_number == nil or product.disc_number == 1 )
				)
				then
					-- Mingle #1261 only display coming soon titles if we can successfully load the artwork
					local image_registered = kernel.dataserviceregisterimage( product.image_file, product.image_file )
					if image_registered then
						results[count] = product.product_id
						kernel.log("> Coming soon " .. product_type_name .. " cache added: " .. product.sort_name )
						count = count + 1
					end
				end
			end
		end
		
		-- Use standard release titles to fill out the cache list
		for _, product in _MasterProductCatalog:pairs() do
			if #results >= max_result_count then break end
			process_product( product )
		end

		FilteredProductList[lookup_hash] = results
		kernel.log( "> " .. lookup_hash .. " has " .. tostring( count - 1 ) .. " values" )
	end


	t.get_products = function( self, items_in_cart, max_result_count )
		kernel.log( ">  coming-soon-generator:get_products() cart_count = " .. tostring( #items_in_cart) )

		-- Determine the shopping cart mix: DVD only, ANY blu-ray but no games, ANY games
		local has_game = false
		local has_bluray = false
		for _, product in ipairs( items_in_cart ) do
			if product.product_type_id ~= nil and product.product_type_id == 2 then
				has_bluray = true
				kernel.log( ">  has_bluray = true" )
			elseif product.product_type_id ~= nil and product.product_type_id > 2 then
				has_game = true
				kernel.log( ">  has_game = true" )
			end
		end

		-- Gather needed suggested products
		local bluray_suggestions = {}
		local games_suggestions = {}
		local dvd_suggestions = {}
		if has_game == true then
			games_suggestions = get_by_type( items_in_cart, "game", 2 )
			kernel.log( ">  suggested games = ".. tostring( #games_suggestions ))
		elseif has_bluray == true then
			-- Games trump blu-ray, only come here if there are NO games in the cart (the elseif is deliberate)
			bluray_suggestions = get_by_type( items_in_cart, "bluray", 3 )
			kernel.log( ">  suggested blurays = ".. tostring( #bluray_suggestions ))
		end
		if #bluray_suggestions < 3 then
			-- We will need DVD selections in all cases except when there are already 3 blu-rays
			dvd_suggestions = get_by_type( items_in_cart, "dvd", 3 )
			kernel.log( ">  suggested dvds = ".. tostring( #dvd_suggestions ))
		end

		-- Assemble the results based on rules
		results = {}
		if #games_suggestions > 0 then
			for _, product in ipairs( games_suggestions ) do
				table.insert( results, product )
				kernel.log("> Coming soon GAME: " .. product.sort_name )
			end
		elseif #bluray_suggestions > 0 then
			for _, product in ipairs( bluray_suggestions ) do
				table.insert( results, product )
				kernel.log("> Coming soon BLURAY: " .. product.sort_name )
			end
		end

		-- Fill empty slots with DVDs
		if #results < 3 and #dvd_suggestions > 0 then
			for _, product in ipairs( dvd_suggestions ) do
				table.insert( results, product )
				kernel.log("> Coming soon DVD: " .. product.sort_name )
				if #results == 3 then
					break
				end
			end
		end

		kernel.log( ">  result count = ".. tostring( #results ))
		return results
	end

	t.create_browse_titles = function( self )
		kernel.log( "Generate coming soon titles for Browse view from Master product catalog" )
		-- cache globals to locals for improved performance
		local _MasterProductCatalog = MasterProductCatalog
		local _ProductCatalog = ProductCatalog

		local add_count = 0
		for _, product in _MasterProductCatalog:pairs() do
			if product.coming_soon_days ~= nil
			and product.coming_soon_days > 0 then
				if VerboseLogging == true then
					kernel.log( "> PID:" .. tostring( product.product_id ) ..
						", CSDays:" .. tostring( product.coming_soon_days ) ..
						", ReleaseDate:" .. StoreManager.PricingManager:get_early_release_date( product ) ..
						", is_in_date_range2:" .. tostring( is_in_date_range2( StoreManager.PricingManager:get_early_release_date( product ), product.coming_soon_days )))
				end
			end
		
			if product.coming_soon_days ~= nil
			and product.coming_soon_days > 0
			and is_in_date_range2( StoreManager.PricingManager:get_early_release_date( product ), product.coming_soon_days ) 
			then
				if _ProductCatalog[ product.product_id ] == nil then
					add_title_without_barcodes( product.product_id )
					add_count = add_count + 1
					kernel.log( "> coming soon title " .. tostring( product.product_id ) .. " added to ProductCatalog and ProductHistory" )
				else
					local history = ProductHistory[ product.product_id ]
					if history ~= nil then
						kernel.log( "> coming soon title " .. tostring( product.product_id ) .. " updated in ProductCatalog" )
						create_or_update_product_catalog_entry( product.product_id, history )
					end
				end
			end
		end
		kernel.log( "> created " .. tostring( add_count ) .. " new coming soon titles" )
	end

	return t
end
