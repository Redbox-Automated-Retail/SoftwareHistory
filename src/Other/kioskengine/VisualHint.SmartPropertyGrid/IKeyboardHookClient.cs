using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal interface IKeyboardHookClient
{
	bool OnKeyboardHooked(int message, KeyEventArgs args);
}
