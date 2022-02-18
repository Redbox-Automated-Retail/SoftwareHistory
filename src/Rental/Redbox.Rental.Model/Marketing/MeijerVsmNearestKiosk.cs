namespace Redbox.Rental.Model.Marketing;

public class MeijerVsmNearestKiosk
{
	public string Banner { get; set; }

	public string Address { get; set; }

	public string City { get; set; }

	public MeijerVsmNearestKiosk(string banner, string address, string city)
	{
		Banner = banner;
		Address = address;
		City = city;
	}
}
