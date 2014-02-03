using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertiesStates
{
	internal class PropertyState
	{
		public bool selected;

		public bool visible;

		public bool expanded;

		public int heightMultiplier;

		public Color backColor;

		public Color foreColor;

		public Color disabledForeColor;

		public Color readOnlyForeColor;

		public Font font;

		public Color valueBackColor;

		public Color valueForeColor;

		public Color valueDisabledForeColor;

		public Color valueReadOnlyForeColor;

		public Font valueFont;

		public PropertyGrid.ImageSources imageSource;

		public ImageList imageList;

		public int imageIndex;

		public Size imageSize;

		public PropertyState(bool selected, bool visible, bool expanded, int heightMultiplier)
		{
			this.selected = selected;
			this.expanded = expanded;
			this.visible = visible;
			this.heightMultiplier = heightMultiplier;
		}
	}

	internal Hashtable _states = new Hashtable();

	internal PropertyGrid.PropertyStateFlags _flags;

	internal int _selectedPropertyVisibleIndex;

	internal int _firstDisplayedPropertyHashcode;

	internal int _firstDisplayedLine;

	internal Control _inPlaceCtrlInAction;

	public PropertiesStates(PropertyGrid.PropertyStateFlags flags)
	{
		_flags = flags;
		_selectedPropertyVisibleIndex = -1;
	}
}
