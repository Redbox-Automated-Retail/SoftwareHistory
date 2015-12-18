--
-- load-tables.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- Read-Only tables distributed from Corporate

-- PROFILE.DATA
Stores = getprofiletable( "profile", "Store" )
if DataAccessAlreadySent ~= true then
	Genres = getcacheprofiletable( "profile", "Genres" )
	ProfileVersion = getprofiletable( "profile", "Version" )
	ProductTypes = getcacheprofiletable( "profile", "ProductType" )
	ProductFamilies = getcacheprofiletable( "profile", "ProductFamily" )
	SubPlatform = getcacheprofiletable("profile","SubPlatform")
	MasterProductCatalog = getprofiletable( "profile", "ProductCatalog" )
	ProductRatings=getcacheprofiletable( "profile", "ProductRating" )
	MasterProductCatalog:prime_cache_keys()
	ProductGroup = getprofiletable( "profile", "ProductGroup" )
	TitleRollupXREF = getprofiletable( "profile", "TitleRollupXREF" )
	MultiDiscXREF = getcacheprofiletable( "profile", "MultiDiscXREF" )
else
	Genres = {}
	ProfileVersion = {}
	ProductTypes = {}
	ProductFamilies = {}
	SubPlatform = {}
	MasterProductCatalog = {}
	ProductRatings = {}
	ProductGroup = {}
	TitleRollupXREF = {}
	MultiDiscXREF = {}
end

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
	ProductHistory = getcachedataservicetable( "cache", "ProductHistory" )
	if DataAccessAlreadySent ~= true then
		ProductCatalog = getcachedataservicetable( "cache", "ProductCatalog" )
		BarcodePosition = getcachedataservicetable( "cache", "BarcodePosition" )
		BarcodePosition:prime_cache_keys()
		ProductToBarcode = getcachedataservicetable( "cache", "ProductToBarcode" )
		
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
