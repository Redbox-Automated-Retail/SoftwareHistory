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

public class UpdateService : IDisposable
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
				m_clientSession = ClientSession.GetClientSession(Redbox_002EIPC_002EFramework1769326_002EIPCProtocol.Parse(Url), null);
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
		return new UpdateService(url)
		{
			Timeout = 30000
		};
	}

	public static UpdateService GetService(string url, int timeout)
	{
		return new UpdateService(url)
		{
			Timeout = timeout
		};
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

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult List<T>(out List<Identifier> list)
	{
		list = new List<Identifier>();
		if (!m_typeToCommandForm.ContainsKey(typeof(T)))
		{
			return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult
			{
				Errors = { Redbox.IPC.Framework.Error.NewError("U334", $"The type '{typeof(T).FullName}' does not have a command form mapping.", "Specify a valid type.") }
			};
		}
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"{m_typeToCommandForm[typeof(T)]} list");
		if (obj.Success)
		{
			list = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult Get<T>(Identifier id, out T t)
	{
		t = default(T);
		if (!m_typeToCommandForm.ContainsKey(typeof(T)))
		{
			return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult
			{
				Errors = { Redbox.IPC.Framework.Error.NewError("U334", $"The type '{typeof(T).FullName}' does not have a command form mapping.", "Specify a valid type.") }
			};
		}
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"{m_typeToCommandForm[typeof(T)]} get id: {id.ID}");
		if (obj.Success)
		{
			t = obj.CommandMessages[0].ToObject<T>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetSubscriptionStatus(string name, out SubscriptionStatus state)
	{
		state = new SubscriptionStatus();
		string command = $"subscription status name: '{name}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			state = obj.CommandMessages[0].ToObject<SubscriptionStatus>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ListTransferStatisticByStoreAndRepository(string number, string name, out List<Identifier> ids)
	{
		ids = new List<Identifier>();
		string command = $"transfer-statistic get-store-repository-report name: '{name}' number: '{number}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			ids = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CheckStoreExists(string number, out bool exists)
	{
		exists = false;
		string command = $"store get name: '{number}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			exists = true;
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DownloadWorkResult(Identifier id, string targetFile)
	{
		string command = $"work-result download id: {id.ID}";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			File.WriteAllText(targetFile, obj.CommandMessages[0]);
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult EfAddStoreToRelease(string name, string number, string user)
	{
		string command = $"store ef-add-to-release number:'{number}' name:'{name}' user:'{user}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult AddStoreToRelease(string name, string number)
	{
		string command = $"store add-to-release name: '{name}' number: '{number}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult StartStoreInstaller(string kioskId, string repositoryHash, string frontEndVersion)
	{
		string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
		string command = $"installer start hash: '{repositoryHash}' version: '{frontEndVersion}' h: '{credentialHash}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult InstallerLog(string kioskId, string guid, string name, string value)
	{
		string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
		string command = string.Format("installer log guid:'{0}' name:'{1}' value:'{2}' h: '{2}'", guid, name, value, credentialHash);
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateNewSetting(string name, string value)
	{
		string command = $"setting set name: '{name}' value: '{value}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateNewSetting(string name, string value, string filter)
	{
		string command = $"setting set name: '{name}' value: '{value}' filter: '{filter}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ListStores(int maxItems, long? startId, out List<Identifier> stores)
	{
		stores = new List<Identifier>();
		string command = $"store list maxitems: {maxItems}{(startId.HasValue ? $"startid: {startId.Value}" : string.Empty)}";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			stores = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetLastCheckIn(string number, out DateTime lastCheckIn)
	{
		lastCheckIn = DateTime.UtcNow;
		string command = $"store last-check-in number: '{number}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			lastCheckIn = obj.CommandMessages[0].ToObject<DateTime>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateSubscription(long generatorId, string name, string repository, string fileName, string path, string createdBy, string system, string displayName)
	{
		string command = $"SUBSCRIPTION create generatorId: {generatorId} name: '{name}' repositoryName: '{repository}' fileName: '{fileName}' path: '{path}' createdBy: '{createdBy}' system: '{system}' displayName: '{displayName}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateFileGenerator(string type, string createdBy, string system)
	{
		string command = $"FILE-GENERATOR create type: '{type}' createdBy: '{createdBy}' system: '{system}'";
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DeleteFile(string repositoryName, string fullPath, string user, string system, out RepositoryEntry entry)
	{
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = GetFilesForRepository(repositoryName, out var files);
		if (!obj.Success)
		{
			entry = new RepositoryEntry();
			return obj;
		}
		List<RepositoryEntry> list = new List<RepositoryEntry>();
		foreach (Identifier item in files)
		{
			obj = Get<RepositoryEntry>(item, out var t);
			if (obj.Success)
			{
				list.Add(t);
			}
		}
		RepositoryEntry repositoryEntry = list.Find((RepositoryEntry f) => Path.Combine(f.Path, f.Name) == fullPath);
		if (repositoryEntry == null)
		{
			obj.Success = false;
			obj.Errors.Add(Redbox.IPC.Framework.Error.NewError("M100", $"No file found with path: {fullPath} in repository {repositoryName}", "Supply a valid file id and retry your command."));
			entry = new RepositoryEntry();
			return obj;
		}
		return DeleteFile(repositoryEntry.ID, user, system, out entry);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DeleteFile(long id, string user, string system, out RepositoryEntry entry)
	{
		entry = new RepositoryEntry();
		string command = $"file delete id: {id} modifiedby: '{user}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ExecuteCommandString(command);
		if (obj.Success)
		{
			entry = obj.CommandMessages[0].ToObject<RepositoryEntry>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult Commit(string name, LocalChangeSet set, string user, string system, string comment, out RevLog log)
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
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj;
		foreach (Dictionary<string, string> item in list)
		{
			obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "data-upload begin-upload");
			if (!obj.Success)
			{
				return obj;
			}
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			long num = Convert.ToInt64(obj.CommandMessages[0]);
			using FileStream file = File.OpenRead(item["Data"]);
			byte[] array = new byte[10485760];
			for (int num2 = ReadChunk(file, array); num2 > 0; num2 = ReadChunk(file, array))
			{
				byte[] array2 = new byte[num2];
				Array.Copy(array, array2, array2.Length);
				obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"data-upload append id: {num} data: '{algorithm.Compress(array2).ToBase64()}'");
				if (!obj.Success)
				{
					return obj;
				}
			}
			item["Data"] = num.ToString();
		}
		string command = $"repository commit name: '{name}' comment: '{comment}' modifiedBy: '{user}' system: '{system}' data: '{list.ToJson().EscapedJson()}'";
		obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			log = obj.CommandMessages[0].ToObject<RevLog>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult UnlinkRepositoryToStore(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"repository remove-link-to-store name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			store = obj.CommandMessages[0].ToObject<Store>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult LinkRepositoryToStore(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"repository link-to-store name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			store = obj.CommandMessages[0].ToObject<Store>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult UnlinkRepositoryToGroup(string groupName, string name, string modifiedBy, string system, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"repository remove-link-to-group name: {name} groupName: {groupName} modifiedby: '{modifiedBy}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			group = obj.CommandMessages[0].ToObject<StoreGroup>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult LinkRepositoryToGroup(string groupName, string name, string modifiedBy, string system, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"repository link-to-group name: {name} groupName: {groupName} modifiedby: '{modifiedBy}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			group = obj.CommandMessages[0].ToObject<StoreGroup>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult LinkStoreToGroup(string number, string name, string modifiedBy, string system, out Store store)
	{
		store = new Store();
		string command = $"store link-to-group name: {name} number: {number} modifiedby: '{modifiedBy}' system: '{system}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			store = obj.CommandMessages[0].ToObject<Store>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateScript(string name, string scriptLocation, string createBy, string system, out Script script)
	{
		script = null;
		if (!File.Exists(scriptLocation))
		{
			return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult
			{
				Errors = { Redbox.IPC.Framework.Error.NewError("U335", $"The script file does not exist: {scriptLocation}.", "Specify a path to a valid script file.") }
			};
		}
		script = new Script();
		string text = Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64().Replace("'", "\\'");
		string command = $"update-script create text: '{text}' createdBy: '{createBy}' system: '{system}' name: '{name}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			script = obj.CommandMessages[0].ToObject<Script>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult UpdateScript(long id, string scriptLocation, string modifiedBy, string system, out Script script)
	{
		script = null;
		if (!File.Exists(scriptLocation))
		{
			return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult
			{
				Errors = { Redbox.IPC.Framework.Error.NewError("U335", $"The script file does not exist: {scriptLocation}.", "Specify a path to a valid script file.") }
			};
		}
		script = new Script();
		string text = Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64().Replace("'", "\\'");
		string command = string.Format("update-script update id: {3} text: '{0}' modifiedby: '{1}' system: '{2}'", text, modifiedBy, system, id);
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			script = obj.CommandMessages[0].ToObject<Script>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetHash(long fileId, out string hash)
	{
		hash = string.Empty;
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"file hash id: {fileId}");
		if (obj.Success)
		{
			hash = obj.CommandMessages[0];
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateStore(string storeNumber, string groupName, string createdBy, string system)
	{
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store create storeNumber: '{storeNumber}' groupName: '{groupName}' createdBy: '{createdBy}' system: '{system}'");
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult EnableStore(string storeNumber)
	{
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store enable number:'{storeNumber}'");
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DeleteStore(string storeNumber)
	{
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store delete number:'{storeNumber}'");
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateRepository(string name, List<string> storeNumbers, List<string> groupNames, string createdBy, string system, out Repository repository)
	{
		repository = new Repository();
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"repository create name: '{name}' storeNumbers: '{storeNumbers.ToJson().EscapedJson()}' storeGroups: '{groupNames.ToJson().EscapedJson()}' createdBy: '{createdBy}' system: '{system}'");
		if (obj.Success)
		{
			repository = obj.CommandMessages[0].ToObject<Repository>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetFile(long fileId, out RepositoryEntry repositoryEntry)
	{
		repositoryEntry = new RepositoryEntry();
		string command = $"file get id: {fileId}";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			repositoryEntry = obj.CommandMessages[0].ToObject<RepositoryEntry>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetFileWithData(long fileId, out RepositoryEntry repositoryEntry, out byte[] data)
	{
		repositoryEntry = new RepositoryEntry();
		data = new byte[0];
		string command = $"file get-data id: {fileId}";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			repositoryEntry = obj.CommandMessages[0].ToObject<RepositoryEntry>();
			CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
			data = algorithm.Decompress(Convert.FromBase64String(obj.CommandMessages[1]));
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult UpdateStoreGroup(string name, Dictionary<string, int> properties, out StoreGroup group)
	{
		group = new StoreGroup();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("STOREGROUP update name: '{0}'", name);
		foreach (KeyValuePair<string, int> property in properties)
		{
			stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
		}
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, stringBuilder.ToString());
		if (obj.Success)
		{
			group = obj.CommandMessages[0].ToObject<StoreGroup>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult UpdateStore(string name, Dictionary<string, int> properties, out StoreGroup group)
	{
		group = new StoreGroup();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("STORE update number: '{0}'", name);
		foreach (KeyValuePair<string, int> property in properties)
		{
			stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
		}
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, stringBuilder.ToString());
		if (obj.Success)
		{
			group = obj.CommandMessages[0].ToObject<StoreGroup>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateStoreGroup(string name, string createdBy, string system, uint maxKPSInSchedule, uint maxKPSOutOfSchedule, byte scheduleStart, byte scheduleEnd, int maxBandwidthInMB, out StoreGroup group)
	{
		group = new StoreGroup();
		string command = $"STORE-GROUP create name: '{name}' createdBy: '{createdBy}' system: '{system}' scheduleStart: {scheduleStart} scheduleEnd: {scheduleEnd} maxKPSInSchedule: {maxKPSInSchedule} maxKPSOutOfSchedule: {maxKPSOutOfSchedule} maxBandwidthInMB: {maxBandwidthInMB}";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			group = obj.CommandMessages[0].ToObject<StoreGroup>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DeleteStoreGroup(string groupName)
	{
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, $"store-group delete name:'{groupName}'");
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult CreateStore(string number, string groupName, string createdBy, string system, uint maxKPSInSchedule, uint maxKPSOutOfSchedule, byte scheduleStart, byte scheduleEnd, int maxBandwidthInMB, out Store store)
	{
		store = new Store();
		string command = string.Format("STORE create name: '{0}' groupName: '{8}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}", number, createdBy, system, scheduleStart, scheduleEnd, maxKPSInSchedule, maxKPSOutOfSchedule, maxBandwidthInMB, groupName);
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			store = obj.CommandMessages[0].ToObject<Store>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetStoreGroups(out List<StoreGroup> groups)
	{
		groups = new List<StoreGroup>();
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "STOREGROUP list");
		if (obj.Success)
		{
			groups = obj.CommandMessages[0].ToObject<List<StoreGroup>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetChangeSet(string storeNumber, Dictionary<string, string> currentFiles, out Store store, out List<ChangeSet> jobs)
	{
		store = new Store();
		jobs = new List<ChangeSet>();
		string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
		string command = $"u p n: '{storeNumber}' c: '{currentFiles.ToJson().EscapedJson()}' h:'{credentialHash}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			jobs = obj.CommandMessages[0].ToObject<List<ChangeSet>>();
			if (jobs.Count > 0)
			{
				store = obj.CommandMessages[1].ToObject<Store>();
			}
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult Poll(string storeNumber, int version, List<PollRequest> pollRequestList, out List<PollReply> pollReplyList)
	{
		List<PollRequestDTO> pollReplyDTOList = PollRequestDTO.GetPollReplyDTOList(pollRequestList);
		string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
		string command = $"u pr n:'{storeNumber}' v:'{version}' c:'{pollReplyDTOList.ToJson().EscapedJson()}' h:'{credentialHash}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		List<PollReplyDTO> list = new List<PollReplyDTO>();
		foreach (string commandMessage in obj.CommandMessages)
		{
			try
			{
				PollReplyDTO item = commandMessage.ToObject<PollReplyDTO>();
				list.Add(item);
			}
			catch (Exception e)
			{
				obj.Errors.Add(Redbox.IPC.Framework.Error.NewError("POLL", "Error parsing poll reply: " + commandMessage, e));
			}
		}
		pollReplyList = PollReplyDTO.GetPollReplyList(list);
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetFilesForRepository(string name, out List<Identifier> files)
	{
		files = new List<Identifier>();
		string command = $"file list repositoryName: '{name}' maxitems:9999";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			files = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetRepositoriesForGroup(string name, out List<Identifier> repositories)
	{
		repositories = new List<Identifier>();
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "repository list ascendingOrder: true");
		if (obj.Success)
		{
			repositories = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetRepositoriesForStore(string number, out List<Identifier> repositories)
	{
		repositories = new List<Identifier>();
		string command = $"repository list number: '{number}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			repositories = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult GetRevisionsForRepository(string name, out List<Identifier> versions)
	{
		versions = new List<Identifier>();
		string command = $"revlog list name: '{name}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		if (obj.Success)
		{
			versions = obj.CommandMessages[0].ToObject<List<Identifier>>();
		}
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ForcePublishRun(string name)
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
			return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		}
		return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult();
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ExecuteCommandString(string command)
	{
		Session.Timeout = Timeout;
		return ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult Ping()
	{
		return ExecuteCommandString("INTERSERVER ping");
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult QuickPost(string data)
	{
		string command = $"interserver quickpost data: '{data}'";
		return ExecuteCommandString(command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult BatchPost(string data)
	{
		string command = $"interserver batchpost data: '{data}'";
		return ExecuteCommandString(command);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ConfigDefaultCreate(long configSetting, string configdefaulttype, long configdefaulttypekey, string configdefaulttypevalue, string value, string path, DateTime effectiveTime, DateTime? expireTime)
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
				return new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult
				{
					Errors = { Redbox.IPC.Framework.Error.NewError("U563", $"The file does not exist: {path}.", "Specify a path to a valid file.") }
				};
			}
			arg = File.ReadAllBytes(path).ToBase64();
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

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult ConfigDefaultUpdate(long id, string path)
	{
		string format = "configdefault update id: '{0}' value: '{1}' ";
		string text = "";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult();
		if (string.IsNullOrEmpty(path))
		{
			obj.Errors.Add(Redbox.IPC.Framework.Error.NewError("US1000", "Invalid Path", "Specify a path to a valid file."));
			return obj;
		}
		if (!File.Exists(path))
		{
			obj.Errors.Add(Redbox.IPC.Framework.Error.NewError("US1010", $"The file does not exist: {path}.", "Specify a path to a valid file."));
			return obj;
		}
		text = File.ReadAllBytes(path).ToBase64();
		string text2 = string.Format(format, id, text);
		Console.WriteLine(text2);
		return ExecuteCommandString(text2);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DownloadFileSetFile(long? id, string name, string filename, string destinationPath, string path)
	{
		ErrorList errorList = new ErrorList();
		if (!id.HasValue && string.IsNullOrEmpty(name))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0500", "DownloadFileSetFile requires either an id or name in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (string.IsNullOrEmpty(filename))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0501", "DownloadFileSetFile requires a filename in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (string.IsNullOrEmpty(destinationPath))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0502", "DownloadFileSetFile requires a destinationPath in order to set a file.", "Supply a valid value and retry your command."));
		}
		if (!File.Exists(path))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0503", $"The file does not exist: {path}.", "Specify a path to a valid file."));
		}
		if (errorList.ContainsError())
		{
			Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult();
			obj.Errors.AddRange(errorList);
			return obj;
		}
		int num = File.ReadAllBytes(path).Length;
		int num2 = ((num != 0) ? (num / 1048576) : 0);
		int num3 = 0;
		Log("Uploading file: {0} - {1} mb", path, num2);
		using FileStream fileStream = File.OpenRead(path);
		string arg = fileStream.ToASCIISHA1Hash();
		fileStream.Seek(0L, SeekOrigin.Begin);
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj2 = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile set-file " + (id.HasValue ? $"id:{id.Value} " : $"name:'{name}' ") + $"filename:'{filename}' destinationpath:'{destinationPath.EscapedJson()}' filekey:'{arg}'");
		if (!obj2.Success)
		{
			return obj2;
		}
		byte[] array = new byte[1048576];
		for (int num4 = ReadChunk(fileStream, array); num4 > 0; num4 = ReadChunk(fileStream, array))
		{
			byte[] array2 = new byte[num4];
			Array.Copy(array, array2, array2.Length);
			obj2 = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile upload-file " + (id.HasValue ? $"id:{id.Value} " : $"name:'{name}' ") + $"data:'{array2.ToBase64()}'");
			if (!obj2.Success)
			{
				return obj2;
			}
			num3 += array2.Length;
			Log("Uploaded {0} mb / {1} mb", (num3 != 0) ? (num3 / 1048576) : 0, num2);
		}
		obj2 = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, "downloadfile upload-file " + (id.HasValue ? $"id:{id.Value}" : $"name:'{name}'") + " finished:true");
		if (obj2.Success)
		{
			Log("Upload complete");
		}
		return obj2;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult DownloadFileSetScript(long? id, string name, string path)
	{
		ErrorList errorList = new ErrorList();
		if (!id.HasValue && string.IsNullOrEmpty(name))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0505", "DownloadFileSetScript requires either an id or name in order to set a script.", "Supply a valid value and retry your command."));
		}
		if (!File.Exists(path))
		{
			errorList.Add(Redbox.IPC.Framework.Error.NewError("US0506", $"The file does not exist: {path}.", "Specify a path to a valid file."));
		}
		if (errorList.ContainsError())
		{
			Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = new Redbox_002EIPC_002EFramework1769326_002EClientCommandResult();
			obj.Errors.AddRange(errorList);
			return obj;
		}
		string text = string.Format("downloadfile set-script " + (id.HasValue ? $" id:{id.Value} " : $" name:'{name}' ") + " text:'{0}'", File.ReadAllBytes(path).ToBase64());
		Console.WriteLine(text);
		return ExecuteCommandString(text);
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult StoreCacheGet(string filterKey, string filterValue, string kioskStatus, string enabled, out List<StoreCacheData> storeCacheData)
	{
		string command = $"storecache list key:'{filterKey}' value:'{filterValue}' kioskStatus:'{kioskStatus}' enabled:'{enabled}'";
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
		storeCacheData = (obj.Success ? obj.CommandMessages[0].ToObject<List<StoreCacheData>>() : new List<StoreCacheData>());
		return obj;
	}

	public Redbox_002EIPC_002EFramework1769326_002EClientCommandResult StatusMessageGet(string key, string subKey, string type, int? numResults, out List<StatusMessage> updates)
	{
		string text = string.Format("STATUSMESSAGE list", key);
		if (!string.IsNullOrEmpty(key))
		{
			text += $" key:{key}";
		}
		if (!string.IsNullOrEmpty(subKey))
		{
			text += $" subkey:{subKey}";
		}
		if (!string.IsNullOrEmpty(type))
		{
			text += $" type:{type}";
		}
		if (numResults.HasValue)
		{
			text += $" numresults:{numResults.Value}";
		}
		Redbox_002EIPC_002EFramework1769326_002EClientCommandResult obj = ClientCommand<Redbox_002EIPC_002EFramework1769326_002EClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, text);
		updates = (obj.Success ? obj.CommandMessages[0].ToObject<List<StatusMessage>>() : new List<StatusMessage>());
		return obj;
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
