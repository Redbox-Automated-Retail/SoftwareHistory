using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskData;

namespace Redbox.Rental.Data;

public class KioskDataInstance : IKioskDataInstance
{
	private List<Collection> _collections = new List<Collection>();

	public DateTime VersionTimeStamp { get; set; }

	public List<Collection> Collections => _collections;

	public KioskDataInstance()
	{
		VersionTimeStamp = new DateTime(2000, 1, 1);
	}
}
