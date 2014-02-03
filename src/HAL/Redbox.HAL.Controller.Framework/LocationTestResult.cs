namespace Redbox.HAL.Controller.Framework;

public class LocationTestResult
{
	public bool TestOk => PickerHardwareError.Success == Error;

	public GripperPeekResult PeekResult { get; internal set; }

	public PickerHardwareError Error { get; internal set; }

	public override string ToString()
	{
		if (!TestOk)
		{
			return Error.ToString().ToUpper();
		}
		return PeekResult.ToString().ToUpper();
	}

	internal LocationTestResult()
	{
		Error = PickerHardwareError.Success;
		PeekResult = GripperPeekResult.Empty;
	}
}
