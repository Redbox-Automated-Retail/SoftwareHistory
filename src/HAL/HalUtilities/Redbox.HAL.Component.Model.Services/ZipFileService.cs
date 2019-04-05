using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.Component.Model.Services;

public sealed class ZipFileService : IZipFileService
{
	private readonly IRuntimeService RuntimeService;

	public ZipResult Zip(IEnumerable<string> files, string zipPath)
	{
		ZipResult zipResult = new ZipResult();
		zipResult.ZipFile = zipPath;
		ZipResult zipResult2 = zipResult;
		try
		{
			using ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(zipPath));
			zipOutputStream.SetLevel(9);
			byte[] array = new byte[4096];
			int num = 0;
			foreach (string file in files)
			{
				FileInfo fileInfo = new FileInfo(file);
				ZipEntry zipEntry = new ZipEntry(fileInfo.Name);
				zipEntry.DateTime = fileInfo.CreationTime;
				zipOutputStream.PutNextEntry(zipEntry);
				using (FileStream fileStream = File.OpenRead(file))
				{
					int num2;
					do
					{
						num2 = fileStream.Read(array, 0, array.Length);
						zipOutputStream.Write(array, 0, num2);
					}
					while (num2 > 0);
				}
				num++;
			}
			zipOutputStream.Finish();
			zipOutputStream.Close();
			zipResult2.EntryCount = num;
			zipResult2.Success = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("!!OnZipImages: exception!!", e);
			zipResult2.Success = false;
		}
		return zipResult2;
	}

	public UnzipResult Unzip(string zipFile)
	{
		FileInfo fileInfo = new FileInfo(zipFile);
		string unzipPath = RuntimeService.RuntimePath(Path.GetFileNameWithoutExtension(fileInfo.Name));
		return Unzip(zipFile, unzipPath);
	}

	public UnzipResult Unzip(string zipFile, string unzipPath)
	{
		UnzipResult unzipResult = new UnzipResult();
		unzipResult.SourceZip = zipFile;
		unzipResult.OutputPath = unzipPath;
		unzipResult.Success = false;
		UnzipResult unzipResult2 = unzipResult;
		if (!SafeCreateDirectory(unzipPath))
		{
			return unzipResult2;
		}
		try
		{
			using (ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(zipFile)))
			{
				ZipEntry nextEntry;
				while ((nextEntry = zipInputStream.GetNextEntry()) != null)
				{
					string fileName = Path.GetFileName(nextEntry.Name);
					if (!(fileName != string.Empty))
					{
						continue;
					}
					string text = Path.Combine(unzipPath, nextEntry.Name);
					using (FileStream fileStream = File.Create(text))
					{
						int num = 2048;
						byte[] array = new byte[2048];
						while (true)
						{
							num = zipInputStream.Read(array, 0, array.Length);
							if (num > 0)
							{
								fileStream.Write(array, 0, num);
								continue;
							}
							break;
						}
					}
					FileInfo fileInfo = new FileInfo(text);
					fileInfo.CreationTime = nextEntry.DateTime;
				}
			}
			unzipResult2.Success = true;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.WithContext("ZipService UnZip failed with an exception", ex);
			unzipResult2.Success = false;
		}
		return unzipResult2;
	}

	public ZipFileService(IRuntimeService rts)
	{
		RuntimeService = rts;
	}

	private UnzipResult OnUnzip(string zipFile, string outputPath)
	{
		UnzipResult unzipResult = new UnzipResult();
		unzipResult.OutputPath = outputPath;
		unzipResult.SourceZip = zipFile;
		unzipResult.Success = false;
		UnzipResult unzipResult2 = unzipResult;
		if (!SafeCreateDirectory(outputPath))
		{
			return unzipResult2;
		}
		int num = 0;
		using ZipFile zipFile2 = new ZipFile(zipFile);
		foreach (ZipEntry item in zipFile2)
		{
			num++;
			if (item.IsDirectory)
			{
				if (!SafeCreateDirectory(item.Name))
				{
					return unzipResult2;
				}
				continue;
			}
			byte[] buffer = new byte[4096];
			Stream inputStream = zipFile2.GetInputStream(item);
			string path = Path.Combine(outputPath, item.Name);
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName.Length > 0 && !SafeCreateDirectory(directoryName))
			{
				return unzipResult2;
			}
			using FileStream destination = File.Create(path);
			StreamUtils.Copy(inputStream, destination, buffer);
		}
		unzipResult2.EntryCount = num;
		unzipResult2.Success = num == zipFile2.Count;
		return unzipResult2;
	}

	private bool SafeCreateDirectory(string directory)
	{
		if (Directory.Exists(directory))
		{
			return true;
		}
		try
		{
			Directory.CreateDirectory(directory);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
