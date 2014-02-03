using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Properties;

[DebuggerNonUserCode]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static byte[] FlippedCursor
	{
		get
		{
			object obj = ResourceManager.GetObject("FlippedCursor", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] MoveDown
	{
		get
		{
			object obj = ResourceManager.GetObject("MoveDown", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] MoveUp
	{
		get
		{
			object obj = ResourceManager.GetObject("MoveUp", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] MoveUpDown
	{
		get
		{
			object obj = ResourceManager.GetObject("MoveUpDown", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static Bitmap SourceCodePrintDocument
	{
		get
		{
			object obj = ResourceManager.GetObject("SourceCodePrintDocument", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap SyntaxBoxControl
	{
		get
		{
			object obj = ResourceManager.GetObject("SyntaxBoxControl", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap SyntaxDocument
	{
		get
		{
			object obj = ResourceManager.GetObject("SyntaxDocument", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
