using System.Runtime.InteropServices;

namespace stdole;

[ComVisible(false)]
[ComEventInterface(typeof(FontEvents), typeof(FontEvents_EventProvider))]
public interface FontEvents_Event
{
	event FontEvents_FontChangedEventHandler FontChanged;
}
