namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseItem : IBrowseItem
{
	private bool _includeInShownCount = true;

	private float _imageOpacity = 1f;

	public string Name { get; set; }

	public object Data { get; set; }

	public string ImageName { get; set; }

	public float ImageOpacity
	{
		get
		{
			return _imageOpacity;
		}
		set
		{
			_imageOpacity = value;
		}
	}

	public bool IsSelected { get; set; }

	public bool IncludeInShownCount
	{
		get
		{
			return _includeInShownCount;
		}
		set
		{
			_includeInShownCount = value;
		}
	}

	public string SortName { get; set; }

	public event BrowseItemEvent OnBeforeRender;

	public virtual void RenderActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
		if (OnBeforeRender != null)
		{
			OnBeforeRender(this);
		}
	}

	public virtual void ClearActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo)
	{
	}
}
