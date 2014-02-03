using System;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class DropDownContentClosedEventArgs : EventArgs
{
	private bool _toBeValidated;

	private bool _reasonLostFocus;

	private Control _wndTakingFocus;

	public bool ToBeValidated => _toBeValidated;

	public bool ReasonLostFocus => _reasonLostFocus;

	public Control WndTakingFocus => _wndTakingFocus;

	public DropDownContentClosedEventArgs(bool toBeValidated)
	{
		_toBeValidated = toBeValidated;
	}

	public DropDownContentClosedEventArgs(bool toBeValidated, Control wndTakingFocus)
	{
		_toBeValidated = toBeValidated;
		_wndTakingFocus = wndTakingFocus;
		_reasonLostFocus = true;
	}
}
