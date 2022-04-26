using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public class ChangeSetFileService : IChangeSetFileService
{
	private readonly ILogger<ChangeSetFileService> _logger;

	private IFileSetRevisionDownloader _revisionDownloader;

	private readonly IFileSetDownloader _fileSetDownloader;

	private readonly IFileSetTransition _fileSetTransition;

	private static object _lock = new object();

	private const string FileSetChangeSetExt = ".changeSet";

	private const string FileSetChangeSetPath = "changesets";

	public ChangeSetFileService(ILogger<ChangeSetFileService> logger, IFileSetRevisionDownloader revisionDownloader, IFileSetDownloader fileSetDownloader, IFileSetTransition fileSetTransition)
	{
		_logger = logger;
		_revisionDownloader = revisionDownloader;
		_fileSetDownloader = fileSetDownloader;
		_fileSetTransition = fileSetTransition;
	}

	public bool IsStaged(RevisionChangeSet revisionChangeSet)
	{
		if (revisionChangeSet.State != ChangesetState.Error)
		{
			return revisionChangeSet.State >= ChangesetState.Staged;
		}
		return false;
	}

	public bool IsActivated(RevisionChangeSet revisionChangeSet)
	{
		return revisionChangeSet.State == ChangesetState.Activated;
	}

	public bool IsInNeedsDependency(RevisionChangeSet revisionChangeSet)
	{
		return revisionChangeSet.State == ChangesetState.ActivationDependencyCheck;
	}

	public bool IsInError(RevisionChangeSet revisionChangeSet)
	{
		return revisionChangeSet.State == ChangesetState.Error;
	}

	public void CleanUp()
	{
		try
		{
			DateTime dateTime = DateTime.Now.AddDays(-10.0);
			string changeSetDirectory = GetChangeSetDirectory();
			if (!Directory.Exists(changeSetDirectory))
			{
				_logger.LogErrorWithSource("Error, no directory found at " + changeSetDirectory, "CleanUp", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				return;
			}
			string[] files = Directory.GetFiles(changeSetDirectory, '*' + ".changeSet", SearchOption.TopDirectoryOnly);
			string[] array = files;
			foreach (string text in array)
			{
				DateTime creationTime = File.GetCreationTime(text);
				if (creationTime <= dateTime)
				{
					Shared.SafeDelete(text);
					_logger.LogInfoWithSource("Deleting file > 60 days old: " + text, "CleanUp", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred");
		}
	}

	public void CreateChangeSetDirectory()
	{
		string changeSetDirectory = GetChangeSetDirectory();
		Directory.CreateDirectory(changeSetDirectory);
	}

	public void Delete(RevisionChangeSet changeset)
	{
		Delete(changeset.FileSetId, changeset.RevisionId);
	}

	public List<Error> Delete(long fileSetId, long revisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				string changeSetPath = GetChangeSetPath(fileSetId, revisionId);
				Shared.SafeDelete(changeSetPath);
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"ChangeSetManager.CleanUp -> An unhandled exception occurred. Exception {arg}"
			});
		}
		return list;
	}

	public List<Error> CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
	{
		List<Error> list = new List<Error>();
		try
		{
			RevisionChangeSet revisionChangeSet = ClientToRevisionChangeSet(set);
			Save(revisionChangeSet);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"ChangeSetFile.CreateFromChangeSet An unhandled exception occurred. Exception: {arg}"
			});
		}
		return list;
	}

	internal List<Error> Save(RevisionChangeSet revisionChangeSet)
	{
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				File.WriteAllText(GetChangeSetPath(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId), revisionChangeSet.ToJson());
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Save");
			list.Add(new Error
			{
				Message = $"ChangeSetFile.Save An unhandled exception occurred. Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> GetAllFiles(out List<RevisionChangeSet> chageSetFiles)
	{
		chageSetFiles = new List<RevisionChangeSet>();
		List<Error> list = new List<Error>();
		try
		{
			lock (_lock)
			{
				CreateChangeSetDirectory();
				string[] files = Directory.GetFiles(GetChangeSetDirectory(), '*' + ".changeSet", SearchOption.TopDirectoryOnly);
				_logger.LogDebug("ChangeSetManager.GetAllFiles -> " + files.ToJson());
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						RevisionChangeSet item = File.ReadAllText(text).ToObject<RevisionChangeSet>();
						chageSetFiles.Add(item);
					}
					catch (Exception arg)
					{
						list.Add(new Error
						{
							Message = $"ChangeSetManager.GetAllFiles Error parsing changeset file deleting {text} . Exception: {arg}"
						});
						Shared.SafeDelete(text);
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "GetAllFiles");
			list.Add(new Error
			{
				Message = $"ChangeSetManager.GetAllFiles Unhandled exception occurred. Exception: {ex}"
			});
		}
		return list;
	}

	private List<DownloadData> AddDownloads(List<DownloadData> allDownloads, RevisionChangeSet revisionChangeSet)
	{
		List<DownloadData> fileSetDownloadDatas = new List<DownloadData>();
		allDownloads.ForEach(delegate(DownloadData d)
		{
			string[] array = d.Key.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Count() > 1 && Convert.ToInt64(array[1]) == revisionChangeSet.FileSetId)
			{
				fileSetDownloadDatas.Add(d);
			}
		});
		return fileSetDownloadDatas;
	}

	public async Task<List<Error>> ProcessChangeSet(List<DownloadData> allDownloads, RevisionChangeSet revisionChangeSet)
	{
		List<Error> errors = new List<Error>();
		try
		{
			List<DownloadData> thisFileSetDownloads = AddDownloads(allDownloads, revisionChangeSet);
			if (revisionChangeSet.State == ChangesetState.Received)
			{
				List<Error> list = errors;
				list.AddRange(await CheckReceived(revisionChangeSet, thisFileSetDownloads));
			}
			if (revisionChangeSet.State == ChangesetState.DownloadingRevision)
			{
				errors.AddRange(CheckDownloadingRevision(revisionChangeSet, thisFileSetDownloads));
			}
			if (revisionChangeSet.State == ChangesetState.DownloadedRevision)
			{
				List<Error> list = errors;
				list.AddRange(await CheckDownloadedRevision(revisionChangeSet, thisFileSetDownloads));
			}
			if (revisionChangeSet.State == ChangesetState.DownloadingFileSet)
			{
				errors.AddRange(CheckDownloadingFileSet(revisionChangeSet, thisFileSetDownloads));
			}
			if (revisionChangeSet.State == ChangesetState.DownloadedFileSet)
			{
				errors.AddRange(CheckDownloadedFileSet(revisionChangeSet));
			}
			if (revisionChangeSet.State == ChangesetState.Staging)
			{
				errors.AddRange(CheckStagingFileSet(revisionChangeSet));
			}
			if (revisionChangeSet.State == ChangesetState.Staged)
			{
				errors.AddRange(CheckStagedFileSet(revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessChangeSet");
			errors.Add(new Error
			{
				Message = $"ChangeSetManager.ProcessChangeSet An unhandled exception occurred. Exception: {ex}"
			});
		}
		return errors;
	}

	public List<Error> ProcessActivationDependencyCheck(Dictionary<long, FileSetDependencyState> dependencyStates, RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivationDependencyCheck";
		List<Error> list = new List<Error>();
		try
		{
			if (revisionChangeSet.State == ChangesetState.ActivationDependencyCheck)
			{
				list.AddRange(CheckActivationDependency(dependencyStates, revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivationDependencyCheck");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred. Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> ProcessActivationPending(RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivationPending";
		List<Error> list = new List<Error>();
		try
		{
			if (revisionChangeSet.State == ChangesetState.ActivationPending)
			{
				list.AddRange(CheckActivationPending(revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivationPending");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred . Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> ProcessActivationBeforeActions(RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivationBeforeActions";
		List<Error> list = new List<Error>();
		try
		{
			if (revisionChangeSet.State == ChangesetState.ActivationBeforeActions)
			{
				list.AddRange(CheckActivationBeforeActions(revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivationBeforeActions");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred . Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> ProcessActivating(RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivating";
		List<Error> list = new List<Error>();
		try
		{
			if (revisionChangeSet.State == ChangesetState.Activating)
			{
				list.AddRange(CheckActivating(revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivating");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred. Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> ProcessActivationAfterActions(RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivationAfterActions";
		List<Error> list = new List<Error>();
		try
		{
			if (revisionChangeSet.State == ChangesetState.ActivationAfterActions)
			{
				list.AddRange(CheckActivationAfterActions(revisionChangeSet));
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivationAfterActions");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred . Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> ProcessActivated(RevisionChangeSet revisionChangeSet)
	{
		string arg = "ChangeSetFile.ProcessActivated";
		List<Error> list = new List<Error>();
		try
		{
			_ = revisionChangeSet.State;
			_ = 35;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "ProcessActivated");
			list.Add(new Error
			{
				Message = $"{arg} An unhandled exception occurred . Exception: {ex}"
			});
		}
		return list;
	}

	private async Task<List<Error>> CheckReceived(RevisionChangeSet revisionChangeSet, List<DownloadData> downloads)
	{
		List<Error> errors = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.Received)
		{
			return errors;
		}
		_logger.LogInfoWithSource($"ChangeSet received fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckReceived", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			if (_revisionDownloader.Exists(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId))
			{
				SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
				return errors;
			}
			if (!_revisionDownloader.Exists(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId))
			{
				DownloadData currentDownloadData = _revisionDownloader.GetCurrentDownloadData(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId, downloads);
				if (!_revisionDownloader.DownloadExists(currentDownloadData))
				{
					DownloadData downloadData = await _revisionDownloader.AddDownload(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId, revisionChangeSet.FileHash, revisionChangeSet.Path, revisionChangeSet.DownloadPriority);
					if (downloadData == null)
					{
						SetError("RevisionDownloader.AddDownloader failed", revisionChangeSet);
						return errors;
					}
					downloads.Add(downloadData);
				}
			}
			SetState(ChangesetState.DownloadingRevision, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckReceived");
			SetError("Unhandled exception in CheckReceived", revisionChangeSet);
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in CheckReceived . Exception: {ex}"
			});
		}
		return errors;
	}

	private List<Error> CheckDownloadingRevision(RevisionChangeSet revisionChangeSet, List<DownloadData> downloads)
	{
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.DownloadingRevision)
		{
			return list;
		}
		_logger.LogInfoWithSource($"ChangeSet downloading fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckDownloadingRevision", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			if (_revisionDownloader.Exists(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId))
			{
				SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
				return list;
			}
			DownloadData currentDownloadData = _revisionDownloader.GetCurrentDownloadData(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId, downloads);
			if (!_revisionDownloader.DownloadExists(currentDownloadData))
			{
				SetError("CheckDownloadingRevision - Download doesn't exist when it should", revisionChangeSet);
				return list;
			}
			if (_revisionDownloader.IsDownloadError(currentDownloadData))
			{
				SetError("CheckDownloadingRevision - Download is in an error state", revisionChangeSet);
				return list;
			}
			if (_revisionDownloader.IsDownloadComplete(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId, currentDownloadData))
			{
				if (_revisionDownloader.CompleteDownload(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId, currentDownloadData))
				{
					SetState(ChangesetState.DownloadedRevision, revisionChangeSet);
					return list;
				}
				SetError("CheckDownloadingRevision - CompleteDownload failed", revisionChangeSet);
				return list;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckDownloadingRevision");
			list.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in CheckReceived . Exception: {ex}"
			});
			SetError("Unhandled exception in CheckDownloadingRevision", revisionChangeSet);
		}
		return list;
	}

	private async Task<List<Error>> CheckDownloadedRevision(RevisionChangeSet revisionChangeSet, List<DownloadData> downloads)
	{
		List<Error> errors = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.DownloadedRevision)
		{
			return errors;
		}
		_logger.LogInfoWithSource($"ChangeSet downloaded fileset: {revisionChangeSet.FileSetId}, revision: {revisionChangeSet.RevisionId}, patchRevision: {revisionChangeSet.PatchRevisionId}", "CheckDownloadedRevision", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return errors;
			}
			if (_fileSetDownloader.IsDownloaded(revision))
			{
				SetState(ChangesetState.DownloadedFileSet, revisionChangeSet);
				return errors;
			}
			List<Error> list = errors;
			list.AddRange(await _fileSetDownloader.DownloadFileSet(revision, downloads, revisionChangeSet.DownloadPriority));
			if (errors.Any())
			{
				SetError("An error while starting fileset downloads", revisionChangeSet);
			}
			else
			{
				SetState(ChangesetState.DownloadingFileSet, revisionChangeSet);
			}
			return errors;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckDownloadedRevision");
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in CheckReceive . Exception: {ex}"
			});
			SetError("Unhandled exception in CheckDownloadedRevision", revisionChangeSet);
		}
		return errors;
	}

	private List<Error> CheckDownloadingFileSet(RevisionChangeSet revisionChangeSet, List<DownloadData> downloads)
	{
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.DownloadingFileSet)
		{
			return list;
		}
		_logger.LogInfoWithSource($"ChangeSet CheckDownloadingFileSet fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckDownloadingFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return list;
			}
			if (downloads == null || downloads.Count == 0)
			{
				SetError("Download does not exist for FileSet", revisionChangeSet);
				return list;
			}
			list.AddRange(_fileSetDownloader.CompleteDownloads(revision, downloads));
			if (list.Any())
			{
				SetError("Complete downloads failed", revisionChangeSet);
				return list;
			}
			if (_fileSetDownloader.IsDownloading(downloads))
			{
				return list;
			}
			if (_fileSetDownloader.IsDownloaded(revision))
			{
				SetState(ChangesetState.DownloadedFileSet, revisionChangeSet);
				return list;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckDownloadingFileSet");
			list.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in CheckReceived . Exception: {ex}"
			});
			SetError("Unhandled exception in CheckDownloadingFileSet", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckDownloadedFileSet(RevisionChangeSet revisionChangeSet)
	{
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.DownloadedFileSet)
		{
			return list;
		}
		_logger.LogInfoWithSource($"ChangeSet CheckDownloadedFileSet fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckDownloadedFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			SetState(ChangesetState.Staging, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckDownloadedFileSet");
			list.Add(new Error
			{
				Message = $"ChangeSetFile.CheckDownloadedFileSet An unhandled exception occurred . Exception: {ex}"
			});
			SetError("Unhandled exception in CheckDownloadedFileSet", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckStagingFileSet(RevisionChangeSet revisionChangeSet)
	{
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.Staging)
		{
			return list;
		}
		_logger.LogInfoWithSource($"ChangeSet CheckStagingFileSet fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckStagingFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet, list);
				return list;
			}
			FileSetTransition fileSetTransition = _fileSetTransition.Initialize(revision);
			list.AddRange(fileSetTransition.Stage());
			if (list.Any())
			{
				SetError("Staging revision failed", revisionChangeSet, list);
				return list;
			}
			SetState(ChangesetState.Staged, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckStagingFileSet");
			list.Add(new Error
			{
				Message = $"ChangeSetFile.CheckStagingFileSet An unhandled exception occurred. Exception: {ex}"
			});
			SetError("Unhandled exception in CheckStagingFileSet", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckStagedFileSet(RevisionChangeSet revisionChangeSet)
	{
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.Staged)
		{
			return list;
		}
		_logger.LogInfoWithSource($"ChangeSet CheckStagedFileSet fileset {revisionChangeSet.FileSetId} revision {revisionChangeSet.RevisionId}", "CheckStagedFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		try
		{
			SetState(ChangesetState.ActivationDependencyCheck, revisionChangeSet);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckStagedFileSet");
			list.Add(new Error
			{
				Message = $"ChangeSetFile.CheckStagedFileSet An unhandled exception occurred . Exception: {ex}"
			});
			SetError("Unhandled exception in CheckStagedFileSet", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckActivationDependency(Dictionary<long, FileSetDependencyState> dependencyStates, RevisionChangeSet revisionChangeSet)
	{
		string text = "ChangeSetFile.CheckActivationDependencyCheck";
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.ActivationDependencyCheck)
		{
			return list;
		}
		Log(text, "Started", revisionChangeSet);
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			IEnumerable<ClientFileSetRevisionDependency> dependencies = revision.Dependencies;
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return list;
			}
			FileSetTransition fileSetTransition = _fileSetTransition.Initialize(revision);
			list.AddRange(fileSetTransition.CheckMeetsDependency(dependencyStates, out var meetsDependencies));
			if (list.Any())
			{
				SetError("Activating revision failed", revisionChangeSet);
				return list;
			}
			if (meetsDependencies)
			{
				SetState(ChangesetState.ActivationPending, revisionChangeSet);
				return list;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "CheckActivationDependency");
			list.Add(new Error
			{
				Message = $"An unhandled exception occurred. Exception: {ex}"
			});
			SetError($"Unhandled exception in {text}", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckActivationPending(RevisionChangeSet revisionChangeSet)
	{
		string text = "ChangeSetFile.CheckActivationPending";
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.ActivationPending)
		{
			return list;
		}
		Log(text, "Started", revisionChangeSet);
		try
		{
			DateTime now = DateTime.Now;
			if (revisionChangeSet.ActiveOn > now)
			{
				Log("ChangeSetFile.CheckActivationPending", $"ActiveOn date ({revisionChangeSet.ActiveOn}) is in the future.", revisionChangeSet);
				return list;
			}
			if (!IsValidTime(revisionChangeSet))
			{
				Log("ChangeSetFile.CheckActivationPending", $"ActivationStartTime and ActivationEndTime are not valid yet ActivateStartTime {revisionChangeSet.ActivateStartTime} ActivateEndTime {revisionChangeSet.ActivateEndTime}.", revisionChangeSet);
				return list;
			}
			SetState(ChangesetState.ActivationBeforeActions, revisionChangeSet);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"{text} An unhandled exception occurred . Exception: {arg}"
			});
			SetError($"Unhandled exception in {text}", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckActivationBeforeActions(RevisionChangeSet revisionChangeSet)
	{
		string text = "ChangeSetFile.CheckActivationBeforeActions";
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.ActivationBeforeActions)
		{
			return list;
		}
		Log(text, "Started", revisionChangeSet);
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return list;
			}
			FileSetTransition fileSetTransition = _fileSetTransition.Initialize(revision);
			list.AddRange(fileSetTransition.BeforeActivate(out var success, delegate(string message)
			{
				SetError(message, revisionChangeSet);
			}));
			if (list.Any())
			{
				SetError("Unhandled exception in " + text + ". Errors -> " + list.ToJson(), revisionChangeSet);
				return list;
			}
			if (success)
			{
				SetState(ChangesetState.Activating, revisionChangeSet);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred");
			list.Add(new Error
			{
				Message = $"{text} An unhandled exception occurred Exception: {ex}"
			});
			SetError($"Unhandled exception in {text}", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckActivating(RevisionChangeSet revisionChangeSet)
	{
		string text = "ChangeSetFile.CheckActivating";
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.Activating)
		{
			return list;
		}
		Log(text, "Started", revisionChangeSet);
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return list;
			}
			FileSetTransition fileSetTransition = _fileSetTransition.Initialize(revision);
			list.AddRange(fileSetTransition.Activate());
			if (list.Any())
			{
				SetError("Activating revision failed", revisionChangeSet);
				return list;
			}
			SetState(ChangesetState.ActivationAfterActions, revisionChangeSet);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $" {text} An unhandled exception occurred . Exception: {arg}"
			});
			SetError($"Unhandled exception in {text}", revisionChangeSet);
		}
		return list;
	}

	private List<Error> CheckActivationAfterActions(RevisionChangeSet revisionChangeSet)
	{
		string methodName = "ChangeSetFile.CheckActivationAfterActions";
		List<Error> list = new List<Error>();
		if (revisionChangeSet.State != ChangesetState.ActivationAfterActions)
		{
			return list;
		}
		Log(methodName, "Started", revisionChangeSet);
		try
		{
			ClientFileSetRevision revision = _revisionDownloader.GetRevision(revisionChangeSet.FileSetId, revisionChangeSet.RevisionId, revisionChangeSet.PatchRevisionId);
			if (revision == null)
			{
				SetError("Revision does not exist", revisionChangeSet);
				return list;
			}
			FileSetTransition fileSetTransition = _fileSetTransition.Initialize(revision);
			list.AddRange(fileSetTransition.AfterActivate());
			if (list.Any())
			{
				_logger.LogWarning($"Revision with FileSetId {revisionChangeSet?.FileSetId}, RevisionId {revisionChangeSet?.RevisionId}, RevisionId {revisionChangeSet?.PatchRevisionId} activated with errors -> {list.ToJson()}");
			}
			SetState(ChangesetState.Activated, revisionChangeSet);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"CheckActivationAfterActions -> An unhandled exception occurred. Exception: {arg}"
			});
			SetState(ChangesetState.Activated, revisionChangeSet);
		}
		return list;
	}

	private void Log(string methodName, string message, RevisionChangeSet revisionChangeSet)
	{
		_logger.LogInfoWithSource($"{methodName} FileSet {revisionChangeSet.FileSetId} Revision {revisionChangeSet.RevisionId} Message: {message}", "Log", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
	}

	private void SetState(ChangesetState state, RevisionChangeSet revisionChangeSet)
	{
		revisionChangeSet.State = state;
		Save(revisionChangeSet);
	}

	internal bool IsValidTime(RevisionChangeSet revisionChangeSet)
	{
		if (string.IsNullOrEmpty(revisionChangeSet.ActivateStartTime) || string.IsNullOrEmpty(revisionChangeSet.ActivateEndTime))
		{
			return true;
		}
		if (!TimeSpan.TryParse(revisionChangeSet.ActivateStartTime, out var result))
		{
			return true;
		}
		if (!TimeSpan.TryParse(revisionChangeSet.ActivateEndTime, out var result2))
		{
			return true;
		}
		if (result == result2)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		if (result2 < result)
		{
			if (!(now.TimeOfDay <= result2))
			{
				return now.TimeOfDay >= result;
			}
			return true;
		}
		if (now.TimeOfDay >= result)
		{
			return now.TimeOfDay <= result2;
		}
		return false;
	}

	private void SetError(string message, RevisionChangeSet revisionChangeSet, List<Error> errors = null)
	{
		if (errors != null && errors.Count > 0)
		{
			message = message + ", Errors -> " + errors.ToJson();
		}
		_logger.LogErrorWithSource($"Setting ChangeSet Error for FileSet: {revisionChangeSet.FileSetId} Revision: {revisionChangeSet.RevisionId} ({message})", "SetError", "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
		revisionChangeSet.Message = message;
		revisionChangeSet.RetryCount++;
		if (revisionChangeSet.RetryCount > 2)
		{
			SetState(ChangesetState.Error, revisionChangeSet);
		}
		else
		{
			SetState(ChangesetState.Received, revisionChangeSet);
		}
	}

	private static string GetChangeSetDirectory()
	{
		string text = Path.Combine(Constants.FileSetsRoot, "changesets");
		Directory.CreateDirectory(text);
		return text;
	}

	private string GetChangeSetPath(long fileSetId, long revisionId)
	{
		return Path.ChangeExtension(Path.Combine(GetChangeSetDirectory(), $"{fileSetId}-{revisionId}"), ".changeSet");
	}

	private static RevisionChangeSet ClientToRevisionChangeSet(ClientFileSetRevisionChangeSet cs)
	{
		return new RevisionChangeSet
		{
			FileSetId = cs.FileSetId,
			RevisionId = cs.RevisionId,
			Received = DateTime.Now,
			Downloaded = null,
			Staged = null,
			Activated = null,
			State = ChangesetState.Received,
			Message = string.Empty,
			RetryCount = 0,
			Path = cs.Path,
			CompressionType = cs.CompressionType,
			ContentHash = cs.ContentHash,
			FileHash = cs.FileHash,
			PatchRevisionId = cs.PatchRevisionId,
			DownloadOn = cs.DownloadOn.ToLocalTime(),
			DownloadPriority = cs.DownloadPriority,
			ActiveOn = cs.ActiveOn.ToLocalTime(),
			ActivateStartTime = cs.ActivateStartTime,
			ActivateEndTime = cs.ActivateEndTime,
			DownloadUrl = cs.DownloadUrl
		};
	}
}
