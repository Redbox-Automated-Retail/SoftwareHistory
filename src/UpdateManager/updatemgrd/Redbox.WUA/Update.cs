using System;
using System.Collections.Generic;
using WUApiLib;

namespace Redbox.WUA;

internal class Update
{
	public string Title;

	public Guid Id;

	public List<string> Categories;

	public Uri MoreInfoUrl;

	public bool IsDownloaded;

	public bool IsInstalled;

	public Update(IUpdate update)
	{
		Title = update.Title;
		Id = new Guid(update.Identity.UpdateID);
		MoreInfoUrl = ((update.MoreInfoUrls.Count > 0) ? new Uri(update.MoreInfoUrls[0]) : null);
		IsInstalled = update.IsInstalled;
		IsDownloaded = update.IsDownloaded;
		Categories = new List<string>();
		foreach (ICategory category in update.Categories)
		{
			Categories.Add(category.Name);
		}
	}
}
