using System;
using System.Globalization;
using System.IO;

namespace Redbox.HAL.PackageInstaller;

internal sealed class BackupEntry
{
	internal readonly DateTime? BackupDate;

	internal readonly string FullPath;

	internal readonly string FileName;

	private static readonly char[] Separator = new char[1] { '.' };

	internal BackupEntry(string fullPath, string dateParseFormat)
	{
		FullPath = fullPath;
		FileName = Path.GetFileName(FullPath);
		BackupDate = null;
		string[] array = FileName.Split(Separator);
		if (array.Length == 3 && DateTime.TryParseExact(array[1], dateParseFormat, null, DateTimeStyles.None, out var result))
		{
			BackupDate = result;
		}
	}
}
