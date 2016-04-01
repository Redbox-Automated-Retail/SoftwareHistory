using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class InventoryDatabaseTest
{
	public bool Test()
	{
		int failures = 0;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		service2.ForAllDecksDo(delegate(IDeck deck)
		{
			ILocation location = service.Get(deck.Number, deck.NumberOfSlots);
			DateTime? returnDate = location.ReturnDate;
			location.ReturnDate = ((!returnDate.HasValue) ? new DateTime?(DateTime.Now) : ((DateTime?)null));
			if (!service.Save(location))
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[InventoryDBTest] Update failure on deck {0}", deck.Number);
				failures++;
			}
			location.ReturnDate = returnDate;
			service.Save(location);
			return true;
		});
		return 0 == failures;
	}
}
