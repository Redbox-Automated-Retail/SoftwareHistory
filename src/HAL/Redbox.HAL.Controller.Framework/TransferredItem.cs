namespace Redbox.HAL.Controller.Framework;

public class TransferredItem
{
	public Location Source { get; internal set; }

	public Location Destination { get; internal set; }

	public string Matrix { get; internal set; }

	public bool SourceIsVMZ
	{
		get
		{
			if (Source != null)
			{
				return VMZ.Instance.LocIsInVMZ(Source);
			}
			return false;
		}
	}

	public bool TargetIsVMZ
	{
		get
		{
			if (Destination != null)
			{
				return VMZ.Instance.LocIsInVMZ(Destination);
			}
			return false;
		}
	}
}
