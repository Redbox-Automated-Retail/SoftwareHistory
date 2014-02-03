using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class ConfigFileService : IConfigFileService, IPollRequestReply
{
	private class ConfigFileStateLabel
	{
		public long GenerationId { get; set; }

		public DateTime? Created { get; set; }
	}

	private const string ConfigFile = ".config";

	private const string ConfigFileStaged = ".configstaged";

	private const string StagedDate = ".staged";

	private const string ActiveDate = ".active";

	private const string ArchiveDirectory = "archive";

	private int _syncID;

	private string _clientConfigFileHash;

	public static ConfigFileService Instance => Singleton<ConfigFileService>.Instance;

	private string _root { get; set; }

	public void Initialize(string rootPath)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		_root = service.ExpandProperties(rootPath);
		if (!Path.IsPathRooted(_root))
		{
			_root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), rootPath);
		}
		MakePathExist();
		if (ServiceLocator.Instance.GetService<IConfigFileService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IConfigFileService), this);
		}
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			ErrorList cCFDTOList = GetCCFDTOList(out var clientConfigFileDataList);
			errorList.AddRange(cCFDTOList);
			string text = ObjectExtensions.ToJson(clientConfigFileDataList);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			string text2 = ByteArrayExtensions.ToASCIISHA1Hash(bytes);
			if (!string.IsNullOrEmpty(_clientConfigFileHash) && _clientConfigFileHash != text2)
			{
				_syncID = 0;
			}
			errorList.AddRange(SetHash());
			if (_syncID == 0)
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.ConfigFiles,
					SyncId = 0,
					Data = text
				});
			}
			else
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.ConfigFiles,
					SyncId = _syncID
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.GetPollRequest.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			PollReply pollReply = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.UpdateConfigFiles
				select pr).FirstOrDefault();
			if (pollReply != null)
			{
				_syncID = 0;
				_clientConfigFileHash = string.Empty;
				LogHelper.Instance.Log("Config File server is clearing the sync id as requested by the server.");
				return new ErrorList();
			}
			PollReply pollReply2 = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.ConfigFileChangeSet
				select pr).FirstOrDefault();
			if (pollReply2 != null)
			{
				if (string.IsNullOrEmpty(pollReply2.Data))
				{
					errorList.Add(Error.NewError("CFS0092", "Error, A poll reply contained a type of config file changeset without the changeset.", ObjectExtensions.ToJson(pollReply2)));
				}
				else
				{
					List<ConfigFileChangeSetData> list = StringExtensions.ToObject<List<ConfigFileChangeSetData>>(pollReply2.Data);
					foreach (ConfigFileChangeSetData item in list)
					{
						ErrorList errorList2 = ProcessChangeSet(item);
						errorList.AddRange(errorList2);
						if (!errorList2.ContainsError())
						{
							errorList.AddRange(StageChangeSet(item));
						}
					}
				}
				_syncID = pollReply2.SyncId;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public List<long> GetAllConfigFiles()
	{
		List<long> list = new List<long>();
		try
		{
			MakePathExist();
			string[] directories = Directory.GetDirectories(_root);
			string[] array = directories;
			foreach (string path in array)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				try
				{
					long item = Convert.ToInt64(directoryInfo.Name);
					list.Add(item);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"(CFS9988) Invalid name in .configfiles directory.  Remove ({directoryInfo.Name})", e);
				}
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.GetAllConfigFiles.", e2);
		}
		return list;
	}

	public ErrorList ActivateConfigFile(long configFileId)
	{
		ErrorList configFileChangeSetData = GetConfigFileChangeSetData(configFileId, out var changeSet, out var _, out var _);
		if (configFileChangeSetData.ContainsError())
		{
			return configFileChangeSetData;
		}
		return ActivateChangeSet(changeSet);
	}

	private ErrorList SetHash()
	{
		ErrorList cCFDTOList = GetCCFDTOList(out var clientConfigFileDataList);
		string s = ObjectExtensions.ToJson(clientConfigFileDataList);
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		string clientConfigFileHash = ByteArrayExtensions.ToASCIISHA1Hash(bytes);
		_clientConfigFileHash = clientConfigFileHash;
		return cCFDTOList;
	}

	private ErrorList GetCCFDTOList(out List<CCFDTO> clientConfigFileDataList)
	{
		ErrorList errorList = new ErrorList();
		clientConfigFileDataList = new List<CCFDTO>();
		List<long> allConfigFiles = GetAllConfigFiles();
		foreach (long item in allConfigFiles)
		{
			ErrorList configFileChangeSetData = GetConfigFileChangeSetData(item, out var changeSet, out var stagedState, out var activeState);
			errorList.AddRange(configFileChangeSetData);
			if (!configFileChangeSetData.ContainsError())
			{
				CCFDTO cCFDTO = new CCFDTO();
				cCFDTO.Id = item;
				cCFDTO.Name = changeSet.Name;
				if (stagedState != null)
				{
					cCFDTO.SGId = stagedState.GenerationId;
					cCFDTO.SGD = stagedState.Created;
				}
				if (activeState != null)
				{
					cCFDTO.AGId = activeState.GenerationId;
					cCFDTO.AGD = activeState.Created;
				}
				clientConfigFileDataList.Add(cCFDTO);
			}
		}
		return errorList;
	}

	private ErrorList GetConfigFileChangeSetData(long configFileId, out ConfigFileChangeSetData changeSet, out ConfigFileStateLabel stagedState, out ConfigFileStateLabel activeState)
	{
		changeSet = null;
		stagedState = null;
		activeState = null;
		ErrorList errorList = new ErrorList();
		try
		{
			string configFilePath = GetConfigFilePath(configFileId);
			string stagedStatePath = GetStagedStatePath(configFileId);
			string activeStatePath = GetActiveStatePath(configFileId);
			if (!File.Exists(configFilePath))
			{
				errorList.Add(Error.NewError("CFS0091", $"Configuration file number {configFileId} does not exist.", ""));
			}
			if (!File.Exists(stagedStatePath))
			{
				errorList.Add(Error.NewError("CFS0091", $"Configuration staged date file number {configFileId} does not exist.", ""));
			}
			string json = File.ReadAllText(configFilePath);
			changeSet = StringExtensions.ToObject<ConfigFileChangeSetData>(json);
			string text = File.ReadAllText(stagedStatePath);
			if (string.IsNullOrEmpty(text))
			{
				errorList.Add(Error.NewError("CFS0091", $"Configuration staged date file number {configFileId} contents should not be empty.", ""));
			}
			else
			{
				stagedState = StringExtensions.ToObject<ConfigFileStateLabel>(text);
			}
			if (File.Exists(activeStatePath))
			{
				string text2 = File.ReadAllText(activeStatePath);
				if (!string.IsNullOrEmpty(text2))
				{
					activeState = StringExtensions.ToObject<ConfigFileStateLabel>(text2);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0091", $"Unhandled exception occurred in ConfigFileService.GetConfigFilechangeSetData for config file {configFileId}.", e));
		}
		if (errorList.ContainsError())
		{
			ClearConfigFile(configFileId);
		}
		return errorList;
	}

	private ErrorList ProcessChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			WriteConfigFile(changeSet);
			DateTime utcNow = DateTime.UtcNow;
			WriteStagedState(changeSet.OID, changeSet.GenerationOID, utcNow);
			WriteConfigFileArchive(changeSet, changeSet.GenerationOID);
			WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, utcNow);
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0089", $"Unhandled exception occurred in ConfigFileService.ProcessChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private ErrorList ActivateChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string destinationActivateFilePath = GetDestinationActivateFilePath(changeSet);
			if (File.Exists(destinationActivateFilePath))
			{
				DateTime utcNow = DateTime.UtcNow;
				WriteActiveState(changeSet.OID, changeSet.GenerationOID, utcNow);
				WriteActiveStateArchive(changeSet.OID, changeSet.GenerationOID, utcNow);
				File.Delete(destinationActivateFilePath);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0088", $"Unhandled exception occurred in ConfigFileService.ActivateChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private string GetDestinationFilePath(ConfigFileChangeSetData changeSet)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(changeSet.Path);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		return Path.ChangeExtension(Path.Combine(text, changeSet.Name), ".config");
	}

	private string GetDestinationActivateFilePath(ConfigFileChangeSetData changeSet)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(changeSet.Path);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		return Path.ChangeExtension(Path.Combine(text, changeSet.Name), ".configstaged");
	}

	private ErrorList StageChangeSet(ConfigFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string destinationFilePath = GetDestinationFilePath(changeSet);
			string destinationActivateFilePath = GetDestinationActivateFilePath(changeSet);
			File.WriteAllText(destinationFilePath, ObjectExtensions.ToJson(changeSet));
			File.WriteAllText(destinationActivateFilePath, "This file is a placeholder to indicate that config file activation is needed.");
			WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, DateTime.UtcNow);
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0087", $"Unhandled exception occurred in ConfigFileService.StageChangeSet for config file {changeSet.OID}.", e));
		}
		return errorList;
	}

	private void ClearConfigFile(long configFileId)
	{
		try
		{
			_syncID = 0;
			LogHelper.Instance.Log("Clearing configFile {0}", configFileId);
			string configFilePath = GetConfigFilePath(configFileId);
			string stagedStatePath = GetStagedStatePath(configFileId);
			string activeStatePath = GetActiveStatePath(configFileId);
			if (File.Exists(configFilePath))
			{
				File.Delete(configFilePath);
			}
			if (File.Exists(stagedStatePath))
			{
				File.Delete(stagedStatePath);
			}
			if (File.Exists(activeStatePath))
			{
				File.Delete(activeStatePath);
			}
			string configDirectory = GetConfigDirectory(configFileId);
			if (Directory.Exists(configDirectory))
			{
				Directory.Delete(configDirectory, recursive: true);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.ClearConfigFile for config file {0}.", e, configFileId);
		}
	}

	private void WriteConfigFile(ConfigFileChangeSetData changeSet)
	{
		File.WriteAllText(GetConfigFilePath(changeSet.OID), ObjectExtensions.ToJson(changeSet));
	}

	private void WriteActiveState(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel configFileStateLabel = new ConfigFileStateLabel();
		configFileStateLabel.GenerationId = generationId;
		configFileStateLabel.Created = date;
		ConfigFileStateLabel instance = configFileStateLabel;
		File.WriteAllText(GetActiveStatePath(configFileId), ObjectExtensions.ToJson(instance));
	}

	private void WriteStagedState(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel configFileStateLabel = new ConfigFileStateLabel();
		configFileStateLabel.GenerationId = generationId;
		configFileStateLabel.Created = date;
		ConfigFileStateLabel instance = configFileStateLabel;
		File.WriteAllText(GetStagedStatePath(configFileId), ObjectExtensions.ToJson(instance));
	}

	private string GetConfigDirectory(long configFileId)
	{
		string text = Path.Combine(_root, configFileId.ToString());
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	private string GetConfigFilePath(long configFileId)
	{
		string configDirectory = GetConfigDirectory(configFileId);
		return Path.Combine(configDirectory, ".config");
	}

	private string GetActiveStatePath(long configFileId)
	{
		string configDirectory = GetConfigDirectory(configFileId);
		return Path.Combine(configDirectory, ".active");
	}

	private string GetStagedStatePath(long configFileId)
	{
		string configDirectory = GetConfigDirectory(configFileId);
		return Path.Combine(configDirectory, ".staged");
	}

	private void WriteConfigFileArchive(ConfigFileChangeSetData changeSet, long generationId)
	{
		File.WriteAllText(GetConfigFilePathArchive(changeSet.OID, generationId), ObjectExtensions.ToJson(changeSet));
	}

	private void WriteActiveStateArchive(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel configFileStateLabel = new ConfigFileStateLabel();
		configFileStateLabel.GenerationId = generationId;
		configFileStateLabel.Created = date;
		ConfigFileStateLabel instance = configFileStateLabel;
		File.WriteAllText(GetActiveStatePathArchive(configFileId, generationId), ObjectExtensions.ToJson(instance));
	}

	private void WriteStagedStateArchive(long configFileId, long generationId, DateTime? date)
	{
		ConfigFileStateLabel configFileStateLabel = new ConfigFileStateLabel();
		configFileStateLabel.GenerationId = generationId;
		configFileStateLabel.Created = date;
		ConfigFileStateLabel instance = configFileStateLabel;
		File.WriteAllText(GetStagedStatePathArchive(configFileId, generationId), ObjectExtensions.ToJson(instance));
	}

	private string GetConfigDirectoryArchive(long configFileId)
	{
		string configDirectory = GetConfigDirectory(configFileId);
		string text = Path.Combine(configDirectory, "archive");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	private string GetConfigFilePathArchive(long configFileId, long generationId)
	{
		string configDirectoryArchive = GetConfigDirectoryArchive(configFileId);
		return Path.Combine(configDirectoryArchive, generationId + ".config");
	}

	private string GetActiveStatePathArchive(long configFileId, long generationId)
	{
		string configDirectoryArchive = GetConfigDirectoryArchive(configFileId);
		return Path.Combine(configDirectoryArchive, generationId + ".active");
	}

	private string GetStagedStatePathArchive(long configFileId, long generationId)
	{
		string configDirectoryArchive = GetConfigDirectoryArchive(configFileId);
		return Path.Combine(configDirectoryArchive, generationId + ".staged");
	}

	private ConfigFileService()
	{
	}

	private void MakePathExist()
	{
		if (!Directory.Exists(_root))
		{
			Directory.CreateDirectory(_root);
		}
	}
}
