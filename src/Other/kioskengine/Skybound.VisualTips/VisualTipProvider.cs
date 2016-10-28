using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Skybound.Drawing.Design;
using Skybound.VisualTips.Rendering;
using x1b802d6029d7be4a;

namespace Skybound.VisualTips;

[DefaultEvent("AccessKeyPressed")]
[ProvideProperty("VisualTip", typeof(object))]
public class VisualTipProvider : Component, IExtenderProvider
{
	internal class x1fc6d0e92d52d447 : ReadOnlyCollectionBase
	{
		private struct xc069b2c9b6cac46f
		{
			public x700822a7c6d92025 x2370723dc8f1aec9;

			public Control xd5a7a92b8cfb14b3;
		}

		public void xd6b6ed77479ef68c(x700822a7c6d92025 x76b3d9d2638e5ecd, Control x43bec302f92080b9)
		{
			xd0a6edd1bc5a0fd5(x43bec302f92080b9);
			xc069b2c9b6cac46f xc069b2c9b6cac46f2 = new xc069b2c9b6cac46f
			{
				x2370723dc8f1aec9 = x76b3d9d2638e5ecd,
				xd5a7a92b8cfb14b3 = x43bec302f92080b9
			};
			base.InnerList.Add(xc069b2c9b6cac46f2);
		}

		private bool xd0a6edd1bc5a0fd5(Control x43bec302f92080b9)
		{
			for (int num = base.InnerList.Count - 1; num >= 0; num--)
			{
				xc069b2c9b6cac46f xc069b2c9b6cac46f2 = (xc069b2c9b6cac46f)base.InnerList[num];
				if (xc069b2c9b6cac46f2.x2370723dc8f1aec9.x7965faace5d79aa4)
				{
					if (x43bec302f92080b9 == null || xc069b2c9b6cac46f2.xd5a7a92b8cfb14b3 == x43bec302f92080b9)
					{
						bool result = (xc069b2c9b6cac46f2.x2370723dc8f1aec9.x73979cef1002ed01 & VisualTipDisplayOptions.ForwardEscapeKey) == 0;
						xc069b2c9b6cac46f2.x2370723dc8f1aec9.xd7692f35479850d5();
						return result;
					}
				}
				else
				{
					base.InnerList.RemoveAt(num);
				}
			}
			return false;
		}

		public bool x5b2e42a68d835b31(Keys xba08ce632055a1d9)
		{
			if (xba08ce632055a1d9 == Keys.Escape)
			{
				return xd0a6edd1bc5a0fd5(null);
			}
			if (base.InnerList.Count > 0)
			{
				xc069b2c9b6cac46f xc069b2c9b6cac46f2 = (xc069b2c9b6cac46f)base.InnerList[base.InnerList.Count - 1];
				VisualTip x8c3cc20aa74dd99c = xc069b2c9b6cac46f2.x2370723dc8f1aec9.x8c3cc20aa74dd99c;
				if (x8c3cc20aa74dd99c.AccessKey == (Shortcut)xba08ce632055a1d9)
				{
					VisualTipEventArgs e = new VisualTipEventArgs(x8c3cc20aa74dd99c.Provider.x2795ceea9a4a3d4f, (x8c3cc20aa74dd99c.Provider.xc16dc8a3b56cfe52 != null) ? x8c3cc20aa74dd99c.Provider.xc16dc8a3b56cfe52 : x8c3cc20aa74dd99c.Provider.x13c2275e5d07ac49);
					xd0a6edd1bc5a0fd5(null);
					x8c3cc20aa74dd99c.Provider.OnAccessKeyPressed(e);
					return true;
				}
			}
			return false;
		}

		public void x52b190e626f65140(x700822a7c6d92025 x76b3d9d2638e5ecd)
		{
			for (int num = base.InnerList.Count - 1; num >= 0; num--)
			{
				xc069b2c9b6cac46f xc069b2c9b6cac46f2 = (xc069b2c9b6cac46f)base.InnerList[num];
				if (xc069b2c9b6cac46f2.x2370723dc8f1aec9 == x76b3d9d2638e5ecd)
				{
					base.InnerList.RemoveAt(num);
					break;
				}
			}
		}
	}

	private delegate void xcbff87a07b81e464(Control control, object component, VisualTip tip, Rectangle toolArea, VisualTipDisplayOptions options);

	private class xef344b8a143f22e8 : IVisualTipExtender
	{
		public object GetChildAtPoint(Control control, int x, int y)
		{
			ToolBar toolBar = control as ToolBar;
			foreach (ToolBarButton button in toolBar.Buttons)
			{
				if (button.Rectangle.Contains(x, y))
				{
					return button;
				}
			}
			return null;
		}

		public object GetParent(object component)
		{
			return (component as ToolBarButton).Parent;
		}

		public Type[] GetChildTypes()
		{
			return new Type[1] { typeof(ToolBarButton) };
		}

		public Rectangle GetBounds(object component)
		{
			return (component as ToolBarButton).Rectangle;
		}
	}

	private class xc08e0eade726123a : IVisualTipExtender
	{
		private const int x6fde3ff166b3799d = 1034;

		public object GetChildAtPoint(Control control, int x, int y)
		{
			StatusBar statusBar = control as StatusBar;
			Size border3DSize = SystemInformation.Border3DSize;
			Rectangle rectangle = new Rectangle(border3DSize.Width, border3DSize.Height, 0, statusBar.Height - border3DSize.Height * 2);
			foreach (StatusBarPanel panel in statusBar.Panels)
			{
				rectangle.Width = panel.Width;
				if (rectangle.Contains(x, y))
				{
					return panel;
				}
				rectangle.X += rectangle.Width + border3DSize.Width;
			}
			return null;
		}

		public object GetParent(object component)
		{
			return (component as StatusBarPanel).Parent;
		}

		public Type[] GetChildTypes()
		{
			return new Type[1] { typeof(StatusBarPanel) };
		}

		[DllImport("user32")]
		private static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, int[] lParam);

		public Rectangle GetBounds(object component)
		{
			StatusBar parent = (component as StatusBarPanel).Parent;
			int[] array = new int[4];
			SendMessage(parent.Handle, 1034, new IntPtr(parent.Panels.IndexOf(component as StatusBarPanel)), array);
			return Rectangle.FromLTRB(array[0], array[1], array[2], array[3]);
		}
	}

	private class xec82ac93b9fc5db2 : IVisualTipExtender
	{
		public object GetChildAtPoint(Control control, int x, int y)
		{
			return control.GetType().GetMethod("GetItemAt", new Type[2]
			{
				typeof(int),
				typeof(int)
			}).Invoke(control, new object[2] { x, y });
		}

		public object GetParent(object component)
		{
			return component.GetType().GetMethod("GetCurrentParent", Type.EmptyTypes).Invoke(component, null);
		}

		public Type[] GetChildTypes()
		{
			return new Type[1] { x2a4816fc89870133.x8b354663120944da };
		}

		public Rectangle GetBounds(object component)
		{
			return (Rectangle)component.GetType().GetMethod("get_Bounds", Type.EmptyTypes).Invoke(component, null);
		}
	}

	private class x6db34da0365f631e
	{
		public static readonly Image xdc1af3a17717bf0a;

		public static readonly Image x97067fe16ec8d098;

		public static readonly Image xc29fd30d5c28606a;

		static x6db34da0365f631e()
		{
			Assembly assembly = typeof(x6db34da0365f631e).Assembly;
			Type typeFromHandle = typeof(x6db34da0365f631e);
			xdc1af3a17717bf0a = Image.FromStream(assembly.GetManifestResourceStream(typeFromHandle, "NotifyError.png"));
			x97067fe16ec8d098 = Image.FromStream(assembly.GetManifestResourceStream(typeFromHandle, "NotifyWarning.png"));
			xc29fd30d5c28606a = Image.FromStream(assembly.GetManifestResourceStream(typeFromHandle, "NotifyInformation.png"));
		}

		public static Image xb97fd8ba87079e95(VisualTipNotifyIcon x8546e7d3d8c4f973)
		{
			return x8546e7d3d8c4f973 switch
			{
				VisualTipNotifyIcon.Error => xdc1af3a17717bf0a, 
				VisualTipNotifyIcon.Warning => x97067fe16ec8d098, 
				VisualTipNotifyIcon.Information => xc29fd30d5c28606a, 
				_ => null, 
			};
		}
	}

	private const Shortcut x614024f8bdde0f2e = (Shortcut)(-1);

	private const int xc34c970702cfdd86 = 214;

	private static ArrayList xf3c98faf50ce46f9;

	private static Hashtable xee83b969c84e0761;

	private VisualTipRenderer _x3b25b5af52069364 = VisualTipRenderer.DefaultRenderer;

	private static object xfb847c48e43aa504;

	private Image _x0d63e2726b9e3054;

	private string _x13d6c070c9daa5a7;

	private string _x13a6fa85ebd01894;

	private Image _xcf43b7d9a276c2b4;

	private double _xd163a712710650fc = 0.94;

	private int _xf8d043784b9b8f56 = 256;

	private Shortcut _xf5c3a4db0a0a4de7 = (Shortcut)(-1);

	private int _xcb2e560e15c23aee = SystemInformation.DoubleClickTime * 2;

	private int _xc7ecfedb65896272 = SystemInformation.DoubleClickTime;

	private bool _xc1f02470e50f3d0a;

	private VisualTipAnimation _x1f5697535eab37b9 = VisualTipAnimation.SystemDefault;

	private VisualTipShadow _xf398ffaf32ffe055;

	private VisualTipDisplayMode _x99f58fed9c6d614d;

	private static object x3127aaed6ffdf6aa;

	private VisualTipDisplayPosition _x463e25e4283e37f4;

	private static object xf8289c94a7ae39ba;

	private bool _x592dc72d017b5b88 = true;

	private Hashtable x3945624715e622a8 = new Hashtable();

	private VisualTipEventHandler xcc479299e19f31e5;

	private VisualTipEventHandler x39d72a260279d477;

	private VisualTip _x2795ceea9a4a3d4f;

	private Control x13c2275e5d07ac49;

	private object xc16dc8a3b56cfe52;

	private static x1fc6d0e92d52d447 _xde1abe0aa61e3828;

	private x700822a7c6d92025 x49287335af20a5d6;

	private static VisualTip x8b2ce0610e2cd7cd;

	private static x700822a7c6d92025 xeee2df8a71c05223;

	private static VisualTipProvider x8293b602e4eb3197;

	public override ISite Site
	{
		get
		{
			return base.Site;
		}
		set
		{
			if (value != null && value.DesignMode)
			{
				x9f6fbdaaabb1eacf();
			}
			base.Site = value;
		}
	}

	[Category("Appearance")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("The renderer used to draw and measure a tip.")]
	public VisualTipRenderer Renderer
	{
		get
		{
			return _x3b25b5af52069364;
		}
		set
		{
			if (value == null)
			{
				value = VisualTipRenderer.DefaultRenderer;
			}
			if (_x3b25b5af52069364 != value)
			{
				_x3b25b5af52069364 = value;
				OnRendererChanged(EventArgs.Empty);
			}
		}
	}

	[Editor(typeof(IconImageEditor), typeof(UITypeEditor))]
	[Category("Appearance")]
	[DefaultValue(null)]
	[Description("The default image displayed beside the footer text on a VisualTip.")]
	[Localizable(true)]
	public Image FooterImage
	{
		get
		{
			return _x0d63e2726b9e3054;
		}
		set
		{
			if (value != null && (value.Width > 128 || value.Height > 128))
			{
				throw new InvalidOperationException("The maximum image size is 128x128.");
			}
			_x0d63e2726b9e3054 = value;
		}
	}

	[DefaultValue("")]
	[Category("Appearance")]
	[Description("The default footer text on a VisualTip.")]
	[Localizable(true)]
	public string FooterText
	{
		get
		{
			if (_x13d6c070c9daa5a7 != null)
			{
				return _x13d6c070c9daa5a7;
			}
			return "";
		}
		set
		{
			_x13d6c070c9daa5a7 = value;
		}
	}

	[Localizable(true)]
	[Category("Appearance")]
	[DefaultValue("")]
	[Description("The title message written in bold above the text when the control is disabled.")]
	public string DisabledMessage
	{
		get
		{
			if (_x13a6fa85ebd01894 != null)
			{
				return _x13a6fa85ebd01894;
			}
			return "";
		}
		set
		{
			_x13a6fa85ebd01894 = value;
		}
	}

	[Description("The image displayed beside the title of a visual tip which does not have another image assigned.")]
	[Category("Appearance")]
	[Editor(typeof(IconImageEditor), typeof(UITypeEditor))]
	[Localizable(true)]
	[DefaultValue(null)]
	public Image TitleImage
	{
		get
		{
			return _xcf43b7d9a276c2b4;
		}
		set
		{
			if (value != null && (value.Width > 128 || value.Height > 128))
			{
				throw new InvalidOperationException("The maximum image size is 128x128.");
			}
			_xcf43b7d9a276c2b4 = value;
		}
	}

	[TypeConverter(typeof(OpacityConverter))]
	[Description("The opacity level of the tip, ranging from 0% (transparent) to 100% (opaque).")]
	[Category("Appearance")]
	[DefaultValue(0.94)]
	public double Opacity
	{
		get
		{
			return _xd163a712710650fc;
		}
		set
		{
			if (value < 0.0)
			{
				value = 0.0;
			}
			else if (value > 1.0)
			{
				value = 1.0;
			}
			_xd163a712710650fc = value;
		}
	}

	[DefaultValue(256)]
	[Description("The maximum width of a VisualTip.")]
	[Category("Appearance")]
	[Localizable(true)]
	public int MaximumWidth
	{
		get
		{
			return _xf8d043784b9b8f56;
		}
		set
		{
			_xf8d043784b9b8f56 = Math.Max(value, 192);
		}
	}

	[Category("Behavior")]
	[Localizable(true)]
	[Description("The keyboard shortcut that may be pressed to raise the AccessKeyPressed event when a tip is displayed.")]
	public Shortcut AccessKey
	{
		get
		{
			if (!ShouldSerializeAccessKey())
			{
				if (DisplayMode != VisualTipDisplayMode.MouseHover)
				{
					return Shortcut.None;
				}
				return Shortcut.F1;
			}
			return _xf5c3a4db0a0a4de7;
		}
		set
		{
			_xf5c3a4db0a0a4de7 = value;
		}
	}

	[Category("Behavior")]
	[Description("The amount of time which passes, in milliseconds, before a tip is displayed.")]
	public int InitialDelay
	{
		get
		{
			return _xcb2e560e15c23aee;
		}
		set
		{
			if (_xcb2e560e15c23aee != value)
			{
				x177d5aea2b7e857a.x00101a8773a2d606(_xcb2e560e15c23aee, xae27d0e39abc839b);
				_xcb2e560e15c23aee = value;
				x177d5aea2b7e857a.x4d4b6d982caf2ebd(_xcb2e560e15c23aee, xae27d0e39abc839b);
			}
		}
	}

	[Category("Behavior")]
	[Description("The amount of time which must pass, in milliseconds, before a tip is displayed when the mouse is moved from one component to another.")]
	public int ReshowDelay
	{
		get
		{
			return _xc7ecfedb65896272;
		}
		set
		{
			_xc7ecfedb65896272 = value;
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Whether VisualTips are displayed even when the form does not have the input focus.")]
	public bool ShowAlways
	{
		get
		{
			return _xc1f02470e50f3d0a;
		}
		set
		{
			_xc1f02470e50f3d0a = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(VisualTipAnimation.SystemDefault)]
	[Description("Determines if and how a VisualTip is animated when it is displayed.")]
	public VisualTipAnimation Animation
	{
		get
		{
			return _x1f5697535eab37b9;
		}
		set
		{
			if (!Enum.IsDefined(typeof(VisualTipAnimation), value))
			{
				throw new InvalidEnumArgumentException("Animation", (int)value, typeof(VisualTipAnimation));
			}
			_x1f5697535eab37b9 = value;
		}
	}

	[Description("Determines whether a VisualTip has a shadow.")]
	[DefaultValue(VisualTipShadow.SystemDefault)]
	[Category("Appearance")]
	public VisualTipShadow Shadow
	{
		get
		{
			return _xf398ffaf32ffe055;
		}
		set
		{
			if (!Enum.IsDefined(typeof(VisualTipShadow), value))
			{
				throw new InvalidEnumArgumentException("Shadow", (int)value, typeof(VisualTipShadow));
			}
			_xf398ffaf32ffe055 = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(VisualTipDisplayMode.MouseHover)]
	[Description("Determines when a visual tip is displayed.")]
	public VisualTipDisplayMode DisplayMode
	{
		get
		{
			return _x99f58fed9c6d614d;
		}
		set
		{
			if (!Enum.IsDefined(typeof(VisualTipDisplayMode), value))
			{
				throw new InvalidEnumArgumentException("DisplayMode", (int)value, typeof(VisualTipDisplayMode));
			}
			if (_x99f58fed9c6d614d == value)
			{
				return;
			}
			_x99f58fed9c6d614d = value;
			foreach (VisualTip value2 in x3945624715e622a8.Values)
			{
				value2?.xba94d50eb5e7aca1(EventArgs.Empty);
			}
			OnDisplayModeChanged(EventArgs.Empty);
		}
	}

	[DefaultValue(VisualTipDisplayPosition.Bottom)]
	[Category("Behavior")]
	[Description("Determines where a visual tip is displayed in relation to the tool area.")]
	public VisualTipDisplayPosition DisplayPosition
	{
		get
		{
			return _x463e25e4283e37f4;
		}
		set
		{
			if (!Enum.IsDefined(typeof(VisualTipDisplayPosition), value))
			{
				throw new InvalidEnumArgumentException("DisplayPosition", (int)value, typeof(VisualTipDisplayPosition));
			}
			if (_x463e25e4283e37f4 != value)
			{
				_x463e25e4283e37f4 = value;
				OnDisplayPositionChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	[Description("Whether VisualTips are displayed at the mouse position.")]
	[Category("Behavior")]
	public bool DisplayAtMousePosition
	{
		get
		{
			return _x592dc72d017b5b88;
		}
		set
		{
			_x592dc72d017b5b88 = value;
		}
	}

	private VisualTip x2795ceea9a4a3d4f => _x2795ceea9a4a3d4f;

	internal static x1fc6d0e92d52d447 xde1abe0aa61e3828 => _xde1abe0aa61e3828;

	[Browsable(false)]
	public bool IsTipDisplayed
	{
		get
		{
			if (x49287335af20a5d6 != null)
			{
				return x49287335af20a5d6.x7965faace5d79aa4;
			}
			return false;
		}
	}

	public static VisualTip TrackedTip
	{
		get
		{
			if (x177d5aea2b7e857a.xce630b15ef968765 != null)
			{
				return x177d5aea2b7e857a.xce630b15ef968765.x2795ceea9a4a3d4f;
			}
			return null;
		}
	}

	internal static Control x0cb6bb318112e288
	{
		get
		{
			if (x177d5aea2b7e857a.xce630b15ef968765 != null)
			{
				return x177d5aea2b7e857a.xce630b15ef968765.x13c2275e5d07ac49;
			}
			return null;
		}
	}

	[Category("Property Changed")]
	[Description("Occurs when the value of the Renderer property is changed.")]
	public event EventHandler RendererChanged
	{
		add
		{
			base.Events.AddHandler(xfb847c48e43aa504, value);
		}
		remove
		{
			base.Events.RemoveHandler(xfb847c48e43aa504, value);
		}
	}

	[Category("Property Changed")]
	[Description("Occurs when the value of the DisplayMode property is changed.")]
	public event EventHandler DisplayModeChanged
	{
		add
		{
			base.Events.AddHandler(x3127aaed6ffdf6aa, value);
		}
		remove
		{
			base.Events.RemoveHandler(x3127aaed6ffdf6aa, value);
		}
	}

	[Description("Occurs when the value of the DisplayPosition property is changed.")]
	[Category("Property Changed")]
	public event EventHandler DisplayPositionChanged
	{
		add
		{
			base.Events.AddHandler(xf8289c94a7ae39ba, value);
		}
		remove
		{
			base.Events.RemoveHandler(xf8289c94a7ae39ba, value);
		}
	}

	[Description("Occurs when the access key is pressed for a VisualTip.")]
	[Category("Key")]
	public event VisualTipEventHandler AccessKeyPressed
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			xcc479299e19f31e5 = (VisualTipEventHandler)Delegate.Combine(xcc479299e19f31e5, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			xcc479299e19f31e5 = (VisualTipEventHandler)Delegate.Remove(xcc479299e19f31e5, value);
		}
	}

	[Description("Occurs before a VisualTip is displayed.")]
	public event VisualTipEventHandler TipPopup
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			x39d72a260279d477 = (VisualTipEventHandler)Delegate.Combine(x39d72a260279d477, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			x39d72a260279d477 = (VisualTipEventHandler)Delegate.Remove(x39d72a260279d477, value);
		}
	}

	public VisualTipProvider()
	{
		x177d5aea2b7e857a.x4d4b6d982caf2ebd(InitialDelay, xae27d0e39abc839b);
		xf3c98faf50ce46f9.Add(this);
		x177d5aea2b7e857a.x3178dfe58af38a3e();
	}

	public VisualTipProvider(IContainer container)
		: this()
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		container.Add(this);
	}

	private void x9f6fbdaaabb1eacf()
	{
		if (xf3c98faf50ce46f9.Contains(this))
		{
			xf3c98faf50ce46f9.Remove(this);
			if (xf3c98faf50ce46f9.Count == 0)
			{
				x177d5aea2b7e857a.x164d925049163070();
			}
		}
	}

	static VisualTipProvider()
	{
		xf3c98faf50ce46f9 = new ArrayList();
		xee83b969c84e0761 = new Hashtable();
		xfb847c48e43aa504 = new object();
		x3127aaed6ffdf6aa = new object();
		xf8289c94a7ae39ba = new object();
		_xde1abe0aa61e3828 = new x1fc6d0e92d52d447();
		SetExtender(typeof(ToolBar), new xef344b8a143f22e8());
		SetExtender(typeof(StatusBar), new xc08e0eade726123a());
		if (Environment.Version.Major >= 2)
		{
			SetExtender(x2a4816fc89870133.xdc0071b1d90f9481, new xec82ac93b9fc5db2());
		}
	}

	public static void SetExtender(Type controlType, IVisualTipExtender extender)
	{
		if ((object)controlType == null)
		{
			throw new ArgumentNullException("controlType");
		}
		if (extender == null)
		{
			throw new ArgumentNullException("extender");
		}
		xee83b969c84e0761[controlType] = extender;
		Type[] childTypes = extender.GetChildTypes();
		if (childTypes != null)
		{
			Type[] array = childTypes;
			foreach (Type key in array)
			{
				xee83b969c84e0761[key] = extender;
			}
		}
	}

	[Description]
	private static void SetExtender(Type controlType, object extender)
	{
		if ((object)controlType == null)
		{
			throw new ArgumentNullException("controlType");
		}
		if (extender == null)
		{
			throw new ArgumentNullException("extender");
		}
		if (!x9249ca59576005f1.x461c3bf969128260(extender))
		{
			throw new ArgumentException("The object did not provide the required methods.  Extenders must have 2 methods withthe following signatures:\r\n\r\nObject GetChildAtPoint(Control,Int32,Int32)\r\nObject GetParent(Object)", "extender");
		}
		SetExtender(controlType, new x9249ca59576005f1(extender));
	}

	internal static IVisualTipExtender x57758263f8154cb2(Type x8797caad1be6f743)
	{
		IVisualTipExtender visualTipExtender = xee83b969c84e0761[x8797caad1be6f743] as IVisualTipExtender;
		if (visualTipExtender == null)
		{
			IDictionaryEnumerator enumerator = xee83b969c84e0761.GetEnumerator();
			while (enumerator.MoveNext())
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)enumerator.Current;
				if (x8797caad1be6f743.IsSubclassOf(dictionaryEntry.Key as Type))
				{
					return dictionaryEntry.Value as IVisualTipExtender;
				}
			}
		}
		return visualTipExtender;
	}

	private static object xaaee85ae186b24b6(Control x43bec302f92080b9, Point x2f7096dac971d6ec)
	{
		object result = x57758263f8154cb2(x43bec302f92080b9.GetType())?.GetChildAtPoint(x43bec302f92080b9, x2f7096dac971d6ec.X, x2f7096dac971d6ec.Y);
		Control control = x7c5d7045924a19c1(x43bec302f92080b9, x43bec302f92080b9.PointToScreen(x2f7096dac971d6ec));
		if (control != x43bec302f92080b9)
		{
			return control;
		}
		return result;
	}

	private static Control x7c5d7045924a19c1(Control x43bec302f92080b9, Point x8a6d69cf001869f5)
	{
		Control childAtPoint = x43bec302f92080b9.GetChildAtPoint(x43bec302f92080b9.PointToClient(x8a6d69cf001869f5));
		if (childAtPoint != null)
		{
			return x7c5d7045924a19c1(childAtPoint, x8a6d69cf001869f5);
		}
		return x43bec302f92080b9;
	}

	private Rectangle xfec1b0c2e2b64a23(Control x43bec302f92080b9, object x7f976b7a7a87b378)
	{
		Rectangle rectangle = Rectangle.Empty;
		if (!DisplayAtMousePosition)
		{
			if (x7f976b7a7a87b378 == null)
			{
				if (!(x43bec302f92080b9 is Form))
				{
					return x43bec302f92080b9.Parent.RectangleToScreen(x43bec302f92080b9.Bounds);
				}
				rectangle = Rectangle.Empty;
			}
			else
			{
				rectangle = x57758263f8154cb2(x43bec302f92080b9.GetType())?.GetBounds(x7f976b7a7a87b378) ?? Rectangle.Empty;
			}
		}
		if (!(rectangle == Rectangle.Empty))
		{
			return x43bec302f92080b9.RectangleToScreen(rectangle);
		}
		return new Rectangle(Cursor.Position, new Size(16, 16));
	}

	protected override void Dispose(bool disposing)
	{
		x177d5aea2b7e857a.x00101a8773a2d606(InitialDelay, xae27d0e39abc839b);
		x9f6fbdaaabb1eacf();
		base.Dispose(disposing);
	}

	private bool ShouldSerializeRenderer()
	{
		return _x3b25b5af52069364 != VisualTipRenderer.DefaultRenderer;
	}

	private void ResetRenderer()
	{
		Renderer = VisualTipRenderer.DefaultRenderer;
	}

	protected virtual void OnRendererChanged(EventArgs e)
	{
		if ((EventHandler)base.Events[xfb847c48e43aa504] != null)
		{
			((EventHandler)base.Events[xfb847c48e43aa504])(this, e);
		}
	}

	private bool ShouldSerializeAccessKey()
	{
		return _xf5c3a4db0a0a4de7 != (Shortcut)(-1);
	}

	private void ResetAccessKey()
	{
		_xf5c3a4db0a0a4de7 = (Shortcut)(-1);
	}

	private bool ShouldSerializeInitialDelay()
	{
		return _xcb2e560e15c23aee != SystemInformation.DoubleClickTime * 2;
	}

	private void ResetInitialDelay()
	{
		_xcb2e560e15c23aee = SystemInformation.DoubleClickTime * 2;
	}

	private bool ShouldSerializeReshowDelay()
	{
		return _xc7ecfedb65896272 != SystemInformation.DoubleClickTime;
	}

	private void ResetReshowDelay()
	{
		_xc7ecfedb65896272 = SystemInformation.DoubleClickTime;
	}

	protected virtual void OnDisplayModeChanged(EventArgs e)
	{
		if ((EventHandler)base.Events[x3127aaed6ffdf6aa] != null)
		{
			((EventHandler)base.Events[x3127aaed6ffdf6aa])(this, e);
		}
	}

	protected virtual void OnDisplayPositionChanged(EventArgs e)
	{
		if ((EventHandler)base.Events[xf8289c94a7ae39ba] != null)
		{
			((EventHandler)base.Events[xf8289c94a7ae39ba])(this, e);
		}
	}

	public bool CanExtend(object extendee)
	{
		if (extendee == null)
		{
			return false;
		}
		Type type = extendee.GetType();
		if (type.IsSubclassOf(typeof(Control)))
		{
			return true;
		}
		if (typeof(ToolBarButton).IsAssignableFrom(type))
		{
			return true;
		}
		if (typeof(StatusBarPanel).IsAssignableFrom(type))
		{
			return true;
		}
		if ((object)type.GetEvent("MouseEnter") == null || (object)type.GetEvent("MouseLeave") == null)
		{
			return (object)type.GetField("IsVisualTipComponent", BindingFlags.Static | BindingFlags.NonPublic) != null;
		}
		return true;
	}

	[Description("The VisualTip displayed when the mouse hovers over the current component.")]
	[MergableProperty(false)]
	public VisualTip GetVisualTip(object component)
	{
		VisualTip visualTip = x3945624715e622a8[component] as VisualTip;
		if (visualTip == null)
		{
			visualTip = (VisualTip)(x3945624715e622a8[component] = new VisualTip());
			visualTip.x732ebd2ce06fed00(this);
			xdb843083498b7d19(visualTip, component);
		}
		return visualTip;
	}

	public void SetVisualTip(object component, VisualTip visualTip)
	{
		if (x3945624715e622a8[component] is VisualTip visualTip2)
		{
			visualTip2.x732ebd2ce06fed00(null);
		}
		if (visualTip != null && visualTip.Provider != null)
		{
			visualTip.Provider.SetVisualTip(component, null);
		}
		x3945624715e622a8[component] = visualTip;
		if (visualTip != null)
		{
			visualTip.x732ebd2ce06fed00(this);
			xdb843083498b7d19(visualTip, component);
		}
	}

	private static void xdb843083498b7d19(VisualTip x2636ebb6bd343dac, object x7f976b7a7a87b378)
	{
		if (x7f976b7a7a87b378 == null)
		{
			x2636ebb6bd343dac.x55fae544baab0cc8(null, null);
			return;
		}
		if (x7f976b7a7a87b378 is Control)
		{
			x2636ebb6bd343dac.x55fae544baab0cc8(x7f976b7a7a87b378 as Control, null);
			return;
		}
		IVisualTipExtender visualTipExtender = x57758263f8154cb2(x7f976b7a7a87b378.GetType());
		if (visualTipExtender != null)
		{
			x2636ebb6bd343dac.x55fae544baab0cc8(visualTipExtender.GetParent(x7f976b7a7a87b378) as Control, x7f976b7a7a87b378);
		}
		else
		{
			x2636ebb6bd343dac.x55fae544baab0cc8(null, x7f976b7a7a87b378);
		}
	}

	private bool ShouldSerializeVisualTip(object component)
	{
		if (!x3945624715e622a8.Contains(component))
		{
			return false;
		}
		return GetVisualTip(component).xe5a18e5ab4fed2d8();
	}

	private void ResetVisualTip(object component)
	{
		if (x3945624715e622a8.Contains(component))
		{
			GetVisualTip(component).x74f5a1ef3906e23c();
		}
	}

	private VisualTip xed132d31bbad8578(Control x43bec302f92080b9, object x7f976b7a7a87b378)
	{
		VisualTip visualTip = null;
		if (x7f976b7a7a87b378 != null)
		{
			visualTip = x3945624715e622a8[x7f976b7a7a87b378] as VisualTip;
		}
		if (visualTip == null)
		{
			visualTip = x3945624715e622a8[x43bec302f92080b9] as VisualTip;
		}
		if (visualTip != null && !visualTip.Active)
		{
			visualTip = null;
		}
		return visualTip;
	}

	internal void xae27d0e39abc839b(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		if (DisplayMode != VisualTipDisplayMode.MouseHover || x2795ceea9a4a3d4f != null || (x177d5aea2b7e857a.xce630b15ef968765 != null && x177d5aea2b7e857a.xce630b15ef968765 != this))
		{
			return;
		}
		Control x2ae3d24e2dae102a = x177d5aea2b7e857a.x2ae3d24e2dae102a;
		if (x2ae3d24e2dae102a == null || x2ae3d24e2dae102a.IsDisposed)
		{
			return;
		}
		Form activeForm = Form.ActiveForm;
		if (activeForm == null)
		{
			return;
		}
		if (!ShowAlways)
		{
			Form form = x2ae3d24e2dae102a.FindForm();
			if (form != null && form != activeForm && (!form.IsMdiChild || form.MdiParent != activeForm || form.MdiParent.ActiveMdiChild != form))
			{
				return;
			}
		}
		object x7f976b7a7a87b = xaaee85ae186b24b6(x2ae3d24e2dae102a, x2ae3d24e2dae102a.PointToClient(Cursor.Position));
		VisualTip visualTip = xed132d31bbad8578(x2ae3d24e2dae102a, x7f976b7a7a87b);
		if (visualTip != null)
		{
			if (visualTip == x8b2ce0610e2cd7cd)
			{
				x177d5aea2b7e857a.xce630b15ef968765 = this;
			}
			else if (visualTip != x2795ceea9a4a3d4f)
			{
				x1a1e5ed525755afa(x2ae3d24e2dae102a, x7f976b7a7a87b, visualTip, xfec1b0c2e2b64a23(x2ae3d24e2dae102a, x7f976b7a7a87b), (VisualTipDisplayOptions)((VisualTipDisplayPosition)0x48 | visualTip.DisplayPosition));
			}
		}
	}

	internal void x59157bfb8127f817(MouseEventArgs xfbf34718e704c6bc)
	{
		Control x2ae3d24e2dae102a = x177d5aea2b7e857a.x2ae3d24e2dae102a;
		if (x2ae3d24e2dae102a != null)
		{
			VisualTip visualTip = xed132d31bbad8578(x2ae3d24e2dae102a, xaaee85ae186b24b6(x2ae3d24e2dae102a, new Point(xfbf34718e704c6bc.X, xfbf34718e704c6bc.Y)));
			if (x8b2ce0610e2cd7cd != null && visualTip != x8b2ce0610e2cd7cd)
			{
				x8b2ce0610e2cd7cd = null;
			}
			else if (visualTip != x2795ceea9a4a3d4f)
			{
				x177d5aea2b7e857a.x5fe94b51388fcd2b(ReshowDelay, xae27d0e39abc839b);
				HideTrackedTip();
			}
		}
	}

	internal void x26745f26d8f8ce5c()
	{
		if ((xeee2df8a71c05223.x73979cef1002ed01 & VisualTipDisplayOptions.HideOnMouseDown) != VisualTipDisplayOptions.Default)
		{
			if (TrackedTip != null)
			{
				x8b2ce0610e2cd7cd = TrackedTip;
			}
			HideTrackedTip();
		}
	}

	internal void x83e9335166b9fd89(EventArgs xfbf34718e704c6bc)
	{
		if ((xeee2df8a71c05223.x73979cef1002ed01 & VisualTipDisplayOptions.HideOnMouseDown) != VisualTipDisplayOptions.Default)
		{
			if (xeee2df8a71c05223.Bounds.Contains(Cursor.Position))
			{
				x8b2ce0610e2cd7cd = x2795ceea9a4a3d4f;
			}
			else
			{
				x8b2ce0610e2cd7cd = null;
				x177d5aea2b7e857a.x5fe94b51388fcd2b(ReshowDelay, xae27d0e39abc839b);
			}
			HideTrackedTip();
		}
	}

	internal void x0808834e03f64d3d(KeyEventArgs xfbf34718e704c6bc)
	{
		if (x2795ceea9a4a3d4f != null)
		{
			if (xfbf34718e704c6bc.KeyCode == Keys.Escape)
			{
				x8b2ce0610e2cd7cd = x2795ceea9a4a3d4f;
				HideTrackedTip();
				xfbf34718e704c6bc.Handled = true;
			}
			else if (xfbf34718e704c6bc.KeyCode == (Keys)x2795ceea9a4a3d4f.AccessKey)
			{
				VisualTipEventArgs e = new VisualTipEventArgs(x2795ceea9a4a3d4f, (xc16dc8a3b56cfe52 != null) ? xc16dc8a3b56cfe52 : x13c2275e5d07ac49);
				x8b2ce0610e2cd7cd = x2795ceea9a4a3d4f;
				HideTrackedTip();
				xfbf34718e704c6bc.Handled = true;
				OnAccessKeyPressed(e);
			}
		}
	}

	public void ShowTip(Control control, VisualTipDisplayOptions options)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		ShowTip(control, options, (control.Parent == null) ? control.Bounds : control.Parent.RectangleToScreen(control.Bounds));
	}

	public void ShowTip(Control control, VisualTipDisplayOptions options, Rectangle exclude)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		VisualTip visualTip = xed132d31bbad8578(control, null);
		if (visualTip != null)
		{
			ShowTip(visualTip, exclude, control, options);
		}
	}

	public void ShowTip(VisualTip tip, Rectangle exclude)
	{
		ShowTip(tip, exclude, null, (VisualTipDisplayOptions)tip.DisplayPosition);
	}

	public void ShowTip(VisualTip tip, Rectangle exclude, Control sourceControl, VisualTipDisplayOptions options)
	{
		if (tip == null)
		{
			throw new ArgumentNullException("tip", "The tip parameter may not be null.");
		}
		if (sourceControl != null && !sourceControl.IsDisposed)
		{
			if ((options & VisualTipDisplayOptions.HideOnKeyDown) == VisualTipDisplayOptions.HideOnKeyDown)
			{
				sourceControl.KeyDown += xb69a4c63d6c37eb9;
			}
			if ((options & VisualTipDisplayOptions.HideOnKeyPress) == VisualTipDisplayOptions.HideOnKeyPress)
			{
				sourceControl.KeyPress += xcf0827538fd7b0d1;
			}
			if ((options & VisualTipDisplayOptions.HideOnLostFocus) == VisualTipDisplayOptions.HideOnLostFocus)
			{
				sourceControl.LostFocus += x11ecff03e41b24ee;
			}
			if ((options & VisualTipDisplayOptions.HideOnMouseDown) == VisualTipDisplayOptions.HideOnMouseDown)
			{
				sourceControl.MouseDown += x86a2736e636bb1da;
			}
			if ((options & VisualTipDisplayOptions.HideOnTextChanged) == VisualTipDisplayOptions.HideOnTextChanged)
			{
				sourceControl.TextChanged += xdaadb6a1a420e544;
			}
			sourceControl.HandleDestroyed += x0aae954e65aa66f3;
			Form form = sourceControl.FindForm();
			if (form != null)
			{
				form.Deactivate += x3442d231cb8379ce;
			}
		}
		x1a1e5ed525755afa(sourceControl, null, tip, exclude, options);
	}

	private void xb69a4c63d6c37eb9(object xe0292b9ed559da7d, KeyEventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void xcf0827538fd7b0d1(object xe0292b9ed559da7d, KeyPressEventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void x11ecff03e41b24ee(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void x86a2736e636bb1da(object xe0292b9ed559da7d, MouseEventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void x0aae954e65aa66f3(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void xdaadb6a1a420e544(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		x8bac11c8fc93d841(xe0292b9ed559da7d as Control);
	}

	private void x3442d231cb8379ce(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		if (x13c2275e5d07ac49 != null)
		{
			x8bac11c8fc93d841(x13c2275e5d07ac49);
		}
	}

	private void x8bac11c8fc93d841(Control x43bec302f92080b9)
	{
		x43bec302f92080b9.MouseDown -= x86a2736e636bb1da;
		x43bec302f92080b9.LostFocus -= x11ecff03e41b24ee;
		x43bec302f92080b9.KeyDown -= xb69a4c63d6c37eb9;
		x43bec302f92080b9.KeyPress -= xcf0827538fd7b0d1;
		x43bec302f92080b9.HandleDestroyed -= x0aae954e65aa66f3;
		Form form = x43bec302f92080b9.FindForm();
		if (form != null)
		{
			form.Deactivate -= x3442d231cb8379ce;
		}
		if (x13c2275e5d07ac49 == x43bec302f92080b9)
		{
			HideTip();
		}
	}

	protected virtual void OnAccessKeyPressed(VisualTipEventArgs e)
	{
		if (xcc479299e19f31e5 != null)
		{
			xcc479299e19f31e5(this, e);
		}
	}

	protected virtual void OnTipPopup(VisualTipEventArgs e)
	{
		if (x39d72a260279d477 != null)
		{
			x39d72a260279d477(this, e);
		}
	}

	private void x1a1e5ed525755afa(Control x43bec302f92080b9, object x7f976b7a7a87b378, VisualTip x2636ebb6bd343dac, Rectangle xd19889359ad9e56f, VisualTipDisplayOptions xdfde339da46db651)
	{
		if (x43bec302f92080b9 != null && x43bec302f92080b9.InvokeRequired)
		{
			x43bec302f92080b9.BeginInvoke(new xcbff87a07b81e464(x1a1e5ed525755afa), x43bec302f92080b9, x7f976b7a7a87b378, x2636ebb6bd343dac, xd19889359ad9e56f, xdfde339da46db651);
			return;
		}
		bool flag = (xdfde339da46db651 & VisualTipDisplayOptions.HideOnMouseLeave) == VisualTipDisplayOptions.HideOnMouseLeave;
		x700822a7c6d92025 x700822a7c6d92026 = (flag ? xeee2df8a71c05223 : x49287335af20a5d6);
		if (x700822a7c6d92026 == null)
		{
			x700822a7c6d92026 = ((!flag) ? (x49287335af20a5d6 = new x700822a7c6d92025()) : (xeee2df8a71c05223 = new x700822a7c6d92025()));
		}
		if (x700822a7c6d92026.x8c3cc20aa74dd99c != x2636ebb6bd343dac)
		{
			x700822a7c6d92026.xd7692f35479850d5();
			VisualTipEventArgs e = new VisualTipEventArgs(x2636ebb6bd343dac, (x7f976b7a7a87b378 == null) ? x43bec302f92080b9 : x7f976b7a7a87b378);
			x2636ebb6bd343dac.x732ebd2ce06fed00(this);
			xdb843083498b7d19(x2636ebb6bd343dac, e.Instance);
			OnTipPopup(e);
			if (e.Cancel)
			{
				x8b2ce0610e2cd7cd = x2636ebb6bd343dac;
				return;
			}
			_x2795ceea9a4a3d4f = x2636ebb6bd343dac;
			x13c2275e5d07ac49 = x43bec302f92080b9;
			xc16dc8a3b56cfe52 = x7f976b7a7a87b378;
			if (flag)
			{
				x177d5aea2b7e857a.xce630b15ef968765 = this;
			}
			xde1abe0aa61e3828.xd6b6ed77479ef68c(x700822a7c6d92026, x43bec302f92080b9);
			x700822a7c6d92026.x2c6cfc461f9dd5e2(this, x2636ebb6bd343dac, xd19889359ad9e56f, xdfde339da46db651);
		}
		else
		{
			x700822a7c6d92026.x4503f6152d2bb863(xd19889359ad9e56f, xdfde339da46db651);
		}
	}

	public void HideTip()
	{
		if (x49287335af20a5d6 != null)
		{
			x49287335af20a5d6.xd7692f35479850d5();
		}
	}

	internal void xc590aa98ef979eff(x700822a7c6d92025 x76b3d9d2638e5ecd)
	{
		if (x76b3d9d2638e5ecd == xeee2df8a71c05223 && x177d5aea2b7e857a.xce630b15ef968765 == this)
		{
			x177d5aea2b7e857a.xce630b15ef968765 = null;
		}
		_x2795ceea9a4a3d4f = null;
		x13c2275e5d07ac49 = null;
		xc16dc8a3b56cfe52 = null;
		xde1abe0aa61e3828.x52b190e626f65140(x76b3d9d2638e5ecd);
	}

	public static void HideTrackedTip()
	{
		if (xeee2df8a71c05223 != null)
		{
			xeee2df8a71c05223.xd7692f35479850d5();
		}
	}

	[DllImport("user32", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

	public static void ShowNotifyTip(Control control, string text, string title)
	{
		ShowNotifyTip(control, text, title, VisualTipNotifyIcon.None, VisualTipDisplayOptions.Default);
	}

	public static void ShowNotifyTip(Control control, string text, string title, VisualTipNotifyIcon icon)
	{
		ShowNotifyTip(control, text, title, icon, VisualTipDisplayOptions.Default);
	}

	public static void ShowNotifyTip(Control control, string text, string title, VisualTipNotifyIcon icon, Rectangle exclude)
	{
		ShowNotifyTip(control, text, title, icon, VisualTipDisplayOptions.Default, exclude);
	}

	public static void ShowNotifyTip(Control control, string text, string title, VisualTipNotifyIcon icon, VisualTipDisplayOptions options)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		ShowNotifyTip(control, text, title, icon, options, (control.Parent == null) ? control.Bounds : control.Parent.RectangleToScreen(control.Bounds));
	}

	public static void ShowNotifyTip(TextBox textBox, string text, string title, VisualTipNotifyIcon icon, int charIndex)
	{
		if (textBox == null || textBox.IsDisposed)
		{
			throw new ArgumentNullException("textBox", "textBox may not be null or disposed.");
		}
		if (charIndex < 0 || charIndex > textBox.TextLength)
		{
			throw new ArgumentNullException("charIndex", "charIndex may not be less than zero or greater than the length of the TextBox text.");
		}
		if (charIndex == textBox.TextLength)
		{
			charIndex = Math.Max(charIndex - 1, 0);
		}
		int dw = SendMessage(textBox.Handle, 214, new IntPtr(Math.Max(charIndex, 0)), IntPtr.Zero).ToInt32();
		Point location = textBox.PointToScreen(new Point(dw));
		Rectangle exclude = new Rectangle(location, new Size(1, textBox.Font.Height));
		ShowNotifyTip(textBox, text, title, icon, VisualTipDisplayOptions.HideOnKeyPress | VisualTipDisplayOptions.HideOnLostFocus, exclude);
	}

	public static void ShowNotifyTip(Control control, string text, string title, VisualTipNotifyIcon icon, VisualTipDisplayOptions options, Rectangle exclude)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		if (control.IsDisposed)
		{
			throw new ArgumentOutOfRangeException("control", control, "A tip may not be displayed for a control which has already been disposed.");
		}
		if (x8293b602e4eb3197 == null)
		{
			x8293b602e4eb3197 = new VisualTipProvider();
			x8293b602e4eb3197.Renderer = new VisualTipBalloonRenderer();
		}
		exclude.X -= 12;
		VisualTip visualTip = new VisualTip(text, title);
		visualTip.TitleImage = x6db34da0365f631e.xb97fd8ba87079e95(icon);
		x8293b602e4eb3197.ShowTip(visualTip, exclude, control, options);
	}
}
