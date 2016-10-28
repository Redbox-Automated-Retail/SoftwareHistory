using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-inventory-store-job", Operand = "DUMP-INVENTORY-STORE-JOB")]
internal sealed class DumpInventoryStoreJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string path = string.Format("StoreDump-{0}", FileHelper.GenerateUniqueFile("txt"));
		string path2 = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service", path);
		using StreamWriter streamWriter = new StreamWriter(path2);
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		service.DumpStore(streamWriter);
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		service2.DumpContents(streamWriter);
		streamWriter.WriteLine();
	}

	internal DumpInventoryStoreJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
