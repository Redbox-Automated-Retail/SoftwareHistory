using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.App;

[Obsolete("Use ApiBaseResponse in middleware.")]
public abstract class BaseResponse
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
	public bool Success { get; set; }

	public IList<Error> Errors { get; set; } = new List<Error>();

	[JsonProperty(Required = Required.DisallowNull)]
	public string HostName => Environment.MachineName;

	public int StatusCode { get; set; }

	public int? ResponseTime { get; set; }

	protected BaseResponse(int statusCode)
	{
		StatusCode = statusCode;
	}

	protected BaseResponse(IList<Error> errors, int? statusCode)
	{
		StatusCode = statusCode ?? 500;
		Errors = errors;
		Success = false;
	}

	public ObjectResult ToObjectResult()
	{
		return new ObjectResult(this)
		{
			StatusCode = StatusCode
		};
	}
}
