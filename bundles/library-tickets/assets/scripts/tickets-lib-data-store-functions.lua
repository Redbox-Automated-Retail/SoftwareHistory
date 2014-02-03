--
-- tickets-lib-data-store-functions.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function getfulltickettable( name )
	local table = gettickettable( name )
	table:getall()
	return table
end

function gettickettable( name )
	local mt = {}
	
	mt.__index = function( vtable, key )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		local table_name = rawget( vtable, "table_name" )

		return cache[key]
	end
	
	mt.__newindex = function( vtable, key, value )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		if type( value ) == "function" then
			cache[key] = value
		else
			if cache.keys == nil then
				cache.keys = {}
			end
			if cache[key] == nil then
				table.insert( cache.keys, key )
			end
			cache[key] = value
		end
	end		

	local t = {}
	t.data_store_name = "tickets-profile"
	t.table_name = name
	t.cache = {}
	
	t.reset_keys_cache = function( self )
		rawset(self,"cache",{})
	end
	
	t.close = function( self )
		rawset(self,"cache",{})
	end
	
	t.getkeys = function( self )
		local cache = rawget( self, "cache" )
		return cache.keys or {}
	end
	
	t.getsize = function( self )
		local cache = rawget( self, "cache" )
		return #cache.keys or 0
	end

	t.remove = function( self, key )
		if key == nil then
			kernel.log( "> :remove() key was NULL" )
			return
		end
		key = tostring( key )
		local cache = rawget( self, "cache" )
		if cache.keys ~= nil then
			for i, v in ipairs( cache.keys ) do
				if v == key then
					table.remove( cache.keys, i )
				end
			end
		end
		cache[key] = nil
	end
	
	t.clear = function( self )
		rawset( self, "cache", {} )
	end

	t.prime_cache_keys = function( self )
	end

	t.ipairs = function( self )
		local function ipairs_it( t1, i )
			i = i + 1
			local v = t1[i]
			if v ~= nil then
				return i,v
			else
				return v
			end
		end
		return ipairs_it, self, 0
	end

	t.pairs = function( self )
		local cache = rawget( self, "cache" )
		t.prime_cache_keys( self )
		--kernel.log( "table " .. table_name .. " has #cache.keys = " .. tostring( #cache.keys ) )
		local i = 0
		local function pairs_it( t1 )
			i = i + 1
			local cache = rawget( t1, "cache" )
			local k = cache.keys[i]
			if k ~= nil then
				local v = t1[k]
				if v ~= nil then
					return k,v
				else
					return v
				end
			else
				return nil
			end
		end
		return pairs_it, self
	end
	
	t.getkeys = function( self )
		--return kernel.datastoregetkeys( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
	end	
	
	t.getall = function ( self )
		all = kernel.ticketsvcs_datatablegetall( rawget( self, "table_name" ))
		rawset( self, "cache", all)
		return nil
	end

	setmetatable( t, mt )

	return t
end

function load_tables()
	kernel.log( "|Tickets| loading tickets cache tables" )

	if DataAccessAlreadySent ~= true then
		kernel.datastoreregistprovider("tickets-cache", "CachedDataStoreProvider")

		-- TICKETS-PROFILE.DATA
		kernel.log("Loading Categories")
		Categories = getfulltickettable( "Categories" )
		kernel.log("Loading Categories complete #rows:" .. Categories:getsize())

		kernel.log("Loading ContentRating")
		ContentRating = getfulltickettable( "ContentRating" )
		kernel.log("Loading ContentRating complete #rows:" .. ContentRating:getsize())

		kernel.log("Loading Events")
		Events = getfulltickettable( "Events" )
		kernel.log("Loading Events complete #rows:" .. Events:getsize())

		kernel.log("Loading Productions")
		Productions = getfulltickettable( "Productions" )
		kernel.log("Loading Productions complete #rows:" .. Productions:getsize())

		kernel.log("Loading Venues")
		Venues = getfulltickettable( "Venues" )
		kernel.log("Loading Venues complete #rows:" .. Venues:getsize())

		kernel.log("Loading Version")
		TicketsProfileVersion = getfulltickettable( "Version" )
		kernel.log("Loading Version complete #rows:" .. TicketsProfileVersion:getsize())

		kernel.log("Loading Pricing")
		TicketsProfilePricing = getfulltickettable( "Pricing" )
		kernel.log("Loading Pricing complete #rows:" .. TicketsProfilePricing:getsize())
		
		kernel.log("Loading ProductionPriceCodes")
		ProductionPriceCodes = getfulltickettable( "ProductionPriceCodes" )
		kernel.log("Loading ProductionPriceCodes complete #rows:" .. ProductionPriceCodes:getsize())
		
		kernel.log("Loading ProductionType")
		ProductionType = getfulltickettable( "ProductionType" )
		kernel.log("Loading ProductionType complete #rows:" .. ProductionType:getsize())
		
		kernel.log("Loading IdentifierType")
		IdentifierType = getfulltickettable( "IdentifierType" )
		kernel.log("Loading IdentifierType complete #rows:" .. IdentifierType:getsize())
		
		kernel.log("Loading Attributes")
		Attributes = getfulltickettable( "Attributes" )
		kernel.log("Loading Attributes complete #rows:" .. Attributes:getsize())
		
		kernel.log("Loading ProductionVenues")
		ProductionVenues = getfulltickettable( "ProductionVenues" )
		kernel.log("Loading ProductionVenues complete #rows:" .. ProductionVenues:getsize())
		
		kernel.log("Loading AttributeIdentifierType")
		AttributeIdentifierType = getfulltickettable( "AttributeIdentifierType" )
		kernel.log("Loading AttributeIdentifierType complete #rows:" .. AttributeIdentifierType:getsize())
		
		kernel.log("Loading ProductionPropertyData")
		ProductionPropertyData = getfulltickettable( "ProductionPropertyData" )
		kernel.log("Loading ProductionPropertyData complete #rows:" .. ProductionPropertyData:getsize())
		
		kernel.log("Loading Promotion")
		Promotion = getfulltickettable( "Promotion" )
		kernel.log("Loading Promotion complete #rows:" .. Promotion:getsize())
		
		kernel.log("Loading PromotionIdentifier")
		PromotionIdentifier = getfulltickettable( "PromotionIdentifier" )
		kernel.log("Loading PromotionIdentifier complete #rows:" .. PromotionIdentifier:getsize())
	end
end

function refresh_views()
	kernel.ticketsvcs_refreshviews(tickets_browse_filter_selected, tickets_browse_sort_filter_selected)
	kernel.log("Loading EventToProductions")
	EventToProductions = getfulltickettable( "EventToProductions" )
	kernel.log("Loading EventToProductions complete #rows:" .. EventToProductions:getsize())
	
	tickets_browse_filtered_products = kernel.ticketsvcs_getbrowseevents()
end

function create_tickets_cache_tables()
	load_tables()
	refresh_views()
end