using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.App;

public class LoggingActionFilter : IAsyncActionFilter, IFilterMetadata
{
	private readonly ILogger<LoggingActionFilter> _logger;

	public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
	{
		_logger = logger;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
		string controllerName = controllerActionDescriptor.ControllerName.Trim();
		string actionName = controllerActionDescriptor.ActionName.Trim();
		StringBuilder scrubbedRequest = new StringBuilder();
		context.ActionArguments.ToList().ForEach(delegate(KeyValuePair<string, object> arg)
		{
			if (arg.Value is IMessageScrub)
			{
				scrubbedRequest.Append($",{arg.Key}:{(arg.Value as IMessageScrub).Scrub()}");
			}
			else
			{
				scrubbedRequest.Append($",{arg.Key}:{arg.Value}");
			}
		});
		_logger.LogInformation($"START: {controllerName}.{actionName} {scrubbedRequest}");
		Stopwatch requestTimer = Stopwatch.StartNew();
		ActionExecutedContext actionExecutedContext = await next();
		StringBuilder stringBuilder = new StringBuilder();
		if (actionExecutedContext != null && actionExecutedContext.Result != null)
		{
			if (actionExecutedContext.Result is ObjectResult)
			{
				ObjectResult objectResult = actionExecutedContext.Result as ObjectResult;
				if (objectResult.Value != null && objectResult.Value is IMessageScrub)
				{
					stringBuilder.Append($"{(objectResult.Value as IMessageScrub).Scrub()}");
				}
				else
				{
					stringBuilder.Append($"{objectResult.StatusCode}");
				}
			}
			else if (actionExecutedContext.Result is ContentResult)
			{
				ContentResult contentResult = actionExecutedContext.Result as ContentResult;
				stringBuilder.Append(contentResult.Content);
			}
		}
		else
		{
			stringBuilder.Append("none");
		}
		requestTimer.Stop();
		_logger.LogInformation($"{controllerName}.{actionName}, response={stringBuilder}");
		_logger.LogInformation($"END: {controllerName}.{actionName}, statuscode={(HttpStatusCode)context.HttpContext.Response.StatusCode}, elapsed={requestTimer.ElapsedMilliseconds}");
	}
}
