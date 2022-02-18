using System.Threading;

namespace UpdateClientService.API.Services.DownloadService;

public static class DownloadServiceLock
{
	public static SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);
}
