using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Alsing.SourceCode;

public class TextStyleUIEditor : UITypeEditor
{
	private IWindowsFormsEditorService edSvc;

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context != null && context.Instance != null && provider != null)
		{
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (edSvc != null)
			{
				TextStyle textStyle = (TextStyle)value;
				using TextStyleDesignerDialog dialog = new TextStyleDesignerDialog(textStyle);
				context.OnComponentChanging();
				if (edSvc.ShowDialog(dialog) == DialogResult.OK)
				{
					ValueChanged(this, EventArgs.Empty);
					context.OnComponentChanged();
					return textStyle;
				}
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	private void ValueChanged(object sender, EventArgs e)
	{
		_ = edSvc;
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		TextStyle textStyle = (TextStyle)e.Value;
		using (SolidBrush brush = new SolidBrush(textStyle.BackColor))
		{
			e.Graphics.FillRectangle(brush, e.Bounds);
		}
		FontStyle fontStyle = FontStyle.Regular;
		if (textStyle.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (textStyle.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (textStyle.Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		Font font = new Font("arial", 8f, fontStyle);
		using (SolidBrush brush2 = new SolidBrush(textStyle.ForeColor))
		{
			e.Graphics.DrawString("abc", font, brush2, e.Bounds);
		}
		font.Dispose();
	}

	public override bool GetPaintValueSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
