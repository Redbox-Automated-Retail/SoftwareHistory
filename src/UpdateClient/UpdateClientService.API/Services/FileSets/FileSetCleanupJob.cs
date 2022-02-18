using System.Diagnostics;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetCleanupJob : IFileSetCleanupJob, IInvocable
{
	private readonly IFileSetCleanup _cleanup;

	private readonly ILogger<FileSetCleanupJob> _logger;

	public FileSetCleanupJob(IFileSetCleanup cleanup, ILogger<FileSetCleanupJob> logger)
	{
		_cleanup = cleanup;
		_logger = logger;
	}

	public Task Invoke()
	{
		return Task.Run(delegate
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			_logger.LogInfoWithSource("Starting FileSetCleanup", "Invoke");
			_cleanup.Run();
			_logger.LogInfoWithSource($"Finished FileSetCleanup in {stopwatch.ElapsedMilliseconds}ms", "Invoke");
		});
	}
}
