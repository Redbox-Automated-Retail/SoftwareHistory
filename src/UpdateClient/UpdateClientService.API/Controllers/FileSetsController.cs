using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.FileSets;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileSetsController : ControllerBase
{
	private readonly IFileSetService _fileSetService;

	private readonly IKioskFileSetVersionsService _versionsService;

	private readonly IFileSetCleanup _cleanup;

	public FileSetsController(IFileSetService fileSetService, IKioskFileSetVersionsService versionsService, IFileSetCleanup cleanup)
	{
		_fileSetService = fileSetService;
		_versionsService = versionsService;
		_cleanup = cleanup;
	}

	[HttpPost("invoke")]
	[ProducesResponseType(typeof(OkResult), 200)]
	public async Task<ActionResult> Invoke()
	{
		await _fileSetService.Invoke();
		return Ok();
	}

	[HttpPost("versions")]
	[ProducesResponseType(typeof(OkResult), 200)]
	public async Task<ActionResult> ReportVersions()
	{
		await _versionsService.ReportFileVersions();
		return Ok();
	}

	[HttpPost("changesets")]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult> ProcessChangeSet(ClientFileSetRevisionChangeSet changeSet)
	{
		List<Error> list = await _fileSetService.ProcessChangeSet(changeSet);
		if (list != null && list.Count > 0)
		{
			return new ObjectResult(list)
			{
				StatusCode = 500
			};
		}
		return Ok();
	}

	[HttpPost("cleanup")]
	[ProducesResponseType(typeof(OkResult), 200)]
	public ActionResult Cleanup()
	{
		_cleanup.Run();
		return Ok();
	}
}
