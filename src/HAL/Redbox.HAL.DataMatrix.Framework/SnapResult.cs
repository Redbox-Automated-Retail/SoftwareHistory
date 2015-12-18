namespace Redbox.HAL.DataMatrix.Framework;

public sealed class SnapResult
{
	public string Path { get; internal set; }

	public bool SnapOk { get; internal set; }

	internal void SnapError()
	{
		Path = null;
		SnapOk = false;
	}

	internal SnapResult()
	{
		SnapError();
	}
}
