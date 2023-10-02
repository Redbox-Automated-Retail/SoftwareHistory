--
-- load-tables.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Read-Only tables distributed from Corporate

-- INVENTORY.DATA
if DataAccessAlreadySent ~= true then
	InventoryVersion = getinventorytable( "inventory", "Version" )
	InventoryVersion:prime_cache_keys()
	BarcodeToProduct = getinventorytable( "inventory", "BarcodeToProduct" )
else
	InventoryVersion = {}
	BarcodeToProduct = {}
end

-- Read-Write tables used by the application

-- LOCAL.DATA
if DataAccessAlreadySent ~= true then
	BadCardList = getworktable( "local", "BadCardList" )
	if DataAccessAlreadySent ~= true then

		DuplicateBarcodes = getworktable( "local", "DuplicateBarcodes" )
		RemovedQueue = getworktable( "local", "RemovedQueue" )
		RemovedQueue:prime_cache_keys()
		BreakingNews = getworktable( "local", "BreakingNews" )
        BarcodeStatus = getworktable( "local", "BarcodeStatus" )
        BarcodeStatus:prime_cache_keys()
	else
		DuplicateBarcodes = {}
		RemovedQueue = {}
		BreakingNews = {}
        BarcodeStatus = {}
	end
else 
	BadCardList = {}
	DuplicateBarcodes = {}
	RemovedQueue = {}
	BreakingNews = {}
    BarcodeStatus = {}
end

-- CACHE.DATA
if DataAccessAlreadySent ~= true then
	ProductHistory = getcachedataservicetable( "cache", "ProductHistory" )
	if DataAccessAlreadySent ~= true then
		ProductCatalog = getcachedataservicetable( "cache", "ProductCatalog" )
		BarcodePosition = getcachedataservicetable( "cache", "BarcodePosition" )
		BarcodePosition:prime_cache_keys()
		ProductToBarcode = getcachedataservicetable( "cache", "ProductToBarcode" )
		
		FilteredProductList = getworktable( "cache", "FilteredProductList" )
		DuplicateSlotFixes = getworktable( "cache", "DuplicateSlotFixes" )
	else
		ProductCatalog = {}
		BarcodePosition = {}
		ProductToBarcode = {}
		FilteredProductList = {}
		DuplicateSlotFixes = {}
	end
else
	ProductHistory = {}
	ProductCatalog = {}
	BarcodePosition = {}
	ProductToBarcode = {}
	FilteredProductList = {}
	DuplicateSlotFixes = {}
end