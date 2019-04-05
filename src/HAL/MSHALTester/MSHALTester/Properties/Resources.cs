using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[DebuggerNonUserCode]
[CompilerGenerated]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
				ResourceManager resourceManager = new ResourceManager("Redbox.HAL.MSHALTester.Properties.Resources", typeof(Resources).Assembly);
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

	internal static string DestDeckSlotInvalid => ResourceManager.GetString("DestDeckSlotInvalid", resourceCulture);

	internal static Bitmap sortAscending2
	{
		get
		{
			object obj = ResourceManager.GetObject("sortAscending2", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap sortDescending2
	{
		get
		{
			object obj = ResourceManager.GetObject("sortDescending2", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static string SourceDeckSlotInvalid => ResourceManager.GetString("SourceDeckSlotInvalid", resourceCulture);

	internal static Bitmap undo
	{
		get
		{
			object obj = ResourceManager.GetObject("undo", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
