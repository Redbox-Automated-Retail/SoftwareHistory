using System.Threading.Tasks;
using KioskClientService.API.Services.ProfileData;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataUpdateController : Controller
{
	private readonly IProfileDataService _profileDataService;

	public DataUpdateController(IProfileDataService profileDataService)
	{
		_profileDataService = profileDataService;
	}

	[HttpPost("triggerproductdataupdate")]
	[ProducesResponseType(typeof(TriggerProductDataUpdateResponse), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> TriggerProductDataUpdate()
	{
		return (await _profileDataService.TriggerProductDataUpdate()).ToObjectResult();
	}
}
