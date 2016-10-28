using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Parameters;

[ComVisible(true)]
public interface IParameterProvider
{
	object this[GenuineParameter genuineParameter] { get; set; }
}
