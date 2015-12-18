namespace Redbox.Rental.Kernel.Views.Browse;

public interface IBrowseItem
{
	string Name { get; set; }

	object Data { get; set; }

	string ImageName { get; set; }

	float ImageOpacity { get; set; }

	bool IsSelected { get; set; }

	bool IncludeInShownCount { get; set; }

	string SortName { get; set; }

	event BrowseItemEvent OnBeforeRender;

	void RenderActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo);

	void ClearActors(IBrowseItemConfigurationInfo browseItemConfigurationInfo);
}
