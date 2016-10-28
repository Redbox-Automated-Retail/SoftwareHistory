using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms;

[ComVisible(true)]
public class NativeMessageArgs : EventArgs
{
	public bool Cancel;

	public Message Message;
}
