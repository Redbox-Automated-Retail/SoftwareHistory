using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Models;

public class SimpleModel : BaseModel<SimpleModel>
{
	public static readonly DependencyProperty BannerImageProperty = BaseModel<SimpleModel>.CreateDependencyProperty("BannerImage", TYPES.BITMAPIMAGE);

	public static readonly DependencyProperty TitleTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("TitleText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty MessageTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("MessageText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CommentTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("CommentText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING, string.Empty);

	public static readonly DependencyProperty ContinueButtonTextProperty = BaseModel<SimpleModel>.CreateDependencyProperty("ContinueButtonText", TYPES.STRING, string.Empty);

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand ContinueButtonCommand { get; set; }

	public BitmapImage BannerImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(BannerImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerImageProperty, value);
			});
		}
	}

	public string TitleText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleTextProperty, value);
			});
		}
	}

	public string MessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MessageTextProperty, value);
			});
		}
	}

	public string CommentText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CommentTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CommentTextProperty, value);
			});
		}
	}

	public string CancelButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonTextProperty, value);
			});
		}
	}

	public string ContinueButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ContinueButtonTextProperty, value);
			});
		}
	}
}
