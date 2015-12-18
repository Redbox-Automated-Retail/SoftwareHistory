using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Model.DataService;

public interface IDataServiceInstance
{
	IProfileDataInstance ProfileDataInstance { get; set; }

	ICacheDataInstance CacheDataInstance { get; set; }

	IKioskDataInstance KioskDataInstance { get; set; }
}
