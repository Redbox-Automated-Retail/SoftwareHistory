using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal static class FileHelper
{
	public static readonly string DefaultFormat = "yyyyMMddHHmmss";

	public static string CreateBackup(string path, string dateFormat, BackupAction action)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		string text = CreateBackupName(path, dateFormat);
		switch (action)
		{
		case BackupAction.None:
			return text;
		case BackupAction.Move:
			File.Move(path, text);
			break;
		default:
			File.Copy(path, text);
			break;
		}
		return text;
	}

	public static string CreateBackup(string path, BackupAction action)
	{
		return CreateBackup(path, DefaultFormat, action);
	}

	public static bool SafeDeleteFile(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return false;
		}
		bool flag = false;
		try
		{
			File.Delete(path);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Delete file failed.", e);
			return false;
		}
	}

	public static bool SafeMove(TextWriter writer, string source, string dest)
	{
		string arg = $"move {source} -> {dest}";
		try
		{
			File.Move(source, dest);
			writer.WriteLine("{0}: SUCCESS", arg);
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("{0}: FAILURE reason = {1}", arg, ex.Message);
			return false;
		}
	}

	public static bool SafeCopy(TextWriter writer, string source, string dest)
	{
		string arg = $"copy {source} -> {dest}";
		try
		{
			File.Copy(source, dest);
			writer.WriteLine("{0}: SUCCESS", arg);
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("{0}: FAILURE reason = {1}", arg, ex.Message);
			return false;
		}
	}

	public static byte[] ReadToBuffer(string fileName)
	{
		if (fileName == null)
		{
			return null;
		}
		byte[] array = null;
		using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		array = new byte[fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		return array;
	}

	public static string GenerateUniqueFile(string suffix)
	{
		DateTime now = DateTime.Now;
		return $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.{suffix}";
	}

	private static string CreateBackupName(string path, string dateFormat)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string directoryName = Path.GetDirectoryName(path);
		string extension = Path.GetExtension(path);
		string path2 = string.Format("{0}.{1}{2}", fileNameWithoutExtension, DateTime.Now.ToString(dateFormat), extension ?? ".bak");
		return Path.Combine(directoryName, path2);
	}
}
