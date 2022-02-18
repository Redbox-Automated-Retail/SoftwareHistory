using System.Threading;

namespace UpdateClientService.API.Services.FileSets;

public static class FileSetServiceLock
{
	public static SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);
}
