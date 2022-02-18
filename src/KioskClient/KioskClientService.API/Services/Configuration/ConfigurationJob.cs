using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.ProductGroup;
using KioskClientService.API.Services.Subscription;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Configuration;

public class ConfigurationJob : IInvocable
{
	private ILogger<ConfigurationJob> _logger;

	private ISubscriptionService _subscriptionService;

	private IProductGroupSortOrderService _productGroupSortOrderService;

	private IConfigurationService _configurationService;

	public ConfigurationJob(ILogger<ConfigurationJob> logger, IConfigurationService configurationService, ISubscriptionService subscriptionService, IProductGroupSortOrderService productGroupSortOrderService)
	{
		_configurationService = configurationService;
		_logger = logger;
		_subscriptionService = subscriptionService;
		_productGroupSortOrderService = productGroupSortOrderService;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking Configuration Job to check for new files", "Invoke");
		_configurationService.CheckForUpdates(out var dict);
		if (dict != null)
		{
			foreach (KeyValuePair<string, string> item in dict)
			{
				if (item.Key.Equals("kioskclient", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInformation("New Configuration file activated " + item.Key + " - " + Path.GetFileName(item.Value) + " file was loaded.");
					await (_productGroupSortOrderService?.RefreshIfNeeded());
					await (_subscriptionService?.RefreshIfNeeded());
				}
			}
		}
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
