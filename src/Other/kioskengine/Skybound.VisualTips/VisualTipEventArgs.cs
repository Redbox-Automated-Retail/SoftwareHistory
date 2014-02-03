using System;

namespace Skybound.VisualTips;

public class VisualTipEventArgs : EventArgs
{
	private VisualTip _xbc0838006ffe53fa;

	private object _x9834ddb0e0bd5996;

	private bool _xd674415062c2b55f;

	public VisualTip Tip => _xbc0838006ffe53fa;

	public object Instance => _x9834ddb0e0bd5996;

	public bool Cancel
	{
		get
		{
			return _xd674415062c2b55f;
		}
		set
		{
			_xd674415062c2b55f = value;
		}
	}

	public VisualTipEventArgs(VisualTip tip, object instance)
	{
		_xbc0838006ffe53fa = tip;
		_x9834ddb0e0bd5996 = instance;
	}
}
