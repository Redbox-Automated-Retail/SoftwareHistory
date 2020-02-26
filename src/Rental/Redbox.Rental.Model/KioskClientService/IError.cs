namespace Redbox.Rental.Model.KioskClientService;

public interface IError
{
	string Code { get; set; }

	string Message { get; set; }
}
