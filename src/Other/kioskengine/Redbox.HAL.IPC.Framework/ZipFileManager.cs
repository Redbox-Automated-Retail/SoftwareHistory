using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.IPC.Framework;

internal sealed class ZipFileManager
{
	private readonly string ZipFile;

	public ZipFileManager(string zipFile)
	{
		ZipFile = zipFile;
	}

	public bool Unzip(string outFolder)
	{
		if (!SafeCreateDirectory(outFolder))
		{
			return false;
		}
		int num = 0;
		using Redbox_002EHAL_002EClient1198959_002EZipFile obj = new Redbox_002EHAL_002EClient1198959_002EZipFile(ZipFile);
		foreach (Redbox_002EHAL_002EClient1198959_002EZipEntry item in obj)
		{
			num++;
			if (item.IsDirectory)
			{
				if (!SafeCreateDirectory(item.Name))
				{
					return false;
				}
				continue;
			}
			byte[] buffer = new byte[4096];
			Stream inputStream = obj.GetInputStream(item);
			string path = Path.Combine(outFolder, item.Name);
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName.Length > 0 && !SafeCreateDirectory(directoryName))
			{
				return false;
			}
			using FileStream destination = File.Create(path);
			Redbox_002EHAL_002EClient1198959_002EStreamUtils.Copy(inputStream, destination, buffer);
		}
		return num == obj.Count;
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
