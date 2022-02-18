using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.FileCache;

public class FileCacheService : IFileCacheService
{
	private readonly ILogger<FileCacheService> _logger;

	public FileCacheService(ILogger<FileCacheService> logger)
	{
		_logger = logger;
	}

	public bool FileExists(long fileSetId, long fileId, long fileRevisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			return File.Exists(filePath);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.FileExists Unhandled exception occurred. Exception: {arg}"
			});
		}
		return false;
	}

	public bool RevisionExists(long fileSetId, long revisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			List<string> allRevisionFiles = GetAllRevisionFiles(fileSetId, revisionId);
			return allRevisionFiles.Count() > 0;
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.RevisionExists. Unhandled exception occurred. Exception: {arg}"
			});
		}
		return false;
	}

	public bool RevisionExists(long fileSetId, long revisionId, long patchRevisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			return File.Exists(revisionPath);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.RevisionExists. Unhandled exception occurred. Exception {arg}"
			});
		}
		return false;
	}

	public List<Error> AddFile(long fileSetId, long fileId, long fileRevisionId, byte[] data, string info)
	{
		List<Error> list = new List<Error>();
		try
		{
			CheckFilePath(fileSetId, fileId);
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			File.WriteAllBytes(filePath, data);
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			File.WriteAllText(fileInfoPath, info);
			_logger.LogInformation("FileCacheService.AddFile - Info: " + info);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unhandled exception occurred");
			list.Add(new Error
			{
				Message = $"FileCacheService.AddFile. Unhandled exception occurred. Exception: {ex}"
			});
		}
		return list;
	}

	public List<Error> AddFilePatch(long fileSetId, long fileId, long fileRevisionId, long filePatchRevisionId, byte[] data, string info)
	{
		List<Error> list = new List<Error>();
		try
		{
			CheckFilePath(fileSetId, fileId);
			if (!GetFile(fileSetId, fileId, filePatchRevisionId, out var _))
			{
				list.Add(new Error
				{
					Message = $"FileCacheService.AddFilePatch. Patch source file is missing FileId: {fileId} FileRevisionId {filePatchRevisionId}"
				});
				return list;
			}
			string filePath = GetFilePath(fileSetId, fileId, filePatchRevisionId);
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				fileStream.Position = 0L;
				using MemoryStream memoryStream = new MemoryStream(data);
				memoryStream.Position = 0L;
				string filePath2 = GetFilePath(fileSetId, fileId, fileRevisionId);
				using FileStream target = new FileStream(filePath2, FileMode.OpenOrCreate, FileAccess.Write);
				list.AddRange(XDeltaHelper.Apply(fileStream, memoryStream, target));
			}
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			File.WriteAllText(fileInfoPath, info);
			_logger.LogInfoWithSource("FileCacheService.AddFilePatch - Info: " + info, "AddFilePatch");
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.AddFilePatch. Unhandled exception occurred. Exception: {arg}"
			});
		}
		return list;
	}

	public List<Error> AddRevision(long fileSetId, long revisionId, long patchRevisionId, byte[] data, string info)
	{
		List<Error> list = new List<Error>();
		try
		{
			CheckFileSetPath(fileSetId);
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			File.WriteAllBytes(revisionPath, data);
			string revisionInfoPath = GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId);
			File.WriteAllText(revisionInfoPath, info);
			_logger.LogInfoWithSource("FileCacheService.AddRevision - Info: " + info, "AddRevision");
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.AddRevision Unhandled exception occurred. {arg}"
			});
		}
		return list;
	}

	public List<Error> AddFile(long fileSetId, long fileId, long fileRevisionId, string pathToMoveFrom)
	{
		List<Error> list = new List<Error>();
		try
		{
			CheckFilePath(fileSetId, fileId);
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			File.Move(pathToMoveFrom, filePath);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.AddFile Unhandled exception occurred{arg}"
			});
		}
		return list;
	}

	public List<Error> AddRevision(long fileSetId, long revisionId, long patchRevisionId, string pathToMoveFrom)
	{
		List<Error> list = new List<Error>();
		try
		{
			CheckFileSetPath(fileSetId);
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			File.Move(pathToMoveFrom, revisionPath);
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.AddRevision Unhandled exception occurred {arg}"
			});
		}
		return list;
	}

	public void DeleteFile(long fileSetId, long fileId, long fileRevisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			double totalDays = (DateTime.Now - File.GetCreationTime(filePath)).TotalDays;
			if (totalDays <= 1.0)
			{
				_logger.LogInfoWithSource($"Cannot delete files less than a day old - FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DeleteFile");
				return;
			}
			_logger.LogInfoWithSource($"FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DeleteFile");
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(fileInfoPath))
			{
				File.Delete(fileInfoPath);
			}
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "Unhandled exception occurred. Exception -> " + e.GetFullMessage()
			});
		}
	}

	public void DeleteRevision(long fileSetId, long revisionId, long patchRevisionId)
	{
		List<Error> list = new List<Error>();
		try
		{
			_logger.LogInfoWithSource($"FileSetId {fileSetId}, RevisionId {revisionId}, PatchRevisionId {patchRevisionId}", "DeleteRevision");
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionPath))
			{
				File.Delete(revisionPath);
			}
			string revisionInfoPath = GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionInfoPath))
			{
				File.Delete(revisionInfoPath);
			}
			string stagingPath = GetStagingPath(fileSetId, revisionId);
			if (Directory.Exists(stagingPath))
			{
				Directory.Delete(stagingPath, recursive: true);
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"Unhandled exception occurred. {arg}"
			});
		}
	}

	public bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(filePath))
			{
				data = File.ReadAllBytes(filePath);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetFile Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public bool GetFileInfo(string path, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetFileInfo Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public bool CopyFileToPath(long fileSetId, long fileId, long fileRevisionId, string copyToPath)
	{
		List<Error> list = new List<Error>();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(filePath))
			{
				File.Copy(filePath, copyToPath, overwrite: true);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.CopyFileToPath Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public bool GetRevision(long fileSetId, long revisionId, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			List<string> allRevisionFiles = GetAllRevisionFiles(fileSetId, revisionId);
			if (allRevisionFiles.Count() == 0)
			{
				return false;
			}
			string text = allRevisionFiles.OrderByDescending((string f) => f).FirstOrDefault((string f) => Path.GetExtension(f).Equals(".revision", StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			if (File.Exists(text))
			{
				data = File.ReadAllBytes(text);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetRevision Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public bool GetRevision(long fileSetId, long revisionId, long patchRevisionId, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionPath))
			{
				data = File.ReadAllBytes(revisionPath);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetRevision Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public bool GetRevision(string path, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetRevision Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public List<string> GetAllRevisionFiles(long fileSetId)
	{
		return Directory.GetFiles(GetRevisionPath(fileSetId), "*.revision", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<long> GetAllFileSets()
	{
		List<long> list = new List<long>();
		if (!Directory.Exists(Constants.FileCacheRootPath))
		{
			return list;
		}
		string[] directories = Directory.GetDirectories(Constants.FileCacheRootPath);
		string[] array = directories;
		foreach (string path in array)
		{
			string fileName = Path.GetFileName(path);
			try
			{
				list.Add(Convert.ToInt64(fileName));
			}
			catch
			{
			}
		}
		return list;
	}

	private List<string> GetAllRevisionFiles(long fileSetId, long revisionId)
	{
		return Directory.GetFiles(GetRevisionPath(fileSetId), $"{revisionId}-*.revision", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<string> GetAllFiles(long fileSetId)
	{
		return Directory.GetFiles(GetFilePath(fileSetId), "*.file", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<string> GetAllFileInfos(long fileSetId)
	{
		return Directory.GetFiles(GetFilePath(fileSetId), "*.fileinfo", SearchOption.TopDirectoryOnly).ToList();
	}

	private string GetRevisionPath(long fileSetId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\");
	}

	private string GetRevisionPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\{GetRevisionName(revisionId, patchRevisionId)}");
	}

	private string GetRevisionInfoPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\{GetRevisionInfoName(revisionId, patchRevisionId)}");
	}

	private string GetStagingPath(long fileSetId, long revisionId)
	{
		return Path.Combine(Constants.FileSetsRoot, $"staging\\{fileSetId}\\{revisionId}");
	}

	private string GetRevisionName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revision";
	}

	private string GetRevisionInfoName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revisioninfo";
	}

	private string GetFilePath(long fileSetId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\File\\");
	}

	private string GetFilePath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\File\\{GetFileName(fileId, fileRevisionId)}");
	}

	private string GetFileInfoPath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\File\\{GetFileInfoName(fileId, fileRevisionId)}");
	}

	public string GetFileName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.file";
	}

	private string GetFileInfoName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.fileinfo";
	}

	private void CheckFilePath(long fileSetId, long fileId)
	{
		string path = Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}\\File");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private void CheckFileSetPath(long fileSetId)
	{
		string path = Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
}
