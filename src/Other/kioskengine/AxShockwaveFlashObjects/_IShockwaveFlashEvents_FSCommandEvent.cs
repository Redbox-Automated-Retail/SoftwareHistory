using System.Runtime.InteropServices;

namespace AxShockwaveFlashObjects;

[ComVisible(true)]
public class _IShockwaveFlashEvents_FSCommandEvent
{
	public string command;

	public string args;

	public _IShockwaveFlashEvents_FSCommandEvent(string command, string args)
	{
		this.command = command;
		this.args = args;
	}
}
