using System.Windows.Forms;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterOutputBox : OutputBox
{
	private readonly Form1 TheForm;

	protected override void PostWrite(string s)
	{
		TheForm.LogMessage(s);
	}

	internal TesterOutputBox(ListBox box, Form1 form)
		: base(box)
	{
		TheForm = form;
	}
}
