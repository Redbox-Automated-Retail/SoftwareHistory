using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Redbox.Compression;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateService.Client;

internal class UpdateService : IDisposable
{
	private const bool TransientSessionFlag = false;

	private ClientSession m_clientSession;

	private static readonly Dictionary<Type, string> m_typeToCommandForm = new Dictionary<Type, string>
	{
		{
			typeof(Repository),
			"repository"
		},
		{
			typeof(Store),
			"store"
		},
		{
			typeof(StoreGroup),
			"store-group"
		},
		{
			typeof(RepositoryEntry),
			"file"
		},
		{
			typeof(RevLog),
			"revlog"
		},
		{
			typeof(Script),
			"update-script"
		},
		{
			typeof(Patch),
			"patch"
		},
		{
			typeof(Label),
			"label"
		},
		{
			typeof(TransferStatistic),
			"transfer-statistic"
		},
		{
			typeof(Release),
			"release"
		}
	};

	public string Url { get; private set; }

	public int Timeout { get; private set; }

	internal ClientSession Session
	{
		get
		{
			if (m_clientSession != null && m_clientSession.IsDisposed)
			{
				throw new ObjectDisposedException("Cannot use disposed Update Service client session.");
			}
			if (m_clientSession == null)
			{
				m_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(Url), null);
				m_clientSession.Timeout = Timeout;
			}
			return m_clientSession;
		}
		set
		{
			m_clientSession = value;
		}
	}

	public static UpdateService GetService(string url)
	{
		UpdateService updateService = new UpdateService(url);
		updateService.Timeout = 30000;
		return updateService;
	}

	public static UpdateService GetService(string url, int timeout)
	{
		UpdateService updateService = new UpdateService(url);
		updateService.Timeout = timeout;
		return updateService;
	}

	public void Dispose()
	{
		if (Session != null && !Session.IsDisposed)
		{
			try
			{
				Session.Dispose();
			}
			catch
			{
			}
		}
	}

	public ClientCommandResult List<T>(out List<Identifier> list)
	{
		list = new List<Identifier>();
		if (!m_typeToCommandForm.ContainsKey(typeof(T)))
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.Add(Error.NewError("U334", $"The type '{typeof(T).FullName}' does not have a command form mapping.", "Specify a valid type."));
			return clientCommandResult;
		}
		ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"{m_typeToCommandForm[typeof(T)]} list");
		if (clientCommandResult2.Success)
		{
			list = StringExtensions.ToObject<List<Identifier>>(clientCommandResult2.CommandMessages[0]);
		}
		return clientCommandResult2;
	}

	public ClientCommandResult Get<T>(Identifier id, out T t)
	{
		t = default(T);
		if (!m_typeToCommandForm.ContainsKey(typeof(T)))
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.Add(Error.NewError("U334", $"The type '{typeof(T).FullName}' does not have a command form mapping.", "Specify a valid type."));
			return clientCommandResult;
		}
		ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"{m_typeToCommandForm[typeof(T)]} get id: {id.ID}");
		if (clientCommandResult2.Success)
		{
			t = StringExtensions.ToObject<T>(clientCommandResult2.CommandMessages[0]);
		}
		return clientCommandResult2;
	}

	public ClientCommandResult GetSubscriptionStatus(string name, out SubscriptionStatus state)
	{
		state = new SubscriptionStatus();
		string command = $"subscription status name: '{name}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			state = StringExtensions.ToObject<SubscriptionStatus>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult ListTransferStatisticByStoreAndRepository(string number, string name, out List<Identifier> ids)
	{
		ids = new List<Identifier>();
		string command = $"transfer-statistic get-store-repository-report name: '{name}' number: '{number}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			ids = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult CheckStoreExists(string number, out bool exists)
	{
		exists = false;
		string command = $"store get name: '{number}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			exists = true;
		}
		return clientCommandResult;
	}

	public ClientCommandResult DownloadWorkResult(Identifier id, string targetFile)
	{
		string command = $"work-result download id: {id.ID}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			File.WriteAllText(targetFile, clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult EfAddStoreToRelease(string name, string number, string user)
	{
		string command = $"store ef-add-to-release number:'{number}' name:'{name}' user:'{user}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult AddStoreToRelease(string name, string number)
	{
		string command = $"store add-to-release name: '{name}' number: '{number}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult StartStoreInstaller(string repositoryHash, string frontEndVersion)
	{
		string command = $"installer start hash: '{repositoryHash}' version: '{frontEndVersion}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult InstallerLog(string guid, string name, string value)
	{
		string command = $"installer log guid:'{guid}' name:'{name}' value:'{value}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult CreateNewSetting(string name, string value)
	{
		string command = $"setting set name: '{name}' value: '{value}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult CreateNewSetting(string name, string value, string filter)
	{
		string command = $"setting set name: '{name}' value: '{value}' filter: '{filter}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult ListStores(int maxItems, long? startId, out List<Identifier> stores)
	{
		stores = new List<Identifier>();
		string command = $"store list maxitems: {maxItems}{(startId.HasValue ? $"startid: {startId.Value}" : string.Empty)}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			stores = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetLastCheckIn(string number, out DateTime lastCheckIn)
	{
		lastCheckIn = DateTime.UtcNow;
		string command = $"store last-check-in number: '{number}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			lastCheckIn = StringExtensions.ToObject<DateTime>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult UploadFile(string number, string fileName, out Upload upload)
	{
		upload = new Upload();
		string command = $"u u n: '{number}' f: '{fileName}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			upload = StringExtensions.ToObject<Upload>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult CreateSubscription(long generatorId, string name, string repository, string fileName, string path, string createdBy, string system, string displayName)
	{
		string command = $"SUBSCRIPTION create generatorId: {generatorId} name: '{name}' repositoryName: '{repository}' fileName: '{fileName}' path: '{path}' createdBy: '{createdBy}' system: '{system}' displayName: '{displayName}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult CreateFileGenerator(string type, string createdBy, string system)
	{
		string command = $"FILE-GENERATOR create type: '{type}' createdBy: '{createdBy}' system: '{system}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult DeleteFile(string repositoryName, string fullPath, string user, string system, out RepositoryEntry entry)
	{
		ClientCommandResult clientCommandResult = GetFilesForRepository(repositoryName, out var files);
		if (!clientCommandResult.Success)
		{
			entry = new RepositoryEntry();
			return clientCommandResult;
		}
		List<RepositoryEntry> list = new List<RepositoryEntry>();
		foreach (Identifier item in files)
		{
			clientCommandResult = Get<RepositoryEntry>(item, out var t);
			if (clientCommandResult.Success)
			{
				list.Add(t);
			}
		}
		RepositoryEntry repositoryEntry = list.Find((RepositoryEntry f) => Path.Combine(f.Path, f.Name) == fullPath);
		if (repositoryEntry == null)
		{
			clientCommandResult.Success = false;
			clientCommandResult.Errors.Add(Error.NewError("M100", $"No file found with path: {fullPath} in repository {repositoryName}", "Supply a valid file id and retry your command."));
			entry = new RepositoryEntry();
			return clientCommandResult;
		}
		return DeleteFile(repositoryEntry.ID, user, system, out entry);
	}

	public ClientCommandResult DeleteFile(long id, string user, string system, out RepositoryEntry entry)
	{
		entry = new RepositoryEntry();
		string command = $"file delete id: {id} modifiedby: '{user}' system: '{system}'";
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		if (clientCommandResult.Success)
		{
			entry = StringExtensions.ToObject<RepositoryEntry>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult Commit(string name, LocalChangeSet set, string user, string system, string comment, out RevLog log)
	{
		log = new RevLog();
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		foreach (KeyValuePair<string, Dictionary<string, string>> newFile in set.m_newFiles)
		{
			list.Add(newFile.Value);
		}
		foreach (KeyValuePair<long, Dictionary<string, string>> update in set.m_updates)
		{
			update.Value.Add("ID", update.Key.ToString());
			list.Add(update.Value);
		}
		ClientCommandResult clientCommandResult;
		foreach (Dictionary<string, string> item in list)
		{
			clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "data-upload begin-upload");
			if (!clientCommandResult.Success)
			{
				return clientCommandResult;
			}
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			long num = Convert.ToInt64(clientCommandResult.CommandMessages[0]);
			using FileStream file = File.OpenRead(item["Data"]);
			byte[] array = new byte[10485760];
			for (int num2 = ReadChunk(file, array); num2 > 0; num2 = ReadChunk(file, array))
			{
				byte[] array2 = new byte[num2];
				Array.Copy(array, array2, array2.Length);
				clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"data-upload append id: {num} data: '{Redbox.Core.ByteArrayExtensions.ToBase64(algorithm.Compress(array2))}'");
				if (!clientCommandResult.Success)
				{
					return clientCommandResult;
				}
			}
			item["Data"] = num.ToString();
		}
		string command = $"repository commit name: '{name}' comment: '{comment}' modifiedBy: '{user}' system: '{system}' data: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(list))}'";
		clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			log = StringExtensions.ToObject<RevLog>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetWorkResultByStore(string number, out List<WorkResult> results)
	{
		results = new List<WorkResult>();
		string command = $"work-result by-store number: '{number}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			results = StringExtensions.ToObject<List<WorkResult>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetWorkResultByScript(long id, out List<WorkResult> results)
	{
		results = new List<WorkResult>();
		string command = $"work-result by-script id: {id}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			results = StringExtensions.ToObject<List<WorkResult>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetWorkResultByScript(string name, out List<WorkResult> results)
	{
		results = new List<WorkResult>();
		string command = $"work-result by-script name: {name}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			results = StringExtensions.ToObject<List<WorkResult>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetWorkResultByID(long id, out WorkResult workResult)
	{
		workResult = null;
		string command = $"work-result get id: {id}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			workResult = StringExtensions.ToObject<WorkResult>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GiveStoreWork(long workId, string number)
	{
		string command = $"work-item give-store-work id: {workId} number: '{number}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult GiveGroupWork(long workId, string name)
	{
		string command = $"work-item give-group-work id: {workId} name: '{name}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult CreateWorkItem(string name, out WorkItem item)
	{
		item = new WorkItem();
		string command = $"work-item create-by-name name: '{name}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			item = StringExtensions.ToObject<WorkItem>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult CreateWorkItem(string name, DateTime effectiveDate, WorkItemRecurrence recurrence, out WorkItem item)
	{
		item = new WorkItem();
		string command = $"work-item create-by-name name: '{name}' effectiveDate: '{effectiveDate}' recurrence: {recurrence}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			item = StringExtensions.ToObject<WorkItem>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult UnlinkRepositoryToStore(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"repository remove-link-to-store name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			store = StringExtensions.ToObject<Store>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult LinkRepositoryToStore(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"repository link-to-store name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			store = StringExtensions.ToObject<Store>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult UnlinkRepositoryToGroup(string groupName, string name, string modifiedBy, string system, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"repository remove-link-to-group name: {name} groupName: {groupName} modifiedby: '{modifiedBy}' system: '{system}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			group = StringExtensions.ToObject<StoreGroup>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult LinkRepositoryToGroup(string groupName, string name, string modifiedBy, string system, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"repository link-to-group name: {name} groupName: {groupName} modifiedby: '{modifiedBy}' system: '{system}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			group = StringExtensions.ToObject<StoreGroup>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult LinkStoreToGroup(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"store link-to-group name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			store = StringExtensions.ToObject<Store>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult CreateScript(string name, string scriptLocation, string createBy, string system, out Script script)
	{
		script = null;
		if (!File.Exists(scriptLocation))
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.Add(Error.NewError("U335", $"The script file does not exist: {scriptLocation}.", "Specify a path to a valid script file."));
			return clientCommandResult;
		}
		script = new Script();
		string text = Redbox.Core.ByteArrayExtensions.ToBase64(Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation))).Replace("'", "\\'");
		string command = $"update-script create text: '{text}' createdBy: '{createBy}' system: '{system}' name: '{name}'";
		ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult2.Success)
		{
			script = StringExtensions.ToObject<Script>(clientCommandResult2.CommandMessages[0]);
		}
		return clientCommandResult2;
	}

	public ClientCommandResult UpdateScript(long id, string scriptLocation, string modifiedBy, string system, out Script script)
	{
		script = null;
		if (!File.Exists(scriptLocation))
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.Add(Error.NewError("U335", $"The script file does not exist: {scriptLocation}.", "Specify a path to a valid script file."));
			return clientCommandResult;
		}
		script = new Script();
		string text = Redbox.Core.ByteArrayExtensions.ToBase64(Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation))).Replace("'", "\\'");
		string command = string.Format("update-script update id: {3} text: '{0}' modifiedby: '{1}' system: '{2}'", text, modifiedBy, system, id);
		ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult2.Success)
		{
			script = StringExtensions.ToObject<Script>(clientCommandResult2.CommandMessages[0]);
		}
		return clientCommandResult2;
	}

	public ClientCommandResult GetHash(long fileId, out string hash)
	{
		hash = string.Empty;
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"file hash id: {fileId}");
		if (clientCommandResult.Success)
		{
			hash = clientCommandResult.CommandMessages[0];
		}
		return clientCommandResult;
	}

	public ClientCommandResult CreateStore(string storeNumber, string groupName, string createdBy, string system)
	{
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store create storeNumber: '{storeNumber}' groupName: '{groupName}' createdBy: '{createdBy}' system: '{system}'");
	}

	public ClientCommandResult EnableStore(string storeNumber)
	{
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store enable number:'{storeNumber}'");
	}

	public ClientCommandResult DeleteStore(string storeNumber)
	{
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store delete number:'{storeNumber}'");
	}

	public ClientCommandResult CreateRepository(string name, List<string> storeNumbers, List<string> groupNames, string createdBy, string system, out Repository repository)
	{
		repository = new Repository();
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"repository create name: '{name}' storeNumbers: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(storeNumbers))}' storeGroups: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(groupNames))}' createdBy: '{createdBy}' system: '{system}'");
		if (clientCommandResult.Success)
		{
			repository = StringExtensions.ToObject<Repository>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetFile(long fileId, out RepositoryEntry repositoryEntry)
	{
		repositoryEntry = new RepositoryEntry();
		string command = $"file get id: {fileId}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			repositoryEntry = StringExtensions.ToObject<RepositoryEntry>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetFileWithData(long fileId, out RepositoryEntry repositoryEntry, out byte[] data)
	{
		repositoryEntry = new RepositoryEntry();
		data = new byte[0];
		string command = $"file get-data id: {fileId}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			repositoryEntry = StringExtensions.ToObject<RepositoryEntry>(clientCommandResult.CommandMessages[0]);
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			data = algorithm.Decompress(Convert.FromBase64String(clientCommandResult.CommandMessages[1]));
		}
		return clientCommandResult;
	}

	public ClientCommandResult UpdateStoreGroup(string name, Dictionary<string, int> properties, out StoreGroup group)
	{
		group = new StoreGroup();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("STOREGROUP update name: '{0}'", name);
		foreach (KeyValuePair<string, int> property in properties)
		{
			stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
		}
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, stringBuilder.ToString());
		if (clientCommandResult.Success)
		{
			group = StringExtensions.ToObject<StoreGroup>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult UpdateStore(string name, Dictionary<string, int> properties, out StoreGroup group)
	{
		group = new StoreGroup();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("STORE update number: '{0}'", name);
		foreach (KeyValuePair<string, int> property in properties)
		{
			stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
		}
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, stringBuilder.ToString());
		if (clientCommandResult.Success)
		{
			group = StringExtensions.ToObject<StoreGroup>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult CreateStoreGroup(string name, string createdBy, string system, uint maxKPSInSchedule, uint maxKPSOutOfSchedule, byte scheduleStart, byte scheduleEnd, int maxBandwidthInMB, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"STORE-GROUP create name: '{name}' createdBy: '{createdBy}' system: '{system}' scheduleStart: {scheduleStart} scheduleEnd: {scheduleEnd} maxKPSInSchedule: {maxKPSInSchedule} maxKPSOutOfSchedule: {maxKPSOutOfSchedule} maxBandwidthInMB: {maxBandwidthInMB}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			group = StringExtensions.ToObject<StoreGroup>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult DeleteStoreGroup(string groupName)
	{
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store-group delete name:'{groupName}'");
	}

	public ClientCommandResult CreateStore(string number, string groupName, string createdBy, string system, uint maxKPSInSchedule, uint maxKPSOutOfSchedule, byte scheduleStart, byte scheduleEnd, int maxBandwidthInMB, out Store store)
	{
		store = new Store();
		string command = string.Format("STORE create name: '{0}' groupName: '{8}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}", number, createdBy, system, scheduleStart, scheduleEnd, maxKPSInSchedule, maxKPSOutOfSchedule, maxBandwidthInMB, groupName);
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			store = StringExtensions.ToObject<Store>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetStoreGroups(out List<StoreGroup> groups)
	{
		groups = new List<StoreGroup>();
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "STOREGROUP list");
		if (clientCommandResult.Success)
		{
			groups = StringExtensions.ToObject<List<StoreGroup>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult SendResult(string storeNumber, long id, object result)
	{
		string command = $"u r n: '{storeNumber}' i: {id} t: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(result))}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult GetChangeSet(string storeNumber, Dictionary<string, string> currentFiles, out Store store, out List<ChangeSet> jobs)
	{
		store = new Store();
		jobs = new List<ChangeSet>();
		string command = $"u p n: '{storeNumber}' c: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(currentFiles))}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			jobs = StringExtensions.ToObject<List<ChangeSet>>(clientCommandResult.CommandMessages[0]);
			if (jobs.Count > 0)
			{
				store = StringExtensions.ToObject<Store>(clientCommandResult.CommandMessages[1]);
			}
		}
		return clientCommandResult;
	}

	public ClientCommandResult Poll(string storeNumber, List<PollRequest> pollRequestList, out List<PollReply> pollReplyList)
	{
		pollReplyList = new List<PollReply>();
		List<PollReplyDTO> list = new List<PollReplyDTO>();
		List<PollRequestDTO> pollReplyDTOList = PollRequestDTO.GetPollReplyDTOList(pollRequestList);
		string command = $"u pr n: '{storeNumber}' c: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(pollReplyDTOList))}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			foreach (string commandMessage in clientCommandResult.CommandMessages)
			{
				PollReplyDTO item = StringExtensions.ToObject<PollReplyDTO>(commandMessage);
				list.Add(item);
			}
			pollReplyList = PollReplyDTO.GetPollReplyList(list);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetFilesForRepository(string name, out List<Identifier> files)
	{
		files = new List<Identifier>();
		string command = $"file list repositoryName: '{name}' maxitems:9999";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			files = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetRepositoriesForGroup(string name, out List<Identifier> repositories)
	{
		repositories = new List<Identifier>();
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "repository list ascendingOrder: true");
		if (clientCommandResult.Success)
		{
			repositories = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetRepositoriesForStore(string number, out List<Identifier> repositories)
	{
		repositories = new List<Identifier>();
		string command = $"repository list number: '{number}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			repositories = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetRevisionsForRepository(string name, out List<Identifier> versions)
	{
		versions = new List<Identifier>();
		string command = $"revlog list name: '{name}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (clientCommandResult.Success)
		{
			versions = StringExtensions.ToObject<List<Identifier>>(clientCommandResult.CommandMessages[0]);
		}
		return clientCommandResult;
	}

	public ClientCommandResult FinishUpload(UploadReport report)
	{
		string command = $"u q d: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(report))}'";
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult ForcePublishRun(string name)
	{
		object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager", "AllowForceDataFiles", false.ToString());
		bool flag = false;
		if (value != null && value is string && bool.TryParse(value as string, out var result))
		{
			flag = result;
		}
		if (flag)
		{
			string command = $"subscription publish name: '{name}'";
			return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		}
		return new ClientCommandResult();
	}

	public ClientCommandResult ExecuteCommandString(string command)
	{
		Session.Timeout = Timeout;
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public ClientCommandResult Ping()
	{
		return ExecuteCommandString("INTERSERVER ping");
	}

	public ClientCommandResult QuickPost(string data)
	{
		string command = $"interserver quickpost data: '{data}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult BatchPost(string data)
	{
		string command = $"interserver batchpost data: '{data}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult ConfigDefaultCreate(long configSetting, string configdefaulttype, long configdefaulttypekey, string configdefaulttypevalue, string value, string path, DateTime effectiveTime, DateTime? expireTime)
	{
		string format = "configdefault create configsetting: '{0}' configdefaulttype: '{1}' configdefaulttypekey: '{2}' configdefaulttypevalue: '{3}' effectiveTime: '{4}'";
		string arg = "";
		if (!string.IsNullOrEmpty(value))
		{
			arg = value;
		}
		else if (!string.IsNullOrEmpty(path))
		{
			if (!File.Exists(path))
			{
				ClientCommandResult clientCommandResult = new ClientCommandResult();
				clientCommandResult.Errors.Add(Error.NewError("U563", $"The file does not exist: {path}.", "Specify a path to a valid file."));
				return clientCommandResult;
			}
			byte[] inputArray = File.ReadAllBytes(path);
			arg = Redbox.Core.ByteArrayExtensions.ToBase64(inputArray);
		}
		string text = string.Format(format, configSetting, configdefaulttype, configdefaulttypekey, configdefaulttypevalue, effectiveTime);
		if (expireTime.HasValue)
		{
			text += $" expireTime: '{expireTime.Value}'";
		}
		text += $" value: '{arg}'";
		Console.WriteLine(text);
		return ExecuteCommandString(text);
	}

	public ClientCommandResult DownloadFileSetFile(long? id, string name, string filename, string destinationPath, string path)
	{
		ErrorList errorList = new ErrorList();
		if (!id.HasValue && string.IsNullOrEmpty(name))
		{
			errorList.Add(Error.NewError("US0500", "DownloadFileSetFile requires either an id or name in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (string.IsNullOrEmpty(filename))
		{
			errorList.Add(Error.NewError("US0501", "DownloadFileSetFile requires a filename in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (string.IsNullOrEmpty(destinationPath))
		{
			errorList.Add(Error.NewError("US0502", "DownloadFileSetFile requires a destinationPath in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (!File.Exists(path))
		{
			errorList.Add(Error.NewError("US0503", $"The file does not exist: {path}.", "Specify a path to a valid file."));
		}
		if (errorList.ContainsError())
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.AddRange(errorList);
			return clientCommandResult;
		}
		byte[] array = File.ReadAllBytes(path);
		int num = array.Length;
		int num2 = ((num != 0) ? (num / 1048576) : 0);
		int num3 = 0;
		Log("Uploading file: {0} - {1} mb", path, num2);
		using FileStream fileStream = File.OpenRead(path);
		string arg = StreamExtensions.ToASCIISHA1Hash(fileStream);
		fileStream.Seek(0L, SeekOrigin.Begin);
		ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile set-file " + (id.HasValue ? $"id:{id.Value} " : $"name:'{name}' ") + $"filename:'{filename}' destinationpath:'{StringExtensions.EscapedJson(destinationPath)}' filekey:'{arg}'");
		if (!clientCommandResult2.Success)
		{
			return clientCommandResult2;
		}
		byte[] array2 = new byte[1048576];
		for (int num4 = ReadChunk(fileStream, array2); num4 > 0; num4 = ReadChunk(fileStream, array2))
		{
			byte[] array3 = new byte[num4];
			Array.Copy(array2, array3, array3.Length);
			clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile upload-file " + (id.HasValue ? $"id:{id.Value} " : $"name:'{name}' ") + $"data:'{Redbox.Core.ByteArrayExtensions.ToBase64(array3)}'");
			if (!clientCommandResult2.Success)
			{
				return clientCommandResult2;
			}
			num3 += array3.Length;
			Log("Uploaded {0} mb / {1} mb", (num3 != 0) ? (num3 / 1048576) : 0, num2);
		}
		clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile upload-file " + (id.HasValue ? $"id:{id.Value}" : $"name:'{name}'") + " finished:true");
		if (clientCommandResult2.Success)
		{
			Log("Upload complete");
		}
		return clientCommandResult2;
	}

	public ClientCommandResult DownloadFileSetScript(long? id, string name, string path)
	{
		ErrorList errorList = new ErrorList();
		if (!id.HasValue && string.IsNullOrEmpty(name))
		{
			errorList.Add(Error.NewError("US0505", "DownloadFileSetScript requires either an id or name in order to set a script.", "Supply a valid value and retry your command."));
		}
		if (!File.Exists(path))
		{
			errorList.Add(Error.NewError("US0506", $"The file does not exist: {path}.", "Specify a path to a valid file."));
		}
		if (errorList.ContainsError())
		{
			ClientCommandResult clientCommandResult = new ClientCommandResult();
			clientCommandResult.Errors.AddRange(errorList);
			return clientCommandResult;
		}
		string format = "downloadfile set-script " + (id.HasValue ? $" id:{id.Value} " : $" name:'{name}' ") + " text:'{0}'";
		string text = string.Format(format, Redbox.Core.ByteArrayExtensions.ToBase64(File.ReadAllBytes(path)));
		Console.WriteLine(text);
		return ExecuteCommandString(text);
	}

	public ClientCommandResult StoreCacheGet(string filterKey, string filterValue, string kioskStatus, string enabled, out List<StoreCacheData> storeCacheData)
	{
		string command = $"storecache list key:'{filterKey}' value:'{filterValue}' kioskStatus:'{kioskStatus}' enabled:'{enabled}'";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		storeCacheData = (clientCommandResult.Success ? StringExtensions.ToObject<List<StoreCacheData>>(clientCommandResult.CommandMessages[0]) : new List<StoreCacheData>());
		return clientCommandResult;
	}

	public ClientCommandResult StatusMessageGet(string key, string subKey, string type, int numResults, out List<StatusMessage> updates)
	{
		string command = $"STATUSMESSAGE list key:{key} subKey:{subKey} type:{type} numResults:{numResults}";
		ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		updates = (clientCommandResult.Success ? StringExtensions.ToObject<List<StatusMessage>>(clientCommandResult.CommandMessages[0]) : new List<StatusMessage>());
		return clientCommandResult;
	}

	internal UpdateService(string url)
	{
		Url = url;
	}

	private static int ReadChunk(Stream file, byte[] buffer)
	{
		int i;
		int num;
		for (i = 0; i < buffer.Length; i += num)
		{
			num = file.Read(buffer, i, buffer.Length - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}

	private static void Log(string text, params object[] parms)
	{
		Console.WriteLine(string.Format(text, parms));
	}
}
