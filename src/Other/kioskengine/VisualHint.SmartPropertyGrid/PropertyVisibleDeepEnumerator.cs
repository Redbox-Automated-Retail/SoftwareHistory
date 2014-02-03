namespace VisualHint.SmartPropertyGrid;

public class PropertyVisibleDeepEnumerator : PropertyDeepEnumerator
{
	internal PropertyVisibleDeepEnumerator(LinkedListNode node)
		: base(node)
	{
		PropertyEnumerator propertyEnumerator = new PropertyDeepEnumerator(base.Node);
		while (propertyEnumerator.HasParent)
		{
			propertyEnumerator.MoveParent();
			if (!propertyEnumerator.Property.Expanded || !propertyEnumerator.Property.Visible)
			{
				base.Node = propertyEnumerator.Node;
			}
		}
		if (this != base.LeftBound)
		{
			while (this != base.RightBound && !base.Property.Visible)
			{
				MoveNext();
			}
		}
	}

	public override object Clone()
	{
		PropertyEnumerator visibleDeepEnumerator = GetVisibleDeepEnumerator();
		visibleDeepEnumerator._restrictToThisLinkedListAndUnder = _restrictToThisLinkedListAndUnder;
		visibleDeepEnumerator._restrictToThisNodeAndUnder = _restrictToThisNodeAndUnder;
		visibleDeepEnumerator.RightBoundInternal = base.RightBound;
		visibleDeepEnumerator.LeftBoundInternal = base.LeftBound;
		return visibleDeepEnumerator;
	}

	public override PropertyEnumerator MoveNext()
	{
		while (!(this == base.RightBound))
		{
			if (base.Property != null && (!base.Property.Visible || !base.Property.Expanded))
			{
				bool flag = !base.Property.Visible;
				PropertyVisibleSiblingEnumerator visibleSiblingEnumerator = GetVisibleSiblingEnumerator();
				if (!flag)
				{
					visibleSiblingEnumerator.MoveNext();
				}
				while (visibleSiblingEnumerator == visibleSiblingEnumerator.RightBound)
				{
					visibleSiblingEnumerator.MoveParent();
					if (visibleSiblingEnumerator == base.RightBound)
					{
						break;
					}
					visibleSiblingEnumerator.MoveNext();
				}
				base.Node = visibleSiblingEnumerator.Node;
			}
			else
			{
				base.MoveNext();
			}
			if (!(this != base.RightBound) || base.Property.Visible)
			{
				break;
			}
		}
		return this;
	}

	public override PropertyEnumerator MovePrev()
	{
		do
		{
			if (base.Node == base.Node.Owner.First)
			{
				if (base.HasParent)
				{
					MoveParent();
				}
				else
				{
					base.Node = base.LeftBound.Node;
				}
				continue;
			}
			base.Node = base.Node.Previous;
			if (base.Property != null && base.Property.Visible && base.Node.Children.Count > 0 && base.Property.Expanded)
			{
				base.Node = base.Node.Children.RightBound;
				MovePrev();
			}
		}
		while (this != base.LeftBound && !base.Property.Visible);
		return this;
	}
}
