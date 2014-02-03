using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class MessageHook
{
	private class MessageHookObject
	{
		internal WeakReference _reference;

		public MessageHookObject(MessageHook parent)
		{
			_reference = new WeakReference(parent, trackResurrection: false);
		}

		public virtual IntPtr Callback(int nCode, IntPtr wparam, IntPtr lparam)
		{
			IntPtr result = IntPtr.Zero;
			try
			{
				MessageHook messageHook = (MessageHook)_reference.Target;
				if (messageHook != null)
				{
					result = messageHook.MessageHookProc(nCode, wparam, lparam);
				}
			}
			catch
			{
			}
			return result;
		}
	}

	private IMessageHookClient _client;

	private Control _control;

	private bool _hookDisable;

	private IntPtr _messageHookHandle = IntPtr.Zero;

	private GCHandle _messageHookRoot;

	private bool _processing;

	internal int thisProcessID;

	public bool DisableMessageHook
	{
		get
		{
			return _hookDisable;
		}
		set
		{
			_hookDisable = value;
			if (value)
			{
				StopHookMessages();
			}
		}
	}

	public virtual bool HookMessages
	{
		get
		{
			GC.KeepAlive(this);
			return _messageHookHandle != IntPtr.Zero;
		}
		set
		{
			if (value && !_hookDisable)
			{
				StartHookMessages();
			}
			else
			{
				StopHookMessages();
			}
		}
	}

	public MessageHook(Control control, IMessageHookClient client)
	{
		_control = control;
		_client = client;
	}

	public void Dispose()
	{
		StopHookMessages();
	}

	private void StartHookMessages()
	{
		GC.KeepAlive(this);
		lock (this)
		{
			if (!(_messageHookHandle != IntPtr.Zero))
			{
				if (thisProcessID == 0)
				{
					Win32Calls.GetWindowThreadProcessId(new HandleRef(_control, _control.Handle), out thisProcessID);
				}
				MessageHookObject messageHookObject = new MessageHookObject(this);
				Win32Calls.HookProc hookProc = messageHookObject.Callback;
				_messageHookRoot = GCHandle.Alloc(hookProc);
				_messageHookHandle = Win32Calls.SetWindowsHookEx(4, hookProc, (IntPtr)0, Win32Calls.GetCurrentThreadId());
			}
		}
	}

	private IntPtr MessageHookProc(int nCode, IntPtr wParam, IntPtr lParam)
	{
		GC.KeepAlive(this);
		if (nCode >= 0)
		{
			Win32Calls.CWPSTRUCT cWPSTRUCT = (Win32Calls.CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32Calls.CWPSTRUCT));
			if (ProcessMessage(cWPSTRUCT.hwnd, cWPSTRUCT.message, cWPSTRUCT.wParam, cWPSTRUCT.lParam))
			{
				return (IntPtr)1;
			}
		}
		return Win32Calls.CallNextHookEx(new HandleRef(this, _messageHookHandle), nCode, wParam, lParam);
	}

	private bool ProcessMessage(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
	{
		if (!_processing)
		{
			Win32Calls.GetWindowThreadProcessId(new HandleRef(null, hWnd), out var lpdwProcessId);
			if (lpdwProcessId != thisProcessID)
			{
				HookMessages = false;
				return false;
			}
			try
			{
				_processing = true;
				if (_client.OnMessageHooked(Control.FromHandle(hWnd), message, wParam, lParam))
				{
					return true;
				}
			}
			finally
			{
				_processing = false;
			}
		}
		return false;
	}

	private void StopHookMessages()
	{
		GC.KeepAlive(this);
		lock (this)
		{
			if (_messageHookHandle != IntPtr.Zero)
			{
				Win32Calls.UnhookWindowsHookEx(new HandleRef(this, _messageHookHandle));
				_messageHookRoot.Free();
				_messageHookHandle = IntPtr.Zero;
			}
		}
	}
}
