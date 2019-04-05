using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-inventory-store-job")]
internal sealed class DumpInventoryStoreJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = string.Format("StoreDump-{0}", service.GenerateUniqueFile("txt"));
		string path2 = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service", path);
		using StreamWriter streamWriter = new StreamWriter(path2);
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		service2.DumpStore(streamWriter);
		IDumpbinService service3 = ServiceLocator.Instance.GetService<IDumpbinService>();
		service3.DumpContents(streamWriter);
		streamWriter.WriteLine();
	}

	internal DumpInventoryStoreJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
