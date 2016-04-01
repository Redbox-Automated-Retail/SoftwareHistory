using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class SnapResult : ISnapResult
{
	public string Path { get; private set; }

	public bool SnapOk
	{
		get
		{
			if (!string.IsNullOrEmpty(Path))
			{
				return File.Exists(Path);
			}
			return false;
		}
	}

	internal SnapResult(string path)
	{
		Path = (string.IsNullOrEmpty(path) ? "NONE" : path);
	}
}
