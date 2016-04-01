namespace Redbox.HAL.Component.Model;

public interface IRuntimeService
{
	string AssemblyDirectory { get; }

	string InstallRoot { get; }

	int PageSize { get; }

	string ExpandRuntimeMacros(string value);

	string ExpandConstantMacros(string value);

	string RuntimePath(string fileName);

	string InstallPath(string subfolder);

	Platform GetPlatform();
}
