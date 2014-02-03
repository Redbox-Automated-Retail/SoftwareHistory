using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.CoreLib;

public class SplitViewChildControl : BaseControl
{
	private Container components;

	private Panel Filler;

	protected HScrollBar hScroll;

	public ThumbControl LeftThumb;

	public ThumbControl TopThumb;

	protected VScrollBar vScroll;

	private ScrollBars _ScrollBars;

	private bool HasThumbs;

	protected Rectangle ClientArea
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Width -= vScroll.Width;
			clientRectangle.Height -= hScroll.Height;
			return clientRectangle;
		}
	}

	public bool LeftThumbVisible
	{
		get
		{
			return LeftThumb.Visible;
		}
		set
		{
			LeftThumb.Visible = value;
			DoResize();
		}
	}

	public bool TopThumbVisible
	{
		get
		{
			return TopThumb.Visible;
		}
		set
		{
			TopThumb.Visible = value;
			DoResize();
		}
	}

	[Browsable(false)]
	public int VisibleClientHeight
	{
		get
		{
			if (hScroll.Visible)
			{
				return base.ClientHeight - hScroll.Height;
			}
			return base.ClientHeight;
		}
	}

	[Browsable(false)]
	public int VisibleClientWidth
	{
		get
		{
			if (hScroll.Visible)
			{
				return base.ClientWidth - vScroll.Width;
			}
			return base.ClientWidth;
		}
	}

	public ScrollBars ScrollBars
	{
		get
		{
			return _ScrollBars;
		}
		set
		{
			_ScrollBars = value;
			if (ScrollBars == ScrollBars.Both)
			{
				hScroll.Visible = true;
				vScroll.Visible = true;
			}
			if (ScrollBars == ScrollBars.None)
			{
				hScroll.Visible = false;
				vScroll.Visible = false;
			}
			if (ScrollBars == ScrollBars.Horizontal)
			{
				hScroll.Visible = true;
				vScroll.Visible = false;
			}
			if (ScrollBars == ScrollBars.Vertical)
			{
				hScroll.Visible = false;
				vScroll.Visible = true;
			}
			Filler.Visible = hScroll.Visible & vScroll.Visible;
			if (vScroll.Visible && HasThumbs)
			{
				TopThumb.Height = 8;
			}
			else
			{
				TopThumb.Height = 0;
			}
			if (hScroll.Visible && HasThumbs)
			{
				LeftThumb.Width = 8;
			}
			else
			{
				LeftThumb.Width = 0;
			}
			DoResize();
			Refresh();
		}
	}

	public SplitViewChildControl()
	{
		InitializeComponent();
	}

	public void HideThumbs()
	{
		TopThumb.Height = 0;
		LeftThumb.Width = 0;
		HasThumbs = false;
		DoResize();
	}

	public void ShowThumbs()
	{
		TopThumb.Height = 8;
		LeftThumb.Width = 8;
		HasThumbs = true;
		DoResize();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		DoResize();
	}

	private void DoResize()
	{
		try
		{
			if (TopThumb == null)
			{
				return;
			}
			TopThumb.Width = SystemInformation.VerticalScrollBarWidth;
			LeftThumb.Height = SystemInformation.HorizontalScrollBarHeight;
			vScroll.Width = SystemInformation.VerticalScrollBarWidth;
			hScroll.Height = SystemInformation.HorizontalScrollBarHeight;
			if (TopThumbVisible)
			{
				vScroll.Top = TopThumb.Height;
				if (hScroll.Visible)
				{
					vScroll.Height = base.ClientHeight - hScroll.Height - TopThumb.Height;
				}
				else
				{
					vScroll.Height = base.ClientHeight - TopThumb.Height;
				}
			}
			else
			{
				if (hScroll.Visible)
				{
					vScroll.Height = base.ClientHeight - hScroll.Height;
				}
				else
				{
					vScroll.Height = base.ClientHeight;
				}
				vScroll.Top = 0;
			}
			if (LeftThumbVisible)
			{
				hScroll.Left = LeftThumb.Width;
				if (vScroll.Visible)
				{
					hScroll.Width = base.ClientWidth - vScroll.Width - LeftThumb.Width;
				}
				else
				{
					hScroll.Width = base.ClientWidth - LeftThumb.Width;
				}
			}
			else
			{
				if (vScroll.Visible)
				{
					hScroll.Width = base.ClientWidth - vScroll.Width;
				}
				else
				{
					hScroll.Width = base.ClientWidth;
				}
				hScroll.Left = 0;
			}
			vScroll.Left = base.ClientWidth - vScroll.Width;
			hScroll.Top = base.ClientHeight - hScroll.Height;
			LeftThumb.Left = 0;
			LeftThumb.Top = hScroll.Top;
			TopThumb.Left = vScroll.Left;
			TopThumb.Top = 0;
			Filler.Left = vScroll.Left;
			Filler.Top = hScroll.Top;
			Filler.Width = vScroll.Width;
			Filler.Height = hScroll.Height;
		}
		catch
		{
		}
	}

	private void InitializeComponent()
	{
		new System.Resources.ResourceManager(typeof(Alsing.Windows.Forms.CoreLib.SplitViewChildControl));
		this.hScroll = new System.Windows.Forms.HScrollBar();
		this.vScroll = new System.Windows.Forms.VScrollBar();
		this.Filler = new System.Windows.Forms.Panel();
		this.TopThumb = new Alsing.Windows.Forms.CoreLib.ThumbControl();
		this.LeftThumb = new Alsing.Windows.Forms.CoreLib.ThumbControl();
		base.SuspendLayout();
		this.hScroll.Location = new System.Drawing.Point(-4, 292);
		this.hScroll.Name = "hScroll";
		this.hScroll.Size = new System.Drawing.Size(440, 16);
		this.hScroll.TabIndex = 0;
		this.vScroll.Location = new System.Drawing.Point(440, 0);
		this.vScroll.Maximum = 300;
		this.vScroll.Minimum = 0;
		this.vScroll.Name = "vScroll";
		this.vScroll.Size = new System.Drawing.Size(16, 360);
		this.vScroll.TabIndex = 1;
		this.Filler.BackColor = System.Drawing.SystemColors.Control;
		this.Filler.Location = new System.Drawing.Point(64, 260);
		this.Filler.Name = "Filler";
		this.Filler.Size = new System.Drawing.Size(20, 20);
		this.Filler.TabIndex = 3;
		this.TopThumb.BackColor = System.Drawing.SystemColors.Control;
		this.TopThumb.Cursor = System.Windows.Forms.Cursors.HSplit;
		this.TopThumb.Location = new System.Drawing.Point(101, 17);
		this.TopThumb.Name = "TopThumb";
		this.TopThumb.Size = new System.Drawing.Size(16, 8);
		this.TopThumb.TabIndex = 3;
		this.TopThumb.Visible = false;
		this.LeftThumb.BackColor = System.Drawing.SystemColors.Control;
		this.LeftThumb.Cursor = System.Windows.Forms.Cursors.VSplit;
		this.LeftThumb.Location = new System.Drawing.Point(423, 17);
		this.LeftThumb.Name = "LeftThumb";
		this.LeftThumb.Size = new System.Drawing.Size(8, 16);
		this.LeftThumb.TabIndex = 3;
		this.LeftThumb.Visible = false;
		base.Controls.AddRange(new System.Windows.Forms.Control[5] { this.TopThumb, this.LeftThumb, this.Filler, this.vScroll, this.hScroll });
		base.Name = "SplitViewChildControl";
		base.Size = new System.Drawing.Size(456, 376);
		base.ResumeLayout(false);
	}
}
