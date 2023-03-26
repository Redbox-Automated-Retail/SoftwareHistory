using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Transaction;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services;

public class FraudService : IFraudService
{
	private const int _uploadDiscImagePriority = 200;

	private const string _remoteFraudSyncKey = "RemoteFraudSyncRequested";

	private const string _paperFraudHalCode = "NoFraudMarkersDetected";

	private const string _imageFolderPath = "c:\\program files\\redbox\\halservice\\video";

	private readonly string _fraudImageFolderPath = Path.Combine("c:\\program files\\redbox\\halservice\\video", "fraud_images");

	private bool? _halSupportsIR;

	private DateTime _lastHalCheck = DateTime.MinValue;

	public static FraudService Instance => Singleton<FraudService>.Instance;

	public bool HardwareSupportsIRScan
	{
		get
		{
			if (_halSupportsIR.HasValue && _lastHalCheck.AddMinutes(2.0) > DateTime.Now)
			{
				return _halSupportsIR.Value;
			}
			_halSupportsIR = ServiceLocator.Instance.GetService<IHardwareService>()?.SupportsCameraFraudScan;
			_lastHalCheck = DateTime.Now;
			return _halSupportsIR ?? false;
		}
	}

	public bool IsIRSecurityEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance.EnableIRSecurity;

	public bool IsFraudImageEnabled => Redbox.Rental.Services.Configuration.Configuration.Instance.EnableImageSecurity;

	public IEnumerable<TitleType> FraudPeelIncludeFormats
	{
		get
		{
			if (!IsFraudImageEnabled)
			{
				return new List<TitleType>();
			}
			return GetFormats("SendReturnImageTypes") ?? (from x in GetAllTitleTypesOfTitleFamily(TitleFamily.Games)
				where x != TitleType.NintendoSwitch
				select x);
		}
	}

	public IEnumerable<TitleType> FraudPaperExcludedFormats
	{
		get
		{
			if (!IsIRSecurityEnabled || !HardwareSupportsIRScan)
			{
				return GetAllTitleTypesOfTitleFamily(TitleFamily.All);
			}
			return GetFormats("PaperFraudExclusion") ?? new List<TitleType> { TitleType.NintendoSwitch };
		}
	}

	public bool SupportsPaperFraud
	{
		get
		{
			if (IsIRSecurityEnabled && HardwareSupportsIRScan)
			{
				return GetAllTitleTypesOfTitleFamily(TitleFamily.All).Any((TitleType x) => !FraudPaperExcludedFormats.Contains(x));
			}
			return false;
		}
	}

	public string ImageFolderPath => "c:\\program files\\redbox\\halservice\\video";

	private int KioskId => (int)(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);

	public static IEnumerable<TitleType> GetAllTitleTypesOfTitleFamily(TitleFamily val)
	{
		List<TitleType> list = new List<TitleType>();
		FieldInfo[] fields = typeof(TitleType).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			TitleFamilyAttribute titleFamilyAttribute = fieldInfo.GetCustomAttributes(inherit: false).OfType<TitleFamilyAttribute>().FirstOrDefault();
			if (titleFamilyAttribute != null && titleFamilyAttribute.TitleFamily == val)
			{
				list.Add((TitleType)fieldInfo.GetValue(null));
			}
		}
		return list;
	}

	private IEnumerable<TitleType> GetFormats(string key)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		string value = null;
		if (service != null && service.TryGetValue<string>("system", "fraud", key, out value) && !string.IsNullOrWhiteSpace(value))
		{
			string[] items = value.Split(',');
			List<TitleType> outputList = new List<TitleType>();
			items.ForEach(delegate(string x)
			{
				TitleFamily result;
				if (Enum.TryParse<TitleType>(x, ignoreCase: true, out var titleType) && titleType != TitleType.All)
				{
					if (!outputList.Any((TitleType t) => t == titleType))
					{
						outputList.Add(titleType);
					}
				}
				else if (Enum.TryParse<TitleFamily>(x, ignoreCase: true, out result))
				{
					if (result != TitleFamily.All)
					{
						foreach (TitleType item in GetAllTitleTypesOfTitleFamily(result))
						{
							if (!outputList.Any((TitleType t) => t == item))
							{
								outputList.Add(item);
							}
						}
						return;
					}
					outputList = Enum.GetValues(typeof(TitleType)).Cast<TitleType>().ToList();
				}
			});
			return outputList;
		}
		LogHelper.Instance.Log("Failed to load formats.");
		return null;
	}

	private IEnumerable<Tuple<int, int>> GetExcludedBarcodeRanges()
	{
		List<Tuple<int, int>> outputList = new List<Tuple<int, int>>();
		string value = null;
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null && service.TryGetValue<string>("system", "fraud", "PaperExcludeBarcodeRange", out value) && !string.IsNullOrWhiteSpace(value))
		{
			(value?.Split(';'))?.ForEach(delegate(string rangeText)
			{
				string[] array = rangeText.Split(',');
				if (array != null && array.Length == 2)
				{
					int num = (int.TryParse(array[0], out var result) ? result : (-1));
					int num2 = (int.TryParse(array[1], out var result2) ? result2 : (-1));
					if (num > 0 && num2 > 0 && num <= num2)
					{
						outputList.Add(Tuple.Create(num, num2));
					}
				}
			});
		}
		else
		{
			outputList.Add(Tuple.Create(354400000, 354499999));
		}
		LogHelper.Instance.Log($"Found {outputList.Count} ranges in PaperExcludeBarcodeRange.");
		return outputList;
	}

	public bool IsPaperFraudCandidate(string barcode)
	{
		if (!IsIRSecurityEnabled || !HardwareSupportsIRScan)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud Configuration Disabled.", LogEntryType.Info);
			return false;
		}
		if (string.IsNullOrWhiteSpace(barcode))
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud barcode is null or empty.", LogEntryType.Info);
			return false;
		}
		if (string.Compare(barcode, "empty", ignoreCase: true) == 0)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud barcode is 'EMPTY'.", LogEntryType.Info);
			return false;
		}
		if (string.Compare(barcode, "unknown", ignoreCase: true) == 0)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud barcode is 'UNKNOWN'.", LogEntryType.Info);
			return false;
		}
		int? num = (int.TryParse(barcode, out var result) ? new int?(result) : ((int?)null));
		if (!num.HasValue)
		{
			LogHelper.Instance.Log($"FraudService.IsPaperFraud barcode failed to parse into an int: {barcode}", LogEntryType.Info);
			return false;
		}
		foreach (Tuple<int, int> excludedBarcodeRange in GetExcludedBarcodeRanges())
		{
			int? num2 = num;
			int item = excludedBarcodeRange.Item1;
			if (num2.GetValueOrDefault() >= item && num2.HasValue && num <= excludedBarcodeRange.Item2)
			{
				LogHelper.Instance.Log($"FraudService.IsPaperFraud barcode: {barcode} is within exclusion range: {excludedBarcodeRange.Item1} and {excludedBarcodeRange.Item2}", LogEntryType.Info);
				return false;
			}
		}
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		IKioskProduct kioskProduct = null;
		ErrorList errorList = service?.GetProductByBarcode(barcode, out kioskProduct);
		if (errorList != null && errorList.Any())
		{
			errorList.ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error x)
			{
				LogHelper.Instance.LogError(x.Code, x.Description);
			});
		}
		TitleType? titleType;
		int? titleId;
		if (kioskProduct == null)
		{
			LogHelper.Instance.Log($"FraudService.IsPaperFraud - unable to find kiosk product for barcode {barcode}.  Looking up title id and title type in inventory.data and profile.data");
			GetTitleTypeByBarcode(barcode, out titleType, out titleId);
		}
		else
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			titleId = (int?)titleProduct?.ProductId;
			titleType = titleProduct?.TitleType;
		}
		if (!titleType.HasValue)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud - title type is unknown.", LogEntryType.Info);
			return false;
		}
		if (titleId.HasValue && titleId == 1139)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraud - product is an empty case.", LogEntryType.Info);
			return false;
		}
		bool flag = FraudPaperExcludedFormats.Any((TitleType x) => x == titleType);
		LogHelper.Instance.Log(string.Format("FraudService.IsPaperFraud - {0} is excluded: {1}", titleType?.ToString() ?? "Unknown Title Type", flag), LogEntryType.Info);
		if (flag)
		{
			return false;
		}
		return true;
	}

	private bool GetTitleTypeByBarcode(string barcode, out TitleType? titleType, out int? titleId)
	{
		titleType = null;
		titleId = null;
		bool flag = false;
		if (!string.IsNullOrWhiteSpace(barcode))
		{
			Inventory inventory = ServiceLocator.Instance.GetService<IInventoryData>()?.GetInventory(barcode);
			if (inventory != null)
			{
				titleId = (int)inventory.TitleId;
				titleType = ServiceLocator.Instance.GetService<IDataService>()?.GetProductTitleType(inventory.TitleId);
				if (titleType.HasValue)
				{
					flag = true;
				}
				else
				{
					LogHelper.Instance.Log($"FraudService.GetProductByBarcode - unable to find title type for barcode: {barcode}", LogEntryType.Info);
				}
			}
			else
			{
				LogHelper.Instance.Log($"FraudService.GetProductByBarcode - unable to find inventory record for barcode: {barcode}", LogEntryType.Info);
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log($"FraudService.GetProductByBarcode - unable to find product by barcode: {barcode}", LogEntryType.Info);
		}
		return flag;
	}

	public void SendFraudAlert(string barcode, bool isFraud, string readTime, DateTime? returnDate = null)
	{
		ServiceLocator.Instance.GetService<IInventoryService>()?.UpdateFraudStatus(KioskId.ToString(), barcode, isFraud.ToString(), readTime, null, null, null, returnDate);
	}

	public void AddToFraudStatistics(string fraudBarcodes)
	{
		if (string.IsNullOrWhiteSpace(fraudBarcodes))
		{
			LogHelper.Instance.Log("AddToFraudStatistics No fraud barcodes data provided.", LogEntryType.Info);
			return;
		}
		IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("Enqueuing Statistic FraudCleanSync Failed.", LogEntryType.Info);
			return;
		}
		LogHelper.Instance.Log($"Enqueuing Statistic FraudCleanSync with value: {fraudBarcodes}", LogEntryType.Info);
		service.EnqueueStatistic("FraudCleanSync", fraudBarcodes);
	}

	public void UploadDiscImage(string imageName, string barcode, long? transactionId, bool hardwareSupportsIRScan, string cpAccountNumber, Action<IRemoteServiceResult> callback)
	{
		if (string.IsNullOrWhiteSpace(barcode))
		{
			LogHelper.Instance.Log("FraudService.UploadDiscImage - barcode is null or empty.", LogEntryType.Info);
			return;
		}
		byte[] imageBytes = GetImageBytes(imageName);
		if (((imageBytes != null && imageBytes.Length != 0) ? 1 : 0) <= (false ? 1 : 0))
		{
			LogHelper.Instance.Log($"FraudService.UploadDiscImage - image {imageName} didn't have enough bytes.");
			return;
		}
		RemoteServiceCallback completeCallback = ((callback != null) ? new RemoteServiceCallback(callback.Invoke) : null);
		ServiceLocator.Instance.GetService<IUploadDiscImageService>()?.UploadDiscImage(KioskId, barcode, ImageType.Full, transactionId, imageBytes, hardwareSupportsIRScan, cpAccountNumber, completeCallback);
	}

	public void UploadDiscImageFromSync(string imageName, string barcode)
	{
		if (IsFraudImageEnabled)
		{
			GetTitleTypeByBarcode(barcode, out var titleType, out var titleId);
			if (titleId.HasValue && titleId == 1139)
			{
				LogHelper.Instance.Log("FraudService.UploadDiscImageFromSync - product is an empty case.", LogEntryType.Info);
			}
			else if (titleType.HasValue && FraudPeelIncludeFormats.Any((TitleType x) => x == titleType))
			{
				UploadDiscImage(imageName, barcode, null, HardwareSupportsIRScan, null, null);
			}
			else
			{
				LogHelper.Instance.Log(string.Format("FraudService.UploadDiscImageFromSync - product of type {0} not in include list {1}", titleType?.ToString() ?? "Unknown", string.Join(",", FraudPeelIncludeFormats)), LogEntryType.Info);
			}
		}
	}

	public void ProcessJobResultsForFraud(string jobId, bool sendStatistic = false)
	{
		if (string.IsNullOrWhiteSpace(jobId))
		{
			LogHelper.Instance.Log("FraudService.ProcessJobResultsForFraud - no jobId provided.");
			return;
		}
		LogHelper.Instance.Log($"FraudService.ProcessJobResultsForFraud - JobId: {jobId}", LogEntryType.Info);
		try
		{
			IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
			ReadOnlyCollection<IHardwareResult> hardwareResults = null;
			ErrorList errorList = service?.GetProgramResults(jobId, out hardwareResults);
			if (errorList != null && errorList.Any())
			{
				LogHelper.Instance.Log("FraudService.ProcessJobResultsForFraud - HardwareService GetProgramResults had errors.", LogEntryType.Info);
				errorList.ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error x)
				{
					LogHelper.Instance.LogError(x.Code, x.Description);
				});
			}
			if (hardwareResults == null || !hardwareResults.Any())
			{
				LogHelper.Instance.Log($"FraudService.ProcessJobResultsForFraud - JobId: {jobId}, had no results.", LogEntryType.Info);
				return;
			}
			List<string> list = new List<string>();
			foreach (IHardwareResult item in hardwareResults)
			{
				if (!string.IsNullOrWhiteSpace(item.ItemID) && string.Compare(item.ItemID, "unknown", ignoreCase: true) != 0 && string.Compare(item.ItemID, "empty", ignoreCase: true) != 0 && item.Code == "NoFraudMarkersDetected" && IsPaperFraudCandidate(item.ItemID))
				{
					list.Add(item.ItemID);
					DateTime? returnDate = (DateTime.TryParse(item.ReturnTime, out var result) ? new DateTime?(result) : ((DateTime?)null));
					SendFraudAlert(item.ItemID, isFraud: true, item.Timestamp, returnDate);
				}
			}
			LogHelper.Instance.Log($"FraudService.ProcessJobResultsForFraud - Found {list.Count()} fraud Items. Send Statistics {sendStatistic.ToString()}", LogEntryType.Info);
			if (sendStatistic && list.Any())
			{
				AddToFraudStatistics(list.Count().ToString());
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Unhandled Exception in FraudService.ProcessJobResultsForFraud - JobId: {jobId}", e);
		}
	}

	public void ProcessReturnUnknownEvent(string barcode, string jobId)
	{
		if (string.IsNullOrWhiteSpace(jobId))
		{
			LogHelper.Instance.Log("FraudService.ProcessReturnUnknownEvent - no jobId provided.");
			return;
		}
		if (string.IsNullOrWhiteSpace(barcode))
		{
			LogHelper.Instance.Log("FraudService.ProcessReturnUnknownEvent - no barcode provided.");
			return;
		}
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		try
		{
			string arg = (IsPaperFraudCandidate(barcode) ? "reject" : "continue");
			ErrorList errorList = service?.ExecuteServiceCommand($"JOB SIGNAL JOB: {jobId} value: {arg}");
			if (errorList != null && errorList.Any())
			{
				LogHelper.Instance.Log("FraudService.ProcessReturnUnknownEvent - HardwareService ExecuteServiceCommand had errors.", LogEntryType.Info);
				errorList.ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error x)
				{
					LogHelper.Instance.LogError(x.Code, x.Description);
				});
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Unhandled Exception in FraudService.ProcessReturnUnknownEvent - Barcode: {barcode}, JobId: {jobId}", e);
		}
	}

	public void HandleBeforeReturnMessageProcessing(IMessage message, out object clientData)
	{
		clientData = null;
		if (message != null && message.Type == "Return")
		{
			LogHelper.Instance.Log($"Executing queue service pre-message processing for message type {message?.Type}");
			ReturnMessageWithClientData returnMessageWithClientData = ServiceLocator.Instance.GetService<IQueueService>()?.GetObjectFromMessageData<ReturnMessageWithClientData>(message);
			if (returnMessageWithClientData.ClientData != null && returnMessageWithClientData.ReturnMessage != null)
			{
				clientData = returnMessageWithClientData.ClientData;
				message.Data = Encoding.ASCII.GetBytes(returnMessageWithClientData.ReturnMessage.ToJson());
			}
			else
			{
				LogHelper.Instance.Log("FraudService.HandleBeforeReturnMessageProcessing: Return message does not contain client data.");
			}
		}
	}

	public void ProcessReturnResponse(IDictionary<string, object> result, object clientData)
	{
		if (!IsFraudImageEnabled)
		{
			return;
		}
		LogHelper.Instance.Log("Executing queue service post-message processing for return message type");
		ReturnMessageClientData returnMessageClientData = clientData as ReturnMessageClientData;
		if (string.IsNullOrEmpty(returnMessageClientData?.Barcode))
		{
			LogHelper.Instance.Log("FraudService.ProcessReturnResponse - clientData is null.  Unable to retrieve barcode and filename info.", LogEntryType.Info);
			return;
		}
		string key = "TransactionId";
		string key2 = "cpAccountNumber";
		string s = ((!result.ContainsKey(key)) ? null : result[key]?.ToString());
		long? transactionId = null;
		if (long.TryParse(s, out var result2))
		{
			transactionId = result2;
		}
		string cpAccountNumber = ((!result.ContainsKey(key2)) ? null : result[key2]?.ToString());
		GetTitleTypeByBarcode(returnMessageClientData.Barcode, out var titleType, out var titleId);
		if (titleId.HasValue && titleId == 1139)
		{
			LogHelper.Instance.Log("FraudService.IsPaperFraudExcluded - product is an empty case.", LogEntryType.Info);
		}
		else if (titleType.HasValue && FraudPeelIncludeFormats.Any((TitleType x) => x == titleType))
		{
			UploadDiscImage(returnMessageClientData.FileName, returnMessageClientData.Barcode, transactionId, HardwareSupportsIRScan, cpAccountNumber, null);
		}
		else
		{
			LogHelper.Instance.Log(string.Format("FraudService.ProcessReturnResponse - product of type {0} not in include list {1}", titleType?.ToString() ?? "Unknown", string.Join(",", FraudPeelIncludeFormats)), LogEntryType.Info);
		}
	}

	public bool ScheduleFraudSync(DateTime startTime, string userName)
	{
		LogHelper.Instance.Log("ScheduleFraudSync > Begin.");
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		ServiceLocator.Instance.GetService<IDataService>().GetUnfilteredNonTitleRollupProducts(out var products);
		List<TitleType> fraudExcludeList = FraudPaperExcludedFormats.ToList();
		Stack<int> halStackLocations = new Stack<int>();
		IEnumerable<Tuple<int, int>> excludedBarcodeRanges = GetExcludedBarcodeRanges();
		products.ForEach(delegate(IKioskProduct x)
		{
			ITitleProduct product = x as ITitleProduct;
			if (product == null)
			{
				LogHelper.Instance.Log($"ScheduleFraudSync > Product {x.ProductName} is not an ITitleProduct.");
			}
			if (!product.IsEmptyCase && !fraudExcludeList.Contains(product.TitleType))
			{
				LogHelper.Instance.Log($"ScheduleFraudSync > Product: {product.ProductId} adding {product.BarCodeProducts.Count()} ");
				product.BarCodeProducts.ForEach(delegate(KeyValuePair<string, BarcodeProduct> y)
				{
					int num = (int.TryParse(y.Key, out var result) ? result : (-1));
					if (num >= 0)
					{
						foreach (Tuple<int, int> item in excludedBarcodeRanges)
						{
							if (num >= item.Item1 && num <= item.Item2)
							{
								LogHelper.Instance.Log($"ScheduleFraudSync > Barcode {y.Key} in Product: {product.ProductId} is in excluded range {item.Item1} to {item.Item2}");
								return;
							}
						}
						string[] array = y.Value.Position.Split('@');
						if (array.Count() != 2)
						{
							LogHelper.Instance.Log($"ScheduleFraudSync > Barcode {y.Key} in Product: {product.ProductId} has invalid location {y.Value.Position}.");
						}
						else
						{
							halStackLocations.Push(int.Parse(array[0]));
							halStackLocations.Push(int.Parse(array[1]));
						}
					}
				});
			}
		});
		if (halStackLocations.Count < 2)
		{
			LogHelper.Instance.Log("ScheduleFraudSync > No Titles To Sync.");
			ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueProperty("IsKioskClean", true.ToString());
			return false;
		}
		string jobId = null;
		string text = string.Format("Fraud Sync by {0}", userName ?? "Unknown");
		ErrorList source = service?.SyncLocations(null, null, text, startTime, halStackLocations.ToList(), out jobId);
		bool flag = !string.IsNullOrEmpty(jobId) && !source.Any();
		string text2 = (flag ? "Successfully" : "Unsuccessfully");
		LogHelper.Instance.Log($"{text2} scheduled job: {jobId ?? string.Empty} - {text} with {halStackLocations.Count() / 2} locations.");
		return flag;
	}

	public void CreateFraudSyncTimer()
	{
		int value = 900000;
		ServiceLocator.Instance.GetService<ITimerService>().CreateTimer("FraudSyncCheck", value, value, CheckForFraudSync).Start();
	}

	private void CheckForFraudSync(object state)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service.GetValue("Store", "RemoteFraudSyncRequested", defaultValue: false))
			{
				if (!SupportsPaperFraud)
				{
					LogHelper.Instance.Log("Fraud Sync Check - PaperFraudNotSupported.");
				}
				else
				{
					LogHelper.Instance.Log("Scheduling Fraud Sync.");
					ScheduleFraudSync(DateTime.Now, "RemoteStart");
					service.SetValue("Store", "RemoteFraudSyncRequested", value: false);
				}
			}
		});
	}

	public byte[] GetImageBytes(string imageName)
	{
		if (string.IsNullOrWhiteSpace(imageName))
		{
			LogHelper.Instance.Log("Get Image failed because no image name provided.", LogEntryType.Info);
			return null;
		}
		string path = Path.Combine("c:\\program files\\redbox\\halservice\\video", imageName);
		if (!File.Exists(path))
		{
			LogHelper.Instance.Log($"Image {imageName} doesn't exist.", LogEntryType.Info);
			return null;
		}
		try
		{
			LogHelper.Instance.Log($"Get Image {imageName}", LogEntryType.Info);
			return File.ReadAllBytes(path);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Failed to get image {imageName}", e);
			return null;
		}
	}

	public void DeleteImage(string imageName)
	{
		if (string.IsNullOrWhiteSpace(imageName))
		{
			LogHelper.Instance.Log("Delete Image failed because no image name provided.", LogEntryType.Info);
			return;
		}
		string path = Path.Combine("c:\\program files\\redbox\\halservice\\video", imageName);
		if (!File.Exists(path))
		{
			LogHelper.Instance.Log($"Image {imageName} doesn't exist.", LogEntryType.Info);
			return;
		}
		try
		{
			LogHelper.Instance.Log($"Delete Image {imageName}", LogEntryType.Info);
			File.Delete(path);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Failed to delete image {imageName}", e);
		}
	}

	public void DeleteImages(IEnumerable<string> images)
	{
		LogHelper.Instance.Log($"Attempting to delete {images?.Count() ?? 0} images.", LogEntryType.Info);
		images?.ForEach(delegate(string x)
		{
			DeleteImage(x);
		});
	}

	public void CleanImageFolder()
	{
		try
		{
			if (!Directory.Exists("c:\\program files\\redbox\\halservice\\video"))
			{
				LogHelper.Instance.Log(string.Format("FraudService.CleanImageFolder did not find the image folder path: {0} - bailing", "c:\\program files\\redbox\\halservice\\video"), LogEntryType.Info);
				return;
			}
			FileInfo[] files = new DirectoryInfo("c:\\program files\\redbox\\halservice\\video").GetFiles();
			int keepImagesForNumberOfDays = Redbox.Rental.Services.Configuration.Configuration.Instance.KeepHalImagesForNumberOfDays;
			IEnumerable<string> images = from x in files
				where (DateTime.Now - x.CreationTime).TotalDays > (double)keepImagesForNumberOfDays
				select x.Name;
			DeleteImages(images);
			if (!Directory.Exists(_fraudImageFolderPath))
			{
				return;
			}
			LogHelper.Instance.Log("Remove Old fraud image folder files.");
			FileInfo[] files2 = new DirectoryInfo(_fraudImageFolderPath).GetFiles();
			if (files2.Any())
			{
				IEnumerable<string> images2 = from x in files2
					where (DateTime.Now - x.CreationTime).TotalDays > 0.0
					select x.Name;
				DeleteImages(images2);
			}
			else
			{
				LogHelper.Instance.Log("Remove Old fraud image directory.");
				Directory.Delete(_fraudImageFolderPath);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("FraudService.CleanImageFolder experienced an unhandled exception", e);
		}
	}

	private FraudService()
	{
	}
}
