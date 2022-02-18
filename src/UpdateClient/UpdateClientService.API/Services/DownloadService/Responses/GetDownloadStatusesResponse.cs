using System.Collections.Generic;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class GetDownloadStatusesResponse : BaseResponse
{
	public List<DownloadData> Statuses { get; set; }

	public GetDownloadStatusesResponse()
		: base(200)
	{
		base.Success = true;
	}
}
