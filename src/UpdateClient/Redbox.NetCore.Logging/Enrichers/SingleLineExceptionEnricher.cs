using Redbox.NetCore.Logging.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Redbox.NetCore.Logging.Enrichers;

public class SingleLineExceptionEnricher : ILogEventEnricher
{
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		if (logEvent.Exception != null)
		{
			LogEventProperty property = propertyFactory.CreateProperty("SingleLineException", logEvent.Exception.ToString().EscapeNewLines());
			logEvent.AddPropertyIfAbsent(property);
		}
	}
}
