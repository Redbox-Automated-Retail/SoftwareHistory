namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class KioskClientServiceTransactionConstants
{
	public class TestData
	{
		public class Authorize
		{
			public const string LiveCall = "Authorize.LiveCall";

			public const string SimulateCommunicationFailure = "Authorize.SimulateCommunicationFailure";

			public const string ErrorCode = "Authorize.ErrorCode";

			public const string HasCustomerProfile = "Authorize.HasCustomerProfile";

			public const string AccountNumber = "Authorize.AccountNumber";

			public const string CustomerProfileNumber = "Authorize.CustomerProfileNumber";

			public const string CustomerProfileFirstName = "Authorize.CustomerProfileFirstName";

			public const string CustomerProfileLastName = "Authorize.CustomerProfileLastName";

			public const string Email = "Authorize.Email";

			public const string ReferenceNumber = "Authorize.ReferenceNumber";

			public const string ConfirmationStatus = "Authorize.ConfirmationStatus";
		}

		public const string CategoryName = "TransactionService";
	}
}
