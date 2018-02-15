using System;
using System.Windows;

namespace Redbox.Rental.UI.Controls;

public class BaseUserControl : RentalUserControl
{
	public BaseUserControl()
	{
		base.DataContextChanged += DataContextHasChanged;
	}

	private void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DataContextHasChanged();
	}

	protected virtual void DataContextHasChanged()
	{
	}

	protected static DependencyProperty CreateDependencyProperty<TOwner>(string propertyName, Type propertyType, object defaultValue = null)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner), new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = true
		});
	}
}
