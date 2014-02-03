using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class ButtonAspects : IDisposable
{
	private Color RunningColor { get; set; }

	private Color CompleteColor { get; set; }

	private Button Button { get; set; }

	public void Dispose()
	{
		if (Button != null)
		{
			Button.BackColor = CompleteColor;
			Application.DoEvents();
		}
	}

	internal ButtonAspects(Button b, Color running)
	{
		RunningColor = running;
		CompleteColor = b.BackColor;
		if (b != null)
		{
			Button = b;
			Button.BackColor = RunningColor;
			Application.DoEvents();
		}
	}
}
