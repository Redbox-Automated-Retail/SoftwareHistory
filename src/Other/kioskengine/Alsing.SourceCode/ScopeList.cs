using System.Collections.Generic;

namespace Alsing.SourceCode;

public sealed class ScopeList : List<Scope>
{
	public SpanDefinition Parent { get; private set; }

	public ScopeList(SpanDefinition parent)
	{
		Parent = parent;
	}
}
