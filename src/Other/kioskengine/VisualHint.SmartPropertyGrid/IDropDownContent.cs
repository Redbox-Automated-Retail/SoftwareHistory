namespace VisualHint.SmartPropertyGrid;

public interface IDropDownContent
{
	void InitializeContent(PropertyEnumerator propEnum, string currentValue, object valueKey);

	bool HasToBeClosedOnClick();

	object GetValue();

	int GetSelectedIndex();
}
