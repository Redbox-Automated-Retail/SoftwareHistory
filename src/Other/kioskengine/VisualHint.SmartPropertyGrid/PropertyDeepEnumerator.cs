namespace VisualHint.SmartPropertyGrid;

public class PropertyDeepEnumerator : PropertyEnumerator
{
	public PropertyDeepEnumerator()
	{
	}

	internal PropertyDeepEnumerator(LinkedListNode node)
		: base(node)
	{
		CalcLeftAndRightBounds();
	}

	public override object Clone()
	{
		PropertyEnumerator deepEnumerator = GetDeepEnumerator();
		deepEnumerator._restrictToThisLinkedListAndUnder = _restrictToThisLinkedListAndUnder;
		deepEnumerator._restrictToThisNodeAndUnder = _restrictToThisNodeAndUnder;
		deepEnumerator.RightBoundInternal = base.RightBound;
		deepEnumerator.LeftBoundInternal = base.LeftBound;
		return deepEnumerator;
	}

	protected override void CalcLeftAndRightBounds()
	{
		if (_restrictToThisLinkedListAndUnder != null)
		{
			mLeftBound = new PropertySiblingEnumerator();
			mLeftBound.Node = base.Node.Owner.LeftBound;
			mRightBound = new PropertySiblingEnumerator();
			mRightBound.Node = base.Node.Owner.RightBound;
		}
		else if (_restrictToThisNodeAndUnder != null)
		{
			mLeftBound = new PropertySiblingEnumerator();
			mLeftBound.Node = _restrictToThisNodeAndUnder.Previous;
			mRightBound = new PropertySiblingEnumerator();
			mRightBound.Node = _restrictToThisNodeAndUnder.Next;
		}
		else
		{
			LinkedList owner = base.Node.Owner;
			while (owner.Parent != null)
			{
				owner = owner.Parent.Owner;
			}
			mLeftBound = new PropertyDeepEnumerator();
			mLeftBound.Node = owner.LeftBound;
			mRightBound = new PropertyDeepEnumerator();
			mRightBound.Node = owner.RightBound;
		}
		mLeftBound.LeftBoundInternal = base.LeftBound;
		mLeftBound.RightBoundInternal = base.RightBound;
		mRightBound.LeftBoundInternal = base.LeftBound;
		mRightBound.RightBoundInternal = base.RightBound;
	}

	public override PropertyEnumerator MoveNext()
	{
		if (this == base.RightBound)
		{
			return this;
		}
		if (base.Node.Previous != null && base.Node.Children.Count > 0)
		{
			base.Node = base.Node.Children.First;
		}
		else
		{
			base.Node = base.Node.Next;
			while (this != base.RightBound && base.Node == base.Node.Owner.RightBound)
			{
				MoveParent();
				base.Node = base.Node.Next;
			}
		}
		return this;
	}

	public override PropertyEnumerator MovePrev()
	{
		if (this == base.LeftBound)
		{
			return this;
		}
		if (base.Node == base.Node.Owner.First)
		{
			MoveParent();
			if (this == base.RightBound)
			{
				base.Node = base.Node.Owner.LeftBound;
			}
		}
		else
		{
			base.Node = base.Node.Previous;
			if (this != base.LeftBound)
			{
				for (LinkedListNode last = base.Node.Children.Last; last != null; last = base.Node.Children.Last)
				{
					base.Node = last;
				}
			}
		}
		return this;
	}

	public override PropertyEnumerator MoveFirst()
	{
		base.Node = base.Node.Owner.LeftBound;
		MoveNext();
		return this;
	}

	public override PropertyEnumerator MoveLast()
	{
		base.Node = base.Node.Owner.RightBound;
		MovePrev();
		if (this == base.LeftBound)
		{
			base.Node = base.Node.Owner.RightBound;
		}
		return this;
	}

	public override PropertyEnumerator MoveParent()
	{
		if (base.HasParent)
		{
			base.Node = base.Node.Owner.Parent;
		}
		else
		{
			base.Node = base.RightBound.Node;
		}
		return this;
	}
}
