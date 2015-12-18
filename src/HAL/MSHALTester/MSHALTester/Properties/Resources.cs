using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[DebuggerNonUserCode]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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

	internal static byte[] pick_at_offset
	{
		get
		{
			object obj = ResourceManager.GetObject("pick_at_offset", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static byte[] RotationTest
	{
		get
		{
			object obj = ResourceManager.GetObject("RotationTest", resourceCulture);
			return (byte[])obj;
		}
	}

	internal static string SourceDeckSlotInvalid => ResourceManager.GetString("SourceDeckSlotInvalid", resourceCulture);

	internal Resources()
	{
	}
}
