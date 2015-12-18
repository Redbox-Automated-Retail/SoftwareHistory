using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseControl : IBrowseControl
{
	public enum BrowseFormat
	{
		Individual,
		Table,
		GroupTable
	}

	public enum BrowseLayoutDirection
	{
		LeftToRightThenTopToBottom,
		TopToBottomThenLeftToRight
	}

	public class Margin
	{
		public int Left { get; set; }

		public int Right { get; set; }

		public int Top { get; set; }

		public int Bottom { get; set; }
	}

	private enum NavigationButtonDirections
	{
		left,
		right
	}

	private Margin _margins = new Margin();

	private Margin _itemMargins = new Margin();

	private Color _backgroundColor = Color.FromArgb(229, 229, 229);

	private int _browseItemIndex;

	private bool _showPaddles = true;

	private bool _showNoResultsMessage = true;

	private List<IBrowseItem> _lastRenderedBrowseItems;

	public string BrowseSetName { get; set; }

	public List<IBrowseItem> Items { get; set; }

	public Action<int> OnPageNumberChanged { get; set; }

	public int RowCount { get; set; }

	public int ColumnCount { get; set; }

	public bool ShowBorderItems { get; set; }

	public bool ShowPageNumbers { get; set; }

	public bool ShowTitle { get; set; }

	public bool ShowSeparator { get; set; }

	public bool ShowBackground { get; set; }

	public bool ShowPaddleShadow { get; set; }

	public bool ShowPaddles
	{
		get
		{
			return _showPaddles;
		}
		set
		{
			_showPaddles = value;
		}
	}

	public bool ShowVacantItemBackground { get; set; }

	public bool ShowNoResultsMessage
	{
		get
		{
			return _showNoResultsMessage;
		}
		set
		{
			_showNoResultsMessage = value;
		}
	}

	public int SeparatorPadding { get; set; }

	public string NoResultsText { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public BrowseLayoutDirection LayoutDirection { get; set; }

	public Color BackgroundColor
	{
		get
		{
			return _backgroundColor;
		}
		set
		{
			_backgroundColor = value;
		}
	}

	public Margin Margins => _margins;

	public Margin ItemMargins => _itemMargins;

	public int PageCount => Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Items.Count) / (double)VisibleItemCount));

	public int CurrentPage
	{
		get
		{
			return GetPageOfBrowseItemIndex(_browseItemIndex);
		}
		set
		{
			_browseItemIndex = Math.Min(VisibleItemCount * (Math.Max(1, value) - 1), PageCount * VisibleItemCount);
		}
	}

	public List<IBrowseItem> LastRenderedBrowseItems => _lastRenderedBrowseItems;

	public int VisibleItemCount => RowCount * ColumnCount;

	private int ItemWidth => (Width - Margins.Left - Margins.Right - ColumnCount * (ItemMargins.Left + ItemMargins.Right)) / ColumnCount;

	private int ItemHeight => (Height - Margins.Top - Margins.Bottom - RowCount * (ItemMargins.Top + ItemMargins.Bottom)) / RowCount;

	public event BrowseItemEvent OnItemSelected;

	public event BrowseItemsEvent OnBrowseItemShown;

	public event BrowseItemActorEvent OnAddItemSelected;

	public event BrowseItemMappingEvent OnUpdateItemMapping;

	public event BrowseItemActionEvent OnItemAction;

	public string GetPrefixedActorName(string actorName)
	{
		return $"{(string.IsNullOrEmpty(BrowseSetName) ? null : $"{BrowseSetName}_")}{actorName}";
	}

	public void NotifyUpdateItemMapping(int actorIndex, IBrowseItem browseItem)
	{
		if (OnUpdateItemMapping != null)
		{
			OnUpdateItemMapping(actorIndex, browseItem);
		}
	}

	public void NotifyAddItemSelected(IBrowseItem browseItem, string actorName)
	{
		if (OnAddItemSelected != null)
		{
			OnAddItemSelected(browseItem, actorName);
		}
	}

	public void NotifyItemSelected(IBrowseItem browseItem)
	{
		if (OnItemSelected != null)
		{
			OnItemSelected(browseItem);
		}
	}

	public void NotifyBrowseItemsShown(List<IBrowseItem> browseItems)
	{
		if (OnBrowseItemShown != null)
		{
			OnBrowseItemShown(browseItems);
		}
	}

	public virtual void NotifyItemAction(IBrowseItem browseItem, string action)
	{
		if (OnItemAction != null)
		{
			OnItemAction(browseItem, action);
		}
	}

	public int GetPageOfBrowseItemIndex(int browseItemIndex)
	{
		return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(browseItemIndex + 1) / (double)VisibleItemCount));
	}

	public void ClearBrowseItemActors()
	{
		ProcessBrowseItems(delegate(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
		{
			ClearBrowseItemActors(browseItem, browseItemConfigurationInfo);
		});
	}

	public void Render()
	{
		RenderBackground();
		_lastRenderedBrowseItems = ProcessBrowseItems(delegate(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
		{
			RenderBrowseItemActors(browseItem, browseItemConfigurationInfo);
		});
		RenderNoResultsMessage();
		RenderNavigationButtons();
		RenderPageNumbers();
		NotifyBrowseItemsShown(_lastRenderedBrowseItems);
	}

	private void RenderNoResultsMessage()
	{
		string prefixedActorName = GetPrefixedActorName("image_filter_empty_background");
		string prefixedActorName2 = GetPrefixedActorName("filter_empty_text");
		ActorFunctions.RemoveActor(prefixedActorName);
		ActorFunctions.RemoveActor(prefixedActorName2);
		if (!ShowNoResultsMessage || (Items != null && Items.Count != 0))
		{
			return;
		}
		ActorFunctions.CreateActor(prefixedActorName, addToFront: false, X, Y, Width, Height, null, null, "image_filter_empty_background_no_notch", null, null, null, null, null, null, "Image", null);
		if (!string.IsNullOrEmpty(NoResultsText))
		{
			ISizeAndPosition sizeAndPosition = ComputueNoResultsTextSizeAndPosition();
			ActorFunctions.CreateActor(prefixedActorName2, addToFront: false, sizeAndPosition.X, sizeAndPosition.Y, sizeAndPosition.Width, sizeAndPosition.Height, NoResultsText, "font_bryant_bold_30", null, null, null, null, null, null, null, "Text", null);
			IActor actor = ActorFunctions.GetActor(prefixedActorName2);
			if (actor != null)
			{
				actor.VerticalAlignment = StringAlignment.Center;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.ForegroundColor = Color.FromArgb(50, 47, 47);
				Font font = new Font(actor.Font.FontFamily, sizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
			}
		}
	}

	private ISizeAndPosition ComputueNoResultsTextSizeAndPosition()
	{
		SizeAndPosition obj = new SizeAndPosition
		{
			X = X,
			Width = Width
		};
		int num = 501;
		int value = 107;
		int num2 = 30;
		double num3 = Convert.ToDouble(221) / (double)num;
		double num4 = Convert.ToDouble(value) / (double)num;
		double num5 = Convert.ToDouble(Height) / (double)num;
		((ISizeAndPosition)obj).Y = Y + Convert.ToInt32((double)Height * num3);
		((ISizeAndPosition)obj).Height = Convert.ToInt32((double)Height * num4);
		((ISizeAndPosition)obj).FontSize = Convert.ToSingle((double)num2 * num5);
		return obj;
	}

	private List<IBrowseItem> ProcessBrowseItems(Action<IBrowseItem, IBrowseItemConfigurationInfo> action)
	{
		PageBrowseItemCollection pageBrowseItems = GetPageBrowseItems(_browseItemIndex, LayoutDirection);
		int browseItemActorIndex = 0;
		List<IBrowseItem> processedBrowseItems = new List<IBrowseItem>();
		Action<int, int> action2 = delegate(int rowLoopIndex, int columnLoopIndex)
		{
			browseItemActorIndex++;
			IBrowseItemConfigurationInfo arg2 = CreateBrowseItemConfigurationInfo(rowLoopIndex, columnLoopIndex, browseItemActorIndex);
			IBrowseItem browseItem = pageBrowseItems.Get(rowLoopIndex, columnLoopIndex);
			action(browseItem, arg2);
			if (browseItem != null && browseItem.IncludeInShownCount)
			{
				processedBrowseItems.Add(browseItem);
			}
		};
		PerformActionAcrossRowsAndColumns(LayoutDirection, action2);
		if (ShowBorderItems)
		{
			PageBrowseItemCollection pageBrowseItems2 = GetPageBrowseItems(_browseItemIndex - VisibleItemCount, LayoutDirection);
			PageBrowseItemCollection pageBrowseItems3 = GetPageBrowseItems(_browseItemIndex + VisibleItemCount, LayoutDirection);
			for (int num = 0; num < RowCount; num++)
			{
				browseItemActorIndex++;
				IBrowseItemConfigurationInfo arg = CreateBrowseItemConfigurationInfo(num, -1, browseItemActorIndex);
				action(pageBrowseItems2.Get(num, ColumnCount - 1), arg);
				browseItemActorIndex++;
				arg = CreateBrowseItemConfigurationInfo(num, ColumnCount, browseItemActorIndex);
				action(pageBrowseItems3.Get(num, 0), arg);
			}
		}
		return processedBrowseItems;
	}

	public void ScrollLeft()
	{
		UpdateBrowseItemIndex(Math.Max(0, _browseItemIndex - VisibleItemCount));
	}

	public void ScrollRight()
	{
		UpdateBrowseItemIndex(Math.Min(_browseItemIndex + VisibleItemCount, Items.Count - 1));
	}

	private void RenderPageNumbers()
	{
		string prefixedActorName = GetPrefixedActorName("browse_pageNumberText");
		ActorFunctions.RemoveActor(prefixedActorName);
		if (Items != null && Items.Count > 0 && ShowPageNumbers)
		{
			ActorFunctions.CreateActor(prefixedActorName, addToFront: false, X, Y + 4, Width, 19, $"{CurrentPage} of {PageCount}", "font_akzidenz_bold_smaller", null, null, null, null, null, null, null, "Text", null);
			IActor actor = ActorFunctions.GetActor(prefixedActorName);
			if (actor != null)
			{
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.ForegroundColor = Color.FromArgb(60, 57, 57);
			}
		}
	}

	private PageBrowseItemCollection GetPageBrowseItems(int startingBrowseItemIndex, BrowseLayoutDirection browseLayoutDirection)
	{
		PageBrowseItemCollection result = new PageBrowseItemCollection();
		Action<int, int> action = delegate(int rowLoopIndex, int columnLoopIndex)
		{
			if (startingBrowseItemIndex >= 0 && startingBrowseItemIndex < Items.Count)
			{
				result.Add(rowLoopIndex, columnLoopIndex, Items[startingBrowseItemIndex]);
			}
			startingBrowseItemIndex++;
		};
		PerformActionAcrossRowsAndColumns(browseLayoutDirection, action);
		return result;
	}

	private void PerformActionAcrossRowsAndColumns(BrowseLayoutDirection browseLayoutDirection, Action<int, int> action)
	{
		if (browseLayoutDirection == BrowseLayoutDirection.TopToBottomThenLeftToRight)
		{
			for (int i = 0; i < ColumnCount; i++)
			{
				for (int j = 0; j < RowCount; j++)
				{
					action(j, i);
				}
			}
			return;
		}
		for (int k = 0; k < RowCount; k++)
		{
			for (int l = 0; l < ColumnCount; l++)
			{
				action(k, l);
			}
		}
	}

	private void RenderBackground()
	{
		string prefixedActorName = GetPrefixedActorName("browse_background");
		ActorFunctions.RemoveActor(prefixedActorName);
		if (ShowBackground)
		{
			ActorFunctions.CreateActor(prefixedActorName, addToFront: false, X, Y, Width, Height, null, null, null, null, null, null, null, null, null, "Background", null);
			IActor actor = ActorFunctions.GetActor(prefixedActorName);
			if (actor != null)
			{
				actor.BackgroundColor = _backgroundColor;
			}
		}
		string prefixedActorName2 = GetPrefixedActorName("paddle_shadow");
		ActorFunctions.RemoveActor(prefixedActorName2);
		if (ShowPaddleShadow)
		{
			ActorFunctions.CreateActor(prefixedActorName2, addToFront: false, X, Y, Width, Height, null, null, "image_paddle_shadow_black_gradient", null, null, null, null, null, null, "Image", null);
		}
	}

	private void RenderNavigationButtons()
	{
		if (!ShowPaddles)
		{
			return;
		}
		foreach (NavigationButtonDirections value in Enum.GetValues(typeof(NavigationButtonDirections)))
		{
			string prefixedActorName = GetPrefixedActorName($"{value}_arrow_button");
			ActorFunctions.RemoveActor(prefixedActorName);
			if ((value != NavigationButtonDirections.left || _browseItemIndex <= 0) && (value != NavigationButtonDirections.right || _browseItemIndex + VisibleItemCount >= Items.Count))
			{
				continue;
			}
			int num = Math.Min(306, Height);
			int num2 = 58;
			ActorFunctions.CreateActor(prefixedActorName, addToFront: false, (value == NavigationButtonDirections.left) ? X : (X + Width - num2), Y + Height / 2 - num / 2, num2, num, null, null, $"image_{value.ToString()}_navigation_arrow", null, null, null, null, null, null, "Image", "Enabled");
			ActorFunctions.SetActorOpacity(prefixedActorName, 1f);
			IActor actor = ActorFunctions.GetActor(prefixedActorName);
			if (actor != null)
			{
				if (value == NavigationButtonDirections.left)
				{
					actor.Hit += BrowseControlScrollLeft;
				}
				else
				{
					actor.Hit += BrowseControlScrollRight;
				}
			}
		}
	}

	private void BrowseControlScrollLeft(object sender, EventArgs e)
	{
		ScrollLeft();
	}

	private void BrowseControlScrollRight(object sender, EventArgs e)
	{
		ScrollRight();
	}

	private void UpdateBrowseItemIndex(int newBrowseItemIndex)
	{
		ClearBrowseItemActors();
		_browseItemIndex = newBrowseItemIndex;
		Render();
		if (OnPageNumberChanged != null)
		{
			OnPageNumberChanged(CurrentPage);
		}
	}

	private int GetXPosition(int columnIndex)
	{
		return X + _margins.Left + columnIndex * (_itemMargins.Left + ItemWidth + _itemMargins.Right) + _itemMargins.Left;
	}

	private int GetYPosition(int rowIndex)
	{
		return Y + _margins.Top + rowIndex * (_itemMargins.Top + ItemHeight + _itemMargins.Bottom) + _itemMargins.Top;
	}

	private void ClearBrowseItemActors(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		browseItem?.ClearActors(browseItemConfigurationInfo);
	}

	private void RenderBrowseItemActors(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		browseItem?.RenderActors(browseItemConfigurationInfo);
	}

	private IBrowseItemConfigurationInfo CreateBrowseItemConfigurationInfo(int rowIndex, int columnIndex, int browseItemActorIndex)
	{
		bool isBorderItem = columnIndex < 0 || columnIndex == ColumnCount;
		SizeAndPosition sizeAndPosition = new SizeAndPosition
		{
			X = GetXPosition(columnIndex),
			Y = GetYPosition(rowIndex),
			Width = ItemWidth,
			Height = ItemHeight
		};
		return new BrowseItemConfigurationInfo
		{
			BrowseControl = this,
			SizeAndPosition = sizeAndPosition,
			BrowseItemActorIndex = browseItemActorIndex,
			RowIndex = rowIndex,
			ColumnIndex = columnIndex,
			IsBorderItem = isBorderItem
		};
	}
}
