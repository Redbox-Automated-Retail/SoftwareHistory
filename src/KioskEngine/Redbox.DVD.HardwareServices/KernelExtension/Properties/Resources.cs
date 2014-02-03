using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.HardwareServices.KernelExtension.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
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
				ResourceManager resourceManager = new ResourceManager("Redbox.HardwareServices.KernelExtension.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap Clean
	{
		get
		{
			object obj = ResourceManager.GetObject("Clean", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Edit
	{
		get
		{
			object obj = ResourceManager.GetObject("Edit", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Execute
	{
		get
		{
			object obj = ResourceManager.GetObject("Execute", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Minus
	{
		get
		{
			object obj = ResourceManager.GetObject("Minus", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Open
	{
		get
		{
			object obj = ResourceManager.GetObject("Open", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Plus
	{
		get
		{
			object obj = ResourceManager.GetObject("Plus", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Refresh
	{
		get
		{
			object obj = ResourceManager.GetObject("Refresh", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap SaveAs
	{
		get
		{
			object obj = ResourceManager.GetObject("SaveAs", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
