using System.Threading.Tasks;

namespace KioskClientService.API.Services.App;

public interface IPersistentDataCacheService
{
	Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false) where T : IPersistentData;

	Task<bool> Write<T>(T persistentData, string fileName) where T : IPersistentData;

	Task<bool> Delete(string fileName);
}
