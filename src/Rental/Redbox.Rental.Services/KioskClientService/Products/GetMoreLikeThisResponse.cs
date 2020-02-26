using System.Collections.Generic;

namespace Redbox.Rental.Services.KioskClientService.Products;

public class GetMoreLikeThisResponse : BaseResponse
{
	public List<long> MoreLikeThis { get; set; }
}
