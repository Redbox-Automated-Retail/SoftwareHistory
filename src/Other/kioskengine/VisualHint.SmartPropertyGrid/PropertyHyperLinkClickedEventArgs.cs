namespace VisualHint.SmartPropertyGrid;

public class PropertyHyperLinkClickedEventArgs : PropertyEventArgs
{
	private string _hyperlink;

	public string HyperLink => _hyperlink;

	public PropertyHyperLinkClickedEventArgs(PropertyEnumerator propEnum, string hyperlink)
		: base(propEnum)
	{
		_hyperlink = hyperlink;
	}
}
