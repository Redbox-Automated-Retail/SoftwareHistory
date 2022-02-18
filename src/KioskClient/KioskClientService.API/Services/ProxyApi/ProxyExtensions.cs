using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public static class ProxyExtensions
{
	public static List<Header> AddCompression(this List<Header> headers)
	{
		if (headers == null)
		{
			headers = new List<Header>();
		}
		headers.Add(new Header("Accept-Encoding", "gzip"));
		return headers;
	}
}
