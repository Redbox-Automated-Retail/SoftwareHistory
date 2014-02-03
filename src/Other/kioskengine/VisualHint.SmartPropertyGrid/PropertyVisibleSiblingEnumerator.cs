namespace VisualHint.SmartPropertyGrid;

public class PropertyVisibleSiblingEnumerator : PropertySiblingEnumerator
{
	internal PropertyVisibleSiblingEnumerator(LinkedListNode node)
		: base(node)
	{
		PropertyEnumerator propertyEnumerator = new PropertyDeepEnumerator(base.Node);
		while (propertyEnumerator.HasParent)
		{
			propertyEnumerator.MoveParent();
			if (!propertyEnumerator.Property.Expanded || !propertyEnumerator.Property.Visible)
			{
				base.Node = base.RightBound.Node;
				break;
			}
		}
		while (this != base.RightBound && !base.Property.Visible)
		{
			MoveNext();
		}
	}

	public override object Clone()
	{
		return GetVisibleSiblingEnumerator();
	}

	public override PropertyEnumerator MoveNext()
	{
		if (this == base.RightBound)
		{
			return this;
		}
		do
		{
			base.Node = base.Node.Next;
		}
		while (this != base.RightBound && !base.Property.Visible);
		return this;
	}

	public override PropertyEnumerator MovePrev()
	{
		if (this == base.LeftBound)
		{
			return this;
		}
		do
		{
			base.Node = base.Node.Previous;
		}
		while (this != base.LeftBound && !base.Property.Visible);
		return this;
	}

	public override PropertyEnumerator MoveFirst()
	{
		if (base.Node.Owner.Count == 0)
		{
			base.Node = base.LeftBound.Node;
		}
		else
		{
			base.Node = base.Node.Owner.First;
			while (this != base.RightBound && !base.Property.Visible)
			{
				base.Node = base.Node.Next;
			}
			if (this == base.RightBound)
			{
				base.Node = base.LeftBound.Node;
			}
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
			while (this != base.LeftBound && !base.Property.Visible)
			{
				base.Node = base.Node.Previous;
			}
			if (this == base.LeftBound)
			{
				base.Node = base.RightBound.Node;
			}
		}
		return this;
	}
}
