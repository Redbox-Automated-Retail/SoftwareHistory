using System.Threading.Tasks;

namespace UpdateClientService.API.Services;

public interface IPersistentDataCacheService
{
	Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, string filePath = null) where T : IPersistentData;

	Task<bool> Write<T>(T persistentData, string fileName, string filePath = null) where T : IPersistentData;

	Task<bool> Delete(string fileName, string filePath = null);
}
