using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyFullWidthButtonLook : PropertyLook
{
	private string _buttonText = "...";

	public override void AttachProperty(PropertyEnumerator propEnum)
	{
		ButtonSettingsAttribute buttonSettingsAttribute = (ButtonSettingsAttribute)propEnum.Property.Value.GetAttribute(typeof(ButtonSettingsAttribute));
		if (buttonSettingsAttribute == null && propEnum.Parent.Property != null && propEnum.Parent.Property.Value != null)
		{
			ArrayList attributes = propEnum.Parent.Property.Value.GetAttributes(typeof(ButtonSettingsAttribute));
			foreach (ButtonSettingsAttribute item in attributes)
			{
				if (item.ChildPropertyName == propEnum.Property.Name)
				{
					buttonSettingsAttribute = item;
					break;
				}
			}
		}
		if (buttonSettingsAttribute != null)
		{
			_buttonText = buttonSettingsAttribute.ButtonText;
		}
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		PropertyValue value = propEnum.Property.Value;
		value.Grid.DrawManager.DrawButton(graphics, value.Grid, valueRect, drawPressed: false, drawHoverEffect: false, drawValueIn);
		if (ThemeRenderer.Enabled)
		{
			ThemeButton.PushButton.Draw(graphics, 1, valueRect);
			Win32Calls.DrawText(graphics, (_buttonText == null) ? value.DisplayString : _buttonText, ref valueRect, value.Font, value.Grid.DrawManager.GetButtonTextColor(), 37);
			return;
		}
		ControlPaint.DrawButton(graphics, valueRect, ButtonState.Normal);
		StringFormat stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		using Brush brush = new SolidBrush(value.Grid.DrawManager.GetButtonTextColor());
		graphics.DrawString((_buttonText == null) ? value.DisplayString : _buttonText, value.Font, brush, valueRect, stringFormat);
	}
}
