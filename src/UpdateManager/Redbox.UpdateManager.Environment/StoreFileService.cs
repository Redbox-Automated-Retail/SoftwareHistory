using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class StoreFileService : IPollRequestReply
{
	private const string StagedStoreFileExt = ".staged";

	private const string StoreFileStateExt = ".state";

	private string m_root;

	public static StoreFileService Instance => Singleton<StoreFileService>.Instance;

	public ErrorList Initialize(string rootPath)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			m_root = rootPath;
			if (!Directory.Exists(m_root))
			{
				Directory.CreateDirectory(m_root);
			}
			if (!Directory.Exists(m_root))
			{
				errorList.Add(Error.NewError("SFS0041", "The root directory is not set up properly", "Please check initialization values"));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SFS0042", "Unhandled exception occurred in StoreFileService.Initialize.", e));
		}
		return errorList;
	}

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			errorList = GetCurrentState(out var storeFilePollRequestList);
			if (storeFilePollRequestList.StoreFilePollRequests.Any())
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.StoreFiles,
					Data = storeFilePollRequestList.ToShortFormat()
				});
			}
			LogHelper.Instance.Log("StoreFileService: Store file poll request: " + ObjectExtensions.ToJson(pollRequestList), LogEntryType.Debug);
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SFS0042", "Unhandled exception occurred in StoreFileService.Initialize.", e));
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			foreach (PollReply item in pollReplyList.Where((PollReply pr) => pr.PollReplyType == PollReplyType.StoreFileChangeSet))
			{
				if (string.IsNullOrEmpty(item.Data))
				{
					errorList.Add(Error.NewError("SFS0092", "Error, A poll reply contained a type of store file changeset without the changeset.", ObjectExtensions.ToJson(item)));
					continue;
				}
				StoreFileChangeSetData changeSet = StringExtensions.ToObject<StoreFileChangeSetData>(item.Data);
				errorList.AddRange(ProcessChangeSet(changeSet));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SFS0091", "Unhandled exception occurred in StoreFileService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private StoreFileService()
	{
	}

	private IEnumerable<Error> Activate(long id)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string path = Path.Combine(m_root, id.ToString());
			string path2 = Path.ChangeExtension(path, ".state");
			string text = Path.ChangeExtension(path, ".staged");
			if (!File.Exists(path2))
			{
				errorList.Add(Error.NewError("SFS0311", "No state file for ID: " + id, "Check the id you are trying to activate"));
				return errorList;
			}
			if (!File.Exists(text))
			{
				errorList.Add(Error.NewError("SFS0312", "No store file for ID: " + id, "Check the id you are trying to activate"));
				return errorList;
			}
			string json = File.ReadAllText(path2);
			StoreFileState storeFileState = StringExtensions.ToObject<StoreFileState>(json);
			string directoryName = Path.GetDirectoryName(storeFileState.Destination);
			if (Directory.Exists(storeFileState.Destination))
			{
				errorList.Add(Error.NewError("SFS0316", "This store file's destination is a directory and not a path: " + id, "Specify a full path, not just a directory for store files"));
				storeFileState.State = FileState.NoDestinationPath;
			}
			else if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
			{
				errorList.Add(Error.NewError("SFS0315", "No directory found at: " + directoryName, string.Empty));
				storeFileState.State = FileState.NoDestinationDirectory;
			}
			else
			{
				FileState state;
				bool flag;
				switch (storeFileState.State)
				{
				case FileState.Staged:
					state = FileState.ActivationFailedOnce;
					flag = true;
					break;
				case FileState.ActivationFailedOnce:
					state = FileState.ActivationFailedTwice;
					flag = true;
					break;
				case FileState.ActivationFailedTwice:
					state = FileState.ActivationFailedNoRetry;
					flag = true;
					break;
				default:
					state = FileState.Unknown;
					flag = false;
					break;
				}
				if (flag)
				{
					try
					{
						bool overwrite = true;
						File.Copy(text, storeFileState.Destination, overwrite);
						storeFileState.State = FileState.Activated;
						LogHelper.Instance.Log("StoreFileService: file activated for ID: " + id, LogEntryType.Debug);
					}
					catch (Exception e)
					{
						storeFileState.State = state;
						errorList.Add(Error.NewError("SFS0313", "File copy failed for store file ID: " + id, e));
					}
				}
			}
			storeFileState.Timestamp = DateTime.Now;
			File.WriteAllText(path2, ObjectExtensions.ToJson(storeFileState));
		}
		catch (Exception e2)
		{
			errorList.Add(Error.NewError("SFS0032", "Unhandled exception occurred in StoreFileService.Activate.", e2));
		}
		return errorList;
	}

	private ErrorList GetCurrentState(out StoreFilePollRequestList storeFilePollRequestList)
	{
		ErrorList errorList = new ErrorList();
		storeFilePollRequestList = new StoreFilePollRequestList();
		try
		{
			if (!Directory.Exists(m_root))
			{
				errorList.Add(Error.NewError("GCS02", "No directory found at " + m_root, "No storefile state will be sent to the server"));
				return errorList;
			}
			string[] files = Directory.GetFiles(m_root, '*' + ".state", SearchOption.TopDirectoryOnly);
			LogHelper.Instance.Log("StoreFileService: GetCurrentState files: " + ObjectExtensions.ToJson(files), LogEntryType.Debug);
			string[] array = files;
			foreach (string text in array)
			{
				try
				{
					StoreFileState storeFileState = StringExtensions.ToObject<StoreFileState>(File.ReadAllText(text));
					if (storeFileState.State == FileState.Staged || storeFileState.State == FileState.ActivationFailedOnce || storeFileState.State == FileState.ActivationFailedTwice)
					{
						errorList.AddRange(Activate(storeFileState.StoreFilePollRequest.StoreFile));
					}
					storeFilePollRequestList.StoreFilePollRequests.Add(storeFileState.StoreFilePollRequest);
					LogHelper.Instance.Log("StoreFileService: StoreFilePollRequest added: " + ObjectExtensions.ToJson(storeFileState.StoreFilePollRequest), LogEntryType.Debug);
				}
				catch (Exception e)
				{
					errorList.Add(Error.NewError("GCS01", "Error parsing storefile state file at " + text, e));
					LogHelper.Instance.Log("Error parsing storefile state file at " + text, e);
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Error.NewError("GCS02", "Unhandled exception occurred in GetCurrentState()", e2));
		}
		return errorList;
	}

	private IEnumerable<Error> ProcessChangeSet(StoreFileChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (changeSet == null)
			{
				errorList.Add(Error.NewError("PCS01", "ChangeSet is null", "Make sure that there is a changeset to process"));
				return errorList;
			}
			string path = Path.Combine(m_root, changeSet.StoreFile.ToString());
			string text = Path.ChangeExtension(path, ".state");
			string text2 = Path.ChangeExtension(path, ".staged");
			switch (changeSet.Action)
			{
			case StoreFileAction.Update:
				errorList.AddRange(UpdateStoreFile(changeSet, text, text2));
				break;
			case StoreFileAction.Delete:
			{
				string destinationPath = string.Empty;
				try
				{
					StoreFileState storeFileState = StringExtensions.ToObject<StoreFileState>(File.ReadAllText(text));
					destinationPath = storeFileState.Destination;
				}
				catch (Exception e)
				{
					errorList.Add(Error.NewError("PCS02", "An exception occurred in parsing the state file at: " + text, e));
				}
				finally
				{
					errorList.AddRange(DeleteStoreFile(destinationPath, text, text2));
				}
				break;
			}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Error.NewError("SFS0092", "Unhandled exception occurred in StoreFileService.ProcessChangeSet.", e2));
		}
		return errorList;
	}

	private static IEnumerable<Error> DeleteStoreFile(string destinationPath, string statePath, string filePath)
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(Delete(destinationPath));
		errorList.AddRange(Delete(filePath));
		errorList.AddRange(Delete(statePath));
		return errorList;
	}

	private static IEnumerable<Error> Delete(string path)
	{
		List<Error> list = new List<Error>();
		try
		{
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception e)
		{
			list.Add(Error.NewError("SFS0099", "Error trying to delete path: " + path, e));
		}
		return list;
	}

	private IEnumerable<Error> UpdateStoreFile(StoreFileChangeSetData changeSet, string statePath, string stagedFilePath)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			byte[] array = Convert.FromBase64String(changeSet.Base64Data);
			string a = ByteArrayExtensions.ToASCIISHA1Hash(array);
			if (!string.Equals(a, changeSet.DataHash, StringComparison.Ordinal))
			{
				errorList.Add(Error.NewError("USF03", "Hash check failed for storefile changeset " + changeSet.Name, "Aborting storefile update"));
				return errorList;
			}
			StoreFileState storeFileState = new StoreFileState();
			storeFileState.Destination = changeSet.Path;
			storeFileState.State = FileState.Staged;
			storeFileState.Timestamp = DateTime.Now;
			storeFileState.StoreFilePollRequest = new StoreFilePollRequest
			{
				StoreFile = changeSet.StoreFile,
				StoreFileData = changeSet.StoreFileData,
				SyncId = changeSet.SyncId
			};
			StoreFileState instance = storeFileState;
			File.WriteAllBytes(stagedFilePath, array);
			File.WriteAllText(statePath, ObjectExtensions.ToJson(instance));
			errorList.AddRange(Activate(changeSet.StoreFile));
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("USF01", "Unknown error in Updating StoreFile", e));
		}
		return errorList;
	}
}
