using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Promotion;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.ProxyApi;

internal class ProxyApi : IProxyApi
{
	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<IProxyApi> _logger;

	private long _kioskId;

	private const string _subscription = "api/subscription";

	private const string _customerProfile = "api/customerprofile";

	private const string _installer = "api/installer";

	private const string _loyalty = "api/loyalty";

	private const string _marketing = "api/marketing";

	private const string _pricing = "api/pricing";

	private const string _promotion = "api/promotion";

	private const string _planogram = "api/planogram";

	public ProxyApi(IStoreService store, IApplicationConfiguration applicationConfiguration, IHttpService httpService, ILogger<IProxyApi> logger)
	{
		_applicationConfiguration = applicationConfiguration;
		_httpService = httpService;
		_logger = logger;
		_httpService.Timeout = _applicationConfiguration.ProxyApi.Timeout ?? 60000;
		_kioskId = store?.KioskId ?? 0;
	}

	public async Task<APIResponse<GetProxySubscriptionPlanResponse>> GetSubscriptionPlans()
	{
		string endpoint = "api/subscription/subscriptionplans";
		return await _httpService.SendRequest<GetProxySubscriptionPlanResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(_kioskId));
	}

	public async Task<APIResponse<CustomerProfileKioskLoginResponse>> Login(CustomerProfileLoginRequest request)
	{
		string endpoint = "api/customerprofile/login";
		return await _httpService.SendRequest<CustomerProfileKioskLoginResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, request, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(request.MessageId, request.KioskId));
	}

	public async Task<APIResponse<InstallerModels.PendingStatesResponse>> GetPendingStates()
	{
		string endpoint = "api/installer/getpendingstates";
		return await _httpService.SendRequest<InstallerModels.PendingStatesResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(null, _kioskId));
	}

	public async Task<APIResponse<InstallerModels.PendingBannersResponse>> GetPendingBanners(int stateid)
	{
		string endpoint = "api/installer" + $"/getpendingbanners?stateid={stateid}";
		return await _httpService.SendRequest<InstallerModels.PendingBannersResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(null, _kioskId));
	}

	public async Task<APIResponse<InstallerModels.PendingKiosksResponse>> GetPendingKiosks(short stateid, int bannerid)
	{
		string endpoint = "api/installer" + $"/getpendingkiosks?stateid={stateid}&bannerid={bannerid}";
		return await _httpService.SendRequest<InstallerModels.PendingKiosksResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(null, _kioskId));
	}

	public async Task<APIResponse<InstallerModels.OpsMarketResponse>> GetOpsMarket(long kioskid)
	{
		string endpoint = "api/installer" + $"/getopsmarket?kioskid={kioskid}";
		return await _httpService.SendRequest<InstallerModels.OpsMarketResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(null, kioskid));
	}

	public async Task<APIResponse<KioskModels.KioskAuthenticateResponse>> Authenticate(KioskModels.KioskAuthenticateRequest request)
	{
		return await _httpService.SendRequest<KioskModels.KioskAuthenticateResponse>(_applicationConfiguration.ProxyApi.Url, "api/kiosk/authenticate", request, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(null, request.KioskId));
	}

	public async Task<APIResponse<EmailConfirmationResponse>> EmailConfirmation(EmailConfirmationPayload payload)
	{
		string endpoint = "api/marketing/emailconfirmation";
		return await _httpService.SendRequest<EmailConfirmationResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<KioskOptInResponse>> KioskOptIn(KioskOptInPayload payload)
	{
		string endpoint = "api/marketing/kioskoptin";
		return await _httpService.SendRequest<KioskOptInResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<GetProxyRedboxPlusPromoCampaignsResponse>> GetRedboxPlusPromoCampaigns()
	{
		string endpoint = "api/subscription/redboxpluspromocampaigns";
		return await _httpService.SendRequest<GetProxyRedboxPlusPromoCampaignsResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(_kioskId));
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload)
	{
		string endpoint = "api/loyalty/estimateaccrual";
		return await _httpService.SendRequest<ServerLoyaltyResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload)
	{
		string endpoint = "api/loyalty/estimateredemption";
		return await _httpService.SendRequest<ServerLoyaltyResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountPayload payload)
	{
		string endpoint = "api/loyalty/updateaccount";
		int hold = _httpService.Timeout;
		_httpService.Timeout = _applicationConfiguration.ProxyApi.UpdateAccountTimeout ?? 60000;
		APIResponse<UpdateAccountResponse> result = await _httpService.SendRequest<UpdateAccountResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
		_httpService.Timeout = hold;
		return result;
	}

	public async Task<APIResponse<PromotionCodeValidationResponse>> PromoCodeValidationAsync(PromotionCodeValidationPayload payload)
	{
		string endpoint = "api/promotion/promocodevalidation";
		return await _httpService.SendRequest<PromotionCodeValidationResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, payload, HttpMethod.Post, _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(payload.MessageId, payload.KioskId));
	}

	public async Task<APIResponse<KioskRentalDefaultPricingResponse>> GetRentalDefaultPricing(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/rental/default";
		return await _httpService.SendRequest<KioskRentalDefaultPricingResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<KioskRentalPricingByPriceResponse>> GetRentalPricingByPrice(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/rentalbyprice";
		return await _httpService.SendRequest<KioskRentalPricingByPriceResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<KioskPurchasePricingByPriceResponse>> GetPurchasePricingByPrice(long kioskId)
	{
		string endpoint = "api/pricing" + $"/kiosk/{kioskId}/purchasebyprice";
		return await _httpService.SendRequest<KioskPurchasePricingByPriceResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	public async Task<APIResponse<GetPlanogramsResponse>> GetPlanograms(long kioskId, DateTime? lastChange = null)
	{
		string text = (lastChange.HasValue ? lastChange.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "null");
		string endpoint = "api/planogram/allcurrent?lastReportedTime=" + text;
		return await _httpService.SendRequest<GetPlanogramsResponse>(_applicationConfiguration.ProxyApi.Url, endpoint, null, HttpMethod.Get, GetHeaders(kioskId));
	}

	private List<Header> GetHeaders(long kioskId)
	{
		List<Header> kioskCloudHeaders = _applicationConfiguration.ProxyApi.GetKioskCloudHeaders(Guid.NewGuid(), kioskId);
		return kioskCloudHeaders.AddCompression();
	}
}
