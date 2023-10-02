using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.KioskEngineApi;
using KioskClientService.API.Services.UpdateClientServiceApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProfileData;

public class ProfileDataService : IProfileDataService
{
	private class GetProductIdsForKioskResponse
	{
		public bool Success { get; set; }

		public List<RecordRequest> RecordRequests { get; set; } = new List<RecordRequest>();
	}

	private class ProductIdStatistics
	{
		public int ProductsFromKiosk { get; set; }

		public int ProductsFromProductGroups { get; set; }
	}

	private class DataUpdateStats
	{
		public int AddedCount { get; set; }

		public int ChangedCount { get; set; }

		public int DeletedCount { get; set; }

		public int DeleteNotFoundCount { get; set; }

		public int NoChangeCount { get; set; }

		public int ServerErrorCount { get; set; }

		public int DeserializationErrorCount { get; set; }

		public bool AnyTableFileUpdates()
		{
			if (AddedCount <= 0 && ChangedCount <= 0)
			{
				return DeletedCount > 0;
			}
			return true;
		}
	}

	private class ProcessDataUpdateData<T> : IDataTableRequest
	{
		public DataUpdateRequest DataUpdateRequest { get; set; }

		public UpdateDataTable TableName => DataUpdateRequest?.TableName ?? UpdateDataTable.None;

		public FileTypeEnum? FileType => DataUpdateRequest?.FileType;

		public string FileName { get; set; }

		public DataUpdateResponse DataUpdateResponse { get; set; }

		public DataTable<T> DataTable { get; set; } = new DataTable<T>();

		public DataUpdateStats DataUpdateStats { get; set; } = new DataUpdateStats();

		public bool IsComplete { get; set; }

		public DateUpdateResult DateUpdateResult { get; set; } = new DateUpdateResult();

		public string IdentifyingText
		{
			get
			{
				string fileName = FileName;
				try
				{
					fileName = Path.GetFileName(FileName);
				}
				catch
				{
				}
				return "for filename " + fileName;
			}
		}
	}

	public class DateUpdateResult
	{
		public bool Success { get; set; }

		public bool TableFileUpdated { get; set; }
	}

	private readonly ILogger<ProfileDataService> _logger;

	private readonly IUpdateClientServiceClient _updateClientServiceClient;

	private readonly IStoreService _store;

	private readonly IKioskEngineApiProfileDataClient _kioskEngineApiProfileDataClient;

	private string _defaultDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox\\Data");

	private static ConcurrentDictionary<UpdateDataTable, object> _dataTables = new ConcurrentDictionary<UpdateDataTable, object>();

	private static int _processingChanges = 0;

	public ProfileDataService(ILogger<ProfileDataService> logger, IStoreService store, IUpdateClientServiceClient updateClientServiceClient, IKioskEngineApiProfileDataClient kioskEngineApiProfileDataClient)
	{
		_logger = logger;
		_store = store;
		_updateClientServiceClient = updateClientServiceClient;
		_kioskEngineApiProfileDataClient = kioskEngineApiProfileDataClient;
	}

	public async Task<DataTable<Product>> GetProducts()
	{
		return await GetDataTable<Product>(UpdateDataTable.Product);
	}

	private async Task<DataTable<T>> GetDataTable<T>(UpdateDataTable updateDataTable)
	{
		DataTable<T> dataTable = null;
		try
		{
			object value = null;
			if (_dataTables.TryGetValue(updateDataTable, out value))
			{
				dataTable = value as DataTable<T>;
			}
			else
			{
				DataTableRequest dataTableRequest = new DataTableRequest
				{
					FileType = FileTypeEnum.ProfileData,
					TableName = updateDataTable
				};
				DataTableResponse<T> dataTableResponse = await ReadDataTable<T>(dataTableRequest);
				if (dataTableResponse.Success)
				{
					if (_dataTables.TryAdd(updateDataTable, dataTableResponse.DataTable))
					{
						dataTable = dataTableResponse.DataTable;
					}
					else if (_dataTables.TryGetValue(updateDataTable, out value))
					{
						dataTable = value as DataTable<T>;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while getting data table for {updateDataTable}", "GetDataTable");
		}
		return dataTable;
	}

	private void ClearDataTable(UpdateDataTable updateDataTable)
	{
		_dataTables.TryRemove(updateDataTable, out var _);
	}

	public async Task<TriggerProductDataUpdateResponse> TriggerProductDataUpdate()
	{
		TriggerProductDataUpdateResponse response = new TriggerProductDataUpdateResponse();
		try
		{
			GetProductIdsForKioskResponse getProductIdsForKioskResponse = await GetProductIdsForKiosk();
			if (getProductIdsForKioskResponse == null || !getProductIdsForKioskResponse.Success)
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				return response;
			}
			DateUpdateResult result = await UpdateProductDataTable(getProductIdsForKioskResponse.RecordRequests);
			List<long> list = await GetMissingProductIdsFromProductGroups();
			if (list.Any())
			{
				_logger.LogInfoWithSource($"Triggering update of {list.Count} that were found in product groups", "TriggerProductDataUpdate");
				List<RecordRequest> missingProductRecordRequests = new List<RecordRequest>();
				list.ForEach(delegate(long eachMissingProductId)
				{
					missingProductRecordRequests.Add(new RecordRequest
					{
						Id = eachMissingProductId
					});
				});
				DateUpdateResult dateUpdateResult = await UpdateProductDataTable(missingProductRecordRequests);
				result.Success &= dateUpdateResult.Success;
				result.TableFileUpdated |= dateUpdateResult.TableFileUpdated;
			}
			response.StatusCode = (result.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception while trigger product data update", "TriggerProductDataUpdate");
		}
		return response;
	}

	private async Task<DateUpdateResult> UpdateProductDataTable(List<RecordRequest> recordRequests)
	{
		DataUpdateRequest dataUpdateRequest = new DataUpdateRequest
		{
			RequestId = Guid.NewGuid(),
			KioskId = _store.KioskId,
			TableName = UpdateDataTable.Product,
			RequestType = DataUpdateRequestType.List,
			FileType = FileTypeEnum.ProfileData,
			RecordRequests = recordRequests
		};
		_logger.LogInformation($"Triggering update of {dataUpdateRequest.RecordRequests?.Count ?? 0} product data records with requestId: {dataUpdateRequest.RequestId}");
		DateUpdateResult dateUpdateResult = await GetDataTableUpdates<Product>(dataUpdateRequest);
		if (dateUpdateResult.TableFileUpdated)
		{
			ClearDataTable(UpdateDataTable.Product);
		}
		return dateUpdateResult;
	}

	private async Task<GetProductIdsForKioskResponse> GetProductIdsForKiosk()
	{
		GetProductIdsForKioskResponse response = new GetProductIdsForKioskResponse();
		HashSet<long> productIds = new HashSet<long>();
		HashSet<long> hashSet = productIds;
		hashSet.UnionWith(await GetExistingDataRecordIds<Product>(UpdateDataTable.Product));
		APIResponse<GetKioskProductIdsResponse> aPIResponse = await _kioskEngineApiProfileDataClient.GetKioskProductIds();
		if (aPIResponse == null || aPIResponse.StatusCode != HttpStatusCode.OK)
		{
			_logger.LogErrorWithSource("Unable to get product ids for kiosk from Kiosk Engine", "GetProductIdsForKiosk");
			return response;
		}
		productIds.UnionWith(aPIResponse.Response.Content);
		ProductIdStatistics productIdStatistics = new ProductIdStatistics
		{
			ProductsFromKiosk = aPIResponse.Response.Content.Count
		};
		hashSet = productIds;
		hashSet.UnionWith(await GetMissingProductIdsFromProductGroups());
		foreach (long item in productIds)
		{
			response.RecordRequests.Add(new RecordRequest
			{
				Id = item
			});
		}
		response.Success = true;
		_logger.LogInfoWithSource("Kiosk Product ids count: " + JsonConvert.SerializeObject(productIdStatistics), "GetProductIdsForKiosk");
		return response;
	}

	private async Task<List<long>> GetExistingDataRecordIds<T>(UpdateDataTable updateDataTable)
	{
		List<long> result = new List<long>();
		DataTable<T> dataTable = await GetDataTable<T>(updateDataTable);
		if (dataTable?.Data?.Any() == true)
		{
			foreach (KeyValuePair<long, DataRecord<T>> datum in dataTable.Data)
			{
				result.Add(datum.Key);
			}
		}
		return result;
	}

	private async Task<List<long>> GetMissingProductIdsFromProductGroups()
	{
		List<long> missingProductIds = new List<long>();
		DataTable<Product> dataTable = await GetProducts();
		if (dataTable?.Data?.Any() == true)
		{
			foreach (KeyValuePair<long, DataRecord<Product>> datum in dataTable.Data)
			{
				Product data = datum.Value.Data;
				if (data.ProductGroup?.ProductFormats?.Any() != true)
				{
					continue;
				}
				foreach (ProductFormat productFormat in data.ProductGroup.ProductFormats)
				{
					if (!dataTable.Data.ContainsKey(productFormat.ProductId))
					{
						missingProductIds.Add(productFormat.ProductId);
					}
				}
			}
		}
		return missingProductIds;
	}

	private async Task<DateUpdateResult> GetDataTableUpdates<T>(DataUpdateRequest dataUpdateRequest)
	{
		DateUpdateResult result = new DateUpdateResult();
		try
		{
			await GetHashValues<T>(dataUpdateRequest);
			APIResponse<GetRecordChangesResponse> aPIResponse = await _updateClientServiceClient.GetRecordChanges<GetRecordChangesResponse>(dataUpdateRequest);
			if (aPIResponse == null || aPIResponse.StatusCode != HttpStatusCode.OK || aPIResponse.Response == null || aPIResponse.Response.StatusCode != HttpStatusCode.OK)
			{
				return result;
			}
			return await ProcessDataUpdateFile<T>(dataUpdateRequest, aPIResponse.Response?.DataUpdateResponseFileName);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Execption while getting data updates for requestId {dataUpdateRequest?.RequestId}", "GetDataTableUpdates");
			return result;
		}
	}

	private async Task GetHashValues<T>(DataUpdateRequest dataUpdateRequest)
	{
		IEnumerable<RecordRequest> recordRequestsWithoutHash = dataUpdateRequest.RecordRequests?.Where((RecordRequest x) => string.IsNullOrEmpty(x.Hash));
		if (!(recordRequestsWithoutHash?.Any() ?? false))
		{
			return;
		}
		int hashCount = 0;
		DataTable<T> dataTable = await GetDataTable<T>(dataUpdateRequest.TableName);
		if (dataTable?.Data?.Any() == true)
		{
			foreach (RecordRequest item in recordRequestsWithoutHash)
			{
				if (dataTable.Data.TryGetValue(item.Id, out var value))
				{
					item.Hash = value.Hash;
					hashCount++;
				}
			}
		}
		_logger.LogInfoWithSource($"Popluated DataUpdateRequest with {hashCount} hash values.", "GetHashValues");
	}

	private async Task<DateUpdateResult> ProcessDataUpdateFile<T>(DataUpdateRequest dataUpdateRequest, string fileName)
	{
		ProcessDataUpdateData<T> processDataUpdateData = new ProcessDataUpdateData<T>
		{
			DataUpdateRequest = dataUpdateRequest,
			FileName = fileName
		};
		if (Interlocked.CompareExchange(ref _processingChanges, 1, 0) == 1)
		{
			_logger.LogInfoWithSource("Locking prevented processing of data update " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
			return processDataUpdateData.DateUpdateResult;
		}
		try
		{
			_ = 6;
			try
			{
				_logger.LogInfoWithSource("Starting processing " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
				await ReadDataUpdates(processDataUpdateData);
				ValidateDataUpdates(processDataUpdateData);
				await ReadDataTable(processDataUpdateData);
				await ApplyDataUpdatesToTable(processDataUpdateData);
				await SaveUpdatedDataTableToStageFile(processDataUpdateData);
				await RenameActiveDataTableAsOld(processDataUpdateData);
				await RenameStagedFileToActive(processDataUpdateData);
				LogDataUpdateResults(processDataUpdateData);
				await CleanupFailedDataUpdateAttempt(processDataUpdateData);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while processing data update " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
			}
		}
		finally
		{
			_processingChanges = 0;
			await DeleteDataUpdateFile(processDataUpdateData);
		}
		return processDataUpdateData.DateUpdateResult;
	}

	private async Task DeleteDataUpdateFile<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(async delegate
		{
			try
			{
				await DeleteFile(processDataUpdateData.FileName);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while deleting data update file " + processDataUpdateData.IdentifyingText, "DeleteDataUpdateFile");
			}
		});
	}

	private async Task CleanupFailedDataUpdateAttempt<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.DateUpdateResult.Success)
		{
			string dataTableFullPath = GetDataTableFullPath(processDataUpdateData);
			string oldDataTableFullPath = GetOldDataTableFullPath(processDataUpdateData);
			if (!File.Exists(dataTableFullPath) && File.Exists(oldDataTableFullPath))
			{
				await RenameFile(oldDataTableFullPath, dataTableFullPath);
			}
			string dataTableStagedFullPath = GetDataTableStagedFullPath(processDataUpdateData);
			await DeleteFile(dataTableStagedFullPath);
		}
	}

	private async Task RenameStagedFileToActive<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string stagedDataTableFileName = GetDataTableStagedFullPath(processDataUpdateData);
			string dataTableFileName = GetDataTableFullPath(processDataUpdateData);
			if (await RenameFile(stagedDataTableFileName, dataTableFileName))
			{
				processDataUpdateData.DateUpdateResult.Success = true;
				processDataUpdateData.DateUpdateResult.TableFileUpdated = true;
				_logger.LogInfoWithSource("Renamed staged DataTable file " + Path.GetFileName(stagedDataTableFileName) + " to " + Path.GetFileName(dataTableFileName) + " " + processDataUpdateData.IdentifyingText, "RenameStagedFileToActive");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while renaming staged file " + processDataUpdateData.IdentifyingText, "RenameStagedFileToActive");
		}
	}

	private void LogDataUpdateResults<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		processDataUpdateData.DateUpdateResult.Success = processDataUpdateData.DateUpdateResult.Success && processDataUpdateData.DataUpdateStats.ServerErrorCount == 0 && processDataUpdateData.DataUpdateStats.DeserializationErrorCount == 0;
		string text = (processDataUpdateData.DateUpdateResult.Success ? "succeeded" : "failed");
		_logger.LogInfoWithSource("Data update " + text + " " + processDataUpdateData.IdentifyingText, "LogDataUpdateResults");
	}

	private async Task RenameActiveDataTableAsOld<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string oldDataTableFileName = GetOldDataTableFullPath(processDataUpdateData);
			if (!(await DeleteFile(oldDataTableFileName)))
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource("Unable to delete " + oldDataTableFileName, "RenameActiveDataTableAsOld");
				return;
			}
			string dataTableFileName = GetDataTableFullPath(processDataUpdateData);
			if (File.Exists(dataTableFileName))
			{
				if (!(await RenameFile(dataTableFileName, oldDataTableFileName)))
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource("Unable to rename " + dataTableFileName + " to " + oldDataTableFileName, "RenameActiveDataTableAsOld");
					return;
				}
				_logger.LogInfoWithSource("Renamed active DataTable file " + Path.GetFileName(dataTableFileName) + " to " + Path.GetFileName(oldDataTableFileName) + " ", "RenameActiveDataTableAsOld");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while renaming active data table file " + processDataUpdateData.IdentifyingText, "RenameActiveDataTableAsOld");
		}
	}

	private async Task SaveUpdatedDataTableToStageFile<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string stagedFileName = GetDataTableStagedFullPath(processDataUpdateData);
			string directoryName = Path.GetDirectoryName(stagedFileName);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!(await DeleteFile(stagedFileName)))
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource("Unable to delete " + stagedFileName, "SaveUpdatedDataTableToStageFile");
			}
			if (!processDataUpdateData.IsComplete)
			{
				SortedDictionary<long, DataRecord<T>> dictionary = new SortedDictionary<long, DataRecord<T>>(processDataUpdateData.DataTable.Data);
				processDataUpdateData.DataTable.Data = new Dictionary<long, DataRecord<T>>(dictionary);
				string contents = JsonConvert.SerializeObject(processDataUpdateData.DataTable, Formatting.Indented);
				File.WriteAllText(stagedFileName, contents);
				_logger.LogInfoWithSource("Saved staged file to " + stagedFileName, "SaveUpdatedDataTableToStageFile");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while saving updates to stage file " + processDataUpdateData.IdentifyingText, "SaveUpdatedDataTableToStageFile");
		}
	}

	private async Task<bool> DeleteFile(string fileName)
	{
		bool result = false;
		await Task.Run(delegate
		{
			try
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
					result = !File.Exists(fileName);
				}
				else
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to delete file " + fileName, "DeleteFile");
			}
		});
		return result;
	}

	private async Task<bool> RenameFile(string originalFileName, string newFileName)
	{
		bool result = false;
		await Task.Run(delegate
		{
			try
			{
				File.Move(originalFileName, newFileName);
				if (File.Exists(newFileName))
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to rename file " + originalFileName + " to " + newFileName, "RenameFile");
			}
		});
		return result;
	}

	private async Task ApplyDataUpdatesToTable<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(delegate
		{
			if (!processDataUpdateData.IsComplete)
			{
				try
				{
					if (processDataUpdateData.DataUpdateResponse?.RecordResponses?.Any() == true)
					{
						foreach (RecordResponse recordResponse in processDataUpdateData.DataUpdateResponse.RecordResponses)
						{
							switch (recordResponse.RecordStatus)
							{
							case DataUpdateRecordStatus.NoChange:
								processDataUpdateData.DataUpdateStats.NoChangeCount++;
								break;
							case DataUpdateRecordStatus.Changed:
								try
								{
									T data = JsonConvert.DeserializeObject<T>(recordResponse.Data);
									DataRecord<T> value = new DataRecord<T>
									{
										Id = recordResponse.Id,
										Data = data,
										Hash = recordResponse.Hash
									};
									if (processDataUpdateData.DataTable.Data.ContainsKey(recordResponse.Id))
									{
										processDataUpdateData.DataUpdateStats.ChangedCount++;
									}
									else
									{
										processDataUpdateData.DataUpdateStats.AddedCount++;
									}
									processDataUpdateData.DataTable.Data[recordResponse.Id] = value;
								}
								catch (Exception ex)
								{
									processDataUpdateData.DataUpdateStats.DeserializationErrorCount++;
									_logger.LogErrorWithSource(ex, $"Exception while updating {processDataUpdateData.DataUpdateRequest.TableName} record with id {recordResponse.Id}", "ApplyDataUpdatesToTable");
								}
								break;
							case DataUpdateRecordStatus.Deleted:
								if (processDataUpdateData.DataTable.Data.ContainsKey(recordResponse.Id))
								{
									processDataUpdateData.DataUpdateStats.DeletedCount++;
									processDataUpdateData.DataTable.Data.Remove(recordResponse.Id);
								}
								else
								{
									processDataUpdateData.DataUpdateStats.DeleteNotFoundCount++;
								}
								break;
							case DataUpdateRecordStatus.Error:
								_logger.LogInfoWithSource($"Record Status Error for {processDataUpdateData.DataUpdateRequest.TableName} record with id {recordResponse.Id}", "ApplyDataUpdatesToTable");
								processDataUpdateData.DataUpdateStats.ServerErrorCount++;
								break;
							}
						}
						_logger.LogInfoWithSource("DataUpates applied: " + JsonConvert.SerializeObject(processDataUpdateData.DataUpdateStats, new JsonSerializerSettings
						{
							DefaultValueHandling = DefaultValueHandling.Ignore
						}), "ApplyDataUpdatesToTable");
						if (!processDataUpdateData.DataUpdateStats.AnyTableFileUpdates())
						{
							processDataUpdateData.DateUpdateResult.Success = true;
							processDataUpdateData.IsComplete = true;
							_logger.LogInfoWithSource($"No {processDataUpdateData.TableName} table updates to apply {processDataUpdateData.IdentifyingText}", "ApplyDataUpdatesToTable");
						}
					}
				}
				catch (Exception ex2)
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource(ex2, "Exception while applying data updates " + processDataUpdateData.IdentifyingText, "ApplyDataUpdatesToTable");
				}
			}
		});
	}

	private async Task ReadDataTable<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.IsComplete)
		{
			DataTableResponse<T> dataTableResponse = await ReadDataTable<T>((IDataTableRequest)processDataUpdateData);
			processDataUpdateData.DataTable = dataTableResponse.DataTable;
			if (!dataTableResponse.Success)
			{
				processDataUpdateData.DataTable = new DataTable<T>();
				_logger.LogErrorWithSource($"Unable to load DataTable for {processDataUpdateData.DataUpdateRequest.TableName}.  Recreating the table.", "ReadDataTable");
			}
		}
	}

	private async Task<DataTableResponse<T>> ReadDataTable<T>(IDataTableRequest dataTableRequest)
	{
		DataTableResponse<T> dataTableResponse = new DataTableResponse<T>
		{
			DataTableRequest = dataTableRequest
		};
		await Task.Run(delegate
		{
			try
			{
				string dataTableFullPath = GetDataTableFullPath(dataTableRequest);
				if (File.Exists(dataTableFullPath))
				{
					string value = File.ReadAllText(dataTableFullPath);
					try
					{
						dataTableResponse.DataTable = JsonConvert.DeserializeObject<DataTable<T>>(value);
						if (dataTableResponse.DataTable == null)
						{
							_logger.LogErrorWithSource("Deserialized DataTable is null for file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
						}
						dataTableResponse.Success = true;
						_logger.LogInfoWithSource($"Read {dataTableResponse.DataTable.Data.Count} records from table {dataTableRequest.TableName}", "ReadDataTable");
						return;
					}
					catch (Exception ex)
					{
						_logger.LogErrorWithSource(ex, "Exception while deserializing data table file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
						return;
					}
				}
				dataTableResponse.DataTable = new DataTable<T>();
				dataTableResponse.Success = true;
				_logger.LogInfoWithSource("File " + dataTableFullPath + " does not exist. No records were read.", "ReadDataTable");
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Exception while reading data table file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
			}
		});
		return dataTableResponse;
	}

	private void ValidateDataUpdates<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.IsComplete)
		{
			DataUpdateResponse dataUpdateResponse = processDataUpdateData.DataUpdateResponse;
			if (dataUpdateResponse != null && dataUpdateResponse.TableName == UpdateDataTable.None)
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"Invalid table name {processDataUpdateData.DataUpdateResponse?.TableName} {processDataUpdateData.IdentifyingText}", "ValidateDataUpdates");
			}
			if (processDataUpdateData.DataUpdateResponse?.RequestId != processDataUpdateData.DataUpdateRequest.RequestId)
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"Id in DataUpateResponse {processDataUpdateData.DataUpdateResponse?.RequestId} doesn't match RequestId {processDataUpdateData.DataUpdateRequest.RequestId}", "ValidateDataUpdates");
			}
			if (processDataUpdateData.DataUpdateResponse?.TableName != processDataUpdateData.DataUpdateRequest.TableName)
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"TableName in DataUpateResponse {processDataUpdateData.DataUpdateResponse?.RequestId} doesn't match Request TableName {processDataUpdateData.DataUpdateRequest.RequestId} {processDataUpdateData.IdentifyingText}", "ValidateDataUpdates");
			}
		}
	}

	private string GetDataTableFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.Combine(GetDataTableDirectory(dataTableRequest), GetDataTableFileName(dataTableRequest));
	}

	private string GetOldDataTableFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.Combine(GetDataTableDirectory(dataTableRequest), GetOldDataTableFileName(dataTableRequest));
	}

	private string GetDataTableDirectory(IDataTableRequest dataTableRequest)
	{
		string path = dataTableRequest.TableName.ToString();
		string value = null;
		if (!dataTableRequest.FileType.HasValue || !FilePaths.TypeMappings.TryGetValue(dataTableRequest.FileType.Value, out value))
		{
			value = _defaultDataFolder;
		}
		return Path.Combine(value, path);
	}

	private string GetDataTableFileName(IDataTableRequest dataTableRequest)
	{
		string text = dataTableRequest.TableName.ToString();
		return text + "Data.json";
	}

	private string GetOldDataTableFileName(IDataTableRequest dataTableRequest)
	{
		string text = dataTableRequest.TableName.ToString();
		return text + "Data old.json";
	}

	private string GetDataTableStagedFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.ChangeExtension(GetDataTableFullPath(dataTableRequest), ".staged");
	}

	private async Task ReadDataUpdates<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(delegate
		{
			if (!processDataUpdateData.IsComplete)
			{
				try
				{
					if (!File.Exists(processDataUpdateData.FileName))
					{
						_logger.LogErrorWithSource("Unable to find file " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
						processDataUpdateData.IsComplete = true;
					}
					else
					{
						string value = File.ReadAllText(processDataUpdateData.FileName);
						processDataUpdateData.DataUpdateResponse = JsonConvert.DeserializeObject<DataUpdateResponse>(value);
						if (processDataUpdateData.DataUpdateResponse == null)
						{
							processDataUpdateData.IsComplete = true;
							_logger.LogErrorWithSource("DataUpdateResponse is null " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
						}
						else
						{
							_logger.LogInfoWithSource($"read {(processDataUpdateData.DataUpdateResponse?.RecordResponses?.Count).GetValueOrDefault()} update records {processDataUpdateData.IdentifyingText}", "ReadDataUpdates");
						}
					}
				}
				catch (Exception ex)
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource(ex, "Exception while reading data updates " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
				}
			}
		});
	}
}
