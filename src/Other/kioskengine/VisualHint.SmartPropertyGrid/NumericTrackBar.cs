using System;
using System.ComponentModel;
using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class NumericTrackBar : GenericTrackBar
{
	public override void ResetSettings()
	{
		mMinimum = Convert.ChangeType(0, _baseType);
		mMaximum = Convert.ChangeType(100, _baseType);
		mSmallChange = Convert.ChangeType(1, _baseType);
		mLargeChange = Convert.ChangeType(5, _baseType);
		mResolution = Convert.ChangeType(1, _baseType);
		mTickFrequency = Convert.ChangeType(10, _baseType);
		mValue = Convert.ChangeType(0, _baseType);
	}

	public NumericTrackBar(Type baseType)
		: base(baseType)
	{
		ResetSettings();
	}

	public NumericTrackBar()
		: base(typeof(double))
	{
		ResetSettings();
	}

	protected override int GetIntegerSteps()
	{
		decimal num = (Convert.ToDecimal(mMaximum) - Convert.ToDecimal(mMinimum)) / Convert.ToDecimal(mResolution);
		return Convert.ToInt32(num);
	}

	protected override int ConvertStepTypeToInteger(object value)
	{
		return Convert.ToInt32(Convert.ToDecimal(value) / Convert.ToDecimal(mResolution));
	}

	protected override int ConvertBaseTypeToInteger(object value)
	{
		decimal num = Convert.ToDecimal(Maximum) - Convert.ToDecimal(Minimum);
		if (num == 0m)
		{
			return 0;
		}
		return Convert.ToInt32((decimal)GetIntegerSteps() * (Convert.ToDecimal(value) - Convert.ToDecimal(Minimum)) / num);
	}

	protected override object ConvertIntegerToBaseType(int position)
	{
		int integerSteps = GetIntegerSteps();
		if (integerSteps == 0)
		{
			return Convert.ToDecimal(Minimum);
		}
		return Convert.ChangeType(Convert.ToDecimal(Minimum) + (decimal)position * (Convert.ToDecimal(Maximum) - Convert.ToDecimal(Minimum)) / (decimal)integerSteps, _baseType);
	}

	public override object GetValueMinusSmallChange()
	{
		return Convert.ChangeType(Convert.ToDecimal(Value) - Convert.ToDecimal(SmallChange), _baseType);
	}

	public override object GetValuePlusSmallChange()
	{
		return Convert.ChangeType(Convert.ToDecimal(Value) + Convert.ToDecimal(SmallChange), _baseType);
	}

	public override object GetValueMinusLargeChange()
	{
		return Convert.ChangeType(Convert.ToDecimal(Value) - Convert.ToDecimal(LargeChange), _baseType);
	}

	public override object GetValuePlusLargeChange()
	{
		return Convert.ChangeType(Convert.ToDecimal(Value) + Convert.ToDecimal(LargeChange), _baseType);
	}

	public override int GetMaxWidth(Graphics graphics, Font font)
	{
		decimal num = Convert.ToDecimal(Minimum);
		decimal num2 = Convert.ToDecimal(Maximum);
		decimal num3 = Convert.ToDecimal(Resolution);
		int num4 = Win32Calls.GetTextExtent(graphics, ".", font).Width;
		int num5 = Win32Calls.GetTextExtent(graphics, ((int)num).ToString(), font).Width;
		int num6 = 0;
		if (num3 < 1.0m)
		{
			int num7 = Win32Calls.GetTextExtent(graphics, new string('0', Math.Abs(num3).ToString().Length - 2), font).Width;
			if (num7 > num6)
			{
				num6 = num7;
			}
		}
		num5 += num6 + ((num6 > 0) ? num4 : 0);
		int num8 = Win32Calls.GetTextExtent(graphics, ((int)num2).ToString(), font).Width;
		num6 = 0;
		if (num3 < 1.0m)
		{
			int num9 = Win32Calls.GetTextExtent(graphics, new string('0', Math.Abs(num3).ToString().Length - 2), font).Width;
			if (num9 > num6)
			{
				num6 = num9;
			}
		}
		num8 += num6 + ((num6 > 0) ? num4 : 0);
		if (num5 < num8)
		{
			num5 = num8;
		}
		return num5;
	}
}
