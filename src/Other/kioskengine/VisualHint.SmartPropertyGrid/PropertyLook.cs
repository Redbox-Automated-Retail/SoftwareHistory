using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public class PropertyLook
{
	protected bool mNoFeelForReadOnly;

	private PropertyValue _value;

	public bool NoFeelForReadOnly => mNoFeelForReadOnly;

	public PropertyValue Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public virtual string DisplayString => _value.ConvertValueToDisplayedString(_value.GetValue());

	public virtual void AttachProperty(PropertyEnumerator propEnum)
	{
	}

	public virtual void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		Value.OnDraw(graphics, valueRect, textColor, Value.Font, propEnum, drawAnotherString, multipleTexts, drawValueIn);
	}

	public virtual Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		return new Rectangle(valueRect.Location, Size.Empty);
	}

	public virtual string TransformDisplayString(string str)
	{
		return str;
	}

	public virtual string GetTooltipString(int line)
	{
		return DisplayString;
	}

	public virtual Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		return Win32Calls.GetTextExtent(graphics, DisplayString, _value.Font);
	}

	public virtual void ControlHeightMultiplier()
	{
	}
}
