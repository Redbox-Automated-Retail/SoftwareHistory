using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
	private readonly IMqttProxy _ioTService;

	private readonly IKioskFilesService _logService;

	public ValuesController(IMqttProxy ioTService, IKioskFilesService logService)
	{
		_ioTService = ioTService;
		_logService = logService;
	}

	[HttpGet]
	public ActionResult<IEnumerable<string>> Get()
	{
		return new string[2] { "value1", "value2" };
	}

	[HttpGet("{id}")]
	public ActionResult<string> Get(int id)
	{
		return "value";
	}

	[HttpPost]
	public void Post([FromBody] string value)
	{
	}

	[HttpPut("{id}")]
	public void Put(int id, [FromBody] string value)
	{
	}

	[HttpDelete("{id}")]
	public void Delete(int id)
	{
	}

	[HttpPost("testPublishMessage")]
	public async Task<IActionResult> GetFileLogs()
	{
		await _ioTService.PublishMessageAsync("qwda", "sa");
		return Ok();
	}
}
