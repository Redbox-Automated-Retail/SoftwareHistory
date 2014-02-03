using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Skybound.VisualTips;

namespace x1b802d6029d7be4a;

internal class x9249ca59576005f1 : IVisualTipExtender
{
	private object x42f4c234c9358072;

	public x9249ca59576005f1(object target)
	{
		x42f4c234c9358072 = target;
	}

	private static MethodInfo x569eb6b436e07d6c(object x11d58b056c032b03, string xc15bd84e01929885, params Type[] x2eb67d2eee91fcba)
	{
		return x11d58b056c032b03.GetType().GetMethod(xc15bd84e01929885, BindingFlags.Instance | BindingFlags.Public, null, x2eb67d2eee91fcba, null);
	}

	public static bool x461c3bf969128260(object x11d58b056c032b03)
	{
		MethodInfo methodInfo = x569eb6b436e07d6c(x11d58b056c032b03, "GetChildAtPoint", typeof(Control), typeof(int), typeof(int));
		MethodInfo methodInfo2 = x569eb6b436e07d6c(x11d58b056c032b03, "GetParent", typeof(object));
		MethodInfo methodInfo3 = x569eb6b436e07d6c(x11d58b056c032b03, "GetChildTypes", Type.EmptyTypes);
		if (methodInfo != null && methodInfo2 != null && methodInfo3 != null)
		{
			return methodInfo3.ReturnType == typeof(Type[]);
		}
		return false;
	}

	public object GetChildAtPoint(Control control, int x, int y)
	{
		return x569eb6b436e07d6c(x42f4c234c9358072, "GetChildAtPoint", typeof(Control), typeof(int), typeof(int)).Invoke(x42f4c234c9358072, new object[3] { control, x, y });
	}

	public object GetParent(object component)
	{
		return x569eb6b436e07d6c(x42f4c234c9358072, "GetParent", typeof(object)).Invoke(x42f4c234c9358072, new object[1] { component });
	}

	public Type[] GetChildTypes()
	{
		return (Type[])x569eb6b436e07d6c(x42f4c234c9358072, "GetChildTypes", Type.EmptyTypes).Invoke(x42f4c234c9358072, null);
	}

	public Rectangle GetBounds(object component)
	{
		MethodInfo methodInfo = x569eb6b436e07d6c(x42f4c234c9358072, "GetBounds", typeof(object));
		if (methodInfo != null)
		{
			return (Rectangle)methodInfo.Invoke(x42f4c234c9358072, new object[1] { component });
		}
		return Rectangle.Empty;
	}
}
