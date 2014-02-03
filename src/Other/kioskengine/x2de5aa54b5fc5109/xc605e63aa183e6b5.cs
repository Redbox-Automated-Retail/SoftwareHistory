using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace x2de5aa54b5fc5109;

internal class xc605e63aa183e6b5 : Form
{
	private ListBox x3376d1b8c03db028;

	private Label xf5622c25220a6c23;

	private Button xc7d65e4a1060dfff;

	private Button x506702aab4d2481f;

	private PictureBox xa7137782a59ccbce;

	private x7d8d2ed69d304345 _x7d8d2ed69d304345;

	public x7d8d2ed69d304345 x7d8d2ed69d304345
	{
		get
		{
			return _x7d8d2ed69d304345;
		}
		set
		{
			_x7d8d2ed69d304345 = value;
			x3376d1b8c03db028.Items.Clear();
			if (_x7d8d2ed69d304345 != null)
			{
				xdc8e11111d3c00f8[] array = _x7d8d2ed69d304345.xdbe5f635faa12ead();
				xdc8e11111d3c00f8[] array2 = array;
				foreach (xdc8e11111d3c00f8 xdc8e11111d3c00f9 in array2)
				{
					x3376d1b8c03db028.Items.Add(xdc8e11111d3c00f9);
				}
				x3376d1b8c03db028.SelectedIndex = 0;
			}
		}
	}

	public xdc8e11111d3c00f8 x88ff53b73da867e0
	{
		get
		{
			if (x3376d1b8c03db028.SelectedItem != null)
			{
				return (xdc8e11111d3c00f8)x3376d1b8c03db028.SelectedItem;
			}
			return xdc8e11111d3c00f8.x45260ad4b94166f2;
		}
	}

	public xc605e63aa183e6b5()
	{
		x85601834555fb7d5();
	}

	private void x85601834555fb7d5()
	{
		x3376d1b8c03db028 = new ListBox();
		xf5622c25220a6c23 = new Label();
		xa7137782a59ccbce = new PictureBox();
		xc7d65e4a1060dfff = new Button();
		x506702aab4d2481f = new Button();
		SuspendLayout();
		x3376d1b8c03db028.IntegralHeight = false;
		x3376d1b8c03db028.Location = new Point(7, 40);
		x3376d1b8c03db028.Name = "lstFormats";
		x3376d1b8c03db028.Size = new Size(124, 256);
		x3376d1b8c03db028.TabIndex = 0;
		x3376d1b8c03db028.DoubleClick += x094d8277732f2245;
		x3376d1b8c03db028.SelectedIndexChanged += x7affc26e915cbd7d;
		xf5622c25220a6c23.Location = new Point(6, 8);
		xf5622c25220a6c23.Name = "label1";
		xf5622c25220a6c23.Size = new Size(380, 32);
		xf5622c25220a6c23.TabIndex = 1;
		xf5622c25220a6c23.Text = "This icon file contains more than one size or color depth.  Please choose the size and color depth to load from the list below.";
		xa7137782a59ccbce.BorderStyle = BorderStyle.Fixed3D;
		xa7137782a59ccbce.Location = new Point(135, 40);
		xa7137782a59ccbce.Name = "picIcon";
		xa7137782a59ccbce.Size = new Size(256, 256);
		xa7137782a59ccbce.TabIndex = 2;
		xa7137782a59ccbce.TabStop = false;
		xa7137782a59ccbce.Paint += x87cf50beaa612702;
		xc7d65e4a1060dfff.Location = new Point(317, 304);
		xc7d65e4a1060dfff.Name = "cmdOK";
		xc7d65e4a1060dfff.Size = new Size(74, 22);
		xc7d65e4a1060dfff.TabIndex = 3;
		xc7d65e4a1060dfff.Text = "OK";
		xc7d65e4a1060dfff.Click += x14de4513dff407b5;
		x506702aab4d2481f.DialogResult = DialogResult.Cancel;
		x506702aab4d2481f.Location = new Point(317, 330);
		x506702aab4d2481f.Name = "cmdCancel";
		x506702aab4d2481f.Size = new Size(74, 22);
		x506702aab4d2481f.TabIndex = 4;
		x506702aab4d2481f.Text = "Cancel";
		base.AcceptButton = xc7d65e4a1060dfff;
		AutoScaleBaseSize = new Size(5, 14);
		base.CancelButton = x506702aab4d2481f;
		base.ClientSize = new Size(400, 361);
		base.Controls.Add(x506702aab4d2481f);
		base.Controls.Add(xc7d65e4a1060dfff);
		base.Controls.Add(xa7137782a59ccbce);
		base.Controls.Add(xf5622c25220a6c23);
		base.Controls.Add(x3376d1b8c03db028);
		Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "IconFormatDialog";
		base.StartPosition = FormStartPosition.CenterParent;
		Text = "Choose Icon";
		ResumeLayout(performLayout: false);
	}

	private void x7affc26e915cbd7d(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		xa7137782a59ccbce.Invalidate();
	}

	private void x87cf50beaa612702(object xe0292b9ed559da7d, PaintEventArgs xfbf34718e704c6bc)
	{
		using (Brush brush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.LightGray, Color.White))
		{
			xfbf34718e704c6bc.Graphics.FillRectangle(brush, xa7137782a59ccbce.ClientRectangle);
		}
		if (x3376d1b8c03db028.SelectedItem != null)
		{
			xfbf34718e704c6bc.Graphics.DrawImage(x7d8d2ed69d304345.xd2b1a4678f6c35f4((xdc8e11111d3c00f8)x3376d1b8c03db028.SelectedItem), 0, 0);
		}
	}

	private void x14de4513dff407b5(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void x094d8277732f2245(object xe0292b9ed559da7d, EventArgs xfbf34718e704c6bc)
	{
		if (x3376d1b8c03db028.SelectedIndex >= 0)
		{
			x14de4513dff407b5(xc7d65e4a1060dfff, EventArgs.Empty);
		}
	}
}
