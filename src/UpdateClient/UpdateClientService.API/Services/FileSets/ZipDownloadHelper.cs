using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

internal class ZipDownloadHelper : IZipDownloadHelper
{
	private readonly IFileCacheService _fileCacheService;

	public ZipDownloadHelper(IFileCacheService fileCacheService)
	{
		_fileCacheService = fileCacheService;
	}

	public List<Error> Extract(string _zipPath, long _fileSetId, long _revisionId, long _patchRevisionId)
	{
		List<Error> errors = new List<Error>();
		try
		{
			ZipArchive zip = ZipFile.OpenRead(_zipPath);
			try
			{
				List<ZipArchiveEntry> list = zip.Entries.Where(delegate(ZipArchiveEntry e)
				{
					string text = Path.GetExtension(e.Name).ToLower();
					return text == ".json";
				}).ToList();
				list.ForEach(delegate(ZipArchiveEntry entry)
				{
					try
					{
						string text;
						Dictionary<string, object> dictionary;
						using (Stream stream = entry.Open())
						{
							text = stream.ReadToEnd();
							dictionary = text.ToObject<Dictionary<string, object>>();
						}
						string fileEntryName = Path.GetFileNameWithoutExtension(entry.Name);
						ZipArchiveEntry zipArchiveEntry = zip.Entries.Where((ZipArchiveEntry e) => e.Name.Equals(fileEntryName)).FirstOrDefault();
						if (fileEntryName == null)
						{
							errors.Add(new Error
							{
								Message = "ZipDownloadHelper.Extract File " + fileEntryName + " is missing from zip file."
							});
						}
						else
						{
							byte[] bytes;
							using (Stream stream2 = zipArchiveEntry.Open())
							{
								bytes = stream2.GetBytes();
							}
							if (dictionary.ContainsKey("RevisionId"))
							{
								errors.AddRange(_fileCacheService.AddRevision(_fileSetId, _revisionId, _patchRevisionId, bytes, text));
							}
							else
							{
								long fileId = Convert.ToInt64(dictionary["FileId"]);
								long fileRevisionId = Convert.ToInt64(dictionary["FileRevisionId"]);
								if (dictionary.ContainsKey("PatchFileRevisionId"))
								{
									long filePatchRevisionId = Convert.ToInt64(dictionary["PatchFileRevisionId"]);
									errors.AddRange(_fileCacheService.AddFilePatch(_fileSetId, fileId, fileRevisionId, filePatchRevisionId, bytes, text));
								}
								else
								{
									errors.AddRange(_fileCacheService.AddFile(_fileSetId, fileId, fileRevisionId, bytes, text));
								}
							}
						}
					}
					catch (Exception arg2)
					{
						errors.Add(new Error
						{
							Message = $"ZipDownloadHelper.Extract An unhandled exception occurred extracting {entry.Name} . Exception: {arg2}"
						});
					}
				});
			}
			finally
			{
				if (zip != null)
				{
					((IDisposable)zip).Dispose();
				}
			}
		}
		catch (Exception arg)
		{
			errors.Add(new Error
			{
				Message = $"ZipDownloadHelper.Extract An unhandled exception occurred. Exception: {arg}"
			});
		}
		return errors;
	}
}
