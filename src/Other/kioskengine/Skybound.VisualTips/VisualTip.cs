using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Skybound.ComponentModel;
using Skybound.Drawing.Design;
using xac548eac1a95799f;

namespace Skybound.VisualTips;

[TypeConverter(typeof(VisualTipConverter))]
[Editor(typeof(VisualTipEditor), typeof(UITypeEditor))]
public class VisualTip : x6e74390626fc219b, ICustomTypeDescriptor
{
	private VisualTipProvider _xb6fac368c5962c3d;

	private Control x13c2275e5d07ac49;

	private object xc16dc8a3b56cfe52;

	private bool xd0e2e2d048bf8825;

	private Rectangle _xb0ac167847d695cc;

	private xa457962045d12902 x4e35c79189b28e26 = new xa457962045d12902();

	private bool _x1ee21b7088908641;

	[Browsable(false)]
	public VisualTipProvider Provider => _xb6fac368c5962c3d;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	private bool x1ee21b7088908641
	{
		get
		{
			return _x1ee21b7088908641;
		}
		set
		{
			_x1ee21b7088908641 = value;
		}
	}

	private bool x3d61f462e31dafa7 => x66fd0cb9f9adfc30(Provider) == CultureInfo.InvariantCulture;

	[AmbientValue(null)]
	[Description("The image displayed beside the title.")]
	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	[Editor(typeof(IconImageEditor), typeof(UITypeEditor))]
	public Image TitleImage
	{
		get
		{
			return (Image)x4e35c79189b28e26.x3f88a25febd23896("TitleImage", (Provider == null) ? null : Provider.TitleImage);
		}
		set
		{
			if (value != null && (value.Width > 128 || value.Height > 128))
			{
				throw new InvalidOperationException("The maximum image size is 128x128.");
			}
			x4e35c79189b28e26.xc7b6e59bfbbe9301("TitleImage", value, (Provider == null) ? null : Provider.TitleImage);
		}
	}

	[Localizable(true)]
	[Description("The title displayed in bold at the top of the tip.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	public string Title
	{
		get
		{
			return (string)x4e35c79189b28e26.x3f88a25febd23896("Title", "");
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Title", (value == null) ? "" : value, "");
		}
	}

	[Localizable(true)]
	[Description("The shortcut key displayed beside the title, if it is not Shortcut.None.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	public Shortcut Shortcut
	{
		get
		{
			return (Shortcut)x4e35c79189b28e26.x3f88a25febd23896("Shortcut", Shortcut.None);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Shortcut", value, Shortcut.None);
		}
	}

	[RefreshProperties(RefreshProperties.Repaint)]
	[Localizable(true)]
	[PropertyListValue]
	[Description("The text displayed on the tooltip.")]
	public string Text
	{
		get
		{
			return (string)x4e35c79189b28e26.x3f88a25febd23896("Text", "");
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Text", (value == null) ? "" : value, "");
		}
	}

	[Editor(typeof(IconImageEditor), typeof(UITypeEditor))]
	[Localizable(true)]
	[PropertyListValue]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("The image displayed beside the text.")]
	public Image Image
	{
		get
		{
			return (Image)x4e35c79189b28e26.x3f88a25febd23896("Image", null);
		}
		set
		{
			if (value != null && (value.Width > 128 || value.Height > 128))
			{
				throw new InvalidOperationException("The maximum image size is 128x128.");
			}
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Image", value, null);
		}
	}

	[AmbientValue(null)]
	[PropertyListValue]
	[Description("The message displayed above the text when the tool is disabled.")]
	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	public string DisabledMessage
	{
		get
		{
			return (string)x4e35c79189b28e26.x3f88a25febd23896("DisabledMessage", (Provider == null) ? "" : Provider.DisabledMessage);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("DisabledMessage", (value == null) ? "" : value, (Provider == null) ? "" : Provider.DisabledMessage);
		}
	}

	[PropertyListValue]
	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("The alternate text displayed when the tool is disabled.  When this property is blank, the regular text is always used.")]
	[AmbientValue(null)]
	public string DisabledText
	{
		get
		{
			return (string)x4e35c79189b28e26.x3f88a25febd23896("DisabledText", "");
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("DisabledText", (value == null) ? "" : value, "");
		}
	}

	[Description("Specifies whether the footer text and image are hidden on this tip.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Localizable(true)]
	[PropertyListValue]
	public bool HideFooter
	{
		get
		{
			return (bool)x4e35c79189b28e26.x3f88a25febd23896("HideFooter", false);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("HideFooter", value, false);
		}
	}

	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	[Localizable(true)]
	[AmbientValue(null)]
	[Description("The text displayed in the footer.")]
	public string FooterText
	{
		get
		{
			return (string)x4e35c79189b28e26.x3f88a25febd23896("FooterText", (Provider == null) ? "" : Provider.FooterText);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("FooterText", (value == null) ? "" : value, (Provider == null) ? "" : Provider.FooterText);
		}
	}

	[AmbientValue(null)]
	[Editor(typeof(IconImageEditor), typeof(UITypeEditor))]
	[Localizable(true)]
	[PropertyListValue]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Description("The image displayed in the footer.")]
	public Image FooterImage
	{
		get
		{
			return (Image)x4e35c79189b28e26.x3f88a25febd23896("FooterImage", (Provider == null) ? null : Provider.FooterImage);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("FooterImage", value, (Provider == null) ? null : Provider.FooterImage);
		}
	}

	[AmbientValue(Shortcut.F1)]
	[Description("The keyboard shortcut that may be pressed to raise the AccessKeyPressed event when the tip is displayed.")]
	[PropertyListValue]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Localizable(true)]
	public Shortcut AccessKey
	{
		get
		{
			return (Shortcut)x4e35c79189b28e26.x3f88a25febd23896("AccessKey", (Provider == null) ? Shortcut.F1 : Provider.AccessKey);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("AccessKey", value, (Provider == null) ? Shortcut.F1 : Provider.AccessKey);
		}
	}

	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	[AmbientValue(256)]
	[Description("The maximum width of the tip.")]
	[PropertyListValue]
	public int MaximumWidth
	{
		get
		{
			return (int)x4e35c79189b28e26.x3f88a25febd23896("MaximumWidth", (Provider == null) ? 256 : Provider.MaximumWidth);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("MaximumWidth", Math.Max(value, 192), (Provider == null) ? 256 : Provider.MaximumWidth);
		}
	}

	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	[Description("Specifies whether the tip will be displayed.")]
	[Localizable(true)]
	public bool Active
	{
		get
		{
			return (bool)x4e35c79189b28e26.x3f88a25febd23896("Active", true);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Active", value, true);
		}
	}

	[PropertyListValue]
	[Description("Determines where a visual tip is displayed in relation to the tool area.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[AmbientValue(VisualTipDisplayPosition.Bottom)]
	public VisualTipDisplayPosition DisplayPosition
	{
		get
		{
			return (VisualTipDisplayPosition)x4e35c79189b28e26.x3f88a25febd23896("DisplayPosition", (Provider != null) ? Provider.DisplayPosition : VisualTipDisplayPosition.Bottom);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("DisplayPosition", value, -1);
		}
	}

	[Description("When this property is false, the disabled message and text and displayed on the tip.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[PropertyListValue]
	public bool Enabled
	{
		get
		{
			return (bool)x4e35c79189b28e26.x3f88a25febd23896("Enabled", x97c8edeeedaec96e);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Enabled", value, x97c8edeeedaec96e);
		}
	}

	private bool x97c8edeeedaec96e
	{
		get
		{
			if (TryGetValue("Enabled", typeof(bool), out var value))
			{
				return (bool)value;
			}
			if (x13c2275e5d07ac49 == null)
			{
				return true;
			}
			return x13c2275e5d07ac49.Enabled;
		}
	}

	[Description("The font used to display the tip text.")]
	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	[AmbientValue(null)]
	[PropertyListValue]
	public Font Font
	{
		get
		{
			return (Font)x4e35c79189b28e26.x3f88a25febd23896("Font", xbbfda0f74367649b);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("Font", (value == null) ? xbbfda0f74367649b : value, xbbfda0f74367649b);
		}
	}

	private Font xbbfda0f74367649b
	{
		get
		{
			if (TryGetValue("Font", typeof(Font), out var value))
			{
				return (Font)value;
			}
			if (x13c2275e5d07ac49 == null)
			{
				return Control.DefaultFont;
			}
			return x13c2275e5d07ac49.Font;
		}
	}

	[PropertyListValue]
	[Description("Whether the tip text is displayed using a right-to-left reading order.")]
	[Localizable(true)]
	[RefreshProperties(RefreshProperties.Repaint)]
	public RightToLeft RightToLeft
	{
		get
		{
			return (RightToLeft)x4e35c79189b28e26.x3f88a25febd23896("RightToLeft", xbf0959fbe407afb3);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("RightToLeft", value, xbf0959fbe407afb3);
		}
	}

	private RightToLeft xbf0959fbe407afb3
	{
		get
		{
			if (TryGetValue("RightToLeft", typeof(RightToLeft), out var value))
			{
				return (RightToLeft)value;
			}
			if (x13c2275e5d07ac49 == null)
			{
				return RightToLeft.No;
			}
			return x13c2275e5d07ac49.RightToLeft;
		}
	}

	public VisualTip()
	{
		x4e35c79189b28e26.x236e2b71e0b477c3 += xcc67a3fd90aab8ac;
		x4e35c79189b28e26.x0ad6cb77c00e4e89 += x8bf74c8da855801e;
	}

	public VisualTip(string text)
		: this(text, null, null, null, Shortcut.None, hideFooter: false)
	{
	}

	public VisualTip(string text, string title)
		: this(text, title, null, null, Shortcut.None, hideFooter: false)
	{
	}

	public VisualTip(string text, string title, Image image)
		: this(text, title, image, null, Shortcut.None, hideFooter: false)
	{
	}

	public VisualTip(string text, string title, Image image, string disabledText, Shortcut shortcut, bool hideFooter)
		: this()
	{
		Text = text;
		Title = title;
		Image = image;
		DisabledText = disabledText;
		Shortcut = shortcut;
		HideFooter = hideFooter;
	}

	internal void x732ebd2ce06fed00(VisualTipProvider xbcea506a33cf9111)
	{
		x55d4e90894f34da4();
		_xb6fac368c5962c3d = xbcea506a33cf9111;
		x13c2275e5d07ac49 = null;
		xc16dc8a3b56cfe52 = null;
	}

	internal void x55fae544baab0cc8(Control x43bec302f92080b9, object x7f976b7a7a87b378)
	{
		x13c2275e5d07ac49 = x43bec302f92080b9;
		xc16dc8a3b56cfe52 = x7f976b7a7a87b378;
		xb45a0f8afa82d154();
	}

	internal void xba94d50eb5e7aca1(EventArgs xfbf34718e704c6bc)
	{
		x55d4e90894f34da4();
		xb45a0f8afa82d154();
	}

	private void xb45a0f8afa82d154()
	{
		if (Provider != null && x13c2275e5d07ac49 != null && xc16dc8a3b56cfe52 == null && Provider.DisplayMode == VisualTipDisplayMode.HelpRequested)
		{
			x13c2275e5d07ac49.KeyDown += x1d079732c0a147ac;
			x13c2275e5d07ac49.HelpRequested += x68bcdbadd2ccb5c1;
			xd0e2e2d048bf8825 = true;
		}
	}

	private void x55d4e90894f34da4()
	{
		if (xd0e2e2d048bf8825)
		{
			x13c2275e5d07ac49.KeyDown -= x1d079732c0a147ac;
			x13c2275e5d07ac49.HelpRequested -= x68bcdbadd2ccb5c1;
			xd0e2e2d048bf8825 = false;
		}
	}

	private void x68bcdbadd2ccb5c1(object xe0292b9ed559da7d, HelpEventArgs xfbf34718e704c6bc)
	{
		if (Control.MouseButtons == MouseButtons.Left)
		{
			Provider.ShowTip(x13c2275e5d07ac49, VisualTipDisplayOptions.HideOnMouseLeave | VisualTipDisplayOptions.HideOnLostFocus);
		}
		xfbf34718e704c6bc.Handled = true;
	}

	private void x1d079732c0a147ac(object xe0292b9ed559da7d, KeyEventArgs xfbf34718e704c6bc)
	{
		if (xfbf34718e704c6bc.KeyData == Keys.F1)
		{
			Provider.ShowTip(x13c2275e5d07ac49, VisualTipDisplayOptions.HideOnLostFocus);
			xfbf34718e704c6bc.Handled = true;
		}
	}

	internal void x2d5b2d77afb66528(Rectangle xd19889359ad9e56f)
	{
		_xb0ac167847d695cc = xd19889359ad9e56f;
	}

	internal Rectangle xf8e45af07afc44e0()
	{
		return _xb0ac167847d695cc;
	}

	private xa457962045d12902 x48ea7d655d68f71a()
	{
		return x4e35c79189b28e26;
	}

	xa457962045d12902 x6e74390626fc219b.GetPropertyList()
	{
		//ILSpy generated this explicit interface implementation from .override directive in x48ea7d655d68f71a
		return this.x48ea7d655d68f71a();
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return ((ICustomTypeDescriptor)this).GetProperties((Attribute[])null);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(this, noCustomTypeDesc: true);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return xa457962045d12902.xfc2d0dcac70a78ed(this, attributes);
	}

	private void xcc67a3fd90aab8ac(object xe0292b9ed559da7d, PropertyChangedEventArgs xfbf34718e704c6bc)
	{
		if (!x3d61f462e31dafa7 && !x1ee21b7088908641)
		{
			throw new InvalidOperationException("New visual tips may not be created while the form is being localized. Set the Language property of the Form or UserControl to (Default) and create a tip in the default language before localizing it.");
		}
	}

	private void x8bf74c8da855801e(object xe0292b9ed559da7d, PropertyChangedEventArgs xfbf34718e704c6bc)
	{
		if (x3d61f462e31dafa7)
		{
			x1ee21b7088908641 = x4e35c79189b28e26.xe5a18e5ab4fed2d8();
		}
	}

	internal static CultureInfo x66fd0cb9f9adfc30(IComponent x7f976b7a7a87b378)
	{
		if (x7f976b7a7a87b378 != null && x7f976b7a7a87b378.Site != null && x7f976b7a7a87b378.Site.DesignMode)
		{
			IDesignerHost designerHost = (IDesignerHost)x7f976b7a7a87b378.Site.GetService(typeof(IDesignerHost));
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(designerHost.RootComponent)["Language"];
			if (propertyDescriptor != null && propertyDescriptor.PropertyType == typeof(CultureInfo))
			{
				return propertyDescriptor.GetValue(designerHost.RootComponent) as CultureInfo;
			}
		}
		return CultureInfo.InvariantCulture;
	}

	internal bool xe5a18e5ab4fed2d8()
	{
		if (!x1ee21b7088908641)
		{
			return x4e35c79189b28e26.xe5a18e5ab4fed2d8();
		}
		return true;
	}

	internal void x74f5a1ef3906e23c()
	{
		x4e35c79189b28e26.x74f5a1ef3906e23c();
	}

	private bool TryGetValue(string propertyName, Type propertyType, out object value)
	{
		if (xc16dc8a3b56cfe52 != null)
		{
			PropertyInfo property = xc16dc8a3b56cfe52.GetType().GetProperty(propertyName);
			if (property != null && property.PropertyType == propertyType)
			{
				value = property.GetValue(xc16dc8a3b56cfe52, null);
				return true;
			}
		}
		value = null;
		return false;
	}
}
