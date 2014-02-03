namespace Redbox.UpdateService.Model;

internal class CreateRepositoryChangeSetArgs
{
	public string SourceRevision { get; set; }

	public string TargetRevision { get; set; }

	public long RepositoryOid { get; set; }

	public string CurrentLabel { get; set; }
}
