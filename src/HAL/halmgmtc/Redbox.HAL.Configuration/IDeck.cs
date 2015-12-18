using System.Collections.Generic;

namespace Redbox.HAL.Configuration;

public interface IDeck
{
	List<IDeckSegment> Segments { get; }
}
