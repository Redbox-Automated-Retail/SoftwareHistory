using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.Fraud;

public class FraudController : ApiController
{
	[HttpGet]
	[Route("api/fraud/info")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(FraudServiceInfoResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetFraudServiceInfo()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		FraudServiceInfo fraudServiceInfo = new FraudServiceInfo();
		FraudServiceInfoResponse response = new FraudServiceInfoResponse
		{
			Content = fraudServiceInfo
		};
		await Task.Run(async delegate
		{
			IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
			if (service != null)
			{
				fraudServiceInfo.HardwareSupportsIRScan = service.HardwareSupportsIRScan;
				fraudServiceInfo.IsFraudImageEnabled = service.IsFraudImageEnabled;
				fraudServiceInfo.IsIRSecurityEnabled = service.IsIRSecurityEnabled;
				fraudServiceInfo.FraudPeelIncludeFormats = service.FraudPeelIncludeFormats;
				fraudServiceInfo.FraudPaperExcludedFormats = service.FraudPaperExcludedFormats;
				response.StatusCode = HttpStatusCode.OK;
				response.Success = true;
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find FraudService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
