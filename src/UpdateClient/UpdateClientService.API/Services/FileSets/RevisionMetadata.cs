namespace UpdateClientService.API.Services.FileSets;

public abstract class RevisionMetadata
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	public long PatchRevisionId { get; set; }
}
