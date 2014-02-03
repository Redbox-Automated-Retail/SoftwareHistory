using System.Collections.Generic;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Camera.Framework;

internal class SimplePipeClient : AbstractPipeClientSession
{
	internal static string SnapImage()
	{
		using SimplePipeClient simplePipeClient = new SimplePipeClient("CAMERATUNER", "NONE");
		simplePipeClient.Connect();
		simplePipeClient.Write("SNAP");
		List<string> list = simplePipeClient.ConsumeMessages();
		if (list.Count == 0)
		{
			return "NONE";
		}
		string text = list[0];
		return text.TrimEnd();
	}

	public List<string> ExecuteCommand(string command)
	{
		Write(command);
		return ConsumeMessages();
	}

	~SimplePipeClient()
	{
		DisposeInner(disposing: false);
	}

	protected override List<string> ProcessRawMessage()
	{
		List<string> list = new List<string>();
		list.Add(base.ReadBuilder.ToString());
		return list;
	}

	private SimplePipeClient(string pipeName, string id)
		: base(id, pipeName)
	{
	}
}
