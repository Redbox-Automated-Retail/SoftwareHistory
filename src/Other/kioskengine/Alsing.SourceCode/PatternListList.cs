using System.Collections;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class PatternListList : IEnumerable
{
	private readonly ArrayList mGroups = new ArrayList();

	public bool IsKeyword;

	public bool IsOperator;

	public SpanDefinition Parent;

	public PatternList this[int index]
	{
		get
		{
			return (PatternList)mGroups[index];
		}
		set
		{
			mGroups[index] = value;
		}
	}

	public PatternListList()
	{
	}

	public PatternListList(SpanDefinition parent)
	{
		Parent = parent;
	}

	public IEnumerator GetEnumerator()
	{
		return mGroups.GetEnumerator();
	}

	public PatternList Add(PatternList Group)
	{
		mGroups.Add(Group);
		Group.Parent = this;
		if (Parent != null && Parent.Parent != null)
		{
			Parent.Parent.ChangeVersion();
		}
		return Group;
	}

	public void Clear()
	{
		mGroups.Clear();
	}
}
