using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReturnResult : BaseResponse, IReturnResult, IMessageScrub
{
	public long? TransactionId { get; set; }

	public string CPAccountNumber { get; set; }

	public object Scrub()
	{
		return this;
	}
}
