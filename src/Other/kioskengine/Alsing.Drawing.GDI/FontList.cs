using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

public class FontList : UITypeEditor
{
	private IWindowsFormsEditorService edSvc;

	private ListBox FontListbox;

	private bool handleLostfocus;

	private void LB_DrawItem(object sender, DrawItemEventArgs e)
	{
		bool flag = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
		if (e.Index != -1)
		{
			object obj = FontListbox.Items[e.Index];
			string text = obj.ToString();
			Brush brush = (flag ? SystemBrushes.HighlightText : SystemBrushes.WindowText);
			if (flag)
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, new Rectangle(37, e.Bounds.Top, e.Bounds.Width - 37, FontListbox.ItemHeight));
				e.Graphics.FillRectangle(SystemBrushes.Highlight, new Rectangle(38, e.Bounds.Top + 1, e.Bounds.Width - 37 - 2, FontListbox.ItemHeight - 2));
				ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(37, e.Bounds.Top, e.Bounds.Width - 37, FontListbox.ItemHeight));
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, 0, e.Bounds.Top, e.Bounds.Width, FontListbox.ItemHeight);
			}
			e.Graphics.DrawString(text, e.Font, brush, 38f, e.Bounds.Top + 4);
			e.Graphics.SetClip(new Rectangle(1, e.Bounds.Top + 2, 34, FontListbox.ItemHeight - 4));
			e.Graphics.FillRectangle(SystemBrushes.Highlight, new Rectangle(1, e.Bounds.Top + 2, 34, FontListbox.ItemHeight - 4));
			IntPtr hdc = e.Graphics.GetHdc();
			GDIFont gDIFont = new GDIFont(text, 9f);
			int lpnTabStopPositions = 0;
			IntPtr hObject = NativeMethods.SelectObject(hdc, gDIFont.hFont);
			NativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(SystemColors.Window));
			NativeMethods.SetBkMode(hdc, 0);
			NativeMethods.TabbedTextOut(hdc, 3, e.Bounds.Top + 5, "abc", 3, 0, ref lpnTabStopPositions, 0);
			NativeMethods.SelectObject(hdc, hObject);
			gDIFont.Dispose();
			e.Graphics.ReleaseHdc(hdc);
			e.Graphics.DrawRectangle(Pens.Black, new Rectangle(1, e.Bounds.Top + 2, 34, FontListbox.ItemHeight - 4));
			e.Graphics.ResetClip();
		}
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context != null && context.Instance != null && provider != null)
		{
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (edSvc != null)
			{
				FontListbox = new ListBox
				{
					DrawMode = DrawMode.OwnerDrawFixed,
					BorderStyle = BorderStyle.None,
					Sorted = true
				};
				FontListbox.MouseDown += OnMouseDown;
				FontListbox.DoubleClick += ValueChanged;
				FontListbox.DrawItem += LB_DrawItem;
				FontListbox.ItemHeight = 20;
				FontListbox.Height = 200;
				FontListbox.Width = 180;
				ICollection collection = new FontEnum().EnumFonts();
				foreach (string item in collection)
				{
					FontListbox.Items.Add(item);
				}
				edSvc.DropDownControl(FontListbox);
				if (FontListbox.SelectedItem != null)
				{
					return FontListbox.SelectedItem.ToString();
				}
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	private void OnMouseDown(object sender, MouseEventArgs e)
	{
		if (!handleLostfocus && FontListbox.ClientRectangle.Contains(FontListbox.PointToClient(new Point(e.X, e.Y))))
		{
			FontListbox.LostFocus += ValueChanged;
			handleLostfocus = true;
		}
	}

	private void ValueChanged(object sender, EventArgs e)
	{
		if (edSvc != null)
		{
			edSvc.CloseDropDown();
		}
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		string fontname = e.Value.ToString();
		Bitmap image = new Bitmap(e.Bounds.Width, e.Bounds.Height);
		Graphics graphics = Graphics.FromImage(image);
		graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
		IntPtr hdc = graphics.GetHdc();
		GDIFont gDIFont = new GDIFont(fontname, 9f);
		int lpnTabStopPositions = 0;
		IntPtr hObject = NativeMethods.SelectObject(hdc, gDIFont.hFont);
		NativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(SystemColors.Window));
		NativeMethods.SetBkMode(hdc, 0);
		NativeMethods.TabbedTextOut(hdc, 1, 1, "abc", 3, 0, ref lpnTabStopPositions, 0);
		NativeMethods.SelectObject(hdc, hObject);
		gDIFont.Dispose();
		graphics.ReleaseHdc(hdc);
		e.Graphics.DrawImage(image, e.Bounds.Left, e.Bounds.Top);
	}

	public override bool GetPaintValueSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
