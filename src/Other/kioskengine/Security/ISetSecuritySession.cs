using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public interface ISetSecuritySession
{
	SecuritySessionParameters SecuritySessionParameters { get; set; }
}
