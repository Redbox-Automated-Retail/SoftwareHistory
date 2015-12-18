using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Redbox.KioskEngine.Environment.Properties;

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
				ResourceManager resourceManager = new ResourceManager("Redbox.KioskEngine.Environment.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap Authentication
	{
		get
		{
			object obj = ResourceManager.GetObject("Authentication", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap DatabaseSchema
	{
		get
		{
			object obj = ResourceManager.GetObject("DatabaseSchema", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Health
	{
		get
		{
			object obj = ResourceManager.GetObject("Health", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Inventory
	{
		get
		{
			object obj = ResourceManager.GetObject("Inventory", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap MerchantService
	{
		get
		{
			object obj = ResourceManager.GetObject("MerchantService", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Promotion
	{
		get
		{
			object obj = ResourceManager.GetObject("Promotion", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap QueueService
	{
		get
		{
			object obj = ResourceManager.GetObject("QueueService", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Reservation
	{
		get
		{
			object obj = ResourceManager.GetObject("Reservation", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ShoppingSession
	{
		get
		{
			object obj = ResourceManager.GetObject("ShoppingSession", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Store
	{
		get
		{
			object obj = ResourceManager.GetObject("Store", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
