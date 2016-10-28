using System;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

internal class Utilities
{
	public static ControlStyles DoubleBufferStyle => ControlStyles.OptimizedDoubleBuffer;

	public static Type GetTypeFromName(string typeName, Type valueType)
	{
		if (typeName == null || typeName.Length == 0)
		{
			return null;
		}
		Type type = Type.GetType(typeName);
		if ((object)type != null)
		{
			return type;
		}
		int num = typeName.IndexOf(',');
		if (num != -1)
		{
			typeName = typeName.Substring(0, num);
		}
		return valueType.Assembly.GetType(typeName);
	}

	public static int GetHashCode(object[] objArray)
	{
		int num = 0;
		foreach (object obj in objArray)
		{
			if (obj != null)
			{
				num ^= obj.GetHashCode();
			}
		}
		return num;
	}

	public static ContainerControl GetTopLevelContainer(Control c)
	{
		Control control = c;
		while (control != null && control.Parent != null)
		{
			control = control.Parent;
		}
		return control as ContainerControl;
	}
}
