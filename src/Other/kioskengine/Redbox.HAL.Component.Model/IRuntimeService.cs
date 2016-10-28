namespace Redbox.HAL.Component.Model;

internal interface IRuntimeService
{
	string DataPath { get; }

	string AssemblyDirectory { get; }

	string InstallRoot { get; }

	int PageSize { get; }

	string ExpandRuntimeMacros(string value);

	string ExpandConstantMacros(string value);

	string RuntimePath(string fileName);

	string InstallPath(string subfolder);

	Platform GetPlatform();
}
