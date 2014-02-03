using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class StoreFileCache
{
	public long Id { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public List<StoreFileDataCache> StoreFileDataCacheList { get; set; }
}
