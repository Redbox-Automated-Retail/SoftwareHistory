namespace Redbox.Rental.Kernel.Views.Browse;

public interface IBrowseControl
{
	int RowCount { get; set; }

	bool ShowSeparator { get; set; }

	int SeparatorPadding { get; set; }

	bool ShowTitle { get; set; }

	string NoResultsText { get; set; }

	bool ShowVacantItemBackground { get; set; }

	event BrowseItemEvent OnItemSelected;

	event BrowseItemActorEvent OnAddItemSelected;

	event BrowseItemMappingEvent OnUpdateItemMapping;

	event BrowseItemActionEvent OnItemAction;

	void NotifyItemSelected(IBrowseItem browseItem);

	void NotifyAddItemSelected(IBrowseItem browseItem, string actorName);

	void NotifyUpdateItemMapping(int actorIndex, IBrowseItem browseItem);

	void NotifyItemAction(IBrowseItem browseItem, string action);

	string GetPrefixedActorName(string actorName);
}
