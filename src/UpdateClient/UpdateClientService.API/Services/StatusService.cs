using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services;

public class StatusService : IStatusService
{
	private readonly ILogger<StatusService> _logger;

	private readonly IOptionsMonitor<AppSettings> _settings;

	private const string FILE_NOT_FOUND = "FileNotFound";

	public StatusService(ILogger<StatusService> logger, IOptionsMonitor<AppSettings> settings)
	{
		_logger = logger;
		_settings = settings;
	}

	public async Task<FileVersionDataResponse> GetFileVersions()
	{
		FileVersionDataResponse fileVersionDataResponse = new FileVersionDataResponse();
		IEnumerable<string> enumerable = _settings?.CurrentValue?.FileVersionPaths;
		if (enumerable == null || !enumerable.Any())
		{
			_logger.LogErrorWithSource("No filepaths found or specified.", "GetFileVersions");
			fileVersionDataResponse.StatusCode = HttpStatusCode.BadRequest;
			return fileVersionDataResponse;
		}
		return await GetFileVersions(enumerable);
	}

	public async Task<FileVersionDataResponse> GetFileVersions(IEnumerable<string> filepaths)
	{
		FileVersionDataResponse fileVersionDataResponse = new FileVersionDataResponse();
		try
		{
			List<FileVersionData> results = new List<FileVersionData>();
			filepaths.AsParallel().ForAll(delegate(string file)
			{
				if (File.Exists(file))
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(file);
					results.Add(new FileVersionData
					{
						Name = versionInfo.FileName,
						Version = versionInfo.FileVersion
					});
				}
				else
				{
					results.Add(new FileVersionData
					{
						Name = Path.GetFileName(file),
						Version = "FileNotFound"
					});
				}
			});
			fileVersionDataResponse.Data = results;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception occurred in StatusService.GetFileVersions");
			fileVersionDataResponse.StatusCode = HttpStatusCode.InternalServerError;
		}
		return fileVersionDataResponse;
	}
}
