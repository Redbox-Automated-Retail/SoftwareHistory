using System;
using System.IO;
using System.Net;
using System.Net.Http;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Products.MoreLikeThis;
using KioskClientService.API.Services.Products.ProductReelsApi;
using KioskClientService.API.Services.TestData;
using KioskClientService.API.Vistar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace KioskClientService.API;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		MvcJsonMvcBuilderExtensions.AddJsonOptions(services.AddMvc(delegate(MvcOptions options)
		{
			options.AddLoggingActionFilter();
		}).SetCompatibilityVersion(CompatibilityVersion.Version_2_1), (Action<MvcJsonOptions>)delegate(MvcJsonOptions options)
		{
			options.SerializerSettings.Converters.Add(new StringEnumConverter());
		});
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			c.SwaggerDoc("v1", new Info
			{
				Title = "Kiosk Client API",
				Version = "v1"
			});
		});
		services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));
		ServiceProvider provider = services.BuildServiceProvider();
		ApplicationSettings value = provider.GetRequiredService<IOptionsSnapshot<ApplicationSettings>>().Value;
		services.AddSingleton((IApplicationSettings)value);
		services.AddSingleton<IConfigurationService, ConfigurationService>();
		services.AddScoped<IApplicationConfiguration, ApplicationConfiguration>();
		ILogger<Startup> requiredService = provider.GetRequiredService<ILogger<Startup>>();
		services.AddHttpClient<IHttpService, HttpService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			AllowAutoRedirect = true,
			UseCookies = true,
			CookieContainer = new CookieContainer()
		});
		services.AddSingleton<IApplicationControl, ApplicationControl>();
		services.AddSingleton<IStoreService, StoreService>();
		services.AddSingleton<IAnalyticsQueue, AnalyticsQueue>();
		services.AddSingleton<IMoreLikeThisData, MoreLikeThisData>();
		services.AddSingleton<IMoreLikeThisCache, MoreLikeThisCache>();
		services.AddScoped<IAnalyticsService, AnalyticsService>();
		services.AddScoped<IApplicationService, ApplicationService>();
		services.AddScoped<ISessionService, SessionService>();
		services.AddScoped<IAdService, AdService>();
		services.AddScoped<IAssetService, AssetService>();
		services.AddScoped<IVistarAPIClient, VistarAPIClient>();
		services.AddScoped<IAdServerClient, AdServerClient>();
		services.AddScoped<IKioskDataServiceClient, KioskDataServiceClient>();
		services.AddScoped<IProductReelsApiClient, ProductReelsApiClient>();
		services.AddScoped<IMoreLikeThisService, MoreLikeThisService>();
		services.AddScoped<ITestDataService, TestDataService>();
		ServiceProvider provider2 = services.BuildServiceProvider();
		InitializeConfiguration(provider2.GetService<IConfigurationService>(), provider2.GetRequiredService<IStoreService>(), provider2.GetService<ILogger<Startup>>());
		LogSettings(requiredService, value, provider2.GetService<IApplicationConfiguration>());
	}

	private void LogSettings(ILogger<Startup> logger, IApplicationSettings settings, IApplicationConfiguration applicationConfiguration)
	{
		logger.LogInformation($"Assembly Version: {GetType().Assembly.GetName().Version}");
		logger.LogInformation("AppSettings - DataFilePath: " + settings.DataFilePath);
		logger.LogInformation("AppSettings - ServiceUrl: " + settings.ServiceUrl);
		logger.LogInformation("AppSettings - UpdateManagerAddress: " + settings.UpdateManagerAddress);
		logger.LogInformation($"AppSettings - UpdateManagerPort: {settings.UpdateManagerPort}");
		logger.LogInformation($"AppConfig - AdsActive: {applicationConfiguration.AdsActive}");
		VistarConfigs configs = VistarConfigs.GetConfigs(applicationConfiguration);
		logger.LogInformation("AppConfig.VistarSettings - Url: " + configs.Url);
		logger.LogInformation($"AppConfig.VistarSettings - Timeout: {configs.Timeout}");
		logger.LogInformation("AppConfig.VistarSettings - NetworkId: " + configs.NetworkId.MaskLogValue(4));
		logger.LogInformation("AppConfig.VistarSettings - APIKey: " + configs.APIKey.MaskLogValue(4));
		AdServerConfigs adServer = applicationConfiguration.AdServer;
		logger.LogInformation("AppConfig.AdServerSettings - Url: " + adServer.Url);
		logger.LogInformation($"AppConfig.AdServerSettings - Timeout: {adServer.Timeout}");
		logger.LogInformation("AppConfig.AdServerSettings - APIKey: " + adServer.APIKey.MaskLogValue(4));
		KioskDataServiceConfigs kioskDataService = applicationConfiguration.KioskDataService;
		logger.LogInformation("AppConfig.KioskDataServiceSettings - Url: " + kioskDataService.Url);
		logger.LogInformation($"AppConfig.KioskDataServiceSettings - Timeout: {kioskDataService.Timeout}");
		logger.LogInformation("AppConfig.KioskDataServiceSettings - APIKey: " + kioskDataService.APIKey.MaskLogValue(4));
	}

	private ServiceResult InitializeConfiguration(IConfigurationService configuration, IStoreService store, ILogger<Startup> logger)
	{
		if (configuration == null)
		{
			logger.LogError("Startup.InitializeConfiguration - configuration parameter is null.");
			return new ServiceResult(success: false);
		}
		logger.LogInformation("Starting Update Manager Configuration Service.");
		ServiceResult serviceResult = configuration.Initialize();
		if (!serviceResult.Success)
		{
			return serviceResult;
		}
		serviceResult = configuration.Start();
		if (!serviceResult.Success)
		{
			return serviceResult;
		}
		string fullPath = Path.GetFullPath(Path.Combine(store.DataPath, "Configuration"));
		if (!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
		try
		{
			FileInfo[] files = directoryInfo.GetFiles("*.config");
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				logger.LogInformation("Startup.InitializeConfiguration adding " + file.Name);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
				serviceResult = configuration.Add(fileNameWithoutExtension, file.FullName, "default", null, null, delegate
				{
					logger.LogInformation("Startup.InitializeConfiguration new {0} file was loaded.", Path.GetFileName(file.Name));
				});
				if (!serviceResult.Success)
				{
					return serviceResult;
				}
			}
		}
		catch (Exception exception)
		{
			logger.LogError(exception, "Startup.InitializeConfiguration - 'data' folder issue.");
			Environment.Exit(0);
		}
		return new ServiceResult(success: true);
	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, ILogger<Startup> logger)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseHsts();
		}
		app.ConfigureExceptionMiddleware();
		app.UseHttpsRedirection();
		app.UseSwagger();
		app.UseSwaggerUI(delegate(SwaggerUIOptions c)
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
		});
		app.UseMvc();
	}
}
