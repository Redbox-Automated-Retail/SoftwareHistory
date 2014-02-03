namespace Redbox.TicketServices.Proxy.ComponentModel;

internal class Venue
{
	public long VenueId { get; set; }

	public string VenueName { get; set; }

	public string Address1 { get; set; }

	public string Address2 { get; set; }

	public string PostalCode { get; set; }

	public string CityName { get; set; }

	public string Region { get; set; }

	public string Country { get; set; }

	public string TimeZoneName { get; set; }

	public string PhoneNumber { get; set; }

	public string ParkingInformation { get; set; }

	public string ShortNote { get; set; }

	public string LongNote { get; set; }

	public bool IsADAFacility { get; set; }

	public string AccesibilityInformation { get; set; }
}
