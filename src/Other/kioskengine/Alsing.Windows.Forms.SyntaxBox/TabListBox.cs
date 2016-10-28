using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.SyntaxBox;

[ToolboxItem(false)]
[ComVisible(true)]
public class TabListBox : ListBox
{
	protected override bool IsInputKey(Keys keyData)
	{
		return true;
	}

	protected override bool IsInputChar(char charCode)
	{
		return true;
	}
}
