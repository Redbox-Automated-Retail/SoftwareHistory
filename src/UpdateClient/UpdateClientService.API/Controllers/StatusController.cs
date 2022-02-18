using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
	private ILogger<StatusController> _logger;

	private IStatusService _status;

	public StatusController(ILogger<StatusController> logger, IStatusService status)
	{
		_logger = logger;
		_status = status;
	}

	[HttpGet("versions")]
	[ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> FileVersions()
	{
		return (await _status.GetFileVersions()).ToObjectResult();
	}

	[HttpPost("versions")]
	[ProducesResponseType(200, Type = typeof(FileVersionDataResponse))]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> FileVersions(IEnumerable<string> filePaths)
	{
		return (await _status.GetFileVersions(filePaths)).ToObjectResult();
	}
}
