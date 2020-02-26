using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Products.MoreLikeThis;
using Microsoft.AspNetCore.Mvc;

namespace KioskClientService.API.Controllers;

[Route("api/productgroups")]
[ApiController]
public class ProductGroupsController : ControllerBase
{
	private readonly IMoreLikeThisService _moreLikeThisService;

	public ProductGroupsController(IMoreLikeThisService moreLikeThisService)
	{
		_moreLikeThisService = moreLikeThisService;
	}

	[HttpGet("{id}/morelikethis")]
	public async Task<ActionResult<GetMoreLikeThisResponse>> MoreLikeThis(long id)
	{
		GetMoreLikeThisResponse mLT = _moreLikeThisService.GetMLT(id);
		if (mLT.Errors.Any())
		{
			return StatusCode(500, mLT);
		}
		if (!mLT.MoreLikeThis.Any())
		{
			return NotFound(mLT);
		}
		return mLT;
	}

	[HttpPost("morelikethis/refresh")]
	public async Task<ActionResult<BaseResponse>> CacheMoreLikeThisTitle([FromBody] List<long> productGroupIds)
	{
		Task.Run(() => _moreLikeThisService.RefreshMoreLikeThisAsync(productGroupIds));
		return Ok();
	}
}
