using System.Collections.Generic;
using System.Threading.Tasks;

namespace UpdateClientService.API.Services;

public interface IStatusService
{
	Task<FileVersionDataResponse> GetFileVersions();

	Task<FileVersionDataResponse> GetFileVersions(IEnumerable<string> filepaths);
}
