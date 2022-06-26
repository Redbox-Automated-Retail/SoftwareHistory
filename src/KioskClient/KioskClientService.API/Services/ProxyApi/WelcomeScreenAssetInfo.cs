using System;

namespace KioskClientService.API.Services.ProxyApi;

public class WelcomeScreenAssetInfo
{
	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public int? Order { get; set; }

	public int? ProductGroupId { get; set; }

	public string FileType { get; set; }

	public string Filename { get; set; }

	public long Size { get; set; }

	public DateTime LastUpdateUtc { get; set; }

	public string PresignedUrl { get; set; }
}
