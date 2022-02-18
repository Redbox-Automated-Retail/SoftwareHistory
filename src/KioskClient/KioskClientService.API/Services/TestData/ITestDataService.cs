using System.Threading.Tasks;

namespace KioskClientService.API.Services.TestData;

public interface ITestDataService
{
	Task<WriteTestDataResponse> WriteTestDataValueAsync(string category, string propertyName, string propertyValue);

	Task<ReadTestDataResponse> ReadTestDataValueAsync(string category, string propertyName);

	Task<IsTestDataEnabledResponse> GetIsTestDataEnabledAsync();
}
