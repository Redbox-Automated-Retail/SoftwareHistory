using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Controllers.Models;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class BrokerController : AbstractBaseIotController
{
	public BrokerController(IIoTCommandClient iotService)
		: base(iotService)
	{
	}

	[HttpPost]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(504)]
	public async Task<IActionResult> Ping([FromHeader(Name = "app-name")] string appName, [FromHeader(Name = "x-redbox-activityid")] string messageId, PingRequest pingRequest)
	{
		return await PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			Version = 1,
			RequestId = messageId,
			AppName = appName,
			Command = CommandEnum.KioskPing,
			Payload = pingRequest,
			QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
		}, new PerformIoTCommandParameters
		{
			IoTTopic = "$aws/rules/kioskping",
			WaitForResponse = false
		});
	}

	[HttpPost]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(504)]
	public async Task<IActionResult> Register([FromHeader(Name = "app-name")] string appName, [FromHeader(Name = "x-redbox-activityid")] string messageId, RegisterRequest request)
	{
		return await PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			Version = 1,
			RequestId = messageId,
			AppName = appName,
			Command = CommandEnum.BrokerRegister,
			Payload = request,
			QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
		}, new PerformIoTCommandParameters
		{
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		});
	}

	[HttpPost]
	[ProducesResponseType(typeof(OkResult), 200)]
	[ProducesResponseType(typeof(BadRequestObjectResult), 400)]
	[ProducesResponseType(500)]
	[ProducesResponseType(504)]
	public async Task<IActionResult> UnRegister([FromHeader(Name = "app-name")] string appName, [FromHeader(Name = "x-redbox-activityid")] string messageId, UnRegisterRequest request)
	{
		return await PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			Version = 1,
			RequestId = messageId,
			AppName = appName,
			Command = CommandEnum.BrokerUnRegister,
			Payload = request,
			QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce
		}, new PerformIoTCommandParameters
		{
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		});
	}
}
