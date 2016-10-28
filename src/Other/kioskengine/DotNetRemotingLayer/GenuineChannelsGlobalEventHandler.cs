using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public delegate void GenuineChannelsGlobalEventHandler(object sender, GenuineEventArgs e);
