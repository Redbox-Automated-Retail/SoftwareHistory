using System.Collections.Generic;
using System.Linq;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class CompleteDownloadResponse : BaseResponse
{
	public CompleteDownloadResponse()
		: base(200)
	{
		base.Success = true;
	}

	public CompleteDownloadResponse(List<Error> errors, int? statusCode = null)
	{
		if (statusCode.HasValue)
		{
			base.StatusCode = statusCode.Value;
		}
		base.Errors = errors;
		if (errors.Select((Error e) => e.Code).Contains<string>(404.ToString()))
		{
			base.StatusCode = 404;
		}
	}
}
