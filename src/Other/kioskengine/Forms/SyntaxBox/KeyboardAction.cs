using System.Windows.Forms;

namespace Alsing.Windows.Forms.SyntaxBox;

public class KeyboardAction
{
	private Keys _Key;

	public bool Shift { get; set; }

	public bool Alt { get; set; }

	public bool Control { get; set; }

	public bool AllowReadOnly { get; set; }

	public Keys Key
	{
		get
		{
			return _Key;
		}
		set
		{
			_Key = value;
		}
	}

	public ActionDelegate Action { get; set; }

	public KeyboardAction()
	{
	}

	public KeyboardAction(Keys key, bool shift, bool control, bool alt, bool allowreadonly, ActionDelegate actionDelegate)
	{
		Key = key;
		Control = control;
		Alt = alt;
		Shift = shift;
		Action = actionDelegate;
		AllowReadOnly = allowreadonly;
	}
}
