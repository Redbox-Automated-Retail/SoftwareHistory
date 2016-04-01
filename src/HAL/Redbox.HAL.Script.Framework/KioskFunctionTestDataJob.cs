using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-function-test-data", Operand = "KIOSK-FUNCTION-TEST-DATA")]
internal sealed class KioskFunctionTestDataJob : NativeJobAdapter
{
	private class FunctionTestData : IKioskFunctionCheckData
	{
		public string VerticalSlotTestResult { get; internal set; }

		public string InitTestResult { get; internal set; }

		public string VendDoorTestResult { get; internal set; }

		public string TrackTestResult { get; internal set; }

		public string SnapDecodeTestResult { get; internal set; }

		public string TouchscreenDriverTestResult { get; internal set; }

		public string CameraDriverTestResult { get; internal set; }

		public DateTime Timestamp { get; internal set; }

		public string UserIdentifier { get; internal set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Kiosk Check Data");
			stringBuilder.AppendLine($"  Vertical slot result = {VerticalSlotTestResult}");
			stringBuilder.AppendLine($"  Init result = {InitTestResult}");
			stringBuilder.AppendLine($"  Vend door result = {VendDoorTestResult}");
			stringBuilder.AppendLine($"  Track result = {TrackTestResult}");
			stringBuilder.AppendLine($"  Snap and decode result = {SnapDecodeTestResult}");
			stringBuilder.AppendLine($"  Camera driver result = {CameraDriverTestResult}");
			stringBuilder.AppendLine($"  Touchscreen driver result = {TouchscreenDriverTestResult}");
			stringBuilder.AppendLine($"  Timestamp = {Timestamp.ToString()}");
			stringBuilder.AppendLine($"  User token = {UserIdentifier}");
			return stringBuilder.ToString();
		}
	}

	protected override void ExecuteInner()
	{
		FunctionTestData functionTestData = new FunctionTestData();
		functionTestData.VerticalSlotTestResult = GetTop<string>();
		functionTestData.InitTestResult = GetTop<string>();
		functionTestData.VendDoorTestResult = GetTop<string>();
		functionTestData.TrackTestResult = GetTop<string>();
		functionTestData.SnapDecodeTestResult = GetTop<string>();
		functionTestData.CameraDriverTestResult = GetTop<string>();
		functionTestData.TouchscreenDriverTestResult = GetTop<string>();
		functionTestData.Timestamp = DateTime.Parse(GetTop<string>());
		functionTestData.UserIdentifier = GetTop<string>();
		IKioskFunctionCheckService service = ServiceLocator.Instance.GetService<IKioskFunctionCheckService>();
		service.Add(functionTestData);
	}

	internal KioskFunctionTestDataJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
