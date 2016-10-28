using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms;

[ComVisible(true)]
public delegate void NativeMessageHandler(object s, NativeMessageArgs e);
