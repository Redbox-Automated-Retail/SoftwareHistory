using System.Threading.Tasks;
using KioskClientService.API.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/configuration")]
[ApiController]
public class ConfigurationController : ControllerBase
{
	private readonly IKioskConfiguration _kioskConfiguration;

	public ConfigurationController(IOptionsSnapshotKioskConfiguration kioskConfiguration)
	{
		_kioskConfiguration = kioskConfiguration;
	}

	[HttpGet("current")]
	[ProducesResponseType(typeof(KioskConfigurationResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<IKioskConfiguration>> GetCurrentKioskConfiguration()
	{
		KioskConfigurationResponse response = null;
		await Task.Run(delegate
		{
			response = new KioskConfigurationResponse
			{
				KioskConfiguration = _kioskConfiguration
			};
		});
		return response.ToObjectResult();
	}
}
