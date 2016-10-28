namespace Redbox.UpdateService.Model;

internal class CreateFileSetChangeSetArgs
{
	public long FileSetId { get; set; }

	public long FileSetRevisionId { get; set; }

	public long PatchFileSetRevisionId { get; set; }
}
