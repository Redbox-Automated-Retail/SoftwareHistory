namespace VisualHint.SmartPropertyGrid;

public abstract class SPGComparer
{
	private PropertyGrid _grid;

	public PropertyGrid Grid => _grid;

	public SPGComparer(PropertyGrid grid)
	{
		_grid = grid;
	}
}
