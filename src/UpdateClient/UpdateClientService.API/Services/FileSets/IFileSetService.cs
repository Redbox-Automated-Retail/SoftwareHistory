using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets;

public interface IFileSetService : IInvocable
{
	bool RebootRequired { get; }

	List<Error> Initialize();

	Task<List<Error>> ProcessPollReply(List<PollReply> pollReplyList);

	Task<List<Error>> ProcessChangeSet(ClientFileSetRevisionChangeSet changeSet);

	FileSetFileInfo GetFileInfo(string path);

	ClientFileSetRevision GetRevision(long fileSetId, long revisionId);

	ClientFileSetRevision GetRevision(string path);

	void MarkRebootRequired();
}
