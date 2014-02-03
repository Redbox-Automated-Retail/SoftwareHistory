--
-- load-tables.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Read-Only tables distributed from Corporate

-- PROFILE.DATA
Stores = getprofiletable( "profile", "Store" )
if DataAccessAlreadySent ~= true then
	Genres = getprofiletable( "profile", "Genres" )
	ProfileVersion = getprofiletable( "profile", "Version" )
	ProductTypes = getprofiletable( "profile", "ProductType" )
	ProductFamilies = getprofiletable( "profile", "ProductFamily" )
	SubPlatform = getprofiletable("profile","SubPlatform")
	MasterProductCatalog = getprofiletable( "profile", "ProductCatalog" )
	ProductRatings=getprofiletable( "profile", "ProductRating" )
	MasterProductCatalog:prime_cache_keys()
else
	Genres = {}
	ProfileVersion = {}
	ProductTypes = {}
	ProductFamilies = {}
	SubPlatform = {}
	MasterProductCatalog = {}
	ProductRatings = {}
end

-- INVENTORY.DATA
if DataAccessAlreadySent ~= true then
	InventoryVersion = getprofiletable( "inventory", "Version" )
	InventoryVersion:prime_cache_keys()
	BarcodeToProduct = getprofiletable( "inventory", "BarcodeToProduct" )
else
	InventoryVersion = {}
	BarcodeToProduct = {}
end

--INVENTORYTHINS.DATA
if DataAccessAlreadySent ~= true then
	kernel.datastoreregistprovider( "InventoryThins", "CachedProductLookupProvider" )
	RapidThinVersion = getfulltable( "InventoryThins", "Version")
	RapidThinBarcodes = getfulltable( "InventoryThins", "BarcodeToProduct" )
	kernel.log( "RapidThinBarcodes file contains " .. tostring( RapidThinBarcodes:getsize()) .. " barcodes."  )
	local rebalancesunthin = kernel.getmacro( "RunningPath" ) .. "\\..\\Data\\RebalanceUnthin.json.inuse"
	
	local store_number = tonumber(kernel.getmachinevalueordefault(
		"Store",
		"ID",
		"None" ) )
		
	kernel.log( "STORENUMBER = " .. store_number )
	local barcode_list = kernel.tablefromfileserializedlistdictionary( rebalancesunthin, store_number ) or {}
	local temp_list = {}
	for _,v in ipairs( barcode_list ) do
		temp_list[ v["Barcode"] ] = "Known"
	end
	RebalancesUnThinInuse = temp_list
	kernel.log( "RebalanceUnThinFile.JSON.Inuse file contains " .. tostring( #barcode_list ) .. " barcodes."  )
else
	RapidThinVersion = {}
	RapidThinBarcodes = {}
	RebalancesUnThinInuse = {}
end

-- Read-Write tables used by the application

-- LOCAL.DATA
if DataAccessAlreadySent ~= true then
	BadCardList = getworktable( "local", "BadCardList" )
	if DataAccessAlreadySent ~= true then
		Reservations = getworktable( "local", "Reservation" )
		Reservations:prime_cache_keys()
		DuplicateBarcodes = getworktable( "local", "DuplicateBarcodes" )
		RemovedQueue = getworktable( "local", "RemovedQueue" )
		RemovedQueue:prime_cache_keys()
		BreakingNews = getworktable( "local", "BreakingNews" )
		UnplayableProduct = getworktable( "local", "UnplayableProduct" )
		UnplayableProduct:prime_cache_keys()
	else
		Reservations = {}
		DuplicateBarcodes = {}
		RemovedQueue = {}
		BreakingNews = {}
		UnplayableProduct = {}
	end
else 
	BadCardList = {}
	Reservations = {}
	DuplicateBarcodes = {}
	RemovedQueue = {}
	BreakingNews = {}
	UnplayableProduct = {}
end

-- CACHE.DATA
if DataAccessAlreadySent ~= true then
	ProductHistory = getworktable( "cache", "ProductHistory" )
	if DataAccessAlreadySent ~= true then
		ProductCatalog = getworktable( "cache", "ProductCatalog" )
		BarcodePosition = getworktable( "cache", "BarcodePosition" )
		BarcodePosition:prime_cache_keys()
		ProductToBarcode = getworktable( "cache", "ProductToBarcode" )
		FilteredProductList = getworktable( "cache", "FilteredProductList" )
		VendScreenImpressions = getworktable( "cache", "VendScreenImpressions" )
		DuplicateSlotFixes = getworktable( "cache", "DuplicateSlotFixes" )
	else
		ProductCatalog = {}
		BarcodePosition = {}
		ProductToBarcode = {}
		FilteredProductList = {}
		VendScreenImpressions = {}
		DuplicateSlotFixes = {}
	end
else
	ProductHistory = {}
	ProductCatalog = {}
	BarcodePosition = {}
	ProductToBarcode = {}
	FilteredProductList = {}
	VendScreenImpressions = {}
	DuplicateSlotFixes = {}
end
