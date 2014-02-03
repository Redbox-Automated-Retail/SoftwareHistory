using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal static class TicketsFile
{
	private const string _ticketsDataFileName = "tickets-profile.data";

	private const string _ticketsImagesFileName = "tickets-images.zip";

	private const string _stagedTicketsDataFileName = "staged_tickets-profile.data";

	private const string _stagedTicketsImagesFileName = "staged_tickets-images.zip";

	private const string _cacheLocationDirectoryName = "tickets-images\\";

	public static string DataPath => GetPath("tickets-profile.data");

	public static string ImagePath => GetPath("tickets-images.zip");

	public static string StagedDataPath => GetPath("staged_tickets-profile.data");

	public static string StagedImagePath => GetPath("staged_tickets-images.zip");

	public static string CacheLocation => GetPath("tickets-images\\");

	public static bool GetHasStaged()
	{
		if (File.Exists(StagedDataPath))
		{
			return File.Exists(StagedImagePath);
		}
		return false;
	}

	public static bool GetHasActive()
	{
		if (File.Exists(DataPath))
		{
			return File.Exists(ImagePath);
		}
		return false;
	}

	public static bool Activate()
	{
		if (!GetHasStaged())
		{
			return false;
		}
		bool flag = false;
		try
		{
			if (File.Exists(DataPath))
			{
				File.Delete(DataPath);
			}
			if (File.Exists(ImagePath))
			{
				File.Delete(ImagePath);
			}
			File.Move(StagedDataPath, DataPath);
			flag = true;
			File.Move(StagedImagePath, ImagePath);
			return ExtractImagesToCache();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketsFile.Activate", e);
			if (flag)
			{
				LogHelper.Instance.Log("Tickets data moved with images.  Moving tickets data back.");
				try
				{
					File.Move(DataPath, StagedDataPath);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("Unhandled exception occurred in TicketsFile.Activate move back.", e2);
				}
			}
			return false;
		}
	}

	public static bool ExtractImagesToCache()
	{
		try
		{
			using (ZipFile zipFile = new ZipFile(ImagePath))
			{
				if (!Directory.Exists(CacheLocation))
				{
					Directory.CreateDirectory(CacheLocation);
				}
				foreach (ZipEntry item in zipFile)
				{
					string path = CacheLocation + item.Name;
					if (File.Exists(path) && !item.Name.Equals("version.txt", StringComparison.CurrentCultureIgnoreCase))
					{
						continue;
					}
					using Stream input = zipFile.GetInputStream(item);
					using BinaryReader binaryReader = new BinaryReader(input);
					byte[] array = binaryReader.ReadBytes((int)item.Size);
					using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
					fileStream.Write(array, 0, array.Length);
					fileStream.Close();
				}
			}
			LogHelper.Instance.Log("Tickets data service ExtractImagesToCache complete");
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketsFile.ExtractImagesToCache", e);
			return false;
		}
	}

	private static string GetPath(string fileName)
	{
		string directoryName = Path.GetDirectoryName(typeof(IKernelService).Assembly.Location);
		return Path.Combine(directoryName, $"..\\data\\{fileName}");
	}
}
