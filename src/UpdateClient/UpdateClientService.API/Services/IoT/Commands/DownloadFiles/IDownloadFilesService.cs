using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Commands.DownloadFiles;

public interface IDownloadFilesService
{
	Task HandleDownloadFileJob(DownloadFileJob job);

	Task HandleScheduledJobs();
}
