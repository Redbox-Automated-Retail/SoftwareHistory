using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Controllers;

public abstract class AbstractBaseIotController : ControllerBase
{
	private IIoTCommandClient _iotCommandCleint;

	public AbstractBaseIotController(IIoTCommandClient iotCommandClient)
	{
		_iotCommandCleint = iotCommandClient;
	}

	internal async Task<IActionResult> PerformIoTCommand<T>(IoTCommandModel request, PerformIoTCommandParameters parameters = null) where T : ObjectResult
	{
		if (!base.ModelState.IsValid)
		{
			return new BadRequestObjectResult(base.ModelState);
		}
		IoTCommandResponse<T> ioTCommandResponse = await _iotCommandCleint.PerformIoTCommand<T>(request, parameters);
		if (parameters == null || parameters.WaitForResponse)
		{
			return ToStatusCodeResult(ioTCommandResponse.Payload);
		}
		ObjectResult objectResult = new ObjectResult(null);
		objectResult.StatusCode = ioTCommandResponse?.StatusCode ?? 500;
		return ToStatusCodeResult(objectResult);
	}

	private StatusCodeResult ToStatusCodeResult(object objectResult)
	{
		return new StatusCodeResult((objectResult as ObjectResult)?.StatusCode ?? 500);
	}
}
