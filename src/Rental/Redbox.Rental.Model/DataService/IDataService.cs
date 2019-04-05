using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Model.DataService;

public interface IDataService
{
	ErrorList Initialize();

	ErrorList Stop();

	ErrorList Start();

	ErrorList GetLuaTableData(string tableName, out IProfileLuaTable data);

	byte[] GetImage(string name);

	ErrorList CacheTablesUpdate(string tableName, string key, object data);

	ErrorList CacheTablesRemove(string tableName, string key);

	bool RefreshIfNeeded();

	ErrorList GetProduct(long productId, bool excludeTitleRollup, out IKioskProduct kioskProduct);

	ErrorList GetProductByBarcode(string barcode, out IKioskProduct kioskProduct);

	ErrorList GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, List<long> include, List<long> exclude, bool isADAView, decimal? priceRangeLow, decimal? priceRangeHigh, out List<IKioskProduct> products);

	ErrorList GetUnfilteredNonTitleRollupProducts(out IEnumerable<IKioskProduct> products);

	ErrorList GetOutOfStockMovieProducts(out IEnumerable<ITitleProduct> products);

	ErrorList GetTitleRollupProduct(long productGroupId, out IKioskProduct kioskProduct);

	ErrorList GetCarouselProducts(bool showCollections, out List<IKioskProduct> products);

	ErrorList GetCollection(long collectionId, out ICollectionProduct collection);

	ErrorList GetCollectionProducts(out List<IKioskProduct> products);

	bool GetIsProductThinned(long productId);

	int GetInStockGameCount();

	Store GetStore();

	List<SubPlatform> GetSubPlatforms();

	List<ProductType> GetProductTypes();

	List<Review> GetReviews(long titleId);

	bool IsAccountOnOfflineBadCardList(string hashId);

	void UpdateAccountOnOfflineBadCardList(string hashId);

	void RemoveAccountFromOfflineBadCardList(string hashId);

	TitleType? GetProductTitleType(long productId);

	List<IReservation> GetReservations(string hashedCardId);

	List<IReservation> GetAllReservations();

	bool SaveReservation(IReservation reservation);

	void DeleteReservation(string referenceNumber);

	IReservation GetReservation(string referenceNumber);

	List<IBarcodeForProductResult> GetBarcodesForProducts(List<ISearchBarcodeForProduct> searchProducts, List<string> excludeBarcodesList);

	bool IsMultiDisc(long productId);

	List<string> GetAllUnmatchedMultiDiscBarcodes();
}
