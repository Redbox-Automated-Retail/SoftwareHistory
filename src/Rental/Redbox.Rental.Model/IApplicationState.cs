namespace Redbox.Rental.Model;

public interface IApplicationState
{
	bool IsADAAccessible { get; set; }

	bool IsABEOn { get; }
}
