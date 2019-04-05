using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class ReviewListUserControl : BaseUserControl, IComponentConnector
{
	private static DependencyProperty ReviewModelsProperty = DependencyProperty.Register("ReviewModels", typeof(ObservableCollection<ReviewModel>), typeof(ReviewListUserControl), new PropertyMetadata(new ObservableCollection<ReviewModel>(), ReviewModelsChanged));

	private static readonly DependencyProperty CurrentPageReviewsProperty = DependencyProperty.Register("CurrentPageReviews", typeof(ObservableCollection<ReviewModel>), typeof(ReviewListUserControl), new PropertyMetadata(new ObservableCollection<ReviewModel>()));

	private static DependencyProperty CurrentPageNumberProperty = DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(ReviewListUserControl), new PropertyMetadata(1, CurrentPageNumberChanged));

	private static DependencyProperty CurrentPageCountProperty = DependencyProperty.Register("CurrentPageCount", typeof(int), typeof(ReviewListUserControl), new PropertyMetadata(1, CurrentPageNumberChanged));

	private static DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage", typeof(int), typeof(ReviewListUserControl), new PropertyMetadata(3, ItemsPerPageChanged));

	private static DependencyProperty NextPageClickedCommandProperty = DependencyProperty.Register("NextPageClickedCommand", typeof(DynamicRoutedCommand), typeof(ReviewListUserControl), new PropertyMetadata(null));

	private static DependencyProperty PreviousPageClickedCommandProperty = DependencyProperty.Register("PreviousPageClickedCommand", typeof(DynamicRoutedCommand), typeof(ReviewListUserControl), new PropertyMetadata(null));

	private static DependencyProperty PageNumberClickedCommandProperty = DependencyProperty.Register("PageNumberClickedCommand", typeof(DynamicRoutedCommand), typeof(ReviewListUserControl), new PropertyMetadata(null));

	private static DependencyProperty PageShownCommandProperty = DependencyProperty.Register("PageShownCommand", typeof(DynamicRoutedCommand), typeof(ReviewListUserControl), new PropertyMetadata(null));

	private static DependencyPropertyKey NumberOfPagesPropertyKey = DependencyProperty.RegisterReadOnly("NumberOfPages", typeof(int), typeof(ReviewListUserControl), new PropertyMetadata(0, NumberOfPagesChanged));

	public static readonly DependencyProperty NumberOfPagesProperty = NumberOfPagesPropertyKey.DependencyProperty;

	private static DependencyPropertyKey PageControlVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("PageControlVisibility", typeof(Visibility), typeof(ReviewListUserControl), new PropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty PageControlVisibilityProperty = PageControlVisibilityPropertyKey.DependencyProperty;

	private int _isUpdatingPage;

	public ObservableCollection<ReviewModel> ReviewModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<ReviewModel>)GetValue(ReviewModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReviewModelsProperty, value);
			});
		}
	}

	public ObservableCollection<ReviewModel> CurrentPageReviews
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<ReviewModel>)GetValue(CurrentPageReviewsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageReviewsProperty, value);
			});
		}
	}

	public int CurrentPageNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageNumberProperty, value);
			});
		}
	}

	public int CurrentPageCount
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageCountProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageCountProperty, value);
			});
		}
	}

	public int ItemsPerPage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(ItemsPerPageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ItemsPerPageProperty, value);
			});
		}
	}

	public DynamicRoutedCommand NextPageClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(NextPageClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NextPageClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PreviousPageClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PreviousPageClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PreviousPageClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PageNumberClickedCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageNumberClickedCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageNumberClickedCommandProperty, value);
			});
		}
	}

	public DynamicRoutedCommand PageShownCommand
	{
		get
		{
			return base.Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageShownCommandProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageShownCommandProperty, value);
			});
		}
	}

	public int NumberOfPages
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(NumberOfPagesProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(NumberOfPagesPropertyKey, value);
			});
		}
	}

	public Visibility PageControlVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PageControlVisibilityProperty));
		}
		private set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PageControlVisibilityPropertyKey, value);
			});
		}
	}

	public ReviewListUserControl()
	{
		InitializeComponent();
		PageControl.ButtonPressed += PageControl_ButtonPressed;
	}

	private void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		UpdatePageInfo();
	}

	private static void ReviewModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ReviewListUserControl reviewListUserControl)
		{
			reviewListUserControl.UpdatePageInfo();
		}
	}

	private void UpdatePageInfo()
	{
		if (Interlocked.CompareExchange(ref _isUpdatingPage, 1, 0) == 1)
		{
			return;
		}
		try
		{
			CurrentPageReviews = new ObservableCollection<ReviewModel>();
			if ((ReviewModels?.Count ?? 0) > 0)
			{
				NumberOfPages = (int)Math.Round((double)ReviewModels.Count / (double)ItemsPerPage + 0.4);
				CurrentPageNumber = Math.Min(CurrentPageNumber, NumberOfPages);
				CurrentPageNumber = Math.Max(CurrentPageNumber, 1);
				for (int i = 0; i < ItemsPerPage; i++)
				{
					int num = (CurrentPageNumber - 1) * ItemsPerPage + i;
					if (num <= ReviewModels.Count - 1)
					{
						CurrentPageReviews.Add(ReviewModels[num]);
					}
				}
			}
			else
			{
				NumberOfPages = 0;
				CurrentPageNumber = 0;
			}
			PageControlVisibility = ((NumberOfPages <= 1) ? Visibility.Collapsed : Visibility.Visible);
		}
		finally
		{
			_isUpdatingPage = 0;
		}
	}

	private void PageControl_ButtonPressed(object sender, PageButtonPressedArgs e)
	{
		switch (e.ButtonType)
		{
		case PageButtonType.LeftArrow:
			PreviousPageClickedCommand?.Execute(null);
			break;
		case PageButtonType.RightArrow:
			NextPageClickedCommand?.Execute(null);
			break;
		default:
			PageNumberClickedCommand?.Execute(e.NewPageNumber);
			break;
		}
		HandleWPFHit();
	}

	private static void CurrentPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ReviewListUserControl reviewListUserControl)
		{
			reviewListUserControl.UpdatePageInfo();
		}
	}

	private static void ItemsPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ReviewListUserControl reviewListUserControl)
		{
			if (reviewListUserControl.ItemsPerPage < 1)
			{
				reviewListUserControl.ItemsPerPage = 1;
			}
			reviewListUserControl.UpdatePageInfo();
		}
	}

	private static void NumberOfPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ReviewListUserControl reviewListUserControl)
		{
			reviewListUserControl.CurrentPageCount = reviewListUserControl.NumberOfPages;
		}
	}
}
