using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.CoreLib;

[ComVisible(true)]
public class SplitViewParentControl : BaseControl
{
	protected SplitViewChildControl _ActiveView;

	private long _ticks;

	public bool DisableScrollBars;

	protected bool DoOnce;

	protected SplitViewChildControl LowerLeft;

	protected SplitViewChildControl LowerRight;

	protected SplitViewControl splitView;

	protected SplitViewChildControl UpperLeft;

	protected SplitViewChildControl UpperRight;

	private List<SplitViewChildControl> _Views;

	private ScrollBars _ScrollBars;

	private bool _SplitView;

	protected List<SplitViewChildControl> Views
	{
		get
		{
			return _Views;
		}
		set
		{
			_Views = value;
		}
	}

	[Browsable(false)]
	public ActiveView ActiveView
	{
		get
		{
			if (_ActiveView == UpperLeft)
			{
				return ActiveView.TopLeft;
			}
			if (_ActiveView == UpperRight)
			{
				return ActiveView.TopRight;
			}
			if (_ActiveView == LowerLeft)
			{
				return ActiveView.BottomLeft;
			}
			if (_ActiveView == LowerRight)
			{
				return ActiveView.BottomRight;
			}
			return ActiveView.TopLeft;
		}
		set
		{
			if (value != ActiveView.BottomRight)
			{
				ActivateSplits();
			}
			if (value == ActiveView.TopLeft)
			{
				_ActiveView = UpperLeft;
			}
			if (value == ActiveView.TopRight)
			{
				_ActiveView = UpperRight;
			}
			if (value == ActiveView.BottomLeft)
			{
				_ActiveView = LowerLeft;
			}
			if (value == ActiveView.BottomRight)
			{
				_ActiveView = LowerRight;
			}
		}
	}

	[Browsable(false)]
	public int SplitviewV
	{
		get
		{
			return splitView.SplitviewV;
		}
		set
		{
			if (splitView != null)
			{
				splitView.SplitviewV = value;
			}
		}
	}

	[Browsable(false)]
	public int SplitviewH
	{
		get
		{
			return splitView.SplitviewH;
		}
		set
		{
			if (splitView != null)
			{
				splitView.SplitviewH = value;
			}
		}
	}

	[Description("Determines what Scrollbars should be visible")]
	[Category("Appearance")]
	[DefaultValue(ScrollBars.Both)]
	public ScrollBars ScrollBars
	{
		get
		{
			return _ScrollBars;
		}
		set
		{
			if (_Views == null)
			{
				return;
			}
			if (DisableScrollBars)
			{
				value = ScrollBars.None;
			}
			foreach (SplitViewChildControl view in _Views)
			{
				view.ScrollBars = value;
			}
			_ScrollBars = value;
		}
	}

	[Description("Determines if the controls should use splitviews")]
	[DefaultValue(true)]
	[Category("Appearance")]
	public bool SplitView
	{
		get
		{
			return _SplitView;
		}
		set
		{
			_SplitView = value;
			if (splitView != null)
			{
				if (!SplitView)
				{
					splitView.Visible = false;
					base.Controls.Add(LowerRight);
					LowerRight.HideThumbs();
					LowerRight.Dock = DockStyle.Fill;
				}
				else
				{
					splitView.Visible = true;
					splitView.LowerRight = LowerRight;
					LowerRight.Dock = DockStyle.None;
					LowerRight.ShowThumbs();
				}
			}
		}
	}

	[DefaultValue(BorderStyle.FixedSingle)]
	[Category("Appearance - Borders")]
	[Description("Gets or Sets the border styles of the split views.")]
	public BorderStyle ChildBorderStyle
	{
		get
		{
			return Views[0].BorderStyle;
		}
		set
		{
			foreach (SplitViewChildControl view in Views)
			{
				view.BorderStyle = value;
			}
		}
	}

	[DefaultValue(typeof(Color), "ControlDark")]
	[Description("Gets or Sets the border color of the split views.")]
	[Category("Appearance - Borders")]
	public Color ChildBorderColor
	{
		get
		{
			return Views[0].BorderColor;
		}
		set
		{
			foreach (SplitViewChildControl view in Views)
			{
				if (view != null)
				{
					view.BorderColor = value;
				}
			}
		}
	}

	public SplitViewParentControl()
	{
		OnCreate();
		InitializeComponent();
		InitializeComponentInternal();
		splitView.Resizing += SplitView_Resizing;
		splitView.HideLeft += SplitView_HideLeft;
		splitView.HideTop += SplitView_HideTop;
		LowerRight = GetNewView();
		LowerRight.AllowDrop = true;
		LowerRight.BorderColor = Color.White;
		LowerRight.BorderStyle = BorderStyle.None;
		LowerRight.Location = new Point(0, 0);
		LowerRight.Size = new Size(100, 100);
		Views = new List<SplitViewChildControl>();
		LowerRight.TopThumb.MouseDown += TopThumb_MouseDown;
		LowerRight.LeftThumb.MouseDown += LeftThumb_MouseDown;
		Views.Add(LowerRight);
		LowerRight.TopThumbVisible = true;
		LowerRight.LeftThumbVisible = true;
		splitView.Controls.Add(LowerRight);
		splitView.LowerRight = LowerRight;
		SplitView = true;
		ScrollBars = ScrollBars.Both;
		base.BorderStyle = BorderStyle.None;
		ChildBorderColor = SystemColors.ControlDark;
		ChildBorderStyle = BorderStyle.FixedSingle;
		BackColor = SystemColors.Window;
		base.Size = new Size(100, 100);
		_ActiveView = LowerRight;
	}

	private void InitializeComponent()
	{
	}

	public void ResetSplitview()
	{
		splitView.ResetSplitview();
	}

	private void SplitView_Resizing(object sender, EventArgs e)
	{
		LowerRight.TopThumbVisible = false;
		LowerRight.LeftThumbVisible = false;
	}

	private void SplitView_HideTop(object sender, EventArgs e)
	{
		LowerRight.TopThumbVisible = true;
	}

	private void SplitView_HideLeft(object sender, EventArgs e)
	{
		LowerRight.LeftThumbVisible = true;
	}

	protected virtual void ActivateSplits()
	{
		if (UpperLeft == null)
		{
			UpperLeft = GetNewView();
			UpperRight = GetNewView();
			LowerLeft = GetNewView();
			splitView.Controls.AddRange(new Control[3] { UpperLeft, LowerLeft, UpperRight });
			splitView.UpperRight = LowerLeft;
			splitView.UpperLeft = UpperLeft;
			splitView.LowerLeft = UpperRight;
			CreateViews();
		}
	}

	protected void TopThumb_MouseDown(object sender, MouseEventArgs e)
	{
		ActivateSplits();
		long num = DateTime.Now.Ticks - _ticks;
		_ticks = DateTime.Now.Ticks;
		if (num < 3000000)
		{
			splitView.Split5050h();
		}
		else
		{
			splitView.InvokeMouseDownh();
		}
	}

	protected void LeftThumb_MouseDown(object sender, MouseEventArgs e)
	{
		ActivateSplits();
		long num = DateTime.Now.Ticks - _ticks;
		_ticks = DateTime.Now.Ticks;
		if (num < 3000000)
		{
			splitView.Split5050v();
		}
		else
		{
			splitView.InvokeMouseDownv();
		}
	}

	protected virtual void OnCreate()
	{
	}

	protected virtual void CreateViews()
	{
		if (UpperRight != null)
		{
			Views.Add(UpperRight);
			Views.Add(UpperLeft);
			Views.Add(LowerLeft);
		}
	}

	protected virtual SplitViewChildControl GetNewView()
	{
		return null;
	}

	protected void View_Enter(object sender, EventArgs e)
	{
		_ActiveView = (SplitViewChildControl)sender;
	}

	protected void View_Leave(object sender, EventArgs e)
	{
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == 7 && _ActiveView != null)
		{
			_ActiveView.Focus();
		}
	}

	private void InitializeComponentInternal()
	{
		splitView = new SplitViewControl();
		SuspendLayout();
		splitView.BackColor = Color.Empty;
		splitView.Dock = DockStyle.Fill;
		splitView.LowerLeft = null;
		splitView.LowerRight = null;
		splitView.Name = "splitView";
		splitView.Size = new Size(248, 216);
		splitView.SplitviewH = -4;
		splitView.SplitviewV = -4;
		splitView.TabIndex = 0;
		splitView.Text = "splitView";
		splitView.UpperLeft = null;
		splitView.UpperRight = null;
		base.Controls.AddRange(new Control[1] { splitView });
		base.Name = "SplitViewParentControl";
		base.Size = new Size(248, 216);
		ResumeLayout(performLayout: false);
	}
}
