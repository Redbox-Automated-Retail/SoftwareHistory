namespace Redbox.HAL.Component.Model;

internal interface IFormattedLogFactoryService
{
	string LogsBasePath { get; }

	IFormattedLog NilLog { get; }

	string CreateSubpath(string subfolder);
}
