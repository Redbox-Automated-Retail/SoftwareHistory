using System;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class CollectionMessageModel
{
	public Action OnCancelDialog;

	public Action OnTimeOut;

	public Action OnSeeAllCollections;

	public string Title { get; set; }

	public string SeeAllCollectionsButtonText { get; set; }

	public DisplayCollectionModel DisplayCollectionModel1 { get; set; }

	public DisplayCollectionModel DisplayCollectionModel2 { get; set; }

	public int TimeOut { get; set; }

	public event BrowseItemModelEvent OnCollectionSelected;

	public void PrcoessCancelDialog()
	{
		if (OnCancelDialog != null)
		{
			OnCancelDialog();
		}
	}

	public void ProcessSeeAllCollections()
	{
		if (OnSeeAllCollections != null)
		{
			OnSeeAllCollections();
		}
	}

	public void ProcessCollectionSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (OnCollectionSelected != null)
		{
			OnCollectionSelected(browseItemModel, parameter);
		}
	}

	public void ProcessTimeOut()
	{
		if (OnTimeOut != null)
		{
			OnTimeOut();
		}
	}
}
