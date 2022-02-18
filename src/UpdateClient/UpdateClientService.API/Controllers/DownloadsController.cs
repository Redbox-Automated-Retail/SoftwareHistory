using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.DownloadService.Responses;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadsController : ControllerBase
{
	private readonly IDownloadService _downloadService;

	private readonly IDownloadFilesService _downloadFilesService;

	private readonly ILogger<DownloadsController> _logger;

	private readonly IMemoryCache _cache;

	public DownloadsController(IDownloadService downloadService, IDownloadFilesService downloadFilesService, ILogger<DownloadsController> logger, IMemoryCache cache)
	{
		_downloadService = downloadService;
		_downloadFilesService = downloadFilesService;
		_logger = logger;
		_cache = cache;
	}

	[HttpHead("s3/proxy/{key}/{type}")]
	[ProducesResponseType(typeof(RedirectResult), 301)]
	public async Task<ActionResult> ProxyForS3Head(string key, DownloadPathType type)
	{
		key = Uri.UnescapeDataString(key);
		if (!_cache.TryGetValue<string>($"{key}/{type}/head", out var value))
		{
			value = await _downloadService.GetProxiedS3Url(key, type, isHead: true);
			if (!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out Uri _))
			{
				_cache.Set($"{key}/{type}/head", value, TimeSpan.FromMinutes(30.0));
			}
		}
		return new RedirectResult(value, permanent: true);
	}

	[HttpGet("s3/proxy/{key}/{type}")]
	[ProducesResponseType(typeof(RedirectResult), 301)]
	public async Task<RedirectResult> ProxyForS3Get(string key, DownloadPathType type)
	{
		key = Uri.UnescapeDataString(key);
		if (!_cache.TryGetValue<string>($"{key}/{type}/get", out var value))
		{
			value = await _downloadService.GetProxiedS3Url(key, type, isHead: true);
			if (!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out Uri _))
			{
				_cache.Set($"{key}/{type}/get", value, TimeSpan.FromMinutes(30.0));
			}
		}
		return new RedirectResult(value, permanent: true);
	}

	[HttpPost]
	[ProducesResponseType(typeof(GetFileResponse), 202)]
	[ProducesResponseType(typeof(GetFileResponse), 500)]
	public async Task<ActionResult<GetFileResponse>> StartDownload(Uri url, string hash = null)
	{
		string key = Guid.NewGuid().ToString();
		List<Error> list = await _downloadService.AddDownload(key, hash, url.ToString(), DownloadPriority.Normal);
		GetFileResponse getFileResponse = (list.Any() ? new GetFileResponse(list)
		{
			Key = key
		} : new GetFileResponse
		{
			Key = key
		});
		return getFileResponse.ToObjectResult();
	}

	[HttpGet]
	[ProducesResponseType(typeof(GetDownloadStatusesResponse), 200)]
	public async Task<ActionResult> GetDownloadStatuses()
	{
		List<DownloadData> statuses = await _downloadService.GetDownloads();
		GetDownloadStatusesResponse getDownloadStatusesResponse = new GetDownloadStatusesResponse
		{
			Statuses = statuses
		};
		return getDownloadStatusesResponse.ToObjectResult();
	}

	[HttpGet("{key}")]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 200)]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 404)]
	public async Task<ActionResult> GetDownloadStatus(string key)
	{
		GetDownloadStatusResponse getDownloadStatusResponse = new GetDownloadStatusResponse(await _downloadService.GetDownloads(), key);
		return getDownloadStatusResponse.ToObjectResult();
	}

	[HttpDelete("{key}")]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 200)]
	[ProducesResponseType(typeof(GetDownloadStatusResponse), 500)]
	public async Task<ActionResult> CancelDownload(string key)
	{
		List<Error> list = await _downloadService.CancelDownload(key);
		DeleteDownloadResponse deleteDownloadResponse = (list.Any() ? new DeleteDownloadResponse(list) : new DeleteDownloadResponse());
		return deleteDownloadResponse.ToObjectResult();
	}

	[HttpPost("{key}")]
	[ProducesResponseType(typeof(CompleteDownloadResponse), 200)]
	[ProducesResponseType(typeof(CompleteDownloadResponse), 500)]
	public async Task<ActionResult<CompleteDownloadResponse>> CompleteDownload(string key)
	{
		List<Error> list = await _downloadService.CompleteDownload(key);
		CompleteDownloadResponse completeDownloadResponse = (list.Any() ? new CompleteDownloadResponse(list) : new CompleteDownloadResponse());
		return completeDownloadResponse.ToObjectResult();
	}
}
