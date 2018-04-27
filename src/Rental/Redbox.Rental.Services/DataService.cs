using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services;

public class DataService : IDataService
{
	private IKernelService _kernelService;

	private IProfileData _profileData;

	private ICacheData _cacheData;

	private IKioskData _kioskData;

	private IEngineApplication _engine;

	private string _runningPath;

	private string _dataPath;

	private ManualResetEvent _manualResetEvent;

	private bool IsFirstTime = true;

	private IProfileDataInstance _profileStagedInstance;

	private IDataServiceInstance _dataServiceInstance;

	private KioskProductView _kioskProductView;

	private IKioskDataInstance _kioskDataStagedInstance;

	private int _inRefresh;

	private Timer _changeTimer;

	private readonly object _lock = new object();

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	private bool HasProfileStagedInstance => _profileStagedInstance != null;

	private bool HasKioskDataStagedInstance => _kioskDataStagedInstance != null;

	public DataService(IKernelService kernelService, IDataServiceInstance dataServiceInstance, IProfileData profileData, ICacheData cacheData, IKioskData kioskData, IEngineApplication engine)
	{
		_kernelService = kernelService;
		_dataServiceInstance = dataServiceInstance;
		_profileData = profileData;
		_cacheData = cacheData;
		_kioskData = kioskData;
		_engine = engine;
		_runningPath = engine.RunningPath;
		_dataPath = engine.DataPath;
	}

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("DataService initialization");
			_manualResetEvent = new ManualResetEvent(initialState: false);
			ThreadPool.QueueUserWorkItem(delegate
			{
				Refresh();
			});
			if (_changeTimer == null)
			{
				_changeTimer = new Timer(delegate
				{
					Refresh();
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.Initialize", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("DataService start");
			_changeTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 120));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.Start", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Profile service stop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("ProfileService.Stop", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool RefreshIfNeeded()
	{
		WaitForFirstLoad();
		bool result = false;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (HasProfileStagedInstance || HasKioskDataStagedInstance)
				{
					UpdateAndRefreshView();
					return true;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("ProfileService.RefreshIfNeeded", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return result;
	}

	public ErrorList GetLuaTableData(string tableName, out IProfileLuaTable data)
	{
		WaitForFirstLoad();
		data = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (!_dataServiceInstance.ProfileDataInstance.ProfileLuaView.Tables.ContainsKey(tableName))
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetLuaTableData", "Unhandled exception", $"Table: {tableName} does not exist in collection."));
				}
				data = _dataServiceInstance.ProfileDataInstance.ProfileLuaView.Tables[tableName];
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetLuaTableData", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public byte[] GetImage(string name)
	{
		try
		{
			lock (_lock)
			{
				string path = Path.Combine(_dataServiceInstance.ProfileDataInstance.ImagesPath, name);
				if (File.Exists(path))
				{
					return File.ReadAllBytes(path);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("DataService.GetImage - Unhandled exception occured", e);
		}
		return null;
	}

	public ErrorList CacheTablesUpdate(string tableName, string key, object data)
	{
		WaitForFirstLoad();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				bool isNew = false;
				if (tableName.Equals("barcodeposition", StringComparison.CurrentCultureIgnoreCase))
				{
					BarcodeProduct barcodeProduct = _cacheData.UpdateBarcodePosition(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new barcode position was added for barcode {barcodeProduct.Barcode} position {barcodeProduct.Position}");
					}
				}
				else if (tableName.Equals("producthistory", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache = _cacheData.UpdateProductHistory(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product history was added for title {titleCache.ProductId} ");
						_kioskProductView.LoadView(titleCache);
					}
				}
				else if (tableName.Equals("productcatalog", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache2 = _cacheData.UpdateProductCatalog(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product catalog was added for title {titleCache2.ProductId} ");
						_kioskProductView.LoadView(titleCache2);
					}
				}
				else if (tableName.Equals("producttobarcode", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache3 = _cacheData.UpdateProductToBarcode(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product to barcode was added for title {titleCache3.ProductId} ");
						_kioskProductView.LoadView(titleCache3);
					}
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.CacheTablesUpdate", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList CacheTablesRemove(string tableName, string key)
	{
		WaitForFirstLoad();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (tableName.Equals("barcodeposition", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveBarcodePosition(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("producthistory", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductHistory(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("productcatalog", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductCatalog(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("producttobarcode", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductToBarcode(_dataServiceInstance.CacheDataInstance, key);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.CacheTablesRemove", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetTitleRollupProduct(long productGroupId, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetTitleRollupProduct(productGroupId);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetTitleRollupForProduct", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetProduct(long productId, bool excludeTitleRollup, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetProduct(productId, excludeTitleRollup);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProduct", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetProductByBarcode(string barcode, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetProductByBarcode(barcode);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProductByBarcode", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, List<long> include, List<long> exclude, bool isADAView, out List<IKioskProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetProducts(browseFilter, titleFamily, titleType, browseSort, genreFilter, include, exclude, isADAView);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetCarouselProducts(bool showCollections, out List<IKioskProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetCarouselProducts(showCollections);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetCollection(long collectionId, out ICollectionProduct collection)
	{
		collection = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				collection = _kioskProductView.GetCollection(collectionId);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetCollectionProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetCollectionProducts(out List<IKioskProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetCollectionProducts();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetCollectionProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool GetIsProductThinned(long productId)
	{
		bool result = false;
		PerformActionWhileLocked(delegate
		{
			TitleCache value = null;
			if (_dataServiceInstance != null && _dataServiceInstance.CacheDataInstance != null && _dataServiceInstance.CacheDataInstance.Titles.TryGetValue(productId, out value) && value != null)
			{
				foreach (string key in value.BarcodeProducts.Keys)
				{
					if (value.BarcodeProducts[key] != null && value.BarcodeProducts[key].Status == "Thinned")
					{
						result = true;
					}
				}
			}
		});
		return result;
	}

	public int GetInStockGameCount()
	{
		return _kioskProductView?.GetInStockGameCount() ?? 0;
	}

	public List<SubPlatform> GetSubPlatforms()
	{
		List<SubPlatform> list = new List<SubPlatform>();
		if (_dataServiceInstance?.ProfileDataInstance?.SubPlatforms != null)
		{
			foreach (KeyValuePair<long, SubPlatform> subPlatform in _dataServiceInstance.ProfileDataInstance.SubPlatforms)
			{
				list.Add(subPlatform.Value);
			}
		}
		return list;
	}

	public List<ProductType> GetProductTypes()
	{
		List<ProductType> list = new List<ProductType>();
		if (_dataServiceInstance?.ProfileDataInstance?.ProductTypes != null)
		{
			foreach (KeyValuePair<long, ProductType> productType in _dataServiceInstance.ProfileDataInstance.ProductTypes)
			{
				list.Add(productType.Value);
			}
		}
		return list;
	}

	public Store GetStore()
	{
		long kioskId = ServiceLocator.Instance.GetService<IStoreManager>().KioskId;
		Store store;
		if (_dataServiceInstance.ProfileDataInstance.Stores.ContainsKey(kioskId))
		{
			store = _dataServiceInstance.ProfileDataInstance.Stores[kioskId];
		}
		else
		{
			store = new Store
			{
				StoreId = 0L
			};
			store.Sellthru = true;
			store.PurchasePrice = 5m;
			store.State = "IL";
			store.PurchaseTaxRate = 8m;
			store.RentalTaxRate = 8m;
			store.ServiceFee = 0.1m;
			store.ServiceFeeTaxRate = 8m;
			LogHelper.Instance.LogError("DataService.RefreshView", "Store is not set in the registry.");
		}
		return store;
	}

	private ErrorList PerformActionWhileLocked(Action action)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				action();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.PerformActionWhileLocked", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private void WaitForFirstLoad()
	{
		if (_manualResetEvent == null)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("DataService wait for initial data refresh");
			_manualResetEvent.WaitOne(new TimeSpan(0, 15, 0));
			_manualResetEvent.Close();
			_manualResetEvent = null;
			LogHelper.Instance.Log("DataService finished waiting for initial data refresh");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DataService.WaitForFirstLoad", "Unhandled exception occurred", ex);
		}
		finally
		{
			_manualResetEvent = null;
		}
	}

	private void Refresh()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("DataService Refresh - exiting already in refresh");
			return;
		}
		try
		{
			RefreshData();
		}
		finally
		{
			_inRefresh = 0;
			if (_manualResetEvent != null)
			{
				_manualResetEvent.Set();
			}
			if (IsFirstTime)
			{
				IsFirstTime = false;
			}
		}
	}

	private void RefreshData()
	{
		if (IsFirstTime)
		{
			_dataServiceInstance.CacheDataInstance = _cacheData.LoadData();
		}
		IProfileDataInstance profileDataInstance = null;
		if (_profileData.HasStaged() || IsFirstTime)
		{
			_profileData.Activate();
			profileDataInstance = _profileData.LoadDataInstance();
		}
		IKioskDataInstance kioskDataInstance = null;
		if (_kioskData.HasStaged() || IsFirstTime)
		{
			kioskDataInstance = _kioskData.GetData();
		}
		if (profileDataInstance == null && kioskDataInstance == null)
		{
			return;
		}
		LogHelper.Instance.Log("DataService RefreshData");
		try
		{
			lock (_lock)
			{
				if (profileDataInstance != null)
				{
					_profileStagedInstance = profileDataInstance;
				}
				if (kioskDataInstance != null)
				{
					_kioskDataStagedInstance = kioskDataInstance;
				}
				if (IsFirstTime)
				{
					LogHelper.Instance.Log("DataService first time refresh if needed");
					UpdateAndRefreshView();
				}
			}
			LogHelper.Instance.Log("DataService RefreshData complete");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DataService.RefreshData", "Unhandled exception occurred", ex);
		}
	}

	private void UpdateAndRefreshView()
	{
		if (HasProfileStagedInstance || HasKioskDataStagedInstance)
		{
			if (HasProfileStagedInstance)
			{
				LogHelper.Instance.Log("DataService.UpdateAndRefreshView - Has profile staged instance, updating.");
				_dataServiceInstance.ProfileDataInstance = _profileStagedInstance;
				_profileStagedInstance = null;
			}
			if (HasKioskDataStagedInstance)
			{
				LogHelper.Instance.Log("DataService.UpdateAndRefreshView - Has kioskdata staged instance, updating.");
				_dataServiceInstance.KioskDataInstance = _kioskDataStagedInstance;
				_kioskDataStagedInstance = null;
			}
			RefreshView();
		}
	}

	private void RefreshView()
	{
		Store store = GetStore();
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		service.SellThru = store.Sellthru;
		service.PurchasePrice = store.PurchasePrice;
		_kioskProductView = new KioskProductView(store, _dataServiceInstance.ProfileDataInstance, _dataServiceInstance.CacheDataInstance, _dataServiceInstance.KioskDataInstance, service);
	}
}
