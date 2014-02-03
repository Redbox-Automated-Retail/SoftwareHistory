using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Skybound.ComponentModel;
using Skybound.Windows.Forms;
using x1b802d6029d7be4a;
using x43af2602ab5a7448;
using xac548eac1a95799f;

namespace Skybound.VisualTips.Rendering;

[Skybound.ComponentModel.DisplayName("Standard")]
[TypeConverter(typeof(VisualTipRendererConverter))]
public class VisualTipRenderer : x6e74390626fc219b, ICustomTypeDescriptor
{
	internal class VisualTipRendererConverter : RendererConverter
	{
		public VisualTipRendererConverter()
			: base(typeof(VisualTipRenderer))
		{
		}
	}

	private class xa15f0e6866d01cb5
	{
		public Rectangle x5a5fada471c71c13 = Rectangle.Empty;

		public Rectangle xd93e0c943f4ea5ad = Rectangle.Empty;

		public Rectangle x6ae4612a8469678e;

		public int xe1bdb88d18410a9c;

		public xa15f0e6866d01cb5(VisualTipRenderer renderer, Image image, Font titleFont, string titleText, Font textFont, string text, int width, int pad)
		{
			x5a5fada471c71c13.Size = image?.Size ?? Size.Empty;
			if (image != null)
			{
				xd93e0c943f4ea5ad.X = x5a5fada471c71c13.Width + pad;
				xd93e0c943f4ea5ad.Width = width - x5a5fada471c71c13.Width - pad;
			}
			else
			{
				xd93e0c943f4ea5ad.Width = width;
			}
			xd93e0c943f4ea5ad.Height = 0;
			if (titleText.Length > 0)
			{
				xd93e0c943f4ea5ad.Height = (xe1bdb88d18410a9c = x5c75d7d53711a685.xbf83406974150862(titleText, titleFont, new Size(xd93e0c943f4ea5ad.Width, 0), (Skybound.Windows.Forms.TextFormatFlags)2064).Height);
			}
			if (text.Length > 0)
			{
				xd93e0c943f4ea5ad.Height += x5c75d7d53711a685.xbf83406974150862(text, textFont, new Size(xd93e0c943f4ea5ad.Width, 0), (Skybound.Windows.Forms.TextFormatFlags)2064).Height;
			}
			x6ae4612a8469678e = Rectangle.FromLTRB(0, 0, width, Math.Max(x5a5fada471c71c13.Bottom, xd93e0c943f4ea5ad.Bottom));
		}

		public void xf1d9b91c9700c401(int x08db3aeabb253cb1, int x1e218ceaee1bb583)
		{
			x5a5fada471c71c13.Offset(x08db3aeabb253cb1, x1e218ceaee1bb583);
			xd93e0c943f4ea5ad.Offset(x08db3aeabb253cb1, x1e218ceaee1bb583);
			x6ae4612a8469678e.Offset(x08db3aeabb253cb1, x1e218ceaee1bb583);
		}

		public void x25e49adc27935e96()
		{
			xd93e0c943f4ea5ad.X = x6ae4612a8469678e.X;
			x5a5fada471c71c13.X = x6ae4612a8469678e.Right - x5a5fada471c71c13.Width;
		}
	}

	[Flags]
	internal enum BorderCorners
	{
		All = 0xF,
		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 4,
		BottomRight = 8
	}

	[Skybound.ComponentModel.DisplayName("(Default)")]
	private class xc81619ffa3feb0ec : VisualTipRenderer
	{
		protected override VisualTipLayout OnCreateLayout(VisualTip tip)
		{
			if (GetDefault() != null)
			{
				return GetDefault().OnCreateLayout(tip);
			}
			return base.OnCreateLayout(tip);
		}

		protected override void OnDrawShadow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
		{
			if (GetDefault() == null)
			{
				base.OnDrawShadow(e, tip, layout);
			}
			else
			{
				GetDefault().OnDrawShadow(e, tip, layout);
			}
		}

		protected override void OnDrawWindow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
		{
			if (GetDefault() == null)
			{
				base.OnDrawWindow(e, tip, layout);
			}
			else
			{
				GetDefault().OnDrawWindow(e, tip, layout);
			}
		}

		protected override void OnDrawElement(PaintEventArgs e, VisualTip tip, VisualTipLayout layout, VisualTipRenderElement element)
		{
			if (GetDefault() == null)
			{
				base.OnDrawElement(e, tip, layout, element);
			}
			else
			{
				GetDefault().OnDrawElement(e, tip, layout, element);
			}
		}

		protected override Font GetElementFont(VisualTip tip, VisualTipRenderElement element)
		{
			if (GetDefault() != null)
			{
				return GetDefault().GetElementFont(tip, element);
			}
			return base.GetElementFont(tip, element);
		}

		protected override Color GetElementTextColor(VisualTip tip, VisualTipRenderElement element)
		{
			if (GetDefault() != null)
			{
				return GetDefault().GetElementTextColor(tip, element);
			}
			return base.GetElementTextColor(tip, element);
		}

		protected override Region OnCreateMaskRegion(VisualTip tip, VisualTipLayout layout)
		{
			if (GetDefault() != null)
			{
				return GetDefault().OnCreateMaskRegion(tip, layout);
			}
			return base.OnCreateMaskRegion(tip, layout);
		}
	}

	private const int x18ad9149890f096d = 6;

	private static VisualTipRenderer _xb9715d2f06b63cf0;

	private static VisualTipRenderer _xe3c997ae7518d1e7 = new xc81619ffa3feb0ec();

	private static EventHandler x5699d200f4156a22;

	internal xa457962045d12902 x4e35c79189b28e26 = new xa457962045d12902();

	public static VisualTipRenderer DefaultRenderer => _xe3c997ae7518d1e7;

	internal static bool x9843c083bd22e3f5 => OSFeature.Feature.IsPresent(OSFeature.LayeredWindows);

	public static event EventHandler DefaultRendererChanged
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			x5699d200f4156a22 = (EventHandler)Delegate.Combine(x5699d200f4156a22, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			x5699d200f4156a22 = (EventHandler)Delegate.Remove(x5699d200f4156a22, value);
		}
	}

	public static void SetDefault(VisualTipRenderer renderer)
	{
		if (renderer != DefaultRenderer && _xb9715d2f06b63cf0 != renderer)
		{
			_xb9715d2f06b63cf0 = renderer;
			xb60832c23a4388ee(EventArgs.Empty);
		}
	}

	public static VisualTipRenderer GetDefault()
	{
		return _xb9715d2f06b63cf0;
	}

	private static void xb60832c23a4388ee(EventArgs xfbf34718e704c6bc)
	{
		if (x5699d200f4156a22 != null)
		{
			x5699d200f4156a22(null, xfbf34718e704c6bc);
		}
	}

	internal void x255a9125fad2d9bb(string xc3513c7f2bbafa84)
	{
		x4e35c79189b28e26.x255a9125fad2d9bb(xc3513c7f2bbafa84);
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

	private void x72265cd62c22d100(PaintEventArgs xfbf34718e704c6bc, VisualTip x2636ebb6bd343dac, ref VisualTipLayout x2612f62f94df47de)
	{
		x97b82ba21b1ae986(xfbf34718e704c6bc);
		x20478c4d711fe4c8(x2636ebb6bd343dac);
		if (x2612f62f94df47de == null)
		{
			x2612f62f94df47de = CreateLayout(x2636ebb6bd343dac);
		}
	}

	private void x20478c4d711fe4c8(VisualTip x2636ebb6bd343dac)
	{
		if (x2636ebb6bd343dac == null)
		{
			throw new ArgumentNullException("tip");
		}
		if (x2636ebb6bd343dac.Provider == null)
		{
			throw new ArgumentException("The tip must be associated with a VisualTipProvider before it is rendered.");
		}
	}

	private void x97b82ba21b1ae986(PaintEventArgs xfbf34718e704c6bc)
	{
		if (xfbf34718e704c6bc == null)
		{
			throw new ArgumentNullException("e");
		}
		if (xfbf34718e704c6bc.Graphics == null)
		{
			throw new ArgumentException("The Graphics specified by PaintEventArgs may not be a null reference.", "e");
		}
	}

	internal Color x794d88e66bc5597a(Color x6c50a99faab7d741)
	{
		if (x6c50a99faab7d741.Equals(Color.Empty) || x6c50a99faab7d741.A == byte.MaxValue)
		{
			return x6c50a99faab7d741;
		}
		return Color.FromArgb(255, x6c50a99faab7d741);
	}

	protected bool HasFooter(VisualTip tip)
	{
		x20478c4d711fe4c8(tip);
		if (!tip.HideFooter)
		{
			if (tip.FooterText.Length <= 0)
			{
				return tip.FooterImage != null;
			}
			return true;
		}
		return false;
	}

	protected string GetTitleText(VisualTip tip)
	{
		x20478c4d711fe4c8(tip);
		string text = "";
		if (tip.Shortcut != Shortcut.None)
		{
			text = "(" + TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)tip.Shortcut) + ")";
		}
		if (tip.Title.Length <= 0)
		{
			return text;
		}
		return tip.Title + " " + text;
	}

	protected string GetBodyText(VisualTip tip)
	{
		x20478c4d711fe4c8(tip);
		if (tip.Enabled || tip.DisabledText.Length <= 0)
		{
			return tip.Text;
		}
		return tip.DisabledText;
	}

	protected virtual Font GetElementFont(VisualTip tip, VisualTipRenderElement element)
	{
		x20478c4d711fe4c8(tip);
		if (element == VisualTipRenderElement.Title || element == VisualTipRenderElement.DisabledMessage || element == VisualTipRenderElement.FooterText)
		{
			return new Font(tip.Font, FontStyle.Bold);
		}
		return tip.Font;
	}

	protected virtual Color GetElementTextColor(VisualTip tip, VisualTipRenderElement element)
	{
		x20478c4d711fe4c8(tip);
		return SystemColors.InfoText;
	}

	protected bool HasShadow(VisualTip tip)
	{
		x20478c4d711fe4c8(tip);
		if (x9843c083bd22e3f5)
		{
			if (tip.Provider.Shadow == VisualTipShadow.Enabled)
			{
				return true;
			}
			if (tip.Provider.Shadow == VisualTipShadow.SystemDefault)
			{
				return xc1ab0cc8a4568e7b.x003e94eb365fa7c9;
			}
		}
		return false;
	}

	public VisualTipLayout CreateLayout(VisualTip tip)
	{
		x20478c4d711fe4c8(tip);
		VisualTipLayout visualTipLayout = OnCreateLayout(tip);
		if (visualTipLayout == null)
		{
			throw new InvalidOperationException("A null value may not be returned by OnCreateLayout.");
		}
		return visualTipLayout;
	}

	protected virtual VisualTipLayout OnCreateLayout(VisualTip tip)
	{
		VisualTipLayout visualTipLayout = new VisualTipLayout();
		string titleText = GetTitleText(tip);
		string bodyText = GetBodyText(tip);
		bool flag = titleText.Length > 0;
		bool flag2 = bodyText.Length > 0 || tip.Image != null;
		Font elementFont = GetElementFont(tip, VisualTipRenderElement.Title);
		Font elementFont2 = GetElementFont(tip, VisualTipRenderElement.Text);
		Font elementFont3 = GetElementFont(tip, VisualTipRenderElement.FooterText);
		Font elementFont4 = GetElementFont(tip, VisualTipRenderElement.DisabledMessage);
		int num = (flag ? xbf83406974150862(titleText, elementFont, 0).Width : 0);
		int num2 = (flag2 ? xbf83406974150862(bodyText, elementFont2, 0).Width : 0);
		int num3 = (HasFooter(tip) ? xbf83406974150862(tip.FooterText, elementFont3, 0).Width : 0);
		if (!tip.Enabled && tip.DisabledMessage.Length > 0)
		{
			num2 = Math.Max(xbf83406974150862(tip.DisabledMessage, elementFont4, 0).Width, num2);
		}
		if (tip.TitleImage != null)
		{
			num += tip.TitleImage.Width + 6;
		}
		if (tip.Image != null)
		{
			num2 += tip.Image.Width + 6;
		}
		if (HasFooter(tip) && tip.FooterImage != null)
		{
			num3 += tip.FooterImage.Width + 6;
		}
		int val = Math.Max(Math.Max(num, num2), num3);
		int num4 = Math.Min(tip.MaximumWidth - 6 - 6, val);
		int num5 = 6;
		xa15f0e6866d01cb5 xa15f0e6866d01cb6 = new xa15f0e6866d01cb5(this, tip.TitleImage, GetElementFont(tip, VisualTipRenderElement.Title), titleText, null, "", num4, 6);
		xa15f0e6866d01cb6.xf1d9b91c9700c401(6, 6);
		if (xa15f0e6866d01cb6.x6ae4612a8469678e.Height > 0)
		{
			num5 += xa15f0e6866d01cb6.x6ae4612a8469678e.Height + 6;
		}
		xa15f0e6866d01cb5 xa15f0e6866d01cb7 = ((!tip.Enabled) ? new xa15f0e6866d01cb5(this, tip.Image, GetElementFont(tip, VisualTipRenderElement.DisabledMessage), tip.DisabledMessage, GetElementFont(tip, VisualTipRenderElement.Text), bodyText, num4, 6) : new xa15f0e6866d01cb5(this, tip.Image, null, "", GetElementFont(tip, VisualTipRenderElement.Text), bodyText, num4, 6));
		xa15f0e6866d01cb7.xf1d9b91c9700c401(6, num5);
		if (tip.RightToLeft == RightToLeft.Yes)
		{
			xa15f0e6866d01cb7.x25e49adc27935e96();
		}
		if (xa15f0e6866d01cb7.x6ae4612a8469678e.Height > 0)
		{
			num5 += xa15f0e6866d01cb7.x6ae4612a8469678e.Height + 6;
		}
		xa15f0e6866d01cb5 xa15f0e6866d01cb8 = (HasFooter(tip) ? new xa15f0e6866d01cb5(this, tip.FooterImage, GetElementFont(tip, VisualTipRenderElement.FooterText), tip.FooterText, null, "", num4, 6) : new xa15f0e6866d01cb5(this, null, null, "", null, "", num4, 6));
		xa15f0e6866d01cb8.xf1d9b91c9700c401(6, num5);
		if (tip.RightToLeft == RightToLeft.Yes)
		{
			xa15f0e6866d01cb8.x25e49adc27935e96();
		}
		if (xa15f0e6866d01cb8.x6ae4612a8469678e.Height > 0)
		{
			xa15f0e6866d01cb8.xf1d9b91c9700c401(0, 6);
		}
		visualTipLayout.WindowBounds = new Rectangle(0, 0, 6 + num4 + 6, xa15f0e6866d01cb8.x6ae4612a8469678e.Bottom + (HasFooter(tip) ? 6 : 0));
		visualTipLayout.SetElementBounds(VisualTipRenderElement.TitleImage, xa15f0e6866d01cb6.x5a5fada471c71c13);
		visualTipLayout.SetElementBounds(VisualTipRenderElement.Title, xa15f0e6866d01cb6.xd93e0c943f4ea5ad);
		visualTipLayout.SetElementBounds(VisualTipRenderElement.Image, xa15f0e6866d01cb7.x5a5fada471c71c13);
		visualTipLayout.SetElementBounds(VisualTipRenderElement.FooterImage, xa15f0e6866d01cb8.x5a5fada471c71c13);
		visualTipLayout.SetElementBounds(VisualTipRenderElement.FooterText, xa15f0e6866d01cb8.xd93e0c943f4ea5ad);
		if (xa15f0e6866d01cb7.xe1bdb88d18410a9c > 0)
		{
			Rectangle xd93e0c943f4ea5ad = xa15f0e6866d01cb7.xd93e0c943f4ea5ad;
			xd93e0c943f4ea5ad.Height = xa15f0e6866d01cb7.xe1bdb88d18410a9c;
			visualTipLayout.SetElementBounds(VisualTipRenderElement.DisabledMessage, xd93e0c943f4ea5ad);
			xd93e0c943f4ea5ad.Y = xd93e0c943f4ea5ad.Bottom;
			xd93e0c943f4ea5ad.Height = xa15f0e6866d01cb7.xd93e0c943f4ea5ad.Height - xa15f0e6866d01cb7.xe1bdb88d18410a9c;
			visualTipLayout.SetElementBounds(VisualTipRenderElement.Text, xd93e0c943f4ea5ad);
		}
		else
		{
			visualTipLayout.SetElementBounds(VisualTipRenderElement.DisabledMessage, Rectangle.Empty);
			visualTipLayout.SetElementBounds(VisualTipRenderElement.Text, xa15f0e6866d01cb7.xd93e0c943f4ea5ad);
		}
		if (HasShadow(tip))
		{
			visualTipLayout.ShadowBounds = new Rectangle(visualTipLayout.WindowBounds.Location, visualTipLayout.WindowBounds.Size + new Size(5, 5));
			if (tip.RightToLeft == RightToLeft.Yes)
			{
				visualTipLayout.x3415cd70babaee65(5, 0);
			}
		}
		else
		{
			visualTipLayout.ShadowBounds = visualTipLayout.WindowBounds;
		}
		return visualTipLayout;
	}

	private Size xbf83406974150862(string xb41faee6912a2313, Font x26094932cf7a9139, int x4140ff975930ddd9)
	{
		return x5c75d7d53711a685.xbf83406974150862(xb41faee6912a2313, x26094932cf7a9139, new Size((x4140ff975930ddd9 == 0) ? 32767 : x4140ff975930ddd9, 0), (Skybound.Windows.Forms.TextFormatFlags)2064);
	}

	protected Size MeasureElement(VisualTip tip, VisualTipRenderElement element, int maximumWidth)
	{
		x20478c4d711fe4c8(tip);
		switch (element)
		{
		case VisualTipRenderElement.Image:
			if (tip.Image != null)
			{
				return tip.Image.Size;
			}
			return Size.Empty;
		case VisualTipRenderElement.FooterImage:
			if (tip.FooterImage != null)
			{
				return tip.FooterImage.Size;
			}
			return Size.Empty;
		case VisualTipRenderElement.TitleImage:
			if (tip.TitleImage != null)
			{
				return tip.TitleImage.Size;
			}
			return Size.Empty;
		default:
		{
			string text = null;
			switch (element)
			{
			case VisualTipRenderElement.DisabledMessage:
				text = tip.DisabledMessage;
				break;
			case VisualTipRenderElement.FooterText:
				text = tip.FooterText;
				break;
			case VisualTipRenderElement.Text:
				text = GetBodyText(tip);
				break;
			case VisualTipRenderElement.Title:
				text = GetTitleText(tip);
				break;
			}
			if (text != null)
			{
				return xbf83406974150862(text, GetElementFont(tip, element), tip.Provider.MaximumWidth);
			}
			return Size.Empty;
		}
		}
	}

	public void Draw(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		x72265cd62c22d100(e, tip, ref layout);
		OnDrawShadow(e, tip, layout);
		OnDrawWindow(e, tip, layout);
		foreach (object value in Enum.GetValues(typeof(VisualTipRenderElement)))
		{
			VisualTipRenderElement element = (VisualTipRenderElement)value;
			OnDrawElement(e, tip, layout, element);
		}
	}

	protected virtual void OnDrawShadow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		if (HasShadow(tip) && Rectangle.Intersect(layout.ShadowBounds, layout.WindowBounds) != layout.ShadowBounds)
		{
			x777ac584ca37e79e(e.Graphics, layout.ShadowBounds, 8, Color.FromArgb(128, Color.Black));
		}
	}

	protected virtual void OnDrawWindow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		Rectangle windowBounds = layout.WindowBounds;
		e.Graphics.FillRectangle(SystemBrushes.Info, windowBounds);
		e.Graphics.DrawRectangle(SystemPens.InfoText, new Rectangle(windowBounds.X, windowBounds.Y, windowBounds.Width - 1, windowBounds.Height - 1));
	}

	protected virtual void OnDrawElement(PaintEventArgs e, VisualTip tip, VisualTipLayout layout, VisualTipRenderElement element)
	{
		Rectangle elementBounds = layout.GetElementBounds(element);
		string text = "";
		Image image = null;
		if (elementBounds.Size.IsEmpty)
		{
			return;
		}
		switch (element)
		{
		case VisualTipRenderElement.DisabledMessage:
			text = tip.DisabledMessage;
			break;
		case VisualTipRenderElement.FooterText:
			text = tip.FooterText;
			break;
		case VisualTipRenderElement.Text:
			text = GetBodyText(tip);
			break;
		case VisualTipRenderElement.Title:
			text = GetTitleText(tip);
			break;
		case VisualTipRenderElement.TitleImage:
			image = tip.TitleImage;
			break;
		case VisualTipRenderElement.Image:
			image = tip.Image;
			break;
		case VisualTipRenderElement.FooterImage:
			image = tip.FooterImage;
			break;
		}
		if (text.Length > 0)
		{
			using (x218122dd818ebd0b x218122dd818ebd0b2 = new x218122dd818ebd0b())
			{
				x218122dd818ebd0b2.x55fae544baab0cc8(e.Graphics, elementBounds);
				Rectangle rectangle = new Rectangle(0, 0, elementBounds.Width, elementBounds.Height);
				layout.Offset(-elementBounds.X, -elementBounds.Y);
				OnDrawWindow(new PaintEventArgs(x218122dd818ebd0b2.xbc626ed723e04991, rectangle), tip, layout);
				layout.Offset(elementBounds.X, elementBounds.Y);
				Skybound.Windows.Forms.TextFormatFlags textFormatFlags = (Skybound.Windows.Forms.TextFormatFlags)2064;
				if (tip.RightToLeft == RightToLeft.Yes)
				{
					textFormatFlags |= (Skybound.Windows.Forms.TextFormatFlags)0x20002;
				}
				x5c75d7d53711a685.xc9f808b2238be32a(x218122dd818ebd0b2.xbc626ed723e04991, text, GetElementFont(tip, element), GetElementTextColor(tip, element), Color.Transparent, rectangle, textFormatFlags);
				x218122dd818ebd0b2.xe406325e56f74b46();
				return;
			}
		}
		if (image != null)
		{
			e.Graphics.DrawImage(image, elementBounds);
		}
	}

	internal static GraphicsPath xe4b7016dff2efe10(Rectangle xda73fcb97c77d998, int xd6ed827fa7f40115, BorderCorners x0c5e475996adcc9c)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if ((x0c5e475996adcc9c & BorderCorners.TopLeft) == 0)
		{
			graphicsPath.AddLine(xda73fcb97c77d998.X, xda73fcb97c77d998.Y, xda73fcb97c77d998.X, xda73fcb97c77d998.Y);
		}
		else
		{
			graphicsPath.AddArc(new Rectangle(xda73fcb97c77d998.X, xda73fcb97c77d998.Y, xd6ed827fa7f40115, xd6ed827fa7f40115), 180f, 90f);
		}
		if ((x0c5e475996adcc9c & BorderCorners.TopRight) == 0)
		{
			graphicsPath.AddLine(xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Y, xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Y);
		}
		else
		{
			graphicsPath.AddArc(new Rectangle(xda73fcb97c77d998.Right - xd6ed827fa7f40115 - 1, xda73fcb97c77d998.Y, xd6ed827fa7f40115, xd6ed827fa7f40115), 270f, 90f);
		}
		if ((x0c5e475996adcc9c & BorderCorners.BottomRight) == 0)
		{
			graphicsPath.AddLine(xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Bottom - 1, xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Bottom - 1);
		}
		else
		{
			graphicsPath.AddArc(new Rectangle(xda73fcb97c77d998.Right - xd6ed827fa7f40115 - 1, xda73fcb97c77d998.Bottom - xd6ed827fa7f40115 - 1, xd6ed827fa7f40115, xd6ed827fa7f40115), 0f, 90f);
		}
		if ((x0c5e475996adcc9c & BorderCorners.BottomLeft) == 0)
		{
			graphicsPath.AddLine(xda73fcb97c77d998.X, xda73fcb97c77d998.Bottom - 1, xda73fcb97c77d998.X, xda73fcb97c77d998.Bottom - 1);
		}
		else
		{
			graphicsPath.AddArc(new Rectangle(xda73fcb97c77d998.X, xda73fcb97c77d998.Bottom - xd6ed827fa7f40115 - 1, xd6ed827fa7f40115, xd6ed827fa7f40115), 90f, 90f);
		}
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal static void x777ac584ca37e79e(Graphics x41347a961b838962, Rectangle xda73fcb97c77d998, int xd6ed827fa7f40115, Color x6c50a99faab7d741)
	{
		x41347a961b838962.SmoothingMode = SmoothingMode.AntiAlias;
		using (Brush brush = new SolidBrush(Color.FromArgb(x6c50a99faab7d741.A / 4, x6c50a99faab7d741.R, x6c50a99faab7d741.G, x6c50a99faab7d741.B)))
		{
			for (int i = 0; i < 4; i++)
			{
				xda73fcb97c77d998.Inflate(-1, -1);
				using GraphicsPath path = xe4b7016dff2efe10(xda73fcb97c77d998, xd6ed827fa7f40115, BorderCorners.All);
				x41347a961b838962.FillPath(brush, path);
			}
		}
		x41347a961b838962.SmoothingMode = SmoothingMode.Default;
	}

	public Region CreateMaskRegion(VisualTip tip, VisualTipLayout layout)
	{
		x20478c4d711fe4c8(tip);
		if (layout == null)
		{
			layout = OnCreateLayout(tip);
		}
		return OnCreateMaskRegion(tip, layout);
	}

	protected virtual Region OnCreateMaskRegion(VisualTip tip, VisualTipLayout layout)
	{
		return null;
	}
}
