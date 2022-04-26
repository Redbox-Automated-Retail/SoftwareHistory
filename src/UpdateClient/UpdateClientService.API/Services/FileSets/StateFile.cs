using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets;

public class StateFile
{
	private static readonly ILogger<StateFile> _logger = SharedLogger.Factory.CreateLogger<StateFile>();

	private static object _lock = new object();

	private long _fileSetId;

	private long _revisionId;

	private long _inProgressRevisionId;

	private FileSetState _fileSetState;

	private FileSetState _inProgressFileSetState;

	private const string FileSetStateActiveExt = ".active";

	private const string FileSetStateInProgressExt = ".inprogress";

	internal long FileSetId => _fileSetId;

	internal long RevisionId
	{
		get
		{
			return _revisionId;
		}
		set
		{
			_revisionId = value;
		}
	}

	internal long InProgressRevisionId
	{
		get
		{
			return _inProgressRevisionId;
		}
		set
		{
			_inProgressRevisionId = value;
		}
	}

	internal FileSetState FileSetState
	{
		get
		{
			return _fileSetState;
		}
		set
		{
			_fileSetState = value;
		}
	}

	internal FileSetState InProgressFileSetState => _inProgressFileSetState;

	internal StateFile(long fileSetId)
	{
		_fileSetId = fileSetId;
	}

	internal bool HasInProgress()
	{
		return _inProgressRevisionId > 0;
	}

	internal bool HasActive()
	{
		return _revisionId > 0;
	}

	internal List<Error> Activate()
	{
		_revisionId = InProgressRevisionId;
		_inProgressRevisionId = 0L;
		FileSetState = FileSetState.Active;
		DeleteInProgress();
		return Save();
	}

	internal void SetInProgressState(FileSetState state)
	{
		ClientFileSetState clientFileSetState = new ClientFileSetState
		{
			FileSetId = _fileSetId,
			RevisionId = _revisionId,
			FileSetState = state
		};
		if (state == FileSetState.Active)
		{
			_revisionId = _inProgressRevisionId;
			_inProgressRevisionId = 0L;
			_fileSetState = state;
		}
		else
		{
			_inProgressFileSetState = state;
		}
		Save();
	}

	internal List<Error> Load()
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				string activeStatePath = GetActiveStatePath();
				string inProgressStatePath = GetInProgressStatePath();
				_revisionId = 0L;
				_inProgressRevisionId = 0L;
				_fileSetState = FileSetState.Active;
				_inProgressFileSetState = FileSetState.InProgress;
				if (File.Exists(activeStatePath))
				{
					try
					{
						ClientFileSetState clientFileSetState = File.ReadAllText(activeStatePath).ToObject<ClientFileSetState>();
						_revisionId = clientFileSetState.RevisionId;
					}
					catch (Exception arg)
					{
						list.Add(new Error
						{
							Message = $"StateFile.Load Error parsing state file deleting {activeStatePath} Exception: {arg}"
						});
						Shared.SafeDelete(activeStatePath);
					}
				}
				if (File.Exists(inProgressStatePath))
				{
					try
					{
						ClientFileSetState clientFileSetState2 = File.ReadAllText(inProgressStatePath).ToObject<ClientFileSetState>();
						_inProgressRevisionId = clientFileSetState2.RevisionId;
						_inProgressFileSetState = clientFileSetState2.FileSetState;
					}
					catch (Exception arg2)
					{
						list.Add(new Error
						{
							Message = $"StateFile.Load Error parsing state file deleting {inProgressStatePath} Exception {arg2}"
						});
						Shared.SafeDelete(inProgressStatePath);
					}
				}
			}
		}
		catch (Exception arg3)
		{
			list.Add(new Error
			{
				Message = $"StateManager.Load An unhandled exception occurred. Exception {arg3}"
			});
		}
		return list;
	}

	internal List<Error> Delete()
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				Shared.SafeDelete(GetActiveStatePath());
				Shared.SafeDelete(GetInProgressStatePath());
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"StateManager.Delete An unhandled exception occurred. Exception {arg}"
			});
		}
		return list;
	}

	internal List<Error> DeleteInProgress()
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				Shared.SafeDelete(GetInProgressStatePath());
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"StateManager.DeleteInProgress An unhandled exception occurred. Exception {arg}"
			});
		}
		return list;
	}

	internal List<Error> Save()
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				string activeStatePath = GetActiveStatePath();
				string inProgressStatePath = GetInProgressStatePath();
				if (_revisionId == 0L)
				{
					Shared.SafeDelete(activeStatePath);
				}
				else
				{
					File.WriteAllText(activeStatePath, new ClientFileSetState
					{
						FileSetId = _fileSetId,
						RevisionId = _revisionId,
						FileSetState = _fileSetState
					}.ToJson());
				}
				if (_inProgressRevisionId == 0L)
				{
					Shared.SafeDelete(inProgressStatePath);
				}
				else
				{
					File.WriteAllText(inProgressStatePath, new ClientFileSetState
					{
						FileSetId = _fileSetId,
						RevisionId = _inProgressRevisionId,
						FileSetState = _inProgressFileSetState
					}.ToJson());
				}
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"StateManager.Save An unhandled exception occurred. Exception {arg}"
			});
		}
		return list;
	}

	internal static List<Error> CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
	{
		List<Error> list = new List<Error>();
		try
		{
			StateFile stateFile = new StateFile(set.FileSetId);
			list.AddRange(stateFile.Load());
			if (list.Count > 0)
			{
				return list;
			}
			stateFile.InProgressRevisionId = set.RevisionId;
			stateFile.SetInProgressState(FileSetState.InProgress);
			list.AddRange(stateFile.Save());
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"StateManager.CreateFromState An unhandled exception occurred. Exception {arg}"
			});
		}
		return list;
	}

	internal static List<Error> GetCurrentState(out FileSetPollRequestList fileSetPollRequestList)
	{
		List<Error> list = new List<Error>();
		fileSetPollRequestList = new FileSetPollRequestList();
		try
		{
			lock (_lock)
			{
				list.AddRange(GetAllFiles(out var stateFiles));
				if (list.Any())
				{
					return list;
				}
				Dictionary<long, FileSetPollRequest> list2 = new Dictionary<long, FileSetPollRequest>();
				stateFiles.Where((ClientFileSetState item) => item.FileSetState == FileSetState.Active).ToList().ForEach(delegate(ClientFileSetState item)
				{
					FileSetPollRequest value = new FileSetPollRequest
					{
						FileSetId = item.FileSetId,
						FileSetRevisionId = item.RevisionId,
						FileSetState = item.FileSetState
					};
					list2[item.FileSetId] = value;
				});
				stateFiles.Where((ClientFileSetState item) => item.FileSetState != FileSetState.Active).ToList().ForEach(delegate(ClientFileSetState item)
				{
					FileSetPollRequest value = new FileSetPollRequest
					{
						FileSetId = item.FileSetId,
						FileSetRevisionId = item.RevisionId,
						FileSetState = item.FileSetState
					};
					list2[item.FileSetId] = value;
				});
				FileSetPollRequestList fileSetPollRequestList2 = new FileSetPollRequestList();
				foreach (KeyValuePair<long, FileSetPollRequest> item in list2)
				{
					fileSetPollRequestList2.FileSetPollRequests.Add(item.Value);
				}
				fileSetPollRequestList = fileSetPollRequestList2;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"StateManager.GetCurrentState Unhandled exception occurred Exception {arg}"
			});
		}
		return list;
	}

	internal static List<Error> GetAllInProgress(out List<StateFile> list)
	{
		List<Error> list2 = new List<Error>();
		list = new List<StateFile>();
		try
		{
			list2.AddRange(GetAll(out var list3));
			if (list2.Any())
			{
				return list2;
			}
			List<StateFile> list4 = list;
			list = list3.Where((StateFile item) => item.HasInProgress()).ToList();
		}
		catch (Exception arg)
		{
			list2.Add(new Error
			{
				Message = $"StateManager.GetCurrentState Unhandled exception occurred Exception {arg}"
			});
		}
		return list2;
	}

	internal static List<Error> GetAll(out List<StateFile> list)
	{
		List<Error> list2 = new List<Error>();
		list = new List<StateFile>();
		try
		{
			lock (_lock)
			{
				List<ClientFileSetState> stateFiles = null;
				list2.AddRange(GetAllFiles(out stateFiles));
				if (list2.Any())
				{
					return list2;
				}
				List<long> list3 = stateFiles.Select((ClientFileSetState a) => a.FileSetId).Distinct().ToList();
				List<StateFile> tempList = list;
				list3.ForEach(delegate(long fileSetId)
				{
					StateFile stateFile = new StateFile(fileSetId);
					List<Error> source = stateFile.Load();
					if (source.Any())
					{
						Shared.SafeDelete(GetActiveStatePath(fileSetId));
						Shared.SafeDelete(GetInProgressStatePath(fileSetId));
					}
					else
					{
						tempList.Add(stateFile);
					}
				});
			}
		}
		catch (Exception arg)
		{
			list2.Add(new Error
			{
				Message = $"StateManager.GetAll Unhandled exception occurred Exception {arg}"
			});
		}
		return list2;
	}

	private static List<Error> GetAllFiles(out List<ClientFileSetState> stateFiles)
	{
		stateFiles = new List<ClientFileSetState>();
		List<Error> list = new List<Error>();
		try
		{
			if (!Directory.Exists(Constants.FileSetsRoot))
			{
				list.Add(new Error
				{
					Message = "StateManager.GetAllFiles No directory found at " + Constants.FileSetsRoot + " . No fileset state will be sent to the server"
				});
				return list;
			}
			List<string> list2 = Directory.GetFiles(Constants.FileSetsRoot, '*' + ".active", SearchOption.TopDirectoryOnly).ToList();
			List<string> list3 = Directory.GetFiles(Constants.FileSetsRoot, '*' + ".inprogress", SearchOption.TopDirectoryOnly).ToList();
			List<string> list4 = new List<string>();
			list2.ForEach(list4.Add);
			list3.ForEach(list4.Add);
			_logger.LogDebugWithSource("State files: " + list4.ToJson(), "GetAllFiles", "/sln/src/UpdateClientService.API/Services/FileSets/StateFile.cs");
			foreach (string item in list4)
			{
				try
				{
					ClientFileSetState clientFileSetState = File.ReadAllText(item).ToObject<ClientFileSetState>();
					if (clientFileSetState != null)
					{
						stateFiles.Add(clientFileSetState);
					}
					else
					{
						_logger.LogWarningWithSource("State for path " + item + " was null", "GetAllFiles", "/sln/src/UpdateClientService.API/Services/FileSets/StateFile.cs");
						Shared.SafeDelete(item);
					}
					stateFiles.Add(clientFileSetState);
				}
				catch (Exception arg)
				{
					list.Add(new Error
					{
						Message = $"StateManager.GetAllFiles Error parsing state file deleting {item} Exception {arg}"
					});
					Shared.SafeDelete(item);
				}
			}
		}
		catch (Exception arg2)
		{
			list.Add(new Error
			{
				Message = $"StateManager.GetAllFiles Unhandled exception occurred. Exception {arg2}"
			});
		}
		return list;
	}

	private string GetActiveStatePath()
	{
		return GetActiveStatePath(_fileSetId);
	}

	private string GetInProgressStatePath()
	{
		return GetInProgressStatePath(_fileSetId);
	}

	private static string GetActiveStatePath(long fileSetId)
	{
		return Path.ChangeExtension(Path.Combine(Constants.FileSetsRoot, fileSetId.ToString()), ".active");
	}

	private static string GetInProgressStatePath(long fileSetId)
	{
		return Path.ChangeExtension(Path.Combine(Constants.FileSetsRoot, fileSetId.ToString()), ".inprogress");
	}
}
