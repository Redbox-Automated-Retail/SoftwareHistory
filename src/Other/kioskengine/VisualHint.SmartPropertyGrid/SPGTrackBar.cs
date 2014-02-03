using System;
using System.Collections;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class SPGTrackBar : NumericTrackBar
{
	private PropertyEnumerator _ownerPropertyEnum;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		set
		{
			_ownerPropertyEnum = value;
			ChangeType(_ownerPropertyEnum.Property.Value.UnderlyingType);
			TrackBarSettingsAttribute trackBarSettingsAttribute = (TrackBarSettingsAttribute)_ownerPropertyEnum.Property.Value.GetAttribute(typeof(TrackBarSettingsAttribute));
			if (trackBarSettingsAttribute == null && _ownerPropertyEnum.Parent.Property != null && _ownerPropertyEnum.Parent.Property.Value != null)
			{
				ArrayList attributes = _ownerPropertyEnum.Parent.Property.Value.GetAttributes(typeof(TrackBarSettingsAttribute));
				foreach (TrackBarSettingsAttribute item in attributes)
				{
					if (item.ChildPropertyName == _ownerPropertyEnum.Property.Name)
					{
						trackBarSettingsAttribute = item;
						break;
					}
				}
			}
			if (trackBarSettingsAttribute != null)
			{
				Resolution = trackBarSettingsAttribute.Resolution;
				SmallChange = trackBarSettingsAttribute.SmallChange;
				LargeChange = trackBarSettingsAttribute.LargeChange;
			}
			if (_ownerPropertyEnum.Property.Value.Validator is PropertyValidatorMinMax propertyValidatorMinMax)
			{
				Minimum = propertyValidatorMinMax.Min;
				Maximum = propertyValidatorMinMax.Max;
			}
			else
			{
				Minimum = 0;
				Maximum = 100;
			}
		}
	}

	public SPGTrackBar(Type baseType)
		: base(baseType)
	{
		AutoSize = false;
		base.TickStyle = TickStyle.None;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 20)
		{
			return;
		}
		if (m.Msg == 15 && base.Width < 15)
		{
			using (Graphics graphics = CreateGraphics())
			{
				_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
				return;
			}
		}
		base.WndProc(ref m);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (base.Width < 15)
		{
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}
}
