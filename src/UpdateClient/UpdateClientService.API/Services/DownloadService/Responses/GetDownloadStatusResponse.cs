using System.Collections.Generic;
using System.Linq;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.DownloadService.Responses;

public class GetDownloadStatusResponse : BaseResponse
{
	public DownloadData Status { get; set; }

	public GetDownloadStatusResponse(List<DownloadData> data, string key)
		: base(200)
	{
		Status = data.Where((DownloadData d) => d.Key == key).FirstOrDefault();
		base.StatusCode = ((Status != null) ? base.StatusCode : 404);
		base.Success = base.StatusCode == 200;
	}
}
