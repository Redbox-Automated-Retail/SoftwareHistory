using Redbox.NetCore.Logging.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Redbox.NetCore.Logging.Enrichers;

public class SingleLineMessageEnricher : ILogEventEnricher
{
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		if (!string.IsNullOrEmpty(logEvent.MessageTemplate.Text))
		{
			LogEventProperty property = propertyFactory.CreateProperty("SingleLineMessage", logEvent.MessageTemplate.Render(logEvent.Properties).EscapeNewLines());
			logEvent.AddPropertyIfAbsent(property);
		}
	}
}
