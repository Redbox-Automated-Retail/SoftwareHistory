namespace VisualHint.SmartPropertyGrid;

public class PropertySiblingEnumerator : PropertyEnumerator
{
	internal PropertySiblingEnumerator()
	{
	}

	internal PropertySiblingEnumerator(LinkedListNode node)
		: base(node)
	{
		mLeftBound = new PropertySiblingEnumerator();
		mLeftBound.Node = base.Node.Owner.LeftBound;
		mRightBound = new PropertySiblingEnumerator();
		mRightBound.Node = base.Node.Owner.RightBound;
		mLeftBound.LeftBoundInternal = base.LeftBound;
		mLeftBound.RightBoundInternal = base.RightBound;
		mRightBound.LeftBoundInternal = base.LeftBound;
		mRightBound.RightBoundInternal = base.RightBound;
	}

	public override object Clone()
	{
		return GetSiblingEnumerator();
	}

	protected override void CalcLeftAndRightBounds()
	{
	}

	public override PropertyEnumerator MoveNext()
	{
		if (this != base.RightBound)
		{
			base.Node = base.Node.Next;
		}
		return this;
	}

	public override PropertyEnumerator MovePrev()
	{
		if (this != base.LeftBound)
		{
			base.Node = base.Node.Previous;
		}
		return this;
	}

	public override PropertyEnumerator MoveFirst()
	{
		if (base.Node.Owner.Count == 0)
		{
			base.Node = base.RightBound.Node;
		}
		else
		{
			base.Node = base.Node.Owner.First;
		}
		return this;
	}

	public override PropertyEnumerator MoveLast()
	{
		if (base.Node.Owner.Count == 0)
		{
			base.Node = base.RightBound.Node;
		}
		else
		{
			base.Node = base.Node.Owner.Last;
		}
		return this;
	}

	public override PropertyEnumerator MoveParent()
	{
		if (base.HasParent)
		{
			base.Node = base.Node.Owner.Parent;
			base.LeftBound.Node = base.Node.Owner.LeftBound;
			base.RightBound.Node = base.Node.Owner.RightBound;
		}
		else
		{
			base.Node = base.RightBound.Node;
		}
		return this;
	}
}
