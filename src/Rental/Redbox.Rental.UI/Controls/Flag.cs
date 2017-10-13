using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Redbox.Rental.UI.Controls;

public class Flag : UserControl
{
	public static readonly DependencyProperty FlagColorProperty = DependencyProperty.Register("FlagColor", typeof(Color), typeof(Flag), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty FlagGradientColorProperty = DependencyProperty.Register("FlagGradientColor", typeof(Color), typeof(Flag), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty FlagShadowColorProperty = DependencyProperty.Register("FlagShadowColor", typeof(Color), typeof(Flag), new FrameworkPropertyMetadata(Colors.Black));

	public Color FlagColor
	{
		get
		{
			return (Color)GetValue(FlagColorProperty);
		}
		set
		{
			SetValue(FlagColorProperty, value);
		}
	}

	public Color FlagGradientColor
	{
		get
		{
			return (Color)GetValue(FlagGradientColorProperty);
		}
		set
		{
			SetValue(FlagGradientColorProperty, value);
		}
	}

	public Color FlagShadowColor
	{
		get
		{
			return (Color)GetValue(FlagShadowColorProperty);
		}
		set
		{
			SetValue(FlagShadowColorProperty, value);
		}
	}
}
