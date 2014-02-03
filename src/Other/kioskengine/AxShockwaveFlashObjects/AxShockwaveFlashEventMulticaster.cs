using ShockwaveFlashObjects;

namespace AxShockwaveFlashObjects;

public class AxShockwaveFlashEventMulticaster : _IShockwaveFlashEvents
{
	private AxShockwaveFlash parent;

	public AxShockwaveFlashEventMulticaster(AxShockwaveFlash parent)
	{
		this.parent = parent;
	}

	public virtual void FlashCall(string request)
	{
		_IShockwaveFlashEvents_FlashCallEvent e = new _IShockwaveFlashEvents_FlashCallEvent(request);
		parent.RaiseOnFlashCall(parent, e);
	}

	public virtual void FSCommand(string command, string args)
	{
		_IShockwaveFlashEvents_FSCommandEvent e = new _IShockwaveFlashEvents_FSCommandEvent(command, args);
		parent.RaiseOnFSCommand(parent, e);
	}

	public virtual void OnProgress(int percentDone)
	{
		_IShockwaveFlashEvents_OnProgressEvent e = new _IShockwaveFlashEvents_OnProgressEvent(percentDone);
		parent.RaiseOnOnProgress(parent, e);
	}

	public virtual void OnReadyStateChange(int newState)
	{
		_IShockwaveFlashEvents_OnReadyStateChangeEvent e = new _IShockwaveFlashEvents_OnReadyStateChangeEvent(newState);
		parent.RaiseOnOnReadyStateChange(parent, e);
	}
}
