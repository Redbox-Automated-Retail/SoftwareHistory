using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Skybound.VisualTips.Rendering;
using Skybound.Windows.Forms;
using x43af2602ab5a7448;

namespace Skybound.VisualTips;

internal class VisualTipEditor : UITypeEditor
{
	private VisualTip x2795ceea9a4a3d4f;

	private VisualTipLayout xb7ae55095fddecd9;

	private IWindowsFormsEditorService x124dde016da43a88;

	private x218122dd818ebd0b x4ae5537db27c5fe4 = new x218122dd818ebd0b();

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		x124dde016da43a88 = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
		VisualTip visualTip = value as VisualTip;
		Control control = new Control();
		if (visualTip.xe5a18e5ab4fed2d8())
		{
			xb7ae55095fddecd9 = visualTip.Provider.Renderer.CreateLayout(visualTip);
			xb7ae55095fddecd9.Offset(8, 8);
			x2795ceea9a4a3d4f = visualTip;
			control.Size = xb7ae55095fddecd9.GetSize() + new Size(16, 16);
			control.Paint += xa9dfe707f168b76a;
			control.MouseDown += x0bf707df71a20a95;
		}
		else
		{
			control.Size = new Size(144, 28);
			control.Paint += x1437e2d1f30942ff;
		}
		x124dde016da43a88.DropDownControl(control);
		return value;
	}

	private void xa9dfe707f168b76a(object xe0292b9ed559da7d, PaintEventArgs xfbf34718e704c6bc)
	{
		x4ae5537db27c5fe4.x55fae544baab0cc8(xfbf34718e704c6bc.Graphics, xfbf34718e704c6bc.ClipRectangle);
		x4ae5537db27c5fe4.xbc626ed723e04991.Clear(Color.White);
		x2795ceea9a4a3d4f.Provider.Renderer.Draw(new PaintEventArgs(x4ae5537db27c5fe4.xbc626ed723e04991, xfbf34718e704c6bc.ClipRectangle), x2795ceea9a4a3d4f, xb7ae55095fddecd9);
		x4ae5537db27c5fe4.xe406325e56f74b46();
	}

	private void x0bf707df71a20a95(object xe0292b9ed559da7d, MouseEventArgs xfbf34718e704c6bc)
	{
		x124dde016da43a88.CloseDropDown();
	}

	private void x1437e2d1f30942ff(object xe0292b9ed559da7d, PaintEventArgs xfbf34718e704c6bc)
	{
		x4ae5537db27c5fe4.x55fae544baab0cc8(xfbf34718e704c6bc.Graphics, xfbf34718e704c6bc.ClipRectangle);
		x4ae5537db27c5fe4.xbc626ed723e04991.Clear(SystemColors.Control);
		using (Font x26094932cf7a = new Font("Tahoma", 8f))
		{
			x5c75d7d53711a685.xc9f808b2238be32a(x4ae5537db27c5fe4.xbc626ed723e04991, "No VisualTip will be displayed\r\nfor this component.", x26094932cf7a, SystemColors.ControlText, SystemColors.Control, (xe0292b9ed559da7d as Control).ClientRectangle, Skybound.Windows.Forms.TextFormatFlags.HorizontalCenter);
		}
		x4ae5537db27c5fe4.xe406325e56f74b46();
	}
}
