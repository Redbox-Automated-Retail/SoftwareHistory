using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets;

public interface IZipDownloadHelper
{
	List<Error> Extract(string _zipPath, long _fileSetId, long _revisionId, long _patchRevisionId);
}
