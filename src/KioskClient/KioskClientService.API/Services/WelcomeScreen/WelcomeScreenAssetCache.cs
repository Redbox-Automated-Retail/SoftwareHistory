using System;
using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.WelcomeScreen;

public class WelcomeScreenAssetCache : IPersistentData
{
	public DateTime LatestChange { get; set; }

	public List<WelcomeScreenAssetInfo> WelcomeScreenAssets { get; set; } = new List<WelcomeScreenAssetInfo>();

	public bool SyncComplete { get; set; }
}
