using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskData;

public interface IKioskDataInstance
{
	DateTime VersionTimeStamp { get; set; }

	List<Collection> Collections { get; }
}
