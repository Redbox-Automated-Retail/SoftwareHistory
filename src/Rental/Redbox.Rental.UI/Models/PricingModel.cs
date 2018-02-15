using System.Windows;

namespace Redbox.Rental.UI.Models;

public class PricingModel : DependencyObject
{
	public static readonly DependencyProperty DiscountValueTextProperty = DependencyProperty.Register("DiscountValueText", typeof(string), typeof(PricingModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SubtotalValueTextProperty = DependencyProperty.Register("SubtotalValueText", typeof(string), typeof(PricingModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TaxValueTextProperty = DependencyProperty.Register("TaxValueText", typeof(string), typeof(PricingModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TotalValueTextProperty = DependencyProperty.Register("TotalValueText", typeof(string), typeof(PricingModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty DiscountLineVisibilityProperty = DependencyProperty.Register("DiscountLineVisibility", typeof(Visibility), typeof(PricingModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty TaxLineVisibilityProperty = DependencyProperty.Register("TaxLineVisibility", typeof(Visibility), typeof(PricingModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public string DiscountLabelText { get; set; }

	public string SubtotalLabelText { get; set; }

	public string TaxLabelText { get; set; }

	public string TotalLabelText { get; set; }

	public Visibility DiscountLineVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(DiscountLineVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DiscountLineVisibilityProperty, value);
			});
		}
	}

	public string DiscountValueText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(DiscountValueTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DiscountValueTextProperty, value);
			});
		}
	}

	public string SubtotalValueText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SubtotalValueTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SubtotalValueTextProperty, value);
			});
		}
	}

	public string TaxValueText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TaxValueTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TaxValueTextProperty, value);
			});
		}
	}

	public string TotalValueText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TotalValueTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TotalValueTextProperty, value);
			});
		}
	}

	public Visibility TaxLineVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(TaxLineVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TaxLineVisibilityProperty, value);
			});
		}
	}
}
