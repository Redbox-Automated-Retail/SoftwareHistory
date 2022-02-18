using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class GetFileResponse : BaseResponse
{
	public string Key { get; set; }

	public GetFileResponse()
		: base(202)
	{
		base.Success = true;
	}

	public GetFileResponse(List<Error> errors, int? statusCode = null)
	{
		base.Errors = errors;
		if (statusCode.HasValue)
		{
			base.StatusCode = statusCode.Value;
		}
	}
}
