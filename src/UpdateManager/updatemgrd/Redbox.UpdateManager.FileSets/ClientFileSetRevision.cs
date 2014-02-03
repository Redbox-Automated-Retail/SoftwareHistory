using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevision
{
	public long FileSetId { get; set; }

	public string FileSetName { get; set; }

	public int RetentionDays { get; set; }

	public int RetentionRevisions { get; set; }

	public long RevisionId { get; set; }

	public string RevisionName { get; set; }

	public string RevisionVersion { get; set; }

	public long PatchRevisionId { get; set; }

	public string SetPath { get; set; }

	public int SetCompressionType { get; set; }

	public string SetFileHash { get; set; }

	public long SetFileSize { get; set; }

	public string PatchSetPath { get; set; }

	public int PatchSetCompressionType { get; set; }

	public string PatchSetFileHash { get; set; }

	public long PatchSetFileSize { get; set; }

	public IEnumerable<ClientFileSetFile> Files { get; set; }

	public IEnumerable<ClientPatchFileSetFile> PatchFiles { get; set; }

	public virtual IEnumerable<ClientFileSetRevisionDependency> Dependencies { get; set; }

	public ClientFileSetRevision Clone()
	{
		ClientFileSetRevision clientFileSetRevision = new ClientFileSetRevision();
		clientFileSetRevision.FileSetId = FileSetId;
		clientFileSetRevision.FileSetName = FileSetName;
		clientFileSetRevision.RetentionDays = RetentionDays;
		clientFileSetRevision.RetentionRevisions = RetentionRevisions;
		clientFileSetRevision.RevisionId = RevisionId;
		clientFileSetRevision.RevisionName = RevisionName;
		clientFileSetRevision.RevisionVersion = RevisionVersion;
		clientFileSetRevision.PatchRevisionId = PatchRevisionId;
		clientFileSetRevision.SetPath = SetPath;
		clientFileSetRevision.SetCompressionType = SetCompressionType;
		clientFileSetRevision.SetFileHash = SetFileHash;
		clientFileSetRevision.SetFileSize = SetFileSize;
		clientFileSetRevision.PatchSetPath = PatchSetPath;
		clientFileSetRevision.PatchSetCompressionType = PatchSetCompressionType;
		clientFileSetRevision.PatchSetFileHash = PatchSetFileHash;
		clientFileSetRevision.PatchSetFileSize = PatchSetFileSize;
		ClientFileSetRevision clientFileSetRevision2 = clientFileSetRevision;
		List<ClientFileSetFile> files = new List<ClientFileSetFile>();
		if (Files != null)
		{
			Files.ToList().ForEach(delegate(ClientFileSetFile item)
			{
				files.Add(item);
			});
		}
		clientFileSetRevision2.Files = files;
		List<ClientPatchFileSetFile> patchFiles = new List<ClientPatchFileSetFile>();
		if (PatchFiles != null)
		{
			PatchFiles.ToList().ForEach(delegate(ClientPatchFileSetFile item)
			{
				patchFiles.Add(item);
			});
		}
		clientFileSetRevision2.PatchFiles = patchFiles;
		List<ClientFileSetRevisionDependency> dependencies = new List<ClientFileSetRevisionDependency>();
		if (Dependencies != null)
		{
			Dependencies.ToList().ForEach(delegate(ClientFileSetRevisionDependency item)
			{
				dependencies.Add(item);
			});
		}
		clientFileSetRevision2.Dependencies = dependencies;
		return clientFileSetRevision2;
	}
}
