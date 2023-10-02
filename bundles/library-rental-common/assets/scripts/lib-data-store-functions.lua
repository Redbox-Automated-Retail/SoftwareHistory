--
-- lib-data-store-functions.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function getcachedataservicetable(store, name)
  return getdataservicetable( store, name, false ) 
end

function getinventorytable( store, name )
	return getworktable( store, name, true )
end

function getdataservicetable( store, name, read_only )
	kernel.log("lib-data-store-functions.getdataservicetable(" .. store .. ", " .. name .. ", " .. tostring(read_only ))
	
	local mt = {}
	read_only = read_only or false
	mt.__index = function( vtable, key )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		local table_name = rawget( vtable, "table_name" )
		local data_store_name = rawget( vtable, "data_store_name" )
		if cache[key] == nil then
			--kernel.log( "cache miss for data_store_name = " .. data_store_name .. ", table_name = " .. table_name .. " key = " .. key )
			local value = kernel.dataserviceget(data_store_name, table_name, key )
			if value ~= nil then
				local chunk = loadstring( "return " .. value )
				if chunk ~= nil then
					cache[key] = chunk()
				else
					kernel.log( "WARNING! invalid lua: " .. value )
				end
			else
				-- TBD - DO SOMETHING HERE...(Gray Screen Issue)
			end
		end
		return cache[key]
	end

	if read_only == true then
		mt.__newindex = function( vtable, key, value )
			if key == nil then
				return
			end
			key = tostring( key )
			local cache = rawget( vtable, "cache" )
			if type( value ) == "function" then
				cache[key] = value
				return
			end
			error( "profile tables (" ..
				rawget( vtable, "table_name" ) .. ", " ..
				tostring( key ) .. ":" ..
				tostring( value ) .. ") do not support adding new keys" )
		end
	else
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
				kernel.dataserviceset(
					rawget( vtable, "data_store_name" ),
					rawget( vtable, "table_name" ),
					key,
					value )
			end
		end
	end

	local t = {}
	t.read_only = read_only
	t.data_store_name = store
	t.table_name = name
	t.cache = {}
	t.reset_keys_cache = function( self )
		local cache = rawget( self, "cache" )
		cache.keys = {}
	end
	t.close = function( self )
		kernel.dataserviceclose( rawget( self, "data_store_name" ) )
	end
	t.getkeys = function( self )
		return kernel.dataservicegetkeys(
			rawget( self, "data_store_name" ),
			rawget( self, "table_name" ) )
	end
	t.getsize = function( self )
		return kernel.dataservicecount(
			rawget( self, "data_store_name" ),
			rawget( self, "table_name" ) )
	end

	if read_only == false then
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
			kernel.dataserviceremove(
				rawget( self, "data_store_name" ),
				rawget( self, "table_name" ),
				key )
		end
		t.clear = function( self )
			rawset( self, "cache", {} )
			kernel.dataserviceclear(
				rawget( self, "data_store_name" ),
				rawget( self, "table_name" ) )
		end
	end

	t.prime_cache_keys = function( self )
		local cache = rawget( self, "cache" )
		local table_name = rawget( self, "table_name" )
		if cache.keys == nil
		or #cache.keys == 0 then
			cache.keys = self:getkeys()
			kernel.log( "loading cache keys from database for table " .. table_name  )
		end
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
            if cache ~= nil and cache.keys ~= nil then
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
			else
				return nil
			end
		end
		return pairs_it, self
	end
	
	t.getall = function ( self )
		all = kernel.dataservicegetall( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
		
		for i, v in ipairs( all.keys ) do
		
			if all[v] ~= nil then
				local chunk = loadstring( "return " .. all[v] )
				if (chunk ~= nil) then
					all[v] = chunk()
					--kernel.log("key: " .. v .. " type: " .. type( all[v]))
					if (type( all[v] ) ~= "table") then
						kernel.log( "WARNING! invalid lua (should have coverted to table): " .. all[v] )
						all[v] = nil
					end
				else
					kernel.log( "WARNING! invalid lua: " .. all[v] )
					all[v] = nil
				end
			end			

		end

		rawset( self, "cache", all)
		return nil
	end	

	setmetatable( t, mt )

	return t
end

function getdataservicecachetable( store, name )
	kernel.log("lib-data-store-functions.getdataservicecachetable(" .. store .. ", " .. name .. ", " .. tostring(true))

	local mt = {}
	
	mt.__index = function( vtable, key )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		local table_name = rawget( vtable, "table_name" )
		local data_store_name = rawget( vtable, "data_store_name" )
		
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
	t.data_store_name = store
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
	
	
	
	t.getall = function ( self )
		--all = kernel.datastoregetall( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
		all = kernel.dataservicegetall( rawget( self, "data_store_name" ), rawget( self, "table_name" ))		
		
		for i, v in ipairs( all.keys ) do
		
			if all[v] ~= nil then
				local chunk = loadstring( "return " .. all[v] )
				if (chunk ~= nil) then
					all[v] = chunk()
					--kernel.log("key: " .. v .. " type: " .. type( all[v]))
					if (type( all[v] ) ~= "table") then
						kernel.log( "WARNING! invalid lua (should have coverted to table): " .. all[v] )
						all[v] = nil
					end
				else
					kernel.log( "WARNING! invalid lua: " .. all[v] )
					all[v] = nil
				end
			end			

		end

		rawset( self, "cache", all)
		return nil
	end

	setmetatable( t, mt )

	return t
end

function getworktable( store, name, read_only )
	kernel.log("lib-data-store-functions.getworktable(" .. store .. ", " .. name .. ", " .. tostring(read_only ))
	
	local mt = {}
	read_only = read_only or false
	mt.__index = function( vtable, key )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		local table_name = rawget( vtable, "table_name" )
		local data_store_name = rawget( vtable, "data_store_name" )
		if cache[key] == nil then
			--kernel.log( "cache miss for data_store_name = " .. data_store_name .. ", table_name = " .. table_name .. " key = " .. key )
			local value = kernel.datastoreget(
				data_store_name,
				table_name,
				key )
			if value ~= nil then
				local chunk = loadstring( "return " .. value )
				if chunk ~= nil then
					cache[key] = chunk()
				else
					kernel.log( "WARNING! invalid lua: " .. value )
				end
			else
				-- TBD - DO SOMETHING HERE...(Gray Screen Issue)
			end
		end
		return cache[key]
	end

	if read_only == true then
		mt.__newindex = function( vtable, key, value )
			if key == nil then
				return
			end
			key = tostring( key )
			local cache = rawget( vtable, "cache" )
			if type( value ) == "function" then
				cache[key] = value
				return
			end
			error( "profile tables (" ..
				rawget( vtable, "table_name" ) .. ", " ..
				tostring( key ) .. ":" ..
				tostring( value ) .. ") do not support adding new keys" )
		end
	else
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
				kernel.datastoreset(
					rawget( vtable, "data_store_name" ),
					rawget( vtable, "table_name" ),
					key,
					value )
			end
		end
	end

	local t = {}
	t.read_only = read_only
	t.data_store_name = store
	t.table_name = name
	t.cache = {}
	t.reset_keys_cache = function( self )
		local cache = rawget( self, "cache" )
		cache.keys = {}
	end
	t.close = function( self )
		kernel.datastoreclose( rawget( self, "data_store_name" ) )
	end
	t.getkeys = function( self )
		return kernel.datastoregetkeys(
			rawget( self, "data_store_name" ),
			rawget( self, "table_name" ) )
	end
	t.getsize = function( self )
		return kernel.datastorecount(
			rawget( self, "data_store_name" ),
			rawget( self, "table_name" ) )
	end

	if read_only == false then
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
			kernel.datastoreremove(
				rawget( self, "data_store_name" ),
				rawget( self, "table_name" ),
				key )
		end
		t.clear = function( self )
			rawset( self, "cache", {} )
			kernel.datastoreclear(
				rawget( self, "data_store_name" ),
				rawget( self, "table_name" ) )
		end
	end

	t.prime_cache_keys = function( self )
		local cache = rawget( self, "cache" )
		local table_name = rawget( self, "table_name" )
		if cache.keys == nil
		or #cache.keys == 0 then
			cache.keys = self:getkeys()
			kernel.log( "loading cache keys from database for table " .. table_name  )
		end
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
	
	t.getall = function ( self )
		all = kernel.datastoregetall( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
		
		for i, v in ipairs( all.keys ) do
		
			if all[v] ~= nil then
				local chunk = loadstring( "return " .. all[v] )
				if (chunk ~= nil) then
					all[v] = chunk()
					--kernel.log("key: " .. v .. " type: " .. type( all[v]))
					if (type( all[v] ) ~= "table") then
						kernel.log( "WARNING! invalid lua (should have coverted to table): " .. all[v] )
						all[v] = nil
					end
				else
					kernel.log( "WARNING! invalid lua: " .. all[v] )
					all[v] = nil
				end
			end			

		end

		rawset( self, "cache", all)
		return nil
	end	

	setmetatable( t, mt )

	return t
end

function getfulltable( store, name )
	local table = getcachetable( store, name )
	table:getall()
	return table
end

function getcachetable( store, name )
	local mt = {}
	
	mt.__index = function( vtable, key )
		if key == nil then
			return
		end
		key = tostring( key )
		local cache = rawget( vtable, "cache" )
		local table_name = rawget( vtable, "table_name" )
		local data_store_name = rawget( vtable, "data_store_name" )
		
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
	t.data_store_name = store
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
		return kernel.datastoregetkeys( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
	end	
	
	t.getall = function ( self )
		all = kernel.datastoregetall( rawget( self, "data_store_name" ), rawget( self, "table_name" ))
		
		for i, v in ipairs( all.keys ) do
		
			if all[v] ~= nil then
				local chunk = loadstring( "return " .. all[v] )
				if (chunk ~= nil) then
					all[v] = chunk()
					--kernel.log("key: " .. v .. " type: " .. type( all[v]))
					if (type( all[v] ) ~= "table") then
						kernel.log( "WARNING! invalid lua (should have coverted to table): " .. all[v] )
						all[v] = nil
					end
				else
					kernel.log( "WARNING! invalid lua: " .. all[v] )
					all[v] = nil
				end
			end			

		end

		rawset( self, "cache", all)
		return nil
	end

	setmetatable( t, mt )

	return t
end
