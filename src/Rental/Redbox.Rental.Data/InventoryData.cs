using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;
using Redbox.Rental.Model;

namespace Redbox.Rental.Data;

public class InventoryData : IInventoryData
{
	private InventoryActionFile _inventoryActionFile;

	private ProductLookupCatalogModel _plcModel;

	private InventoryDataJsonFile _inventoryDataJsonFile;

	private DateTime? _inventoryActionModificationTime;

	private DateTime? _rapidStatusModificationTime;

	private IKernelHelperService _kernelHelperService;

	private string _runningPath;

	private string _dataPath;

	private string _rapidStatusStagedPath;

	private string _rapidStatusActivePath;

	private string _inventoryDataPath;

	private const string InventoryDataName = "inventory.data";

	private const string RapidStatusStagedName = "RapidStatus.data";

	private const string RapidStatusActiveName = "RapidStatus.active";

	private string _inventoryActionStagedPath;

	private string _inventoryActionActivePath;

	private const string InventoryActionStagedName = "InventoryAction.data";

	private const string InventoryActionActiveName = "InventoryAction.active";

	private string _inventoryDataJsonPath;

	private const string InventoryDataJsonName = "InventoryData.json";

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public InventoryData(IKernelHelperService kernelHelperService)
	{
		_kernelHelperService = kernelHelperService;
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		_runningPath = service.RunningPath;
		_dataPath = service.DataPath;
		_rapidStatusStagedPath = Path.Combine(_dataPath, "RapidStatus.data");
		_rapidStatusActivePath = Path.Combine(_dataPath, "RapidStatus.active");
		_inventoryActionStagedPath = Path.Combine(_dataPath, "InventoryAction.data");
		_inventoryActionActivePath = Path.Combine(_dataPath, "InventoryAction.active");
		_inventoryDataJsonPath = Path.Combine(_dataPath, "InventoryData.json");
		_inventoryDataPath = Path.Combine(_dataPath, "inventory.data");
	}

	public void Initialize()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.Initialize", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LoadInventoryData();
			LoadRapidStatus();
			LoadInventoryAction();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.Initialize - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void CheckForNewFiles(out bool rapidStatusChanged)
	{
		rapidStatusChanged = false;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.CheckForNewFile", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			rapidStatusChanged = CheckForRapidStatusStagedFile();
			CheckForInventoryActionStagedFile();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.CheckForNewFile - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public long GetInventoryDataCount()
	{
		long result = 0L;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataCount", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.Count;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataCount - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public string GetInventoryDataMachineOrigin()
	{
		string result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataMachineOrigin", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.GetOriginMachine();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataMachineOrigin - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public string GetInventoryDataCreatedOn()
	{
		string result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataGeneratedOn", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.GetCreatedOn();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataGeneratedOn - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.Inventory GetInventory(string barcode)
	{
		Redbox.KioskEngine.ComponentModel.Inventory value = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventory", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				GetInventoriesAlreadyLocked(new List<string> { barcode }).TryGetValue(barcode, out value);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventory - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return value;
	}

	public Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventories(List<string> barcodes)
	{
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventories", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				result = GetInventoriesAlreadyLocked(barcodes);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventories - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	private Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventoriesAlreadyLocked(List<string> barcodes)
	{
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> dictionary = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		List<string> list = new List<string>();
		CheckForRapidStatusStagedFile();
		if (_plcModel != null)
		{
			foreach (string barcode in barcodes)
			{
				_plcModel.Barcodes.TryGetValue(barcode, out var value);
				if (value != null)
				{
					dictionary[barcode] = value;
				}
				else
				{
					list.Add(barcode);
				}
			}
		}
		else
		{
			list.AddRange(barcodes);
		}
		if (list.Count > 0)
		{
			foreach (KeyValuePair<string, Redbox.KioskEngine.ComponentModel.Inventory> item in GetInventoriesFromInventoryDataFile(list))
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	private Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventoriesFromInventoryDataFile(List<string> barcodes)
	{
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> dictionary = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		if (File.Exists(_inventoryDataPath))
		{
			using Archive archive = Archive.Open(_inventoryDataPath);
			foreach (string barcode in barcodes)
			{
				Redbox.ProductLookupCatalog.Inventory inventory = archive.Find(barcode);
				if (inventory != null)
				{
					dictionary[barcode] = new Redbox.KioskEngine.ComponentModel.Inventory
					{
						Barcode = barcode,
						Code = (Redbox.KioskEngine.ComponentModel.InventoryStatusCode)inventory.Code,
						TitleId = inventory.TitleId,
						TotalRentalCount = inventory.TotalRentalCount
					};
				}
			}
		}
		return dictionary;
	}

	public Redbox.KioskEngine.ComponentModel.Inventory GetRapidStatus(string barcode)
	{
		new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatus", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			CheckForRapidStatusStagedFile();
			if (_plcModel == null)
			{
				return null;
			}
			if (_plcModel.Barcodes.TryGetValue(barcode, out var value))
			{
				return value;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatus - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public ProductLookupCatalogModel GetRapidStatus()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatus", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			CheckForRapidStatusStagedFile();
			return _plcModel;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatus - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public InventoryActionFile GetInventoryActionFile()
	{
		InventoryActionFile result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryActionData", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			CheckForInventoryActionStagedFile();
			return _inventoryActionFile;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionData - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetLastCompletedDate()
	{
		DateTime? result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetLastCompletedDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				return null;
			}
			return _inventoryDataJsonFile.LastCompletedDate;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionData - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetLastRunThinJobDate()
	{
		DateTime? result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetLastRunThinJobDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				return null;
			}
			return _inventoryDataJsonFile.LastRunThinJobDate;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetLastRunThinJobDate - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SetLastCompletedDate(DateTime dt)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.SetLastCompletedDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				_inventoryDataJsonFile = new InventoryDataJsonFile();
			}
			_inventoryDataJsonFile.LastCompletedDate = dt;
			WriteInventoryData();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.SetLastCompletedDate - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SetLastRunThinJobDate(DateTime dt)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.SetLastRunThinJobDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				_inventoryDataJsonFile = new InventoryDataJsonFile();
			}
			_inventoryDataJsonFile.LastRunThinJobDate = dt;
			WriteInventoryData();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.SetLastRunThinJobDate - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetInventoryActionModificationTime()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryActionModificationTime", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			return _inventoryActionModificationTime;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionModificationTime - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public DateTime? GetRapidStatusModificationTime()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatusModificationTime", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			return _rapidStatusModificationTime;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatusModificationTime - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	private bool CheckForRapidStatusStagedFile()
	{
		if (File.Exists(_rapidStatusStagedPath))
		{
			LogHelper.Instance.Log("InventoryData.CheckForRapidStatusStagedFile - found staged rapid status file, activating.");
			if (File.Exists(_rapidStatusActivePath))
			{
				File.Delete(_rapidStatusActivePath);
			}
			File.Move(_rapidStatusStagedPath, _rapidStatusActivePath);
			LoadRapidStatus();
			return true;
		}
		return false;
	}

	private void CheckForInventoryActionStagedFile()
	{
		if (File.Exists(_inventoryActionStagedPath))
		{
			LogHelper.Instance.Log("InventoryData.CheckForInventoryActionStagedFile - found staged inventory action file, activating.");
			if (File.Exists(_inventoryActionActivePath))
			{
				File.Delete(_inventoryActionActivePath);
			}
			File.Move(_inventoryActionStagedPath, _inventoryActionActivePath);
			LoadInventoryAction();
			if (_kernelHelperService != null)
			{
				_kernelHelperService.EnqueueStatistic("InventoryActionData", File.GetLastWriteTimeUtc(_inventoryActionActivePath).ToString("yyyy/MM/dd HH:mm:ss"));
			}
		}
	}

	private void WriteInventoryData()
	{
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				LogHelper.Instance.Log("InventoryData.WriteInventoryData - no data to write");
				return;
			}
			File.WriteAllBytes(_inventoryDataJsonPath, Encoding.ASCII.GetBytes(_inventoryDataJsonFile.ToJson()));
			LogHelper.Instance.Log("InventoryData.WriteInventoryData - Writing {0}.", _inventoryDataJsonFile.ToJson());
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryData - An unhandled exception occurred", e);
			_inventoryDataJsonFile = null;
		}
	}

	private void LoadInventoryData()
	{
		_inventoryDataJsonFile = null;
		try
		{
			if (!File.Exists(_inventoryDataJsonPath))
			{
				LogHelper.Instance.Log("InventoryData.LoadInventoryData - InventoryData file ({0}) does not exist", _inventoryDataJsonPath);
			}
			else
			{
				_inventoryDataJsonFile = Encoding.ASCII.GetString(File.ReadAllBytes(_inventoryDataJsonPath)).ToObject<InventoryDataJsonFile>();
				LogHelper.Instance.Log("InventoryData.LoadInventoryData - InventoryData file contains LastCompletedDate {0}.", _inventoryDataJsonFile.LastCompletedDate);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryData - An unhandled exception occurred", e);
			_inventoryDataJsonFile = null;
			SafeDelete(_inventoryDataJsonPath);
		}
	}

	private void LoadRapidStatus()
	{
		try
		{
			_plcModel = null;
			_rapidStatusModificationTime = null;
			if (!File.Exists(_rapidStatusActivePath))
			{
				LogHelper.Instance.Log("InventoryData.LoadRapidStatus - RapidStatus file ({0}) does not exist", _rapidStatusActivePath);
				return;
			}
			ProductLookupCatalogModel productLookupCatalog = ServiceLocator.Instance.GetService<IProductLookupCatalogService>().GetProductLookupCatalog(_rapidStatusActivePath);
			if (productLookupCatalog == null)
			{
				LogHelper.Instance.LogError("InventoryData.LoadRapidStatus", "An error occurred loading file. Deleting file.");
				SafeDelete(_rapidStatusActivePath);
			}
			_plcModel = productLookupCatalog;
			_rapidStatusModificationTime = File.GetLastWriteTime(_rapidStatusActivePath);
			LogHelper.Instance.Log("InventoryData.LoadRapidStatus - Rapid status file contains {0} barcodes.", productLookupCatalog.Barcodes.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadRapidStatus - An unhandled exception occurred", e);
		}
	}

	private void LoadInventoryAction()
	{
		_inventoryActionFile = null;
		_inventoryActionModificationTime = null;
		try
		{
			if (!File.Exists(_inventoryActionActivePath))
			{
				LogHelper.Instance.Log("InventoryData.LoadInventoryAction - InventoryAction file ({0}) does not exist", _inventoryActionActivePath);
				return;
			}
			_inventoryActionFile = Encoding.ASCII.GetString(File.ReadAllBytes(_inventoryActionActivePath)).ToObject<InventoryActionFile>();
			_inventoryActionModificationTime = File.GetLastWriteTime(_inventoryActionActivePath);
			LogHelper.Instance.Log("InventoryData.LoadInventoryAction - InventoryAction file contains {0} rebalance titles, Completed Date {1}, Fill Quantity {2}.", _inventoryActionFile.RebalancesIn.Count, _inventoryActionFile.CompletedDate, _inventoryActionFile.FillQty);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryAction - An unhandled exception occurred", e);
			_inventoryActionFile = null;
			SafeDelete(_inventoryActionActivePath);
		}
	}

	private void SafeDelete(string fileName)
	{
		try
		{
			if (File.Exists(fileName))
			{
				LogHelper.Instance.Log("InventoryData.SafeDelete - file ({0})", fileName);
				File.Delete(fileName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.SafeDelete - file ({fileName})", e);
		}
	}
}
