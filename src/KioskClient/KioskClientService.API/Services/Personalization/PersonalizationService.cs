using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.TestData;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Personalization;

public class PersonalizationService : IPersonalizationService
{
	private readonly ILogger<IPersonalizationService> _logger;

	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly IStoreService _storeService;

	private readonly ITestDataService _testDataService;

	private readonly IHttpService _httpService;

	public PersonalizationService(IApplicationConfiguration applicationConfiguration, ILogger<IPersonalizationService> logger, ITestDataService testDataService, IStoreService storeService, IHttpService httpService)
	{
		_logger = logger;
		_testDataService = testDataService;
		_applicationConfiguration = applicationConfiguration;
		_storeService = storeService;
		_httpService = httpService;
	}
}
