using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

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

	ErrorList GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, List<long> include, List<long> exclude, bool isADAView, out List<IKioskProduct> products);

	ErrorList GetTitleRollupProduct(long productGroupId, out IKioskProduct kioskProduct);

	ErrorList GetCarouselProducts(bool showCollections, out List<IKioskProduct> products);

	ErrorList GetCollection(long collectionId, out ICollectionProduct collection);

	ErrorList GetCollectionProducts(out List<IKioskProduct> products);

	bool GetIsProductThinned(long productId);

	int GetInStockGameCount();
}
