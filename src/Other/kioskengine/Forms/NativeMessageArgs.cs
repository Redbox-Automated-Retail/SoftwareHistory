using System;
using System.Windows.Forms;

namespace Alsing.Windows.Forms;

public class NativeMessageArgs : EventArgs
{
	public bool Cancel;

	public Message Message;
}
