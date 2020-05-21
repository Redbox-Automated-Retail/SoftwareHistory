using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService;

public interface IBaseResponse
{
	bool Success { get; set; }

	IList<Error> Errors { get; set; }

	string HostName { get; }

	int StatusCode { get; set; }
}
