namespace Redbox.UpdateManager.FileSets;

internal class FileSetRevisionChangeSet
{
	public long FileSetId { get; set; }

	public long RevisionId { get; set; }

	public string Path { get; set; }

	public int CompressionType { get; set; }

	public string ContentHash { get; set; }

	public string FileHash { get; set; }

	public long ContentSize { get; set; }

	public long FileSize { get; set; }

	public long PatchRevisionId { get; set; }

	public ClientFileSetRevisionChangeSet ToClient()
	{
		ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet = new ClientFileSetRevisionChangeSet();
		clientFileSetRevisionChangeSet.FileSetId = FileSetId;
		clientFileSetRevisionChangeSet.RevisionId = RevisionId;
		clientFileSetRevisionChangeSet.Path = Path;
		clientFileSetRevisionChangeSet.CompressionType = CompressionType;
		clientFileSetRevisionChangeSet.FileHash = FileHash;
		clientFileSetRevisionChangeSet.ContentHash = ContentHash;
		clientFileSetRevisionChangeSet.PatchRevisionId = PatchRevisionId;
		clientFileSetRevisionChangeSet.ContentSize = ContentSize;
		clientFileSetRevisionChangeSet.FileSize = FileSize;
		return clientFileSetRevisionChangeSet;
	}

	public FileSetRevisionChangeSet Clone()
	{
		FileSetRevisionChangeSet fileSetRevisionChangeSet = new FileSetRevisionChangeSet();
		fileSetRevisionChangeSet.FileSetId = FileSetId;
		fileSetRevisionChangeSet.RevisionId = RevisionId;
		fileSetRevisionChangeSet.Path = Path;
		fileSetRevisionChangeSet.CompressionType = CompressionType;
		fileSetRevisionChangeSet.ContentHash = ContentHash;
		fileSetRevisionChangeSet.FileHash = FileHash;
		fileSetRevisionChangeSet.PatchRevisionId = PatchRevisionId;
		fileSetRevisionChangeSet.ContentSize = ContentSize;
		fileSetRevisionChangeSet.FileSize = FileSize;
		return fileSetRevisionChangeSet;
	}
}
