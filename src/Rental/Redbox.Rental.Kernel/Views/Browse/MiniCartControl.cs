using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class MiniCartControl
{
	private BrowseControl _productBrowseControl;

	private bool _addToCartLabelEnabled = true;

	public List<IBrowseItem> Items { get; set; }

	public string BrowseSetName { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public bool AddToCartLabelEnabled
	{
		get
		{
			return _addToCartLabelEnabled;
		}
		set
		{
			_addToCartLabelEnabled = value;
		}
	}

	private string LabelActorName => GetPrefixedActorName("add_to_cart_label");

	private string LabelImageActorName => GetPrefixedActorName("add_to_cart_label_image");

	public event BrowseItemEvent OnCancelItem;

	public void Render()
	{
		CreateMiniCartBrowseView();
		_productBrowseControl.Render();
		RenderAddToCartLabel();
	}

	public void ClearActors()
	{
		ActorFunctions.RemoveActor(LabelActorName);
		ActorFunctions.RemoveActor(LabelImageActorName);
		_productBrowseControl.ClearBrowseItemActors();
	}

	private string GetPrefixedActorName(string actorName)
	{
		return $"{(string.IsNullOrEmpty(BrowseSetName) ? null : $"{BrowseSetName}_")}{actorName}";
	}

	private void RenderAddToCartLabel()
	{
		if (_addToCartLabelEnabled && Items.Count((IBrowseItem x) => !string.IsNullOrEmpty(x.ImageName)) == 0)
		{
			string text = ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"];
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "add_to_cart_label");
			ActorFunctions.CreateActor(LabelActorName, addToFront: false, (text == "en-US") ? (X + 85) : (X + 40), Y + 38, (text == "en-US") ? 409 : 499, 41, dictionaryStringValue, "font_akzidenz_regular_medium", null, null, null, null, null, null, null, "Background, DrawBorder, Image, Text, TextShadow", null);
			IActor actor = ActorFunctions.GetActor(LabelActorName);
			if (actor != null)
			{
				actor.ForegroundColor = Color.Gray;
				actor.StrokeWeight = 1f;
				actor.Opacity = 0.8f;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.BackgroundColor = Color.FromArgb(50, 50, 50);
				actor.BorderColor = Color.FromArgb(80, 80, 80);
			}
			ActorFunctions.CreateActor(LabelImageActorName, addToFront: false, (text == "en-US") ? (X + 165) : (X + 175), Y + 39, 41, 40, null, null, "image_add_to_cart", null, null, null, null, null, null, "Image", null);
			actor = ActorFunctions.GetActor(LabelImageActorName);
			if (actor != null)
			{
				actor.Opacity = 0.9f;
			}
		}
	}

	private void CreateMiniCartBrowseView()
	{
		_productBrowseControl = new BrowseControl
		{
			BrowseSetName = string.Format("{0}_{1}", BrowseSetName, "minicart"),
			ColumnCount = 5,
			RowCount = 1,
			Width = Width,
			Height = Height,
			Y = Y,
			X = X,
			Items = Items,
			ShowBorderItems = false,
			ShowBackground = false,
			ShowPageNumbers = false,
			ShowVacantItemBackground = true
		};
		_productBrowseControl.ItemMargins.Left = 16;
		_productBrowseControl.ItemMargins.Right = 16;
		_productBrowseControl.ItemMargins.Top = 0;
		_productBrowseControl.ItemMargins.Bottom = 0;
		_productBrowseControl.OnItemAction += productBrowseControl_OnItemAction;
	}

	private void productBrowseControl_OnItemAction(IBrowseItem browseItem, string action)
	{
		if (action == ProductBrowseItemActions.Cancel.ToString() && OnCancelItem != null)
		{
			OnCancelItem(browseItem);
		}
	}
}
