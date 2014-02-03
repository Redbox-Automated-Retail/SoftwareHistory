using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace Skybound.Windows.Forms;

internal struct ThemeRenderer
{
	private class ThemeControlHelper : Control
	{
		public ThemeControlHelper()
		{
			CreateControl();
		}
	}

	private class ThemeChangedWatcher : ThemeControlHelper
	{
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 794)
			{
				ResetInitialization();
			}
			base.WndProc(ref m);
		}
	}

	private struct Htheme : IDisposable
	{
		private bool _CachedHandle;

		private static string CurrentApplicationName;

		private IntPtr _Handle;

		public bool IsValid => _Handle != IntPtr.Zero;

		public IntPtr Handle => _Handle;

		public Htheme(ThemeRenderer theme)
		{
			bool flag = theme.Value >> 24 != 0;
			if (flag)
			{
				string applicationName = theme.ApplicationName;
				if (applicationName != CurrentApplicationName)
				{
					ThemeInternal.UxTheme.SetWindowTheme(ThemeControl.Handle, CurrentApplicationName = applicationName, null);
				}
			}
			if (!flag)
			{
				int num = (theme.Value >> 8) & 0xFFFF;
				if (num > 0 && num <= HthemeCache.Length)
				{
					num--;
					Htheme htheme = HthemeCache[num];
					if (htheme._Handle == IntPtr.Zero)
					{
						htheme._Handle = ThemeInternal.UxTheme.OpenThemeData(CachedThemeControl.Handle, theme.ClassName);
						HthemeCache[num] = htheme;
					}
					_Handle = htheme._Handle;
					_CachedHandle = true;
					return;
				}
			}
			_Handle = ThemeInternal.UxTheme.OpenThemeData(ThemeControl.Handle, theme.ClassName);
			_CachedHandle = false;
		}

		public void Dispose()
		{
			if (!_CachedHandle)
			{
				ThemeInternal.UxTheme.CloseThemeData(_Handle);
			}
		}
	}

	private class ThemeInternal
	{
		public struct RECT(Rectangle r)
		{
			public int Left = r.Left;

			public int Top = r.Top;

			public int Right = r.Right;

			public int Bottom = r.Bottom;

			public Rectangle ToRectangle()
			{
				return Rectangle.FromLTRB(Left, Top, Right, Bottom);
			}

			public override string ToString()
			{
				return ToRectangle().ToString();
			}
		}

		[SuppressUnmanagedCodeSecurity]
		public sealed class UxTheme
		{
			private UxTheme()
			{
			}

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern IntPtr OpenThemeData(IntPtr hwnd, string pszClassList);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern bool CloseThemeData(IntPtr hTheme);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, ref RECT pClipRect);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int DrawThemeBackgroundEx(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, ref DTBGOPTS pOptions);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int DrawThemeParentBackground(IntPtr hwnd, IntPtr hdc, ref RECT prc);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeColor(IntPtr hTheme, int partId, int stateId, int propId, out int color);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeMargins(IntPtr hTheme, IntPtr hdc, int partId, int stateId, int propId, ref RECT pRect, int[] pMargins);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeMetric(IntPtr hTheme, IntPtr hdc, int partId, int stateId, int propId, out int piVal);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeBackgroundContentRect(IntPtr hTheme, IntPtr hdc, int partId, int stateId, ref RECT pBoundingRect, out RECT pContentRect);

			[DllImport("uxtheme", CharSet = CharSet.Auto)]
			public static extern int GetThemePartSize(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, IntPtr prc, ThemeSizeType eSize, out POINTAPI psz);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int DrawThemeText(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlag, int dwTextFlags2, ref RECT pRect);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeFilename(IntPtr hTheme, int iPartId, int iStateId, int iPropId, [MarshalAs(UnmanagedType.LPWStr)] string pszThemeFilename, int cchMaxBuffChars);

			[DllImport("uxtheme", CharSet = CharSet.Unicode, ExactSpelling = true)]
			public static extern int GetCurrentThemeName(string stringThemeName, int lengthThemeName, string stringColorName, int lengthColorName, string stringSizeName, int lengthSizeName);

			[DllImport("uxtheme", ExactSpelling = true)]
			public static extern bool IsAppThemed();

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int SetWindowTheme(IntPtr hwnd, string subAppName, string subIdList);

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern bool IsThemeActive();

			[DllImport("uxtheme", CharSet = CharSet.Unicode)]
			public static extern int GetThemeAppProperties();
		}

		public struct DTBGOPTS
		{
			public int dwSize;

			public int dwFlags;

			public RECT rcClip;
		}

		public struct DLLVERSIONINFO
		{
			public int cbSize;

			public int dwMajorVersion;

			public int dwMinorVersion;

			public int dwBuildNumber;

			public int dwPlatformID;
		}

		public struct POINTAPI
		{
			public int X;

			public int Y;
		}

		[DllImport("comctl32.dll")]
		public static extern int DllGetVersion(ref DLLVERSIONINFO dvi);

		[DllImport("gdi32.dll")]
		public static extern bool GetWindowOrgEx(IntPtr hdc, out POINTAPI lpPoint);
	}

	private const int PartMask = 255;

	private const int ClassShift = 8;

	private const int ClassMask = 65535;

	private const int ApplicationShift = 24;

	private const int ApplicationMask = 255;

	private const int SortedClassNameCount = 25;

	private const int SortedApplicationNameCount = 15;

	private static bool InitializationComplete;

	private static bool _Supported;

	private static bool _IsThemeActive;

	private static Version _CommonControlVersion;

	private static Control CachedThemeControl;

	private static Control ThemeControl;

	private static Control ThemeChangedControl;

	private int Value;

	private static ArrayList ClassNameCache;

	private static ArrayList ApplicationNameCache;

	private static Htheme[] HthemeCache;

	public static bool Supported
	{
		get
		{
			EnsureInitialized();
			return _Supported;
		}
	}

	public static bool Enabled
	{
		get
		{
			EnsureInitialized();
			if (_CommonControlVersion == null || _CommonControlVersion.Major < 6)
			{
				return false;
			}
			if (_IsThemeActive)
			{
				return InvokeIsAppThemed();
			}
			return false;
		}
	}

	public static Version CommonControlVersion => _CommonControlVersion;

	public string ApplicationName
	{
		get
		{
			return GetCachedString(ApplicationNameCache, 24, 255);
		}
		set
		{
			SetCachedString(ApplicationNameCache, 15, 24, 255, value);
		}
	}

	public string ClassName
	{
		get
		{
			return GetCachedString(ClassNameCache, 8, 65535);
		}
		set
		{
			SetCachedString(ClassNameCache, 25, 8, 65535, value);
		}
	}

	public int PartID
	{
		get
		{
			return Value & 0xFF;
		}
		set
		{
			Value = (Value & -256) | (value & 0xFF);
		}
	}

	private static void EnsureInitialized()
	{
		if (!InitializationComplete && Application.MessageLoop)
		{
			_CommonControlVersion = GetCommonControlVersion();
			_Supported = OSFeature.Feature.IsPresent(OSFeature.Themes);
			if (_CommonControlVersion.Major >= 6)
			{
				_IsThemeActive = ThemeInternal.UxTheme.IsThemeActive();
			}
			if (ThemeControl == null)
			{
				CachedThemeControl = new ThemeControlHelper();
				ThemeControl = new ThemeControlHelper();
			}
			InitializationComplete = true;
		}
		if (ThemeChangedControl == null)
		{
			ThemeChangedControl = new ThemeChangedWatcher();
		}
	}

	private static Version GetCommonControlVersion()
	{
		ThemeInternal.DLLVERSIONINFO dvi = new ThemeInternal.DLLVERSIONINFO
		{
			cbSize = Marshal.SizeOf(typeof(ThemeInternal.DLLVERSIONINFO))
		};
		ThemeInternal.DllGetVersion(ref dvi);
		return new Version(dvi.dwMajorVersion, dvi.dwMinorVersion, dvi.dwBuildNumber);
	}

	private static bool InvokeIsAppThemed()
	{
		return ThemeInternal.UxTheme.IsAppThemed();
	}

	public static void ResetInitialization()
	{
		InitializationComplete = false;
		_CommonControlVersion = null;
		HthemeCache = new Htheme[ClassNameCache.Count];
	}

	public static bool HasTransparentBackground(Control control)
	{
		if (control.BackgroundImage != null)
		{
			return false;
		}
		if (!control.BackColor.Equals(SystemColors.Control))
		{
			return false;
		}
		return true;
	}

	public static void PaintBackground(Control control, PaintEventArgs e)
	{
		if (Supported)
		{
			IntPtr hdc = IntPtr.Zero;
			try
			{
				hdc = e.Graphics.GetHdc();
				PaintBackground(control, hdc, e.ClipRectangle);
			}
			finally
			{
				e.Graphics.ReleaseHdc(hdc);
			}
		}
	}

	public static void PaintBackground(Control control, IntPtr hdc, Rectangle clipBounds)
	{
		ThemeInternal.RECT prc = new ThemeInternal.RECT(clipBounds);
		ThemeInternal.UxTheme.DrawThemeParentBackground(control.Handle, hdc, ref prc);
	}

	public ThemeRenderer(int partId)
	{
		Value = partId;
	}

	private string GetCachedString(ArrayList cache, int shift, int mask)
	{
		int num = (Value >> shift) & mask;
		if (num > 0 && num <= cache.Count)
		{
			return cache[num - 1] as string;
		}
		return null;
	}

	private void SetCachedString(ArrayList cache, int sortedItemCount, int shift, int mask, string value)
	{
		if (value == null)
		{
			Value &= ~(mask << shift);
			return;
		}
		int num = cache.BinarySearch(0, sortedItemCount, value, null);
		if (num < 0)
		{
			num = cache.IndexOf(value, sortedItemCount);
			if (num < 0)
			{
				num = cache.Add(value);
			}
		}
		Value = (num + 1 << shift) | (Value & ~(mask << shift));
	}

	static ThemeRenderer()
	{
		ClassNameCache = new ArrayList();
		ClassNameCache.Add("Button");
		ClassNameCache.Add("Clock");
		ClassNameCache.Add("ComboBox");
		ClassNameCache.Add("Edit");
		ClassNameCache.Add("ExplorerBar");
		ClassNameCache.Add("Header");
		ClassNameCache.Add("ListView");
		ClassNameCache.Add("Menu");
		ClassNameCache.Add("MenuBand");
		ClassNameCache.Add("Page");
		ClassNameCache.Add("Progress");
		ClassNameCache.Add("Rebar");
		ClassNameCache.Add("Scrollbar");
		ClassNameCache.Add("Spin");
		ClassNameCache.Add("StartPanel");
		ClassNameCache.Add("Status");
		ClassNameCache.Add("Tab");
		ClassNameCache.Add("TaskBand");
		ClassNameCache.Add("TaskBar");
		ClassNameCache.Add("Toolbar");
		ClassNameCache.Add("Tooltip");
		ClassNameCache.Add("Trackbar");
		ClassNameCache.Add("TrayNotify");
		ClassNameCache.Add("Treeview");
		ClassNameCache.Add("Window");
		ApplicationNameCache = new ArrayList();
		ApplicationNameCache.Add("ExplorerBar");
		ApplicationNameCache.Add("PlacesBar");
		ApplicationNameCache.Add("Start");
		ApplicationNameCache.Add("StartMenu");
		ApplicationNameCache.Add("TaskBand");
		ApplicationNameCache.Add("TaskBandGroupMenu");
		ApplicationNameCache.Add("TaskBandVert");
		ApplicationNameCache.Add("TaskBar");
		ApplicationNameCache.Add("TaskBarVert");
		ApplicationNameCache.Add("TrayNotify");
		ApplicationNameCache.Add("TrayNotifyHoriz");
		ApplicationNameCache.Add("TrayNotifyHorizOpen");
		ApplicationNameCache.Add("TrayNotifyVert");
		ApplicationNameCache.Add("TrayNotifyVertOpen");
		ApplicationNameCache.Add("Shell");
		HthemeCache = new Htheme[ClassNameCache.Count];
	}

	public void Draw(Graphics graphics, int state, Rectangle bounds)
	{
		Draw(graphics, state, bounds, bounds);
	}

	public void Draw(Graphics graphics, int state, Rectangle bounds, Rectangle clipBounds)
	{
		if (!Supported)
		{
			return;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			DrawCore(intPtr = graphics.GetHdc(), state, bounds, clipBounds);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				graphics.ReleaseHdc(intPtr);
			}
		}
	}

	public void Draw(IntPtr hdc, int state, Rectangle bounds, Rectangle clipBounds)
	{
		if (Supported)
		{
			DrawCore(hdc, state, bounds, clipBounds);
		}
	}

	private void DrawCore(IntPtr hdc, int state, Rectangle bounds, Rectangle clipBounds)
	{
		using Htheme htheme = new Htheme(this);
		if (htheme.IsValid)
		{
			ThemeInternal.RECT pRect = new ThemeInternal.RECT(bounds);
			ThemeInternal.RECT pClipRect = new ThemeInternal.RECT(clipBounds);
			ThemeInternal.UxTheme.DrawThemeBackground(htheme.Handle, hdc, PartID, state, ref pRect, ref pClipRect);
		}
	}

	public void Draw(IntPtr hdc, int state, Rectangle bounds, Rectangle clipBounds, ThemeDrawFlags flags)
	{
		using Htheme htheme = new Htheme(this);
		if (htheme.IsValid)
		{
			if ((flags & ThemeDrawFlags.UseDCOrigin) == ThemeDrawFlags.UseDCOrigin)
			{
				ThemeInternal.GetWindowOrgEx(hdc, out var lpPoint);
				bounds.Offset(lpPoint.X * 2, lpPoint.Y * 2);
				clipBounds.Offset(lpPoint.X * 2, lpPoint.Y * 2);
				flags &= (ThemeDrawFlags)(-65);
			}
			ThemeInternal.RECT pRect = new ThemeInternal.RECT(bounds);
			ThemeInternal.DTBGOPTS pOptions = new ThemeInternal.DTBGOPTS
			{
				dwSize = Marshal.SizeOf(typeof(ThemeInternal.DTBGOPTS)),
				dwFlags = (int)flags,
				rcClip = new ThemeInternal.RECT(clipBounds)
			};
			ThemeInternal.UxTheme.DrawThemeBackgroundEx(htheme.Handle, hdc, PartID, state, ref pRect, ref pOptions);
		}
	}

	public void DrawText(IntPtr hdc, int state, Rectangle bounds, string text, bool disabled, ThemeTextFormatFlags flags)
	{
		if (!Supported || text == null || text.Length <= 0 || !(hdc != IntPtr.Zero))
		{
			return;
		}
		using Htheme htheme = new Htheme(this);
		if (htheme.IsValid)
		{
			ThemeInternal.RECT pRect = new ThemeInternal.RECT(bounds);
			ThemeInternal.UxTheme.DrawThemeText(htheme.Handle, hdc, PartID, state, text, text.Length, (int)flags, disabled ? 1 : 0, ref pRect);
		}
	}

	public Color GetColor(int state, ThemeColor color)
	{
		return GetColorCore(state, (int)color);
	}

	public Color GetTextColor(int state)
	{
		return GetColorCore(state, 3803);
	}

	private Color GetColorCore(int state, int colorConstant)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid && ThemeInternal.UxTheme.GetThemeColor(htheme.Handle, PartID, state, colorConstant, out var color) == 0)
			{
				return ColorTranslator.FromWin32(color);
			}
		}
		return Color.Red;
	}

	public int GetMetric(int state, ThemeMetric metric)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid && ThemeInternal.UxTheme.GetThemeMetric(htheme.Handle, IntPtr.Zero, PartID, state, (int)metric, out var piVal) == 0)
			{
				return piVal;
			}
		}
		return 0;
	}

	public string GetFileName(int state, ThemeFileName filename)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid)
			{
				string text = new string('\0', 128);
				if (ThemeInternal.UxTheme.GetThemeFilename(htheme.Handle, PartID, state, (int)filename, text, 127) == 0)
				{
					return text.Substring(0, text.IndexOf('\0'));
				}
			}
		}
		return null;
	}

	public Rectangle AdjustRectangle(int state, Rectangle bounds, ThemeMargin margin)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid)
			{
				ThemeInternal.RECT pRect = new ThemeInternal.RECT(bounds);
				int[] array = new int[4];
				if (ThemeInternal.UxTheme.GetThemeMargins(htheme.Handle, IntPtr.Zero, PartID, state, (int)margin, ref pRect, array) == 0)
				{
					return Rectangle.FromLTRB(bounds.X + array[0], bounds.Y + array[2], bounds.Right - array[1], bounds.Bottom - array[3]);
				}
			}
		}
		return bounds;
	}

	public Rectangle GetBackgroundContentRectangle(IntPtr hdc, int state, Rectangle bounds)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid)
			{
				ThemeInternal.RECT pBoundingRect = new ThemeInternal.RECT(bounds);
				if (ThemeInternal.UxTheme.GetThemeBackgroundContentRect(htheme.Handle, hdc, PartID, state, ref pBoundingRect, out var pContentRect) == 0)
				{
					return pContentRect.ToRectangle();
				}
			}
		}
		return bounds;
	}

	public Size GetPartSize(IntPtr hdc, int state, ThemeSizeType sizeType)
	{
		if (Supported)
		{
			using Htheme htheme = new Htheme(this);
			if (htheme.IsValid && ThemeInternal.UxTheme.GetThemePartSize(htheme.Handle, hdc, PartID, state, IntPtr.Zero, sizeType, out var psz) == 0)
			{
				return new Size(psz.X, psz.Y);
			}
		}
		return Size.Empty;
	}
}
