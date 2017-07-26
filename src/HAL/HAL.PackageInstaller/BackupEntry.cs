using System;
using System.IO;

namespace Redbox.HAL.PackageInstaller;

internal sealed class BackupEntry
{
	internal readonly DateTime? CreateDate;

	internal readonly string FullPath;

	internal readonly string FileName;

	internal BackupEntry(string fullPath)
	{
		FullPath = fullPath;
		FileName = Path.GetFileName(FullPath);
		try
		{
			CreateDate = File.GetCreationTime(FullPath);
		}
		catch (Exception)
		{
			CreateDate = null;
		}
	}
}
