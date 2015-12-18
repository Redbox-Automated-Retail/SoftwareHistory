using System;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.Rental.Kernel.Views.Browse;

public class MovieOrGameBrowseItem(LuaTable luaTable) : LuaBasedBrowseItem(luaTable), IMovieOrGameBrowseItem, ILuaBasedBrowseItem, IBrowseItem
{
	private IBrowseItemConfigurationInfo _browseItemConfigurationInfo;

	private ISizeAndPosition _addImageSizeAndPosition;

	private ISizeAndPosition _imageSizeAndPosition;

	private ISizeAndPosition _flagSizeAndPosition;

	private ISizeAndPosition _bannerSizeAndPosition;

	private TitleActor _titleActor;

	private SeparatorActor _separatorActor;

	private Color _imageBorderColor = Color.FromArgb(60, 56, 57);

	private float _imageBorderStrokeWeight = 1f;

	private const int originalImageWidth = 148;

	private const int originalImageHeight = 205;

	private const int originalCommingSoonBannerHeight = 60;

	private const int originalCommingSoonBannerWidth = 148;

	private bool _isInStock = true;

	public bool ShowAddButton { get; set; }

	public bool AllowAddEvenIfSelected { get; set; }

	public bool ShowIndexOfAddButton { get; set; }

	public bool ShowCancelButton { get; set; }

	public bool ShowFlag { get; set; }

	public string FlagText { get; set; }

	public ProductFlag Flag { get; set; }

	public Banner Banner { get; set; }

	public string BannerText { get; set; }

	public string EmailOptinButtonText { get; set; }

	public bool IsComingSoonOptinEnabled { get; set; }

	public bool IsEmailReminderRegistered { get; set; }

	public bool IsInStock
	{
		get
		{
			return _isInStock;
		}
		set
		{
			_isInStock = value;
		}
	}

	public Color ImageBorderColor
	{
		get
		{
			return _imageBorderColor;
		}
		set
		{
			_imageBorderColor = value;
		}
	}

	public float ImageBorderStrokeWeight
	{
		get
		{
			return _imageBorderStrokeWeight;
		}
		set
		{
			_imageBorderStrokeWeight = value;
		}
	}

	private int OriginalButtonWidth
	{
		get
		{
			if (!IsEmailReminderRegistered)
			{
				return 139;
			}
			return 143;
		}
	}

	private int OriginalButtonHeight
	{
		get
		{
			if (!IsEmailReminderRegistered)
			{
				return 34;
			}
			return 43;
		}
	}

	public string RegisteredImageName
	{
		get
		{
			string text = null;
			if (!string.IsNullOrEmpty(base.ImageName) && DataServiceFunctions.DataServiceRegisterImage(base.ImageName, base.ImageName))
			{
				text = $"cache:{base.ImageName}";
			}
			if (text == null)
			{
				text = GetDefaultImageName();
			}
			return text;
		}
	}

	private string ImageActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"movie_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	private string ImageAddActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"add_movie_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	private string FlagActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"flag_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	private string BannerActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"banner_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	private string ButtonEmailReminderActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"button_email_reminder_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	private string CancelButtonActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"cancel_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

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
		ConfigureFlagActor();
		ConfigureTitleActor();
		ConfigureBannerActor();
		ConfigureButtonEmailReminderActor();
		ConfigureAddImageActor();
		ConfigureCancelActor();
		ConfigureSeparatorActor();
		UpdateItemMapping();
	}

	protected virtual string GetDefaultImageName()
	{
		return null;
	}

	private void ConfigureButtonEmailReminderActor()
	{
		if (Banner == null || Banner.BannerType != BannerType.ComingSoon || !IsComingSoonOptinEnabled || _browseItemConfigurationInfo.IsBorderItem)
		{
			return;
		}
		ISizeAndPosition sizeAndPosition = ComputeButtonEmailReminderSizeAndPosition();
		ActorFunctions.CreateActor(ButtonEmailReminderActorName, addToFront: false, sizeAndPosition.X, sizeAndPosition.Y, sizeAndPosition.Width, sizeAndPosition.Height, EmailOptinButtonText, "font_target_smaller", IsEmailReminderRegistered ? "image_button_reminder_set" : null, null, "white", null, null, null, null, IsEmailReminderRegistered ? "Image, Text" : "BackgroundGradient, DrawBorder, Text", "Enabled");
		IActor actor = ActorFunctions.GetActor(ButtonEmailReminderActorName);
		if (actor != null)
		{
			if (!IsEmailReminderRegistered)
			{
				actor.GradientAngle = 90f;
				actor.CornerSize = new Size(18, 18);
				actor.BorderColor = Color.FromArgb(134, 12, 19);
				actor.BackgroundColor = Color.FromArgb(181, 18, 27);
				actor.GradientTargetColor = Color.FromArgb(143, 14, 21);
				actor.StrokeWeight = 2f;
			}
			else
			{
				actor.TextRegion = ComputeButtonEmailReminderTextSizeAndPosition(sizeAndPosition);
			}
			Font font = new Font(actor.Font.FontFamily, sizeAndPosition.FontSize, actor.Font.Style);
			actor.Font = font;
			actor.HorizontalAlignment = ((!IsEmailReminderRegistered) ? StringAlignment.Center : StringAlignment.Near);
			actor.VerticalAlignment = StringAlignment.Center;
			actor.Tag = this;
			actor.Hit += ItemAction;
		}
	}

	private void ItemAction(object sender, EventArgs e)
	{
		IActor actor = sender as IActor;
		if (actor.Tag != null && actor.Tag is ILuaBasedBrowseItem { LuaObject: not null } luaBasedBrowseItem)
		{
			_browseItemConfigurationInfo.BrowseControl.NotifyItemAction(luaBasedBrowseItem, IsEmailReminderRegistered ? ProductBrowseItemActions.UnregisterEmailReminder.ToString() : ProductBrowseItemActions.RegisterEmailReminder.ToString());
		}
	}

	private void ConfigureBannerActor()
	{
		if (Banner != null && !_browseItemConfigurationInfo.IsBorderItem)
		{
			_bannerSizeAndPosition = ComputeBannerSizeAndPosition();
			ActorFunctions.CreateActor(BannerActorName, addToFront: false, _bannerSizeAndPosition.X, _bannerSizeAndPosition.Y, _bannerSizeAndPosition.Width, _bannerSizeAndPosition.Height, (BannerText != null) ? BannerText : Banner.Text, (Banner.BannerType == BannerType.ComingSoon) ? "font_target_smaller_11" : "font_akzidenz_bold_tiny", Banner.ImageName, null, "white", null, null, null, null, "Image, Text", null);
			IActor actor = ActorFunctions.GetActor(BannerActorName);
			if (actor != null)
			{
				Font font = new Font(actor.Font.FontFamily, _bannerSizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.VerticalAlignment = ((Banner.BannerType != BannerType.ComingSoon) ? StringAlignment.Center : StringAlignment.Near);
			}
		}
	}

	private ISizeAndPosition ComputeButtonEmailReminderSizeAndPosition()
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		int value = (IsEmailReminderRegistered ? 11 : 20);
		int value2 = (IsEmailReminderRegistered ? 2 : 4);
		int num = 12;
		double num2 = Convert.ToDouble(OriginalButtonWidth) / 148.0;
		double num3 = Convert.ToDouble(OriginalButtonHeight) / 60.0;
		double num4 = Convert.ToDouble(value) / 60.0;
		double num5 = Convert.ToDouble(value2) / 148.0;
		((ISizeAndPosition)sizeAndPosition).X = _bannerSizeAndPosition.X + Convert.ToInt32((double)_bannerSizeAndPosition.Width * num5);
		((ISizeAndPosition)sizeAndPosition).Width = Convert.ToInt32((double)_bannerSizeAndPosition.Width * num2);
		((ISizeAndPosition)sizeAndPosition).Y = _bannerSizeAndPosition.Y + Convert.ToInt32((double)_bannerSizeAndPosition.Height * num4);
		((ISizeAndPosition)sizeAndPosition).Height = Convert.ToInt32((double)_bannerSizeAndPosition.Height * num3);
		double num6 = Convert.ToDouble(((ISizeAndPosition)sizeAndPosition).Width) / (double)OriginalButtonWidth;
		((ISizeAndPosition)sizeAndPosition).FontSize = Convert.ToSingle((double)num * num6);
		return sizeAndPosition;
	}

	private Rectangle ComputeButtonEmailReminderTextSizeAndPosition(ISizeAndPosition buttonEmailReminderSizeAndPosition)
	{
		Rectangle result = default(Rectangle);
		int value = 43;
		double num = Convert.ToDouble(139) / (double)OriginalButtonWidth;
		double num2 = Convert.ToDouble(value) / (double)OriginalButtonHeight;
		double num3 = Convert.ToDouble(4) / (double)OriginalButtonHeight;
		double num4 = Convert.ToDouble(7) / (double)OriginalButtonWidth;
		result.X = Convert.ToInt32((double)buttonEmailReminderSizeAndPosition.Width * num4);
		result.Width = Convert.ToInt32((double)buttonEmailReminderSizeAndPosition.Width * num);
		result.Y = Convert.ToInt32((double)buttonEmailReminderSizeAndPosition.Height * num3);
		result.Height = Convert.ToInt32((double)buttonEmailReminderSizeAndPosition.Height * num2);
		return result;
	}

	private ISizeAndPosition ComputeBannerSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = new SizeAndPosition();
		int num = 10;
		if (Banner.BannerStyle == BannerStyle.Mini)
		{
			num = 14;
		}
		else if (Banner.BannerType == BannerType.ComingSoon)
		{
			num = 11;
		}
		sizeAndPosition.X = _imageSizeAndPosition.X;
		sizeAndPosition.Width = _imageSizeAndPosition.Width;
		int num2 = 40;
		if (Banner.BannerStyle == BannerStyle.Mini)
		{
			num2 = 20;
		}
		else if (Banner.BannerType == BannerType.ComingSoon)
		{
			num2 = 60;
		}
		double num3 = Convert.ToDouble(num2) / 205.0;
		sizeAndPosition.Height = Convert.ToInt32((double)_imageSizeAndPosition.Height * num3);
		sizeAndPosition.Y = _imageSizeAndPosition.Y + _imageSizeAndPosition.Height - sizeAndPosition.Height;
		double num4 = Convert.ToDouble(sizeAndPosition.Height) / (double)num2;
		sizeAndPosition.FontSize = Convert.ToSingle((double)num * num4);
		return sizeAndPosition;
	}

	private void ConfigureFlagActor()
	{
		if (ShowFlag && Flag != null && !_browseItemConfigurationInfo.IsBorderItem)
		{
			_flagSizeAndPosition = ComputeFlagImageSizeAndPosition();
			ActorFunctions.CreateActor(FlagActorName, addToFront: false, _flagSizeAndPosition.X, _flagSizeAndPosition.Y, _flagSizeAndPosition.Width, _flagSizeAndPosition.Height, (!string.IsNullOrEmpty(FlagText)) ? FlagText : Flag.Text, "font_akzidenz_bold_tiny", Flag.ImageName, null, "white", null, null, null, null, "Image, Text", null);
			IActor actor = ActorFunctions.GetActor(FlagActorName);
			if (actor != null)
			{
				Font font = new Font(actor.Font.FontFamily, _flagSizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
				actor.TextRegion = ComputeFlagTextSizeAndPosition();
				actor.HorizontalAlignment = StringAlignment.Center;
			}
		}
	}

	private ISizeAndPosition ComputeFlagImageSizeAndPosition()
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		int num = -17;
		int num2 = 140;
		int num3 = 10;
		double num4 = (double)_imageSizeAndPosition.Width / Convert.ToDouble(148);
		((ISizeAndPosition)sizeAndPosition).X = _imageSizeAndPosition.X + Convert.ToInt32((double)num * num4);
		((ISizeAndPosition)sizeAndPosition).Width = Convert.ToInt32((double)num2 * num4);
		int num5 = -19;
		int num6 = 30;
		double num7 = (double)_imageSizeAndPosition.Height / Convert.ToDouble(205);
		((ISizeAndPosition)sizeAndPosition).Y = _imageSizeAndPosition.Y + Convert.ToInt32((double)num5 * num7);
		((ISizeAndPosition)sizeAndPosition).Height = Convert.ToInt32((double)num6 * num7);
		((ISizeAndPosition)sizeAndPosition).FontSize = Convert.ToSingle((double)num3 * num4);
		return sizeAndPosition;
	}

	private Rectangle ComputeFlagTextSizeAndPosition()
	{
		int num = 3;
		int num2 = 3;
		int num3 = 118;
		int num4 = 20;
		double num5 = (double)_imageSizeAndPosition.Width / Convert.ToDouble(148);
		double num6 = (double)_imageSizeAndPosition.Height / Convert.ToDouble(205);
		return new Rectangle
		{
			X = Convert.ToInt32((double)num * num5),
			Y = Convert.ToInt32((double)num2 * num6),
			Width = Convert.ToInt32((double)num3 * num5),
			Height = Convert.ToInt32((double)num4 * num6)
		};
	}

	private void ConfigureSeparatorActor()
	{
		_separatorActor = new SeparatorActor(this, _browseItemConfigurationInfo);
		_separatorActor.ConfigureActor();
	}

	private void ConfigureTitleActor()
	{
		if (_browseItemConfigurationInfo.BrowseControl.ShowTitle)
		{
			IBrowseItemConfigurationInfo browseItemConfigurationInfo = _browseItemConfigurationInfo.Clone();
			browseItemConfigurationInfo.SizeAndPosition.X += _imageSizeAndPosition.Width + _imageSizeAndPosition.X - _browseItemConfigurationInfo.SizeAndPosition.X;
			browseItemConfigurationInfo.SizeAndPosition.Width -= _imageSizeAndPosition.Width;
			_titleActor = new TitleActor(this, browseItemConfigurationInfo);
			_titleActor.ConfigureActor();
			IActor actor = _titleActor.GetActor();
			if (actor != null)
			{
				actor.Hit += ItemSelected;
			}
		}
	}

	private void UpdateItemMapping()
	{
		if (!_browseItemConfigurationInfo.IsBorderItem)
		{
			_browseItemConfigurationInfo.BrowseControl.NotifyUpdateItemMapping(_browseItemConfigurationInfo.BrowseItemActorIndex, this);
		}
	}

	private void ConfigureCancelActor()
	{
		if (ShowCancelButton)
		{
			ISizeAndPosition sizeAndPosition = ComputeCancelImageWithTitleSizeAndPosition();
			ActorFunctions.CreateActor(CancelButtonActorName, addToFront: false, sizeAndPosition.X, sizeAndPosition.Y, sizeAndPosition.Width, sizeAndPosition.Height, "X", "font_akzidenz_bold_small", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text, TextShadow", "Enabled");
			IActor actor = ActorFunctions.GetActor(CancelButtonActorName);
			if (actor != null)
			{
				actor.CornerSize = new Size(sizeAndPosition.Width / 2, sizeAndPosition.Width / 2);
				actor.GradientAngle = 90f;
				actor.BorderColor = Color.FromArgb(162, 16, 24);
				actor.BackgroundColor = Color.FromArgb(80, 80, 80);
				actor.GradientTargetColor = Color.FromArgb(0, 0, 0);
				actor.StrokeWeight = 3f;
				actor.ForegroundColor = Color.White;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.HorizontalAlignment = StringAlignment.Center;
				Font font = new Font(actor.Font.FontFamily, sizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
				actor.Tag = this;
				actor.Hit += OnItemCancelled;
			}
		}
	}

	private void OnItemCancelled(object sender, EventArgs e)
	{
		if (sender is IActor { Tag: IBrowseItem tag })
		{
			_browseItemConfigurationInfo.BrowseControl.NotifyItemAction(tag, ProductBrowseItemActions.Cancel.ToString());
		}
	}

	private void ConfigureAddImageActor()
	{
		if (_browseItemConfigurationInfo.IsBorderItem || !ShowAddButton)
		{
			return;
		}
		if (_browseItemConfigurationInfo.BrowseControl.ShowTitle)
		{
			_addImageSizeAndPosition = ComputeAddImageWithTitleSizeAndPosition();
		}
		else if (ShowIndexOfAddButton)
		{
			if (base.ImageName == null)
			{
				_addImageSizeAndPosition = ComputeADAAccessibleAddImageSizeAndPosition();
			}
			else
			{
				_addImageSizeAndPosition = ComputeIndexedAddImageSizeAndPosition();
			}
		}
		else
		{
			_addImageSizeAndPosition = ComputeAddImageSizeAndPosition();
		}
		bool flag = !base.IsSelected || (AllowAddEvenIfSelected && IsInStock);
		string image = "image_add_to_cart_corner_shadow";
		if (ShowIndexOfAddButton)
		{
			image = (flag ? "image_round_red_button" : "image_round_gray_button");
		}
		else if (_browseItemConfigurationInfo.BrowseControl.ShowTitle)
		{
			image = "image_add_to_cart";
		}
		ActorFunctions.CreateActor(ImageAddActorName, addToFront: false, _addImageSizeAndPosition.X, _addImageSizeAndPosition.Y, _addImageSizeAndPosition.Width, _addImageSizeAndPosition.Height, _browseItemConfigurationInfo.BrowseItemActorIndex.ToString(), "font_akzidenz_bold_small", image, null, null, null, null, null, null, _browseItemConfigurationInfo.IsBorderItem ? null : (ShowIndexOfAddButton ? "Image, Text" : "Image"), (!_browseItemConfigurationInfo.IsBorderItem) ? "Enabled" : null);
		IActor actor = ActorFunctions.GetActor(ImageAddActorName);
		if (actor != null)
		{
			if (_browseItemConfigurationInfo.BrowseControl.ShowTitle)
			{
				actor.HotSpot = ComputeAddImageHotspotWithTitleSizeAndPosition(_addImageSizeAndPosition);
				actor.Hit += ItemSelected;
			}
			else
			{
				actor.HotSpot = ComputeAddImageHotspotSizeAndPosition(_addImageSizeAndPosition);
				actor.Hit += AddItemSelected;
			}
			if (ShowIndexOfAddButton)
			{
				Font font = new Font(actor.Font.FontFamily, _addImageSizeAndPosition.FontSize, actor.Font.Style);
				actor.Font = font;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.ForegroundColor = (flag ? Color.FromArgb(234, 234, 234) : Color.FromArgb(200, 200, 200));
				actor.TextRegion = ComputeIndexTextRegion();
			}
		}
		ActorFunctions.SetActorTag(ImageAddActorName, this);
	}

	private Rectangle? ComputeIndexTextRegion()
	{
		int num = 50;
		int num2 = 45;
		int num3 = ((_addImageSizeAndPosition.Width == 0) ? num : _addImageSizeAndPosition.Width);
		double num4 = Convert.ToDouble(num3) / (double)num;
		return new Rectangle(0, 0, num3, Convert.ToInt32((double)num2 * num4));
	}

	private void ConfigureImageActor()
	{
		_imageSizeAndPosition = ComputeImageSizeAndPosition();
		ActorFunctions.CreateActor(ImageActorName, addToFront: false, _imageSizeAndPosition.X, _imageSizeAndPosition.Y, _imageSizeAndPosition.Width, _imageSizeAndPosition.Height, base.Name, null, (base.ImageName != null) ? RegisteredImageName : null, null, null, null, null, null, null, (base.ImageName == null) ? "Background" : (_browseItemConfigurationInfo.IsBorderItem ? "Image" : "Image, DrawBorder"), (!_browseItemConfigurationInfo.IsBorderItem) ? "Enabled" : null);
		IActor actor = ActorFunctions.GetActor(ImageActorName);
		if (actor != null)
		{
			actor.Hit += ItemSelected;
			actor.StrokeWeight = _imageBorderStrokeWeight;
			actor.BorderColor = _imageBorderColor;
			actor.BackgroundColor = Color.FromArgb(30, 30, 30);
		}
		ActorFunctions.SetActorTag(ImageActorName, this);
		float num = base.ImageOpacity;
		if (_browseItemConfigurationInfo.IsBorderItem && num > 0.8f)
		{
			num = 0.8f;
		}
		ActorFunctions.SetActorOpacity(ImageActorName, num);
	}

	private void AddItemSelected(object sender, EventArgs e)
	{
		IActor actor = sender as IActor;
		if (actor.Tag != null && actor.Tag is ILuaBasedBrowseItem { LuaObject: not null } luaBasedBrowseItem && (!luaBasedBrowseItem.IsSelected || (AllowAddEvenIfSelected && IsInStock)))
		{
			string actorName = actor.Name.Replace("add_movie", "movie");
			_browseItemConfigurationInfo.BrowseControl.NotifyAddItemSelected(luaBasedBrowseItem, actorName);
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

	private ISizeAndPosition ComputeCancelImageWithTitleSizeAndPosition()
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		double num = Convert.ToDouble(102) / 148.0;
		double num2 = Convert.ToDouble(-18) / 205.0;
		((ISizeAndPosition)sizeAndPosition).X = _imageSizeAndPosition.X + Convert.ToInt32((double)_imageSizeAndPosition.Width * num);
		((ISizeAndPosition)sizeAndPosition).Y = _imageSizeAndPosition.Y + Convert.ToInt32((double)_imageSizeAndPosition.Height * num2);
		double num3 = Convert.ToDouble(75) / 205.0;
		((ISizeAndPosition)sizeAndPosition).Height = Convert.ToInt32((double)_imageSizeAndPosition.Height * num3);
		((ISizeAndPosition)sizeAndPosition).Width = ((ISizeAndPosition)sizeAndPosition).Height;
		float num4 = 32f;
		double num5 = Convert.ToDouble(_imageSizeAndPosition.Height) / 205.0;
		((ISizeAndPosition)sizeAndPosition).FontSize = Convert.ToSingle((double)num4 * num5);
		return sizeAndPosition;
	}

	private ISizeAndPosition ComputeADAAccessibleAddImageSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = _browseItemConfigurationInfo.SizeAndPosition.Clone();
		sizeAndPosition.Width = Math.Min(sizeAndPosition.Width, sizeAndPosition.Height);
		sizeAndPosition.Height = sizeAndPosition.Width;
		int num = 50;
		int num2 = 18;
		double num3 = Convert.ToDouble(sizeAndPosition.Width) / (double)num;
		sizeAndPosition.FontSize = Convert.ToSingle(num3 * (double)num2);
		return sizeAndPosition;
	}

	private ISizeAndPosition ComputeIndexedAddImageSizeAndPosition()
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		double num = Convert.ToDouble(100) / 148.0;
		double num2 = Convert.ToDouble(160) / 205.0;
		((ISizeAndPosition)sizeAndPosition).X = _imageSizeAndPosition.X + Convert.ToInt32((double)_imageSizeAndPosition.Width * num);
		((ISizeAndPosition)sizeAndPosition).Y = _imageSizeAndPosition.Y + Convert.ToInt32((double)_imageSizeAndPosition.Height * num2);
		int num3 = 50;
		double num4 = Convert.ToDouble(num3) / 205.0;
		((ISizeAndPosition)sizeAndPosition).Height = Convert.ToInt32((double)_imageSizeAndPosition.Height * num4);
		((ISizeAndPosition)sizeAndPosition).Width = ((ISizeAndPosition)sizeAndPosition).Height;
		int num5 = 18;
		double num6 = Convert.ToDouble(((ISizeAndPosition)sizeAndPosition).Width) / (double)num3;
		((ISizeAndPosition)sizeAndPosition).FontSize = Convert.ToSingle(num6 * (double)num5);
		return sizeAndPosition;
	}

	private ISizeAndPosition ComputeAddImageSizeAndPosition()
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		double num = Convert.ToDouble(71) / 148.0;
		double num2 = Convert.ToDouble(122) / 205.0;
		((ISizeAndPosition)sizeAndPosition).X = _imageSizeAndPosition.X + Convert.ToInt32((double)_imageSizeAndPosition.Width * num);
		((ISizeAndPosition)sizeAndPosition).Y = _imageSizeAndPosition.Y + Convert.ToInt32((double)_imageSizeAndPosition.Height * num2);
		double num3 = Convert.ToDouble(85) / 205.0;
		((ISizeAndPosition)sizeAndPosition).Height = Convert.ToInt32((double)_imageSizeAndPosition.Height * num3);
		((ISizeAndPosition)sizeAndPosition).Width = ((ISizeAndPosition)sizeAndPosition).Height;
		return sizeAndPosition;
	}

	private ISizeAndPosition ComputeAddImageWithTitleSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = new SizeAndPosition();
		int num = 40;
		_ = Convert.ToDouble(220) / (double)num;
		int num2 = 56;
		double num3 = Convert.ToDouble(9) / (double)num2;
		sizeAndPosition.Y = _imageSizeAndPosition.Y + Convert.ToInt32((double)_imageSizeAndPosition.Height * num3);
		double num4 = Convert.ToDouble(43) / (double)num2;
		sizeAndPosition.Height = Convert.ToInt32((double)_imageSizeAndPosition.Height * num4);
		sizeAndPosition.Width = sizeAndPosition.Height;
		sizeAndPosition.X = _browseItemConfigurationInfo.SizeAndPosition.X + _browseItemConfigurationInfo.SizeAndPosition.Width - sizeAndPosition.Width + 5;
		return sizeAndPosition;
	}

	private Rectangle ComputeAddImageHotspotWithTitleSizeAndPosition(ISizeAndPosition addImageSizeAndPosition)
	{
		int num = 10;
		return new Rectangle
		{
			X = num * -1,
			Y = num * -1,
			Width = addImageSizeAndPosition.Width + num * 2,
			Height = addImageSizeAndPosition.Height + num * 2
		};
	}

	private Rectangle ComputeAddImageHotspotSizeAndPosition(ISizeAndPosition addImageSizeAndPosition)
	{
		return new Rectangle
		{
			X = 0,
			Y = 0,
			Width = addImageSizeAndPosition.Width + 6,
			Height = addImageSizeAndPosition.Height + 2
		};
	}

	private ISizeAndPosition ComputeImageSizeAndPosition()
	{
		ISizeAndPosition sizeAndPosition = _browseItemConfigurationInfo.SizeAndPosition.Clone();
		if (!string.IsNullOrEmpty(base.ImageName) || _browseItemConfigurationInfo.BrowseControl.ShowVacantItemBackground)
		{
			sizeAndPosition.Width = _browseItemConfigurationInfo.SizeAndPosition.Width;
			if (_browseItemConfigurationInfo.BrowseControl.ShowSeparator)
			{
				sizeAndPosition.Height -= _browseItemConfigurationInfo.BrowseControl.SeparatorPadding;
			}
			double num = Convert.ToDouble(148) / Convert.ToDouble(205);
			double num2 = Convert.ToDouble(sizeAndPosition.Width) / Convert.ToDouble(sizeAndPosition.Height);
			if (num > num2)
			{
				sizeAndPosition.Height = Convert.ToInt32((double)_browseItemConfigurationInfo.SizeAndPosition.Width / num);
				sizeAndPosition.Y += (_browseItemConfigurationInfo.SizeAndPosition.Height - sizeAndPosition.Height) / 2;
			}
			else if (num < num2)
			{
				sizeAndPosition.Width = Convert.ToInt32((double)sizeAndPosition.Height * num);
				if (!_browseItemConfigurationInfo.BrowseControl.ShowTitle)
				{
					sizeAndPosition.X += (_browseItemConfigurationInfo.SizeAndPosition.Width - sizeAndPosition.Width) / 2;
				}
				else
				{
					sizeAndPosition.X += 9;
				}
			}
		}
		else
		{
			sizeAndPosition.Width = 0;
		}
		return sizeAndPosition;
	}

	private void RemoveOldActors()
	{
		ActorFunctions.RemoveActor(ImageActorName);
		ActorFunctions.RemoveActor(FlagActorName);
		if (_titleActor != null)
		{
			_titleActor.RemoveActor();
		}
		if (_separatorActor != null)
		{
			_separatorActor.RemoveActor();
		}
		ActorFunctions.RemoveActor(ImageAddActorName);
		ActorFunctions.RemoveActor(CancelButtonActorName);
		ActorFunctions.RemoveActor(BannerActorName);
		ActorFunctions.RemoveActor(ButtonEmailReminderActorName);
	}
}
