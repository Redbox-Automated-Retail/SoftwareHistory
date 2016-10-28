using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public interface ISessionSupport
{
	object this[object key] { get; set; }
}
