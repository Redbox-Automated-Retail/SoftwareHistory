using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.Rental.Services.KioskClientService;

public class BaseResponse : IBaseResponse
{
	public bool Success { get; set; }

	public IList<IError> Errors { get; set; } = new List<IError>();

	public string HostName => Environment.MachineName;

	public int StatusCode { get; set; }
}
