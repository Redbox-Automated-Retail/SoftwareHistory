using System.Collections.Generic;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileCache;

public interface IFileCacheService
{
	bool FileExists(long fileSetId, long fileId, long fileRevisionId);

	bool RevisionExists(long fileSetId, long revisionId);

	bool RevisionExists(long fileSetId, long revisionId, long patchRevisionId);

	List<Error> AddFile(long fileSetId, long fileId, long fileRevisionId, byte[] data, string info);

	List<Error> AddFilePatch(long fileSetId, long fileId, long fileRevisionId, long filePatchRevisionId, byte[] data, string info);

	List<Error> AddRevision(long fileSetId, long revisionId, long patchRevisionId, byte[] data, string info);

	List<Error> AddFile(long fileSetId, long fileId, long fileRevisionId, string pathToMoveFrom);

	List<Error> AddRevision(long fileSetId, long revisionId, long patchRevisionId, string pathToMoveFrom);

	void DeleteFile(long fileSetId, long fileId, long fileRevisionId);

	void DeleteRevision(long fileSetId, long revisionId, long patchRevisionId);

	bool GetFileInfo(string path, out byte[] data);

	bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data);

	bool CopyFileToPath(long fileSetId, long fileId, long fileRevisionId, string copyToPath);

	bool GetRevision(long fileSetId, long revisionId, out byte[] data);

	bool GetRevision(long fileSetId, long revisionId, long patchRevisionId, out byte[] data);

	string GetFileName(long fileId, long fileRevisionId);

	bool GetRevision(string path, out byte[] data);

	List<string> GetAllRevisionFiles(long fileSetId);

	List<long> GetAllFileSets();

	List<string> GetAllFileInfos(long fileSetId);
}
