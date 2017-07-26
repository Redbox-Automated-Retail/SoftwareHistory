using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class DisplayProductModel : DependencyObject
{
	public enum AddButtonTypes
	{
		PlusSign,
		PlusSignWithText
	}

	public enum AddButtonPositions
	{
		LowerLeftCorner,
		OverhangLowerLeftCorner
	}

	public static readonly DependencyProperty AddButtonLabelTextProperty = DependencyProperty.Register("AddButtonLabelText", typeof(string), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(DisplayProductModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ImageBorderColorProperty = DependencyProperty.Register("ImageBorderColor", typeof(Color), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Colors.Gray));

	public static readonly DependencyProperty ImageBorderThicknessProperty = DependencyProperty.Register("ImageBorderThickness", typeof(Thickness), typeof(DisplayProductModel), new FrameworkPropertyMetadata(new Thickness(0.0))
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageBorderCornerRadiusProperty = DependencyProperty.Register("ImageBorderCornerRadius", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register("ImageOpacity", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonOpacityProperty = DependencyProperty.Register("AddButtonOpacity", typeof(double), typeof(DisplayProductModel), new FrameworkPropertyMetadata(1.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty AddButtonPositionProperty = DependencyProperty.Register("AddButtonPosition", typeof(AddButtonPositions), typeof(DisplayProductModel), new FrameworkPropertyMetadata(AddButtonPositions.LowerLeftCorner));

	public static readonly DependencyProperty AddButtonTypeProperty = DependencyProperty.Register("AddButtonType", typeof(AddButtonTypes), typeof(DisplayProductModel), new FrameworkPropertyMetadata(AddButtonTypes.PlusSign));

	public static readonly DependencyProperty AddButtonVisibilityProperty = DependencyProperty.Register("AddButtonVisibility", typeof(Visibility), typeof(DisplayProductModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public object Data { get; set; }

	public BitmapImage Image
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(ImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageProperty, value);
			});
		}
	}

	public Color ImageBorderColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Color)GetValue(ImageBorderColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderColorProperty, value);
			});
		}
	}

	public Thickness ImageBorderThickness
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(ImageBorderThicknessProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderThicknessProperty, value);
			});
		}
	}

	public double ImageBorderCornerRadius
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ImageBorderCornerRadiusProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageBorderCornerRadiusProperty, value);
			});
		}
	}

	public double AddButtonOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(AddButtonOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonOpacityProperty, value);
			});
		}
	}

	public string AddButtonLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AddButtonLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonLabelTextProperty, value);
			});
		}
	}

	public AddButtonPositions AddButtonPosition
	{
		get
		{
			return base.Dispatcher.Invoke(() => (AddButtonPositions)GetValue(AddButtonPositionProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonPositionProperty, value);
			});
		}
	}

	public AddButtonTypes AddButtonType
	{
		get
		{
			return base.Dispatcher.Invoke(() => (AddButtonTypes)GetValue(AddButtonTypeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonTypeProperty, value);
			});
		}
	}

	public double ImageOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ImageOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageOpacityProperty, value);
			});
		}
	}

	public Visibility AddButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddButtonVisibilityProperty, value);
			});
		}
	}

	public void ConfigureForCarousel()
	{
		ImageBorderColor = Color.FromRgb(60, 56, 57);
		ImageBorderThickness = new Thickness(2.0);
		ImageBorderCornerRadius = 5.0;
		ImageOpacity = 1.0;
		AddButtonOpacity = 1.0;
		AddButtonVisibility = Visibility.Collapsed;
		AddButtonType = AddButtonTypes.PlusSignWithText;
		AddButtonPosition = AddButtonPositions.OverhangLowerLeftCorner;
	}
}
