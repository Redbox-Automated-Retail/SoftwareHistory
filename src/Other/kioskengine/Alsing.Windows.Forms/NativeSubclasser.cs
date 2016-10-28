using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms;

[ComVisible(true)]
public class NativeSubclasser : NativeWindow
{
	public event NativeMessageHandler Message;

	public NativeSubclasser()
	{
	}

	public NativeSubclasser(Control Target)
	{
		AssignHandle(Target.Handle);
		Target.HandleCreated += Handle_Created;
		Target.HandleDestroyed += Handle_Destroyed;
	}

	public NativeSubclasser(IntPtr hWnd)
	{
		AssignHandle(hWnd);
	}

	protected virtual void OnMessage(NativeMessageArgs e)
	{
		if (Message != null)
		{
			Message(this, e);
		}
	}

	private void Handle_Created(object o, EventArgs e)
	{
		AssignHandle(((Control)o).Handle);
	}

	private void Handle_Destroyed(object o, EventArgs e)
	{
		ReleaseHandle();
	}

	public void Detatch()
	{
	}

	protected override void WndProc(ref Message m)
	{
		try
		{
			NativeMessageArgs nativeMessageArgs = new NativeMessageArgs();
			nativeMessageArgs.Message = m;
			nativeMessageArgs.Cancel = false;
			NativeMessageArgs nativeMessageArgs2 = nativeMessageArgs;
			OnMessage(nativeMessageArgs2);
			if (!nativeMessageArgs2.Cancel)
			{
				base.WndProc(ref m);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}
