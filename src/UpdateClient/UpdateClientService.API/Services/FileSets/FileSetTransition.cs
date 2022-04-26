using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetTransition : IFileSetTransition
{
	[Flags]
	private enum MoveFileFlags
	{
		MOVEFILE_REPLACE_EXISTING = 1,
		MOVEFILE_COPY_ALLOWED = 2,
		MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		MOVEFILE_WRITE_THROUGH = 8
	}

	private const string ResultTableName = "Results";

	private const string FileSetJsonFileName = "fileset.json";

	private ClientFileSetRevision _revision;

	private IFileCacheService _fileCacheService;

	private IHttpService _httpService;

	private IWindowsServiceFunctions _winServiceFunctions;

	private readonly ICommandLineService _cmd;

	private AppSettings _appSettings;

	private ILogger<FileSetTransition> _logger;

	private const string FileSetChangeSetPath = "staging";

	public FileSetTransition(IFileCacheService fileCacheService, IHttpService httpService, IWindowsServiceFunctions winServiceFunctions, ICommandLineService cmd, IOptions<AppSettings> appSettings, ILogger<FileSetTransition> logger)
	{
		_fileCacheService = fileCacheService;
		_httpService = httpService;
		_winServiceFunctions = winServiceFunctions;
		_cmd = cmd;
		_appSettings = appSettings.Value;
		_logger = logger;
	}

	public FileSetTransition Initialize(ClientFileSetRevision revision)
	{
		_revision = revision;
		return this;
	}

	internal List<Error> Stage()
	{
		List<Error> errors = new List<Error>();
		try
		{
			string stagingFileSetRevisionPath = GetStagingFileSetRevisionPath();
			if (!Directory.Exists(stagingFileSetRevisionPath))
			{
				Directory.CreateDirectory(stagingFileSetRevisionPath);
			}
			List<FileData> fileData = GetFileData();
			fileData.ForEach(delegate(FileData f)
			{
				if (!_fileCacheService.FileExists(_revision.FileSetId, f.FileId, f.FileRevisionId))
				{
					errors.Add(new Error
					{
						Message = $"FileSetTransition.Stage FileId {f.FileId} Revision {f.FileRevisionId} doesn't exist"
					});
				}
			});
			if (errors.Any())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				if (!_fileCacheService.CopyFileToPath(_revision.FileSetId, f.FileId, f.FileRevisionId, f.StagePath))
				{
					errors.Add(new Error
					{
						Message = $"FileSetTransition.Stage FileId {f.FileId} Revision {f.FileRevisionId} couldn't be copied to it's staging directory"
					});
				}
			});
			if (errors.Any())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				if (!CheckHash(f.StagePath, f.Hash))
				{
					errors.Add(new Error
					{
						Message = $"FileSetTransition.Stage FileId {f.FileId} Revision {f.FileRevisionId} hash check failed"
					});
				}
			});
			return errors;
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"FileSetTransition.Stage An unhandled exception occurred . Exception: {arg}"
			});
		}
		return errors;
	}

	internal List<Error> CheckMeetsDependency(Dictionary<long, FileSetDependencyState> dependencyStates, out bool meetsDependencies)
	{
		List<Error> list = new List<Error>();
		meetsDependencies = false;
		try
		{
			IEnumerable<ClientFileSetRevisionDependency> dependencies = _revision.Dependencies;
			if (dependencies.Count() == 0)
			{
				meetsDependencies = true;
				return list;
			}
			bool meetsAll = true;
			dependencies.ToList().ForEach(delegate(ClientFileSetRevisionDependency dep)
			{
				if (!dependencyStates.ContainsKey(dep.DependsOnFileSetId))
				{
					_logger.LogWarningWithSource($"FileSet {_revision.FileSetId} Revision {_revision.RevisionId} Message: Depends on FileSet {dep.DependsOnFileSetId} not found.", "CheckMeetsDependency", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
					meetsAll = false;
				}
				else
				{
					FileSetDependencyState fileSetDependencyState = dependencyStates[dep.DependsOnFileSetId];
					if (!fileSetDependencyState.IsDependencyMet(dep.DependencyType, dep.MinimumVersion, dep.MaximumVersion))
					{
						_logger.LogWarningWithSource($"FileSet {_revision.FileSetId} Revision {_revision.RevisionId} Message: Dependency not met. Expected version {ExpectedDependencyVersionInfo(dep, fileSetDependencyState)}. Current version: '{((fileSetDependencyState.InProgressRevisionId > 0) ? fileSetDependencyState.InProgressVersion : fileSetDependencyState.ActiveVersion)}'", "CheckMeetsDependency", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						meetsAll = false;
					}
				}
			});
			meetsDependencies = meetsAll;
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetTransition.CheckMeetsDependency An unhandled exception occurred . Exception: {arg}"
			});
		}
		return list;
	}

	private static string ExpectedDependencyVersionInfo(ClientFileSetRevisionDependency dep, FileSetDependencyState ds)
	{
		return new
		{
			FileSetId = ds.FileSetId,
			DependencyType = dep.DependencyType.ToString(),
			MinimumVersion = dep.MinimumVersion,
			MaximumVersion = dep.MaximumVersion
		}.ToJson();
	}

	private async Task CallUpdateClientInstaller(Action<string> setError)
	{
		APIResponse<object> aPIResponse = await _httpService.SendRequestAsync<object>(_appSettings.UpdateClientInstallerUrl, "api/activation?waitForResponse=false", _revision, HttpMethod.Post, null, isForm: false, null, "CallUpdateClientInstaller", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs", null, logRequest: false);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			setError("Something went wrong while calling UpdateClientInstaller. Error -> " + aPIResponse.Errors.ToJson());
		}
	}

	internal List<Error> BeforeActivate(out bool success, Action<string> setError)
	{
		success = true;
		List<Error> list = new List<Error>();
		try
		{
			FileSetJsonModel fileSetJsonModel = GetFileSetJsonModel(beforeActivations: true);
			if (fileSetJsonModel == null)
			{
				_logger.LogWarningWithSource("FileSetJson was null skipping BeforeActivate.", "BeforeActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return list;
			}
			if (fileSetJsonModel?.WindowsServiceName == "updateclient$service")
			{
				Task.Run(async delegate
				{
					await CallUpdateClientInstaller(setError);
				});
				success = false;
				return list;
			}
			if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName) && _winServiceFunctions.ServiceExists(fileSetJsonModel.WindowsServiceName))
			{
				_winServiceFunctions.StopService(fileSetJsonModel.WindowsServiceName);
			}
			List<FileData> fileData = GetFileData();
			FileData fileData2 = fileData.Where((FileData f) => string.Equals(Path.GetFileName(f.FileDestination), "beforeactivate.ps1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (fileData2 != null)
			{
				success = RunScript(fileData2.StagePath);
				if (!success)
				{
					list.Add(new Error
					{
						Message = $"There was an error running beforeactivate script for FileSet {_revision.FileSetId} Revision {_revision.RevisionId}"
					});
				}
				return list;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetTransition.BeforeActivate An unhandled exception occurred . Exception: {arg}"
			});
		}
		return list;
	}

	private FileSetJsonModel GetFileSetJsonModel(bool beforeActivations)
	{
		FileSetJsonModel result = null;
		List<FileData> fileData = GetFileData();
		FileData fileData2 = fileData.Where((FileData item) => Path.GetFileName(item.FileDestination).ToLower() == "fileset.json").FirstOrDefault();
		if (fileData2 == null)
		{
			_logger.LogWarningWithSource("fileset.json is not exist for revision: " + _revision.ToJson(), "GetFileSetJsonModel", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
			return null;
		}
		string text = (beforeActivations ? File.ReadAllText(fileData2?.StagePath) : File.ReadAllText(fileData2?.FileDestination));
		try
		{
			result = text.ToObject<FileSetJsonModel>();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Couldn't deserialize fileset.json file. FileContent was: " + text + " . Path: was: " + fileData2.StagePath);
		}
		return result;
	}

	internal List<Error> AfterActivate()
	{
		List<Error> list = new List<Error>();
		try
		{
			FileSetJsonModel fileSetJsonModel = GetFileSetJsonModel(beforeActivations: false);
			if (fileSetJsonModel == null)
			{
				_logger.LogWarningWithSource("FileSetJson was null skipping AfterActivate.", "AfterActivate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return list;
			}
			if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName))
			{
				list.AddRange(AfterActivateHandleWindowsServices(fileSetJsonModel));
			}
			List<FileData> fileData = GetFileData();
			FileData fileData2 = fileData.Where((FileData f) => string.Equals(Path.GetFileName(f.FileDestination), "afteractivate.ps1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (fileData2 != null)
			{
				if (!RunScript(fileData2.FileDestination))
				{
					list.Add(new Error
					{
						Message = $"There was an error running afteractivate script for FileSet {_revision.FileSetId} Revision {_revision.RevisionId}"
					});
				}
				return list;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetTransition.AfterActivate An unhandled exception occurred. Exception: {arg}"
			});
		}
		return list;
	}

	private List<Error> AfterActivateHandleWindowsServices(FileSetJsonModel fileSetJson)
	{
		List<Error> list = new List<Error>();
		if (!_winServiceFunctions.ServiceExists(fileSetJson.WindowsServiceName))
		{
			if (!string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceStartFile))
			{
				string displayName = (string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceDisplayName) ? fileSetJson.WindowsServiceName : fileSetJson.WindowsServiceDisplayName);
				string binPath = Path.Combine(fileSetJson.InstallPath, fileSetJson.WindowsServiceStartFile);
				_winServiceFunctions.InstallService(fileSetJson.WindowsServiceName, displayName, binPath);
			}
			else
			{
				string message = "There's a windowsServiceName: " + fileSetJson.WindowsServiceName + " in fileSetJson but windowsServiceStartPath was null!";
				_logger.LogErrorWithSource(message, "AfterActivateHandleWindowsServices", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				list.Add(new Error
				{
					Message = message
				});
			}
		}
		_winServiceFunctions.StartService(fileSetJson.WindowsServiceName);
		return list;
	}

	internal List<Error> Activate()
	{
		List<Error> errors = new List<Error>();
		try
		{
			List<FileData> fileData = GetFileData();
			fileData.ForEach(delegate(FileData f)
			{
				if (!File.Exists(f.StagePath))
				{
					errors.Add(new Error
					{
						Message = $"FileSetTransition.Activate Staged FileId {f.FileId} Revision {f.FileRevisionId} doesn't exist"
					});
				}
			});
			if (errors.Any())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				string directoryName = Path.GetDirectoryName(f.FileDestination);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				try
				{
					if (File.Exists(f.FileDestination))
					{
						File.Delete(f.FileDestination);
					}
					File.Move(f.StagePath, f.FileDestination);
				}
				catch (Exception ex)
				{
					try
					{
						_logger.LogInfoWithSource(f.FileDestination + " is locked by anther process attempting to retry.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						_logger.LogErrorWithSource(ex, "Error from activate copy - retrying.");
						ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
						manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(100.0));
						manualResetEvent.Close();
						if (File.Exists(f.FileDestination))
						{
							File.Delete(f.FileDestination);
						}
						File.Move(f.StagePath, f.FileDestination);
					}
					catch (Exception ex2)
					{
						_logger.LogInfoWithSource(f.FileDestination + " is locked by anther process doing a deferred move.", "Activate", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
						_logger.LogErrorWithSource(ex2, "Error from activate copy.");
						string tempFileName = Path.GetTempFileName();
						if (File.Exists(tempFileName))
						{
							File.Delete(tempFileName);
						}
						File.Move(f.StagePath, tempFileName);
						MoveLockedFileSystemEntry(tempFileName, f.FileDestination);
					}
				}
			});
			errors.Any();
			return errors;
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"FileSetTransition.Activate An unhandled exception occurred . Exception: {arg}"
			});
		}
		return errors;
	}

	private static void MoveLockedFileSystemEntry(string source, string destination)
	{
		MoveFileFlags moveFileFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
		if (!Directory.Exists(source) && !Directory.Exists(destination))
		{
			moveFileFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
		}
		MoveFileEx(source, destination, moveFileFlags);
	}

	[DllImport("kernel32.dll")]
	private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

	private List<FileData> GetFileData()
	{
		List<FileData> result = new List<FileData>();
		string path = GetStagingFileSetRevisionPath();
		_revision.Files.ToList().ForEach(delegate(ClientFileSetFile f)
		{
			FileData item = new FileData
			{
				FileId = f.FileId,
				FileRevisionId = f.FileRevisionId,
				Size = f.ContentSize,
				Hash = f.ContentHash,
				StagePath = Path.Combine(path, _fileCacheService.GetFileName(f.FileId, f.FileRevisionId)),
				FileDestination = f.FileDestination
			};
			result.Add(item);
		});
		return result;
	}

	private bool RunScript(string script)
	{
		try
		{
			if (string.IsNullOrEmpty(script))
			{
				_logger.LogWarningWithSource($"Script for FileSet {_revision.FileSetId} Revision {_revision.RevisionId} was empty. Skipping...", "RunScript", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
				return true;
			}
			if (Path.GetExtension(script) == ".file")
			{
				string text = Path.GetTempFileName().Replace(".tmp", ".ps1");
				File.Copy(script, text);
				bool result = _cmd.TryExecutePowerShellScriptFromFile(text);
				File.Delete(text);
				return result;
			}
			return _cmd.TryExecutePowerShellScriptFromFile(script);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred while running script");
			return false;
		}
	}

	private string GetStagingDirectory()
	{
		return Path.Combine(Constants.FileSetsRoot, "staging");
	}

	private string GetStagingFileSetPath()
	{
		return Path.Combine(GetStagingDirectory(), _revision.FileSetId.ToString());
	}

	private string GetStagingFileSetRevisionPath()
	{
		return Path.Combine(GetStagingFileSetPath(), _revision.RevisionId.ToString());
	}

	private bool CheckHash(string filePath, string hash)
	{
		List<Error> list = new List<Error>();
		try
		{
			using FileStream stream = File.OpenRead(filePath);
			string sHA1Hash = stream.GetSHA1Hash();
			return sHA1Hash == hash;
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileSetTransition An unhandled exception occurred in CheckHash.. Exception: {arg}"
			});
		}
		return false;
	}
}
