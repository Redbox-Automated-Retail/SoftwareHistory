using System.Collections.Generic;

namespace KioskClientService.API.Services.ProfileData;

public class DataTable<T>
{
	public Dictionary<long, DataRecord<T>> Data { get; set; } = new Dictionary<long, DataRecord<T>>();
}
