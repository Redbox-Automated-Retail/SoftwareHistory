namespace Redbox.UpdateManager.ComponentModel;

internal interface IMacroService
{
	string this[string name] { get; set; }

	string ExpandProperties(string input);
}
