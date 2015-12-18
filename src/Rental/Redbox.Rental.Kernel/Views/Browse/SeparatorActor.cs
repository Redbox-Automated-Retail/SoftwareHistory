using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class SeparatorActor
{
	private IBrowseItemConfigurationInfo _browseItemConfigurationInfo;

	private IBrowseItem _browseItem;

	private string SeparatorActorName => _browseItemConfigurationInfo.BrowseControl.GetPrefixedActorName($"movie_title_{_browseItemConfigurationInfo.BrowseItemActorIndex}");

	public SeparatorActor(IBrowseItem browseItem, IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		_browseItemConfigurationInfo = browseItemConfigurationInfo;
		_browseItem = browseItem;
	}

	public void RemoveActor()
	{
		ActorFunctions.RemoveActor(SeparatorActorName);
	}

	public void ConfigureActor()
	{
		if (_browseItemConfigurationInfo.BrowseControl.ShowSeparator && _browseItemConfigurationInfo.RowIndex != _browseItemConfigurationInfo.BrowseControl.RowCount - 1 && !_browseItemConfigurationInfo.IsBorderItem)
		{
			bool flag = _browseItem is IGroupHeaderBrowseItem;
			ActorFunctions.CreateActor(SeparatorActorName, addToFront: false, _browseItemConfigurationInfo.SizeAndPosition.X, _browseItemConfigurationInfo.SizeAndPosition.Y + _browseItemConfigurationInfo.SizeAndPosition.Height - 1, _browseItemConfigurationInfo.SizeAndPosition.Width, (!flag) ? 1 : 2, null, null, null, flag ? "black" : "gray", null, null, null, null, null, "Background", null);
		}
	}

	public IActor GetActor()
	{
		return ActorFunctions.GetActor(SeparatorActorName);
	}
}
