namespace Redbox.Rental.Model.Analytics;

public class CollectionData : AnalyticsData
{
	public long Id { get; set; }

	public string Name { get; set; }

	public CollectionData(long id, string name)
	{
		Id = id;
		Name = name;
		base.DataType = "Collection";
	}
}
