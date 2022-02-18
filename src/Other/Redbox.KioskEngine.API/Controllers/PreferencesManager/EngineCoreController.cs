using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.REDS.Framework;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.PreferencesManager;

public class EngineCoreController : ApiController
{
	[HttpGet]
	[Route("api/enginecore/getkioskengineversions")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(EngineCoreKioskEngineVersionResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetVersions()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		EngineCoreKioskEngineVersionResponse response = new EngineCoreKioskEngineVersionResponse();
		await Task.Run(delegate
		{
			KioskEngineVersions kioskEngineVersions = response.Content;
			AppDomain.CurrentDomain.GetAssemblies().ForEach(delegate(Assembly eachAssembly)
			{
				AssemblyName assemblyName = eachAssembly?.GetName();
				if ((assemblyName?.Name?.ToUpper()?.StartsWith("REDBOX") ?? false) || assemblyName?.Name?.ToUpper() == "KIOSKENGINE")
				{
					kioskEngineVersions.Assemblies.Add(new AssemblyInfo
					{
						Name = assemblyName.Name,
						Version = assemblyName.Version?.ToString()
					});
				}
			});
			kioskEngineVersions.Assemblies.Sort((AssemblyInfo a, AssemblyInfo b) => a.Name.CompareTo(b.Name));
			ServiceLocator.Instance.GetService<IResourceBundleService>()?.Bundles?.ForEach(delegate(IBundleSpecifier eachBundle)
			{
				kioskEngineVersions.Bundles.Add(new BundleInfo
				{
					Name = eachBundle?.Name,
					Version = eachBundle?.Version?.ToString()
				});
			});
			kioskEngineVersions.Bundles.Sort((BundleInfo a, BundleInfo b) => a.Name.CompareTo(b.Name));
			LogHelper.Instance.Log($"EngineCoreController.GetVersions result={JsonConvert.SerializeObject(response.Content)}");
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/getstoreinfo")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetStoreInfoResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetStoreInfo()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetStoreInfoResponse response = new GetStoreInfoResponse();
		await Task.Run(delegate
		{
			Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
			if (store != null)
			{
				response.Content = new StoreInfo
				{
					Address = store.Address,
					Address1 = store.Address1,
					Address2 = store.Address2,
					BannerId = store.BannerId,
					City = store.City,
					CollectionMethod = store.CollectionMethod,
					County = store.County,
					DigitalPurchaseTaxRate = store.DigitalPurchaseTaxRate,
					InvSync = store.InvSync,
					Market = new MarketInfo
					{
						MarketId = (store.Market?.MarketId ?? 0),
						MarketName = store.Market?.MarketName
					},
					MarketId = store.MarketId,
					OpenDate = store.OpenDate,
					PurchasePrice = store.PurchasePrice,
					PurchaseTaxRate = store.PurchaseTaxRate,
					RentalTaxRate = store.RentalTaxRate,
					ReturnOnlyModeDate = store.ReturnOnlyModeDate,
					ReturnTime = store.ReturnTime,
					Sellthru = store.Sellthru,
					ServiceFee = store.ServiceFee,
					ServiceFeeTaxRate = store.ServiceFeeTaxRate,
					State = store.State,
					StoreId = store.StoreId,
					VendorId = store.VendorId,
					Zip = store.Zip
				};
				LogHelper.Instance.Log($"EngineCoreController.GetStoreInfo result={JsonConvert.SerializeObject(response.Content)}");
			}
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/enginecore/getpreferencevalue")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetPreferenceValueResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetPreferenceValue(string pageName, string settingName)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetPreferenceValueResponse response = new GetPreferenceValueResponse();
		await Task.Run(delegate
		{
			string text = ServiceLocator.Instance.GetService<IPreferenceService>()?.GetValue<string>(pageName, settingName);
			response.Content = text;
			LogHelper.Instance.Log($"EngineCoreController.GetPreferenceValue pageName={pageName}, settingName={settingName}, result={text}");
		});
		response.SetResponseTime((int)stopwatch.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
