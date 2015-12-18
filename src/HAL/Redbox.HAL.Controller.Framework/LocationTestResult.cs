namespace Redbox.HAL.Controller.Framework;

public sealed class LocationTestResult
{
	public readonly Deck Deck;

	public readonly int Slot;

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

	internal LocationTestResult(int deck, int slot)
	{
		Error = PickerHardwareError.Success;
		PeekResult = GripperPeekResult.Empty;
		Deck = DecksManager.Instance.GetDeck(deck);
		Slot = slot;
	}
}
