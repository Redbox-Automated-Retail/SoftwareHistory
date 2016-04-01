using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.Browse;

public class AccessibilityControl
{
	private BrowseControl _productBrowseControl;

	private const int _addToCartLabelHeight = 26;

	private const int _browseControlWidth = 375;

	public string BrowseSetName { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public BrowseControl SourceBrowseControl { get; set; }

	public TitleFamily SelectedTitleFamily { get; set; }

	public List<IBrowseItem> Items { get; set; }

	private string AddToCartLabelActorName => _productBrowseControl.GetPrefixedActorName("add_to_cart_ada_label");

	private string LeftBorderActorName => _productBrowseControl.GetPrefixedActorName("left_ada_border");

	private string MiddleBorderActorName => _productBrowseControl.GetPrefixedActorName("middle_ada_border");

	private string RightBorderActorName => _productBrowseControl.GetPrefixedActorName("right_ada_border");

	private string SwitchProductFamilyButtonActorName => "tab_switch_product_family";

	public event BrowseItemActorEvent OnAddItemSelected;

	public event EventHandler OnSwitchProductFamily;

	public void ClearActors()
	{
		ActorFunctions.RemoveActor(AddToCartLabelActorName);
		ActorFunctions.RemoveActor(LeftBorderActorName);
		ActorFunctions.RemoveActor(MiddleBorderActorName);
		ActorFunctions.RemoveActor(RightBorderActorName);
		ActorFunctions.RemoveActor(SwitchProductFamilyButtonActorName);
		_productBrowseControl.ClearBrowseItemActors();
	}

	public void Render()
	{
		CreateBrowseControl();
		_productBrowseControl.Render();
		RenderAddToCartLabel();
		RenderBorders();
		RenderSwitchProductFamilyButton();
	}

	private void RenderSwitchProductFamilyButton()
	{
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", (SelectedTitleFamily == TitleFamily.Movies) ? "To_Games" : "To_Movies");
		ActorFunctions.CreateActor(SwitchProductFamilyButtonActorName, addToFront: false, X + 375 + 20, Y + 14, 106, 106, dictionaryStringValue, "font_akzidenz_bold_smaller", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", "Enabled");
		IActor actor = ActorFunctions.GetActor(SwitchProductFamilyButtonActorName);
		if (actor != null)
		{
			actor.CornerSize = new Size(52, 52);
			actor.GradientAngle = 90f;
			actor.BorderColor = Color.FromArgb(134, 12, 19);
			actor.BackgroundColor = Color.FromArgb(181, 18, 27);
			actor.GradientTargetColor = Color.FromArgb(143, 14, 21);
			actor.ForegroundColor = Color.White;
			actor.StrokeWeight = 3f;
			actor.VerticalAlignment = StringAlignment.Center;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.Hit += OnHitSwitchProductFamilyButton;
		}
	}

	private void OnHitSwitchProductFamilyButton(object sender, EventArgs e)
	{
		if (OnSwitchProductFamily != null)
		{
			OnSwitchProductFamily(this, null);
		}
	}

	private void RenderBorders()
	{
		ActorFunctions.CreateActor(LeftBorderActorName, addToFront: false, X, Y, 1, Height, null, null, null, null, null, null, null, null, null, "DrawBorder", null);
		IActor actor = ActorFunctions.GetActor(LeftBorderActorName);
		if (actor != null)
		{
			actor.BorderColor = Color.FromArgb(78, 78, 78);
		}
		ActorFunctions.CreateActor(MiddleBorderActorName, addToFront: false, X + 375, Y, 1, Height, null, null, null, null, null, null, null, null, null, "DrawBorder", null);
		actor = ActorFunctions.GetActor(MiddleBorderActorName);
		if (actor != null)
		{
			actor.BorderColor = Color.FromArgb(78, 78, 78);
		}
		ActorFunctions.CreateActor(RightBorderActorName, addToFront: false, X + Width, Y, 1, Height, null, null, null, null, null, null, null, null, null, "DrawBorder", null);
		actor = ActorFunctions.GetActor(RightBorderActorName);
		if (actor != null)
		{
			actor.BorderColor = Color.FromArgb(78, 78, 78);
		}
	}

	private void RenderAddToCartLabel()
	{
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "add_to_cart_ada_label");
		ActorFunctions.CreateActor(AddToCartLabelActorName, addToFront: false, X, Y + Height - 26, 375, 26, dictionaryStringValue, "font_akzidenz_regular_smaller", null, null, null, null, null, null, null, "Text", null);
		IActor actor = ActorFunctions.GetActor(AddToCartLabelActorName);
		if (actor != null)
		{
			actor.ForegroundColor = Color.FromArgb(234, 234, 234);
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.VerticalAlignment = StringAlignment.Center;
		}
	}

	private void CreateBrowseControl()
	{
		_productBrowseControl = new BrowseControl
		{
			BrowseSetName = string.Format("{0}_{1}", BrowseSetName, "ada"),
			ColumnCount = SourceBrowseControl.ColumnCount,
			RowCount = SourceBrowseControl.RowCount,
			Width = 375,
			Height = Height - 26,
			Y = Y,
			X = X,
			Items = GetBrowseItemsFromSourceBrowseControl(),
			ShowBorderItems = false,
			ShowBackground = false,
			ShowPageNumbers = false,
			ShowVacantItemBackground = false,
			ShowSeparator = false,
			ShowPaddleShadow = false,
			ShowPaddles = false,
			ShowNoResultsMessage = false,
			CurrentPage = SourceBrowseControl.CurrentPage
		};
		_productBrowseControl.ItemMargins.Left = 10;
		_productBrowseControl.ItemMargins.Right = 10;
		_productBrowseControl.ItemMargins.Top = 10;
		_productBrowseControl.ItemMargins.Bottom = 0;
		_productBrowseControl.OnAddItemSelected += _productBrowseControl_OnAddItemSelected;
	}

	private void _productBrowseControl_OnAddItemSelected(IBrowseItem browseItem, string imageActorName)
	{
		if (OnAddItemSelected != null)
		{
			OnAddItemSelected(browseItem, imageActorName);
		}
	}

	private List<IBrowseItem> GetBrowseItemsFromSourceBrowseControl()
	{
		List<IBrowseItem> list = new List<IBrowseItem>();
		if (SourceBrowseControl != null)
		{
			foreach (IBrowseItem item in SourceBrowseControl.Items)
			{
				if (item is MovieOrGameBrowseItem movieOrGameBrowseItem)
				{
					list.Add(new MovieOrGameBrowseItem(movieOrGameBrowseItem.LuaObject)
					{
						Name = movieOrGameBrowseItem.Name,
						ShowAddButton = true,
						IsSelected = (movieOrGameBrowseItem.IsSelected || !movieOrGameBrowseItem.ShowAddButton),
						ImageOpacity = movieOrGameBrowseItem.ImageOpacity,
						ShowIndexOfAddButton = movieOrGameBrowseItem.ShowIndexOfAddButton,
						Data = movieOrGameBrowseItem.Data,
						IsInStock = movieOrGameBrowseItem.IsInStock,
						AllowAddEvenIfSelected = movieOrGameBrowseItem.AllowAddEvenIfSelected,
						ImageName = null
					});
				}
			}
		}
		return list;
	}
}
