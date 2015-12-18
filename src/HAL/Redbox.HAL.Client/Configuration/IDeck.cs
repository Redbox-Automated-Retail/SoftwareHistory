using System.Collections.Generic;

namespace Redbox.HAL.Configuration;

internal interface IDeck
{
	List<IDeckSegment> Segments { get; }
}
