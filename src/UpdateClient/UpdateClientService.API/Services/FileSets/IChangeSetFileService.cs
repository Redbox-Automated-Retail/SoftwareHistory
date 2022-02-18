using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public interface IChangeSetFileService
{
	bool IsStaged(RevisionChangeSet revisionChangeSet);

	bool IsActivated(RevisionChangeSet revisionChangeSet);

	bool IsInNeedsDependency(RevisionChangeSet revisionChangeSet);

	bool IsInError(RevisionChangeSet revisionChangeSet);

	Task<List<Error>> ProcessChangeSet(List<DownloadData> downloads, RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivationDependencyCheck(Dictionary<long, FileSetDependencyState> dependencyStates, RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivationPending(RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivationBeforeActions(RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivating(RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivationAfterActions(RevisionChangeSet revisionChangeSet);

	List<Error> ProcessActivated(RevisionChangeSet revisionChangeSet);

	List<Error> CreateFromChangeSet(ClientFileSetRevisionChangeSet set);

	void Delete(RevisionChangeSet changeset);

	List<Error> Delete(long fileSetId, long revisionId);

	void CleanUp();

	void CreateChangeSetDirectory();

	List<Error> GetAllFiles(out List<RevisionChangeSet> chageSetFiles);
}
