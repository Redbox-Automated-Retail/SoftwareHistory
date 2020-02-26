using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public interface IMoreLikeThisService
{
	GetMoreLikeThisResponse GetMLT(long productGroupId);

	Task RefreshMoreLikeThisAsync(List<long> productGroupIds);
}
