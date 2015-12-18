using System;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class TitleActor
{
	private IBrowseItemConfigurationInfo _browseItemConfigurationInfo;

	private IBrowseItem _browseItem;

	private int _numberOfVisibleTitleCharacters = 18;

	private string TitleActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"movie_title_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	public TitleActor(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		_browseItemConfigurationInfo = browseItemConfigurationInfo;
		_browseItem = browseItem;
	}

	public void RemoveActor()
	{
		ActorFunctions.RemoveActor(TitleActorName);
	}

	public void ConfigureActor()
	{
		if (!_browseItemConfigurationInfo.BrowseControl.ShowTitle)
		{
			return;
		}
		bool flag = _browseItem is IGroupHeaderBrowseItem;
		string text = _browseItem.Name;
		if (text.Length > _numberOfVisibleTitleCharacters)
		{
			text = text.Substring(0, _numberOfVisibleTitleCharacters);
			int num = text.LastIndexOf(' ');
			if (num > -1)
			{
				text = text.Substring(0, num);
			}
			text = $"{text}...";
		}
		ISizeAndPosition sizeAndPosition = ComputeSizeAndPosition(flag);
		ActorFunctions.CreateActor(TitleActorName, addToFront: false, sizeAndPosition.X, sizeAndPosition.Y, sizeAndPosition.Width, sizeAndPosition.Height, text, flag ? "font_akzidenz_bold_small" : "font_akzidenz_regular_smaller_13", null, null, "black", null, null, null, null, "Text", (!_browseItemConfigurationInfo.IsBorderItem) ? "Enabled" : null);
		ActorFunctions.SetActorTag(TitleActorName, _browseItem);
		ActorFunctions.SetActorTextAlignY(TitleActorName, "center");
		IActor actor = GetActor();
		if (actor != null)
		{
			Font font = new Font(actor.Font.FontFamily, sizeAndPosition.FontSize, actor.Font.Style);
			actor.Font = font;
			Color color = Color.FromArgb(153, 0, 0);
			Color color2 = Color.FromArgb(0, 0, 0);
			actor.ForegroundColor = ((_browseItem.IsSelected & flag) ? color : color2);
		}
	}

	public IActor GetActor()
	{
		return ActorFunctions.GetActor(TitleActorName);
	}

	private ISizeAndPosition ComputeSizeAndPosition(bool isGroupHeader)
	{
		SizeAndPosition sizeAndPosition = new SizeAndPosition();
		int num = 238;
		int num2 = (isGroupHeader ? 18 : 13);
		((ISizeAndPosition)sizeAndPosition).X = _browseItemConfigurationInfo.SizeAndPosition.X + 10;
		((ISizeAndPosition)sizeAndPosition).Y = _browseItemConfigurationInfo.SizeAndPosition.Y;
		((ISizeAndPosition)sizeAndPosition).Width = _browseItemConfigurationInfo.SizeAndPosition.Width - 20;
		((ISizeAndPosition)sizeAndPosition).Height = _browseItemConfigurationInfo.SizeAndPosition.Height;
		double num3 = Convert.ToDouble(_browseItemConfigurationInfo.SizeAndPosition.Width) / (double)num;
		((ISizeAndPosition)sizeAndPosition).FontSize = Convert.ToSingle((double)num2 * num3);
		return sizeAndPosition;
	}
}
