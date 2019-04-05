namespace Redbox.Rental.Model.Analytics;

public interface IAnalyticsService
{
	void Initialize();

	void WriteSessionToFile(string sessionId, string xml);
}
