namespace Redbox.Rental.Kernel.Views.Browse;

public class GroupHeaderBrowseItem : BrowseItem, IGroupHeaderBrowseItem, IBrowseItem
{
	private IBrowseItemConfigurationInfo _browseItemConfigurationInfo;

	private TitleActor _titleActor;

	private SeparatorActor _separatorActor;

	public GroupHeaderBrowseItem()
	{
		base.IncludeInShownCount = false;
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
		_titleActor = new TitleActor(this, _browseItemConfigurationInfo);
		_titleActor.ConfigureActor();
		_separatorActor = new SeparatorActor(this, _browseItemConfigurationInfo);
		_separatorActor.ConfigureActor();
		UpdateItemMapping();
	}

	private void RemoveOldActors()
	{
		if (_titleActor != null)
		{
			_titleActor.RemoveActor();
		}
		if (_separatorActor != null)
		{
			_separatorActor.RemoveActor();
		}
	}

	private void UpdateItemMapping()
	{
		if (!_browseItemConfigurationInfo.IsBorderItem)
		{
			_browseItemConfigurationInfo.BrowseControl.NotifyUpdateItemMapping(_browseItemConfigurationInfo.BrowseItemActorIndex, this);
		}
	}
}
