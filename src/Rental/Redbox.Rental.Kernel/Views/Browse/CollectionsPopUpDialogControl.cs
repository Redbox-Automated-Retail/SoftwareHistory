using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class CollectionsPopUpDialogControl
{
	private const string DialogBoxActorName = "collections_popup_dialog_box";

	private const string DialogBoxTextActorName = "collections_popup_dialog_box_text";

	private const string SeeAllCollectionsButtonActorName = "collections_popup_see_all_collections";

	private const string TopHorizontalLineActorName = "collections_popup_top_horizontal_line";

	private const string BottomHorizontalLineActorName = "collections_popup_bottom_horizontal_line";

	private const string CancelButtonActorName = "collections_popup_cancel_button";

	private const string ModalBackgroundActorName = "collections_popup_modal_background";

	private BrowseControl _productBrowseControl;

	public Rectangle Position { get; set; }

	public List<IBrowseItem> BrowseItems { get; set; }

	public event EventHandler OnCancel;

	public event EventHandler OnSeeAllCollections;

	public event BrowseItemEvent OnCollectionSelected;

	public void Render()
	{
		RenderModalBackground();
		RenderDialogBox();
		RenderDialogBoxText();
		RenderHorizontalLines();
		RenderSeeAllCollectionsButton();
		RenderCancelButton();
		RenderBrowseControl();
	}

	private void RenderModalBackground()
	{
		IActor actor = ActorFunctions.CreateActor("collections_popup_modal_background", addToFront: false, 0, 0, 1024, 768, null, null, null, null, null, null, null, null, null, "Background", "Enabled");
		if (actor != null)
		{
			actor.BackgroundColor = Color.Black;
			actor.Opacity = 0.6f;
		}
	}

	private void RenderBrowseControl()
	{
		CreateBrowseView();
		_productBrowseControl.Render();
	}

	private void CreateBrowseView()
	{
		_productBrowseControl = new BrowseControl
		{
			BrowseSetName = "CollectionsPopupDialog",
			ColumnCount = Math.Min(2, BrowseItems.Count),
			RowCount = 1,
			Width = Position.Width - 60,
			Height = Position.Height - 200,
			Y = Position.Y + 96,
			X = Position.X + 30,
			Items = BrowseItems,
			ShowBorderItems = false,
			ShowBackground = false,
			ShowPageNumbers = false,
			ShowVacantItemBackground = false,
			ShowPaddles = false,
			ShowPaddleShadow = false,
			ShowNoResultsMessage = false
		};
		_productBrowseControl.ItemMargins.Left = 16;
		_productBrowseControl.ItemMargins.Right = 16;
		_productBrowseControl.ItemMargins.Top = 0;
		_productBrowseControl.ItemMargins.Bottom = 0;
		_productBrowseControl.OnItemSelected += _productBrowseControl_OnItemSelected;
	}

	private void _productBrowseControl_OnItemSelected(IBrowseItem browseItem)
	{
		if (OnCollectionSelected != null)
		{
			OnCollectionSelected(browseItem);
		}
	}

	private void RenderCancelButton()
	{
		IActor actor = ActorFunctions.CreateActor("collections_popup_cancel_button", addToFront: false, Position.X + Position.Width - 25, Position.Y - 25, 50, 50, "X", "font_akzidenz_bold_small", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", "Enabled");
		if (actor != null)
		{
			actor.CornerSize = new Size(25, 25);
			actor.StrokeWeight = 3f;
			actor.GradientAngle = 90f;
			actor.BorderColor = Color.FromArgb(151, 14, 22);
			actor.BackgroundColor = Color.FromArgb(80, 80, 80);
			actor.GradientTargetColor = Color.FromArgb(0, 0, 0);
			actor.ForegroundColor = Color.White;
			actor.VerticalAlignment = StringAlignment.Center;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.Hit += OnCancelButtonClick;
		}
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		if (OnCancel != null)
		{
			OnCancel(this, null);
		}
	}

	private void RenderHorizontalLines()
	{
		IActor actor = ActorFunctions.CreateActor("collections_popup_top_horizontal_line", addToFront: false, Position.X + 35, Position.Y + 70, Position.Width - 70, 3, null, null, null, null, null, null, null, null, null, "Background", null);
		if (actor != null)
		{
			actor.BackgroundColor = Color.Black;
		}
		actor = ActorFunctions.CreateActor("collections_popup_bottom_horizontal_line", addToFront: false, Position.X + 35, Position.Y + Position.Height - 80, Position.Width - 70, 3, null, null, null, null, null, null, null, null, null, "Background", null);
		if (actor != null)
		{
			actor.BackgroundColor = Color.Black;
		}
	}

	private void RenderSeeAllCollectionsButton()
	{
		int num = 300;
		IActor actor = ActorFunctions.CreateActor("collections_popup_see_all_collections", addToFront: false, Position.X + (Position.Width - num) / 2, Position.Y + Position.Height - 65, num, 50, BundleFunctions.GetDictionaryStringValue("strings", "collections_popup_see_all_collections"), "font_target_smaller", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", "Enabled");
		if (actor != null)
		{
			Font font = new Font(actor.Font.FontFamily, 16f, actor.Font.Style);
			actor.Font = font;
			actor.CornerSize = new Size(25, 25);
			actor.StrokeWeight = 2f;
			actor.GradientAngle = 90f;
			actor.BorderColor = Color.FromArgb(255, 192, 0);
			actor.BackgroundColor = Color.FromArgb(239, 174, 0);
			actor.GradientTargetColor = Color.FromArgb(231, 130, 0);
			actor.ForegroundColor = Color.Black;
			actor.VerticalAlignment = StringAlignment.Center;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.Hit += OnClickSeeAllCollections;
		}
	}

	private void OnClickSeeAllCollections(object sender, EventArgs e)
	{
		if (OnSeeAllCollections != null)
		{
			OnSeeAllCollections(this, null);
		}
	}

	public void ClearActors()
	{
		ActorFunctions.RemoveActor("collections_popup_modal_background");
		ActorFunctions.RemoveActor("collections_popup_dialog_box");
		ActorFunctions.RemoveActor("collections_popup_dialog_box_text");
		ActorFunctions.RemoveActor("collections_popup_see_all_collections");
		ActorFunctions.RemoveActor("collections_popup_top_horizontal_line");
		ActorFunctions.RemoveActor("collections_popup_bottom_horizontal_line");
		ActorFunctions.RemoveActor("collections_popup_cancel_button");
		if (_productBrowseControl != null)
		{
			_productBrowseControl.ClearBrowseItemActors();
		}
	}

	private void RenderDialogBox()
	{
		IActor actor = ActorFunctions.CreateActor("collections_popup_dialog_box", addToFront: false, Position.X, Position.Y, Position.Width, Position.Height, null, null, null, null, null, null, null, null, null, "Background, DrawBorder", null);
		if (actor != null)
		{
			actor.CornerSize = new Size(15, 15);
			actor.StrokeWeight = 6f;
			actor.BorderColor = Color.FromArgb(151, 14, 22);
			actor.BackgroundColor = Color.White;
		}
	}

	private void RenderDialogBoxText()
	{
		IActor actor = ActorFunctions.CreateActor("collections_popup_dialog_box_text", addToFront: false, Position.X, Position.Y + 20, Position.Width, 40, BundleFunctions.GetDictionaryStringValue("strings", "collections_popup_text"), "font_akzidenz_regular_medium", null, null, null, null, null, null, null, "Text", null);
		if (actor != null)
		{
			Font font = new Font(actor.Font.FontFamily, 17f, actor.Font.Style);
			actor.Font = font;
			actor.ForegroundColor = Color.Black;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.VerticalAlignment = StringAlignment.Center;
		}
	}
}
