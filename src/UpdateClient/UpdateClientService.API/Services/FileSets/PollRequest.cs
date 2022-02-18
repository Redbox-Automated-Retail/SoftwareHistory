namespace UpdateClientService.API.Services.FileSets;

public class PollRequest
{
	public PollRequestType PollRequestType { get; set; }

	public int SyncId { get; set; }

	public string Data { get; set; }
}
