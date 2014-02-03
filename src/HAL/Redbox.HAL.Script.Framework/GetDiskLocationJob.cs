using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetDiskLocationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		for (int i = 0; i < top; i++)
		{
			string top2 = GetTop<string>();
			Location location = ControllerService.Instance.Lookup(top2);
			if (location == null)
			{
				ResultHelper.Instance.CreateInfoResult("BarcodeNotInInventory", "The barcode isn't in inventory.");
			}
			else
			{
				ResultHelper.Instance.CreateResult(VMZ.Instance.LocInCompressedZone(location) ? "BarcodeInVMZ" : "BarcodeOutsideVMZ", "Barcode status", location.Deck, location.Slot, top2, null, null);
			}
		}
	}
}
