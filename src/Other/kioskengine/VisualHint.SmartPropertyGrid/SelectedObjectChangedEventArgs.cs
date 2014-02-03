using System;

namespace VisualHint.SmartPropertyGrid;

public class SelectedObjectChangedEventArgs : EventArgs
{
	private object[] _previousTargets;

	public object[] PreviousTargets => _previousTargets;

	public SelectedObjectChangedEventArgs(object[] previousTargets)
	{
		_previousTargets = previousTargets;
	}
}
