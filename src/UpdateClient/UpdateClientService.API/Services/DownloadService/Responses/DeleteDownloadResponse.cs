using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class DeleteDownloadResponse : BaseResponse
{
	public DeleteDownloadResponse()
		: base(200)
	{
		base.Success = true;
	}

	public DeleteDownloadResponse(List<Error> errors, int? statusCode = null)
	{
		base.Errors = errors;
		if (statusCode.HasValue)
		{
			base.StatusCode = statusCode.Value;
		}
	}
}
