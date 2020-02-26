using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskFunctionCheckMessageProcessorService : AWSBaseMessageProcessorService<KioskFunctionCheckPayload, AWSKioskFunctionCheckPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskFunctionCheck;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskFunctionCheck" };

	protected override void CopyFields(AWSKioskFunctionCheckPayload destination, KioskFunctionCheckPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.ReportTime = source.ReportTime.ToUniversalTime();
		destination.UserName = source.UserName;
		destination.InitTest = source.InitTest;
		destination.TrackTest = source.TrackTest;
		destination.VendDoor = source.VendDoor;
		destination.VerticalSlot = source.VerticalSlot;
		destination.CameraDriver = source.CameraDriver;
		destination.SnapDecode = source.SnapDecode;
		destination.TouchScreenDriver = source.TouchScreenDriver;
	}
}
