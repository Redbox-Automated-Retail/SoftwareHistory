using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Skybound.Windows.Forms;
using x43af2602ab5a7448;
using xac548eac1a95799f;

namespace Skybound.ComponentModel;

internal class EnumTypeEditor : UITypeEditor
{
	private class x4cc0d5befaf88710 : ContainerControl
	{
		private class xf04999ef713ec51e : NativeWindow
		{
			private x4cc0d5befaf88710 x9ce35bc295da5a81;

			private static Hashtable x9226f270e6929dfa = new Hashtable();

			private xf04999ef713ec51e(Form parentForm)
			{
				AssignHandle(parentForm.Handle);
				parentForm.Disposed += xbb7a11baf3cb1f21;
			}

			public static void xb24b2d91a8649a10(x4cc0d5befaf88710 x071bde1041617fce)
			{
				Form form = x071bde1041617fce.FindForm();
				xf04999ef713ec51e xf04999ef713ec51e2 = x9226f270e6929dfa[form] as xf04999ef713ec51e;
				if (xf04999ef713ec51e2 == null)
				{
					xf04999ef713ec51e2 = (xf04999ef713ec51e)(x9226f270e6929dfa[form] = new xf04999ef713ec51e(form));
				}
				xf04999ef713ec51e2.x9ce35bc295da5a81 = x071bde1041617fce;
			}

			protected override void WndProc(ref Message m)
			{
				if (x9ce35bc295da5a81 != null && !x9ce35bc295da5a81.IsDisposed && m.Msg == 132 && x9ce35bc295da5a81.x844c53236188036c)
				{
					Point pt = x9ce35bc295da5a81.PointToClient(new Point(m.LParam.ToInt32()));
					if (x9ce35bc295da5a81.x2574607afa6829b0.Contains(pt))
					{
						m.Result = ((x9ce35bc295da5a81.x0ad8a5c1615d65f3 == LeftRightAlignment.Left) ? new IntPtr(16) : new IntPtr(17));
						return;
					}
				}
				base.WndProc(ref m);
			}

			private void xbb7a11baf3cb1f21(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
			{
				if (x9226f270e6929dfa.Contains(this))
				{
					x9226f270e6929dfa.Remove(this);
				}
			}
		}

		private const int x0f5e12a3f39e3a5d = 132;

		private const int x0d55972abb53b6b3 = 16;

		private const int xb5478fefbcf78dda = 17;

		private const int x6e8d259e3d88f80d = -1;

		private bool _x844c53236188036c;

		private LeftRightAlignment _x0ad8a5c1615d65f3;

		public bool x844c53236188036c
		{
			get
			{
				return _x844c53236188036c;
			}
			set
			{
				_x844c53236188036c = value;
				base.DockPadding.Bottom = (value ? 16 : 0);
			}
		}

		public Rectangle x2574607afa6829b0
		{
			get
			{
				if (x0ad8a5c1615d65f3 == LeftRightAlignment.Left)
				{
					return new Rectangle(0, base.Height - 14, 14, 14);
				}
				return new Rectangle(base.Width - 14, base.Height - 14, 14, 14);
			}
		}

		public LeftRightAlignment x0ad8a5c1615d65f3 => _x0ad8a5c1615d65f3;

		public x4cc0d5befaf88710()
		{
			BackColor = SystemColors.Control;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (!x844c53236188036c)
			{
				return;
			}
			Rectangle rect = new Rectangle(0, base.Height - base.DockPadding.Bottom, base.Width, base.DockPadding.Bottom);
			using (LinearGradientBrush brush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), SystemColors.ControlLightLight, SystemColors.Control, LinearGradientMode.Vertical))
			{
				e.Graphics.FillRectangle(brush, rect);
			}
			if (x0ad8a5c1615d65f3 == LeftRightAlignment.Left)
			{
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j <= i; j++)
					{
						e.Graphics.FillRectangle(SystemBrushes.ControlLightLight, 4 + j * 4, base.Height - 11 + i * 4, 2, 2);
						e.Graphics.FillRectangle(SystemBrushes.ControlDark, 3 + j * 4, base.Height - 12 + i * 4, 2, 2);
					}
				}
				return;
			}
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l <= k; l++)
				{
					e.Graphics.FillRectangle(SystemBrushes.ControlLightLight, base.Width - 4 - l * 4, base.Height - 11 + k * 4, 2, 2);
					e.Graphics.FillRectangle(SystemBrushes.ControlDark, base.Width - 3 - l * 4, base.Height - 12 + k * 4, 2, 2);
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 132 && x844c53236188036c)
			{
				Point pt = PointToClient(new Point(m.LParam.ToInt32()));
				if (x2574607afa6829b0.Contains(pt))
				{
					m.Result = new IntPtr(-1);
					return;
				}
			}
			base.WndProc(ref m);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (base.Visible && FindForm() != null)
			{
				Dock = DockStyle.Fill;
				_x0ad8a5c1615d65f3 = ((PointToScreen(new Point(0, 0)).X < Screen.GetBounds(this).Width / 2) ? LeftRightAlignment.Right : LeftRightAlignment.Left);
				xf04999ef713ec51e.xb24b2d91a8649a10(this);
			}
		}
	}

	private class x5c96afa18a935783 : ListBox
	{
		private string _x0cc62080d561909c;

		public string x0cc62080d561909c
		{
			get
			{
				return _x0cc62080d561909c;
			}
			set
			{
				_x0cc62080d561909c = value;
			}
		}

		public x5c96afa18a935783()
		{
			base.BorderStyle = BorderStyle.None;
			Dock = DockStyle.Fill;
			DrawMode = DrawMode.OwnerDrawVariable;
			SetStyle(ControlStyles.ResizeRedraw, value: true);
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			xe82d2d35cb0c4d81 xe82d2d35cb0c4d82 = ((e.Index == -1) ? null : (base.Items[e.Index] as xe82d2d35cb0c4d81));
			Size size = xe82d2d35cb0c4d82.x2f9881556fe66cc1(e.Graphics, Font);
			e.ItemWidth = size.Width;
			e.ItemHeight = size.Height;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			xe82d2d35cb0c4d81 xe82d2d35cb0c4d82 = ((e.Index == -1) ? null : (base.Items[e.Index] as xe82d2d35cb0c4d81));
			e.DrawBackground();
			Rectangle bounds = e.Bounds;
			bounds.X++;
			bounds.Width -= 2;
			if (xe82d2d35cb0c4d82.x3d235fc95c355365.Length > 0)
			{
				bounds.Height >>= 1;
			}
			if (xe82d2d35cb0c4d82.xa460a0b649265441 != null)
			{
				e.Graphics.DrawImage(xe82d2d35cb0c4d82.xa460a0b649265441, bounds.X + 3, bounds.Y + 2, xe82d2d35cb0c4d82.xa460a0b649265441.Width, xe82d2d35cb0c4d82.xa460a0b649265441.Height);
				bounds.X += xe82d2d35cb0c4d82.xa460a0b649265441.Width + 8;
				bounds.Width -= xe82d2d35cb0c4d82.xa460a0b649265441.Width + 8;
			}
			using (Font x26094932cf7a = new Font(e.Font, (x0cc62080d561909c == xe82d2d35cb0c4d82.x759aa16c2016a289) ? FontStyle.Bold : FontStyle.Regular))
			{
				x5c75d7d53711a685.xc9f808b2238be32a(e.Graphics, xe82d2d35cb0c4d82.x759aa16c2016a289, x26094932cf7a, e.ForeColor, Color.Transparent, bounds, (Skybound.Windows.Forms.TextFormatFlags)36);
			}
			if (xe82d2d35cb0c4d82.x3d235fc95c355365.Length > 0)
			{
				bounds.Y += bounds.Height;
				x5c75d7d53711a685.xc9f808b2238be32a(e.Graphics, xe82d2d35cb0c4d82.x3d235fc95c355365, e.Font, ControlPaint.LightLight(e.ForeColor), Color.Transparent, bounds, (Skybound.Windows.Forms.TextFormatFlags)32804);
			}
			e.Graphics.DrawLine(SystemPens.Control, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
		}
	}

	private class xe82d2d35cb0c4d81
	{
		private string _x759aa16c2016a289;

		private string _x3d235fc95c355365;

		private Image _xa460a0b649265441;

		public string x759aa16c2016a289 => _x759aa16c2016a289;

		public string x3d235fc95c355365 => _x3d235fc95c355365;

		public Image xa460a0b649265441 => _xa460a0b649265441;

		public xe82d2d35cb0c4d81(string name, string description, Image image)
		{
			_x759aa16c2016a289 = name;
			_x3d235fc95c355365 = description;
			_xa460a0b649265441 = image;
		}

		public Size x2f9881556fe66cc1(Graphics x4b101060f4767186, Font x26094932cf7a9139)
		{
			Size size = ((xa460a0b649265441 == null) ? Size.Empty : (xa460a0b649265441.Size + new Size(8, 4)));
			Size size2 = Size.Round(x4b101060f4767186.MeasureString(x759aa16c2016a289, x26094932cf7a9139));
			Size size3 = Size.Round(x4b101060f4767186.MeasureString(x3d235fc95c355365, x26094932cf7a9139, int.MaxValue, StringFormat.GenericTypographic));
			return new Size(size.Width + Math.Max(size2.Width, size3.Width), Math.Max(size2.Height + size3.Height + 5, size.Height));
		}
	}

	private IWindowsFormsEditorService xccb3f70bb23c4475;

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		xccb3f70bb23c4475 = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
		if (xccb3f70bb23c4475 != null)
		{
			Type type = value.GetType();
			string[] names = Enum.GetNames(type);
			using x4cc0d5befaf88710 x4cc0d5befaf88711 = new x4cc0d5befaf88710();
			using x5c96afa18a935783 x5c96afa18a935784 = new x5c96afa18a935783();
			x5c96afa18a935784.x0cc62080d561909c = value.ToString();
			x4cc0d5befaf88711.Controls.Add(x5c96afa18a935784);
			int num = 0;
			int num2 = 0;
			using (Graphics x4b101060f = x4cc0d5befaf88711.CreateGraphics())
			{
				string[] array = names;
				foreach (string text in array)
				{
					xe82d2d35cb0c4d81 xe82d2d35cb0c4d82 = xee7520ae6a9712b9(context, type, text);
					x5c96afa18a935784.Items.Add(xe82d2d35cb0c4d82);
					if (text == value.ToString())
					{
						x5c96afa18a935784.SelectedIndex = x5c96afa18a935784.Items.Count - 1;
					}
					Size size = xe82d2d35cb0c4d82.x2f9881556fe66cc1(x4b101060f, x4cc0d5befaf88711.Font);
					num = Math.Max(num, size.Width + 6 + SystemInformation.VerticalScrollBarWidth);
					num2 += size.Height;
				}
			}
			Rectangle bounds = Screen.PrimaryScreen.Bounds;
			if (num < bounds.Width / 4)
			{
				x4cc0d5befaf88711.Width = num;
			}
			else
			{
				x4cc0d5befaf88711.Width = bounds.Width / 4;
				x4cc0d5befaf88711.x844c53236188036c = true;
				num2 += 16;
			}
			x4cc0d5befaf88711.Height = Math.Min(bounds.Height / 4, num2);
			x4cc0d5befaf88711.x844c53236188036c |= num2 > Screen.PrimaryScreen.Bounds.Height / 4;
			x5c96afa18a935784.MouseUp += x31327f60d865968c;
			xccb3f70bb23c4475.DropDownControl(x4cc0d5befaf88711);
			return (x5c96afa18a935784.SelectedItem == null) ? value : Enum.Parse(type, (x5c96afa18a935784.SelectedItem as xe82d2d35cb0c4d81).x759aa16c2016a289, ignoreCase: false);
		}
		return base.EditValue(context, provider, value);
	}

	private void x31327f60d865968c(object xe0292b9ed559da7d, MouseEventArgs xfbf34718e704c6bc)
	{
		if (xfbf34718e704c6bc.Button == MouseButtons.Left)
		{
			xccb3f70bb23c4475.CloseDropDown();
		}
	}

	private xe82d2d35cb0c4d81 xee7520ae6a9712b9(ITypeDescriptorContext x0f7b23d1c393aed9, Type x7cbaa9e36b617ca7, string xc15bd84e01929885)
	{
		Type type = null;
		if (x0f7b23d1c393aed9 != null)
		{
			foreach (Attribute attribute3 in x0f7b23d1c393aed9.PropertyDescriptor.Attributes)
			{
				if (attribute3 is EnumAttributeProviderAttribute)
				{
					type = (attribute3 as EnumAttributeProviderAttribute).ProviderType;
					break;
				}
			}
		}
		Attribute[] array = null;
		if ((object)type != null && Activator.CreateInstance(type) is x9adde36402496575 x9adde364024965752)
		{
			array = x9adde364024965752.GetAttributes(Enum.Parse(x7cbaa9e36b617ca7, xc15bd84e01929885, ignoreCase: false));
		}
		checked
		{
			if (array == null)
			{
				FieldInfo field = x7cbaa9e36b617ca7.GetField(xc15bd84e01929885, BindingFlags.Static | BindingFlags.Public);
				if (Attribute.IsDefined(field, typeof(DescriptionAttribute)))
				{
					array = Attribute.GetCustomAttributes(field, typeof(DescriptionAttribute));
				}
				if (Attribute.IsDefined(field, typeof(EnumValueImageProviderAttribute)))
				{
					Attribute[] customAttributes = Attribute.GetCustomAttributes(field, typeof(EnumValueImageProviderAttribute));
					if (array != null)
					{
						Attribute[] array2 = new Attribute[(uint)unchecked(array.Length + customAttributes.Length)];
						array.CopyTo(array2, 0);
						customAttributes.CopyTo(array2, array.Length);
						array = array2;
					}
				}
			}
			string description = string.Empty;
			Image image = null;
			Attribute[] array3 = array;
			foreach (Attribute attribute2 in array3)
			{
				if (attribute2 is DescriptionAttribute)
				{
					description = (attribute2 as DescriptionAttribute).Description;
				}
				else if (attribute2 is EnumValueImageProviderAttribute)
				{
					image = (attribute2 as EnumValueImageProviderAttribute).GetImage();
				}
			}
			return new xe82d2d35cb0c4d81(xc15bd84e01929885, description, image);
		}
	}
}
