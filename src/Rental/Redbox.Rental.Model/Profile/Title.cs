using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Profile;

public class Title : ILuaKeyValue
{
	public string Key { get; set; }

	public string Value { get; set; }

	public long ProductId { get; set; }

	public int Stars { get; set; }

	public string RunningTime { get; set; }

	public List<string> Starring { get; set; }

	public int ProductTypeId { get; set; }

	public long? RatingId { get; set; }

	public string LongName { get; set; }

	public string SortName { get; set; }

	public string ImageFile { get; set; }

	public string Description { get; set; }

	public List<long> Genres { get; set; }

	public DateTime? ReleaseDate { get; set; }

	public bool SellThru { get; set; }

	public bool SellThruNew { get; set; }

	public decimal BoxOfficeGross { get; set; }

	public List<string> Directors { get; set; }

	public int ComingSoonDays { get; set; }

	public DateTime? SortDate { get; set; }

	public DateTime? NationalStreetDate { get; set; }

	public bool ClosedCaptioned { get; set; }

	public DateTime? MerchandiseDate { get; set; }

	public long? SubPlatformId { get; set; }

	public string NumberOfPlayersText { get; set; }

	public DateTime? DoNotRentDate { get; set; }

	public DateTime? SellThruDate { get; set; }

	public decimal? SellThruPrice { get; set; }

	public int? DiscNumber { get; set; }
}
