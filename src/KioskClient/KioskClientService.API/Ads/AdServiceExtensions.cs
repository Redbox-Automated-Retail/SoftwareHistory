using System.Collections.Generic;
using System.Linq;

namespace KioskClientService.API.Ads;

public static class AdServiceExtensions
{
	public static IAdService GetImplementation<T>(this IEnumerable<IAdService> services)
	{
		return services.FirstOrDefault((IAdService service) => service.GetType() == typeof(T));
	}
}
