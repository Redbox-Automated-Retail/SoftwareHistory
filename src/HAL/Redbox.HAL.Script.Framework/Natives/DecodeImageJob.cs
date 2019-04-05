using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "decode-image")]
internal sealed class DecodeImageJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.Context.ShouldCleanup = false;
		string path = base.Context.PopTop<string>();
		IBarcodeReaderFactory service = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>();
		string text = Path.Combine(service.ImagePath, path);
		if (!File.Exists(text))
		{
			LogHelper.Instance.WithContext("The image '{0}' was not found", text);
			AddError("Image not found.");
		}
		else
		{
			IBarcodeReader configuredReader = service.GetConfiguredReader();
			IScanResult sr = configuredReader.Scan(text);
			ScanResult scanResult = new ScanResult(sr);
			scanResult.PushTo(base.Context);
		}
	}

	internal DecodeImageJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
