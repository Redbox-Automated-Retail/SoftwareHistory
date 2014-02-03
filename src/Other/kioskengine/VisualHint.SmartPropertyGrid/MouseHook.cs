using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class MouseHook
{
	private class MouseHookObject
	{
		internal WeakReference _reference;

		public MouseHookObject(MouseHook parent)
		{
			_reference = new WeakReference(parent, trackResurrection: false);
		}

		public virtual IntPtr Callback(int nCode, IntPtr wparam, IntPtr lparam)
		{
			IntPtr result = IntPtr.Zero;
			try
			{
				MouseHook mouseHook = (MouseHook)_reference.Target;
				if (mouseHook != null)
				{
					result = mouseHook.MouseHookProc(nCode, wparam, lparam);
				}
			}
			catch
			{
			}
			return result;
		}
	}

	private IMouseHookClient _client;

	private Control _control;

	private bool _hookDisable;

	private IntPtr _mouseHookHandle = IntPtr.Zero;

	private GCHandle _mouseHookRoot;

	private bool _processing;

	private int _thisProcessID;

	public bool DisableMouseHook
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
			return _mouseHookHandle != IntPtr.Zero;
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

	public MouseHook(Control control, IMouseHookClient client)
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
			if (!(_mouseHookHandle != IntPtr.Zero))
			{
				if (_thisProcessID == 0)
				{
					Win32Calls.GetWindowThreadProcessId(new HandleRef(_control, _control.Handle), out _thisProcessID);
				}
				MouseHookObject mouseHookObject = new MouseHookObject(this);
				Win32Calls.HookProc hookProc = mouseHookObject.Callback;
				_mouseHookRoot = GCHandle.Alloc(hookProc);
				_mouseHookHandle = Win32Calls.SetWindowsHookEx(7, hookProc, (IntPtr)0, Win32Calls.GetCurrentThreadId());
			}
		}
	}

	private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
	{
		GC.KeepAlive(this);
		if (nCode == 0)
		{
			Win32Calls.CMHSTRUCT cMHSTRUCT = (Win32Calls.CMHSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32Calls.CMHSTRUCT));
			if (ProcessMessage(cMHSTRUCT.hwnd, (uint)(int)wParam, cMHSTRUCT.pt, cMHSTRUCT.wHitTestCode))
			{
				return (IntPtr)1;
			}
		}
		return Win32Calls.CallNextHookEx(new HandleRef(this, _mouseHookHandle), nCode, wParam, lParam);
	}

	private bool ProcessMessage(IntPtr hWnd, uint message, Point pt, uint hitTest)
	{
		if (!_processing)
		{
			Win32Calls.GetWindowThreadProcessId(new HandleRef(null, hWnd), out var lpdwProcessId);
			if (lpdwProcessId != _thisProcessID)
			{
				HookMessages = false;
				return false;
			}
			try
			{
				_processing = true;
				if (_client.OnMouseHooked(hWnd, message, pt, hitTest))
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
			if (_mouseHookHandle != IntPtr.Zero)
			{
				Win32Calls.UnhookWindowsHookEx(new HandleRef(this, _mouseHookHandle));
				_mouseHookRoot.Free();
				_mouseHookHandle = IntPtr.Zero;
			}
		}
	}
}
