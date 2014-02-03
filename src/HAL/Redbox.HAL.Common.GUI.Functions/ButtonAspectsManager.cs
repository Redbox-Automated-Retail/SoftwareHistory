using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class ButtonAspectsManager
{
	private Color Running { get; set; }

	public ButtonAspectsManager(Color running)
	{
		Running = running;
	}

	public ButtonAspectsManager()
	{
		Running = Color.Red;
	}

	public ButtonAspects MakeAspect(object button)
	{
		if (!(button is Button b))
		{
			return null;
		}
		return new ButtonAspects(b, Running);
	}
}
