using System.ComponentModel;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.CoreLib;

public class ThumbControl : Control
{
	private IContainer components;

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}

	public ThumbControl()
	{
		InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		ControlPaint.DrawBorder3D(e.Graphics, 0, 0, base.Width, base.Height, Border3DStyle.Raised);
	}
}
