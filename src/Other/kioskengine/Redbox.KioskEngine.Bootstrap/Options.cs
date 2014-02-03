using System.Collections.Generic;
using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.KioskEngine.Bootstrap;

[Usage("kioskengine [-D:property=value ...] [--bundle[:|=]file]")]
public class Options
{
	[Description("The name of the resource bundle to load.")]
	[Option(LongName = "bundle")]
	public string BundleFile;

	[Description("Define properties to inject into runtime environment.")]
	[Option(ShortName = "D")]
	public IDictionary<string, string> Properties = new Dictionary<string, string>();

	public static Options Instance => Singleton<Options>.Instance;

	private Options()
	{
	}
}
