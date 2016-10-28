using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class ScopeList : List<Scope>
{
	public SpanDefinition Parent { get; private set; }

	public ScopeList(SpanDefinition parent)
	{
		Parent = parent;
	}
}
