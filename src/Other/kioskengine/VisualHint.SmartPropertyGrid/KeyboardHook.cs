using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class KeyboardHook
{
	private class KeyboardHookObject
	{
		internal WeakReference _reference;

		public KeyboardHookObject(KeyboardHook parent)
		{
			_reference = new WeakReference(parent, trackResurrection: false);
		}

		public virtual IntPtr Callback(int nCode, IntPtr wparam, IntPtr lparam)
		{
			IntPtr result = IntPtr.Zero;
			try
			{
				KeyboardHook keyboardHook = (KeyboardHook)_reference.Target;
				if (keyboardHook != null)
				{
					result = keyboardHook.KeyboardHookProc(nCode, wparam, lparam);
				}
			}
			catch
			{
			}
			return result;
		}
	}

	private IKeyboardHookClient _client;

	private Control _control;

	private bool _hookDisable;

	private IntPtr _keyboardHookHandle = IntPtr.Zero;

	private GCHandle _keyboardHookRoot;

	private bool _processing;

	public bool DisableKeyboardHook
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
			return _keyboardHookHandle != IntPtr.Zero;
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

	public KeyboardHook(Control control, IKeyboardHookClient client)
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
			if (!(_keyboardHookHandle != IntPtr.Zero))
			{
				KeyboardHookObject keyboardHookObject = new KeyboardHookObject(this);
				Win32Calls.HookProc hookProc = keyboardHookObject.Callback;
				_keyboardHookRoot = GCHandle.Alloc(hookProc);
				_keyboardHookHandle = Win32Calls.SetWindowsHookEx(2, hookProc, (IntPtr)0, Win32Calls.GetCurrentThreadId());
			}
		}
	}

	private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
	{
		GC.KeepAlive(this);
		if (nCode == 0 && ProcessMessage(wParam, lParam))
		{
			return (IntPtr)1;
		}
		return Win32Calls.CallNextHookEx(new HandleRef(this, _keyboardHookHandle), nCode, wParam, lParam);
	}

	private bool ProcessMessage(IntPtr wParam, IntPtr lParam)
	{
		if (!_processing)
		{
			try
			{
				_processing = true;
				int message = ((((long)lParam & int.MinValue) != 0) ? 257 : 256);
				if (_client.OnKeyboardHooked(message, new KeyEventArgs((Keys)((int)(long)wParam | (int)Control.ModifierKeys))))
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
			if (_keyboardHookHandle != IntPtr.Zero)
			{
				Win32Calls.UnhookWindowsHookEx(new HandleRef(this, _keyboardHookHandle));
				_keyboardHookRoot.Free();
				_keyboardHookHandle = IntPtr.Zero;
			}
		}
	}
}
