using System;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.Browse;

public class CollectionBrowseItem : LuaBasedBrowseItem, ICollectionBrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	private IBrowseItemConfigurationInfo _browseItemConfigurationInfo;

	private ISizeAndPosition _imageSizeAndPosition;

	private ISizeAndPosition _totalSizeAndPosition;

	private const double _buttonOffsetPercentange = 0.12;

	public Image Image { get; set; }

	private int ButtonOffsetWidth => Convert.ToInt32((double)_totalSizeAndPosition.Width * 0.12);

	private string ImageActorName => $"collection_image_{_browseItemConfigurationInfo.BrowseItemActorIndex}";

	private string ButtonActorName => $"collection_button_{_browseItemConfigurationInfo.BrowseItemActorIndex}";

	public CollectionBrowseItem(LuaTable luaTable)
		: base(luaTable)
	{
	}

	public override void ClearActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		_browseItemConfigurationInfo = browseItemConfigurationInfo;
		RemoveOldActors();
	}

	public override void RenderActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		base.RenderActors(browseItemConfigurationInfo);
		_browseItemConfigurationInfo = browseItemConfigurationInfo;
		ConfigureImageActor();
		ConfigureButtonActor();
	}

	private void ConfigureButtonActor()
	{
		ISizeAndPosition sizeAndPosition = ComputeButtonSizeAndPosition();
		if (!_browseItemConfigurationInfo.IsBorderItem)
		{
			ActorFunctions.CreateActor(ButtonActorName, addToFront: false, sizeAndPosition.X, sizeAndPosition.Y, sizeAndPosition.Width, sizeAndPosition.Height, base.Name, "font_target_small_bold", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", (!_browseItemConfigurationInfo.IsBorderItem) ? "Enabled" : null);
			IActor actor = ActorFunctions.GetActor(ButtonActorName);
			if (actor != null)
			{
				Font font = new Font(actor.Font.FontFamily, sizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
				actor.TextRegion = ComputeButtonTextRegion(sizeAndPosition);
				actor.GradientAngle = 90f;
				actor.BorderColor = Color.FromArgb(134, 12, 19);
				actor.BackgroundColor = Color.FromArgb(181, 18, 27);
				actor.GradientTargetColor = Color.FromArgb(143, 14, 21);
				actor.ForegroundColor = Color.White;
				actor.StrokeWeight = 3f;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.CornerSize = new Size(sizeAndPosition.Height / 2, sizeAndPosition.Height / 2);
				actor.Hit += ItemSelected;
				actor.Tag = this;
			}
		}
	}

	private Rectangle? ComputeButtonTextRegion(ISizeAndPosition buttonSizeAndPosition)
	{
		int num = Convert.ToInt32((float)buttonSizeAndPosition.Width * 0.9f);
		return new Rectangle((buttonSizeAndPosition.Width - num) / 2, 0, num, buttonSizeAndPosition.Height);
	}

	private void ConfigureImageActor()
	{
		_totalSizeAndPosition = ComputeTotalSizeAndPosition();
		_imageSizeAndPosition = ComputeImageSizeAndPosition();
		if (Image == null)
		{
			return;
		}
		ActorFunctions.CreateActor(ImageActorName, addToFront: false, _imageSizeAndPosition.X, _imageSizeAndPosition.Y, _imageSizeAndPosition.Width, _imageSizeAndPosition.Height, base.Name, null, "image_movie_missing_box_art", null, null, null, null, null, null, "Image", (!_browseItemConfigurationInfo.IsBorderItem) ? "Enabled" : null);
		IActor actor = ActorFunctions.GetActor(ImageActorName);
		if (actor != null)
		{
			if (Image != null)
			{
				actor.Image = Image;
			}
			actor.Hit += ItemSelected;
			actor.Tag = this;
			if (_browseItemConfigurationInfo.IsBorderItem)
			{
				actor.Opacity = 0.8f;
			}
		}
	}

	private void ItemSelected(object sender, EventArgs e)
	{
		IActor actor = sender as IActor;
		if (actor.Tag != null && actor.Tag is ILuaBasedBrowseItem { LuaObject: not null } luaBasedBrowseItem)
		{
			_browseItemConfigurationInfo.BrowseControl.NotifyItemSelected(luaBasedBrowseItem);
		}
	}

	private ISizeAndPosition ComputeImageSizeAndPosition()
	{
		return new SizeAndPosition
		{
			X = _totalSizeAndPosition.X,
			Y = _totalSizeAndPosition.Y,
			Width = _totalSizeAndPosition.Width - ButtonOffsetWidth,
			Height = _totalSizeAndPosition.Height - ButtonOffsetWidth
		};
	}

	private ISizeAndPosition ComputeTotalSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = _browseItemConfigurationInfo.SizeAndPosition.Clone();
		if (Image != null)
		{
			int num = Convert.ToInt32((double)Image.Width * 0.12);
			int value = num + Image.Width;
			int num2 = num + Image.Height;
			double num3 = Convert.ToDouble(value) / (double)num2;
			double num4 = Convert.ToDouble(_browseItemConfigurationInfo.SizeAndPosition.Width) / Convert.ToDouble(_browseItemConfigurationInfo.SizeAndPosition.Height);
			if (num3 > num4)
			{
				sizeAndPosition.Height = Convert.ToInt32((double)sizeAndPosition.Width / num3);
				sizeAndPosition.Y += (_browseItemConfigurationInfo.SizeAndPosition.Height - sizeAndPosition.Height) / 2;
			}
			else
			{
				sizeAndPosition.Width = Convert.ToInt32((double)sizeAndPosition.Height * num3);
				sizeAndPosition.X += (_browseItemConfigurationInfo.SizeAndPosition.Width - sizeAndPosition.Width) / 2;
			}
		}
		return sizeAndPosition;
	}

	private ISizeAndPosition ComputeButtonSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = new SizeAndPosition();
		int num = 200;
		int num2 = 16;
		sizeAndPosition.Height = ButtonOffsetWidth * 2;
		sizeAndPosition.Y = _totalSizeAndPosition.Y + _totalSizeAndPosition.Height - sizeAndPosition.Height;
		sizeAndPosition.Width = Convert.ToInt32((double)_totalSizeAndPosition.Width * 0.85);
		sizeAndPosition.X = _totalSizeAndPosition.X + _totalSizeAndPosition.Width - sizeAndPosition.Width;
		double num3 = Convert.ToDouble(sizeAndPosition.Width) / (double)num;
		sizeAndPosition.FontSize = Convert.ToSingle((double)num2 * num3);
		return sizeAndPosition;
	}

	private void RemoveOldActors()
	{
		ActorFunctions.RemoveActor(ButtonActorName);
		ActorFunctions.RemoveActor(ImageActorName);
	}
}
