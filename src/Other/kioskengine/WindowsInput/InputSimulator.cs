using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsInput;

public static class InputSimulator
{
	[DllImport("User32.dll", SetLastError = true)]
	private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

	[DllImport("User32.dll", SetLastError = true)]
	private static extern short GetAsyncKeyState(ushort virtualKeyCode);

	[DllImport("User32.dll", SetLastError = true)]
	private static extern short GetKeyState(ushort virtualKeyCode);

	[DllImport("User32.dll")]
	private static extern IntPtr GetMessageExtraInfo();

	public static bool IsKeyDownAsync(VirtualKeyCode keyCode)
	{
		short asyncKeyState = GetAsyncKeyState((ushort)keyCode);
		return asyncKeyState < 0;
	}

	public static bool IsKeyDown(VirtualKeyCode keyCode)
	{
		short keyState = GetKeyState((ushort)keyCode);
		return keyState < 0;
	}

	public static bool IsTogglingKeyInEffect(VirtualKeyCode keyCode)
	{
		short keyState = GetKeyState((ushort)keyCode);
		return (keyState & 1) == 1;
	}

	public static void SimulateLeftMouseDown(int x, int y)
	{
		INPUT iNPUT = default(INPUT);
		iNPUT.Type = 0u;
		iNPUT.Data.Mouse = default(MOUSEINPUT);
		iNPUT.Data.Mouse.X = x;
		iNPUT.Data.Mouse.Y = y;
		iNPUT.Data.Mouse.Time = 0u;
		iNPUT.Data.Mouse.MouseData = 0u;
		iNPUT.Data.Mouse.Flags = 2u;
		iNPUT.Data.Mouse.ExtraInfo = IntPtr.Zero;
		if (SendInput(1u, new INPUT[1] { iNPUT }, Marshal.SizeOf(typeof(INPUT))) == 0)
		{
			throw new Exception("The left mouse key down simulation was not successful.");
		}
	}

	public static void SimulateLeftMouseUp(int x, int y)
	{
		INPUT iNPUT = default(INPUT);
		iNPUT.Type = 0u;
		iNPUT.Data.Mouse = default(MOUSEINPUT);
		iNPUT.Data.Mouse.X = x;
		iNPUT.Data.Mouse.Y = y;
		iNPUT.Data.Mouse.Time = 0u;
		iNPUT.Data.Mouse.MouseData = 0u;
		iNPUT.Data.Mouse.Flags = 4u;
		iNPUT.Data.Mouse.ExtraInfo = IntPtr.Zero;
		if (SendInput(1u, new INPUT[1] { iNPUT }, Marshal.SizeOf(typeof(INPUT))) == 0)
		{
			throw new Exception("The left mouse key up simulation was not successful.");
		}
	}

	public static void SimulateKeyDown(VirtualKeyCode keyCode)
	{
		INPUT iNPUT = default(INPUT);
		iNPUT.Type = 1u;
		iNPUT.Data.Keyboard = default(KEYBDINPUT);
		iNPUT.Data.Keyboard.Vk = (ushort)keyCode;
		iNPUT.Data.Keyboard.Scan = 0;
		iNPUT.Data.Keyboard.Flags = 0u;
		iNPUT.Data.Keyboard.Time = 0u;
		iNPUT.Data.Keyboard.ExtraInfo = IntPtr.Zero;
		if (SendInput(1u, new INPUT[1] { iNPUT }, Marshal.SizeOf(typeof(INPUT))) == 0)
		{
			throw new Exception($"The key down simulation for {keyCode} was not successful.");
		}
	}

	public static void SimulateKeyUp(VirtualKeyCode keyCode)
	{
		INPUT iNPUT = default(INPUT);
		iNPUT.Type = 1u;
		iNPUT.Data.Keyboard = default(KEYBDINPUT);
		iNPUT.Data.Keyboard.Vk = (ushort)keyCode;
		iNPUT.Data.Keyboard.Scan = 0;
		iNPUT.Data.Keyboard.Flags = 2u;
		iNPUT.Data.Keyboard.Time = 0u;
		iNPUT.Data.Keyboard.ExtraInfo = IntPtr.Zero;
		if (SendInput(1u, new INPUT[1] { iNPUT }, Marshal.SizeOf(typeof(INPUT))) == 0)
		{
			throw new Exception($"The key up simulation for {keyCode} was not successful.");
		}
	}

	public static void SimulateKeyPress(VirtualKeyCode keyCode)
	{
		INPUT iNPUT = default(INPUT);
		iNPUT.Type = 1u;
		iNPUT.Data.Keyboard = default(KEYBDINPUT);
		iNPUT.Data.Keyboard.Vk = (ushort)keyCode;
		iNPUT.Data.Keyboard.Scan = 0;
		iNPUT.Data.Keyboard.Flags = 0u;
		iNPUT.Data.Keyboard.Time = 0u;
		iNPUT.Data.Keyboard.ExtraInfo = IntPtr.Zero;
		INPUT iNPUT2 = default(INPUT);
		iNPUT2.Type = 1u;
		iNPUT2.Data.Keyboard = default(KEYBDINPUT);
		iNPUT2.Data.Keyboard.Vk = (ushort)keyCode;
		iNPUT2.Data.Keyboard.Scan = 0;
		iNPUT2.Data.Keyboard.Flags = 2u;
		iNPUT2.Data.Keyboard.Time = 0u;
		iNPUT2.Data.Keyboard.ExtraInfo = IntPtr.Zero;
		if (SendInput(2u, new INPUT[2] { iNPUT, iNPUT2 }, Marshal.SizeOf(typeof(INPUT))) == 0)
		{
			throw new Exception($"The key press simulation for {keyCode} was not successful.");
		}
	}

	public static void SimulateTextEntry(string text)
	{
		if ((long)text.Length > 2147483647L)
		{
			throw new ArgumentException($"The text parameter is too long. It must be less than {2147483647u} characters.", "text");
		}
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		int num = bytes.Length;
		INPUT[] array = new INPUT[num * 2];
		for (int i = 0; i < num; i++)
		{
			ushort num2 = bytes[i];
			INPUT iNPUT = default(INPUT);
			iNPUT.Type = 1u;
			iNPUT.Data.Keyboard = default(KEYBDINPUT);
			iNPUT.Data.Keyboard.Vk = 0;
			iNPUT.Data.Keyboard.Scan = num2;
			iNPUT.Data.Keyboard.Flags = 4u;
			iNPUT.Data.Keyboard.Time = 0u;
			iNPUT.Data.Keyboard.ExtraInfo = IntPtr.Zero;
			INPUT iNPUT2 = default(INPUT);
			iNPUT2.Type = 1u;
			iNPUT2.Data.Keyboard = default(KEYBDINPUT);
			iNPUT2.Data.Keyboard.Vk = 0;
			iNPUT2.Data.Keyboard.Scan = num2;
			iNPUT2.Data.Keyboard.Flags = 6u;
			iNPUT2.Data.Keyboard.Time = 0u;
			iNPUT2.Data.Keyboard.ExtraInfo = IntPtr.Zero;
			if ((num2 & 0xFF00) == 57344)
			{
				iNPUT.Data.Keyboard.Flags |= 1u;
				iNPUT2.Data.Keyboard.Flags |= 1u;
			}
			array[2 * i] = iNPUT;
			array[2 * i + 1] = iNPUT2;
		}
		uint num3 = SendInput((uint)(num * 2), array, Marshal.SizeOf(typeof(INPUT)));
	}

	public static void SimulateModifiedKeyStroke(VirtualKeyCode modifierKeyCode, VirtualKeyCode keyCode)
	{
		SimulateKeyDown(modifierKeyCode);
		SimulateKeyPress(keyCode);
		SimulateKeyUp(modifierKeyCode);
	}

	public static void SimulateModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode)
	{
		modifierKeyCodes?.ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyDown(x);
		});
		SimulateKeyPress(keyCode);
		modifierKeyCodes?.Reverse().ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyUp(x);
		});
	}

	public static void SimulateModifiedKeyStroke(VirtualKeyCode modifierKey, IEnumerable<VirtualKeyCode> keyCodes)
	{
		SimulateKeyDown(modifierKey);
		keyCodes?.ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyPress(x);
		});
		SimulateKeyUp(modifierKey);
	}

	public static void SimulateModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes)
	{
		modifierKeyCodes?.ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyDown(x);
		});
		keyCodes?.ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyPress(x);
		});
		modifierKeyCodes?.Reverse().ToList().ForEach(delegate(VirtualKeyCode x)
		{
			SimulateKeyUp(x);
		});
	}
}
