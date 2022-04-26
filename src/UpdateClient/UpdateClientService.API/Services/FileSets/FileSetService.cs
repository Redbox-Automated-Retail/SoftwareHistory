using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.IoT.FileSets;
using UpdateClientService.API.Services.Kernel;
using UpdateClientService.API.Services.KioskEngine;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetService : IFileSetService, IInvocable
{
	private static readonly string _root = Constants.FileSetsRoot;

	private static readonly TimeSpan _processLockTimeout = TimeSpan.FromSeconds(3.0);

	private readonly IFileCacheService _fileCacheService;

	private readonly IDownloadService _downloadService;

	private readonly IDownloader _downloader;

	private readonly IChangeSetFileService _changeSetFileService;

	private readonly ILogger<FileSetService> _logger;

	private readonly IKioskFileSetVersionsService _versionsService;

	private readonly IKernelService _kernelService;

	private readonly IKioskEngineService _kioskEngineService;

	private readonly FileSetSettings _settings;

	private bool _rebootRequired;

	private SemaphoreSlim Lock => FileSetServiceLock.Lock;

	public bool RebootRequired
	{
		get
		{
			return _rebootRequired;
		}
		internal set
		{
			_rebootRequired = value;
		}
	}

	public FileSetService(IFileCacheService fileCacheService, IDownloadService downloadService, IDownloader downloader, IChangeSetFileService changeSetFileService, ILogger<FileSetService> logger, IKioskFileSetVersionsService versionsService, IKernelService kernelService, IKioskEngineService kioskEngineService, IOptionsMonitor<AppSettings> settings)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_downloader = downloader;
		_changeSetFileService = changeSetFileService;
		_logger = logger;
		_versionsService = versionsService;
		_kernelService = kernelService;
		_kioskEngineService = kioskEngineService;
		_settings = settings.CurrentValue.FileSet;
		Initialize();
	}

	public List<Error> Initialize()
	{
		List<Error> list = new List<Error>();
		try
		{
			Directory.CreateDirectory(_root);
			_changeSetFileService.CreateChangeSetDirectory();
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "Unhandled exception occurred in FileSetService.Initialize. Exception: " + e.GetFullMessage()
			});
		}
		return list;
	}

	public async Task<List<Error>> ProcessPollReply(List<PollReply> pollReplyList)
	{
		List<Error> errors = new List<Error>();
		if (errors.Any() || pollReplyList == null || !pollReplyList.Any())
		{
			return errors;
		}
		try
		{
			IEnumerable<ClientFileSetRevisionChangeSet> fileSets = pollReplyList.Select((PollReply pollReply) => pollReply.Data.ToObject<ClientFileSetRevisionChangeSet>());
			if (fileSets == null || fileSets.Count() == 0)
			{
				return errors;
			}
			try
			{
				if (!(await Lock.WaitAsync(_processLockTimeout)))
				{
					errors.Add(new Error
					{
						Message = "FileSetService.ProcessPollReply Timeout expired prior to acquiring the lock."
					});
					return errors;
				}
			}
			catch (Exception ex)
			{
				errors.Add(new Error
				{
					Message = "FileSetService.ProcessPollReply Could not acquire lock. Exception -> " + ex.Message
				});
				return errors;
			}
			try
			{
				errors.AddRange(ProcessChangeSets(fileSets));
			}
			finally
			{
				SemaphoreSlim semaphoreSlim = Lock;
				if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
				{
					Lock.Release();
				}
			}
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"FS0010 An unhandled exception occurred in FileSetService.ProcessPollReply. Exception: {arg}"
			});
		}
		errors.ForEach(delegate(Error x)
		{
			_logger.LogErrorWithSource(x.Message, "ProcessPollReply", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		});
		return errors;
	}

	public async Task<List<Error>> ProcessChangeSet(ClientFileSetRevisionChangeSet changeSet)
	{
		List<Error> errors = new List<Error>();
		try
		{
			if (changeSet == null)
			{
				_logger.LogWarningWithSource("Changeset was null. Skipping...", "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return errors;
			}
			try
			{
				if (!(await Lock.WaitAsync(_processLockTimeout)))
				{
					errors.Add(new Error
					{
						Message = "FileSetService.ProcessChangeSet Timeout expired prior to acquiring the lock."
					});
					return errors;
				}
			}
			catch (Exception ex)
			{
				errors.Add(new Error
				{
					Message = "FileSetService.ProcessChangeSet Could not acquire lock. Exception -> " + ex.Message
				});
				return errors;
			}
			try
			{
				errors.AddRange(ProcessChangeSets(new List<ClientFileSetRevisionChangeSet> { changeSet }));
				if (TryGetVersion(changeSet, (errors == null || errors.Count <= 0) ? FileSetState.InProgress : FileSetState.Error, out var version))
				{
					await _versionsService.ReportFileVersion(version);
				}
			}
			finally
			{
				SemaphoreSlim semaphoreSlim = Lock;
				if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
				{
					Lock.Release();
				}
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "An unhandled exception occurred in FileSetService.ProcessChangeSet. Exception: " + e.GetFullMessage()
			});
		}
		if (errors.Count > 0)
		{
			_logger.LogErrorWithSource("Something went wrong while processing ChangeSet " + changeSet.ToJson() + ", " + errors.ToJson(), "ProcessChangeSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		}
		return errors;
	}

	public async Task Invoke()
	{
		try
		{
			if (!(await Lock.WaitAsync(_processLockTimeout)))
			{
				_logger.LogErrorWithSource("Timeout expired prior to acquiring the lock.", "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Could not acquire lock");
		}
		try
		{
			List<Error> list = await ProcessChangeSetWork();
			if (list != null && list.Count > 0)
			{
				_logger.LogWarningWithSource("Finished Invoke with errors. " + list.ToJson(), "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
			}
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "Invoke");
		}
		finally
		{
			SemaphoreSlim semaphoreSlim = Lock;
			if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
			{
				Lock.Release();
			}
		}
	}

	private List<Error> ProcessChangeSets(IEnumerable<ClientFileSetRevisionChangeSet> changeSets)
	{
		List<Error> list = new List<Error>();
		_logger.LogInfoWithSource("Executing FileSetService.ProcessChangeSets", "ProcessChangeSets", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
		try
		{
			foreach (ClientFileSetRevisionChangeSet changeSet in changeSets)
			{
				if (changeSet.Action == FileSetAction.Delete)
				{
					list.AddRange(_changeSetFileService.Delete(changeSet.FileSetId, changeSet.RevisionId));
					StateFile stateFile = new StateFile(changeSet.FileSetId);
					list.AddRange(stateFile.Delete());
				}
				else if (changeSet.Action == FileSetAction.Update)
				{
					list.AddRange(FileSetUpdate(changeSet));
				}
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetService.ProcessChangeSets An unhandled exception occurred. Exception: {arg}"
			});
		}
		return list;
	}

	private List<Error> FileSetUpdate(ClientFileSetRevisionChangeSet set)
	{
		List<Error> list = new List<Error>();
		try
		{
			list.AddRange(_changeSetFileService.CreateFromChangeSet(set));
			list.AddRange(StateFile.CreateFromChangeSet(set));
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetService.FileSetUpdate An unhandled exception occurred. Exception: {arg}"
			});
		}
		return list;
	}

	private async Task<List<Error>> ProcessChangeSetWork()
	{
		List<Error> errors = new List<Error>();
		try
		{
			errors = StateFile.GetAllInProgress(out var list);
			if (errors.Count() > 0)
			{
				return errors;
			}
			errors = _changeSetFileService.GetAllFiles(out var chageSetFiles);
			if (errors.Count() > 0)
			{
				return errors;
			}
			var bothGood = (from state in list
				join set in chageSetFiles on new
				{
					FileSetId = state.FileSetId,
					RevisionId = state.InProgressRevisionId
				} equals new { set.FileSetId, set.RevisionId }
				select new
				{
					State = state,
					Set = set
				}).ToList();
			List<StateFile> second = bothGood.Select(anon => anon.State).ToList();
			List<RevisionChangeSet> second2 = bothGood.Select(anon => anon.Set).ToList();
			list.Except(second).ToList().ForEach(delegate(StateFile stateFile)
			{
				stateFile.DeleteInProgress();
			});
			chageSetFiles.Except(second2).ToList().ForEach(delegate(RevisionChangeSet revisionChangeSet)
			{
				_changeSetFileService.Delete(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId);
			});
			List<DownloadData> allDownloads = await _downloadService.GetDownloads(new Regex("^(?!(FILE|SCRIPT|downloadFileList.json)).*"));
			if (bothGood.Count() == 0 && allDownloads.Count() == 0)
			{
				return errors;
			}
			bothGood.ForEach(async anon =>
			{
				List<Error> list3 = errors;
				list3.AddRange(await _changeSetFileService.ProcessChangeSet(allDownloads, anon.Set));
			});
			Dictionary<long, FileSetDependencyState> dependencyStates = GetDependencyStates();
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivationDependencyCheck(dependencyStates, anon.Set));
			});
			bothGood.ForEach(anon =>
			{
				if (_changeSetFileService.IsInNeedsDependency(anon.Set))
				{
					anon.State.SetInProgressState(FileSetState.NeedsDependency);
				}
			});
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivationPending(anon.Set));
			});
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivationBeforeActions(anon.Set));
			});
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivating(anon.Set));
			});
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivationAfterActions(anon.Set));
			});
			bothGood.ForEach(anon =>
			{
				errors.AddRange(_changeSetFileService.ProcessActivated(anon.Set));
			});
			foreach (var item2 in bothGood)
			{
				if (_changeSetFileService.IsActivated(item2.Set))
				{
					item2.State.Activate();
					_changeSetFileService.Delete(item2.Set.FileSetId, item2.Set.RevisionId);
					if (TryGetVersion(item2?.Set, FileSetState.Active, out var version))
					{
						await _versionsService.ReportFileVersion(version);
					}
				}
			}
			foreach (var item in bothGood)
			{
				if (_changeSetFileService.IsInError(item.Set))
				{
					item.State.SetInProgressState(FileSetState.Error);
					bool flag = TryGetVersion(item?.Set, FileSetState.Error, out var version2);
					bool flag2 = flag;
					if (flag2)
					{
						flag2 = await _versionsService.ReportFileVersion(version2);
					}
					if (flag2)
					{
						_changeSetFileService.Delete(item.Set);
						item.State.Delete();
					}
				}
			}
			if (RebootRequired)
			{
				List<Error> list2 = errors;
				list2.AddRange(await Reboot());
			}
			allDownloads?.ForEach(delegate(DownloadData d)
			{
				if (d != null && d.DownloadState == DownloadState.PostDownload)
				{
					_downloader.Complete(d);
				}
			});
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"FileSetService.ProcessChangeSetWork An unhandled exception occurred. Exception: {arg}"
			});
		}
		return errors;
	}

	private bool TryGetVersion(RevisionMetadata changeSetMetadata, FileSetState state, out FileSetPollRequest version)
	{
		version = null;
		try
		{
			if (changeSetMetadata != null && changeSetMetadata.FileSetId > 0 && (changeSetMetadata.RevisionId > 0 || changeSetMetadata.PatchRevisionId > 0))
			{
				version = new FileSetPollRequest
				{
					FileSetId = changeSetMetadata.FileSetId,
					FileSetRevisionId = ((changeSetMetadata.PatchRevisionId > 0) ? changeSetMetadata.PatchRevisionId : changeSetMetadata.RevisionId),
					FileSetState = state
				};
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred.");
		}
		return version != null;
	}

	private Dictionary<long, FileSetDependencyState> GetDependencyStates()
	{
		List<Error> list = new List<Error>();
		Dictionary<long, FileSetDependencyState> dependencies = new Dictionary<long, FileSetDependencyState>();
		try
		{
			list = StateFile.GetAll(out var list2);
			if (list.Count() > 0)
			{
				return null;
			}
			list = _changeSetFileService.GetAllFiles(out var changeSets);
			if (list.Count() > 0)
			{
				return null;
			}
			list2.ForEach(delegate(StateFile item)
			{
				FileSetDependencyState fileSetDependencyState = new FileSetDependencyState
				{
					FileSetId = item.FileSetId,
					IsInProgressStaged = false
				};
				if (item.HasInProgress())
				{
					RevisionChangeSet revisionChangeSet = changeSets.Where((RevisionChangeSet cs) => cs.FileSetId == item.FileSetId).FirstOrDefault();
					if (revisionChangeSet != null)
					{
						ClientFileSetRevision revision = GetRevision(item.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
						if (revision != null)
						{
							fileSetDependencyState.InProgressRevisionId = revisionChangeSet.RevisionId;
							fileSetDependencyState.InProgressVersion = revision.RevisionVersion;
							fileSetDependencyState.IsInProgressStaged = _changeSetFileService.IsStaged(revisionChangeSet);
						}
					}
				}
				if (item.HasActive())
				{
					fileSetDependencyState.ActiveRevisionId = item.RevisionId;
					ClientFileSetRevision revision2 = GetRevision(item.FileSetId, item.RevisionId);
					if (revision2 != null)
					{
						fileSetDependencyState.ActiveVersion = revision2.RevisionVersion;
					}
				}
				dependencies[item.FileSetId] = fileSetDependencyState;
			});
			return dependencies;
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetService.GetDependencyState An unhandled exception occurred . Exception: {arg}"
			});
		}
		return null;
	}

	private ClientFileSetRevision GetRevision(long fileSetId, long revisionId, long patchRevisionId)
	{
		if (_fileCacheService.RevisionExists(fileSetId, revisionId, patchRevisionId) && _fileCacheService.GetRevision(fileSetId, revisionId, patchRevisionId, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	public ClientFileSetRevision GetRevision(long fileSetId, long revisionId)
	{
		if (_fileCacheService.GetRevision(fileSetId, revisionId, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	public ClientFileSetRevision GetRevision(string path)
	{
		if (_fileCacheService.GetRevision(path, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
		}
		return null;
	}

	public FileSetFileInfo GetFileInfo(string path)
	{
		if (_fileCacheService.GetFileInfo(path, out var data))
		{
			return Encoding.ASCII.GetString(data).ToObject<FileSetFileInfo>();
		}
		return null;
	}

	public void MarkRebootRequired()
	{
		_rebootRequired = true;
	}

	private async Task<List<Error>> Reboot()
	{
		List<Error> errors = new List<Error>();
		PerformShutdownResponse performShutdownResponse = await _kioskEngineService.PerformShutdown(_settings.KioskEngineShutdownTimeoutMs, _settings.KioskEngineShutdownTimeoutMs);
		if (!string.IsNullOrWhiteSpace(performShutdownResponse.Error))
		{
			errors.Add(new Error
			{
				Message = performShutdownResponse.Error
			});
		}
		else if (!_kernelService.PerformShutdown(ShutdownType.Reboot))
		{
			errors.Add(new Error
			{
				Message = "Encountered an error while trying to reboot."
			});
		}
		return errors;
	}
}
