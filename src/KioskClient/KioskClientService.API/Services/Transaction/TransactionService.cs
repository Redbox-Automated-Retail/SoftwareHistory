using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.TestData;
using KioskClientService.API.Services.Transaction.API;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Transaction;

public class TransactionService : ITransactionService
{
	private class Constants
	{
		public class AuthorizeErrorCodes
		{
			public const string CheckoutApiError = "(A030)";

			public const string MissingOrExtraSubscriptionLineItem = "(A031)";

			public const string CardCPDifferentFromSubCP = "(A032)";

			public const string SubscriptionNotProcessedDueToAuthFailure = "(A033)";

			public const string InvalidCPServiceResponse = "(A035)";

			public const string CPServiceApiError = "(A036)";

			public const string InventoryServiceApiError = "(A037)";

			public const string CustomerBlockListed = "(A038)";

			public const string AuthorizeProcessingError = "(A100)";

			public const string MerchantServiceConnectionError = "(A101)";

			public const string KioskDatConnectionError = "(A102)";

			public const string CPServiceConnectionError = "(A203)";

			public const string PromoServiceConnectionError = "(A104)";

			public const string CheckoutApiConnectionError = "(A105)";

			public const string InventoryServiceConnectionError = "(A106)";

			public const string InvalidCheckoutApiResponse = "(A034)";
		}

		public class TestData
		{
			public class Authorize
			{
				public const string LiveCall = "Authorize.LiveCall";

				public const string SimulateCommunicationFailure = "Authorize.SimulateCommunicationFailure";

				public const string SimulateServerBusy = "Authorize.SimulateServerBusy";

				public const string ErrorCode = "Authorize.ErrorCode";

				public const string AccountNumber = "Authorize.AccountNumber";

				public const string HasCustomerProfile = "Authorize.HasCustomerProfile";

				public const string CustomerProfileNumber = "Authorize.CustomerProfileNumber";

				public const string CustomerProfileFirstName = "Authorize.CustomerProfileFirstName";

				public const string CustomerProfileLastName = "Authorize.CustomerProfileLastName";

				public const string Email = "Authorize.Email";

				public const string ReferenceNumber = "Authorize.ReferenceNumber";

				public const string ConfirmationStatus = "Authorize.ConfirmationStatus";
			}

			public class GCAuth
			{
				public const string LiveCall = "GCAuth.LiveCall";

				public const string SimulateCommunicationFailure = "GCAuth.SimulateCommunicationFailure";

				public const string SimulateServerBusy = "GCAuth.SimulateServerBusy";

				public const string ErrorCode = "GCAuth.ErrorCode";

				public const string GCResponseType = "GCAuth.GCResponseType";

				public const string Exists = "GCAuth.Exists";

				public const string GCStatusType = "GCAuth.GCStatusType";

				public const string Balance = "GCAuth.Balance";

				public const string MaxDiscs = "GCAuth.MaxDiscs";
			}

			public const string CategoryName = "TransactionService";
		}
	}

	private const int _cacheExpiry = 30;

	private readonly ITransactionApiClient _transactionClient;

	private readonly IMemoryCache _cache;

	private readonly ILogger<ITransactionService> _logger;

	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly ITestDataService _testDataService;

	private readonly string _promoCheckCachePrefix = "[RPC]";

	private readonly TimeSpan _promoCheckCacheMinutes = TimeSpan.FromMinutes(30.0);

	private static readonly List<string> _subscriptionErrorCodes = new List<string> { "(A030)", "(A031)", "(A033)", "(A034)" };

	public TransactionService(IApplicationConfiguration applicationConfiguration, ITestDataService testDataService, IMemoryCache cache, ITransactionApiClient client, ILogger<ITransactionService> logger)
	{
		_applicationConfiguration = applicationConfiguration;
		_testDataService = testDataService;
		_transactionClient = client;
		_cache = cache;
		_logger = logger;
	}

	public async Task<AuthorizeResponse> Authorize(AuthorizeRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.Authorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "Authorize was unsuccessful.", "Authorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	private void CheckForAuthOfflineError<T>(APIResponse<T> response) where T : AuthorizeResponse
	{
		if (response.StatusCode == HttpStatusCode.InternalServerError && response.Response != null)
		{
			response.Response.Errors.Add(new Error
			{
				Code = "(A100)",
				Message = "Server Error"
			});
			response.Response.Success = false;
		}
	}

	public async Task<GCAuthResponse> GiftCardAuthorize(GCAuthRequest request)
	{
		GCAuthResponse gCAuthResponse;
		if (await GetUseTestData("GCAuth.LiveCall"))
		{
			gCAuthResponse = await GetTestDataGCAuthorizeResponse(request);
		}
		else
		{
			APIResponse<GCAuthResponse> aPIResponse = await _transactionClient.GiftCardAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			gCAuthResponse = aPIResponse.Response;
			if (gCAuthResponse == null || !gCAuthResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "GCAuthorize was unsuccessful.", "GiftCardAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return gCAuthResponse;
	}

	public async Task<AuthorizeResponse> PickupAuthorize(PickupAuthorizeRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.PickupAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "PickupAuthorize was unsuccessful.", "PickupAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	public async Task<AuthorizeResponse> ReservationAuthorize(ResvAuthRequest request)
	{
		AuthorizeResponse authorizeResponse;
		if (await GetUseTestData("Authorize.LiveCall"))
		{
			authorizeResponse = await GetTestDataAuthorizeResponse(request);
		}
		else
		{
			APIResponse<AuthorizeResponse> aPIResponse = await _transactionClient.ReservationAuthorize(request);
			CheckForAuthOfflineError(aPIResponse);
			authorizeResponse = aPIResponse.Response;
			if (authorizeResponse == null || !authorizeResponse.Success)
			{
				_logger.LogErrorWithSource(aPIResponse.Exception, "ReservationAuthorize was unsuccessful.", "ReservationAuthorize");
				LogResponseErrors(aPIResponse);
			}
		}
		return authorizeResponse;
	}

	public async Task<APIResponse<CancelReservationResponse>> CancelReservation(CancelReservationRequest request)
	{
		APIResponse<CancelReservationResponse> aPIResponse = await _transactionClient.CancelReservation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "CancelReservation was unsuccessful.", "CancelReservation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ExpireReservationResponse>> ExpireReservation(ExpireReservationRequest request)
	{
		APIResponse<ExpireReservationResponse> aPIResponse = await _transactionClient.ExpireReservation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "ExpireReservation was unsuccessful.", "ExpireReservation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReturnResponse>> Return(ReturnRequest request)
	{
		APIResponse<ReturnResponse> aPIResponse = await _transactionClient.Return(request);
		if (aPIResponse?.Response?.Success != true)
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "Return was unsuccessful.", "Return");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReconcileResponse>> Reconcile(ReconcileRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionClient.Reconcile(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "Reconcile was unsuccessful.", "Reconcile");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<ReconcileResponse>> ReservationPickup(ReservationPickupRequest request)
	{
		APIResponse<ReconcileResponse> aPIResponse = await _transactionClient.ReservationPickup(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "ReservationPickup was unsuccessful.", "ReservationPickup");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<APIResponse<PickupValidationResponse>> PickupValidation(PickupValidationRequest request)
	{
		APIResponse<PickupValidationResponse> aPIResponse = await _transactionClient.PickupValidation(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "PickupValidation was unsuccessful.", "PickupValidation");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response?.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}

	public async Task<APIResponse<GetCustomerOffersResponse>> GetCustomerOffers(GetCustomerOffersRequest request)
	{
		APIResponse<GetCustomerOffersResponse> aPIResponse = await _transactionClient.GetCustomerOffers(request);
		if (!(aPIResponse.Response?.Success ?? false))
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "GetCustomerOffers was unsuccessful.", "GetCustomerOffers");
			LogResponseErrors(aPIResponse);
		}
		return aPIResponse;
	}

	public async Task<ReturnPromoCheckResponse> ReturnPromoCheck(string barcode)
	{
		ReturnPromoCheckResponse result = new ReturnPromoCheckResponse
		{
			StatusCode = HttpStatusCode.OK,
			IsLastDisc = false
		};
		if (!_cache.TryGetValue<ReturnPromoCheckApiResponse>(ToRpcKey(barcode), out var value) || value == null)
		{
			value = await CallReturnPromoCheck(barcode);
			if (value == null || value.StatusCode != HttpStatusCode.OK)
			{
				result.StatusCode = value.StatusCode;
				result.Promotion = null;
				return result;
			}
		}
		else
		{
			_logger.LogDebugWithSource("Promo check response for barcode " + barcode + " retrieved from cache", "ReturnPromoCheck");
		}
		if (value.Discs.Any((DiscReturnStatus d) => !d.IsReturned && d.Barcode != barcode))
		{
			_logger.LogDebugWithSource("Not all discs are returned, returning null promotion.", "ReturnPromoCheck");
			foreach (DiscReturnStatus disc in value.Discs)
			{
				if (disc.Barcode == barcode)
				{
					disc.IsReturned = true;
				}
			}
			foreach (DiscReturnStatus disc2 in value.Discs)
			{
				_cache.Set(ToRpcKey(disc2.Barcode), value, _promoCheckCacheMinutes);
			}
			result.Promotion = null;
		}
		else
		{
			result.Promotion = value.Promotion;
			result.IsLastDisc = true;
		}
		return result;
	}

	private string ToRpcKey(string key)
	{
		return _promoCheckCachePrefix + " " + key;
	}

	private async Task<ReturnPromoCheckApiResponse> CallReturnPromoCheck(string barcode)
	{
		APIResponse<ReturnPromoCheckApiResponse> aPIResponse = await _transactionClient.ReturnPromoCheck(barcode);
		ReturnPromoCheckApiResponse returnPromoCheckApiResponse = aPIResponse.Response;
		if (!aPIResponse.IsSuccessStatusCode)
		{
			string text = $"ReturnPromoCheck was unsuccessful. Status Code: {aPIResponse.StatusCode}";
			if (returnPromoCheckApiResponse != null)
			{
				returnPromoCheckApiResponse.StatusCode = aPIResponse.StatusCode;
				text = text + " Message: " + returnPromoCheckApiResponse.Error;
			}
			else
			{
				returnPromoCheckApiResponse = new ReturnPromoCheckApiResponse
				{
					StatusCode = aPIResponse.StatusCode
				};
				if ((aPIResponse.Errors?.Count ?? 0) > 0)
				{
					returnPromoCheckApiResponse.Error = aPIResponse.Errors.First().Message;
				}
			}
			_logger.LogErrorWithSource(text, "CallReturnPromoCheck");
			LogResponseErrors(aPIResponse);
		}
		return returnPromoCheckApiResponse;
	}

	private async Task<AuthorizeResponse> GetTestDataAuthorizeResponse(AuthorizeRequest authorizeRequest)
	{
		_logger?.LogInformation("TransactionService.Authorize returning test data result");
		if (bool.TryParse(await GetTestDataValue("Authorize.SimulateCommunicationFailure"), out var result) && result)
		{
			return null;
		}
		AuthorizeResponse response = new AuthorizeResponse
		{
			MessageId = authorizeRequest.MessageId
		};
		response.Errors = new List<Error>();
		string text = await GetTestDataValue("Authorize.ErrorCode");
		if (!string.IsNullOrEmpty(text))
		{
			response.Errors.Add(new Error
			{
				Code = text,
				Message = "error code " + text
			});
		}
		await FillTestDataAuthorizeResponse(authorizeRequest, response);
		return response;
	}

	private async Task FillTestDataAuthorizeResponse(AuthorizeRequest authorizeRequest, AuthorizeResponse response)
	{
		bool flag = (authorizeRequest.ShoppingCart?.Groups?.FirstOrDefault((LineItemGroup x) => x.GroupType == LineItemGroupType.Subscription)?.Items?.Count).GetValueOrDefault() > 0;
		bool flag2 = (authorizeRequest.ShoppingCart?.Groups?.FirstOrDefault((LineItemGroup x) => x.GroupType == LineItemGroupType.Rental || x.GroupType == LineItemGroupType.Purchase)?.Items?.Count).GetValueOrDefault() > 0;
		bool flag3 = response.Errors.Any((Error x) => _subscriptionErrorCodes.Contains(x.Code));
		response.KioskTransactionSuccess = flag2 && (response.Errors.Count() == 0 || response.Errors.All((Error x) => _subscriptionErrorCodes.Contains(x.Code)));
		response.SubscriptionTransactionSuccess = flag && !flag3 && (!flag2 || response.KioskTransactionSuccess);
		response.Success = response.KioskTransactionSuccess || response.SubscriptionTransactionSuccess;
		AuthorizeResponse authorizeResponse = response;
		authorizeResponse.AccountNumber = await GetTestDataValue("Authorize.AccountNumber");
		if (bool.TryParse(await GetTestDataValue("Authorize.HasCustomerProfile"), out var result))
		{
			response.HasProfile = result;
		}
		authorizeResponse = response;
		authorizeResponse.CustomerProfileNumber = await GetTestDataValue("Authorize.CustomerProfileNumber");
		authorizeResponse = response;
		authorizeResponse.CustomerProfileFirstName = await GetTestDataValue("Authorize.CustomerProfileFirstName");
		authorizeResponse = response;
		authorizeResponse.CustomerProfileLastName = await GetTestDataValue("Authorize.CustomerProfileLastName");
		authorizeResponse = response;
		authorizeResponse.Email = await GetTestDataValue("Authorize.Email");
		if (long.TryParse(await GetTestDataValue("Authorize.ReferenceNumber"), out var result2))
		{
			response.ReferenceNumber = result2;
		}
		if (Enum.TryParse<ConfirmationStatus>(await GetTestDataValue("Authorize.ConfirmationStatus"), out var result3))
		{
			response.ConfirmationStatus = result3;
		}
	}

	private async Task<GCAuthResponse> GetTestDataGCAuthorizeResponse(GCAuthRequest request)
	{
		_logger?.LogInformation("TransactionService.GiftCardAuthorize returning test data result");
		if (bool.TryParse(await GetTestDataValue("GCAuth.SimulateCommunicationFailure"), out var result) && result)
		{
			return null;
		}
		GCAuthResponse response = new GCAuthResponse
		{
			MessageId = request.MessageId
		};
		response.Errors = new List<Error>();
		string text = await GetTestDataValue("GCAuth.ErrorCode");
		if (!string.IsNullOrEmpty(text))
		{
			response.Errors.Add(new Error
			{
				Code = text,
				Message = "error code " + text
			});
		}
		await FillTestDataAuthorizeResponse(request, response);
		GiftCardSummary gcSummary = new GiftCardSummary();
		if (decimal.TryParse(await GetTestDataValue("GCAuth.Balance"), out var result2))
		{
			gcSummary.Balance = result2;
		}
		if (bool.TryParse(await GetTestDataValue("GCAuth.Exists"), out var result3))
		{
			gcSummary.Exists = result3;
		}
		if (int.TryParse(await GetTestDataValue("GCAuth.MaxDiscs"), out var result4))
		{
			gcSummary.MaxDiscs = result4;
		}
		if (Enum.TryParse<GiftCardStatus>(await GetTestDataValue("GCAuth.GCStatusType"), out var result5))
		{
			gcSummary.Status = result5;
		}
		return response;
	}

	private async Task<string> GetTestDataValue(string propertyName)
	{
		return (await (_testDataService?.ReadTestDataValueAsync("TransactionService", propertyName)))?.PropertyValue;
	}

	private async Task<bool> GetUseTestData(string liveCallPropertyName)
	{
		bool result = false;
		if (_applicationConfiguration?.TestDataEnabled ?? false)
		{
			ReadTestDataResponse readTestDataResponse = await (_testDataService?.ReadTestDataValueAsync("TransactionService", liveCallPropertyName));
			if (readTestDataResponse != null && readTestDataResponse.Success && readTestDataResponse.GetPropertyValue<bool?>() == false)
			{
				result = true;
			}
		}
		return result;
	}
}
