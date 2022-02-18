using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetCleanup : IFileSetCleanup
{
	private readonly IFileCacheService _fileCacheService;

	private readonly IFileSetService _fileSetService;

	private readonly ILogger<FileSetCleanup> _logger;

	public FileSetCleanup(IFileCacheService fileCacheService, IFileSetService fileSetService, ILogger<FileSetCleanup> logger)
	{
		_fileCacheService = fileCacheService;
		_fileSetService = fileSetService;
		_logger = logger;
	}

	public void Run()
	{
		try
		{
			List<Error> all = StateFile.GetAll(out var stateFiles);
			if (all.Any())
			{
				_logger.LogErrorWithSource("Encountered errors getting state files. Errors -> " + all.ToJson(), "Run");
				return;
			}
			List<long> allFileSets = _fileCacheService.GetAllFileSets();
			allFileSets.ForEach(delegate(long fileSetId)
			{
				List<long> revisionIds = new List<long>();
				stateFiles.ForEach(delegate(StateFile sf)
				{
					if (sf.FileSetId == fileSetId)
					{
						if (sf.HasActive())
						{
							revisionIds.Add(sf.RevisionId);
						}
						if (sf.HasInProgress())
						{
							revisionIds.Add(sf.InProgressRevisionId);
						}
					}
				});
				revisionIds = revisionIds.Distinct().ToList();
				if (revisionIds.Count != 0)
				{
					List<ClientFileSetRevision> revisions = new List<ClientFileSetRevision>();
					revisionIds.ForEach(delegate(long r)
					{
						ClientFileSetRevision revision = _fileSetService.GetRevision(fileSetId, r);
						if (revision != null)
						{
							revisions.Add(revision);
						}
					});
					if (revisions.Count != 0)
					{
						ClientFileSetRevision clientFileSetRevision = revisions.Last();
						int retentionDays = clientFileSetRevision.RetentionDays;
						int retentionRevisions = clientFileSetRevision.RetentionRevisions;
						if (retentionDays != 0 || retentionRevisions != 0)
						{
							List<FileSetFileInfo> allFileInfos = new List<FileSetFileInfo>();
							List<string> allFileInfos2 = _fileCacheService.GetAllFileInfos(fileSetId);
							allFileInfos2.ForEach(delegate(string path)
							{
								allFileInfos.Add(_fileSetService.GetFileInfo(path));
							});
							List<FileSetFileInfo> requiredFiles = new List<FileSetFileInfo>();
							List<string> list = (from s in _fileCacheService.GetAllRevisionFiles(fileSetId)
								orderby s descending
								select s).ToList();
							int pos = 1;
							list.ForEach(delegate(string path)
							{
								ClientFileSetRevision revision = _fileSetService.GetRevision(path);
								if (revision != null)
								{
									bool flag = true;
									try
									{
										if (revisions.Exists((ClientFileSetRevision r) => r.RevisionId == revision.RevisionId))
										{
											return;
										}
										double totalDays = (DateTime.Now - File.GetCreationTime(path)).TotalDays;
										if (totalDays <= 1.0)
										{
											return;
										}
										bool flag2 = retentionDays > 0 && totalDays > (double)retentionDays;
										bool flag3 = retentionRevisions > 0 && pos > retentionRevisions;
										if (flag3 | flag2)
										{
											flag = false;
											_logger.LogInfoWithSource($"Deleting FileSetId: {fileSetId} RevisionId {revision.RevisionId} PatchRevisionId {revision.PatchRevisionId} CountDelete {flag3} DateDelete {flag2} ", "Run");
											_fileCacheService.DeleteRevision(fileSetId, revision.RevisionId, revision.PatchRevisionId);
										}
									}
									finally
									{
										if (flag)
										{
											AddRequiredFiles(allFileInfos, requiredFiles, revision);
										}
									}
									pos++;
								}
							});
							RemoveFiles(fileSetId, allFileInfos, requiredFiles);
						}
					}
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Run");
		}
	}

	private void AddRequiredFiles(List<FileSetFileInfo> allFileInfos, List<FileSetFileInfo> requiredFiles, ClientFileSetRevision revision)
	{
		List<FileSetFileInfo> collection = (from a in revision.Files
			join b in allFileInfos on new { a.FileId, a.FileRevisionId } equals new { b.FileId, b.FileRevisionId }
			select b).ToList();
		requiredFiles.AddRange(collection);
		List<FileSetFileInfo> collection2 = (from a in revision.PatchFiles
			join b in allFileInfos on new
			{
				FileId = a.FileId,
				FileRevisionId = a.PatchFileRevisionId
			} equals new { b.FileId, b.FileRevisionId }
			select b).ToList();
		requiredFiles.AddRange(collection2);
	}

	private void RemoveFiles(long fileSetId, List<FileSetFileInfo> allFileInfos, List<FileSetFileInfo> requiredFiles)
	{
		List<FileSetFileInfo> list = allFileInfos.Except(requiredFiles.Distinct()).ToList();
		list.ForEach(delegate(FileSetFileInfo r)
		{
			_logger.LogInfoWithSource($"Deleting FileSetId: {fileSetId} FileId {r.FileId} FileRevisionId {r.FileRevisionId}", "RemoveFiles");
			_fileCacheService.DeleteFile(fileSetId, r.FileId, r.FileRevisionId);
		});
	}
}
