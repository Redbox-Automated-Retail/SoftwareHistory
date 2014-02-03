using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class AlphaColorPicker : Control, IDropDownContent, IColorComponentDrawingImplementer
{
	private bool _alphaVisible;

	private CustomTrackBar _hueBar;

	private SaturationBrightnessMapPicker _sbMapPicker = new SaturationBrightnessMapPicker();

	private CustomTrackBar _alphaBar;

	private TextBox _rEdit = new TextBox();

	private TextBox _gEdit = new TextBox();

	private TextBox _bEdit = new TextBox();

	private TextBox _aEdit = new TextBox();

	private Label _rLabel = new Label();

	private Label _gLabel = new Label();

	private Label _bLabel = new Label();

	private Label _aLabel = new Label();

	private bool _preventReentrantCalls;

	private Color _color;

	private PropertyEnumerator _ownerPropertyEnumerator;

	protected bool AlphaVisible => _alphaVisible;

	protected CustomTrackBar HueBar => _hueBar;

	protected SaturationBrightnessMapPicker SbMapPicker => _sbMapPicker;

	protected CustomTrackBar AlphaBar => _alphaBar;

	protected TextBox REdit => _rEdit;

	protected TextBox GEdit => _gEdit;

	protected TextBox BEdit => _bEdit;

	protected TextBox AEdit => _aEdit;

	protected Label RLabel => _rLabel;

	protected Label GLabel => _gLabel;

	protected Label BLabel => _bLabel;

	protected Label ALabel => _aLabel;

	protected Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
		}
	}

	protected PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnumerator;
		}
		set
		{
			_ownerPropertyEnumerator = value;
		}
	}

	public int GetSelectedIndex()
	{
		return _ownerPropertyEnumerator.Property.Value.ImageIndex;
	}

	public AlphaColorPicker()
		: this(alphaVisible: true)
	{
	}

	public AlphaColorPicker(bool alphaVisible)
	{
		base.TabStop = false;
		_hueBar = new CustomTrackBar(this);
		_alphaBar = new CustomTrackBar(this);
		_alphaVisible = alphaVisible;
	}

	public virtual void InitializeContent(PropertyEnumerator propEnum, string currentValue, object valueKey)
	{
		_ownerPropertyEnumerator = propEnum;
		if (propEnum.Property.Value.HasMultipleValues)
		{
			_color = Color.Empty;
		}
		else
		{
			_color = (Color)propEnum.Property.Value.GetValue();
		}
		base.Width = (_alphaVisible ? 200 : 170);
		base.Height = 212;
		BackColor = SystemColors.Control;
		ColorUtils.RgbToHsb(_color, out var h, out var _, out var _);
		if (_alphaVisible)
		{
			_alphaBar.Bounds = new Rectangle(10, 10, 20, 100);
			_alphaBar.Margins = new Win32Calls.RECT(1, 1, 1, 1);
			_alphaBar.Orientation = Orientation.Vertical;
			_alphaBar.ReversedOrigin = true;
			_alphaBar.Value = _color.A;
			_alphaBar.ValueChanged += OnTrackBarValueChanged;
			_alphaBar.TabIndex = 1;
			_alphaBar.MouseUp += OnTrackBarMouseUp;
			_alphaBar.KeyUp += OnTrackBarKeyUp;
		}
		_hueBar.Bounds = new Rectangle(_alphaVisible ? 35 : 10, 10, _alphaVisible ? 155 : 150, 12);
		_hueBar.Margins = new Win32Calls.RECT(25, 1, 1, 1);
		_hueBar.Label = "Hue";
		_hueBar.Value = (int)Math.Round(h / 360.0 * 255.0);
		_hueBar.ValueChanged += OnTrackBarValueChanged;
		_hueBar.TabIndex = 2;
		_hueBar.MouseUp += OnTrackBarMouseUp;
		_hueBar.KeyUp += OnTrackBarKeyUp;
		_sbMapPicker.Color = _color;
		_sbMapPicker.Bounds = new Rectangle(_alphaVisible ? 40 : 10, 30, 150, 150);
		_sbMapPicker.ValueChanged += OnSbMapValueChanged;
		_sbMapPicker.TabIndex = 3;
		_sbMapPicker.MouseUp += OnSbMapMouseUp;
		_sbMapPicker.KeyUp += OnSbMapKeyUp;
		_rLabel.TextAlign = ContentAlignment.MiddleLeft;
		_rLabel.Bounds = new Rectangle(10, 185, 15, 19);
		_rLabel.Text = "R";
		_rEdit.AutoSize = false;
		_rEdit.Bounds = new Rectangle(25, 185, 26, 19);
		_rEdit.Text = _color.R.ToString();
		_rEdit.MaxLength = 3;
		_rEdit.KeyPress += OnEditKeyPress;
		_rEdit.TextChanged += OnEditTextChanged;
		_rEdit.TabIndex = 4;
		_gLabel.TextAlign = ContentAlignment.MiddleLeft;
		_gLabel.Bounds = new Rectangle(56, 185, 15, 19);
		_gLabel.Text = "G";
		_gEdit.AutoSize = false;
		_gEdit.Bounds = new Rectangle(71, 185, 26, 19);
		_gEdit.Text = _color.G.ToString();
		_gEdit.MaxLength = 3;
		_gEdit.KeyPress += OnEditKeyPress;
		_gEdit.TextChanged += OnEditTextChanged;
		_gEdit.TabIndex = 5;
		_bLabel.TextAlign = ContentAlignment.MiddleLeft;
		_bLabel.Bounds = new Rectangle(102, 185, 15, 19);
		_bLabel.Text = "B";
		_bEdit.AutoSize = false;
		_bEdit.Bounds = new Rectangle(117, 185, 26, 19);
		_bEdit.Text = _color.B.ToString();
		_bEdit.MaxLength = 3;
		_bEdit.KeyPress += OnEditKeyPress;
		_bEdit.TextChanged += OnEditTextChanged;
		_bEdit.TabIndex = 6;
		if (_alphaVisible)
		{
			_aLabel.TextAlign = ContentAlignment.MiddleLeft;
			_aLabel.Bounds = new Rectangle(148, 185, 15, 19);
			_aLabel.Text = "A";
			_aEdit.AutoSize = false;
			_aEdit.Bounds = new Rectangle(163, 185, 26, 19);
			_aEdit.Text = _color.A.ToString();
			_aEdit.MaxLength = 3;
			_aEdit.KeyPress += OnEditKeyPress;
			_aEdit.TextChanged += OnEditTextChanged;
			_aEdit.TabIndex = 7;
		}
		if (_alphaVisible)
		{
			base.Controls.Add(_alphaBar);
		}
		base.Controls.Add(_hueBar);
		base.Controls.Add(_sbMapPicker);
		base.Controls.Add(_rEdit);
		base.Controls.Add(_gEdit);
		base.Controls.Add(_bEdit);
		if (_alphaVisible)
		{
			base.Controls.Add(_aEdit);
		}
		base.Controls.Add(_rLabel);
		base.Controls.Add(_gLabel);
		base.Controls.Add(_bLabel);
		if (_alphaVisible)
		{
			base.Controls.Add(_aLabel);
		}
	}

	public object GetValue()
	{
		return _color;
	}

	public bool HasToBeClosedOnClick()
	{
		return false;
	}

	public void DrawContent(Control sender, Graphics graphics, Rectangle rect)
	{
		if (sender == _hueBar)
		{
			using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Color.Black, Color.Black, 0f, isAngleScaleable: false))
			{
				ColorBlend colorBlend = new ColorBlend();
				colorBlend.Colors = new Color[7]
				{
					Color.FromArgb(255, 0, 0),
					Color.FromArgb(255, 255, 0),
					Color.FromArgb(0, 255, 0),
					Color.FromArgb(0, 255, 255),
					Color.FromArgb(0, 0, 255),
					Color.FromArgb(255, 0, 255),
					Color.FromArgb(255, 0, 0)
				};
				colorBlend.Positions = new float[7] { 0f, 0.1667f, 0.3372f, 0.502f, 0.6686f, 0.8313f, 1f };
				linearGradientBrush.InterpolationColors = colorBlend;
				graphics.FillRectangle(linearGradientBrush, rect);
				return;
			}
		}
		if (sender != _alphaBar)
		{
			return;
		}
		Brush brush = new SolidBrush(Color.White);
		Brush brush2 = new SolidBrush(Color.Gray);
		for (int i = rect.Top; i <= rect.Bottom; i += 4)
		{
			int num = ((i + 4 > rect.Bottom) ? (rect.Bottom - i) : 4);
			for (int j = rect.Left; j <= rect.Right; j += 5)
			{
				int num2 = ((j + 5 > rect.Right) ? (rect.Right - j) : 5);
				if ((j - rect.Left) % 10 == (((i - rect.Top) % 8 != 0) ? 5 : 0))
				{
					graphics.FillRectangle(brush, j, i, num2, num);
				}
				else
				{
					graphics.FillRectangle(brush2, j, i, num2, num);
				}
			}
		}
		brush.Dispose();
		brush2.Dispose();
		using Brush brush3 = new LinearGradientBrush(rect, Color.FromArgb(255, _color), Color.FromArgb(0, _color), 90f);
		graphics.FillRectangle(brush3, rect);
	}

	protected void CommitChanges()
	{
		PropInPlaceList propInPlaceList = _ownerPropertyEnumerator.Property.ParentGrid.InPlaceControl as PropInPlaceList;
		if (propInPlaceList.RealtimeChange)
		{
			propInPlaceList.CommitChangesFromLiveDropDown();
		}
	}

	protected virtual void OnTrackBarKeyUp(object sender, KeyEventArgs e)
	{
		CommitChanges();
	}

	protected virtual void OnTrackBarMouseUp(object sender, MouseEventArgs e)
	{
		CommitChanges();
	}

	protected virtual void OnTrackBarValueChanged(object sender, EventArgs e)
	{
		if (!_preventReentrantCalls)
		{
			if (sender == _hueBar)
			{
				double hue = (double)_hueBar.Value / 255.0 * 359.0;
				_sbMapPicker.Hue = hue;
				OnSbMapValueChanged(sender, EventArgs.Empty);
			}
			else if (sender == _alphaBar)
			{
				_preventReentrantCalls = true;
				_color = Color.FromArgb(_alphaBar.Value, _color);
				_sbMapPicker.Color = _color;
				_aEdit.Text = _color.A.ToString();
				_preventReentrantCalls = false;
			}
		}
	}

	protected virtual void OnSbMapKeyUp(object sender, KeyEventArgs e)
	{
		CommitChanges();
	}

	protected virtual void OnSbMapMouseUp(object sender, MouseEventArgs e)
	{
		CommitChanges();
	}

	protected void OnSbMapValueChanged(object sender, EventArgs e)
	{
		_color = _sbMapPicker.Color;
		_alphaBar.Invalidate();
		_preventReentrantCalls = true;
		_rEdit.Text = _color.R.ToString();
		_gEdit.Text = _color.G.ToString();
		_bEdit.Text = _color.B.ToString();
		_aEdit.Text = _color.A.ToString();
		_preventReentrantCalls = false;
	}

	protected virtual void OnEditKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\b' && !char.IsDigit(e.KeyChar))
		{
			e.Handled = true;
		}
	}

	protected virtual void OnEditTextChanged(object sender, EventArgs e)
	{
		if (!_preventReentrantCalls)
		{
			TextBox textBox = sender as TextBox;
			int num = ((textBox.Text.Length > 0) ? int.Parse(textBox.Text) : 0);
			if (num > 255)
			{
				num = 255;
			}
			if (sender == _rEdit)
			{
				_color = Color.FromArgb(_color.A, num, _color.G, _color.B);
			}
			else if (sender == _gEdit)
			{
				_color = Color.FromArgb(_color.A, _color.R, num, _color.B);
			}
			else if (sender == _bEdit)
			{
				_color = Color.FromArgb(_color.A, _color.R, _color.G, num);
			}
			else if (sender == _aEdit)
			{
				_color = Color.FromArgb(num, _color.R, _color.G, _color.B);
			}
			_alphaBar.Value = _color.A;
			_sbMapPicker.ResetColor();
			_sbMapPicker.Color = _color;
			ColorUtils.RgbToHsb(_color, out var h, out var _, out var _);
			_preventReentrantCalls = true;
			_hueBar.Value = (int)Math.Round(h / 360.0 * 255.0);
			_preventReentrantCalls = false;
			CommitChanges();
		}
	}
}
