using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Personalization;

public class PersonalizationConfigs
{
	private const string AUTH_HEADER = "Authorization";

	public bool LiveCall { get; set; }

	public string Url { get; set; }

	public int? Timeout { get; set; }

	public string ApiKey { get; set; }

	public List<Header> GetAuthHeaders()
	{
		List<Header> list = new List<Header>();
		list.Add(new Header("Authorization", "bearer " + ApiKey));
		return list;
	}
}
